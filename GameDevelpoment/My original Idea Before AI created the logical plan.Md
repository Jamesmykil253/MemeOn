1. Concept Document
Working Title: Meme Online Battle ArenaGenre: 3D Platformer MOBA with dark/ironic comedyCore Theme: Meme-based satire with parody characters (Elom Nusk, Doge Dog, Tronald Dump).Tone: Over-the-top humor, internet meme aesthetics, crypto satire.Target Platforms: PC (initial), expandable to consoles.Target Audience: 16+ players who enjoy short-session competitive games, memes, and parody humor.
Core Gameplay Summary:
	•	Short-session matches:
	◦	1v1 (1 min)
	◦	3v3 (3 min)
	◦	3v3 (5 min) — Initial focus
	◦	5v5 (10 min)
	•	Movement: run, jump, double-jump, dash.
	•	Combat: basic attacks + meme-themed special abilities.
	•	Objectives: defeat opponents, collect dropped crypto coins for points.
	•	Victory: most points at match end.

2. Game Design Document (GDD)
2.1 Core Loop
	1	Spawn
	2	Navigate arena (platforming + vertical movement)
	3	Fight enemies (player or AI)
	4	Collect crypto coins dropped by defeated enemies
	5	Return coins to base to bank score
	6	Repeat until timer ends
	7	Match results screen

2.2 Player Mechanics
	•	Movement:
	◦	Jump
	◦	Double-jump
	◦	Dash
	◦	Air control
	•	Combat:
	◦	Light attack
	◦	Heavy attack (charged)
	◦	Two special abilities (cooldown-based)
	•	Scoring:
	◦	Each KO drops coins.
	◦	Coins can be picked up by any player.
	◦	Points banked at base.

2.3 Enemy AI Mechanics (Stationary Sentinel)
	•	Remain at spawn until provoked.
	•	Trigger Aggro when damaged or attacked.
	•	Chase attacker within a fixed Aggro Radius.
	•	Attempt melee/ranged attack when in range.
	•	Disengage and return to spawn if:
	◦	Target leaves Aggro Radius
	◦	No successful hit after X seconds

2.4 Enemy AI FSM
States & Transitions:
	1	Idle → Aggro (on attacked)
	2	Aggro (Chase) → Attack (in range)→ Return (target out of radius / timeout)
	3	Attack → Aggro (target moves out of range)→ Return (target leaves radius / timeout)
	4	Return → Idle (on reaching spawn)

2.5 Enemy AI Behavior Tree
	•	Idle: Wait, listen for “attacked” event.
	•	Aggro: Pathfind to attacker, attempt attack if in range.
	•	Attack: Play animation, apply damage, repeat.
	•	Return: Pathfind back to spawn, reset state.

2.6 Match Modes
	•	5 min 3v3 — focus for MVP build.
	•	Shorter and longer match lengths unlock later.

3. Technical Design Document (TDD)
3.1 Tech Stack
	•	Unity Editor: 6000.1.x (LTS)
	•	Input System: 1.14.2 (Action maps for movement/attack)
	•	AI Navigation: 2.0.8 (NavMesh + offmesh links for jump routes)
	•	Netcode for GameObjects: 2.4.4 (server authoritative)
	•	Unity Physics: 1.3.14 (deterministic collision & movement)
	•	Behavior Tree Framework: Unity’s com.unity.behavior

3.2 Architecture
	•	Server Authoritative Simulation:
	◦	FSM + BT run entirely on server.
	◦	Clients predict only for their own characters.
	•	Modules:
	◦	Input Handler
	◦	Player Controller (movement, physics, abilities)
	◦	Combat Manager
	◦	Coin System
	◦	AI Controller (FSM + BT integration)
	◦	Networking Layer
	◦	UI Manager

3.3 FSM–BT Integration
	•	BT issues commands: MoveTo(target), Attack(target), ReturnToSpawn()
	•	FSM executes movement & animation sequences.
	•	FSM reports events: OnAttackHit, OnReturnComplete, OnAggroLost.

3.4 Networking Data Flow
	•	Server → Client: Positions, animations, health, coin counts.
	•	Client → Server: Player inputs (movement, ability use).
	•	Prediction & Reconciliation: Clients predict only movement; server corrects.

4. Project Plan
Phase 1 – Core Systems (Weeks 1–4)
	•	Input & movement (jump, double-jump, dash)
	•	Physics-based collision (Unity Physics 1.3.14)
	•	Networked player movement (Netcode 2.4.4)
	•	FSM system base
	•	BT system base
	•	Sentinel AI prototype
Phase 2 – Gameplay Loop (Weeks 5–8)
	•	Coin drop/collect logic
	•	Score banking system
	•	Match timer & results
	•	UI (HUD + score)
Phase 3 – Content & Polish (Weeks 9–12)
	•	Meme-themed abilities
	•	Character models (Elom Nusk, Doge Dog, Tronald Dump)
	•	Animations & sound
	•	Optimizations

5. Asset List
	•	Characters:
	◦	Elom Nusk (placeholder rigged humanoid)
	◦	Doge Dog (biped dog model)
	◦	Tronald Dump (placeholder rigged humanoid)
	•	Environment:
	◦	Platforms (varied sizes)
	◦	Crypto coin prefab
	◦	Spawn points
	•	UI:
	◦	HUD elements (health, coins, timer)
	•	Audio:
	◦	Placeholder SFX for jumps, hits, coin pickup
	•	Animations:
	◦	Idle, walk, jump, double jump, attack, return

6. Developer Checklist
Core Mechanics
	•	Implement Input System 1.14.2 bindings
	•	Implement jump/double-jump physics
	•	Implement dash mechanic
	•	Implement attack and damage system
AI
	•	Implement FSM states: Idle, Aggro, Attack, Return
	•	Implement BT tasks for each state
	•	Aggro trigger on “attacked”
	•	Leash logic to Aggro Radius
	•	Timeout logic for disengage
Networking
	•	Server authoritative FSM & BT
	•	NetworkTransform for position sync
	•	RPCs for attack triggers
UI
	•	HUD coin counter
	•	Match timer
	•	End screen
Testing
	•	Verify Aggro works with latency
	•	Test double-jump edge cases
	•	Test AI return-to-spawn behavior
