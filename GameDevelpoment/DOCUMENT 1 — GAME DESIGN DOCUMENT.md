========================================
DOCUMENT 1 — CONCEPT DOCUMENT
========================================

TITLE:
Meme Online Battle Arena (MOBA)
(Working Title — subject to change at production milestone 3)

VERSION:
v0.1 — Concept Approval Draft

AUTHORSHIP:
Lead Designer: [Redacted]
Engineering Lead: [Redacted]
Narrative/Creative: [Redacted]
AI Systems Architect: [Redacted]
Date: [YYYY-MM-DD]

REVISION HISTORY:
0.1 — Initial PHD-level engineering-oriented concept draft.

----------------------------------------
1. HIGH-CONCEPT STATEMENT
----------------------------------------
Meme Online Battle Arena is a 3D platformer Multiplayer Online Battle Arena 
that fuses high-mobility platforming (jump, double jump, dash) with satirical 
dark comedy rooted in Internet meme culture. The core loop is intentionally 
short-form, focusing on matches that deliver high engagement in 1–10 minute 
bursts while emphasizing mechanical precision, deterministic server simulation, 
and hybrid AI systems (Finite State Machine + Behavior Tree).

The game’s thematic tone derives from ironic, over-the-top caricatures of 
real-world and meme-famous personalities. It parodies corporate, political, 
and cryptocurrency culture in an arena-based PvP format.

----------------------------------------
2. CREATIVE PILLARS
----------------------------------------
• Mechanical Clarity:
  Movement and combat interactions are deterministic, with consistent 
  frame-perfect rules, ensuring competitive integrity.

• Competitive Parity:
  Symmetric maps for fairness; asymmetric character kits for varied 
  playstyles.

• Satirical Identity:
  Humor and parody are integrated into visual/audio feedback without 
  impairing competitive readability.

• Network Determinism:
  Authoritative server simulation using Netcode for GameObjects with 
  rollback-friendly state management.

----------------------------------------
3. TARGET AUDIENCE
----------------------------------------
Core: Competitive gamers aged 16–35 with interest in short-session PvP 
and meme culture.
Secondary: Casual meme enthusiasts interested in humor and satire 
delivered via interactive media.

----------------------------------------
4. TARGET PLATFORMS
----------------------------------------
• Primary: PC (Windows 10+, DirectX 11/12 capable GPU)
• Secondary (post-launch): PlayStation, Xbox, and potentially Switch 
(with adjusted performance budgets).

----------------------------------------
5. CORE GAMEPLAY LOOP
----------------------------------------
1. Spawn into the arena with selected meme-character kit.
2. Navigate environment using 3D platforming skills (jump, double jump, dash).
3. Engage in combat with enemy players and/or AI-controlled opponents.
4. Defeat opponents to force crypto coin drops.
5. Collect coins and deposit them at team’s scoring zone.
6. Repeat until match timer expires.
7. Match concludes with scoreboard and performance summary.

----------------------------------------
6. GAME MODES
----------------------------------------
• 1v1 — 1 minute (duel)
• 3v3 — 3 minutes (skirmish)
• 3v3 — 5 minutes (PROTOTYPE FOCUS)
• 5v5 — 10 minutes (full team mode)

Prototype scope is limited to the 5-minute 3v3 mode to reduce complexity 
and deliver a functional vertical slice of core mechanics.

----------------------------------------
7. NARRATIVE AND THEME
----------------------------------------
Setting:
An absurdist digital coliseum that exists inside a crypto-verse simulation.

Playable Characters:
• Elom Nusk — eccentric billionaire parody.
• Doge Dog — anthropomorphic Shiba Inu with meme-based charm.
• Tronald Dump — exaggerated political figure caricature.

Tone:
Self-aware, comedic, leaning heavily on Internet meme conventions 
(impact text overlays, ironic voice lines, exaggerated VFX).

----------------------------------------
8. NON-FUNCTIONAL REQUIREMENTS
----------------------------------------
• Engine: Unity Editor 60001.4f (LTS release)
• Packages:
  - Input System 1.14.2
  - AI Navigation 2.0.8
  - Netcode for GameObjects 2.4.4
  - Unity Physics 1.3.14
• Latency budget: ≤100ms round-trip for core actions
• Tick rate: 60 Hz simulation target
• Cross-platform deterministic physics compliance

----------------------------------------
9. DEVELOPMENT GOALS
----------------------------------------
Primary Goal:
Deliver a fully networked, deterministic, prototype-ready 5-minute 
3v3 match mode with complete core loop: movement, combat, coin 
collection, AI enemies, and scoring.

Secondary Goals:
Integrate placeholder animation and VFX markers for rapid iteration, 
leaving polish for later sprints.

----------------------------------------
10. SUCCESS CRITERIA
----------------------------------------
• Stable multiplayer matches with <5% packet loss tolerance
• AI behavior meeting stationary sentinel spec (idle until provoked)
• Fully functioning jump and double-jump mechanics
• Deterministic scoring system with no desync between clients
• Playtesters able to complete match loop without critical bugs
