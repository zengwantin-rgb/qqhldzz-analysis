# 原生运行环境探针 0.1.0

[下载 MJRuntimeProbe.dylib](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/download/runtime-probe-v0.1.0-beta1/MJRuntimeProbe.dylib) · [测试版 Release](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/tag/runtime-probe-v0.1.0-beta1) · [手机操作](../../mobile/RuntimeProbe/README.md)

收到 1.2 真机新包后，确认本局传统消息仍有输出，但没有蓝图 TraceBin。用户已确认安装 TrollFools，本次交付可实际导入游戏的原生 dylib，先验证进程内消息采集的技术前提。它仅查询元数据，尚未订阅游戏消息或显示对手暗牌。

## 接口依据与探针行为

[程序集检查](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42693484243)从 91 个恢复程序集读取了 6 个选定源码类型。[接口索引](event-observer-schema.json)确认 TSDK4CSharpService 的静态 dispatcher 属性及 backing field、IEventDispatcher 的 AddListener(Object, EventCallback)、EventCallback(IEvent) 委托形状，以及传统游戏消息枚举 9 和 protobuf 游戏消息枚举 217。值是源码常量，不是抓包结果。直接字段引用扫描未还原枚举常量对应的订阅者，不能将 candidates 为空解释成没有订阅者。

探针限定 com.tencent.happymj，约在启动后 10、30、60 秒执行三次检查。查询已加载 UnityFramework 的预设 IL2CPP API、7 个固定类型是否存在、少量方法的参数数量，以及指定字段的元数据是否存在。主线程上保留既有线程附着状态；只有自行附着的线程才在检查后分离。

没有读取玩家对象或静态字段的实际值，没有调用游戏业务方法、添加事件监听器、修改接收函数、替换指令或请求动态代码签名，也没有网络请求。它只依赖系统 Foundation、CoreFoundation、libobjc 和 libSystem。

结果写入游戏自身 Documents/MJRuntimeProbe/runtime-probe.log，已有 1.2 导出器可发现这一 .log。文件带固定格式头、限制 64 KiB、权限 0600，采用追加方式；文件和目录符号链接、同名但不属于探针的内容会被拒绝。原始游戏日志和设置不被探针修改。

## 构建与实际限制

[构建 42693999221](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42693999221)完整成功，源码 b713545ea118bcfe625ba28a8c850b4391628893。测试覆盖目标隔离、私有追加文件、已有内容保护、文件/目录符号链接和大小限制；ARM64 编译、ad-hoc 签名、部署信息与依赖检查通过。SHA-256 为 d8ba5b43305cbb6d38b1bc8fc4cf1bc1574a7d8d3816e5c05b4e6e64f993b28a，发布后已重新下载核对。见[构建校验](build-validation.json)。

TrollFools 的[官方项目](https://github.com/Lessica/TrollFools)说明支持其列出的 TrollStore 系统范围内的应用注入；这不保证本游戏在用户设备上的实际加载结果。本探针尚未完成真机注入和运行时查询验证。若启动受影响，应在 TrollFools 中移除此诊断库；无需卸载游戏。

已准备只输出固定 API/类型词汇、布尔值和有限计数的日志解析器；未知字段、任意字符串和不完整记录不会进入公开输出，并按每次 library_loaded 划分启动会话，避免继承旧会话的成功状态。真正能否订阅解码后的对局事件，要根据手机返回的 API 和类型可用性继续实现，不能把构建成功当作运行时采集成功。
