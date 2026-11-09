# 欢乐麻将运行环境探针 0.1.0（测试版）

[下载 dylib](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/download/runtime-probe-v0.1.0-beta1/MJRuntimeProbe.dylib) · [构建与范围](../../reports/runtime-probe/FINDINGS.md)

用于确认 iOS 17.0 / TrollStore 环境下，游戏内的 IL2CPP API、传统消息服务和事件接口是否可访问。它尚未订阅对局消息，不采集牌值，也不显示对手手牌。

## 手机操作

1. 把 MJRuntimeProbe.dylib 保存到“文件”App。
2. 完全关闭欢乐麻将。如果需要退出 1.2 的记录设置测试，可在此时用工具恢复原设置。
3. 打开 TrollFools，选择原包名欢乐麻将，将这个 dylib 作为注入库导入。
4. 打开欢乐麻将，在大厅等待约 70 秒即可，这一步不用再打一局。
5. 完全关闭游戏，打开诊断工具 1.2，执行“扫描 → 完整选择 → 导出”。新结果位于 Documents/MJRuntimeProbe/runtime-probe.log，会随其他 .log 文件一起导出。

若游戏不能正常启动，在 TrollFools 中移除这个诊断库后再启动。不要为了这项测试卸载游戏。测试结束后也可通过 TrollFools 移除此库。

## 范围与验证

只在 com.tencent.happymj 进程启用。探针仅查询预先指定的 API 可用性和 7 个源码类型的元数据，不读取玩家对象或静态字段的实际值，不调用游戏业务方法，不修改接收函数，不改变对局逻辑，不联网或自动上传。

每次启动最多执行 3 次元数据检查，分别在约 10、30、60 秒；日志限制为 64 KiB，拒绝覆盖同名的其他内容或跟随输出路径中的符号链接。库只依赖系统框架，无需其他 hook 库。

GitHub Actions 已通过输出文件测试、ARM64 编译、ad-hoc 签名和依赖检查。真机加载和运行时类型查询尚待此次验证。详见 runtime-probe-validation.json。来源：https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42693999221

TrollFools 官方项目：https://github.com/Lessica/TrollFools
