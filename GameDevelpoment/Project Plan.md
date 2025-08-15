5 — Project Plan (milestones, sprints, deliverables)
Assumptions: Team = 2 engineers + 1 animator/sfx (contractor) + automated AIs, 8‑week prototype.
Sprint overview (2‑week sprints)
Sprint 0 (setup) — 1 week
	•	Repo, CI, Unity project baseline 60001.4f.
	•	Install & lock packages: Input System 1.14.2, AI Navigation 2.0.8, Netcode 2.4.4, Unity Physics 1.3.14.
	•	Empty scene, NavMesh surface, simple cube arena.
Sprint 1 — Movement & Single Player Controller (2 weeks)
	•	Implement Input System action maps; map move/jump/double jump testing.
	•	Local prediction + server reconciliation skeleton (Netcode).
	•	Physics integration for jump/double-jump. Acceptance: player can jump and double jump reliably on authoritative server with client prediction.
Sprint 2 — Basic Combat & Projectiles (2 weeks)
	•	Implement basic attack, hit windows via animation markers.
	•	Projectile prefab + server validated hit.
	•	Coin pickup spawner (server).
	•	Acceptance: damage is server-authoritative; coin pickup replicates.
Sprint 3 — AI Baseline (2 weeks)
	•	Implement FSM + BT server components for enemy archetype(s).
	•	AI spawn with Idle until damage; on damage, pursue & attack; giveUp rules implemented.
	•	Acceptance: enemy remains idle until shot; pursues until outside radius or timeout.
Sprint 4 — Netcode & 3v3 Match Flow (2 weeks)
	•	Implement Netcode multiplayer flow: hosting, joining, spawning players.
	•	Implement match manager: spawn teams, coin spawns, scoring.
	•	Acceptance: two clients can join host and play a 3v3 match in local network.
Sprint 5 — Polish, Debugging, Playtest & Metrics (2 weeks)
	•	Tuning movement constants, attack windows.
	•	Instrumentation & analytics (hit success, giveup counters).
	•	Automated tests covering FSM transitions.
Deliverables per sprint: CI green build, playable scene with acceptance criteria, documentation for each module.
