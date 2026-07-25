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

### 💀 Death Animation & Respawn Delay (`respawnDelayTime = 1.0s`)
- `PlayerController2D.cs` now exposes **`respawnDelayTime`** in the Inspector (Default: `1.0s`).
- When player dies, the character immediately stops, plays the `Hurt`/`Dead` animation, and pauses for `1.0` second so you can see the death before teleporting back to spawn!

### ⚡ 1. Laser Beam Hazards & Destructible Emitters (`LaserEmitter2D.cs`, `LaserBeam2D.cs`)
- **Reliable Range Detection (`targetingRange = 25f`)**: The laser machine aims and shoots laser blasts smoothly whenever the player is within range.
- **Gizmos Clean Selection**: Switched to `OnDrawGizmosSelected()` across all scripts to eliminate Unity Editor `Screen position out of view frustum` warnings!
- **Lifetime-Limited Laser Blast Beam (`LaserBeam2D.cs`)**:
  - Automatically destroys itself after `beamLifetime = 2.5s` if it touches nothing.
  - When the beam touches **ANY obstacle, wall, floor, or push box**, **ONLY the beam GameObject destroys itself on impact**, leaving environment objects completely safe and intact!
  - When the beam hits the `Player`, it triggers `player.RespawnPlayer()` and destroys itself.
- **Horizontal Beam Sprite Alignment**: The laser beam sprite is stretched **horizontally** along the X-axis (`transform.right`) so it flies head-first towards the player!
- **Shooting Interval (`fireRateInterval = 1.2s`)**: Machine periodically shoots laser beam blasts.
- **Destructible Machine Target**: Machine takes 3 hits from gun bullets (or box impacts) and breaks permanently!

### 📍 Auto-Spawning Player System (`SpawnPoint.cs`)
- **Auto-Player Spawning (`autoSpawnIfMissing = true`)**: `SpawnPoint.cs` checks if a `Player` exists in the scene. If missing, it automatically instantiates `Player.prefab` at the `SpawnPoint` location (`transform.position`)!

### 🧱 Serpentine (Zig-Zag) Floor Progression & 16-Unit Camera Y-Snapping
- All 9 building floors are stacked vertically in a single Unity scene.
- **Floor Height**: Each floor is **16 units high** (Camera Orthographic Size `8`).
  - **Floor 1** (`Y = 0` to `16`): Start Left $\rightarrow$ Navigate Right $\rightarrow$ Exit Door on far Right.
  - **Floor 2** (`Y = 16` to `32`): Spawn on Far Right (directly above Floor 1 exit) $\rightarrow$ Navigate Left $\rightarrow$ Exit Door on far Left.
  - **Floor 3** (`Y = 32` to `48`): Spawn on Far Left (directly above Floor 2 exit) $\rightarrow$ Navigate Right $\rightarrow$ Exit Door on far Right.
  - Continues in a serpentine zig-zag pattern up to the Roof Delivery Goal on Floor 9!
- **Automatic Floor Camera Y-Snapping (`CameraController2D.cs`)**:
  - Automatically calculates `currentFloorIndex = Mathf.FloorToInt(player.y / 16f)`.
  - Centers camera Y at `(currentFloorIndex * 16) + 8` (Floor 1 = `Y 8`, Floor 2 = `Y 24`, Floor 3 = `Y 40`).
  - Smoothly slides vertically when entering doors to frame the new floor perfectly!

### 🧗 2. 120° Free Rotation Elastic Rope Physics (`PlayerRopeSwing.cs`, `Rope.cs`, `RopeSegment.cs`)
- **Edit Mode Scene View Visibility (`[ExecuteAlways]` + `EditorApplication.delayCall`)**: Ropes automatically generate and render their 3 segments directly in Scene View while designing levels in Edit Mode without any `OnValidate` warnings or errors!
- **Exact Tip-to-Tip HingeSockets**:
  - `Anchor` (This Segment): Top tip `(0, +halfHeight)`.
  - `Connected Anchor` (Previous Segment): Bottom tip `(0, -halfHeight)`.
  - Guarantees 3 segments connect seamlessly with zero gaps, zero overlaps, and zero physics stretching!
- **Non-Solid Trigger Colliders (`isTrigger = true`)**: `Rope.cs` and `RopeSegment.cs` force all segment colliders to be triggers so the player passes smoothly through the rope without physical collision bumping or blocking!
- **360° Top Ceiling Anchor**: Top joint rotates 360 degrees freely (`allowFull360TopAnchor = true`) for full loop swings!
- **120° Free Rotation Arc (-60° to +60°)**: Inner joints rotate completely freely within a 120-degree arc with zero resistance.
- **Strict Launch Velocity Clamping**: `DetachFromRope()` clamps segment velocity to `maxLaunchSpeed = 16f` before applying multipliers, guaranteeing smooth launches even inside intense wind fields!
- **Body Center Snapping**: Calculates the exact body/grab center (`GetCenterPosition()`) and snaps the **hands/chest** directly onto the rope handle.

### 💨 3. Wind Fan System (`WindFan2D.cs`)
- Directional wind force fields supporting Updrafts (`Up`), Horizontal Wind Resistance (`Left` / `Right`), and custom angles.
- **Default Settings**: `windForce = 6`, `verticalWindMultiplier = 8`, `maxSpeedInWind = 10`.
- **Vertical Gravity Compensation**: Automatically balances gravity for vertical updrafts.
- **Active Movement Integration**: `PlayerController2D.cs` integrates `ActiveWindVelocity` directly into `FixedUpdate()` target speed calculation `(moveInput * moveSpeed) + ActiveWindVelocity.x`.
- **Inspector Area Controls**: Exposed `windAreaSize` (Width x Height) and `windAreaOffset` fields directly in the Inspector! Automatically resizes the trigger box and draws blue scene gizmos for instant visual range tuning.

### 📦 4. Pushable / Overhead Carry Box & Pressure Plate (`PushBox2D.cs`, `PressurePlate2D.cs`)
- **PushBox2D**: Physics-based push box. Walk into it to push, or press **`E`** to pick it up overhead on top of the character's head without needing custom animations! Press **`E`** again to drop or toss it forward. Modern Unity 6 `RB.bodyType` API.
- **PressurePlate2D**: Weight-activated button plate. Triggers UnityEvents when pressed by a player or box (used for opening puzzle doors or disabling laser beams).

### 🔑 5. Key Collectible System (`KeyCollectible2D.cs`)
- Special key item placed on a specific building floor.
- Collecting the key updates persistent state in [GameManager.cs](file:///Users/dhruv/Documents/gameProjects/Pizza-on-top/Assets/Scripts/Managers/GameManager.cs) (`HasSpecialKey = true` & `IsGunUnlocked = true`).

### 🔫 6. Player Gun Weapon & Bullets (`PlayerGun2D.cs`, `Bullet2D.cs`)
- Unlocked when the special key is collected. Press **`F`** or **Left Click** to fire projectiles.
- Destroys laser beam machines and deals damage to the roof Mini-Boss!

---

## 3. Standardized Prefabs & Level Design Workflow

### 📦 Drag-and-Drop Prefabs Directory (`Assets/Prefabs/`)
- `Player.prefab` — Platformer controller, rope swing, animator, overhead box carrying socket, and gun controller.
- `Door.prefab` — Entrance / Exit floor transition gate.
- `SpawnPoint.prefab` — Auto-spawner arrival & respawn point.
- `RopeSegment.prefab` — Individual physics rope segment using your custom rope sprite.
- `Rope.prefab` — 360° top anchor elastic rope parent prefab with Edit Mode Scene View visibility.
- `CrumblingTilemap` — Painted breakable ground traps.
- `PushBox.prefab` — Pushable & overhead carry box.
- `PressurePlate.prefab` — Weight button plate.
- `KeyCollectible.prefab` — Special key item.
- `LaserBeam.prefab` — Horizontal lifetime-limited laser blast projectile.
- `LaserEmitter.prefab` — Destructible laser machine aiming and shooting reliably when player is in range.
- `WindFan.prefab` — Updraft wind lift shaft with adjustable area size.

---

## 4. Unity Project Architecture

```
Assets/
├── Scripts/
│   ├── Camera/
│   │   └── CameraController2D.cs       # Automatic 16-unit floor Y-snapping & smooth dampening
│   ├── Managers/
│   │   ├── GameManager.cs              # Global game state, persistent timer, key & gun tracking
│   │   ├── TimerManager.cs             # Persistent countdown timer across scenes
│   │   └── LevelTransitionManager.cs   # Door scene loading & intra-scene teleportation
│   ├── Player/
│   │   ├── PlayerController2D.cs       # Snappy Hollow Knight jump physics & respawnDelayTime (1.0s) setting
│   │   ├── PlayerAnimator.cs           # Sprite flipping, Player_Run direct playback, safe parameter checks
│   │   ├── PlayerRopeSwing.cs          # Rope swing with physics force pumping & launch speed clamping
│   │   ├── PlayerInteract.cs           # Door interaction trigger with body-center positioning
│   │   ├── PlayerGun2D.cs              # Key-unlocked shooting weapon controller
│   │   └── Bullet2D.cs                 # Projectile physics & obstacle/laser damage dealing
│   ├── Environment/
│   │   ├── Door.cs                     # Floor entrance/exit triggers (teleports to target SpawnPoint & updates floor)
│   │   ├── SpawnPoint.cs               # Auto-player spawning & arrival locations with unique SpawnIDs
│   │   ├── Rope.cs & RopeSegment.cs    # 360° rope with [ExecuteAlways] Edit Mode Scene View visibility
│   │   ├── CrumblingTilemap2D.cs       # Direct foot cell sampling crumbling tilemaps with matrix shaking
│   │   ├── WindFan2D.cs                # Updraft & horizontal wind force field shafts
│   │   ├── PushBox2D.cs                # Pushable & overhead carry box (RB.bodyType API)
│   │   ├── PressurePlate2D.cs          # Weight-activated button plate
│   │   ├── KeyCollectible2D.cs         # Special key item pick-up
│   │   ├── LaserEmitter2D.cs           # Reliable laser machine aiming and shooting when player in range
│   │   ├── LaserBeam2D.cs              # Horizontal lifetime-limited laser blast
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
| 2026-07-23 | Core C# Scripts Implementation | ✅ Completed | Created all C# scripts (`TimerManager`, `GameManager`, `LevelTransitionManager`, `PlayerController2D`, `PlayerInteract`, `Door`, `SpringPad2D`, `MovingPlatform2D`, `HUDController`). |
| 2026-07-25 | Out-of-Frustum Gizmos Clean Fix | ✅ Completed | Updated `LaserEmitter2D.cs` & `SpawnPoint.cs` to use `OnDrawGizmosSelected()`, resolving Unity Editor `Screen position out of view frustum` GUI warnings. |

---

## 6. Where We Left Off
- **Current Task**: Updated Gizmos to OnDrawGizmosSelected resolving Unity Editor out-of-frustum warning.
- **Next Steps**: Level building!
