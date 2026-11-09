# 欢乐麻将 iOS 文件静态分析

输入为用户提供的 Info.plist、UnityFramework 和欢乐麻将_Decrypted.ipa，保存在原始仓库 zwthys-cyber/qqhldzz-analysis的 inputs-v1 Release 附件中。

所有脚本编译、ZIP CRC、SHA-256、Mach-O、文件一致性和 IL2CPP 元数据验证均在 GitHub Actions 上执行。当前服务器仅用于读取文件、准备脚本和推送。

原始输入没有游戏源码、Xcode 工程或签名配置，因此无法重编译原游戏。CI 中的编译步骤是分析脚本的字节码编译。检查加密标志并不等于证明完整脱壳成功；静态结果也不证明运行行为或漏洞。

在 Actions 中下载 ios-analysis-report 查看 JSON 与 Markdown 报告。
