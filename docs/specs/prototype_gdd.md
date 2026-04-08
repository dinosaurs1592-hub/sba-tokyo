# SBA Tokyo Prototype GDD

## Objective

Build a minimal 3-lane runner to validate the core SBA Tokyo feel before implementing landing windows, advanced gestures, and combo systems.

## In Scope

- automatic forward movement
- 3-lane switching
- down swipe push input
- speed decay after inactivity
- obstacle collision and game over
- distance and speed HUD
- future-ready player state tracking
- explicit surface typing for later SBA Tokyo rules

## Out of Scope

- jump chains
- landing window logic
- rail logic
- combo continuation
- counter swipes
- auto-drive
- full chunk generation

## Tuning Baseline

- lane width: `3.0`
- start speed: `8.0`
- max speed: `20.0`
- push amount: `2.0`
- idle decay delay: `1.0`
- speed decay per second: `2.0`

## Success Criteria

The prototype succeeds if the player can immediately understand the loop and feels real pressure to maintain speed.

## Forward Compatibility Notes

- `PlayerState` is introduced now so full SBA Tokyo can expand into `Airborne`, `Grinding`, and `LandingWindow` without refactoring the entire loop.
- `PrototypeSurfaceType` is introduced now so later `Ground`, `Object`, and `Rail` behavior can branch from shared surface metadata.
