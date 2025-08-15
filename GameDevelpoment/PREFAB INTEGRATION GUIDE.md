=====================================================
PREFAB INTEGRATION GUIDE — AI STATIONARY SENTINEL
=====================================================

PROJECT TARGET:
Unity 60001.4f LTS  
Packages:
    - com.unity.inputsystem (1.14.2)
    - com.unity.ai.navigation (2.0.8)
    - com.unity.netcode.gameobjects (2.4.4)
    - com.unity.physics (1.3.14)

PURPOSE:
This guide details the exact steps to wire the provided AI scripts
(AIBlackboard.cs, AIState.cs, AIController.cs, AIBehaviorTree.cs)
into a functional server-authoritative AI prefab, integrated with
NavMeshAgent, Animator, and Netcode for GameObjects.

-----------------------------------------------------
1. DIRECTORY & SCRIPT PLACEMENT
-----------------------------------------------------
Place scripts in:
    Assets/Scripts/AI/AIBlackboard.cs
    Assets/Scripts/AI/AIState.cs
    Assets/Scripts/AI/AIController.cs
    Assets/Scripts/AI/AIBehaviorTree.cs

Ensure namespace is not set unless using a shared project namespace.

-----------------------------------------------------
2. CREATE AI PREFAB
-----------------------------------------------------
2.1 In Unity Hierarchy:
    - Right-click > Create Empty
    - Rename: "AI_StationarySentinel"

2.2 Add Components:
    [ ] Transform — Position set to 0,0,0
    [ ] Rigidbody
        - Use Gravity: true
        - Is Kinematic: false
        - Interpolate: Interpolate
        - Collision Detection: Continuous
    [ ] Capsule Collider
        - Center: (0,1,0)
        - Radius: 0.5
        - Height: 2
    [ ] NavMeshAgent
        - Speed: 3.5
        - Angular Speed: 120
        - Acceleration: 8
        - Auto Braking: OFF
    [ ] Animator
        - Apply Root Motion: false
        - Controller: (assign Animator Controller created in step 5)
    [ ] NetworkObject
        - Check "Destroy With Scene": false
    [ ] AIController (from scripts)
        - Assign Rigidbody to "rb"
        - Assign NavMeshAgent to "agent"
        - Assign Animator to "animator"

-----------------------------------------------------
3. NAVMESH SETUP
-----------------------------------------------------
3.1 Bake NavMesh:
    - Window > AI > Navigation
    - Mark walkable surfaces as "Navigation Static"
    - Bake NavMesh with agent radius matching Capsule Collider (0.5)
3.2 Stationary behavior:
    - AI will not move until FSM transitions from Idle → Alerted → Pursue
    - Ensure spawn area is part of baked NavMesh

-----------------------------------------------------
4. NETCODE SETUP
-----------------------------------------------------
4.1 NetworkManager:
    - Create empty GameObject in scene called "_NetworkManager"
    - Add NetworkManager component
    - Set Transport: Unity Transport (UTP)
4.2 Register AI Prefab:
    - In NetworkManager > Network Prefabs, add "AI_StationarySentinel" prefab
4.3 Ensure AIController logic runs only on server:
    - Confirm IsServer checks in Update()

-----------------------------------------------------
5. ANIMATION SETUP
-----------------------------------------------------
5.1 Animator Controller:
    - Create Animator Controller: "AI_Sentinel_Controller"
    - States:
        - Idle
        - Alert
        - Run
        - Walk
        - MeleeAttack
        - RangedAttack
        - Stunned
        - Death
    - Transitions:
        - All animations return to Idle by default after completion
    - Add animation events for:
        - Hit frame in MeleeAttack
        - Projectile spawn in RangedAttack
5.2 Assign this Animator Controller to Animator on AI prefab

-----------------------------------------------------
6. SCRIPT HOOKS
-----------------------------------------------------
6.1 AIController:
    - `OnDamageReceived(ulong attackerId)` should be called by damage system
      when player attack hits this AI
    - `OnSuccessfulHit()` called when AI attack connects
    - `OnFailedHit()` called when AI misses
6.2 AIBehaviorTree:
    - Integrated into AIController partial; no separate MonoBehaviour needed
    - Initialize BT in `OnNetworkSpawn` for server instances:
        ```
        if (IsServer)
        {
            InitializeAI();
            InitBehaviorTree();
        }
        ```

-----------------------------------------------------
7. SERVER AUTHORITATIVE FLOW
-----------------------------------------------------
• AI logic runs exclusively on server (FSM + BT execution in Update)
• Client receives:
    - NetworkTransform sync
    - Animator sync via server-driven parameter changes
• All damage resolution and state transitions occur server-side

-----------------------------------------------------
8. PREFAB FINALIZATION
-----------------------------------------------------
8.1 Drag "AI_StationarySentinel" from Hierarchy to Project window to save as prefab  
8.2 Remove from scene if spawning via script:
    - AI spawns: `Instantiate(aiPrefab, spawnPosition, Quaternion.identity).GetComponent<NetworkObject>().Spawn();`
8.3 Verify:
    - AI remains idle until damaged
    - AI pursues attacker within radius
    - AI returns to spawn if attacker leaves max radius or no hit within timeout
    - Animator plays correct state based on FSM

-----------------------------------------------------
9. TESTING
-----------------------------------------------------
• Test singleplayer (Host mode) to ensure NavMesh, Animator, and FSM transitions work
• Test multiplayer with server-client separation:
    - Server runs AI logic
    - Clients see synced movement/animation
• Simulate latency: Edit > Project Settings > Network > Simulate Latency

-----------------------------------------------------
EXIT CRITERIA:
[ ] AI prefab exists and registered in NetworkManager
[ ] NavMesh baked and functional
[ ] Animator Controller set with correct states/events
[ ] FSM + BT hybrid AI runs server-only
[ ] AI pursues, attacks, and returns to spawn according to spec
[ ] No client-authoritative movement on AI objects
