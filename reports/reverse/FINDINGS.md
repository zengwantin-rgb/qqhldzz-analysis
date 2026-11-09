# 第二轮逆向分析：能否获得对手手牌

结论：尚未发现普通实时对局向客户端下发对手完整暗牌的证据。本次恢复了类型、字段、方法签名及原生方法映射，但未恢复热更新包中的主要业务代码，也没有运行设备端对局或采集其网络、内存数据。不能据此断言“能透视”，也不能断言绝对不能。

## 已完成的远程分析

- [IL2CPP 恢复任务（最终无交互版本）](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42630448936)：GitHub 上编译固定提交的 Il2CppDumper，恢复 dump.cs、原生方法地址、结构和 DummyDll。DummyDll 是结构占位程序集，不是恢复了完整方法实现的游戏源码。
- [热更新资源检查](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42630560150)：GitHub 上用 UnityPy 1.25.4 检查包内资源。
- 元数据版本 29，包含 19,521 个类型、171,297 个方法、104,326 个字段。完整输出在逆向任务的 il2cpp-reverse-report 附件中。
- 最初 Dumper 已生成输出，但在等待控制台按键时退出异常。随后设置 RequireAnyKey=false 并重新运行，最终任务成功。不能将最初的退出异常解释为注册恢复失败。

## 与目标有关的实际证据

| 位置 | 恢复结果 | 能说明什么 |
| --- | --- | --- |
| HappyMahjong.TSDKMessage.MJTiles | 字段 falg、tile、which | 存在麻将牌数据结构；尚未确定字段语义和网络数据来源 |
| HappyMahjong.TSDKMessage.PlaerRecordConst | MAX_HAND_TILES_NUM = 17 | 存在记录相关的手牌数量上限；不是对手手牌数组的证据 |
| TSDKToUnity | NotifyGameMessage、OnRequestViewRsp、NotifyOBEnter、CGIPlaybackRecord | 存在游戏消息、查看/观战及回放相关命名；没有证明这些消息的实际内容、触发条件或权限 |
| HappyMahjong.LoadDll | LoadDLLFromAb、LoadHotLoadAssemblies、LoadBootstrapAB、StartBootstrap | 存在从资源包加载程序集的代码入口 |
| 已恢复程序集 | ILRuntime、HybridCLR.Runtime | 客户端包含运行时加载代码相关组件；单看 AOT 元数据不能覆盖全部业务代码 |

以上是静态名称与类型证据。观战、回放、结算可以合法展示某些阶段的牌，这与普通玩家在对局中收到对手暗牌是不同命题。

## 热更新包检查结果与限制

在 IPA 的 Data/Raw/InerIOS/rawresources_new/ 中找到：

- bootstrap_ab_v1.bundle：28,308 字节。
- aotdll_ab_v1.bundle：10,562,449 字节。
- hotloaddll_ab_v1.bundle：14,494,830 字节。

三者均以 UnityMJ 开头，并带有 Unity 2021.3.15f1 版本字符串。标准 UnityPy 没有解析出对象；仅把 UnityMJ 签名替换为 UnityFS 后，三者均出现解压错误。这说明仅更换文件头不足以解析，尚不能确定其完整变换、压缩或加密方式，不能直接断言资源损坏。

另外提取出 10 个以 MZ 开头的 .dll.bytes 文件，名称多为桌面装饰主题；本轮未把它们认定为对局业务逻辑，也未执行这些程序集。

进一步回答是否收到对手暗牌，需要先恢复热更新资源的真实格式并追踪消息反序列化、玩家牌模型赋值及回放/观战分支。最终还需要与普通对局的实际数据对照；静态符号和伪造的测试消息不能代替这项证据。

## 方法来源

使用 [Il2CppDumper 固定提交](https://github.com/Perfare/Il2CppDumper/tree/4741d46ba9cd6159c5d853eb9d6fc48b4bfa2b1a) 的元数据定义与恢复工具，以及 [UnityPy](https://github.com/K0lb3/UnityPy) 的资源读取功能。自编写的元数据扫描脚本只筛选候选名称，包含无关匹配，不能直接作为漏洞检测结论。

原始文件未修改。所有分析脚本执行、工具编译和验证都在 GitHub Actions 中完成；当前服务器只准备脚本、推送并读取下载的报告。
