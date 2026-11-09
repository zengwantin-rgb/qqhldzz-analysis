# 手机麻将诊断工具 1.2

[下载 1.2 Beta 1 IPA](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/download/log-reader-v1.2-beta1/MJLogReader.ipa) · [功能与验证范围](../../reports/mobile/LOG-READER-1.2.md)

1.2 增加实验性的“诊断记录”：检查磁盘上的记录级别、开启完整 TraceBin 记录请求、恢复原设置。开启和恢复都要求先从多任务界面完全关闭游戏；工具只调整已经从源码确认的一个整数偏好键，操作前保存原值，写入后重新读取核对。其他偏好不导出、不写入备份，恢复时保留它们的后续变化。

使用顺序：安装 1.2 → 底部“诊断记录” → 开启完整记录 → 重新打开游戏进入对局 → 返回工具检查设置与记录文件 → 扫描、完整选择、导出。此时需要新的对局，是为了验证新开启的记录路径；无需再次发送此前相同 ZIP。

**这是待真机验证的诊断功能，不是已实现的对手手牌显示。** 游戏的偏好缓存可能覆盖磁盘设置，当前房间也可能不使用蓝图 TraceBin 路径。界面分别报告磁盘值、非空跟踪文件数及开启后更新的文件数，不把设置读回当作运行时采集成功。没有原值备份或记录值被其他操作改变时，恢复操作会拒绝覆盖。

备份保存在工具自身 Documents/TraceSettingsBackup，不包含游戏完整偏好文件，不会自动加入日志 ZIP。原游戏偏好文件不存在、格式异常、目标包名不符或路径含非系统符号链接时拒绝修改。默认启动、扫描和导出仍不会修改游戏设置；只有用户主动选择开启/恢复并在界面确认后才写入。没有进程注入、内存读取或网络捕获能力。

以下为文件导出流程与历史版本说明；其中“只读”适用于扫描导出和 1.0/1.1，1.2 的主动诊断设置操作是上述明确例外。

# 麻将日志导出（TrollStore）

[下载 1.1 IPA](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/download/log-reader-v1.1/MJLogReader.ipa) · [1.1 构建与验证](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42661993471) · [校验记录](../../reports/mobile/build-validation-v1.1.json)

1.1 新增“完整选择”、配置/补丁优先选择、未导出原因及完整性清单。构建、签名、选择/覆盖测试、独立 ZIP 校验及模拟器启动检查均已通过。

适用于已有 TrollStore 的设备，构建目标为 ARM64、iOS 15 及以上；用户目标设备为 iPhone 15 Pro Max / iOS 17.0。

这是独立的只读文件导出工具，用来检查欢乐麻将已经保存的日志、回放和规则配置候选文件。它不会注入游戏、读取运行内存、抓取网络消息、向游戏服务器发送请求或显示对手手牌。日志可能没有原始消息体，也可能是加密格式。

1. 在 Safari 下载 Release 中的 MJLogReader.ipa，通过分享菜单用 TrollStore 打开并安装。
2. 已有对局日志可直接扫描，无需重新打一局。
3. 打开“麻将日志导出”，点击“扫描”，再点底部“完整选择”。检查已选数量与候选总数；也可手动调整。
4. 点击“导出”，在系统分享菜单里存储到“文件”。扫描截断、未选文件和读取失败均记录在 manifest.json 中；ZIP 名称带 complete、partial 或 manifest_only 状态。
5. 如未发现文件或报访问错误，也可直接导出仅含 manifest.json 的诊断清单，帮助确认容器是否可读。

工具精确匹配原包名 com.tencent.happymj；改包名分身不会自动扫描。单文件上限 20 MB、ZIP 内容总上限 64 MB；不会自动选择文件或上传。导出文件只写入工具自己的 Documents/Exports；可以通过“文件”管理导出副本。

“完整选择”由用户主动点击，优先保留已知规则配置、补丁及主日志/TraceBinLogs，再在大小预算内选择其他候选。超过限制的项目会列入 selection_plan_skips；未选择的文件、实际读取失败和扫描错误分别保留，不会被标成完整导出。complete 仅表示本次扫描发现的候选全部导出，不能代表游戏所有数据或所有网络消息，也不能恢复已删除的旧文件。

权限根据 [TrollStore 官方文档](https://github.com/opa334/TrollStore#unsandboxing)配置为 no-sandbox、platform-application 和 AppDataContainers。未使用官方列为禁用的调试/动态代码签名 entitlement，没有 root helper。

GitHub Actions 编译 IPA，验证签名和 entitlements，使用临时测试目录检查目标容器隔离、符号链接过滤与只读扫描，并由 Python 独立验证 ZIP 的 CRC 和 Unicode 文件名。另有模拟器编译和启动检查；1.1 的新增选择与覆盖状态流程仍需在用户手机上确认。

1.0 已在用户的 iOS 17.0 设备成功导出日志；1.1 的新增完整选择流程尚未在该设备运行。1.1 对应源码提交 bcb1990947be4cd7c1adcf7b261a38bdc137a3ad，发布 IPA 的 SHA-256 为 `36dcd1c709fc2284d60307d5de775597aa9d3a3a5676faca5342d95ff44c168b`。完整 16 文件导出及旧版 5/16 漏选场景均有测试覆盖。

完整源码和工作流保存在仓库中。请不要把“编译通过、能导出文件”理解为“已确认普通对局下发对手暗牌”。
