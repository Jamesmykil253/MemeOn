Root Selector
├─ Idle branch:
│   Condition: IsDamaged? 
│     Yes -> SetTarget(lastAttacker), Switch FSM to Aggro
│     No  -> Do Nothing (remain idle)
└─ Aggro branch (Sequence):
    Condition: Within Aggro Radius?
      Yes -> Check: Target in Attack Range?
             ├─ Yes -> Attack Target
             └─ No  -> MoveTo(TargetPosition)
      No  -> MoveTo(SpawnPosition), Switch FSM to Idle
