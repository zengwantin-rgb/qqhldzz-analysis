# 运行时模型手牌面板 0.3.1 Beta

这一版新增直接读取客户端牌局模型的面板，继续保留 0.2.2 的消息观察器。支持已核对的欢乐麻将 build 8.19.112、大众麻将 ModuleTuidaohu49（149）。

新增面板每两秒读取当前四家模型。下家、对家、上家有正常牌值时显示牌名；零值显示“牌背，未读到牌值”；暗杠单独标注。胡牌/结算与对局中采用不同标题，阶段未确认或同步过期时暂停显示牌名，换模型后重新确认。读取成功不等于已经取得真实对手暗牌，真机模型字段是否存有这些值仍待验证，项目的实时看对手牌目标尚未完成。

## 安装与观察

1. 将 MJMessageObserver.dylib 保存到“文件”App，完全关闭欢乐麻将。
2. 在 TrollFools 选择欢乐麻将，移除旧的 MJMessageObserver.dylib，以及仍存在的 MJRuntimeProbe.dylib，再导入本版。仅保留一个观察器版本。
3. 打开游戏，在大厅等待“已就绪”，进入大众麻将。新增的“客户端模型”面板显示三家读取结果。
4. “牌背 ×N，未读到牌值”表示客户端模型中读到的仍是未知值；“等待阶段确认”表示还没有观察到当前模型的开局锚点；“读取未完成”表示字段或运行时接口还需检查。
5. 用现有日志工具 1.2 导出当前观察器记录即可，不必打完整一局，也不必重新安装日志工具。

## 数据与边界

模型入口为 RoundModeUtil.RoundModel。读取 GuoBiaoRoundModel.m_playerGameInfo → PlayerGameInfoVO.tiles → MJTileData.m_inHandList，并按实际 selfSeat 将视角座位映射到服务器座位。m_anGangList 的已知 mjValue 只标为暗杠；curRedrawMjValue、LastMJ、天胡和额外摸牌字段单独记录为辅助信息，不混入当前手牌。

内部扫描直接读取当前牌局 m_moduleDataModels 注册模块及 blackboard.data，忽略字符串字典键、账号字段和 Unity 对象；支持数值列表及牌值对象。候选只保存在私密日志，不显示为对手手牌。扫描限制为深度6、96个对象、48个候选、4096个整数及每次25毫秒，记录根入口覆盖、读取错误和预算状态。未扫描完不代表客户端不存在其他数据。

观察器不写游戏字段，不调用游戏 getter/setter、单例构造器或对局请求。字段及标准集合只读，结果复制到自己的数组。屏幕面板不接收触摸。没有接入外部收件服务，也不自动上传。

本地保存目录沿用 Documents/MJMessageObserver。运行时模型使用独立 runtime_model_event / runtime_model_snapshot 标记，与 TDR/protobuf 消息区分。每进程最多保存 128 个变化的模型状态，与消息共用 512 条、18 MiB 的记录上限；到达保存上限后，屏幕模型读取和阶段更新仍继续。原始牌值、辅助字段和缓存仅存在手机及私密导出中，公开分析只输出数量与阶段。

停止使用时关闭游戏，在 TrollFools 移除此库后重开即可。

## 验证情况

已通过：原版元数据编译、ARM64 构建与 ad-hoc 签名、文件隔离测试、核心模型/缓存边界测试、阶段变化和记录上限测试、公开报告隐私测试。旧版 v8 ZIP 的 163 条记录回归校验仍通过。

0.3 的字段读取和面板已由截图及 v9 日志验证：自己的13张牌与重连包一致，三家手牌字段仍是未知值。旧版漏扫注册模块入口，因此不能用旧缓存结果排除该入口。0.3.1 补齐此入口并修正发牌过程中空手牌的阶段显示；新版模块扫描尚待真机导出验证。

构建：https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42730674972
解析器测试：https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42730680596
源码：cb16bcbfdfc1ec359d609a61d1e9fc83c1782d37
SHA256：71370fa61971174d082ee6ee640af1dda5002fc7e31045efba19d0af0f71d840
