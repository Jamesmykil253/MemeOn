# Development Checklist — Updated Audit
**Project:** Meme Online Battle Arena (MOBA)
**Date:** 2025-08-16
**Engine/Editor:** Unity 6000.1.15f1
**Networking:** NGO 2.4.4 (server authoritative)
**Status:** Prototype

---

## ✅ Completed
- Core **movement system** (jump, double-jump, air control)
- **Server-authoritative damage** with friendly-fire enforcement
- **ProjectileServer / MeleeWeaponServer** updated with team logic
- **CrowdControlServer** (knockback, root, stun)
- **AIController** with FSM + Behavior Tree integration
- **AISpawnerManager** with respawn/death hook
- **Camera prototype** (Pokemon Unite-style with dynamic limited view)
- **Prefab pass** (scripts wired, AI prefabs registered with NetworkManager)
- **Compile errors resolved** — build compiles clean
- **Networking hooks** in combat, AI, CC

---

## ⚠️ In Progress
- Camera **dynamic viewport limits** (prototype working, needs polish)
- AI **state debug color feedback** (currently via material swap in network listener)
- Prefab **auditing** — ensure all prefab links are assigned (AI_StationarySentinel confirmed; verify others)
- **Inspector setup** reminders (Team components, Blackboard defaults)

---

## ⏳ Pending
- Animation event alignment for melee hit windows
- VFX/audio pooling system (budget: ≤ 1 KB/frame GC)
- Full **map prefab integration** (spawn points, deposit goals)
- Thermal/network stress test on mobile hardware
- Lobby/matchmaking (stub only)
- UI/UX polish (coin counter, deposit UI, results screen)
- Match overtime rule (first deposit wins) logic

---

## ❌ Not Yet Started
- Cosmetic systems (skins, meme cosmetics)
- Player progression/meta loop
- Store/economy integration
- Localization
- Analytics/telemetry

---

## Notes
- Ensure **AI_StationarySentinel prefab** is assigned in `AISpawnerManager` inspector.
- Verify **Team component** exists on all players and AI, synced server-side.
- Network tick: 20–30 Hz transforms, < 25 kB/s per client average.
- GC allocations monitored; pooling required for skills/VFX.
- Follow **performance budgets** from Executive Concept.

---
