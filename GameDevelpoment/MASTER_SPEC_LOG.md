# Meme Online Battle Arena — Master Spec Log (Unified)

**Engine & Pipeline**
- Unity: `6000.1.15f1`
- URP Mobile: `6.1.2`
- Physics: **PhysX (GameObject)** — *No DOTS/Unity Physics packages*
- Networking: **Netcode for GameObjects** (server-authoritative, client prediction allowed for players; AI runs on server)

**Authoritative Model**
- Server simulates AI, health, hits, scoring, and match timer.
- Clients are visual only for AI; positions sync via `NetworkTransform`.
- Standard event names unified: `OnDamageReceived`, `OnSuccessfulHit`, `OnFailedHit`.

**Match & Modes**
- 3v3 mode, match length **300s** (5 minutes).
- Deposit throttle: **0.5s** minimum between deposits.
- Respawn: **3.0s** default.

**AI State Set (FSM)**
- `Idle`, `Alert`, `Pursue`, `MeleeAttack`, `RangedAttack`, `Evade`, `Stunned`, `ReturnToSpawn`, `Dead/Respawn` (here as `Dead` state with timed respawn).

**Behavior Tree (BT)**
- Runs perception (LOS/distance stubs) at **5 Hz**, generates intents (e.g., `ShouldEvade`).
- FSM executes concrete actions based on BT signals.

**Blackboard Keys**
- `spawnPosition: Vector3`
- `targetId: ulong`
- `aggroed: bool`
- `lastHitTimestamp: float`
- `timeSinceLastSuccessfulHit: float`
- `lastKnownTargetPos: Vector3`

**Performance Budgets (Mobile)**
- AI tick: **10 Hz** server-side (configurable).
- Timer tick: **1 Hz**.
- Anim parameter hashing cached; no per-frame allocations; LINQ avoided.
- Component caching (`NavMeshAgent`, `Animator`, `HealthServer`).

**Consistency & Naming**
- Constants centralized in `ProjectConstants` to eliminate drift.
- Event names and blackboard keys mirrored exactly across systems.

**Gaps / Needs Confirmation**
- Exact player movement speeds & acceleration per class (defaults used).
- Concrete projectile/melee hit resolution (placeholders emit events).
- Layer/Tag indices may need alignment with project settings.
- Scoring rules & UI integration hooks — placeholders in `NetworkGameManager`.
- NavMesh baking settings for mobile maps (cell size/agent radius).

**Change History (this pass)**
- Created `ProjectConstants.cs` with gameplay, AI, events, keys.
- Implemented `CombatEvents.cs` (server-only event bus).
- Implemented `HealthServer.cs` (authoritative health & death).
- Implemented `AIBlackboard.cs` (allocation-free blackboard).
- Implemented `AIState.cs` base class.
- Implemented `AIController.cs` (FSM w/ states including `Evade`; AI-only server tick at 10 Hz; BT integration).
- Implemented `AIBehaviorTree.cs` (perception/intent at 5 Hz).
- Implemented `NetworkGameManager.cs` (match state & 1 Hz timer).

