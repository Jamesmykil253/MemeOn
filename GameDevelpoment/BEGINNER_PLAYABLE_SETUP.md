# MemeOn — Beginner Playable Setup Guide (Unity + Netcode for GameObjects)

This guide explains, step by step, how to configure your project so the current codebase is playable. It also calls out gaps and gotchas discovered during an audit so you can avoid common pitfalls.

Use this as a checklist while wiring the scene, prefabs, layers, and ScriptableObjects.


## Quick checklist

- Packages: Netcode for GameObjects (NGO), Unity Transport, Input System, TextMeshPro.
- Layers: Environment=3, Player=6, Enemy=7, AI=8, Projectile=9, Minion=10 (or adjust ProjectConstants).
- Tags: Player, AI, Projectile.
- NetworkManager in bootstrap scene, with Unity Transport and Player Prefab assigned.
- Gameplay scene in Build Settings and NGO SceneManager; optional SceneLoaderOnHost to auto-load.
- Camera: Main Camera has CameraBootstrap + UniteCameraController (+ optional LimitedViewController).
- Player prefab: CharacterController + NetworkObject (PlayerObject) + NetworkTransform + PlayerMovement + PlayerCombatController + PlayerInventory + NetworkHealth + TeamId.
- AI prefab: NetworkObject + CharacterController + NetworkTransform + AIController (+ AIBlackboard) + NetworkHealth + TeamId + optional muzzle + projectile prefab (for ranged).
- Projectile prefab: NetworkObject + NetworkTransform + Collider (Trigger) + ProjectileServer.
- Coin prefab: NetworkObject + Trigger Collider + Coin; add CoinSpawner (with NetworkObject) and spawn points in scene.
- Goals: DepositZone components with Trigger colliders and teamId for each team (ensure NetworkObject on the zone).
- MatchManager in scene (with NetworkObject) for timer/score; optional MatchEventsUI on a Canvas.
- Network prefabs registered in NetworkManager (DefaultNetworkPrefabs.asset or runtime registrar).


## Project-wide configuration

1) Packages
- Netcode for GameObjects (com.unity.netcode.gameobjects)
- Unity Transport (com.unity.transport)
- Input System (com.unity.inputsystem)
- TextMeshPro (built-in)

2) Layers and Tags
- Layers used by code (ProjectConstants.Layers):
  - Environment = 3
  - Player = 6
  - Enemy = 7
  - AI = 8
  - Projectile = 9
  - Minion = 10 (adjust or update ProjectConstants if your index differs)
- Tags (ProjectConstants.Tags): Player, AI, Projectile
- Ensure your prefabs and scene objects use the correct layers and tags where applicable.

3) Input System
- The repo provides `Assets/InputSystem_Actions.inputactions` and generated `InputSystem_Actions.cs`.
- Player and camera read input via `InputActionReference` or fallback to this generated asset automatically.
- If you customize actions, either assign references in the Inspector or regenerate the C# class.


## Scenes and startup flow

1) Bootstrap scene
- Create an empty scene (e.g., `Bootstrap`) with:
  - GameObject: `NetworkManager`
    - Components:
      - NetworkManager
      - UnityTransport
      - (Optional) NetworkManagerHud (from NGO samples) or your own start UI
      - Assign Player Prefab (see Player prefab section)
  - GameObject: `NetworkGameManager` (Assets/Scripts/Network/NetworkGameManager.cs)
    - Optionally assign a `NetworkPrefabsRegistrar` to auto-register required prefabs at runtime.
  - GameObject: `SceneLoaderOnHost` (Assets/Scripts/Network/SceneLoaderOnHost.cs)
    - Set `gameplaySceneName` to your gameplay scene (e.g., `Gameplay_01`).

- Add both `Bootstrap` and your gameplay scene (`Gameplay_01`) to Build Settings.
- In NGO’s SceneManager settings (via NetworkManager), ensure your gameplay scene is registered so it can be loaded networked.

2) Gameplay scene (loaded by SceneLoaderOnHost)
- Must contain the actual level and runtime objects:
  - Main Camera with camera scripts (see next section)
  - `MatchManager` (with NetworkObject)
  - Spawners (AI and coins), deposit/goal zones, and any environment geometry
  - If you are not using a Bootstrap loader, you can host directly in this scene; keep a NetworkManager here too.


## Camera setup
- On the Main Camera add:
  - `CameraBootstrap` (Assets/Scripts/Camera/CameraBootstrap.cs): auto-finds the local player to follow.
  - `UniteCameraController` (Assets/Scripts/Camera/UniteCameraController.cs): follow, live pan, free-pan on death.
    - Optional: wire `lookAction`, `moveAction`, `panModifierAction`; otherwise it uses the generated actions (Sprint is the pan modifier).
    - Tune `offset`, `pitchDeg`, `livePan*` speeds.
  - Optional: `LimitedViewController` (Assets/Scripts/Camera/LimitedViewController.cs) to set per-layer culling distances.


## Player prefab
- Required components:
  - NetworkObject (mark as PlayerObject by assigning it as Player Prefab in NetworkManager)
  - CharacterController
  - NetworkTransform
  - TeamId (Assets/Scripts/Utilities/TeamId.cs) — set `team = 0` (team index)
  - NetworkHealth (Assets/Scripts/Combat/NetworkHealth.cs) — set `maxHealth`
  - PlayerInventory (server-authoritative coins)
  - PlayerMovement (server-authoritative movement)
    - Optional InputActionReferences (Move/Attack/Jump) or rely on fallback
    - Gravity, rotation, jump settings
  - PlayerCombatController (optional if you want basic shooting)
    - Assign `projectilePrefab`

- Optional child objects:
  - World-space Canvas with `HealthBarUI` bound to the player’s `NetworkHealth`.


## AI prefab
- Required components:
  - NetworkObject
  - CharacterController
  - NetworkTransform
  - TeamId (set to 1 for the opposing team)
  - NetworkHealth
  - AIBlackboard (present in AI folder) on the same GameObject
  - AIController (Assets/Scripts/AI/Core/AIController.cs)
    - `config` (AIConfig ScriptableObject)
    - `stats` (CharacterStats ScriptableObject)
    - Optional: `projectilePrefab` (if ranged)
    - Optional: `muzzle` transform for projectile spawn

- ScriptableObjects needed:
  - Create `CharacterStats` (Assets > Create > MemeArena > Character Stats)
    - maxHealth, moveSpeed, rotationSpeed
  - Create `AIConfig` (Assets > Create > MemeArena > AI > Config)
    - aggroRadius, pursue radius, attack ranges, evasion and timings


## Projectile prefab
- Required components:
  - NetworkObject
  - NetworkTransform
  - Collider (prefer Capsule/Sphere), set `Is Trigger = true` (script sets it, but a collider must exist)
  - ProjectileServer (Assets/Scripts/Combat/ProjectileServer.cs)
- Layer: set to `Projectile` (index from ProjectConstants) to benefit from camera culling.
- Register in NetworkManager prefabs.


## Coins and Coin Spawner
- Coin prefab:
  - NetworkObject
  - Trigger Collider
  - Coin (Assets/Scripts/Items/Coin.cs)
  - Optional: visual spin speed via `rotateDegPerSec`

- CoinSpawner in scene:
  - Add a GameObject with:
    - NetworkObject (required, since CoinSpawner is a NetworkBehaviour)
    - CoinSpawner (Assets/Scripts/Spawning/CoinSpawner.cs)
      - Assign `coinPrefab`
      - Assign `spawnPoints` (array of Transforms in the scene)
      - Set respawn delay if desired


## Deposit/Goal zones
- For each team, create a goal trigger:
  - GameObject with a Trigger Collider (Box/Sphere)
  - NetworkObject (required; DepositZone is a NetworkBehaviour)
  - DepositZone (Assets/Scripts/Game/DepositZone.cs)
    - `zoneType = TeamGoal`
    - `teamId` = team index for this goal
    - `healPerSecond` for friendly heals
    - `enemySlowMultiplier` for enemies inside the goal

- Neutral deposit/contest areas:
  - Same as above but `zoneType = Neutral` (no passive heal/slow by default).


## Match management and UI
- `MatchManager` in gameplay scene:
  - Components: NetworkObject, MatchManager (Assets/Scripts/Game/MatchManager.cs)
  - Configure `matchLengthSeconds` if desired
  - Handles team scores and overtime/end-of-match notifications via client events

- Optional UI for overtime/end banners:
  - Canvas with two TMP_Texts
  - MatchEventsUI (Assets/Scripts/UI/MatchEventsUI.cs)
    - Assign the overtime and end banners


## Network prefabs registration
You can register network prefabs in either of two ways:

1) Asset-driven (recommended):
- Use `Assets/DefaultNetworkPrefabs.asset` and add:
  - Player prefab
  - AI prefab(s)
  - Projectile prefab(s)
  - Coin prefab

2) Runtime (also fine):
- Add a `NetworkPrefabsRegistrar` to a GameObject in the bootstrap scene.
- Populate its `networkPrefabs` list with the prefabs above.
- Assign this registrar to `NetworkGameManager.registrar`.


## Running locally (dev loop)
- In the Editor:
  - Open `Bootstrap` scene and press Play.
  - Start as Host (using a HUD or your own UI). The loader will open `Gameplay_01` networked.
  - The camera should automatically follow your local player. Use WASD/Left Stick to move; Space to jump; Attack to fire (if configured).
  - Coins spawn at `CoinSpawner` points. Walk over them to collect. Enter your team’s goal to deposit; enemy goals slow you.

- Without a HUD:
  - Add a minimal start UI, or temporarily attach an NGO sample `NetworkManagerHud` to the `NetworkManager`.


## Troubleshooting and gaps found (audit)
- DepositZone/AISpawner/CoinSpawner are NetworkBehaviours:
  - Ensure a NetworkObject component is on these GameObjects (scene objects are network-spawned automatically by NGO when the scene loads). Without it, `IsServer` checks won’t behave correctly and `OnNetworkSpawn` won’t run.

- TeamId replication:
  - `TeamId.team` is a plain int (not a NetworkVariable). Current gameplay checks run on the server, so this is fine. If you need team-aware UI on clients that can change at runtime, switch to a NetworkVariable or sync via RPC/event.

- Projectile damage path:
  - `ProjectileServer.OnTriggerEnter` calls `NetworkHealth.TakeDamageServerRpc` from the server. This works but is unnecessary overhead; calling a server-only method would be cheaper. Consider refactoring to a direct server method later.

- Layers alignment:
  - `ProjectConstants.Layers.Minion = 10` is a placeholder; match it to your TagManager index or adjust the constant.

- Scenes and NGO SceneManager:
  - `SceneLoaderOnHost` loads `gameplaySceneName`; ensure the scene exists, is in Build Settings, and is registered in NGO’s SceneManager.

- Player ownership and input:
  - No movement usually means the spawned player isn’t owned by the local client or input actions aren’t enabled. Ensure the Player Prefab is set in NetworkManager, and the object has `IsPlayerObject` and `OwnerClientId` is the local client.

- Health/UI initialization:
  - `NetworkHealth` now initializes on server and replicates. UI scripts that read current health on `OnEnable` may briefly show 0 until replication arrives; they update when the event fires.

- Camera free-pan on death:
  - The camera assumes the player is alive until health replicates. Confirm the player has `NetworkHealth` and a nonzero `maxHealth`.

- Input modifier for live pan:
  - The camera uses Sprint as the default pan modifier in fallback actions. You can assign a specific `panModifierAction` in the Inspector if you prefer a different key/mouse button.


## Minimal asset list (quick reference)
- Prefabs:
  - Player, AI, Projectile, Coin
- ScriptableObjects:
  - CharacterStats (one or more variants), AIConfig (one or more variants)
- Scenes:
  - Bootstrap, Gameplay_01 (or your chosen name)
- Scene GOs:
  - NetworkManager + UnityTransport (+ start UI)
  - NetworkGameManager (+ optional NetworkPrefabsRegistrar)
  - SceneLoaderOnHost (if using bootstrap)
  - MatchManager (Gameplay scene)
  - Camera with camera scripts
  - CoinSpawner (+ spawn point transforms)
  - AISpawnerManager (+ spawn points)
  - DepositZone(s) for each team
  - Environment geometry set to Environment layer


## Final notes
This setup mirrors a server-authoritative loop: clients send input, the server simulates, and state replicates back. Keep writes to NetworkVariable values on the server only. When adding new networked behaviours, put them on a GameObject with a NetworkObject and initialize any networked state in `OnNetworkSpawn()`.

For future polish, consider:
- Adding a simple start menu to start Host/Client, show controller bindings, and join flows.
- Exposing more tunables via ScriptableObjects for easy tweaking without code changes.
- Adding player/enemy world-space health bars and coin counters.
