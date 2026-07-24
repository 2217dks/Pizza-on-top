# Pizza On Top - Game Design & Development Document

## 1. Game Overview
- **Title**: Pizza On Top
- **Genre**: 2D Side-Scrolling Vertical Platformer / Metroidvania
- **Art Style**: 2D Pixel Art (Tilemaps, Rule Tiles, Pixel-perfect Camera)
- **Controls**: Unity New Input System (`UnityEngine.InputSystem`) with primary Keyboard & Mouse support
- **Core Loop**: Deliver a fresh pizza to the customer waiting on the building roof (Floors 1 to 9) before the global countdown timer hits zero.

---

## 2. Core Gameplay Mechanics

### ⏱️ Global Countdown Timer & Progress Penalty
- A single overall timer (e.g. 5:00 minutes) governs the entire run across all 7–9 building floors.
- **Death & Trap Rule**: No artificial time point deductions are taken on respawn. Instead, **the timer continues running continuously**, so falling down or dying forces you to redo parkour sections, losing valuable time naturally!

### 🎨 Character Animation & Visual Polish
- **PlayerAnimator ([PlayerAnimator.cs](file:///Users/dhruv/Documents/gameProjects/Pizza-on-top/Assets/Scripts/Player/PlayerAnimator.cs))**:
  - Auto-flips sprite based on movement direction.
  - Controls animation states (`Idle`, `Run`, `Jump`, `Rope`, `Hurt`, `Dead`, `Attack`).
  - **Direct State Playback**: Uses **Player_Run** for horizontal running movement!
  - **Safe Parameter Hashes**: Automatically detects which parameters exist in your `PlayerAnimatorController` and suppresses missing parameter warnings (`Hash parameter does not exist`).
  - **Procedural Squash & Stretch**: Gives instant visual feedback for jumping and landing.

### 🎥 2D Smooth Camera System
- **SmoothDamp Following**: Auto-targets player tagged `Player` with configurable offsets and smooth dampening (`0.2s`).
- **Level Bounding Support**: Optional `minBounds` and `maxBounds` to lock camera inside building walls.

### 🏃 Snappy 2D Platformer Physics & Debug Controls
- **Punchy Gravity**: Snappy gravity scale (`3.2x`) and punchy jump force (`17.5f`) for fast rising and crisp falling.
- **Variable Jump Height & Jump Cut**: Active `isJumping` state tracking. Releasing jump key early cuts upward velocity by `0.4f`.
- **Air Control**: Smooth air control multiplier (`0.75f`) preserving jump momentum.
- **Modern Unity Physics API**: Uses non-deprecated `Physics2D.OverlapBox` and `Physics2D.OverlapCircle` with `ContactFilter2D` for zero-allocation performance.
- **Debug Death Testing Key**: Press **K** at any time to trigger player death, play the `Hurt`/`Dead` animation, and test entrance respawning!

### 🧗 120° Free Rotation Elastic Rope Physics
- **360° Top Ceiling Anchor**: Top joint rotates 360 degrees freely (`allowFull360TopAnchor = true`) for full loop swings!
- **120° Free Rotation Arc (-60° to +60°)**: Inner joints rotate completely freely within a 120-degree arc with zero resistance.
- **Minor Over-Bend Resistance**: Past ±60°, joints flex up to ±135° under load against a gentle minor torque resistance (`minorResistanceTorque = 4f`).
- **Taut Bottom Weight**: Heavier bottom handle mass (`1.6`) keeps the rope straight under gravity.
- **Jump-to-Swing Inertia**: Incoming jump velocity transfers directly into the rope segment (`momentumTransferFactor = 0.85f`) upon latching.
- **Body Center Snapping**: Calculates the exact body/grab center (`GetCenterPosition()`) and snaps the **hands/chest** directly onto the rope handle.
- **FixedAnchor Hinge Connection**: Sets `playerHingeJoint.anchor = bodyOffsetFromFeet`, ensuring the pendulum pivot point stays locked at the character's hands/chest while swinging.
- **FixedUpdate Swing Pumping**: Pressing `A` / `D` in rhythm pumps continuous force (`180f`) inside `FixedUpdate()`, amplifying the swing arc higher with each pump!
- **Inspector Circle Radius & Offset Tuning**: Exposed radius & offset fields in Inspector for `PlayerRopeSwing` (`Max Grab Distance`) and `PlayerInteract` (`Interact Radius`).
- **Fluid Release**: Clean 1:1 launch velocity clamping (`18f`).

---

## 3. Unity Project Architecture

```
Assets/
├── Scripts/
│   ├── Camera/
│   │   └── CameraController2D.cs       # Smooth 2D camera follow with offset and bounds
│   ├── Managers/
│   │   ├── GameManager.cs              # Global game state & scene management
│   │   ├── TimerManager.cs             # Persistent countdown timer across scenes
│   │   └── LevelTransitionManager.cs   # Door scene loading & spawn positioning (GC-free cached YieldInstructions)
│   ├── Player/
│   │   ├── PlayerController2D.cs       # Snappy 3.2x gravity platformer controller (Modern ContactFilter2D Physics)
│   │   ├── PlayerAnimator.cs           # Sprite flipping, Player_Run direct playback, safe parameter checks
│   │   ├── PlayerRopeSwing.cs          # Rope swing with physics force pumping & Modern ContactFilter2D Physics
│   │   └── PlayerInteract.cs           # Door interaction trigger with body-center positioning
│   ├── Environment/
│   │   ├── Door.cs                     # Floor entrance/exit triggers
│   │   ├── SpawnPoint.cs               # Player arrival locations
│   │   ├── Rope.cs & RopeSegment.cs    # 360° rope with 120° free rotation and minor over-bend resistance
│   │   ├── CrumblingPlatform2D.cs      # Collapsible platforms
│   │   ├── Hazard2D.cs                 # Traps & pitfalls
│   │   ├── SpringPad2D.cs              # Bouncy launchpad
│   │   └── MovingPlatform2D.cs         # Moving platforms
│   └── UI/
│       └── HUDController.cs            # Timer HUD & Floor counter
├── Prefabs/
├── Scenes/
└── Tilemaps/
```

---

## 4. Development Progress & Log

| Date | Phase / Feature | Status | Notes |
| :--- | :--- | :---: | :--- |
| 2026-07-23 | Project Architecture & Core Design | ✅ Completed | Created C# script specifications, persistent timer rules, physics rope swing, door transitions, and design doc sync. |
| 2026-07-23 | Core C# Scripts Implementation | ✅ Completed | Created all C# scripts (`TimerManager`, `GameManager`, `LevelTransitionManager`, `PlayerController2D`, `PlayerInteract`, `Door`, `SpawnPoint`, `Rope`, `Hazard2D`, `SpringPad2D`, `MovingPlatform2D`, `HUDController`). |
| 2026-07-24 | Core Player & Rope Physics Complete | ✅ Completed | Player movement, animations, debug death key, body-center snapping, 360° rope loops, 120° free rotation arc, and GC-free physics are 100% complete and verified. |

---

## 5. Where We Left Off
- **Current Task**: Core player character, animations, physics, and rope swinging are 100% COMPLETE & VERIFIED!
- **Next Steps**: Level building, 2D Tilemap floor layouts, and building mechanics!
