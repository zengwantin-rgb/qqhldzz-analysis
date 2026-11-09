# 第九轮：记录文件的实际读写格式

本轮继续检查原有输入，追查动态文件名及记录读写代码，仍未取得能显示对手实时暗牌的数据。

## 扫描结果

扫描全部恢复程序集的方法，选择同时包含文件/路径 IO 调用与 record 字符串，或 IO 调用与直接 16 字节数组分配的方法，得到 [21 个候选](record-io-candidates.json)。其中 2 个是基础类库方法，19 个是业务方法。命中的唯一直接 16 字节分配在 System.IO.BinaryWriter 构造函数中；内部缓冲区的大小不能用来认定磁盘文件的大小或来源。

原生文件名扫描补充了大小写不敏感的匹配，并将 record 后缀匹配扩展到非数字前缀生成的文件名；后缀匹配仍单独标记为弱关联。[结果](record-name-followup.json)未为设备的 16 字节文件找到引用，不能据此判定它的用途。

## 四种需要区分的记录路径

| 路径 | 从代码确认的格式或用途 | 与 16 字节文件的关系 |
| --- | --- | --- |
| ProtocolHierarchy.ProtocolSerializer | 默认生成带 ProtocolRecord 的 .json 文件；每条协议的 protobuf 字节写入 Base64 BinaryArray，并保存 ClassName | 默认路径和载体不一致，尚未发现关联 |
| ReplayGameServer.SaveRecord / LoadRecord | 默认保存带 record 的 .json 文件，条目使用 type 和 objectJson；读取时按 JSON 反序列化 | 默认格式不一致；显式路径、补丁及其他写入者仍未排除 |
| PlaybackRecordService / CSReplayRecordData | 从响应缓冲区或 CGI 响应读取 GetPlaybackRecordRes，再解压 PlaybackRecord 并解析 RoundRecordV3 | 是回放响应处理路径，没有证明它写入该 .dat 文件 |
| MemoryMonitorUtil | 保存 MemoryStats 目录下的内存统计 .data，内容是 MemoryMonitorConfig | 是诊断数据，不能当作进程内存转储或手牌模型 |

源码证据：[ProtocolSerializer](record-evidence/ProtocolSerializer.cs)、[本地回放](record-evidence/ReplayGameServer.cs)、[回放响应](record-evidence/PlaybackRecordService.cs)、[压缩回放解析](record-evidence/CSReplayRecordData.cs)、[内存统计](record-evidence/MemoryMonitorUtil.cs)。这些是恢复出的实现，实际运行仍可能受补丁影响。

本地回放的 IsRecording 同时检查 DynamicConfig 中 Record/Open 与记录模式。存在 SaveRecord 方法并不证明用户那局已启用记录，更不说明它能记录服务端未提供的数据。内存统计默认读取 MEMORY_MONITOR_ENABLED 开关，其统计快照也不是其他玩家手牌的内存读取接口。

## 原 ZIP 是否含有这些载体

本轮将以下已恢复字段补入全文件字节搜索：ProtocolDataList、BinaryArray、ClassName、objectJson、PlaybackRecord、RecordVersion、so_notify_msg、round_env。[覆盖结果](record-envelope-coverage.json)显示原 ZIP 的全部 5 个文件中，这 8 个字段均为零次命中。

因此，当前导出没有提供这些字段名所标识的文本记录载体；不能将日志里一般的 replay/record 名称当作已经取得完整回放。字段名搜索不能排除无字段名的二进制编码，所以本轮仍保留 16 字节文件格式未知的结论，没有为它猜测牌值。

## 验证与后续边界

[最终 GitHub Actions 任务成功](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42653118152)，对应提交 f1d2af1。8 项合成测试通过，包括本轮增加的大小写精确匹配与非数字前缀弱匹配；全部程序集扫描、三类新源码反编译、原有牌局分析、全文件覆盖检查及原生追查均成功。已有 ReplayGameServer 与 CSReplayRecordData 证据来自此前恢复的同版本程序集。

本轮将“record 可能是牌局文件”的泛化猜测改为具体的格式、字段与读写路径，但没有建立 16 字节文件与某个写入者的联系。原 ZIP 也没有补出此前缺失的主日志、设备主配置或补丁内容。实时对手暗牌功能仍未实现；不能通过为现有日志增加显示层来补出未取得的牌值。
