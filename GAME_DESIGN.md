# Pizza On Top - Game Design & Development Document

## 1. Game Overview
- **Title**: Pizza On Top
- **Genre**: 2D Side-Scrolling Vertical Platformer / Metroidvania
- **Art Style**: 2D Pixel Art (Tilemaps, Rule Tiles, Retro CRT Post-Processing, Pixel-Perfect Camera)
- **Controls**:
  - `Space` / `Up Arrow` — Jump
  - `A` / `D` or `Left` / `Right` — Move
  - `E` — Interact (Tutorial boards, pick up / drop push box)
  - `F` / `Left Mouse Click` — Fire weapon (unlocked after collecting key)
  - `Left Shift` / `Left Mouse Click` — Grab & swing on ropes
- **Core Loop**: Deliver a fresh pizza to the customer waiting on the building roof (Floors 1 to 9) before the global countdown timer hits zero.

---

## 2. Core Gameplay Mechanics & Architecture

### 🧱 1. Single Continuous Level Scene (`Level.unity`) & Serpentine Floor Progression
- **Single Vertical Scene Architecture**: All building floors are stacked vertically inside a single scene (`Assets/Scenes/Level.unity`), eliminating multi-scene loading delays.
- **16-Unit Floor Height**: Each floor is **16 units high** (Camera Orthographic Size `8`).
  - **Floor 1** (`Y = 0` to `16`): Start Left $\rightarrow$ Navigate Right $\rightarrow$ Ascend to Floor 2.
  - **Floor 2** (`Y = 16` to `32`): Spawn Right $\rightarrow$ Navigate Left $\rightarrow$ Ascend to Floor 3.
  - **Floor 3** (`Y = 32` to `48`): Spawn Left $\rightarrow$ Navigate Right $\rightarrow$ Ascend to Floor 4.
  - Continues in a serpentine zig-zag pattern up to the Roof Delivery Goal on Floor 9!
- **Floor Manager (`FloorManager.cs`)**: Dynamically computes `CurrentFloor = Mathf.FloorToInt(playerY / 16f)`.
- **Automatic Camera Y-Snapping (`CameraController2D.cs`)**:
  - Snaps camera Y-center to `(currentFloorIndex * 16) + 8` with smooth dampening.
  - Optional horizontal bounds (`minXBounds`, `maxXBounds`) to prevent out-of-bounds camera view.

### ⏱️ 2. Global Countdown Timer & Death Rules (`TimerManager.cs`)
- A continuous timer (e.g. 5:00 minutes) governs the entire run across all building floors.
- **No Direct Point Deduction**: The timer runs continuously in real-time. Dying forces you to redo parkour sections, losing time naturally!
- **Timer Pause during Tutorials**: Interacting with tutorial signboards pauses the timer until the reader closes the popup.

### 💀 3. Death Animation & Respawn Delay (`PlayerController2D.cs`, `SpawnPoint.cs`)
- `respawnDelayTime` exposed in Inspector (Default: `1.0s`).
- When dying (touching hazards or laser beams), the character stops, plays `Hurt`/`Dead` animation, and pauses for `1.0s` so the player sees the death sequence.
- Teleports the character to the active floor's `SpawnPoint` registered in `SpawnPoint.All` dictionary.
- Automatically resets all `CrumblingTilemap2D` blocks across the scene on respawn!

### 🔊 4. Dynamic Audio System (`AudioManager.cs`)
- Singleton pattern (`AudioManager.Instance`) persisting across scenes.
- **Background Music**: Loops background music (`fight.ogg`) with dedicated music volume control.
- **SFX Triggers**:
  - Footsteps: Dynamic footstep sound selection from clips array (`footstepClips`) played on movement interval (`0.35s`).
  - Jump & Land SFX: Played during jump execution and grounded landing impact.
  - Death SFX: Plays `vgdeathsound.ogg` immediately when player dies.

### 📖 5. Interactive Tutorial System (`TutorialTrigger.cs`)
- Implements `IInteractable` interface triggered via `E` key near signboards.
- Toggles overlay canvas (`popup`).
- Pauses global timer (`TimerManager.Instance.PauseTimer()`) and disables player input while open.

### 🧗 6. 120° Free Rotation Elastic Rope Physics (`PlayerRopeSwing.cs`, `Rope.cs`, `RopeSegment.cs`)
- **Edit Mode Scene View Rendering (`[ExecuteAlways]`)**: Ropes generate and render 3 connected segments live in Scene View.
- **Tip-to-Tip HingeSockets**: Zero-gap connection between segment anchors.
- **360° Top Ceiling Anchor**: Top joint rotates freely (`allowFull360TopAnchor = true`).
- **120° Swing Arc**: Inner joints rotate within a 120-degree arc with zero resistance.
- **Launch Velocity Cap**: Clamps detach speed to `maxLaunchSpeed = 16f` for smooth launches.
- **Body Center Snapping**: Snaps player chest/hands cleanly onto the rope handle.

### 💨 7. Directional Wind Fan System (`WindFan2D.cs`)
- Updrafts and horizontal wind force fields with vertical gravity compensation.
- Integrated directly into `PlayerController2D.cs` physics update loop.

### ⚡ 8. Fixed-Direction Laser Emitters & Cover Blocking (`LaserEmitter2D.cs`, `LaserBeam2D.cs`)
- **Fixed Nozzle Direction**: Laser machine targeting rotation removed. Emitter stays locked in its scene placement orientation, periodically shooting laser blasts straight along its nozzle direction (`transform.right`).
- **Laser Blast Beam (`LaserBeam2D.cs`)**: Auto-destroys after `2.5s` lifetime or on impact with walls, push boxes, or player. Includes frame linecasting to prevent fast beam clipping.

### 📦 9. Pushable / Overhead Carry Box as Laser Cover (`PushBox2D.cs`, `PressurePlate2D.cs`)
- **Laser Cover Shield**: Walking behind a `PushBox2D` or carrying it overhead blocks incoming laser beams. The beam collides with the box and gets destroyed on impact, keeping the player behind it 100% safe.
- **PushBox2D**: Walk into it to push, or press `E` to pick up overhead (uses `Physics2D.IgnoreCollision` to keep the collider active for laser protection). Press `E` again to drop or toss it forward.
- **PressurePlate2D**: Weight-activated button plate triggered by player or push box.

### 🔑 10. Key Collectible & Weapon System (`KeyCollectible2D.cs`, `PlayerGun2D.cs`, `Bullet2D.cs`)
- Collecting key sets `HasSpecialKey = true` and `IsGunUnlocked = true` in `GameManager.cs`.
- Unlocks gun weapon firing via `F` or Left Click.

---

## 3. Planned Features & Development Roadmap

### 🎮 Main Menu, Difficulty Selection & Credits Page
- **Menu Scene**: Using `SampleScene.unity` or a dedicated menu scene as Scene 0 in Unity Build Settings.
- **Difficulty Selection**:
  - Easy / Normal / Hard modes adjusting timer length (e.g., 7:00 vs 5:00 vs 3:00) or hazard fire rates.
- **Credits Page**: Showcase team members and asset creators.

### 🚨 Endgame Delivery Failure Popup & Flow
- **Delivery Fail Popup**: When timer hits `0:00`, trigger a dramatic Game Over popup:
  - Text: *"You won't be able to complete the delivery!"*
  - Controls: Press `Enter` to Restart or Return to Main Menu.

### 🍕 Roof Delivery & Boss Encounter (Floor 9)
- Final delivery location on the roof of Floor 9.
- Mini-Boss encounter requiring bullet shooting and box throwing mechanics to reach the customer!

---

## 4. Unity Project Architecture

```
Assets/
├── Animations/          # Player & Fan animation clips
├── Audio / Sounds/      # Footsteps, Jumps, Landings, Death SFX, Background music (fight.ogg)
├── Prefabs/             # Drag-and-drop prefabs (Player, Rope, Fan, Laser, Ground, PushBox, Key, etc.)
├── Scenes/
│   ├── Level.unity      # Main vertical gameplay scene (Scene 1 in Build Settings)
│   ├── SampleScene.unity# Menu / Difficulty / Credits scene (Scene 0 in Build Settings)
│   └── Level/           # Global Volume Profile (Retro CRT Post-Processing)
├── Scripts/
│   ├── Camera/          # CameraController2D.cs (16-unit Y-snapping & bounds)
│   ├── Environment/     # FloorManager, SpawnPoint, Rope, WindFan2D, PushBox2D, PressurePlate2D,
│   │                    # KeyCollectible2D, LaserEmitter2D, LaserBeam2D, CrumblingTilemap2D, Hazard2D
│   ├── Managers/        # GameManager.cs, TimerManager.cs, AudioManager.cs
│   ├── Player/          # PlayerController2D, PlayerAnimator, PlayerRopeSwing, PlayerInteract,
│   │                    # PlayerGun2D, Bullet2D
│   ├── Tutorial/        # TutorialTrigger.cs (Interactable UI popup)
│   └── UI/              # HUDController.cs
├── Settings/            # InputSystem_Actions, URP Global Settings
└── tiles / backgrounds/ # Industrial pixel art tilemaps & city backgrounds
```

---

## 5. Development Progress & Log

| Date | Phase / Feature | Status | Notes |
| :--- | :--- | :---: | :--- |
| 2026-07-23 | Project Architecture & Core Design | ✅ Completed | Created initial C# specifications and mechanics outline. |
| 2026-07-23 | Core C# Scripts Implementation | ✅ Completed | Implemented movement, jump physics, rope swing, and hazard mechanics. |
| 2026-07-25 | Out-of-Frustum Gizmos Clean Fix | ✅ Completed | Switched to `OnDrawGizmosSelected()` across scripts resolving editor warnings. |
| 2026-07-26 | Single-Scene Architecture & Systems | ✅ Completed | Stacked floors in `Level.unity`, added `FloorManager.cs`, `AudioManager.cs`, `TutorialTrigger.cs`, and CRT post-processing. |
| 2026-07-26 | Build Settings & SpawnPoint Cleanup | ✅ Completed | Synchronized `EditorBuildSettings.asset` (adding `Level.unity`), cleaned spawn point naming in `Level.unity`. |
