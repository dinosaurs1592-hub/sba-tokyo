# SBA Tokyo 3D Roadmap

## Direction

The prototype remains intentionally simple, but the production target is a full 3D mobile runner. All prototype systems should stay compatible with a character viewed in perspective, lane-based world geometry, and authored 3D chunks.

## Phase 1: Prototype Validation

- validate lane readability in perspective view
- validate push-to-maintain-speed loop
- validate obstacle timing and restart loop

## Phase 2: SBA Tokyo Core

- introduce landing windows
- split surfaces into `Ground`, `Object`, and `Rail`
- add jump and slide-jump behavior
- add combo continuation rules

## Phase 3: 3D Production Layer

- replace primitive shapes with modular 3D environment kits
- author urban road chunks, vehicles, rails, and skyline props
- move camera from prototype follow to polished chase camera
- add animation controller, VFX, and motion-driven feedback

## Technical Rule

Prototype code should avoid assumptions that only fit a 2D game. Systems should prefer:

- world-space lanes instead of 2D transforms
- explicit surface metadata
- state-based movement logic
- reusable 3D chunk-friendly spawning
