# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

SBA Tokyo — 3D mobile endless runner prototype built in Unity 6000.4.1f1 with URP 17.4.0.  
Active scene: `Assets/Scenes/RunnerPrototype.unity`

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
- Use **SBA Tokyo → Build Prototype Scene** (menu) to regenerate the scene via `PrototypeSceneBuilder.cs`
- Play in the Editor with keyboard fallback (see Input below)
- No automated test suite exists yet; validation is manual playtest against `docs/specs/prototype_gdd.md`

## Architecture

### Namespaces and folders

```
SBATokyo.Prototype.Core      →  Assets/Scripts/Core/
SBATokyo.Prototype.Gameplay  →  Assets/Scripts/Gameplay/
(UI scripts live in)         →  Assets/Scripts/UI/
```

### Data flow

```
PrototypeInputManager  →  events  →  PrototypeRunnerController
                                   →  PrototypeGameManager (Push, Restart)

PrototypeRunnerController  →  PrototypeGameManager.RegisterLandingResult()
                           →  PrototypeGameManager.SetState()

PrototypeGameManager  →  events (RunReset, GameOverTriggered)  →  ObstacleSpawner, HUD
```

`PrototypeGameManager` owns all authoritative game state (speed, score, combo, `PlayerState`).  
`PrototypeRunnerController` owns all physical movement and landing window logic.  
These two never call each other's setters except through the defined public API.

### Key design contracts

- **Landing window**: `PrototypeRunnerController` buffers a `PrototypeLandingActionType` pre-jump and passes it with `elapsedSinceLanding` to `GameManager.RegisterLandingResult()` on landing. The manager decides success based on surface type.
- **Surface typing**: `PrototypeSurface` component on scene objects carries a `PrototypeSurfaceType`. `RunnerController` reads it via downward raycast (`ProbeGround`). Rail surfaces pause speed decay and set `PlayerState.Grinding`.
- **Object pooling**: `ObstacleSpawner` maintains a fixed pool of 12 obstacles, recycled by Z position. Reset via `RunReset` event.
- **Speed decay**: passive decay starts after `idleDecayDelay` seconds of no push. Rails pause decay. Game over when speed reaches 0.

### Forward-compatibility notes (do not remove)

- `PlayerState` enum must remain the stable contract for full SBA Tokyo expansion (`Airborne`, `Grinding`, `LandingWindow` are already live).
- `PrototypeSurfaceType` (`Ground`, `Object`, `Rail`, `Obstacle`) must not be renamed; future landing rules branch on these values.

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

Defined in `PrototypeGameManager` inspector fields and documented in `docs/specs/prototype_gdd.md`.  
Change values in the Inspector; do not hardcode them in other scripts.

| Parameter | Default |
|---|---|
| Start speed | 8.0 |
| Max speed | 20.0 |
| Push amount | 2.0 |
| Idle decay delay | 1.0 s |
| Speed decay/sec | 2.0 |
| Landing window | 0.15 s |
| Perfect window | 0.08 s |

## Document Map

- `AGENTS.md` — operating rules and harness entrypoint
- `docs/specs/prototype_gdd.md` — in-scope features and success criteria
- `docs/specs/full_game_3d_roadmap.md` — 3-phase production roadmap
- `docs/plans/prototype_plan.md` — milestone breakdown (M1–M4 complete)
- `docs/reports/` — QA notes and status reports
