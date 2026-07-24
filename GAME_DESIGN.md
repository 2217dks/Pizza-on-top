# Pizza On Top - Game Design & Development Document

## 1. Game Overview
- **Title**: Pizza On Top
- **Genre**: 2D Side-Scrolling Vertical Platformer / Metroidvania
- **Art Style**: 2D Pixel Art (Tilemaps, Rule Tiles, Pixel-perfect Camera)
- **Controls**: Unity New Input System (`UnityEngine.InputSystem`) with primary Keyboard & Mouse support (`Space` / `Up Arrow` to jump; `A` / `D` or `Left` / `Right` to move; `E` to interact/door/carry box; `F` or Left Click to fire gun).
- **Core Loop**: Deliver a fresh pizza to the customer waiting on the building roof (Floors 1 to 9) before the global countdown timer hits zero.

---

## 2. Core Gameplay Mechanics & Hazards

### ⏱️ Global Countdown Timer & Progress Penalty
- A single overall timer (e.g. 5:00 minutes) governs the entire run across all 7–9 building floors.
- **Death & Trap Rule**: No artificial time point deductions are taken on respawn. Instead, **the timer continues running continuously**, so falling down or dying forces you to redo parkour sections, losing valuable time naturally!

### 💨 1. Wind Fan System (`WindFan2D.cs`)
- Directional wind force fields supporting Updrafts (`Up`), Horizontal Wind Resistance (`Left` / `Right`), and custom angles.
- Updraft shafts allow the player to float or lift smoothly up vertical building shafts!

### 📦 2. Pushable / Overhead Carry Box & Pressure Plate (`PushBox2D.cs`, `PressurePlate2D.cs`)
- **PushBox2D**: Physics-based push box. Walk into it to push, or press **`E`** to pick it up overhead on top of the character's head without needing custom animations! Press **`E`** again to drop or toss it forward.
- **PressurePlate2D**: Weight-activated button plate. Triggers UnityEvents when pressed by a player or box (used for opening puzzle doors or disabling laser beams).

### 🔑 3. Key Collectible System (`KeyCollectible2D.cs`)
- Special key item placed on a specific building floor.
- Collecting the key updates persistent state in [GameManager.cs](file:///Users/dhruv/Documents/gameProjects/Pizza-on-top/Assets/Scripts/Managers/GameManager.cs) (`HasSpecialKey = true` & `IsGunUnlocked = true`).

### 🔫 4. Player Gun Weapon & Bullets (`PlayerGun2D.cs`, `Bullet2D.cs`)
- Unlocked when the special key is collected. Press **`F`** or **Left Click** to fire projectiles.
- Destroys laser beam machines and deals damage to the roof Mini-Boss!

### ⚡ 5. Laser Beam Hazards & Destructible Emitters (`LaserEmitter2D.cs`)
- Hollow Knight Crystal Peak style continuous/pulsing 2D raycast laser beams. Touching the beam triggers player death (`RespawnPlayer()`).
- **Destructible Machine Target**: Machine takes damage from bullet hits (or box impacts) and breaks permanently, disabling the beam hazard!

---

## 3. Standardized Prefabs & Level Design Workflow

### 📦 Drag-and-Drop Prefabs Directory (`Assets/Prefabs/`)
- `Player.prefab` — Platformer controller, rope swing, animator, overhead box carrying socket, and gun controller.
- `Door.prefab` — Entrance / Exit floor transition gate.
- `SpawnPoint.prefab` — Player arrival & respawn point.
- `Rope.prefab` — 360° top anchor elastic rope with 120° free rotation arc.
- `CrumblingTilemap` — Painted breakable ground traps.
- `PushBox.prefab` — Pushable & overhead carry box.
- `PressurePlate.prefab` — Weight button plate.
- `KeyCollectible.prefab` — Special key item.
- `LaserEmitter.prefab` — Destructible laser beam hazard.
- `WindFan.prefab` — Updraft wind lift shaft.

---

## 4. Unity Project Architecture

```
Assets/
├── Scripts/
│   ├── Camera/
│   │   └── CameraController2D.cs       # Vertically locked 2D camera follow with ResetCameraVelocity()
│   ├── Managers/
│   │   ├── GameManager.cs              # Global game state, persistent timer, key & gun tracking
│   │   ├── TimerManager.cs             # Persistent countdown timer across scenes
│   │   └── LevelTransitionManager.cs   # Door scene loading & spawn positioning (GC-free cached YieldInstructions)
│   ├── Player/
│   │   ├── PlayerController2D.cs       # Snappy Hollow Knight jump physics (Coyote Time & Jump Buffer)
│   │   ├── PlayerAnimator.cs           # Sprite flipping, Player_Run direct playback, safe parameter checks
│   │   ├── PlayerRopeSwing.cs          # Rope swing with physics force pumping & Modern ContactFilter2D Physics
│   │   ├── PlayerInteract.cs           # Door interaction trigger with body-center positioning
│   │   ├── PlayerGun2D.cs              # Key-unlocked shooting weapon controller
│   │   └── Bullet2D.cs                 # Projectile physics & obstacle/laser damage dealing
│   ├── Environment/
│   │   ├── Door.cs                     # Floor entrance/exit triggers
│   │   ├── SpawnPoint.cs               # Player arrival locations
│   │   ├── Rope.cs & RopeSegment.cs    # 360° rope with 120° free rotation and minor over-bend resistance
│   │   ├── CrumblingTilemap2D.cs       # Direct foot cell sampling crumbling tilemaps with matrix shaking
│   │   ├── WindFan2D.cs                # Updraft & horizontal wind force field shafts
│   │   ├── PushBox2D.cs                # Pushable & overhead carry box
│   │   ├── PressurePlate2D.cs          # Weight-activated button plate
│   │   ├── KeyCollectible2D.cs         # Special key item pick-up
│   │   ├── LaserEmitter2D.cs           # Destructible laser beam hazard generator
│   │   ├── Hazard2D.cs                 # Traps & pitfalls
│   │   └── MovingPlatform2D.cs         # Moving platforms
│   └── UI/
│       └── HUDController.cs            # Timer HUD & Floor counter
├── Prefabs/
├── Scenes/
└── Tilemaps/
```

---

## 5. Development Progress & Log

| Date | Phase / Feature | Status | Notes |
| :--- | :--- | :---: | :--- |
| 2026-07-23 | Project Architecture & Core Design | ✅ Completed | Created C# script specifications, persistent timer rules, physics rope swing, door transitions, and design doc sync. |
| 2026-07-23 | Core C# Scripts Implementation | ✅ Completed | Created all C# scripts (`TimerManager`, `GameManager`, `LevelTransitionManager`, `PlayerController2D`, `PlayerInteract`, `Door`, `SpawnPoint`, `Rope`, `Hazard2D`, `SpringPad2D`, `MovingPlatform2D`, `HUDController`). |
| 2026-07-24 | Cleanup Unused Mechanics | ✅ Completed | Removed `SpringPad2D.cs` and `CrumblingPlatform2D.cs` completely from the codebase as requested. |

---

## 6. Where We Left Off
- **Current Task**: Removed unused SpringPad2D and CrumblingPlatform2D scripts.
- **Next Steps**: Level building with prefabs!
