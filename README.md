# 欢乐麻将 iOS 分析与运行时观察

当前新增：[0.3 运行时模型手牌面板](mobile/MessageObserver/README.md)，[下载 dylib](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/download/message-observer-v0.3.0-beta1/MJMessageObserver.dylib)。已实现直接读取当前四家模型、区分牌背与已知牌值、单独显示暗杠，并限量记录辅助字段及内部缓存。构建和回归检查已通过；新运行时入口已由真机截图验证能读取并显示三家的牌背数量，辅助字段与缓存内容仍待新导出核对；实时看到真实对手暗牌的目标尚未完成。[真机面板验证](reports/runtime-model/DEVICE-VISUAL-CHECK.md)。[设计与验证范围](reports/runtime-model/FINDINGS.md)。

[0.2.2 真机局中快照结果](reports/message-observer/device-v8/FINDINGS.md)：消息采集与字段备份已验证成功；重连快照含自己的 13 张牌值，其他三家只有 14/13/13 张数量。该结论限于已检查的消息路径，后续转向独立的客户端运行时模型入口。

以下保留各阶段报告，旧报告中的“待验证”指对应阶段当时的状态。

[原生运行环境探针](reports/runtime-probe/FINDINGS.md)已发布，可通过 TrollFools 导入游戏，验证传统消息入口的运行时 API 与类型是否可见。早期 0.1 探针只查询元数据；后续 0.2.x 已实现消息订阅与真机采集，0.3 新增模型显示分支。

[第十六轮：1.2 真机新包](reports/phone-v3/FINDINGS.md)：20/20 个文件完整，新增一局；磁盘记录级别报告为 3，但没有 TraceBin，新增手牌 TLV 已验证归属自己。

[可安装的诊断工具 1.2 Beta 1](reports/mobile/LOG-READER-1.2.md)已发布：新增主动开启记录、状态检查及原设置恢复。设备包与设置事务测试通过，真机记录生成待验证；仍未实现实时对手暗牌显示。

[第十五轮：TraceBin 写入与清理](reports/trace-format/FINDINGS.md)：确认原始字节追加、按文件数清理，以及根目录最终来自 Unity persistentDataPath；现有扫描支持 TraceBin 自定义扩展名，仍无实际负载样本。

[第十四轮：现有二进制跟踪入口](reports/trace-config/FINDINGS.md)：确认 TraceBin 默认关闭、GM 覆盖使用 PlayerPrefs 保存，以及虚拟机初始化和字节写入调用链；未取得实际 TraceBin 样本。

[第十三轮：二进制日志解码](reports/binary-logs/FINDINGS.md)：两份 xlog 的 6 条记录完整解压，已完成 5364 个源码字段与 JSON 结构复查；尚无实时对手手牌证据。

[第十二轮：完整设备包核对](reports/complete-v2/FINDINGS.md)：独立验证 16/16 候选文件，解析设备配置及 3 个热补丁；未取得实时对手暗牌。

[手机日志导出 1.1](mobile/LogReader/README.md)已发布：新增完整选择及覆盖清单，用于导出此前漏选的现有配置、补丁和日志；编译、签名、ZIP 测试及模拟器启动检查通过。

[第十一轮玩家归属路径](reports/device-round/OWNERSHIP-PATHS.md)：核对推荐目标与带座位的能力手牌结构；能力手牌仅找到转换器直接引用，本局对应类型记录为 0，尚无新的实时牌值来源。

[第十轮完整结构与手牌归属](reports/device-round/STRUCTURE-FOLLOWUP.md)：补查 JSON 及非 JSON 牌列表，验证推荐中的当前手牌与自己一致；确认胡牌后四个座位剩余手牌，其他三家为 13、7、4 张，实时暗牌仍未取得。

[第九轮记录 IO 追查](reports/device-round/RECORD-IO-FOLLOWUP.md)：核对协议记录、本地回放、回放响应及内存统计的真实格式；补查 8 个载体字段，在原 ZIP 全部文件中均未命中，16 字节文件仍未与写入者关联。

[第八轮原生与配置追查](reports/device-round/NATIVE-FOLLOWUP.md)：扫描全部 91 个恢复程序集及 34 个原生/元数据目标，解析包内录像配置为 12 行配置表，并发现设备主配置大小与包内版本不同；实时暗牌仍未取得。

[第七轮原 ZIP 覆盖审计](reports/device-round/EXPORT-AUDIT.md)：补齐全部导出文本的负载探测，5 个文件完整，无新增对手暗牌证据；另有 11 个文件仅在扫描清单中，实际未装入 ZIP。

[第六轮协议复核](reports/device-round/PROTOCOL-FOLLOWUP.md)：已核对压缩消息的听牌/推荐消费路径及牌墙结构；当前日志没有可恢复的对应负载，实时对手暗牌仍未验证。

分析输入是用户提供的 Info.plist、UnityFramework 和欢乐麻将_Decrypted.ipa。原始文件位于公开仓库 zwthys-cyber/qqhldzz-analysis 的 inputs-v1 Release 附件中。

[最新业务代码分析报告](reports/managed/FINDINGS.md)记录了 UnityMJ 格式恢复、热更新程序集反编译，以及普通对局、回放、组队和明牌的数据差异。当前尚未证明大众麻将普通对局会下发对手完整暗牌。

原始文件分析、应用构建、格式恢复及反编译在 GitHub Actions 中完成。当前服务器准备源代码、加密输入并读取下载的筛选报告，不执行用户原始输入或本机应用构建。

## 工作流与输出

- `analyze.yml`：输入完整性、文件对应关系、Mach-O 和 IL2CPP 元数据基本检查。输出 ios-analysis-report。
- `reverse.yml`：编译 Il2CppDumper 并恢复 AOT 元数据和原生方法映射。输出 il2cpp-reverse-report。
- `recover.yml`：恢复三个 UnityMJ 包，验证唯一目录、完整块边界及资源解析，提取业务 DLL。输出 unitymj-recovered-report。
- `managed.yml`：编译 Mono.Cecil 元数据读取器，使用 ILSpy 反编译对局业务代码。输出 managed-game-report。
- `dazhong.yml`：进一步查看游戏标识和规则配置来源。输出 dazhong-rule-report。

原始输入没有游戏源码工程、Xcode 构建配置和签名材料；反编译输出也不能直接等同于可重编译的完整原游戏。观察库已在手机上加载并采集消息；新增 0.3 模型入口尚待真机验证，也没有证明实时对手手牌泄露。旧阶段报告保留了当时的结果，UnityMJ 解析障碍已在最新报告中解决。

[第四轮与手机诊断工具](reports/mobile/FINDINGS.md)：已发现新的动态规则引擎路径，进一步限定了传统手牌模型结论的适用范围。提供 [TrollStore 日志导出工具源码与使用说明](mobile/LogReader/README.md)，用于在只有手机的条件下取得现有诊断文件；实际对局消息是否可从日志恢复尚未验证。

[第五轮：实际大众对局日志结果](reports/device-round/FINDINGS.md)：已成功分析用户 iOS 17.0 设备导出的日志。一局 ModuleTuidaohu49 中，52 条等待消息的额外牌字段均为空值；自己的初始手牌和摸牌可见，尚未发现能支持实时对手暗牌显示的数据。明文日志不进入公开 Git 历史。
