# Playable Build Asset & Wiring Checklist (Unity + Netcode for GameObjects)

This document lists every asset, prefab, component, and setting you need to make the current build format work end-to-end. Use it as a cut-and-dry QA checklist.

## 1) Packages
- Netcode for GameObjects (com.unity.netcode.gameobjects)
- Unity Transport (com.unity.transport)
- Input System (com.unity.inputsystem)
- TextMeshPro (built-in)

## 2) Layers & Tags
- Layers (must match `ProjectConstants.Layers` or adjust constants):
  - Environment = 3
  - Player = 6
  - Enemy = 7
  - AI = 8
  - Projectile = 9
  - Minion = 10 (placeholder; set to your actual index or update constants)
- Tags:
  - Player, AI, Projectile

## 3) Scenes & Build Settings
- Scenes in Build Settings:
  - `Assets/Scenes/Bootstrap.unity`
  - Gameplay scene (e.g., `Assets/Scenes/Game/Gameplay_01.unity`)
- NGO SceneManager config: gameplay scene registered and loadable.

## 4) Bootstrap Scene Objects
- NetworkManager (with UnityTransport)
  - Player Prefab assigned (see Player Prefab section)
  - Optional: NetworkManagerHud for quick host/client start
- NetworkGameManager (script: `Assets/Scripts/Network/NetworkGameManager.cs`)
  - Optional: `NetworkPrefabsRegistrar` assigned
- SceneLoaderOnHost (script: `Assets/Scripts/Network/SceneLoaderOnHost.cs`)
  - `gameplaySceneName` matches your gameplay scene

## 5) Camera (in Gameplay Scene)
- Main Camera
  - `CameraBootstrap` (auto-retargets to local player)
  - `UniteCameraController`
    - RotationSource: PitchYaw (or Transform)
    - Offset: e.g., (0, 6, -6)
    - Smooth/Zoom: defaults OK
    - Manual Free Pan: optional; set deadzone if enabled
  - Optional: `LimitedViewController` for layer culling distances

## 6) Player Prefab (assigned in NetworkManager)
- Root GameObject (tag Player, layer Player)
  - NetworkObject (Is Player Object via NetworkManager assignment)
  - CharacterController (tune radius/height/stepOffset)
  - NetworkTransform (Server authority)
  - TeamId (team = 0 by default)
  - NetworkHealth (maxHealth set)
  - PlayerInventory
  - PlayerMovement
    - InputActionReferences optional (uses fallbacks)
    - Jump/double-jump tuned as desired
  - PlayerCombatController (optional)
    - projectilePrefab assigned
- Optional child: world-space Canvas with `HealthBarUI` bound to NetworkHealth

## 7) AI Prefab(s)
- Root (tag AI, layer AI)
  - NetworkObject
  - CharacterController
  - NetworkTransform
  - TeamId (team = 1 by default)
  - NetworkHealth
  - AIController (+ AIBlackboard)
  - Optional: muzzle transform
  - Optional: projectilePrefab assigned

## 8) Projectile Prefab(s)
- Root (layer Projectile)
  - NetworkObject
  - NetworkTransform
  - Collider (Is Trigger)
  - ProjectileServer
- Register in NetworkManager prefabs list

## 9) Coin System
- Coin Prefab
  - NetworkObject
  - Trigger Collider
  - Coin
- CoinSpawner (scene object)
  - NetworkObject
  - CoinSpawner script
  - coinPrefab assigned
  - spawnPoints populated with Transforms

## 10) Deposit/Goal Zones
- For each team goal:
  - GameObject with Trigger Collider
  - NetworkObject
  - DepositZone
    - zoneType = TeamGoal
    - teamId set
    - healPerSecond, enemySlowMultiplier tuned
- Neutral zones: zoneType = Neutral

## 11) Match Systems & UI
- MatchManager (scene object)
  - NetworkObject
  - MatchManager script
  - matchLengthSeconds set
- UI (optional): MatchEventsUI on a Canvas with TMP texts

## 12) Network Prefab Registration
Choose ONE:
- Asset: `Assets/DefaultNetworkPrefabs.asset` contains Player, AI, Projectile(s), Coin
- Runtime: `NetworkPrefabsRegistrar` in Bootstrap, list includes the same prefabs

## 13) Input
- Ensure `Assets/InputSystem_Actions.inputactions` exists and generated `InputSystem_Actions.cs` present
- If you customize, either assign `InputActionReference`s or regenerate the class

## 14) Validation Pass (pre-play)
- Player Prefab set in NetworkManager
- Scenes in Build Settings match loader and NGO SceneManager
- All networked scene objects have NetworkObject
- Colliders on coins and projectiles are present and triggers as required
- TeamId set on players/AI and goal zones configured for teams
- Network prefabs registered (asset or registrar)

## 15) Runbook (Editor)
- Open Bootstrap scene
- Press Play
- Start Host (or Server+Client)
- Observe: gameplay scene loads networked; local player spawns and camera follows
- WASD/Left Stick to move; Space to jump; Attack if configured
- Collect coins, deposit at your team goal; enemy goals slow you

## 16) Known Gaps / Options
- `ProjectConstants.Layers.Minion` may need to match your TagManager index
- Consider adding coyote time + jump buffering to PlayerMovement for feel
- Prefer adding NetworkTransform to prefabs (don’t rely on runtime add)
- If you need team-aware UI on clients, migrate `TeamId.team` to NetworkVariable
