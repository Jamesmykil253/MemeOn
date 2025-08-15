========================================
DOCUMENT 7 — AI LOGIC REFERENCE (FSM + BT)
========================================

TITLE:
Meme Online Battle Arena — AI Stationary Sentinel Logic
Finite State Machine (FSM) + Behavior Tree (BT) Pseudocode

VERSION:
v0.1 — AI-Executable Logic Spec

AUTHORSHIP:
AI Systems Architect: [Redacted]
Engineering Lead: [Redacted]
Date: [YYYY-MM-DD]

REVISION HISTORY:
0.1 — Initial pseudocode extraction and compilation from TDD.

----------------------------------------
1. PURPOSE
----------------------------------------
This document provides explicit pseudocode for the hybrid AI design
used in the stationary sentinel NPCs. It combines a Finite State Machine
for low-level locomotion and animation control with a Behavior Tree for
high-level tactical decision-making.

This reference is AI-executable and structured for direct porting into C# 
scripts using Unity 60001.4f with:
- AI Navigation 2.0.8
- Unity Physics 1.3.14
- Netcode for GameObjects 2.4.4

----------------------------------------
2. BLACKBOARD SCHEMA
----------------------------------------
Blackboard {
    Vector3 spawnPosition
    NetworkObjectId targetId
    bool aggroed
    float lastHitTimestamp
    float timeSinceLastHit
    int failedHitCounter
    float giveUpTimeout   // seconds
    float maxPursueRadius // meters
}

----------------------------------------
3. FINITE STATE MACHINE (FSM) PSEUDOCODE
----------------------------------------
State: Idle
    Enter:
        StopMovement()
        aggroed = false
        targetId = null
    Update:
        if OnDamageReceived(attackerId):
            targetId = attackerId
            aggroed = true
            ChangeState(Alerted)

State: Alerted
    Enter:
        PlayAnimation("Alert")
        SetTimer("alertDelay", 0.5f)
    Update:
        if TimerExpired("alertDelay"):
            ChangeState(Pursue)

State: Pursue
    Enter:
        PlayAnimation("Run")
    Update:
        if TargetLost():
            ChangeState(ReturnToSpawn)
        else if DistanceTo(targetId) <= meleeRange && AttackReady():
            ChangeState(MeleeAttack)
        else if InRangedWindow(targetId) && AttackReady():
            ChangeState(RangedAttack)
        else if timeSinceLastHit >= giveUpTimeout:
            ChangeState(ReturnToSpawn)
        else:
            MoveToward(targetId)

State: MeleeAttack
    Enter:
        PlayAnimation("MeleeAttack")
        ResetAttackCooldown()
    Update:
        if AnimationHitFrame():
            ApplyMeleeDamage(targetId)
        if AnimationComplete():
            ChangeState(Pursue)

State: RangedAttack
    Enter:
        PlayAnimation("RangedAttack")
        ResetAttackCooldown()
    Update:
        if AnimationHitFrame():
            SpawnProjectile(targetId)
        if AnimationComplete():
            ChangeState(Pursue)

State: Stunned
    Enter:
        PlayAnimation("Stunned")
        DisableMovement()
        SetTimer("stunDuration", stunTime)
    Update:
        if TimerExpired("stunDuration"):
            ChangeState(Pursue)

State: ReturnToSpawn
    Enter:
        PlayAnimation("Walk")
    Update:
        MoveToward(spawnPosition)
        if DistanceTo(spawnPosition) <= 0.5f:
            ChangeState(Idle)

State: Dead
    Enter:
        PlayAnimation("Death")
        DisableAllActions()
        SetTimer("respawnDelay", respawnTime)
    Update:
        if TimerExpired("respawnDelay"):
            RespawnAt(spawnPosition)
            ChangeState(Idle)

----------------------------------------
4. BEHAVIOR TREE (BT) PSEUDOCODE
----------------------------------------
RootSelector
    ├── If(IsDead) → HandleRespawn
    ├── If(HasTarget) → EngageSequence
    └── IdleAction

EngageSequence (Sequence)
    1. SetTarget (blackboard.targetId from aggro source)
    2. EvaluateThreat
        IF TargetThreatLevel() == HIGH → EvadeSequence
        ELSE Continue
    3. MoveToTarget (NavMesh path follow)
    4. AttackDecisionSelector
        ├── If(InMeleeRange) → MeleeAttackAction
        └── ElseIf(InRangedWindow) → RangedAttackAction

EvadeSequence (Sequence)
    1. ComputeSafePoint (NavMesh.SamplePosition away from target)
    2. MoveToSafePoint
    3. Wait(2.0f)
    4. ReturnToSpawn

IdleAction (Action)
    PlayAnimation("Idle")
    RemainStationary()

MeleeAttackAction (Action)
    ChangeState(MeleeAttack) // FSM call

RangedAttackAction (Action)
    ChangeState(RangedAttack) // FSM call

HandleRespawn (Action)
    RespawnAt(blackboard.spawnPosition)
    ChangeState(Idle)

----------------------------------------
5. EVENT HOOKS
----------------------------------------
OnDamageReceived(attackerId):
    if !aggroed:
        targetId = attackerId
        aggroed = true
        ChangeState(Alerted)

OnSuccessfulHit():
    lastHitTimestamp = CurrentTime()
    timeSinceLastHit = 0
    failedHitCounter = 0

OnFailedHit():
    failedHitCounter += 1
    if failedHitCounter > MaxAllowedMisses:
        ChangeState(ReturnToSpawn)

----------------------------------------
6. NETWORK RULES FOR AI
----------------------------------------
• AI logic runs exclusively on the server
• FSM state changes → send lightweight RPC to all clients for animation sync
• Blackboard values not directly synced; only relevant state changes are networked
• Damage resolution and aggro acquisition are server-only events

----------------------------------------
7. UNITY IMPLEMENTATION NOTES
----------------------------------------
- NavMeshAgent: auto-braking disabled; path updates every 0.25s
- OffMeshLinks: used for jump navigation; AI will only traverse if in Pursue state
- Unity Physics: all melee/ranged hit detection via server Physics queries
- Netcode: AI prefabs registered with NetworkManager, spawn controlled by server
