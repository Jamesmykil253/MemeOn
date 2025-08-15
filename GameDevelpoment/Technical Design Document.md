4 — Technical Design Document (TDD) — systems, data, network, testability
4.1 System components (modular)
	1	Input Layer
	◦	Input System (1.14.2) action maps: Player/Move, Player/Jump, Player/Attack, Player/Use.
	◦	Local prediction: movement & jump inputs produce local predicted motion; server authoritative body reconciles. Use Input System's action callback + custom network queue for client->server input batching. Unity User Manual
	2	Player Controller
	◦	Movement component (authoritative on server; client side prediction).
	◦	Jump controller with double-jump resource; uses Physics queries for ground checks (Contact manifold via Unity Physics 1.3.x). Unity User Manual
	3	Enemy AI
	◦	FSM (low-level) module per agent.
	◦	Behavior Tree module per agent (server‑only).
	◦	Navigation adapter using NavMesh surface & agent interfaces (AI Navigation 2.0.8). Unity Documentation
	4	Network Layer
	◦	Netcode for GameObjects (2.4.4) for object spawning, RPCs, NetworkVariable sync for minimal state (HP, position fallback). Provide reconciliation hooks for predicted movement. Use server authoritative model. Unity Discussions
	5	Physics & Collisions
	◦	Unity Physics (ECS-style) for fast queries for projectile collisions and hit detection; or if prototype is GameObject-based, use Physics package queries consistent with that package. (Unity Physics 1.3.14 API for simulation results.) Unity User Manual
	6	Game Manager
	◦	Match lifecycle, coin spawn system, scoring, round timer.
	7	Animation & VFX
	◦	Animation markers mapped to gameplay events.
	◦	VFX layer decoupled; data-driven event hooks.
4.2 Data model (canonical JSON schemas)
	•	Provide concise table for entities:
PlayerProfile (schema)
json
CopyEdit
{
  "id":"uuid",
  "archetype":"ElomNusk|DogeDog|Tronald",
  "health": 100,
  "move": { "maxSpeed": 6.0, "accel": 35.0, "jumpVel": 7.5, "doubleJumpAllowed": true },
  "abilities":[ ... ],
  "spawnPosition":[x,y,z]
}
AI_Blackboard (schema) (subset)
json
CopyEdit
{
  "spawnPosition": [x,y,z],
  "targetId": null,
  "timeSinceLastHit": 0.0,
  "failedHitCounter": 0,
  "aggroed": false
}
4.3 Network determinism & authoritative model
	•	Authoritative server: server runs AI, validates client input, resolves physics interactions; clients send inputs to server.
	•	Netcode considerations:
	◦	Use NetworkTransform with optimized interpolation for positions; for fast twitch movement (jumps), use custom transform replication with reconciliation semantics.
	◦	Use server RPCs for authoritative events (damage, pickup).
	◦	Avoid sending BT internals across network — only replicate outcomes (targetId, state) if necessary for visuals.
4.4 Concurrency & tick rates
	•	Server tick: 20–60 Hz (configurable); physics substep at stable rate (e.g., 60 Hz).
	•	Network send rate: 20–30 updates per second for transforms; events via RPC as needed.
4.5 Testing & validation hooks
	•	Unit tests for FSM transitions (pure logic).
	•	Integration tests for BT decision outcomes (mock blackboard).
	•	Netcode fuzz tests: simulate packet loss, latency.
	•	Performance tests: NPC count, physics overhead, pathfinding concurrency.
