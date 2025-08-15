7 — Engineering checklist / acceptance matrix
Infrastructure
	•	Unity project created with Editor baseline 60001.4f.
	•	Packages locked: Input System 1.14.2, AI Navigation 2.0.8, Netcode 2.4.4, Unity Physics 1.3.14. (documented manifest.json).
Movement & Controls
	•	Input System action maps defined.
	•	Ground check deterministic and reconciled server-side.
	•	Double jump flag consistently resets on land.
AI
	•	Enemies idle until damaged.
	•	Enemies pursue attacker until distanceFromSpawn > R or timeSinceLastSuccessfulHit >= T.
	•	Attack hit windows align with animation events.
	•	NavMesh integration for pathing and jumping.
Network
	•	Server authoritative damage resolution.
	•	Network reconciliation tested under 100ms simulated latency.
	•	Minimal NetworkVariable usage (HP, position backup).
Tests
	•	Unit-tests for FSM transition matrix.
	•	Integration tests for BT decisions (mock).
	•	Netcode automated test for spawn/sync.
