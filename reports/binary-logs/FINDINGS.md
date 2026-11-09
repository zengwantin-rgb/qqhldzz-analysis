# 第十三轮：二进制日志解码

完整包的两份 .xlog 已按 Tencent Mars 公开格式成功解压，共恢复 1,728,013 字节文本。这消除了上一轮“两份 xlog 未解码”的覆盖缺口，但没有新增实时对手手牌证据。原始及解压后日志均不进入仓库或公开附件。

## 格式与覆盖

格式依据为 [Tencent Mars 官方无加密解码脚本](https://github.com/Tencent/mars/blob/master/mars/xlog/crypt/decode_mars_nocrypt_log_file.py)。自建读取器使用明确的小端长度、记录结束标记、连续记录检查和 4 MiB 解压预算，不运行官方旧版 Python 2 脚本，不尝试猜测密钥。

| 完整包条目 | 输入字节 | 记录数 | 魔数 | 解码字节 |
|---|---:|---:|---|---:|
| 11 | 109324 | 2 | 0x09 × 2 | 1090135 |
| 12 | 60827 | 4 | 0x08 × 2、0x09 × 2 | 637878 |

全部记录从文件起点连续覆盖到结尾，无前缀跳过、未解析尾部或解压错误。0x08 为未压缩无加密记录，0x09 为无加密 raw-deflate；无需取得加密私钥。4 条 deflate 记录均没有最终流结束标记，符合逐缓冲区 sync-flush 的读取方式，报告明确计数；这不等于宣称日志历史从未丢失。

两份解码文本均无 UTF-8 替换字符。MSDK 标记共 13065 次，Login 共 1521 次，HTTP 共 166 次。首轮对已知发牌、等待、结算、自摸、额外牌、手牌、牌墙、能力手牌和记录载体字段的检查均为 0。标记频次支持其主要为 SDK 活动日志，不能单靠频次证明每一行的业务来源。

## 原生组件线索

[安装包静态扫描](native-file-sources.json)在 GCloudCore 检出 .xlog 与 LogCrypt，在 UnityFramework 检出 appender_open、LogCrypt 与 MSDKLog，在多个 MSDKPIX 组件检出 MSDKLog。ilink_live 另有 xlogger 与 Mars 命名标记。这些是日志组件存在的线索，不是实际文件写入者的调用栈证明，也不能据此认定所有 SDK 使用同一日志格式。

## 验证

[首次成功任务](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42671728280)在 macos-15-intel 完成，6 项新测试通过，覆盖明文/压缩混合、sync-flush、加密记录不当明文、错误边界、伪候选、前后未解析字节、分块长度及解压预算。此前 3 次完整工作流在创建作业前遇到 GitHub Internal server error，未执行分析；改用独立工作流后取得实际结果。

解码文本只在运行器内存中处理。公开输出仅包含计数、恢复源码中的字段名称和结构形状。牌值、用户字段、令牌、原文和设备路径均不公开。

## 尚未完成的完整字段复查

后续源码 099e81d 为恢复文本加入了 5364 个源码字段的 token 索引及完整 JSON 结构检查，并处理连续帧边界。此扩展尚无成功运行结果，不能将首轮 11 项已知游戏标记的零命中扩大为“全部字段均已检查”。

[任务 42671948777](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42671948777)的首次与重试，以及[任务 42672356079](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42672356079)均在创建任何作业前失败，页面明确报告 Internal server error；没有测试日志或分析产物。6 项测试通过与两份 xlog 解码结果来自此前成功的 42671728280，源码 5c2652e。扩展代码已推送，可在服务恢复后用 xlog.yml 重试，不需要用户重新打牌或导出本包。

当前证据范围仍限于已解压文本的受限标记检查、结构边界以及静态 SDK 标记。未完成的完整字段检查与运行时解码消息采集都不能宣称已经实现；现有结果不支持实时显示其他玩家暗牌。
