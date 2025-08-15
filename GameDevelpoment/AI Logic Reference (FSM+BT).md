# AI Logic Reference — Stationary Sentinel (v1.0, 2025-08-15)

## Purpose
Authoritative server‑side hybrid AI: **FSM** for atomic locomotion/attacks, **BT** for tactical intent.

## Blackboard (server-only)
- `spawnPosition: Vector3`
- `targetId: ulong`
- `aggroed: bool`
- `lastHitTimestamp: float`
- `timeSinceLastSuccessfulHit: float`
- `failedHitCounter: int`
- `giveUpTimeout: float` (seconds)
- `maxPursueRadius: float` (meters)
- `patienceThreshold: int`
- `currentState: enum`

## FSM States
- **Idle** → (OnDamageReceived(attackerId)) → **Alerted**  
- **Alerted** (0.5s reaction) → **Pursue**  
- **Pursue** → 
  - if `distance<=meleeRange && attackReady` → **MeleeAttack**  
  - else if `inRangedWindow && attackReady` → **RangedAttack**  
  - else if `timeSinceLastSuccessfulHit>=giveUpTimeout or distanceFromSpawn>maxPursueRadius` → **ReturnToSpawn**
- **MeleeAttack / RangedAttack** → (on anim complete) → **Pursue**  
- **Stunned** (timer) → **Pursue**  
- **Evade** (optional backoff) → **Pursue**/**ReturnToSpawn**  
- **ReturnToSpawn** → (at spawn) → **Idle**  
- **Dead** (respawn timer) → **Idle**

**Events:** `OnDamageReceived(ulong)`, `OnSuccessfulHit()`, `OnFailedHit()`.

## BT Pseudocode (custom minimal)
```
RootSelector
  ├─ If(IsDead)        → HandleRespawn
  ├─ If(HasTarget)     → EngageSequence
  └─ IdleAction
EngageSequence:
  1. SetTarget(targetId)
  2. EvaluateThreat → if HIGH → EvadeSequence else AttackSequence
  3. MoveToTarget (NavMesh path; physics motion)
  4. AttackDecision:
        if InMeleeRange → MeleeAttackAction
        else if InRangedWindow → RangedAttackAction
EvadeSequence:
  ComputeSafePoint → MoveToSafePoint → Wait(2s) → Reassess/ReturnToSpawn
```
