========================================
DOCUMENT 4 — PROJECT PLAN
========================================

TITLE:
Meme Online Battle Arena — Prototype 5-Minute 3v3 Mode
Project Execution & Milestone Plan

VERSION:
v0.1 — Production Scheduling Draft

AUTHORSHIP:
Production Manager: [Redacted]
Engineering Lead: [Redacted]
Design Lead: [Redacted]
Date: [YYYY-MM-DD]

REVISION HISTORY:
0.1 — Initial production roadmap with phased deliverables.

----------------------------------------
1. PROJECT SCOPE
----------------------------------------
Deliverable:
A functional, server-authoritative 3D platformer MOBA prototype focused on the 5-minute 3v3 mode with:
- Fully networked player movement and combat
- Coin economy loop
- Stationary sentinel AI enemies (FSM + BT hybrid)
- Placeholder assets and animations
- Deterministic physics for all core mechanics

Constraints:
- Unity Editor 60001.4f
- Input System 1.14.2, AI Navigation 2.0.8, Netcode for GameObjects 2.4.4, Unity Physics 1.3.14
- Mid-tier PC performance target: 60 FPS on GTX 1060-class GPU

----------------------------------------
2. MILESTONE SCHEDULE
----------------------------------------

MILESTONE 1 — CORE FRAMEWORK (Weeks 1–4)
Objectives:
• Project setup with required Unity packages installed and configured
• Input System 1.14.2 action maps defined for all movement/combat interactions
• Physics-based PlayerController with jump and double-jump
• Server-authoritative network movement (Netcode 2.4.4) with reconciliation
• FSM/BT base architecture with blackboard integration
• AI Navigation NavMesh bake for prototype map

Deliverables:
- Working networked movement demo
- AI test agent moving via NavMesh
- Physics and network prediction validated

Dependencies:
- Package installation complete
- Prototype map rough layout in place

----------------------------------------

MILESTONE 2 — CORE GAMEPLAY LOOP (Weeks 5–8)
Objectives:
• Melee and ranged combat systems (server-side hit detection)
• Coin spawn and drop/collect mechanics
• Deposit zone scoring system
• Full stationary sentinel AI spec implemented
• AI engages players per FSM/BT design, respects aggro radius and give-up timeout
• Match manager with timer, score tracking, and win condition evaluation

Deliverables:
- End-to-end playable match with bots and players
- Coins drop, collect, and score correctly
- AI returns to spawn after disengagement
- Basic HUD displaying health, coins, and timer

Dependencies:
- M1 complete and tested
- Combat system integrated with PlayerController

----------------------------------------

MILESTONE 3 — CONTENT & POLISH (Weeks 9–12)
Objectives:
• Three playable characters with unique stats and ability kits
• Placeholder animations for locomotion and combat
• Basic audio/VFX markers for core interactions
• Balancing for movement speed, jump height, and ability cooldowns
• Performance profiling and optimization
• Bug fixing and stability passes

Deliverables:
- Fully playable prototype with 3 characters
- AI stable at target tick rate and network load
- Matches can run full duration without major issues
- Prototype ready for closed playtest

Dependencies:
- M2 complete and stable
- Placeholder art and sound assets imported

----------------------------------------
3. SPRINT BREAKDOWN
----------------------------------------
Each milestone split into 2-week sprints:

Sprint 1:
- Unity project setup
- Input System integration
- PlayerController basic movement

Sprint 2:
- Networking movement
- Physics jump/double jump
- AI FSM skeleton

Sprint 3:
- Combat mechanics
- Coin system
- Deposit scoring

Sprint 4:
- AI BT logic
- Aggro/return behavior
- Match timer and flow

Sprint 5:
- Character kits
- Animation placeholders
- Basic UI

Sprint 6:
- Audio/VFX placeholders
- Balancing pass
- Stability fixes

----------------------------------------
4. DEPENDENCY GRAPH
----------------------------------------
Core Dependencies:
- Input System must be operational before PlayerController implementation
- PlayerController and Networking must be complete before Combat and Coin System
- NavMesh bake must precede AI pathfinding implementation
- FSM base must exist before BT tactical layer

----------------------------------------
5. RISK REGISTER
----------------------------------------
Risk: Networking desync due to deterministic physics mismatch.
Mitigation: Server reconciliation with delta correction.

Risk: AI NavMesh pathfinding conflict with Unity Physics collisions.
Mitigation: Use NavMesh path for direction vector, apply via physics-based movement.

Risk: Scope creep from adding non-essential features early.
Mitigation: Lock feature set per milestone; defer all extras.

----------------------------------------
6. EXIT CRITERIA (PROTOTYPE)
----------------------------------------
• Matches run from start to finish without network disconnects >5% occurrence rate.
• AI follows stationary sentinel rules in ≥ 95% of cases.
• Physics is consistent across client and server with no critical desync.
• All core systems tested with simulated latency (up to 150ms RTT).
Next will be Document 5 – Asset List with classification and dependencies in plain text. Do you want me to continue?
