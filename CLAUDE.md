# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

SBA Tokyo — 3D mobile endless runner built in Unity 6000.4.1f1 with URP 17.4.0.  
Active scene: `Assets/Scenes/GameScene.unity`

## Operating Model

Follow the harness defined in `AGENTS.md`:

- **Secretary**: intake, task structure, risk reporting
- **Builder**: implementation, scripts, scene changes
- **Evaluator**: spec checks against `docs/specs/`, regression review

Every task must resolve to: objective / owner / output / completion check / blockers.  
Label suggestions as `Optional`, `Recommended`, or `Urgent`.

## Build and Development

This is a Unity project — there is no CLI build or test command. All iteration happens inside the Unity Editor.

- Open the project in Unity `6000.4.1f1`
- Use **SBA Tokyo → Build Game Scene** (menu) to regenerate the scene via `SBASceneBuilder.cs`
- Play in the Editor with keyboard fallback (see Input below)
- No automated test suite exists yet; validation is manual playtest against `docs/specs/prototype_gdd.md`

## Architecture

### Namespaces and folders

```
SBATokyo.Core      →  Assets/Scripts/Core/      (SBAGameManager, SBAInputManager, PlayerState)
SBATokyo.Gameplay  →  Assets/Scripts/Gameplay/  (RunnerController, CameraFollow, RoadSurface,
                                                  Obstacle, ChunkSpawner, ChunkController,
                                                  SurfaceType, LandingActionType)
SBATokyo.UI        →  Assets/Scripts/UI/        (GameHUD)
```

`Assets/Scripts/Archive/` contains the original `Prototype*` scripts for reference only.

### Data flow

```
SBAInputManager  →  events  →  RunnerController
                            →  SBAGameManager (Push, Restart)

RunnerController  →  SBAGameManager.RegisterLandingResult()
                  →  SBAGameManager.SetState()

SBAGameManager  →  events (RunReset, GameOverTriggered)  →  ChunkSpawner, GameHUD
```

`SBAGameManager` owns all authoritative game state (speed, score, combo, `PlayerState`).  
`RunnerController` owns all physical movement and surface detection.  
These two never call each other's setters except through the defined public API.

### Key design contracts

- **Landing window**: `RunnerController` buffers a `LandingActionType` pre-jump and passes it with `elapsedSinceLanding` to `SBAGameManager.RegisterLandingResult()` on landing.
- **Surface typing**: `RoadSurface` component on scene objects carries a `SurfaceType`. `RunnerController` reads it via downward raycast (`ProbeGround`). Rail surfaces pause speed decay and set `PlayerState.Grinding`.
- **Chunk pooling**: `ChunkSpawner` references real `.prefab` assets in `Assets/Prefabs/Chunks/`. Each chunk is managed by `ChunkController`. Reset via `RunReset` event.
- **Scene builder**: `SBASceneBuilder` saves chunk prefabs via `PrefabUtility.SaveAsPrefabAsset()` before scene creation, ensuring `Instantiate` works correctly in Play mode.
- **Speed decay**: passive decay starts after `idleDecayDelay` seconds of no push. Rails pause decay. Game over when speed reaches 0.

### Forward-compatibility notes (do not remove)

- `PlayerState` enum must remain stable (`Airborne`, `Grinding`, `LandingWindow` are live).
- `SurfaceType` (`Ground`, `Object`, `Rail`, `Obstacle`) must not be renamed; landing rules branch on these values.

## Input Reference (Editor)

| Action | Keys |
|---|---|
| Lane left / right | A/D or ←/→ |
| Push (speed up) | S or ↓ |
| Jump | Space, W, or ↑ |
| Tap (landing action) | Enter |
| Rail trick | F |
| Restart | R |

Mouse drag (60px minimum) also works in editor: horizontal = lane change, swipe down = push, swipe up = jump.

## Tuning Baselines

Defined in `SBAGameManager` inspector fields. Change values in the Inspector; do not hardcode them.

| Parameter | Default |
|---|---|
| Start speed | 24.0 |
| Max speed | 52.0 |
| Push amount | 6.0 |
| Idle decay delay | 1.2 s |
| Speed decay/sec | 3.4 |
| Landing window | 0.15 s |
| Perfect window | 0.08 s |

## Document Map

- `AGENTS.md` — operating rules and harness entrypoint
- `docs/contracts/2026-04-14-sba-tokyo-rebuild.md` — rebuild design contract
- `docs/specs/prototype_gdd.md` — feature specs (still valid for mechanics reference)
- `docs/specs/full_game_3d_roadmap.md` — 3-phase production roadmap
- `docs/reports/` — QA notes and status reports
