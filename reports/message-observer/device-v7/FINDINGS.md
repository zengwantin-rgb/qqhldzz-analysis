# v7 phone capture: missing deal/table capture is now fixed

The complete export contains 33/33 declared files, with valid ZIP CRC and no missing/undeclared entries. Three event files contain 125 valid records: 58 retained from prior captures and 67 new records in entry 6. The new capture has zero invalid records and no sequence discontinuities. The latest status session is 3; it registers on attempt 1 and records progress 25/50. Earlier sessions and their failures must not be attributed to session 3.

## Verified phone behavior

The new file contains 65 re-encoded TDR objects plus two direct public-field JSON snapshots. Sequence 2 is table state (type 8); sequence 3 is deal (type 1). Pack still reports argument errors for these two messages, but the active-branch fallback saves both snapshots and later events continue. No current-session field_snapshot or capture_failed status occurs. This is the on-device verification of the missing-message capture fix, separate from historical build receipts whose unverified flags remain unchanged.

Snapshot JSON is checked against its declared type and saved under a distinct private .snapshot.json suffix. It bypasses the binary decoder; provenance remains public_field_snapshot throughout the counter analysis. JSON bytes, card values, raw messages, identifiers and exception messages remain private.

## What the captured data actually establishes

The table-state snapshot precedes the deal. It has four seat/count records, all at zero, without tile arrays; an additional self-seat THandTiles object has an empty array. This is not a mid-round reconnect sample and cannot establish what a reconnect response would contain after all players have hands.

The deal snapshot contains 14 nonzero tiles in its main szTiles array. Recovered RevcDealCommand.AddPlayerCard uses this array only for self view seat 0 and creates MJ_BACK entries for other seats. All 38 CurHandTiles arrays in the deal's recommendation targets privately match the exact self-deal multiset. No raw values or hashes of these hands are emitted. These repeated recommendation arrays are not independent opponent hands.

The complete numeric-array inventory of both snapshots contains no additional explicit opponent hand array. Other arrays are dice, shared-item destination seats, exposed flower tiles, score/protection data, round IDs, rule buffers, embedded advice and target/flag arrays. ShareTableInfo's static schema has source/destination seats and item-sharing counters, not a card payload.

There are 43 wait objects; all extra-command and declared extra-tile fields are zero. All 13 draw-field objects name seat 0. The only nonzero complete THandTiles objects in the new capture are in Hu notification 50, sequence 66: seats 0/1/2/3 have 13/10/7/4 entries respectively. This is post-Hu disclosure and is not evidence of ordinary live opponent-hand visibility.

All 13 embedded buffers decode as bounded LZ4 blocks and match the known advice/ting/tile/fan schemas, with zero unknown nested tags and zero wire-type mismatches. Both 81-byte configuration buffers have Lua type 1 and validate against the fixed rule-condition vocabulary, with five condition references each. Recovered NextToDoHelper.HandleLuaString passes type-1 UTF-8 strings to CommonRule.SetRuleCondition; ProcessLuaString handles condition_ predicates, return, parentheses and boolean and/or. No Lua is executed by this analysis. The config strings are validated as rule expressions, not silently ignored as unknown bytes.

## Limits and the remaining targeted check

All 65 new binary objects produce modeled-field JSON, but only 64 consume every input byte and only 39 pass exact-byte regeneration. Type 160 at sequence 65 leaves 8 bytes unmodeled. Direct deal/table snapshots are not reconstructed through this older binary schema. The analysis does not claim universal or lossless decoding, that the server never sends hidden values in any state, or that native/runtime patch behavior is fully characterized.

A single in-round reconnect snapshot is the remaining narrow untested state: earlier exports had such table-state dispatches, but the old collector failed to save them. Keep 0.2.2 loaded and ready in the current game process, let the deal occur, temporarily disconnect the phone network and restore it, then return to the same table and export. Keeping the process alive preserves the registered listener; a full app restart could deliver reconnect state before the delayed loader subscribes. No library upgrade or repeat of an ordinary uninterrupted full round is needed for this check.

Live opponent-hand display remains unimplemented. Current evidence establishes successful capture and self/public/post-Hu fields; it does not supply a verified live opponent hand to render.

Input audit: https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42711286328
Direct-snapshot audit: https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42711628250
Rule/config audit: https://github.com/zengwantin-rgb/qqhldzz-analysis/actions/runs/42712261968
