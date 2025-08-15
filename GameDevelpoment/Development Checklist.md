# Development Checklist — Sequential Plan (v1.0, 2025-08-15)

## 1. Setup
[ ] Unity 6000.1.15f1 project (URP Mobile).  
[ ] Packages: Input System 1.14.2; AI Navigation 2.0.8; NGO 2.4.4.  
[ ] Project settings: Fixed Timestep 0.0166667; Max Allowed Timestep 0.05.  
[ ] Git/LFS enabled.

## 2. Input
[ ] Create `PlayerControls.inputactions` with Move/Jump/AttackPrimary/AttackSecondary/Interact.  
[ ] Generate C# class; wire PlayerInput to events.

## 3. Player Controller
[ ] Rigidbody movement; ground checks; jump + **double‑jump** flag; air control clamp.  
[ ] Animation events for melee hit windows.

## 4. Networking
[ ] NetworkManager + UTP; NetworkPlayer prefab with NetworkObject/NetworkTransform.  
[ ] Server authoritative movement + reconciliation.

## 5. AI Base
[ ] AIController.cs with FSM (Idle, Alerted, Pursue, Melee, Ranged, **Evade**, Stunned, Return, Dead).  
[ ] Custom BT module; Blackboard with normalized keys.  
[ ] NavMesh bake + OffMeshLinks.

## 6. Economy
[ ] Coin prefab (NetworkObject) + server spawn manager.  
[ ] On‑death coin drops.  
[ ] DepositZone prefab with **0.5s/coin** throttle (server enforced).

## 7. Combat
[ ] Melee via OverlapSphere on anim window; ranged via Raycast/projectile.  
[ ] Server applies damage; clients receive UI events.

## 8. Match
[ ] MatchManager: timer, scores, overtime (first **deposit**), respawns.

## 9. UI
[ ] HUD: health, coins, team score, timer, cooldowns.  
[ ] Results screen.

## 10. Placeholders
[ ] Import low‑poly meshes, simple materials, pooled SFX/VFX, single UI atlas.

## 11. Testing
[ ] FSM unit tests; BT integration with mocks.  
[ ] Latency 50–150 ms, 1–3% loss; AI state sync throttled.  
[ ] Mobile thermals 10‑min 3v3; perf budgets met.

## Exit
[ ] All systems integrated; matches stable end‑to‑end; AI ≥95% spec‑adherence.
