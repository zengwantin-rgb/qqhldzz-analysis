# v8: in-round table snapshot captured and inspected

The export is complete: 38/38 declared files, valid ZIP CRC, no missing or undeclared entries. Four event files contain 163 valid events: 125 retained from previous exports and 38 new events in entry 7. The new file has zero invalid records and no sequence gaps. The latest status session is 4 and reports subscription on attempt 1, followed by progress 25; it contains no field_snapshot failure, capture_failed or capture_limit_reached event.

## The previously missing state is now present

New sequence 2 is the pre-deal table snapshot. Sequence 3 is the deal. Sequence 5 is another table snapshot, before captured Hu at sequence 36. Its nonzero hand counts, actual self hand and already-discarded tiles prove this is an in-round refresh rather than another empty pre-deal snapshot. The transport cause of the refresh is not inferred solely from its type number.

All three failed TDR packs were successfully preserved by the on-device direct public-field fallback. They are validated JSON snapshots, not binary-decoder reconstructions. Their current-message and TLV branches are selected using the original schema maps. The collector fix is verified; no additional collector upgrade or repeated ordinary round is needed to reproduce this finding.

| Seat | Declared tiles in the in-round snapshot | Actual complete hand array |
|---|---:|---|
| 0, self | 13 | 13 nonzero tile entries |
| 1 | 14 | Absent |
| 2 | 13 | Absent |
| 3 | 13 | Absent |

The in-round snapshot has one complete THandTiles object, seat 0. The other seat/count structures do not contain szTiles arrays. Other explicit-seat card arrays in this snapshot are previously discarded tiles and exposed flowers, not concealed hands. Full numeric-array shape inventory is retained in decoded-gameplay-counts.json without values.

The initial deal has 14 self tiles, and all 44 recommendation CurHandTiles arrays match that exact multiset. In the in-round snapshot, all 21 recommendation CurHandTiles arrays match the explicit 13-tile seat-0 hand. Comparison happens privately; neither values nor hashes of these hands are emitted. The recommendation arrays therefore cannot be counted as other players' hands.

There are 20 wait objects with zero extra-tile counts and zero nonzero extra commands. All seven draw-field objects name seat 0, agreeing with the self-hand interpretation. Eight embedded buffers decode as LZ4 blocks and validate through the known advice/ting/tile/fan schemas, with no unknown nested tags or wire mismatches. Three 81-byte Lua-type-1 buffers validate as known rule-condition expressions with five condition references each. No Lua is executed during inspection.

Only the captured Hu notification at sequence 36 supplies all four complete hands: seat counts 7/13/13/13, all nonzero. This is post-Hu disclosure and does not provide ordinary live opponent visibility.

## Scope of the conclusion

This sample closes the specific in-round table-state gap identified in v7. The captured deal, in-round snapshot and other inspected live fields do not supply verified complete opponent hands for a live display. The current inbound-message path cannot implement that display using these samples. We do not infer real concealed values from hypothetical targets, public discards or counts.

The result is scoped to these captured messages and this game mode, not a proof about every server/mode/state. All 35 new binary objects produce modeled fields, but only 34 consume every byte and 22 pass exact regeneration. A type-160 side notification leaves eight bytes unmodeled; binary decoding is not claimed lossless. These binary limitations do not turn the direct in-round snapshot's count-only opponent structures into complete hand arrays.

The phase parser now resets at every captured deal, preventing later rounds in a process from inheriting a prior Hu label. A regression covers two deals with Hu/finish between them. A separate regression checks that reconnect recommendation comparisons use an explicit self hand and never publish private values.

Live opponent-hand display remains unimplemented. The capture and the specific reconnect check have succeeded, while the required opponent values are absent from the verified live hand fields. Repeating the same phone export is not indicated by this finding.

Input audit: https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42719530017
Decode and private self-hand comparison: https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42719820272
