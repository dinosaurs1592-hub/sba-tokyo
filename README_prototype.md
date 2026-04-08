# SBA Tokyo Prototype

Prototype project for validating the core SBA Tokyo loop before full production.

## Goal

Prove the feel of:

- 3-lane movement
- speed pressure
- push-to-maintain-speed input
- obstacle avoidance basics

## Editor

- Unity `6000.4.1f1`

## First Build Scope

- auto-runner movement
- lane switching
- down swipe or keyboard fallback push
- speed decay
- game over at zero speed
- simple obstacle collision
- simple HUD

## Project Layout

- `Assets/Scenes/`: Unity scenes
- `Assets/Scripts/Core/`: game state and bootstrapping
- `Assets/Scripts/Gameplay/`: runner, obstacles, spawners
- `Assets/Scripts/UI/`: HUD and game over display
- `docs/specs/`: prototype and game specs
- `docs/plans/`: milestone plans
