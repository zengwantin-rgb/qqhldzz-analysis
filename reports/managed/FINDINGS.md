# 第三轮：热更新包已恢复，继续追踪大众麻将手牌数据
> 适用范围更新：客户端可动态切换新的规则引擎，下面的牌背占位证据仅覆盖已分析的传统路径。请同时阅读[第四轮报告](../mobile/FINDINGS.md)。


本轮已解决上一轮的 UnityMJ 解析障碍，并读取到真实的 .NET 方法实现。当前对普通对局的静态证据显示：客户端用真实牌值初始化自己，用 MJ_BACK 占位初始化其他座位；其他玩家的普通摸牌也默认使用牌背。尚未证明大众麻将普通对局会额外下发完整对手暗牌，因此不能承诺通过修改显示或开启回放标志实现实时透视。

这比“只找到相关字段名”更进一步，但依然不是服务器运行时数据的实测结论。客户端本身包含热补丁分支，运行时也可能加载不同的配置和代码。

## 自定义资源格式恢复

参照 [UnityPy 的标准 UnityFS 读取实现](https://github.com/K0lb3/UnityPy/blob/master/UnityPy/files/BundleFile.py)，比较三个包的长度与字节分布，验证了以下变换：

1. UnityMJ 签名对应自定义容器；为标准解析器制作副本时换成 UnityFS。
2. 包头中的目录压缩长度与解压长度相对于标准格式交换。
3. 仅对实际压缩目录数据做单字节 XOR 还原。各包枚举 256 个候选键后，要求目录记录全部合法且恰好消费完整目录。
4. 目录后的数据块按标准布局读取。三个样本均完整解析出资源和程序集，原始文件未修改。

| 包 | 实际目录压缩字节数 | 目录解压字节数 | 样本 XOR 键 |
| --- | ---: | ---: | --- |
| bootstrap | 65 | 91 | 0xE4 |
| aotdll | 876 | 1871 | 0xD4 |
| hotloaddll | 1442 | 3181 | 0x44 |

这是对提供的三个样本验证的恢复方法，没有声称上述键或推导方式适用于所有版本。验证同时要求每个包仅有一个合法目录候选、数据块结束位置与文件大小一致，并能成功读取 TextAsset。

[恢复与完整性检查通过的 GitHub 任务](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42631920188)。[恢复脚本](../../tools/recover_unitymj.py)和[原始恢复结果](bundle-recovery.json)已保存。

## 恢复的业务程序集

| 程序集 | 文件字节数 | 类型数 | 方法数 | 包含方法体的方法数 |
| --- | ---: | ---: | ---: | ---: |
| HappyMahjong.InGameScripts | 16,292,864 | 6,062 | 70,051 | 69,233 |
| HappyMahjong.Hall | 21,176,832 | 10,522 | 90,453 | 88,616 |
| HappyMahjong.Bootstrap | 56,832 | 19 | 177 | 172 |
| Assembly-CSharp | 3,028,992 | 322 | 6,330 | 6,318 |

在 GitHub 上编译 Mono.Cecil 读取工具，使用 ILSpy 9.1.0.7988 反编译了对局程序集和 Bootstrap。这些业务 DLL 具有实际 IL 方法体，区别于上一轮生成的 DummyDll。反编译输出不是原始工程，也不等于恢复了资源、构建环境和服务端。

[方法读取及反编译任务](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42631716994)，完整反编译输出在该任务的 managed-game-report 附件中。本目录 evidence/ 保存了以下结论所引用的部分文件。

## 普通发牌与回放的区别

[RevcDealCommand.cs](evidence/RevcDealCommand.cs) 的 GetAllPlayerMJ：只有 sIsReplayFromServer() 与 IsObStatus() 同时满足时才进入 AddPlayerCardInReplay；否则调用 AddPlayerCard。

普通 AddPlayerCard：

- 视角座位 0 使用 CNotifyDeal.nTilesNum 与 szTiles 构造自己的牌列表。
- 其他座位按初始手牌数填入 MJDef.MJ_BACK；庄家第十四张同样填牌背。
- allPlayerMJs 因而不代表一定保存了所有人的真实牌：在普通分支中，它主要包含自己的真牌和其他人的占位。

AddPlayerCardInReplay 会读取 TLV 中按座位标识的 THandTiles，属于另一条有数据前提的路径。单独修改分支条件不能生成缺失的数据。

[DealGameReplayListener.cs](evidence/DealGameReplayListener.cs) 的 AppendDealTileTlv 更明确地说明回放数据如何补齐：它从 TRoundEnv.PlayerEnv[i].InitTiles 生成每个座位的 THandTiles，追加到回放消息。不能把回放中出现所有人初始牌误认为实时普通发牌消息本来就携带这些数据。

## 普通摸牌与特定可见信息

[NextToDoHelper.cs](evidence/NextToDoHelper.cs) 中：

- HandleSelfRedraw 使用自己的 TDoGiveInfo.chTile 或 DrawTiles。
- HandleOtherRedraw 在没有其他合法牌值来源时，给其他座位加入 MJDef.MJ_BACK。
- 回放时才从 chExCmd、extraTiles 等额外数据恢复相应牌值。
- ShowOtherRedrawTile 还处理组队、自身特定听牌状态、明牌座位、并行摸牌玩法等分支，但都依赖消息中实际存在对应 TLV；条件成立本身不能产生牌值。
- HandleNextToDo 的 othersPai 默认是 -1。已查看的普通消息接收调用使用默认值，没有证明存在一个总是填好的对手摸牌缓存。

[RevcGiveNotifyCommand.cs](evidence/RevcGiveNotifyCommand.cs) 中的 HAND_TILES、KEY_HAND_TILES 分支会用于听牌后显示、规则允许的手牌/关键牌信息。它们说明协议能表达这类数据，不能证明普通未公开暗牌也会下发。

## 牌桌状态重建

[TMJPlayerInfo.cs](evidence/TMJPlayerInfo.cs) 的基本玩家信息仅有座位、手牌数量、状态和标志，没有手牌数组。

[RevcGameReplayCommand.cs](evidence/RevcGameReplayCommand.cs) 中的状态重建逻辑先按其他座位的手牌数量填充牌背，自己的手牌另从 THandTiles 设置。该类名含 Replay，但不能据此把全部调用都解释为录像回放；代码同时用于牌桌状态重建，必须结合调用场景判断。

## 当前结论与尚缺的证据

用户目标是大众麻将。当前共享发牌、摸牌与状态重建代码支持的结论是：默认模型中的其他座位暗牌为占位，不是“已经有全牌，只是模型被 UI 盖住”。客户端还支持回放、队友、明牌、特定听牌状态等不同数据分支。

本轮没有在手机上运行修改版，没有实际对局消息样本，没有核对服务器是否在某一具体房间发送了额外数据，也没有读取设备实际加载的热补丁和配置。因此不能将静态结果升级为“所有大众房间都绝不下发”，也没有产出可用的实时透视 IPA。

后续运行时核对需要确定实际 room/scene/game ID、是否回放或观战、所加载的代码版本，以及普通发牌/摸牌消息里是否存在其他座位的手牌 TLV 与真实牌值。制造测试消息、修改回放标志或强制显示牌背模型都不能代替这项证据。

## 聚焦大众麻将后的标识与配置核对

[大众规则配置检查任务已通过](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42633010271)。检查使用恢复出的 Hall 程序集，不执行游戏代码。

[HappyMahjong.Common.Util.cs](evidence/HappyMahjong.Common.Util.cs) 提供了更具体的映射：ServerGameTypeToString 将服务端类型 2、3、20 标记为“大众”；GetGameTypeByRoomType 将这三个类型映射为 ModuleTuidaohu49。[GameType 枚举](evidence/HappyMahjong.Common.GameType.cs) 中 ModuleTuidaohu49 = 149。此外还有新大众血战、组队、无限胡等不同类型，不能仅凭“大众”名称把它们都当成同一套运行规则。用户设备当前房间的实际类型仍未采集。

[GameModule.MJGameBaseConfigUtil.cs](evidence/GameModule.MJGameBaseConfigUtil.cs) 显示：基础规则从 MJGameBaseConfig.data 取得，按 gameID 读取分号分隔的 configValues；配置可以从资源管理器请求并更新缓存。私人房间配置还可通过 ChangeBaseConfigBySvrConfig 修改基础规则字段。因此安装包内的静态定义不足以确定当前房间实际采用的全部规则。

对“大众麻将”目标，本轮能明确指出普通初始化/摸牌中的牌背占位、真实牌值的特定消息入口，以及必须实测的具体消息条件；尚没有取得“普通大众房间始终向本机下发另外三家完整暗牌”的证据。
