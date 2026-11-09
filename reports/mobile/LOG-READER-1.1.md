# 日志导出 1.1：补齐候选文件与覆盖状态

[下载 IPA](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/download/log-reader-v1.1/MJLogReader.ipa) · [Release](https://github.com/zengwantin-rgb/qqhldzz-analysis/releases/tag/log-reader-v1.1) · [构建任务](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42661993471)

原设备 ZIP 的清单有 16 个候选，实际只导出了 5 个，缺少另一份主日志、设备主配置、补丁等实际内容。1.1 为这项取证缺口提供明确的“完整选择”操作，已有文件可直接扫描，不要求新打一局。

## 使用与变化

安装后执行“扫描 → 完整选择 → 导出”，检查已选数量与候选数量。完整选择在大小预算内优先保留已知规则配置、补丁、主日志及 TraceBinLogs；限制内的其他候选也会选入。用户可继续手动调整，文件仍默认不选中，分享由用户主动操作。

清单 schema 更新为 2，保留旧版 exported_files、skipped_files、scans 等字段，新增 selection_mode、selection_plan_skips 及 coverage。coverage 区分 complete、partial 与 manifest_only，列出候选、已选、成功、未选、失败数量以及扫描错误和截断状态。导出后的界面和 ZIP 文件名也显示结果状态。

complete 只表示本次发现的候选全部成功导出。扫描截断、错误、读取失败、未选择、重复导出路径或缺失预期条目都会影响完整性判断。它不表示掌握游戏所有数据，也不能恢复设备已删除的旧文件。

## 验证

最终构建在 macos-15-intel 完整成功，源码提交为 bcb1990947be4cd7c1adcf7b261a38bdc137a3ad：

- Foundation 测试覆盖目标隔离、只读扫描、敏感路径与符号链接排除、完整 16 项选择、5/16 时明确列出 11 项未选、读取失败、重复路径、截断扫描、空清单及大小预算优先级。
- 独立 Python ZIP 读取器验证 CRC、Unicode 文件名、16 个文件正文及覆盖清单；完整 ZIP 有 16 个候选文件和 1 个 manifest.json。
- ARM64 编译、ad-hoc 签名、权限与包内版本检查通过；权限与 1.0 相同。
- 模拟器编译、安装、启动及截图检查通过；[界面截图](log-reader-v1.1-simulator.png)保留了完整选择入口。

发布附件与构建产物的 SHA-256 一致，并已从 Release 再下载核对。校验记录见 [build-validation-v1.1.json](build-validation-v1.1.json)。

1.0 已由用户真机导出验证；1.1 新流程仍需实际安装后确认。工具导出已有文件，用于下一轮检查缺失负载、配置与补丁；实时对手暗牌显示仍未实现。游戏文件没有被修改，也没有新增进程内存或网络采集能力。
