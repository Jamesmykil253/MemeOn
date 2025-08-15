# Executive Concept — Master-Reviewed (v1.0, 2025-08-15)

**Project:** Meme Online Battle Arena (MOBA)  
**Editor:** Unity 6000.1.15f1  
**Template/Render:** 3D Mobile Template 6.1.2 (URP Mobile)  
**Networking:** Netcode for GameObjects (NGO) 2.4.4 (server authoritative)  
**Physics:** GameObject/PhysX (no DOTS/Unity Physics for prototype)

## High-Concept
A dark/ironic comedic **3D platformer MOBA** with short, high‑tempo rounds (prototype focus: **5‑minute 3v3**) emphasizing tight platforming (jump, **double‑jump**), objective play via **coin pickups and deposit scoring**, and satirical meme characters (Elom Nusk, Doge Dog, Tronald Dump).

## Core Pillars
1. **Mechanical clarity** — crisp ground/air control, consistent hit windows, short readable cooldowns.
2. **Competitive parity** — symmetric map, asymmetric kits via data‑driven stats.
3. **Comedy without noise** — humor never compromises readability/telegraphing.
4. **Server authoritative + reconciliation** — server is the source of truth; client prediction only for local player.

## Prototype Scope
- **Mode:** 3v3, **5:00** timer, sudden‑death overtime if tied (**first deposit** wins).
- **Loop:** spawn → fight → coins drop/spawn → collect → **deposit** → repeat → results.
- **AI:** Stationary Sentinel: idle‑until‑damaged, pursue/attack with leash and give‑up timeout.

## Non‑Functional Baseline
- **Editor:** 6000.1.15f1; **URP Mobile** quality tiers.
- **Packages:** Input System 1.14.2; AI Navigation 2.0.8; NGO 2.4.4.
- **Target devices:** mid‑tier Android/iOS; 60 FPS target (see budgets).

## Performance Budgets (Mobile)
- CPU: ≤ 8 ms/frame client; AI tick 20–30 Hz; physics 60 Hz.
- GPU: ≤ 70 draw calls; ≤ 400k tris; ≤ 8 active particle systems.
- Memory/GC: < 1 KB/frame GC; pooled VFX/audio; UI atlases.
- Network: 20–30 Hz transform interpolation; < 25 kB/s per client average.
- Thermals: no throttling during a 10‑minute 3v3 session.

## Success Criteria
- Matches complete without >5% disconnects; server remains authoritative.
- Platforming/double‑jump consistent under latency; deposit loop error‑free.
- AI behaves per spec ≥95% cases; animation events align with hit windows.
