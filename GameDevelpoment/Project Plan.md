# Project Plan — Prototype 5‑Minute 3v3 (v1.0, 2025-08-15)

## Milestone 1 — Core Framework (Weeks 1–4)
- Project setup; Input System; Rigidbody controller (jump/double‑jump);  
- Server‑auth movement + reconciliation; NavMesh bake; FSM/BT skeleton.
**Deliverables:** Networked movement demo; AI test agent moves via NavMesh; physics/network prediction validated.

## Milestone 2 — Core Gameplay Loop (Weeks 5–8)
- Melee/ranged combat (server hit validation); coin spawn/drop/collect;  
- Deposit scoring with **0.5s/coin** throttle; full Stationary Sentinel spec;  
- MatchManager timer/score/win; basic HUD.
**Deliverables:** End‑to‑end playable match (bots + players).

## Milestone 3 — Content & Mobile Optimization (Weeks 9–12)
- Three characters with data‑driven kits; placeholder animations/SFX/VFX;  
- Performance profiling, **mobile budgets** enforcement; thermals & jitter QA; bug fixes.
**Deliverables:** Playable prototype meeting 60 FPS target on mid‑tier devices; ready for closed playtest.

## Risks & Mitigation
- NavMesh vs physics conflicts → agent for path, physics for motion; path refresh 0.25s.  
- Reconciliation artifacts → increase correction rate; cap client prediction window.  
- Network load from AI → throttle state sync to animation changes only.

## Exit Criteria
- Start‑to‑finish matches with ≤5% disconnects; AI adheres to spec ≥95%;  
- Overtime behaves correctly (first **deposit**); perf/thermal budgets met.
