# 第十二轮：完整设备包、配置与热补丁核对

本次完整导出补齐此前缺失的 11 个候选文件，独立验证全部 16/16 个文件。但没有取得新的实时对手暗牌；可确认的其他三家剩余手牌仍仅出现在胡牌后，数量为 13、7、4 张。不能据此实现实时显示，也不能推断服务器绝不下发未记录的数据。

## 完整性与两份主日志

[覆盖审计](export-coverage.json)验证 ZIP CRC、清单数量、预期条目路径及唯一性。selection_mode 为 complete_candidates，complete 声明独立验证通过：16 个候选、16 个文件，缺失、重复、未声明、未选均为 0。清单报告工具 1.1、iOS 17.0、游戏版本 8.19.112，无扫描/导出错误或截断。此结果验证 1.1 的真机完整选择与导出流程，完整性范围限于本次扫描候选。

[逐日志结果](round-analysis.json)分别处理两份主日志，避免将阶段状态跨文件串联。条目 13 没有对局分发；条目 14 仍是此前同一局 ModuleTuidaohu49（枚举值 149），不是新增的一局。79 次分发中有 52 条等待消息，chExCmd 和额外牌数量均为 0，额外牌列表为空；15 次正值摸牌属于自己。胡牌在第 31360 行，其他三家剩余牌记录在第 31367–31369 行，FINISH_V2 在第 31377 行。推荐列表中的当前手牌归属仍指向自己，不能当作其他玩家实时手牌。

## 设备配置

[配置结果](complete-config-facts.json)将已知配置按 zlib/protobuf 结构解析，并与原安装包比较：

| 文件 | 设备与安装包比较 |
|---|---|
| MJGameBaseConfig.data | 不同；设备压缩 6580 字节，解压 108722 字节 |
| ModuleFanConfig.data | 字节一致 |
| SvrNotifyTips.data | 字节一致 |
| VedioRecordConfig.data | 字节一致，为配置表而非实际对局录像 |

主配置顶层第 1 张表为设备 486 / 包内 484 项，第 2 张表为 134 / 94 项，均有变化；第 3 张表均为 28 项且序列化内容一致。第 1 张表新增 ID 400、401，变化 ID 为 841398、842398、843398、844398、8801398，无删除。不能将这些 ID 自动解释为本局房间。

源码 [MJGameBaseConfigUtil](evidence/MJGameBaseConfigUtil.cs)显示按 ID 查表并将 configValues 按分号拆开，由 [MJGameBaseConfigDetail](evidence/MJGameBaseConfigDetail.cs)按索引赋值。149 行只有 34 个值且未变，不能直接当成本局有效配置。

条目 14 的第 15871、16030、16043、16346 行打印的 60 项基础规则向量，与设备表的 270398、271398、272398 三行均完全相同，无法唯一选择房间 ID。四次记录均显示 totalPlayerNum=4、totalCardNum=144；canTing、showTingGiveCard、afterTingShowAllHandTile、afterTingShowHuHandTile、showRecommendFanPanel、HasSingleWall、IsParallelDraw 均为 0。这些是观测到的基础规则；源码另有服务端和私人房覆盖，不能宣称所有后续有效状态均已验证。

## 热补丁

[补丁结构结果](device-patch-summary.json)依据恢复源码的 [CombinedPatchProcessor](evidence/CombinedPatchProcessor.cs)、[PatchCombiner](evidence/PatchCombiner.cs)及 [PatchManager](evidence/PatchManager.cs)读取格式。只解析字节，不执行补丁、虚拟机指令或反射绑定；任意内部字符串不公开。

76786 字节 PatchIOS.bytes 是版本 1 的组合 IFix 补丁，包含 3 段，完整消费全部字节，无间隙或尾部余量：

| 程序集 | 偏移 | 字节 | VM 方法 | 指令 | 替换目标 |
|---|---:|---:|---:|---:|---:|
| Core | 36 | 819 | 2 | 12 | 2 |
| Hall | 855 | 48563 | 37 | 2071 | 16 |
| InGameScripts | 49418 | 27368 | 15 | 956 | 7 |

共 25 个替换目标全部解析到源码类型与方法。涉及版本、大厅请求、番型配置刷新、活动与记录界面、资源及动画等；没有直接替换本轮跟踪的发牌、摸牌、结算、推荐及配置关键接收方法。不能因此将整个补丁视为纯界面变化或断言没有间接影响。Core 的桥接类型和总计两个外部类型未唯一匹配，完整结构解析不等于运行时绑定与语义全部验证。

日志出现 3 次补丁 ready 标记，均匹配版本 8.19.112；未记录加载异常、版本不符卸载或空输入。[IFixManager](evidence/IFixManager.cs)及[调用点索引](patch-loader-callers.json)支持加载路径核对。但版本和 ready 标记不能证明当时加载的字节摘要与本次磁盘文件完全一致。

## 验证与限制

[Actions 42669940693](https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42669940693)在源码 11b0e2002c94bbef274dff7f5b29b7070b66fbd8 上完成分析，28 项测试通过，涵盖完整性、JSON、跨日志阶段隔离、配置及补丁边界。原始输入以加密附件传入，运行时解密，公开结果仅保留受限结构、计数与方法元数据。

两份 .xlog 仍为未解码二进制，16 字节 ano_r_record.dat 的用途与写入者仍未确定。完整候选导出不等于覆盖所有应用状态、内存或网络消息。现有工具为只读文件导出器，没有新增进程内或网络消息采集能力。

本包的缺失文件核对已完成。继续验证实时牌值需要新的运行时解码消息证据；重复分析或导出这批既有文件不能填补该证据缺口。
