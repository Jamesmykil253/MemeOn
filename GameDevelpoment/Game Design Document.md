2 — Game Design Document (GDD) — Core Gameplay (focused on 5‑minute 3v3 prototype)
2.1 Game modes & match timing
	•	Prototype focus: 5‑minute 3v3 (objective: collect crypto coins and deposit at zone / or hold more coins than opponents on end-of-match tally).
	•	Additional planned modes: 1m 1v1, 3m 3v3 (short), 10m 5v5 (large). But the first dev iteration concentrates on 5m 3v3.
2.2 Map design (prototype)
	•	Small arena with verticality (platforms, ledges), 3 coin spawn clusters, one central contested zone.
	•	Telemetry points: spawn positions, coin spawn nodes (deterministic spawn timers), safe deposit zone.
2.3 Characters & kits (prototype: 3 chassis)
	•	Elom Nusk: medium mobility, ranged basic attack, dash, energy shield.
	•	Doge Dog: high mobility (double jump buff), melee burst, bark‑stun.
	•	Tronald Dump: tanky, short knockback melee, area taunt.
Each kit defined by data tables: movement (maxSpeed, accel, jumpVelocity, doubleJumpAvailable), combat (range, DPS, cooldowns), health, move/attack animations placeholders.
2.4 Economy
	•	Pickup: crypto coin item (value = 1). Dropped on ground after certain events, or spawn nodes.
	•	Deposit: deposit zone generates team score. Points persist until round end. Coins also drop from defeated players.
2.5 Combat & hit registration
	•	Physics‑based hit detection via Unity Physics for projectiles and melee (use query APIs; ensure consistent mode between server & client). For authoritative server model: server validates hits; client predicts and reconciles.
2.6 Player movement mechanics
	•	Ground move: standard acceleration/damping.
	•	Jump: single jump on ground; consumes jump resource.
	•	Double jump: available if not grounded and doubleJump not used. Reset on land or after specific conditions (e.g., wall contact).
	•	Air control: limited; tuned constants in data table.
2.7 Match flow
	•	Spawn → 5:00 timer → coin spawns + pickups → combat & movement → deposits + scoring → tiebreaker rule (highest score wins; else sudden death).
2.8 UX and animation placeholders
	•	Each ability and state has an animation marker (string ID). For prototype, use simple animation placeholders and event hooks:
	◦	"Anim_Move" "Anim_Jump" "Anim_DoubleJump" "Anim_Attack_Light" "Anim_Attack_Heavy" "Anim_Hurt" "Anim_Death" "Anim_Respawn".
	•	Animation events used to trigger gameplay events (e.g., damage windows).
