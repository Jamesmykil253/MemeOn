# AI coding guidance for MemeOn (Unity + Netcode for GameObjects)

- Project: Unity C# game using Netcode for GameObjects (NGO) and the Input System. Scripts are organized by feature under `Assets/Scripts/*` (e.g., `Players/`, `Combat/`, `Network/`).

## Architecture and patterns
- Server-authoritative gameplay: clients read input; the server simulates movement and combat.
- Networked scripts inherit from `NetworkBehaviour` and must live on a `NetworkObject`.
- Networked state uses `NetworkVariable<T>` with server write permission. Mutate only when `IsServer`.
  - Example: `Combat/HealthNetwork.cs` sets `CurrentHealth` as `NetworkVariable<int>(0, Read=Everyone, Write=Server)` and writes only on server.
- Initialize networked state in `OnNetworkSpawn()` (not `Awake/Start`) to avoid pre-spawn warnings. See `HealthNetwork.OnNetworkSpawn`.
- Prefer runtime guards (`IsServer`, `IsOwner`) over Mirror-style attributes; do not use `[Server]`/`[Client]` attributes.
- Remote calls: use `[ServerRpc]` for client→server input. Name methods with the `...ServerRpc` suffix (NGO convention).

## Movement and input
- `Players/PlayerMovement.cs` demonstrates authoritative `CharacterController` movement:
  - Owner reads `InputActionReference` each `FixedUpdate` and calls `SubmitInputServerRpc(Vector2 move, float dt)`.
  - Server stores last input, applies gravity/rotation, and calls `_cc.Move(...)`.
  - External modifiers (e.g., slows) change a server-held multiplier via a server-guarded method.
- Owner-only input wiring is enabled in `OnNetworkSpawn` when `IsOwner`, and disabled in `OnDisable`.
- `Players/PlayerController.cs` shows the same input→ServerRpc pattern and invokes combat RPCs from movement logic when needed.

## Combat/health
- `Combat/HealthNetwork.cs`: server-owned health with `OnDeath` event when health reaches 0.
  - `Heal`/`Damage` are public but no-op unless `IsServer`.
  - Clamp values; never write to `NetworkVariable` from clients.
- `Combat/Damageable.cs` defines `IDamageable` for damage application across systems.

## Networking setup
- Ensure critical prefabs are registered with `NetworkManager`:
  - At runtime via `Network/NetworkPrefabsRegistrar.RegisterAll()` with a list of prefabs, or
  - Via assets such as `Assets/DefaultNetworkPrefabs.asset` in the editor.
- `Network/NetworkGameManager.cs` can auto-register at startup if a registrar is assigned.

## Conventions and gotchas
- Always add `[RequireComponent(typeof(NetworkObject))]` to networked behaviours and other required components (e.g., `CharacterController`).
- Do not set `NetworkVariable` values before `OnNetworkSpawn`.
- Input System: use `InputActionReference` or generated actions in `Assets/InputSystem_Actions.cs` and read input only on the owner.
- Rotation is applied server-side using `Quaternion.RotateTowards` toward movement direction to keep clients in sync.

## Workflow (Editor)
- Open in Unity, enter Play Mode, then start Host/Server/Client via your scene flow. Verify prefabs are in the NetworkManager configuration.
- Use `Debug.Log` for diagnostics. No test suite is present in this repo.

## Key entry points
- Networking: `Assets/Scripts/Network/*`
- Players: `Assets/Scripts/Players/PlayerMovement.cs`, `PlayerController.cs`
- Combat: `Assets/Scripts/Combat/HealthNetwork.cs`, `Damageable.cs`
- High-level design: `GameDevelpoment/*.md`
