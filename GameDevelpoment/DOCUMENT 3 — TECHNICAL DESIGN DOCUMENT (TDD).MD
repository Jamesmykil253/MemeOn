========================================
DOCUMENT 3 — TECHNICAL DESIGN DOCUMENT (TDD)
========================================

TITLE:
Meme Online Battle Arena — Technical Architecture (Prototype 5-Minute 3v3 Mode)

VERSION:
v0.1 — Engineering Architecture Draft

AUTHORSHIP:
Engineering Lead: [Redacted]
AI Systems Architect: [Redacted]
Networking Lead: [Redacted]
Physics & Systems Engineer: [Redacted]
Date: [YYYY-MM-DD]

REVISION HISTORY:
0.1 — Initial technical draft with FSM/BT hybrid AI, Unity package integration, network & physics audit.

----------------------------------------
1. ARCHITECTURAL OVERVIEW
----------------------------------------
This TDD defines the full technical systems stack for the prototype, with emphasis on:
• Hybrid AI design (Finite State Machine + Behavior Tree)
• Server-authoritative networking
• Deterministic physics
• Modular subsystem interfaces

----------------------------------------
2. ENGINE & PACKAGE REQUIREMENTS
----------------------------------------
Engine: Unity Editor 60001.4f (LTS)
Mandatory Packages:
- Input System 1.14.2 (com.unity.inputsystem)
- AI Navigation 2.0.8 (com.unity.ai.navigation)
- Netcode for GameObjects 2.4.4 (com.unity.netcode.gameobjects)
- Unity Physics 1.3.14 (com.unity.physics)

Technical implications:
- Input System 1.14.2: Action Map–based event-driven input handling with PlayerInput component and custom network input queuing.
- AI Navigation 2.0.8: NavMeshSurface for bake-time static navigation; OffMeshLinks for jump zones; runtime NavMeshAgent path following.
- Netcode for GameObjects 2.4.4: NetworkBehaviour, NetworkVariable<T>, RPCs, server-authoritative transform sync.
- Unity Physics 1.3.14: Deterministic broadphase queries, per-frame step sync with server tick rate.

----------------------------------------
3. SYSTEM MODULES
----------------------------------------
3.1 Input Layer
- Implement InputActionAsset with bindings:
  • Move (Vector2)
  • Jump (Button)
  • AttackPrimary (Button)
  • AttackSecondary (Button)
  • Interact (Button)
  • Look (Vector2)
  • PanModifier (Button) — bind to Player.Sprint (Left Shift)
- Client sends buffered input frames to server; server applies validated inputs.

3.2 Player Controller
- Handles movement physics (velocity, acceleration, friction) using Unity Physics.
- Jump & double-jump tracked via state variables:
  bool isGrounded, bool canDoubleJump.
- Ground check: SphereCast to ground layer; resets jump flags on hit.
- Animation event hooks for attack timing.
 - Death signalling: NetworkHealth.OnDeath used to inform camera system to enter free-pan mode for the owning player.

3.3 Enemy AI Controller
- Server-side only. Each NPC has:
  - FSM (atomic locomotion/combat states)
  - BT (high-level tactical decision-making)
  - Shared blackboard for cross-layer state.
- Aggro acquisition: OnDamage event → set target in blackboard.
- Give-up logic: if timeSinceLastSuccessfulHit >= giveUpTimeout OR distanceFromSpawn >= maxPursueRadius → ReturnToSpawn.

3.4 Networking Layer
- Server authoritative model:
  - Player movement, AI decisions, physics resolved server-side.
  - Netcode for GameObjects used for spawning, despawning, and minimal state sync.
- Sync objects:
  - Player/Enemy positions via NetworkTransform (with interpolation).
  - Health, coin count via NetworkVariable<int>.
- RPC usage:
  - Client → Server: Input frames, ability activation requests.
  - Server → Client: Damage events, coin spawn events, match state changes.

3.5 Physics & Collision
- Unity Physics 1.3.14 deterministic mode enabled.
- Projectile collision: PhysicsWorld.CastRay with collision filter.
- Melee collision: OverlapSphere query at animation hit window.
- Ground checks: CapsuleCast against environment layer.

3.6 Game Manager
- Runs match timer, coin spawn logic, and win condition evaluation.
- Spawns characters and assigns team IDs.

3.7 Camera System
- UniteCameraController
  - Alive state: Follows target with smoothing and offset. If PanModifier (Sprint/Left Shift) is pressed (or if not bound, any Look input), applies a temporary pan offset: Look.x pans along camera-right projected to XZ; Look.y pans along world Y. Pan offset is clamped to a configurable horizontal radius and recenters with a spring when not panning.
  - Dead state: When NetworkHealth for the target reaches 0, the camera detaches to free mode. Move.x affects horizontal panning along camera-right; Move.y affects world Y elevation (not Z). Zoom uses mouse scroll for editor convenience. Camera continues to LookAt the last known target position.
  - Input sources: Uses InputActionReferences if assigned; otherwise falls back to generated InputSystem_Actions (Player.Look, Player.Move, Player.Jump, Player.Crouch, UI.RightClick for PanModifier).

----------------------------------------
4. HYBRID AI DESIGN
----------------------------------------
4.1 Finite State Machine (FSM)
States:
1. Idle — stationary at spawn, no target.
2. Alerted — reaction delay after aggro acquisition.
3. Pursue — move toward target via NavMesh pathing.
4. MeleeAttack — execute melee animation; apply damage at hit frame.
5. RangedAttack — spawn projectile prefab; server handles physics.
6. Stunned — temporary disable on receiving CC.
7. ReturnToSpawn — path back to spawn point.
8. Dead — disable AI until respawn timer expires.

Transitions:
- Idle → Alerted: OnDamageReceived.
- Alerted → Pursue: After reaction delay.
- Pursue → MeleeAttack: Target in melee range && attackReady.
- Pursue → RangedAttack: Target in range window && attackReady.
- Any → Stunned: OnStunEvent.
- Pursue → ReturnToSpawn: giveUpTimeout expired OR exceeded maxPursueRadius.
- ReturnToSpawn → Idle: At spawn position.

4.2 Behavior Tree (BT)
Root Selector:
- Condition: IsDead? → HandleRespawn
- Condition: HasTarget? → EngageSequence
- Fallback: IdleAction

EngageSequence (Sequence):
1. SetTarget (blackboard write)
2. EvaluateThreat (conditional branch to EvadeSequence if target high threat)
3. MoveToTarget (NavMesh path follow)
4. AttackDecisionSelector:
   - If melee range → MeleeAttackAction
   - Else if ranged window → RangedAttackAction

EvadeSequence (Sequence):
1. ComputeSafePoint (NavMesh sample)
2. MoveToSafePoint
3. Wait/Assess
4. ReturnToSpawn if safe

Blackboard Keys:
- spawnPosition (Vector3)
- targetId (NetworkObjectId)
- lastHitTimestamp (float)
- timeSinceLastHit (float)
- failedHitCounter (int)
- aggroed (bool)
- giveUpTimeout (float)
- maxPursueRadius (float)

----------------------------------------
5. NETWORK SYNCHRONIZATION RULES
----------------------------------------
- AI runs entirely server-side; clients only receive position, state, and animation triggers.
- FSM state changes trigger lightweight RPCs for animation sync.
- Damage application only occurs server-side; clients receive events to update UI.

----------------------------------------
6. DATA SCHEMAS
----------------------------------------
PlayerProfile:
{
  "id": "UUID",
  "characterType": "ElomNusk|DogeDog|TronaldDump",
  "health": 100,
  "move": { "maxSpeed": 6.0, "accel": 35.0, "jumpVel": 7.5, "doubleJump": true },
  "abilities": [...],
  "spawnPosition": [x,y,z]
}

AI_Blackboard:
{
  "spawnPosition": [x,y,z],
  "targetId": null,
  "aggroed": false,
  "lastHitTimestamp": 0.0,
  "failedHitCounter": 0,
  "giveUpTimeout": 5.0,
  "maxPursueRadius": 20.0
}

----------------------------------------
7. TICK RATE & PERFORMANCE
----------------------------------------
- Server Tick Rate: 60 Hz
- Physics substeps: 1 (fixed timestep 1/60s)
- Network send rate: 20 Hz for transform sync; event-based for RPCs.
- Target: ≤ 5 ms server frame time at ≤12 players + 6 AI NPCs.

----------------------------------------
8. TEST PLAN HOOKS
----------------------------------------
- Unit test FSM transitions (mock blackboard inputs).
- BT decision tests with simulated conditions.
- Network latency simulation (50–150 ms) with packet loss injection.
- Physics consistency check between client and server simulation.

----------------------------------------
9. RISKS & MITIGATIONS
----------------------------------------
Risk: NavMesh agent vs. physics character controller conflicts in AI.
Mitigation: Separate pathfinding (NavMesh) from actual movement execution (physics forces).

Risk: Client prediction divergence with Unity Physics.
Mitigation: Regular server reconciliation and delta correction.

Risk: Excessive network load from frequent AI state changes.
Mitigation: Throttle state sync to animation-change events only.

----------------------------------------
10. DELIVERABLES FROM THIS TDD
----------------------------------------
- FSM and BT pseudocode reference for implementation.
- Blackboard schema for AI integration.
- Network API usage reference per package version.
- Deterministic physics settings for Unity Physics 1.3.14.
