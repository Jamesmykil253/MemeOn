8 — Implementation notes, pitfalls & mitigations
	•	Physics mismatch: Unity Physics and classic GameObject Physics differ; pick one approach early. If using Unity Physics (ECS), AI + Netcode integration needs bridging. Unity Physics 1.3.x behavior specifics must be tested (overlap queries, simulation results). Unity User Manual
	•	NavMesh + airborne moves: OffMeshLinks or custom jump arcs are needed for double‑jump maneuvers. Bake OffMeshLink for designed jump pads; otherwise trigger physics impulse when near jump gap.
	•	Netcode: NGO has known docs gaps; rely on sample repos and test cases; avoid over‑relying on any undocumented behavior. Unity Discussions
	•	Input System: Use action maps and server input batching to reduce bandwidth; consult changelog for 1.14.x breaking changes. Unity User Manual
	•	Tune giveUpTimeout & radii: Implement telemetry counters (failedHitCounter, timeSinceLastSuccessfulHit) to empirically tune AI.



Delta — Camera + Movement (2025-08-17)
- PlayerMovement: Added server-authoritative jump and double-jump. Owner requests jump via RPC; server applies vertical impulse and manages canDoubleJump. Gravity remains server-simulated. Inspector fields: jumpHeight, doubleJumpHeight.
- UniteCameraController: Live pan while alive using Look and optional PanModifier. Horizontal pans along camera-right (XZ only); vertical pans along world Y. Radius clamped and recenter spring. Free-pan on death using Move.x for horizontal (camera-right) and Move.y for world Y elevation. Scroll zoom preserved for editor.
- CameraBootstrap: now calls SetTarget to bind/unbind NetworkHealth for death detection.

QA quick checks
- Alive: Hold Right Mouse + move mouse → camera pans; release → recenters.
- Dead: After death, WASD/LeftStick moves camera; W/S (Move.y) raise/lower along Y; A/D pan horizontally; camera still looks at last known target position.
- Jump: Space/Gamepad South triggers jump; pressing again in air triggers double jump; landing restores double jump.


