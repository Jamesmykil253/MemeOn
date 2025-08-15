# Technical Design Document (TDD) — Master-Reviewed (v1.0, 2025-08-15)

## 1. Stack & Versions
- Unity **6000.1.15f1** (URP Mobile 6.1.2).
- Input System **1.14.2**; AI Navigation **2.0.8**; NGO **2.4.4**.
- **Physics:** GameObject/PhysX only for prototype.

## 2. Architecture
- **Server authoritative** simulation: server runs physics, AI (FSM+BT), validates inputs; clients predict only their own movement.
- Modules: Input → Player Controller → Combat → Coin/Deposit → AI Controller (FSM+BT) → Networking → UI.

## 3. Input Layer
- `Move (Vector2)`, `Jump`, `AttackPrimary`, `AttackSecondary`, `Interact` via action maps.
- Client → Server input queue with frame stamping; reconciliation hooks.

## 4. Player Controller
- Rigidbody motion; Sphere/Capsule ground checks; jump & **double‑jump** flag; air control clamped.
- Animation events drive melee hit windows.

## 5. AI
- **FSM** (Idle, Alerted, Pursue, MeleeAttack, RangedAttack, **Evade**, Stunned, ReturnToSpawn, Dead/Respawn).
- **BT** (custom, minimal): Root Selector → (IsDead? → HandleRespawn) | (HasTarget? → EngageSequence) | IdleAction.
- **Blackboard (server)**: 
  - `spawnPosition(Vector3)`, `targetId(ulong)`, `aggroed(bool)`
  - `lastHitTimestamp(float)`, `timeSinceLastSuccessfulHit(float)`
  - `failedHitCounter(int)`, `giveUpTimeout(float)`, `maxPursueRadius(float)`
  - `patienceThreshold(int)`, `currentState(enum)`

## 6. Navigation
- NavMeshSurface bake; OffMeshLinks for jump gaps. NavMeshAgent used for path **planning** (Auto Braking off, updates ~0.25s); movement applied via physics.

## 7. Networking
- Transform: `NetworkTransform` with interpolation at 20–30 Hz.  
- State: minimal `NetworkVariable<int>` for HP/coins; RPCs for damage, spawns, deposits, match state.
- Do **not** replicate BT internals; replicate only animation/state changes necessary for visuals.

## 8. Physics & Collision (PhysX)
- Melee: `Physics.OverlapSphere` during hit window.  
- Ranged: `Physics.Raycast` or Rigidbody projectile + continuous collision where needed.  
- Ground: `Physics.SphereCast`/`CapsuleCast` for stable landing detection.

## 9. Data Schemas
**PlayerProfile**
```json
{
  "id":"uuid",
  "archetype":"ElomNusk|DogeDog|TronaldDump",
  "health":100,
  "move":{"maxSpeed":6.0,"accel":35.0,"jumpVelocity":7.5,"doubleJumpAllowed":true},
  "abilities":[],
  "spawnPosition":[0,0,0]
}
```
**AI_Blackboard**
```json
{
  "spawnPosition":[0,0,0],
  "targetId":null,
  "aggroed":false,
  "lastHitTimestamp":0.0,
  "timeSinceLastSuccessfulHit":0.0,
  "failedHitCounter":0,
  "giveUpTimeout":5.0,
  "maxPursueRadius":20.0,
  "patienceThreshold":3,
  "currentState":"Idle"
}
```

## 10. Tick Rates & Budgets
- Server tick: 60 Hz physics; AI tick 20–30 Hz; network send 20–30 Hz.  
- Apply mobile budgets from Executive Concept.

## 11. Tests
- Unit: FSM transitions; BT branches (mock blackboard).  
- Net: latency 50–150 ms, 1–3% loss; reconciliation correctness.  
- Perf: NPC count, pathfinding, GPU draw calls; thermals 10‑min soak.
