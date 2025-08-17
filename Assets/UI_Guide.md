# User Interface and Gameplay Systems Guide

This document outlines how to set up the in‑game UI, coin spawning, deposit zones and match management for the **Meme Arena** prototype.  The goal is to provide clear inspector guidelines so you can configure the prefabs and scripts without writing any additional code.

## 1  Game overview and design context

The core gameplay loop is a **3‑v‑3**, five‑minute match where players fight AI sentinels and each other, collect coins and deposit them at their team’s base to score points【425351184769408†L53-L57】.  Coins are worth one point each and can be dropped or banked; deposit actions are limited by a **0.5 s cooldown** to prevent spamming【425351184769408†L118-L124】.  If a player dies, all collected coins are dropped and can be picked up by others【425351184769408†L53-L57】.  AI enemies follow a deterministic finite‑state machine with states *Idle*, *Alert*, *Pursue*, *MeleeAttack*, *RangedAttack*, *Evade*, *Stunned*, *ReturnToSpawn* and *Dead/Respawn*【425351184769408†L46-L139】.  Match length, respawn delay and deposit cooldown are defined in `ProjectConstants.Match`.

## 2  Coin spawning and pickup

### 2.1  Coin spawner (`CoinSpawner`)

The `CoinSpawner` component spawns coin prefabs at a list of spawn points and respawns them after a delay when collected.  It should be added to a scene object and configured as follows:

1. **Prefab** – Assign your coin prefab to **Coin Prefab**.  The prefab must have a `NetworkObject` and the `Coin` component (see below).
2. **Spawn Points** – Populate the **Spawn Points** array with empty child transforms placed where coins should appear.  You can set these positions manually in the scene.
3. **Respawn Delay Seconds** – How long to wait before respawning a collected coin (default 10 s).  Adjust based on desired coin availability.
4. **Network** – Ensure the spawner object has a `NetworkObject` component if you plan to add it dynamically.  Only the server runs the spawning logic; clients receive spawned coins via NGO replication.

### 2.2  Coin prefab (`Coin`)

Create a new **Coin** prefab and configure it with the following components and settings:

1. **NetworkObject** – Required for NGO replication.  Register this prefab in **NetworkManager → Network Prefabs**.
2. **Collider (trigger)** – Add a capsule or sphere collider and tick **Is Trigger**.  Coins do not use a rigidbody.
3. **Coin** script – Attach the `Coin` script from `Assets/Scripts/Items/Coin.cs`.  Set **Rotation Speed** to control how quickly the coin spins on clients (e.g. 180 °/s).  Do not assign the **Spawner** or **Spawn Point** fields; they are set automatically when the spawner instantiates the coin.
4. **Tag** – Set the tag to `Projectile` or define a specific tag (e.g. `Coin`) if desired, though the script currently checks only the player tag.

At runtime, the coin rotates around the Y axis for visual feedback.  When a player enters the trigger and the server detects the collision, the coin awards a single coin to the player’s `PlayerInventory` component, informs its spawner to schedule a respawn, and despawns itself.

### 2.3  Player inventory (`PlayerInventory`)

Players need a `PlayerInventory` component attached to their root GameObject (alongside `NetworkObject`, `CharacterController`, etc.).  This component stores the number of coins the player has collected as a `NetworkVariable<int>` so clients can show it in the UI.  Only the server modifies the coin count via `AddCoin()` and `WithdrawCoins()`.

To display coin count on the HUD, create a UI Text or TextMeshPro element and write a simple script to read `PlayerInventory.CoinCount` on the local player and update the text when it changes.

## 3  Deposit zones

Deposit zones are team‑specific triggers where players bank their coins to score points.  To create a deposit zone:

1. **GameObject** – Create an empty GameObject at the desired base location.  Add a **Box Collider** or **Cylinder Collider** component, set **Is Trigger** to true, and resize the collider to define the deposit area.
2. **NetworkObject** – Add a `NetworkObject` component so the deposit zone is replicated across the network.
3. **DepositZone** script – Attach `DepositZone.cs` from `Assets/Scripts/Game/DepositZone.cs`.  In the Inspector:
   * **Team Id** – Set to `0` for Team A and `1` for Team B.  Only players with a matching `TeamId` component can deposit at this zone.
   * **Deposit Cooldown** – Use the default `ProjectConstants.Match.DepositCooldown` (0.5 s) or override per zone.  Players cannot deposit again until this time has elapsed since their last deposit.
4. **Visuals** – (Optional) Add a model, light or particle effect under the zone to indicate where to deposit.  You can also add UI hints that appear when the player enters the trigger.

When a player enters and stays inside the trigger, the deposit zone checks if their `TeamId.team` matches the zone’s team and if they have coins.  If so, and the cooldown has expired, it withdraws all coins from their `PlayerInventory`, adds the coins to the match score via `MatchManager`, and records the time.  Players must leave and re‑enter the trigger or wait for the cooldown to deposit again.

## 4  Match manager

The `MatchManager` script manages team scores and the match timer.  Add it to a dedicated GameObject (e.g. `GameManager`) and ensure it has a `NetworkObject` component.  The script will:

* Initialise the match timer from `ProjectConstants.Match.MatchLength` (5 minutes by default) on the server.
* Decrease the timer every frame and call `EndMatch()` when it reaches zero.  You can extend `EndMatch()` to show a results screen or reset the scene.
* Expose `AddScore(int teamId, int amount)` for deposit zones and other scoring events.
* Provide `GetScore(int teamId)` and `GetTimeRemaining()` for UI scripts to display scores and the countdown.

The `MatchManager` uses network variables to replicate scores to clients; the timer is not replicated but can be read via RPCs or polled on the client to update the HUD.

## 5  UI setup

Use Unity’s UI system (UGUI or TextMeshPro) to create the HUD.  A basic layout might include:

1. **Health bar** – Use an `Image` with `fillAmount` tied to the player’s `NetworkHealth.GetCurrentHealth() / maxHealth`.  Place it near the top left.  Write a small script (e.g. `HealthBarUI`) that subscribes to the local player’s health events or polls the value each frame.
2. **Coin counter** – Add a `Text` or `TMP_Text` element showing the current coin count.  Write a `CoinCounterUI` script that references the local `PlayerInventory` component and updates the text when the `NetworkVariable<int>` changes.
3. **Match timer** – Place a timer label at the top centre.  A `MatchTimerUI` script should call `MatchManager.Instance.GetTimeRemaining()` and format it as minutes and seconds.
4. **Scoreboard** – Show the team scores (e.g. `Team A: 0   Team B: 0`) using a `Text` or group of elements.  Update these via a `ScoreboardUI` script calling `MatchManager.GetScore(0/1)` on clients.
5. **Ability icons & cooldowns** – (Optional) Add images for each ability with overlay text or radial fill to show cooldowns.  Hook them into your ability system’s cooldown data.

When implementing UI scripts, remember to reference the appropriate components via the inspector and ensure they read data from network variables rather than local fields.  All UI updates should happen on the client side; do not attempt to modify network variables from the UI.

## 6  Inspector checklist summary

| System | GameObject & Component Setup | Critical Inspector Fields |
|-------|--------------------------------|-------------------------|
| **Coin** | Prefab with `NetworkObject`, `Collider (Is Trigger)`, `Coin` | `rotationSpeed` for spin, no need to assign spawner or spawn point (set automatically) |
| **CoinSpawner** | Scene object with `CoinSpawner` | `coinPrefab`, `spawnPoints[]`, `respawnDelaySeconds` |
| **PlayerInventory** | Attached to player root with `NetworkObject` | No inspector fields; used for coin count |
| **DepositZone** | GameObject with `NetworkObject`, `Collider (Is Trigger)`, `DepositZone` | `teamId`, `depositCooldown` (default 0.5 s) |
| **MatchManager** | Scene object with `NetworkObject`, `MatchManager` | No inspector fields; timer uses `ProjectConstants.Match.MatchLength` |

Following this guide will allow you to set up a functional coin economy, deposit system, and scoreboard in line with the design document’s rules【425351184769408†L53-L57】【425351184769408†L118-L124】.  You can expand these systems further—for example, adding UI effects, sound cues or visual feedback—but the scripts provided here implement the core mechanics in a deterministic, server‑authoritative manner.