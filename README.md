# ZShopping TBS

Turn-based strategy test project built with Unity 2022 LTS and Universal Render Pipeline (URP). This project demonstrates procedural field generation, turn-based combat, and basic multiplayer integration using Netcode for GameObjects.

## Overview

ZShopping TBS is a prototype for a turn-based tactics game with the following core features:

- Procedural field generation with biome-based floor variations and random obstacle placement.
- Team-based turn sequencing with a shared timer per team (60 seconds).
- Dynamic move and attack highlights with line-of-sight checks.
- Server-authoritative turn logic and action handling for multiplayer.

## Setup

1. Open `ZShopping_TBS.sln` in Unity 2022 LTS.
2. Ensure the following Unity packages are installed via Package Manager:
   - Universal RP
   - Netcode for GameObjects
   - Unity Transport (or Unity Relay/Transport solution)
   - UniTask (for future async logic)
   - Profiling Core (for performance analysis)
3. Open the `NetworkDemo` scene (or the main sample scene) and press Play.

## Current Implementation Status

### Procedural Field Generation
- Supports multiple biomes (`BrokenRoad`, `DestroedPlaza`), each with a list of floor prefabs.
- Random obstacle distribution with uniform spacing.
- Server-authoritative generation: only the host builds the field; network spawns synchronize objects.

### Turn-Based Combat
- `TurnManager` is a `NetworkBehaviour`: turn state (`currentTeam`, `currentUnit`, `timer`) uses `NetworkVariable<T>`.
- Clients receive updates via `OnValueChanged` callbacks and highlight the active unit.
- Move and attack actions are requested by clients via `[ServerRpc]` methods:
  - `MoveServerRpc(Vector3 position)`
  - `AttackServerRpc(ulong targetId)`
- Server sets `usedMove`/`usedAttack`, executes the action, and automatically ends the unit’s turn:
  - After attack: immediate turn end.
  - After move: automatic turn end if no valid targets remain.

### Highlights and UI
- Move and attack ranges are highlighted for both local and remote clients.
- UI elements (`TurnText`, `TimerText`, `EndTurnButton`) sync with networked state.

## Next Steps

1. **Network Setup UI**: Create in-game UI for Host/Client with IP/Port input.
2. **NetworkManager Configuration**: Setup a persistent `NetworkManager` prefab and configure transport layers.
3. **NetworkObject Prefabs**: Add `NetworkObject` and `NetworkTransform` to all unit and obstacle prefabs; register in `NetworkManager`.
4. **UnitBase Refinement**: Expose health and state via `NetworkVariable<T>`; implement `[ClientRpc]` for animations and damage feedback.
5. **Interactive Camera Controls**: Implement WASD/screen-edge panning, scroll-to-zoom, and right-click rotation.
6. **Field Generation Sync**: Ensure deterministic obstacle and floor instantiation across clients or full server spawning.
7. **End-to-End Multiplayer Test**: Test host/client workflows, movement replication, attack events, and end-game conditions.
8. **Optimization & Pooling**: Add object pooling for units and obstacles; profile with Profiling Core.

## Project TODO

- net1: Set up a `NetworkManager` prefab/scene with Netcode and transport.
- net2: Add `NetworkObject` and `NetworkTransform` to unit prefabs and no-spawn zones.
- net3: Refactor `UnitBase` to use `NetworkVariable` for health and walking state; add server RPCs for actions.  
- net4: Complete client UI syncing and input handling via RPC.  
- net5: Implement Biome selection and UI adjustment.  
- net6: Interactive camera implementation.  
- net7: Full synchronization of field generation.  
- net8: Final testing and polish.

---

Ready for the next phase of development! Feel free to update or refine the plan as needed.
