# Device v9: model reader verified, opponent values absent in sampled fields

The partial export includes 9 of 43 candidates and contains the required observer journal: 29 valid records, including 15 runtime observations and 14 TDR events. Thirteen runtime observations are marked live by version 0.3. All sampled opponent hand entries are placeholders. Opponent auxiliary draw fields are zero or negative-one sentinels, with no valid hidden tile codes.

The first runtime sample following the reconnect table event contains the same 13 self tiles as that table snapshot. Comparison takes place privately; only equality and counts are published. The immediate post-deal model is not populated yet and is reported as not ready, rather than a mismatching hand.

Version 0.3 scanned the blackboard but omitted GuoBiaoRoundModel.m_moduleDataModels. Therefore its empty cache result does not establish absence of data in registered module models. GameContext.BindMJModuleEvent registers each rule module DataModel through RegisterModel. AOT metadata identifies Blackboard.data as its separate backing dictionary.

Version 0.3.1 reads both roots directly, adds bounded coverage statistics, numeric lists and tile-object candidates, and labels empty-self live snapshots as dealing. Candidate values remain private and are never rendered as verified opponent hands. The new registered-module scan requires a device observation; this report does not claim that it has recovered live opponent values. No full new round or full 43-file export is required for that check.

Phone analysis run: 42730219684. Final 0.3.1 native build and managed regression tests: 42730674972. Parser tests: 42730680596. Overall live opponent-hand goal remains incomplete.
