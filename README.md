# 欢乐麻将 iOS 静态逆向分析

[第七轮原 ZIP 覆盖审计](reports/device-round/EXPORT-AUDIT.md)：补齐全部导出文本的负载探测，5 个文件完整，无新增对手暗牌证据；另有 11 个文件仅在扫描清单中，实际未装入 ZIP。

[第六轮协议复核](reports/device-round/PROTOCOL-FOLLOWUP.md)：已核对压缩消息的听牌/推荐消费路径及牌墙结构；当前日志没有可恢复的对应负载，实时对手暗牌仍未验证。

分析输入是用户提供的 Info.plist、UnityFramework 和欢乐麻将_Decrypted.ipa。原始文件位于公开仓库 zwthys-cyber/qqhldzz-analysis 的 inputs-v1 Release 附件中。

[最新业务代码分析报告](reports/managed/FINDINGS.md)记录了 UnityMJ 格式恢复、热更新程序集反编译，以及普通对局、回放、组队和明牌的数据差异。当前尚未证明大众麻将普通对局会下发对手完整暗牌。

所有分析工具的编译和执行、原始文件校验、格式恢复及反编译均在 GitHub Actions 中完成。当前服务器只准备代码、推送并读取下载的结果，不编译或运行应用与分析工具。

## 工作流与输出

- `analyze.yml`：输入完整性、文件对应关系、Mach-O 和 IL2CPP 元数据基本检查。输出 ios-analysis-report。
- `reverse.yml`：编译 Il2CppDumper 并恢复 AOT 元数据和原生方法映射。输出 il2cpp-reverse-report。
- `recover.yml`：恢复三个 UnityMJ 包，验证唯一目录、完整块边界及资源解析，提取业务 DLL。输出 unitymj-recovered-report。
- `managed.yml`：编译 Mono.Cecil 元数据读取器，使用 ILSpy 反编译对局业务代码。输出 managed-game-report。
- `dazhong.yml`：进一步查看游戏标识和规则配置来源。输出 dazhong-rule-report。

原始输入没有游戏源码工程、Xcode 构建配置和签名材料；反编译输出也不能直接等同于可重编译的完整原游戏。没有在手机上执行修改版，也没有证明实时对手手牌泄露。旧阶段报告保留了当时的结果，UnityMJ 解析障碍已在最新报告中解决。

[第四轮与手机诊断工具](reports/mobile/FINDINGS.md)：已发现新的动态规则引擎路径，进一步限定了传统手牌模型结论的适用范围。提供 [TrollStore 日志导出工具源码与使用说明](mobile/LogReader/README.md)，用于在只有手机的条件下取得现有诊断文件；实际对局消息是否可从日志恢复尚未验证。

[第五轮：实际大众对局日志结果](reports/device-round/FINDINGS.md)：已成功分析用户 iOS 17.0 设备导出的日志。一局 ModuleTuidaohu49 中，52 条等待消息的额外牌字段均为空值；自己的初始手牌和摸牌可见，尚未发现能支持实时对手暗牌显示的数据。明文日志不进入公开 Git 历史。
