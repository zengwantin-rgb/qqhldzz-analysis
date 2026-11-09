# 麻将日志导出（TrollStore）

适用于已有 TrollStore 的设备，构建目标为 ARM64、iOS 15 及以上；用户目标设备为 iPhone 15 Pro Max / iOS 17.0。

这是独立的只读文件导出工具，用来检查欢乐麻将已经保存的日志、回放和规则配置候选文件。它不会注入游戏、读取运行内存、抓取网络消息、向游戏服务器发送请求或显示对手手牌。日志可能没有原始消息体，也可能是加密格式。

1. 在 Safari 下载 Release 中的 MJLogReader.ipa，通过分享菜单用 TrollStore 打开并安装。
2. 在欢乐麻将大众房间完成一局，记下大致时间和房间名称。
3. 打开“麻将日志导出”，点击“扫描”。
4. 选择刚才时间附近的候选日志或回放，点击“导出”，在系统分享菜单里存储到“文件”。
5. 如未发现文件或报访问错误，也可直接导出仅含 manifest.json 的诊断清单，帮助确认容器是否可读。

工具精确匹配原包名 com.tencent.happymj；改包名分身不会自动扫描。单文件上限 20 MB、ZIP 内容总上限 64 MB；不会自动选择文件或上传。导出文件只写入工具自己的 Documents/Exports；可以通过“文件”管理导出副本。

权限根据 [TrollStore 官方文档](https://github.com/opa334/TrollStore#unsandboxing)配置为 no-sandbox、platform-application 和 AppDataContainers。未使用官方列为禁用的调试/动态代码签名 entitlement，没有 root helper。

GitHub Actions 编译 IPA，验证签名和 entitlements，使用临时测试目录检查目标容器隔离、符号链接过滤与只读扫描，并由 Python 独立验证 ZIP 的 CRC 和 Unicode 文件名。另有模拟器编译和启动检查。模拟器测试不能证明 TrollStore 真机权限有效；iOS 17.0 真机上的跨容器读取与实际日志内容仍需用户安装后验证。

完整源码和工作流保存在仓库中。请不要把“编译通过、能导出文件”理解为“已确认普通对局下发对手暗牌”。
