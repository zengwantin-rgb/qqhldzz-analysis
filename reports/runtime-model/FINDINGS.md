# Continue beyond the inbound-message audit

The v8 audit established that the captured in-round table message contains self values and only opponent counts. This did not complete the requested live opponent display. Version 0.3 therefore implements a separate, read-only runtime model path and an actual native text panel.

The static root is HappyMahjong.Game.RoundModeUtil.RoundModel. Source and metadata trace GetTilesByViewSeat to GuoBiaoRoundModel.m_playerGameInfo[view].tiles; MJTileData stores its hand in private m_inHandList. Direct field reading can test whether the loaded client keeps values outside the audited decoded messages, including runtime behavior not established by static packet inspection. The original source's normal placeholder construction remains relevant, but is not substituted for a real runtime observation.

The model panel reads all four view seats, uses sampled m_selfSeat for raw-seat conversion, and renders only the three opponent rows. It copies the hand list, reads known concealed-kong entries separately, and records selected auxiliary draw fields without rendering them as current hand tiles. It does not call model accessors or write game objects. Client-model values, actual server hand values, hypothetical targets and public post-Hu hands are not treated as interchangeable.

A model is labeled live only when an observed deal anchors the same object reference and recent game traffic exists. Hu/finish updates the phase before journal budget checks; a later deal resets it. Replacement models, unanchored models, stale traffic and non-playing game status clear or qualify the panel. Journal limits do not freeze a prior live label. Tests exercise these transitions, including a full 512-record journal.

The optional blackboard scan follows instance fields and standard dictionary values, without reading keys. It keeps only bounded hand/wall/tile numeric arrays from game-related model types, plus possible owner-field metadata. Limits are depth 4, 32 visited references, 128 fields per object, 512 dictionary slots, 16 arrays, 200 entries per array and 1024 total integers. Cache candidates are never rendered as opponent hands. Unknown storage shapes may remain unsupported; this is not a general memory dump or proof that no other state exists.

Runtime observations have a distinct wrapper/channel/encoding. Public validation reports only hand counts, placeholders, valid-code counts, phase and bounded cache statistics. Raw values, auxiliary values, candidate metadata paths, panel text, tokens and exception messages are omitted from public output. Private candidate source metadata remains available for later runner-only analysis.

Static metadata schema build: 42721634134. Final native build and core tests: 42724037280. Diagnostic privacy tests: 42724043983. Existing v8 input regression: 42724049117, 163 valid records retained, zero legacy runtime observations (expected for 0.2.2). These checks do not establish on-device success of the new model sampler or actual live opponent values. Historical 0.2.x build/device receipts remain unchanged.

The installable 0.3 dylib and source implement the runtime panel. Live opponent-hand visibility is still unverified and the user's overall goal is not marked complete.


## Device v9 and correction in 0.3.1

See device-v9/FINDINGS.md. Device data verifies the primary model reader and self-hand equality, but finds only opponent placeholders. Version 0.3 omitted the separate registered-module map; 0.3.1 corrects that omission and adds explicit bounded coverage. New module scanning remains device-unverified. The original build receipts above are historical.
