# PREFAB INTEGRATION GUIDE — AI Stationary Sentinel (Mobile-Tuned, v1.0, 2025-08-15)

## Target
Unity 6000.1.15f1 (URP Mobile 6.1.2)

## Components
- Transform
- **Rigidbody** (Use Gravity: true; Interpolate: Interpolate; Collision Detection: Discrete (switch to Continuous only if needed))
- **CapsuleCollider** (center 0,1,0; radius 0.5; height 2)
- **NavMeshAgent** (Speed 3.5; Angular 120; Accel 8; **Auto Braking OFF**)
- **Animator** (Root Motion OFF; Controller: `AI_Sentinel_Controller`)
- **NetworkObject**
- **AIController** (assign rb/agent/animator; server‑only logic)

## NavMesh
- Mark walkable as Navigation Static; bake with agent radius matching collider.
- Use **OffMeshLinks** across jump gaps. Ensure spawn area is on the NavMesh.

## Netcode
- Scene `_NetworkManager` with UTP; register AI prefab in Network Prefabs.
- Run AI only on server (`if (!IsServer) return;` in Update).

## Animator
States: Idle, Alert, Run, Walk, **Evade**, MeleeAttack, RangedAttack, Stunned, Death.  
Add Animation Events: Melee **HitWindow**; Ranged **ProjectileSpawn**.

## Script Hooks (AIController)
- `OnDamageReceived(ulong attackerId)` → set target/aggro; FSM Alerted.
- `OnSuccessfulHit()` → reset timeout counters.
- `OnFailedHit()` → increment failedHitCounter; BT may signal ReturnToSpawn.

## Verification
- Idle until damaged → pursue within radius → attack → give‑up by timeout/leash → return to spawn.
- Animator and NetworkTransform sync only on state changes (throttled).
