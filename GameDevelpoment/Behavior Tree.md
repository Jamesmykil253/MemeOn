# Behavior Tree — Stationary Sentinel (v1.0, 2025-08-15)

Root Selector
├─ Idle branch:
│   If(IsDamaged) → SetTarget(lastAttacker) → FSM.Switch(Alerted)
│   else → IdleAction
└─ Engage branch (Sequence):
    Check Within Aggro Radius?
      yes → InAttackRange?
              yes → FSM.Switch(Attack type)
              no  → MoveTo(TargetPosition)
      no  → MoveTo(SpawnPosition) → FSM.Switch(Idle)
**Threat Modulation:** If target threat HIGH, run EvadeSequence before re‑engage.
