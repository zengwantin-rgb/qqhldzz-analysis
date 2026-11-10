# First on-device 0.3 panel verification

The user-provided screenshot shows the new native client-model panel during ordinary play. The panel labels the phase as live and displays placeholder counts 13, 13 and 11 for next/opposite/previous view seats. No opponent tile names are displayed. The screenshot itself, avatars, balances and visible card values are not published.

Under the tested panel contract this verifies that the native timer invoked the managed model reader, the source/class/game/seat guards passed, private model hand fields were readable, and the returned UTF-16 JSON reached the native label. The placeholder wording corresponds to lists containing only MJ_BACK zero values. This is visual verification, separate from exported raw-log validation and from the historical build receipt whose on-device flags were false when built.

Auxiliary fields, concealed-kong records and blackboard candidates are not visible in this panel state. Their actual private capture contents require the new 0.3 export; the screenshot cannot establish their absence. No new round is needed to inspect already-captured data. The overall live opponent-hand goal remains incomplete.
