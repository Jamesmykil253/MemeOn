# Asset List — Prototype (Mobile-Optimized, v1.0, 2025-08-15)

## Placeholder First
- **Characters (3):** rigged humanoids (≤12k tris each), 1k atlas, single material.
- **AI Enemy:** low‑poly mesh (≤8k tris), shared 1k atlas.
- **Environment:** modular platforms; combined atlases; baked light if needed.
- **Coins:** simple mesh + spin anim; shared gold material.
- **VFX:** lightweight particles with soft budget (≤8 active systems).
- **Audio:** jump/attack/coin/hit (short, pooled).
- **UI:** timer, scores, coins; single atlas.

## Animations (per character/enemy placeholder)
Idle, Walk, Run, Jump, DoubleJump, LightAttack, HeavyAttack, Hurt, Death, Respawn.

## Scriptable Data
- `CharacterStats` (movement, health, abilities, cooldowns)
- `AIConfig` (aggro radius, giveUpTimeout, attack ranges)
- `GameModeConfig` (match duration, coin values)
- `AbilityConfig` (cooldowns, ranges, effects)

## Dependencies
- Player/Enemy prefabs require their Stats SO + Animator clips.
- AI requires NavMesh baked scene + AIConfig.
- Coins require NetworkObject + Coin script.
- Deposit zones require scoring logic + FX.
