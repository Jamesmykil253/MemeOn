========================================
DOCUMENT 2 — GAME DESIGN DOCUMENT (GDD)
========================================

TITLE:
Meme Online Battle Arena (MOBA) — Prototype 5-Minute 3v3 Mode

VERSION:
v0.1 — Pre-production GDD (PHD-level engineering edition)

AUTHORSHIP:
Design Lead: [Redacted]
Engineering Lead: [Redacted]
Systems Designer: [Redacted]
AI Architect: [Redacted]
Date: [YYYY-MM-DD]

REVISION HISTORY:
0.1 — Initial AAA studio-style engineering-oriented design draft.

----------------------------------------
1. GAME OVERVIEW
----------------------------------------
Meme Online Battle Arena is a hybrid 3D platformer MOBA with satirical character archetypes,
fast-paced PvP combat, and short-duration matches. This GDD covers the prototype scope:
the 5-minute 3v3 mode on a single symmetrical map.

----------------------------------------
2. CORE GAMEPLAY PILLARS
----------------------------------------
• Platforming Precision:
  Jumping and double-jumping are core navigational tools, fully integrated into combat 
  and positional strategy.
  Vertical 3D arenas emphasize Z-forward traversal with discrete Y-layer elevation and air mobility.

• Objective-Driven Combat:
  Players must balance aggression with economy — defeating enemies yields coins, but 
  depositing them in a scoring zone is the win condition.

• Comedic Layering:
  The satire complements competitive play without obstructing UI clarity or animation telegraphing.

• AI Stationary Sentinel Model:
  AI-controlled enemies remain in place until attacked, then pursue with constraints.

----------------------------------------
3. GAME MODES (Prototype Scope)
----------------------------------------
• 3v3, 5 minutes — single map
• Respawn timer: 5 seconds
• Coin value: 1 point per coin deposited
• Sudden death: If tied at timer end, instant-respawn overtime until first score.

----------------------------------------
4. MAP DESIGN (Prototype Map)
----------------------------------------
Map Name: Meme Coliseum (working title)
• Size: ~60m x 60m arena with 3 vertical layers.
• Symmetry: mirrored left/right for competitive balance.
• Coin Spawn Nodes: 3 fixed positions, each with 10-second respawn timer for coins.
• Deposits: One per team, positioned symmetrically.
• Obstacles: Jump gaps requiring single or double jump to traverse.
• NavMesh Baking: OffMeshLinks for jump gaps; separate area cost for elevated zones.

----------------------------------------
5. CHARACTER DESIGN (Prototype Roster)
----------------------------------------
**Elom Nusk**
Role: Balanced ranged fighter
- Health: 100
- Speed: 6 m/s
- Jump Height: 2.5 m (double jump enabled)
- Abilities: Dash (short burst), Energy Shield (damage reduction)

**Doge Dog**
Role: Agile melee skirmisher
- Health: 80
- Speed: 7.5 m/s
- Jump Height: 2.7 m (double jump enabled with shorter cooldown)
- Abilities: Bark Stun (AoE), Sprint Boost

**Tronald Dump**
Role: Tank / bruiser
- Health: 140
- Speed: 5 m/s
- Jump Height: 2.2 m (double jump disabled)
- Abilities: Knockback Melee, Area Taunt

----------------------------------------
6. PLAYER MOVEMENT MECHANICS
----------------------------------------
• Ground movement: acceleration 35 m/s², friction 5 m/s².
• Jump: triggered via Input System “Jump” action, vertical velocity impulse applied.
• Double jump: available if `doubleJumpAvailable` flag is true; resets on ground contact.
• Air control: capped at 60% of ground acceleration.
• Camera control (alive): Holding the Pan Modifier (default: Right Mouse) while using Look allows camera panning without detaching from the player. Horizontal look pans along camera-right (XZ only), vertical look pans along world Y. Pan radius clamps to ~6m and recenters when released.
• Camera control (dead): On death the camera detaches into free-pan mode. Move.x controls horizontal panning along camera-right; Move.y maps to world Y (up/down). Scroll adjusts zoom. Camera continues to look toward the last target position for context.

----------------------------------------
7. COMBAT SYSTEM
----------------------------------------
• Melee attacks: sphere overlap check at animation hit frame; damage applied server-side.
• Ranged attacks: projectile simulation using Unity Physics rigidbodies; collision resolved on server.
• Cooldowns: stored per ability; updated on server, predicted on client.
• Damage feedback: screen shake + VFX flash on hit.

----------------------------------------
8. ECONOMY SYSTEM
----------------------------------------
• Coin spawn: server-controlled, deterministic spawn timers.
• Drop on death: all carried coins drop at death position.
• Deposit zone: triggers server RPC to increment score and remove coins from inventory.
• Anti-exploit: deposit cooldown of 0.5s per coin to avoid network flooding.

----------------------------------------
9. AI ENEMY DESIGN (Stationary Sentinel Spec)
----------------------------------------
Behavioral Summary:
- Idle at spawn until receiving damage.
- On damage: set `target = attacker`, enter Pursue state.
- Pursue until:
  a) Target leaves `aggroRadius` (configurable per enemy archetype)
  b) No successful hit on target within `giveUpTimeout` seconds.
- If either condition met: Return to spawn, reset to Idle.

Technical AI Implementation:
- FSM controls low-level locomotion and combat.
- BT governs tactical decisions and target prioritization.
- Aggro radius and give-up timers stored in blackboard and synced to FSM.

----------------------------------------
10. GAME FLOW (MATCH LOOP)
----------------------------------------
1. Pre-match: Lobby → character selection → load arena.
2. Match start: countdown → players spawn in bases.
3. Active play: movement, combat, coin collection, deposits.
4. Match end: scoreboard → post-match rewards (cosmetic).
5. Loop back to lobby.

----------------------------------------
11. USER INTERFACE (Prototype)
----------------------------------------
HUD Elements:
- Health bar (player + target)
- Coin counter (carried)
- Timer
- Team scores
- Ability cooldown icons

----------------------------------------
12. AUDIO / VFX PLACEHOLDERS
----------------------------------------
• Animation placeholders: Idle, Walk, Run, Jump, DoubleJump, MeleeAttack, RangedAttack, Hurt, Death, Respawn.
• Audio placeholders: jump sound, attack swing, coin pickup, damage grunt.
• VFX placeholders: coin sparkle, hit flash, respawn poof.

----------------------------------------
13. NON-FUNCTIONAL REQUIREMENTS
----------------------------------------
• Unity Editor 60001.4f
• Input System 1.14.2 — action maps for Move, Jump, Attack, Use
• AI Navigation 2.0.8 — NavMesh baking + OffMeshLinks
• Netcode for GameObjects 2.4.4 — server authoritative sync
• Unity Physics 1.3.14 — deterministic physics queries
• Target frame rate: 60 FPS on mid-tier GPU (GTX 1060 / RX 580)

----------------------------------------
14. SUCCESS METRICS (Prototype)
----------------------------------------
• AI enemies consistently follow stationary sentinel spec.
• Players can jump/double-jump with no desync.
• Match loop completes without major bugs.
• Coin economy works as designed in 95%+ of playtest runs.
