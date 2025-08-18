# Engineering Audit — MemeOn (Unity + NGO)
Date: 2025-08-18
Scope: Runtime code (C#), Editor tooling, networking architecture, assets/prefabs layout, build/playability.

## Executive summary
Strengths
- Server-authoritative gameplay with NGO patterns is in place (owner input → ServerRpc → server simulation).
- Solid modular UI (HUD widgets + binder), robust camera bootstrap, and runtime validator.
- Editor automation is strong: prefab builders, layout auditor, deprecated scanner, and playability auditor.

Key risks / debt
- Legacy classes still present (PlayerController, EnemyWorldUI, AIBehaviorTree) can re-surface via old prefabs/scenes.
- No assembly definition (.asmdef) boundaries; slow domain reloads and no analyzer scoping.
- Minimal guardrails around Build Settings and network prefab registration at build time.
- Limited tests and CI; regressions possible despite good editor tooling.
- Reflection-heavy editor utilities; useful, but should be constrained and logged.

Top priorities (2-week horizon)
1) Remove legacy components from the repo (post-migration) and add a safety check to block usage.
2) Introduce asmdefs (Runtime/Editor per feature) with warnings-as-errors + analyzers.
3) Add minimal PlayMode tests (spawn, RPC path) and GitHub Actions Unity Builder CI.
4) Harden NetworkManager config checks (transport, player prefab) and Build Settings validation.
5) Optimize hot paths: event-driven local-player discovery, pooling verification for projectiles, and network send-rate tuning.

---

## Architecture & networking (NGO)
- Pattern: Owner reads input, calls ServerRpc; server simulates movement/rotation (e.g., `PlayerMovement`), projectiles driven from server (`ProjectileServer`), health via `NetworkHealth` on server → OK.
- NetworkVariables: Ensure write-permission is Server-only and mutated only on `IsServer` paths (audit pass spot-check).
- Initialization: Prefer `OnNetworkSpawn()` for networked state; ensure no writes in `Awake/Start`.
- RPC naming convention `...ServerRpc` followed → OK.
- Recommendations
  - Add server-side validation for any client-exposed RPCs (e.g., fire rate limits, distance checks for melee) to mitigate cheats.
  - Centralize tick-rate and interpolation settings (NetworkTransform/replication) to balance bandwidth vs smoothness.
  - Add a single “NetworkPolicy” scriptable (rates, channels, QoS), referenced by `NetworkGameManager`.

## Input & movement
- Uses Input System and owner-gated input submission → Good. Movement and rotation handled server-side with `CharacterController`.
- Recommendations
  - Ensure `FixedUpdate` cadence is consistent with NGO tick; optionally drive from a single simulation clock.
  - Add deadzone clamping and max-speed authority checks server-side.

## Combat/health
- `NetworkHealth` appears server-owned with events; UI widgets consume it → Good.
- `BoostedAttackTracker` sequence (melee ×3 → boosted shot) implemented → Good.
- Recommendations
  - Add damage provenance validation on server (team/friendly-fire rules; rate-limit).
  - Ensure `Damageable` only processes on server and ignores client writes.

## UI & camera
- Modular HUD (HealthBarUI, XPBarUI, LevelUI, CoinCounterUI, BoostedAttackUI) + `PlayerHUDBinder`.
- CameraBootstrap + UniteCameraController; validator ensures single AudioListener → Good.
- Recommendations
  - Convert remaining HUD text to TMP consistently (minor polish).
  - Binder: switch to event-driven local-player discovery (subscribe to NGO spawn/connect callbacks) to avoid polling.

## AI
- Canonical FSM exists; BehaviorTree present; legacy `AIBehaviorTree` is a placeholder.
- Recommendations
  - Remove `AIBehaviorTree` (legacy) and ensure `BehaviorTree`/FSM coexistence is intentional; document the selection criteria.
  - Add simple perception tests for LOS/aggro.

## Editor tooling
- Builders (UI/core prefabs), PrefabLayoutAuditor, DeprecatedScanner, ProjectPlayabilityAuditor (modern Find APIs).
- Recommendations
  - Add a unified “Project Tools” window to run flows with dry-run mode; output to a dedicated report asset.
  - Constrain reflection usage to one helper and log types/methods invoked.
  - For scanners, never unload last scene; already addressed. Also add a per-change Undo group and summary.

## Assets, prefabs, scenes
- Canonical: `Resources/NetworkPrefabs` for Player/Enemy/Projectile/MeleeHitbox; `Prefabs/UI/UI.prefab`.
- Playability auditor instantiates NetworkManager, transport, SceneLoaderOnHost, camera, and HUD if missing → Good.
- Recommendations
  - Build Settings guard: ensure Bootstrap + at least one gameplay scene are added for builds (editor menu + CI check).
  - Prefer Addressables long-term for content; keep resources for NGO prefab discovery in dev.

## Performance & GC
- Potential hotspots: polling for local player in HUD binder; heavy reflection scans in editor utilities; per-frame allocations from LINQ.
- Recommendations
  - Replace binder polling with NGO events; cache queries and avoid LINQ in hot paths.
  - Verify projectile pooling path (ObjectPool usage) and lifetime cleanup.
  - Profile in PlayMode with 16+ clients (editor simulations or headless) to tune send rates and transforms.

## Code quality & safety
- Namespaces consistent under `MemeArena.*`.
- Recommendations
  - Add asmdefs: `MemeArena.Runtime`, `MemeArena.Editor`, plus feature splits (AI, Players, Combat, UI, Network) where helpful.
  - Enable analyzers (Unity Analyzers, Roslyn CA) and treat warnings as errors in CI for Editor assembly.
  - Introduce nullability context and defensive guards in public APIs.

## Testing & CI
- No tests/CI noted.
- Recommendations
  - Add PlayMode tests: (1) NetworkManager + player spawn; (2) owner input → server moves; (3) melee-hit increments boosted tracker; (4) HUD binder wires after spawn.
  - GitHub Actions: use GameCI/Unity Builder to run -batchmode -nographics builds and test suites; cache Library.
  - Add a lightweight linter step (dotnet-format or StyleCop for Editor-only assembly).

## Security/cheat surface
- Ensure all state mutations happen on server; validate incoming ServerRpcs.
- Rate-limit inputs (fire/melee) server-side; ignore client timestamps.
- Don’t trust client-side hit detection.

## Documentation
- Docs are rich in `GameDevelpoment/`. Add:
  - Quick Start (press Play flow using auditors),
  - Prefab & Scene layout reference,
  - Networking policy overview and tuning guide.

---

## Specific cleanup targets
- Remove (after verifying no scene/prefab references):
  - `Assets/Scripts/Players/PlayerController.cs`
  - `Assets/Scripts/UI/EnemyWorldUI.cs`
  - `Assets/Scripts/AI/BT/AIBehaviorTree.cs`
- Keep DeprecatedScanner migration active for 1-2 more iterations, then drop aliases.

## Proposed asmdef layout (minimal)
- Assets/Scripts/MemeArena.Runtime.asmdef — references Unity essentials + NGO + InputSystem.
- Assets/Editor/MemeArena.Editor.asmdef — Editor only; references Runtime assembly.
- Optional feature splits: MemeArena.Players, MemeArena.Combat, MemeArena.AI, MemeArena.UI, MemeArena.Network, MemeArena.Utilities.

## Next steps (actionable)
1) Add asmdefs (Runtime + Editor). Turn on analyzers; fix any fallout.
2) Remove legacy classes; run DeprecatedScanner + PlayabilityAuditor; commit.
3) Replace HUD binder polling with event-driven player discovery.
4) Add Build Settings auditor and CI (Unity Builder). Include quick PlayMode tests.
5) Document Quick Start + Prefab/Scene layout and Networking policy.

## Quality gates (current session)
- Build: Editor scripts compile; modern Find APIs used — PASS (file-level check).
- Lint/Typecheck: No analyzers configured — Deferred.
- Unit tests: None — Deferred.
- Smoke test: Use Tools → MemeArena → Audit Playability (Active Scene) then press Play — Recommended.
