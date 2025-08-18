# MemeOn MOBA - Immediate Action Plan
**Date:** August 17, 2025  
**Based On:** Comprehensive Repository Audit  
**Priority:** Critical Issues First  

## Sprint 1 - Critical Fixes (1-2 weeks)

### 🔴 MUST FIX - Blocks Playable Build
1. **Fix Layer Constants Mismatch**
   - **File:** `Assets/Scripts/Network/ProjectConstants.cs`
   - **Issue:** Layer indices don't match TagManager.asset
   - **Fix:** Update Layer constants to match actual Unity settings:
     ```csharp
     public const int Environment = 3;  // Currently 6
     public const int Player = 6;       // Currently 7  
     public const int Enemy = 7;        // Add this
     public const int AI = 8;           // Correct
     public const int Projectile = 9;   // Currently 9, name mismatch (Projectiles)
     ```

2. **Complete Match Management**
   - **File:** `Assets/Scripts/Game/MatchManager.cs`
   - **Issue:** Missing overtime and winner determination
   - **Fix:** Implement the TODO in Update() method for match end logic
   - **Requirements:** First team to score in overtime wins (per GDD)

3. **Consolidate Health Systems** 
   - **Issue:** Multiple overlapping health implementations
   - **Decision Needed:** Choose NetworkHealth OR HealthServer as primary
   - **Files to Review:**
     - `Assets/Scripts/Combat/NetworkHealth.cs`
     - `Assets/Scripts/Combat/HealthServer.cs`  
     - `Assets/Scripts/Combat/HealthNetwork.cs`

### 🟡 HIGH PRIORITY - Needed for Testing
4. **Verify AISpawnerManager Prefab Assignment**
   - **Action:** Open AISpawnerManager in scene, assign AI_Sentinel prefab
   - **File:** Check scene with AISpawnerManager component
   - **Validation:** Ensure aiPrefab field is not null

5. **Create Character Stat Implementations**
   - **Issue:** No character-specific prefabs despite GDD specifications
   - **Required Characters:**
     - Elom Nusk: 100 HP, 6 m/s speed, 2.5m jump
     - Doge Dog: 80 HP, 7.5 m/s speed, 2.7m jump  
     - Tronald Dump: 140 HP, 5 m/s speed, 2.2m jump
   - **Action:** Create prefab variants with CharacterStats configured

6. **Camera Pan + Jump/Double Jump Validation**
   - **Files:** `Assets/Scripts/Camera/UniteCameraController.cs`, `Assets/Scripts/Players/PlayerMovement.cs`
   - **Action:** Assign Look and set PanModifier to Sprint (Left Shift) for live pan; ensure Move is bound for free-cam on death. Verify Jump action.
   - **QA:**
     - Alive: Hold Sprint (Left Shift) + move mouse → camera pans horizontally and vertically (Y only), release → recenters.
     - Dead: Move.x pans horizontally; Move.y raises/lowers along Y; scroll adjusts zoom.
     - Jump: Jump from ground; double jump mid-air; landing restores double jump.

## Sprint 2 - Core Functionality (2-3 weeks)

### Input System Integration
- **Verify:** PlayerController uses InputSystem_Actions.inputactions
- **Test:** All input actions (Move, Jump, AttackPrimary, AttackSecondary, Interact) work

### Team System Standardization  
- **Issue:** Multiple team implementations (Team.cs, TeamId.cs, TeamUtility.cs)
- **Decision:** Choose one primary system, refactor references
- **Impact:** Combat, AI targeting, scoring systems

### UI System Implementation
- **Components Needed:**
  - Health bar display connected to health system
  - Coin counter UI connected to PlayerInventory
  - Match timer display connected to MatchManager
  - Team score display

## Sprint 3 - Polish & Integration (1-2 weeks)

### Camera System Polish
- **File:** `Assets/Scripts/Camera/UniteCameraController.cs`
- **Enhancement:** Add viewport boundary constraints
- **Reference:** Pokemon Unite-style camera with dynamic limits

### AI Behavior Tree Enhancement
- **File:** `Assets/Scripts/AI/BT/AIBehaviorTree.cs`  
- **Issue:** Currently placeholder implementation
- **Reference:** Detailed pseudocode in `DOCUMENT 7 — AI LOGIC REFERENCE (FSM + BT).md`
- **Focus:** Perception system and target selection logic

### Performance Optimization
- **Add GC monitoring:** Implement allocation tracking for mobile performance
- **Verify tick rates:** Confirm AI (10Hz) and BT (5Hz) performance on mobile
- **Profile network:** Validate < 25 kB/s bandwidth target

## Quality Assurance Checklist

Before each sprint completion, verify:
- [ ] Project compiles without errors
- [ ] All required prefabs assigned in NetworkManager  
- [ ] AI spawns and follows basic FSM states
- [ ] Players can move and attack
- [ ] Network synchronization works client-server
- [ ] Match timer counts down properly
- [ ] Health/damage system functions correctly

## Testing Strategy

### Manual Testing Priority
1. **AI Behavior:** Spawn AI, attack it, verify state transitions
2. **Combat System:** Player vs Player damage, team checking
3. **Match Flow:** Full match from start to end condition
4. **Network Sync:** Two clients + server, verify state consistency

### Automated Testing (Future)
- Unit tests for AI state transitions
- Integration tests for combat system
- Performance regression tests

## Documentation Updates Needed

As issues are fixed, update:
1. **Updated_Development_Checklist.md** - Move items from Pending to Completed
2. **MASTER_SPEC_LOG.md** - Add implementation notes for major fixes  
3. **Usage Notes for AISpawnerManager.md** - Update prefab names if changed

## Risk Mitigation

### High Risk Items
- **Health System Refactor:** Could break existing combat interactions
  - *Mitigation:* Test thoroughly in isolated scene first
- **Layer Constant Changes:** Could break physics interactions
  - *Mitigation:* Update all references, test collision detection

### Medium Risk Items  
- **Character Prefab Creation:** Could affect balance testing
  - *Mitigation:* Start with one character, validate system works
- **Team System Changes:** Could break AI targeting
  - *Mitigation:* Test AI can still find and attack players

## Success Metrics

**Sprint 1 Complete When:**
- AI spawns correctly in scene
- Match timer reaches zero and declares winner  
- All compilation errors resolved
- Layer constants match Unity settings

**Sprint 2 Complete When:**
- All three characters available with correct stats
- Players can complete full match gameplay loop
- UI displays essential game state information
- Input system fully functional

**Sprint 3 Complete When:**
- Game runs smoothly on mobile hardware simulation
- AI exhibits intelligent targeting and decision-making
- Camera provides smooth, constrained viewing experience
- Performance metrics meet mobile targets

---

**Next Steps:** Begin with Sprint 1 critical fixes. Each fix should be tested immediately before moving to the next item. Use the existing excellent architecture as foundation - avoid major refactoring unless absolutely necessary.