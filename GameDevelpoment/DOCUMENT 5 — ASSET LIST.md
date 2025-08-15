========================================
DOCUMENT 5 — ASSET LIST
========================================

TITLE:
Meme Online Battle Arena — Prototype 5-Minute 3v3 Mode
Asset Inventory & Classification

VERSION:
v0.1 — Prototype Asset Specification

AUTHORSHIP:
Art Director: [Redacted]
Technical Artist: [Redacted]
Production Manager: [Redacted]
Date: [YYYY-MM-DD]

REVISION HISTORY:
0.1 — Initial placeholder-oriented asset catalog.

----------------------------------------
1. PURPOSE
----------------------------------------
This document enumerates all assets required for the prototype, 
classifies them by type (art, audio, animation, UI, scriptable data), 
and defines dependencies between systems and assets. 
The intent is to ensure development proceeds without asset-blocking, 
by enabling placeholders to be used in early stages.

----------------------------------------
2. ASSET CLASSIFICATION LEGEND
----------------------------------------
[3D-MESH]     — Static or skinned mesh asset
[TEXTURE]     — 2D texture map (albedo, normal, etc.)
[MATERIAL]    — Unity material asset referencing textures/shaders
[ANIMATION]   — Unity animation clip
[AUDIO]       — Sound file asset
[PREFAB]      — Prefab containing mesh, components, logic
[UI]          — Unity UI element/prefab
[DATA]        — ScriptableObject or config file
[FX]          — Particle system or VFX Graph asset
[PLACEHOLDER] — Temporary stand-in asset

----------------------------------------
3. CHARACTER ASSETS
----------------------------------------
Elom Nusk:
- [3D-MESH] Humanoid mesh, rigged (PLACEHOLDER initially)
- [TEXTURE] Diffuse/albedo, normal map (PLACEHOLDER initially)
- [ANIMATION] Idle, walk, run, jump, double jump, attack light, attack heavy, hurt, death
- [AUDIO] Voice lines (PLACEHOLDER), attack SFX, hurt SFX
- [PREFAB] PlayerCharacter prefab with PlayerController component
- [DATA] CharacterStats SO (health, speed, jump height, abilities)

Doge Dog:
- Same asset breakdown as Elom Nusk, different model/animations

Tronald Dump:
- Same asset breakdown, no double-jump animation

----------------------------------------
4. AI ENEMY ASSETS (Stationary Sentinel)
----------------------------------------
- [3D-MESH] Humanoid/creature mesh (PLACEHOLDER)
- [TEXTURE] Basic color and normal maps (PLACEHOLDER)
- [ANIMATION] Idle, walk, attack, hurt, death
- [AUDIO] Attack SFX, hurt SFX
- [PREFAB] AIEnemy prefab with AIController (FSM + BT)
- [DATA] AIConfig SO (aggro radius, give-up timeout, attack range)

----------------------------------------
5. ENVIRONMENT ASSETS
----------------------------------------
Map: Meme Coliseum (prototype)
- [3D-MESH] Ground plane, platforms, ramps, decorative elements
- [TEXTURE] Platform textures, ground texture, wall texture
- [MATERIAL] Arena floor, platform material
- [PREFAB] Platform prefabs (varied heights)
- [FX] Ambient particle effects (PLACEHOLDER for atmosphere)
- [DATA] NavMeshSurface bake asset

----------------------------------------
6. INTERACTIVE OBJECTS
----------------------------------------
Coins:
- [3D-MESH] Coin model (PLACEHOLDER)
- [TEXTURE] Gold material (PLACEHOLDER)
- [ANIMATION] Spin animation
- [AUDIO] Pickup SFX (PLACEHOLDER)
- [PREFAB] Coin prefab with Coin script (NetworkObject)
- [FX] Coin sparkle particles

Deposit Zones:
- [3D-MESH] Pedestal or zone indicator (PLACEHOLDER)
- [MATERIAL] Team-colored indicator material
- [FX] Glow effect
- [PREFAB] DepositZone prefab with scoring logic

----------------------------------------
7. UI ASSETS
----------------------------------------
HUD:
- [UI] Health bar prefab
- [UI] Coin counter prefab
- [UI] Timer display
- [UI] Ability cooldown indicators
- [TEXTURE] UI icons for abilities
- [DATA] UI layout config

Menus:
- [UI] Lobby screen
- [UI] Character select
- [UI] Match results screen

----------------------------------------
8. AUDIO & VFX
----------------------------------------
Audio:
- [AUDIO] Jump SFX
- [AUDIO] Attack SFX
- [AUDIO] Coin pickup SFX
- [AUDIO] Hit confirmation SFX

VFX:
- [FX] Hit impact effect
- [FX] Respawn poof
- [FX] Death effect
- [FX] Team-colored aura (deposit zones)

----------------------------------------
9. SCRIPTABLE DATA ASSETS
----------------------------------------
- CharacterStats SO (per character)
- AIConfig SO (per enemy type)
- GameModeConfig SO (match duration, coin values)
- AbilityConfig SO (cooldowns, ranges, effects)

----------------------------------------
10. DEPENDENCIES
----------------------------------------
- Player prefabs require CharacterStats and Animation assets
- AI prefabs require AIConfig and NavMesh baked environment
- Coin prefabs require Coin script and network registration
- Deposit zone requires scoring script and FX

----------------------------------------
11. PLACEHOLDER STRATEGY
----------------------------------------
- All gameplay-affecting assets (meshes, animations, audio) use placeholders until mechanics are verified.
- Final asset pass deferred to post-prototype phase.
