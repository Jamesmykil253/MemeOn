8 — Implementation notes, pitfalls & mitigations
	•	Physics mismatch: Unity Physics and classic GameObject Physics differ; pick one approach early. If using Unity Physics (ECS), AI + Netcode integration needs bridging. Unity Physics 1.3.x behavior specifics must be tested (overlap queries, simulation results). Unity User Manual
	•	NavMesh + airborne moves: OffMeshLinks or custom jump arcs are needed for double‑jump maneuvers. Bake OffMeshLink for designed jump pads; otherwise trigger physics impulse when near jump gap.
	•	Netcode: NGO has known docs gaps; rely on sample repos and test cases; avoid over‑relying on any undocumented behavior. Unity Discussions
	•	Input System: Use action maps and server input batching to reduce bandwidth; consult changelog for 1.14.x breaking changes. Unity User Manual
	•	Tune giveUpTimeout & radii: Implement telemetry counters (failedHitCounter, timeSinceLastSuccessfulHit) to empirically tune AI.


