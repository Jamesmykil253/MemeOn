========================================
DOCUMENT 6 — DEVELOPMENT CHECKLIST
========================================

TITLE:
Meme Online Battle Arena — Prototype 5-Minute 3v3 Mode
Stepwise Implementation Plan (AI-Executable Format)

VERSION:
v0.1 — Sequential Task Execution Guide

AUTHORSHIP:
Engineering Lead: [Redacted]
Production Manager: [Redacted]
AI Systems Architect: [Redacted]
Date: [YYYY-MM-DD]

REVISION HISTORY:
0.1 — Initial sequential implementation checklist.

----------------------------------------
1. PRE-PROJECT SETUP
----------------------------------------
[ ] Install Unity Editor 60001.4f (LTS)
[ ] Create new Unity 3D URP project
[ ] Add required packages via Package Manager:
    - com.unity.inputsystem (1.14.2)
    - com.unity.ai.navigation (2.0.8)
    - com.unity.netcode.gameobjects (2.4.4)
    - com.unity.physics (1.3.14)
[ ] Configure Editor settings:
    - Fixed Timestep = 0.0166667 (60Hz)
    - Maximum Allowed Timestep = 0.05
    - Script Runtime Version = .NET 4.x
[ ] Create Git repository and enable LFS for large assets

----------------------------------------
2. INPUT SYSTEM CONFIGURATION
----------------------------------------
[ ] Create InputActionAsset: "PlayerControls.inputactions"
[ ] Define Action Map: "Gameplay"
    - Move (Value/Vector2)
    - Jump (Button)
    - AttackPrimary (Button)
    - AttackSecondary (Button)
    - Interact (Button)
[ ] Generate C# class from InputActionAsset
[ ] Implement PlayerInput component in Player prefab
[ ] Set PlayerInput to "Send Messages" or "Invoke Unity Events"

----------------------------------------
3. PLAYER CONTROLLER IMPLEMENTATION
----------------------------------------
[ ] Create PlayerController.cs
[ ] Implement movement with Unity Physics:
    - Rigidbody-based with force/velocity application
[ ] Ground detection via SphereCast
[ ] Jump mechanic with velocity impulse
[ ] Double jump logic with flag reset on ground contact
[ ] Hook input actions to movement/jump methods
[ ] Integrate NetworkBehaviour for server-authoritative input

----------------------------------------
4. NETWORKING FOUNDATION
----------------------------------------
[ ] Configure Netcode for GameObjects:
    - NetworkManager prefab
    - Transport = Unity Transport (UTP)
[ ] Create NetworkPlayer prefab:
    - NetworkObject
    - PlayerController
    - NetworkTransform for sync
[ ] Implement server-authoritative movement:
    - Client sends input to server
    - Server simulates movement and sends back transform
[ ] Implement reconciliation to correct client drift

----------------------------------------
5. AI BASE ARCHITECTURE
----------------------------------------
[ ] Create AIController.cs
[ ] Implement FSM core states:
    - Idle
    - Alerted
    - Pursue
    - MeleeAttack
    - RangedAttack
    - Stunned
    - ReturnToSpawn
    - Dead
[ ] Implement BT layer for tactical decision-making:
    - Root Selector
    - EngageSequence
    - EvadeSequence
[ ] Create Blackboard system for AI state
[ ] Integrate AI Navigation:
    - NavMesh bake
    - OffMeshLinks for jump zones

----------------------------------------
6. COIN & ECONOMY SYSTEM
----------------------------------------
[ ] Create Coin prefab:
    - NetworkObject
    - Coin.cs script
[ ] Implement spawn manager for coins (server-controlled)
[ ] Implement coin pickup:
    - Trigger collider
    - Server increments player coin count
[ ] Implement drop on death:
    - Spawn coin prefabs at death position
[ ] Implement DepositZone prefab:
    - Trigger collider
    - Server increments team score

----------------------------------------
7. COMBAT SYSTEM
----------------------------------------
[ ] Implement melee attack detection via OverlapSphere
[ ] Implement ranged attacks with Physics raycasts/projectiles
[ ] Server applies damage and sends RPC to update health UI
[ ] Implement knockback and stun effects
[ ] Cooldown tracking on server with predicted UI client-side

----------------------------------------
8. MATCH MANAGEMENT
----------------------------------------
[ ] Create MatchManager.cs
[ ] Implement:
    - Match timer countdown
    - Team score tracking
    - Win condition evaluation
    - Sudden death on tie
[ ] RPC match start/end events to clients

----------------------------------------
9. UI IMPLEMENTATION
----------------------------------------
[ ] Create HUD canvas:
    - Health bar
    - Coin counter
    - Timer
    - Ability cooldown icons
[ ] Link UI to player stats via NetworkVariables
[ ] Create Lobby UI for character select
[ ] Create Match End screen with scoreboard

----------------------------------------
10. PLACEHOLDER ASSET INTEGRATION
----------------------------------------
[ ] Import temp meshes for characters, AI, coins, and environment
[ ] Import placeholder animations for locomotion and combat
[ ] Assign temporary materials/textures
[ ] Import placeholder audio clips and link to events
[ ] Create basic VFX placeholders for hit, coin pickup, respawn

----------------------------------------
11. TESTING & VALIDATION
----------------------------------------
[ ] Unit test FSM transitions with mock blackboard values
[ ] Simulate BT decision flow with various inputs
[ ] Perform latency simulation (50–150 ms RTT) with packet loss
[ ] Validate deterministic physics with same seed on client/server
[ ] Playtest matches with mixed human/AI players

----------------------------------------
12. OPTIMIZATION & FINALIZATION
----------------------------------------
[ ] Profile performance and memory usage
[ ] Optimize NavMeshAgent update frequency
[ ] Reduce network bandwidth via delta compression
[ ] Fix critical bugs
[ ] Tag release candidate build for playtesting

----------------------------------------
EXIT CRITERIA:
• All systems functional and integrated
• Prototype matches run start-to-finish without major errors
• AI adheres to stationary sentinel spec in 95%+ test cases
• Network remains stable under 12 players + 6 AI load
