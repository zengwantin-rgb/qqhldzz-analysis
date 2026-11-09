# 第十五轮：TraceBin 写入、清理与导出范围

已确认 TraceBin 的托管写入层直接保存引擎提供的字节，没有在这一层增加记录头或压缩。清理规则受文件数量配置控制，不是固定保留若干天。当前完整设备包仍没有 TraceBin 样本，无法据此解析实际记录或取得实时对手暗牌。

## 文件写入层

Happy.Blueprint.TraceBinDefaultWriter 构造时检查文件是否存在：不存在用 FileMode.OpenOrCreate，存在用 FileMode.Append；访问为 Write、共享为 Read。随后用 BinaryWriter 包装流。Write(byte[]) 在 writer 和参数非空时调用 BinaryWriter.Write(byte[])，Finish() 调用 Dispose 并将 writer 置空，终结器也调用 Finish。

因此，同一路径的既有字节不会被该构造流程清空，多个回调的数据可连续追加。此层没有写入长度、magic、JSON 封装或压缩流；引擎提供的 byte[] 内部仍可能有这些结构。不能把“托管 writer 未加密/压缩”扩大成“实际 TraceBin 一定是明文”。没有真实文件或生产者格式定义，不能发明一个解码器，也不能将 .xlog 的 Mars 格式套用到 TraceBin。

SimpleTraceBinWriter 按 tag 在字典中管理 writer，将根目录与 tag 组合成路径；WriteTraceBin 会在尚无 writer 时先创建，随后写入 byte[]。现有接口还支持完成与重命名。它能记录什么取决于引擎回调实际提供什么，不能因为写入的是字节数组就宣称包含全部网络消息或其他玩家暗牌。

## 清理条件

SelectionStartCommand.InitHbpEngineService 读取 HBPGame/MaxBinFileNum，静态默认 -1，并传给 CheckDeleteOldRecord。未被 IFix 替换时，只有配置值大于 0 才执行清理；0 或负数跳过。执行时只列出给定目录的直接文件，按 File.GetLastWriteTime 升序排序，从索引“文件数 − 保留数 − 1”向前删除至 0，保留排序末尾的指定数量文件。

这提供了文件缺失的一种条件性解释，但没有本机当时的动态配置或删除记录，不能断定 TraceBin 曾生成后被删。默认关闭跟踪与默认不清理是两个独立设置。

## 与现有导出器的关系

日志导出 1.1 的 MJScanContainer 遍历目标应用的整个数据容器。MJIsCandidate 对 TraceBinLogs/ 目录单独匹配，接受自定义扩展名；既有构建测试已覆盖 Documents/TraceBinLogs/session.capture。因此，普通相对 tag 放在该目录中的文件，不会单纯因为扩展名不是 .bin 而漏选。

该判断仍有明确范围：隐藏文件、敏感路径、符号链接被排除，扫描有访问数量限制，导出有每文件 20 MiB 和总负载 63 MiB 预算。完整包清单没有报告扫描截断或错误，也没有发现 TraceBin 候选。它证明的是这次扫描结果，不能排除未落盘、已清理、另一个容器或其他未记录状态。本轮未修改导出器的扫描或大小策略。


## 路径与验证

路径链已完整追到托管代码边界：PathManager.GetPersistentPath → GetPersistentDirectoryPath → PathManager.GetPath → HappyMahjong.Core.PathUtil.GetPath → UnityEngine.Application.persistentDataPath。TraceBinLogs/ 拼接在该根目录下；目录不存在时由路径辅助方法尝试创建。这里不是临时缓存目录，也没有在这条托管路径中找到另一个专用共享容器根目录。真正的绝对路径仍由 Unity 在手机运行时提供，本轮没有读取它。

还核对了同包 mscorlib 的 BinaryWriter.Write(byte[])：它直接调用 OutStream.Write(buffer, 0, buffer.Length)，不写额外长度前缀。因此，恢复记录边界必须依赖生产者提供的数据格式，不能从每次托管 Write 的调用边界反推磁盘中的记录分段。正在写入时可共享读取，也不等于能够取得一份语义完整的跟踪记录。

[首次写入检查](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42680458582)、[清理排序检查](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42680766850)和[最终根目录检查](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42681051215)均成功。最终源码为 def10d2，.NET 8 构建通过；扫描 91 个恢复程序集，输出 47 个选定方法、6 个枚举、4 个跟踪相关类型和 18 个直接调用者。FileMode、FileAccess、FileShare 的数值解释也与同包枚举交叉核对。证据见[受限 IL 与类型索引](trace-format.json)。

未执行游戏代码、修改偏好或启用跟踪，也未调用文件清理。本轮确认的是静态写入层和路径规则，没有实际 TraceBin 负载、当前运行时有效配置或实时对手牌值。至此，单靠现有导出文件和已恢复的写入层，仍不能实现实时对手暗牌显示；缺少的是实际运行中的消息证据和生产者格式，而非再一次相同文件的导出。
