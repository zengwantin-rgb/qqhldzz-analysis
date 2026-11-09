# 第四轮：只用手机验证的准备与新规则引擎边界

用户确认只能在 iPhone 15 Pro Max / iOS 17.0 / TrollStore 上操作。本轮继续检查了大众麻将消息字段和日志位置，并准备独立的日志导出应用。

## 对上一轮结论的适用范围作进一步限定

客户端不只有一套游戏逻辑。此前 RevcDealCommand、NextToDoHelper 中“自己用真牌、他人填牌背”的结论适用于已恢复的传统流程，不能直接断言设备当前房间必然采用这条路径。

已保存的 [Common.Util](../managed/evidence/HappyMahjong.Common.Util.cs) 中 IsRuleV2 读取 NewRuleV2Config/UseNewRuleGameType，并可由 NewRuleAdapter、SceneAdapter 等配置转换 game/scene ID。这意味着同一入口名称可能在运行时选择新的引擎路径。

[MJGameContext](evidence/MJGameContext.cs) 的 OnRecvData 将原始消息交给 m_hbpEngine.OnRecvCSMsg；InitVM 从文件读取蓝图二进制，并创建虚拟机。需要实际房间的配置、蓝图与日志来确认正在使用的路径，不能用传统模型代替全部路径分析。

新引擎也有表示未知牌的结构：[GameHBPUtil.GetMJValue](evidence/GameHBPUtil.cs) 将 CardColorSystem、Seq=2 转为 MJDef.MJ_BACK。存在 GameCard 或 PlayerHandTiles 对象本身不证明对象里保存了对手真实牌值。

[HandCardHeap](evidence/HandCardHeap.cs) 通过手牌区的 GetViewCard 获取实例列表，再把实体转成显示对象。实例 ID 和牌值是不同数据；单有每张牌的实例 ID 不能直接解释成具体牌。

## 传统摸牌消息的额外字段

[TDoWaitInfo](evidence/TDoWaitInfo.cs) 包含 chExCmd、nExtraTilesNum、ExtraTiles，默认初始化时 chExCmd=0、nExtraTilesNum=0。NextToDoHelper 仅在回放标志成立时把这些字段用于相应回放恢复分支。

当前未得到实际消息样本，不能证明普通大众房间的这些字段总是空，也不能证明它们装着完整对手暗牌。字段定义和默认初始化值不能代替运行时证据。

## 已核对的现有日志来源

[GitHub 路径逆向任务](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42634832227) 反编译了日志/反馈相关实现：

- GMLogSystemHandler.GetTraceBinDir 使用 PathManager.GetPersistentPath("TraceBinLogs/", null)。
- GMLogSystemHandler.GetConfigFileList 使用持久化目录下 CsConfig/。
- 日志目录来自 TSDKService.GetLogFilePath 原生入口；本轮没有静态确定其最终绝对路径。
- FeedBack.MakeTSDKLogFileName 使用 398_yyyy-MM-dd_HH.log 格式。

这些实现仅作为位置和命名证据。本轮没有调用游戏的上传接口，也没有向开发商或其他地址发送用户数据。

## 纯手机工具

[麻将日志导出源码与说明](../../mobile/LogReader/README.md)，[GitHub 编译工作流](../../.github/workflows/log-reader.yml)。

独立应用按 com.tencent.happymj 精确识别欢乐麻将的数据容器，列出候选日志、回放、规则配置和补丁文件；用户选中文件后，在工具自己的目录生成 ZIP，再通过系统分享菜单保存或发送。默认不选文件，不联网，不修改游戏文件。

权限参照 [TrollStore 官方 Unsandboxing 文档](https://github.com/opa334/TrollStore#unsandboxing)，使用 no-sandbox、platform-application、AppDataContainers。没有请求官方列为禁用的动态签名或调试 entitlement。

工具导出的是已经存在的文件，不是实时消息抓取器、进程内存读取器或透视显示层。编译、ZIP/隔离测试、签名检查及模拟器启动检查在 GitHub 执行；真机权限、文件内容和运行时状态需要实际安装后检查。

如日志为空、加密或不含相关消息体，不能从空结果断言服务器没有下发数据。导出 manifest.json 的意义是确认文件可见性、版本和读取限制，不能代替对局消息本体。

## 可下载的构建与检查结果

[下载 MJLogReader.ipa](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/download/log-reader-v1/MJLogReader.ipa)，[Release 与手机安装说明](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/tag/log-reader-v1)。

[最终产物核验任务已成功](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42639002456)，校验记录保存在 [build-validation.json](build-validation.json)。IPA 对应源代码提交 2ba56c51bcbc4a991d99413979e3b141a5c91350。

源构建中的扫描隔离/ZIP 测试、ARM64 编译签名、模拟器启动、打包和权限检查步骤均成功，已留存模拟器界面截图。源工作流在等待结束时取消，发布步骤中断；最终核验任务重新检查了各成功步骤和不可变构建附件的 SHA-256，随后由维护账号发布同一附件。不能将源工作流的整体 cancelled 状态写成整体 success。

在 Mac Foundation 测试中发现并修正了 /var 与 /private/var 别名造成的目录比较问题。导出 ZIP 还由独立 Python ZIP 读取器校验 CRC 和中文文件名。所有编译与测试均在 GitHub 执行，当前服务器仅准备源码、读取检查产物和上传已核验的发布附件。

尚未在用户 iOS 17.0 真机安装运行；跨容器访问权限和实际文件内容仍需安装后确认。下一步是用户在大众房间完成一局，使用工具扫描并选择该时段日志，导出 ZIP 后提供给分析；如没有候选文件，也可导出只含清单的 ZIP。
