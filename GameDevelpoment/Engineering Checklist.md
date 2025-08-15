# Engineering Acceptance Matrix (v1.0, 2025-08-15)

## Infrastructure
- Unity 6000.1.15f1; packages pinned (Input 1.14.2, AI Nav 2.0.8, NGO 2.4.4).

## Movement & Controls
- Input maps defined; ground check reconciled server‑side; **double‑jump** flag resets on land.

## AI
- Idle‑until‑damaged; pursue‑until (leash OR timeout); attack hit windows align with animation; OffMeshLink traversal verified.

## Network
- Server‑auth damage; reconciliation tested at 50–150 ms RTT, 1–3% loss; minimal NetworkVariables (HP, coins).

## Performance
- Meets mobile budgets (draw calls, tris, VFX, GC); no thermal throttle in 10‑minute soak.

## Tests
- FSM transition unit tests; BT decision integration tests; NGO spawn/sync tests; packet loss & jitter harness.

## Pass/Fail
- PASS when all above are true in 3 consecutive 10‑minute internal playtests.
