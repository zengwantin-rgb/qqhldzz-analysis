# 第十四轮：现有二进制跟踪入口

此前未完成的 xlog 完整字段复查现已成功，见[第十三轮更新](../binary-logs/FINDINGS.md)：两份解压文本共 4783 行，没有识别到源码目录中的游戏 JSON 字段，数字数组长度仅为 0 或 1。没有取得实时对手手牌。本轮进一步核对客户端已有的二进制跟踪配置，避免继续重复导出相同文件。

## 已恢复的级别选择

91 个恢复程序集中的 HappyMahjong.Common.HBPUtil.GetTraceBinLevelByDeviceType 按以下顺序选择级别：先走 IFix 替换检查；未替换时，如 GMModel.VMTraceBinLevel 不为 -1，直接返回；否则读取 DynamicConfig 的 HBPGame/TraceBinLevels，按逗号拆成 6 项后由 Device.type 索引。项数不为 6 时返回 0。GetVMLogLevelByDeviceType 使用相同结构，但配置项为 VMLogLevels，项数不为 6 时返回 1。

| TraceBinLevel | 数值 |
|---|---:|
| TRACE_LEVEL_OFF | 0 |
| TRACE_LEVEL_ONLY_FLOW | 1 |
| TRACE_LEVEL_VAR_NO_REF_ENTITY | 2 |
| TRACE_LEVEL_ALL | 3 |

这是客户端跟踪级别枚举，并不说明每一级实际记录哪些消息内容。级别名称不能代替真实二进制样本。

SelectionStartCommand.InitHbpEngineService 调用 SimpleTraceBinWriter.SetTraceBinRoot，并把 OnTraceBinLog 接入日志回调；MJGameContext.InitVM 读取上述级别后创建虚拟机。回调支持开始、写入、结束与重命名。程序包含这条路径不代表本次传统对局使用了蓝图虚拟机。

完整设备包的两份主日志中，InitVM、CreateVirtualMachine、新引擎消息入口及 TraceBinLogs 标记均未命中；扫描候选也没有实际 TraceBin 文件。该结果只限定本包的可见记录，不能证明运行时绝不存在对应操作。

## 默认值与保存方式

扩展检查确认 TraceBinLevels 的静态默认字符串为 `0,0,0,0,0,0`，即六档设备均默认关闭；VMLogLevels 的默认字符串为 `1,1,1,1,1,1`。设备档位枚举为 SuperLow、Low、LowNormal、Normal、High、SuperHigh，对应索引 0–5。动态配置的 GetString 在找不到配置时返回调用方传入的默认值。

GMModel 的 VMTraceBinLevel 与 VMLogLevel 属性通过 UnityEngine.PlayerPrefs.GetInt / SetInt 读取和保存，默认读取值为 -1。它们不是仅存于当前模型的普通内存字段。GMVMSystemHandler.OnSetTraceBinLevel 将输入解析成整数并设置该属性，但方法本身没有调用虚拟机重建或运行中修改级别接口。已恢复的 MJGameContext 在 InitVM 时读取级别；不能假定设置后立即影响已经创建的虚拟机。也未证明用户版本能访问这套 GM 界面。

SelectionStartCommand.InitHbpEngineService 另受 HBPGame/EnableHBPGame 控制，静态默认 true，并调用 CheckDeleteOldRecord。根目录设置在持久化 TraceBinLogs/ 下，写入回调通过 tag 查找或创建 TraceBinDefaultWriter，再调用 TraceBinWriter.Write(byte[])；结束时调用 Finish 并删除对应 writer 映射。这只是找到字节写入调用链，未解析最终 writer 的二进制序列化格式，也未取得其输出样本。

本轮没有读取或修改设备 PlayerPrefs，没有修改动态配置，没有调用 GM 设置、文件上传、删除或游戏代码。磁盘补丁也不能与当时实际加载补丁直接等同。默认关闭可以解释无跟踪文件的一种可能性，但无法证明本机当时的有效值；传统游戏流程是否进入该新引擎路径仍须独立验证。

## 验证与当前边界

[首次检查](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42676154265)与[扩展检查](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42676425546)均成功。扩展检查源码为 2965f9c，.NET 8 编译通过；扫描全部 91 个恢复程序集，输出 20 个选定方法、2 个枚举、18 个直接调用者。证据为[受限 IL 索引](trace-config.json)，包含指令、数值常量、源码方法/字段名与少量诊断配置键；任意字符串常量均被省略。此操作只读取程序集，不执行游戏代码。

现有导出器读取已经存在的文件，无法单靠扫描启用尚未产生的记录。本轮完成了此前被调度错误阻断的完整字段检查，并将新诊断来源落实到默认值、偏好保存与写入调用链。仍无可供实时展示的对手暗牌，也没有已验证的手机端消息采集或显示功能。

后续更新：[第十五轮](../trace-format/FINDINGS.md)已核对最终 writer、底层 BinaryWriter、根目录来源与清理排序；上文未解析 writer 的表述保留本轮完成时的历史状态。
