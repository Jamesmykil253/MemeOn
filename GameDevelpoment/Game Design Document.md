# Game Design Document (GDD) — Master-Reviewed (v1.0, 2025-08-15)

## 1. Overview
Hybrid 3D platformer MOBA with satirical characters and short‑form matches. Prototype delivers **5‑minute 3v3** on a single symmetric arena.

## 2. Core Loop
1. Spawn into base.
2. Navigate (run, jump, **double‑jump**, dash).
3. Fight (melee/ranged), coins drop from KOs and spawn nodes.
4. **Collect** coins and **deposit** at team zone for score.
5. Repeat until timer ends; if tied → **sudden‑death overtime (first deposit wins)**.
6. Results screen.

## 3. Mode Rules (Prototype)
- Team size: **3v3**.
- Timer: **5:00**; Respawn: 5s.
- Coin value: 1 point per deposited coin.
- Anti‑flood: **0.5s per coin** deposit throttle (server enforced).
- Overtime: infinite instant‑respawn until first **deposit**.

## 4. Map (Meme Coliseum — working title)
- ~60×60m, 3 vertical layers, mirrored left/right.
- **3 coin spawn nodes** with deterministic timers.
- **2 deposit zones** (one per team), symmetric.
- Jump gaps require single or double‑jump; **OffMeshLinks** for AI traversal.

## 5. Characters (Prototype)
**Elom Nusk** — balanced ranged; Health 100; Speed 6 m/s; JumpHeight 2.5 m; DoubleJump ✓; Abilities: Dash, Energy Shield.  
**Doge Dog** — agile melee; Health 80; Speed 7.5 m/s; JumpHeight 2.7 m; DoubleJump ✓ (short cooldown); Abilities: Bark Stun (AoE), Sprint.  
**Tronald Dump** — tank/bruiser; Health 140; Speed 5 m/s; JumpHeight 2.2 m; DoubleJump ✗; Abilities: Knockback, Area Taunt.

## 6. Player Mechanics
- Ground accel 35 m/s², friction/damping tuned; air control at ~60% of ground accel.
- **Jump/double‑jump**: double‑jump enabled when airborne and flag unused; reset on ground.
- Combat: melee via **OverlapSphere** on animation hit window; ranged via raycast/projectile; server resolves hits.

## 7. Economy
- Server controls coin spawns (nodes + on‑death drops).
- Inventory: carried coin count (NetworkVariable<int>).
- **Deposit** increments team score; removes coins from carrier; **0.5s/coin** throttle.

## 8. UI/HUD (Prototype)
- Health bar (self/target), carried coins, team scores, timer, ability cooldowns.

## 9. Non‑Functional
- **Authoritative server**; clients predict only local player; reconciliation corrects drift.
- Target 60 FPS on mid‑tier mobile under stated budgets.

## 10. Acceptance
- Players can complete the loop without critical bugs.
- Overtime rule triggers correctly (first deposit).  
- AI adheres to Stationary Sentinel spec (see AI Logic).
