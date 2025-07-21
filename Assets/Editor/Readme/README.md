# ZShopping TBS

A multiplayer turn-based strategy game built with Unity 2022 LTS and Universal Render Pipeline.

## Features
- Procedural field generation with configurable obstacles and spawn zones
- Two unit types: slow long-range and fast short-range
- NavMesh-based pathfinding and move/attack actions
- Turn system: one move + one attack or early end, 60 sec per turn, turn counter
- Multiplayer via Netcode for GameObjects (one Host + many Clients)
- Simple UI: timer, turn number, current player, action availability
- In-game buttons to Host or Connect (with IP/Port field)
- Optional: draw attack ranges, resolve draws after 15 turns

## New Since Networking Integration
### Networking
* Added `NetworkManagerInitializer` that auto-creates/uses a `NetworkManager` with Unity Transport.  
* All dynamic tiles, obstacles, units and ragdolls now carry `NetworkObject`.  
* `FieldGenerator` generates map only on the Host; Clients clear local scene and receive spawned objects from the server.  
* Turn logic, timer and unit state replicated via `NetworkVariables`.  

### Camera & UX
* RTS-style camera (`CameraController`) – WASD / edge pan, scroll zoom, RMB rotate.  
* Interactive network UI (`NetworkUI`) with `Host Game` / `Connect` buttons and IP/Port input.  

### Quality-of-life
* `noSpawnZones` colliders are enabled only during generation, preventing click blocking during gameplay.  

## Recent Updates
- Integrated `FieldGenerator` with auto NavMeshSurface baking for procedural fields
- Added `UnitBase`, `LongRangeUnit`, and `ShortRangeUnit` classes for unit movement and combat
- Created `TurnManager` with unified timer, sequential turn order, and UI integration (turn text, timer, End Turn button)
- Implemented `HighlightManager` for dynamic move and attack range visualization based on NavMesh paths
- Updated move-highlights to only show navigable tiles using `NavMesh.CalculatePath`
- Renamed unit scripts to `ZShopping.Units` namespace for project consistency

## Project Setup
1. Install Unity 2022.3 LTS via Unity Hub
2. Open this project folder in Unity Hub
3. In Package Manager, install:
   - Netcode for GameObjects
   - UniTask (from OpenUPM registry)
   - Profiling Core (Unity Profiler Tools)
4. Create and assign URP Asset:
   - Create → Rendering → URP Asset (with Universal Renderer)
   - Edit → Project Settings → Graphics → Scriptable Render Pipeline Settings → select created URP Asset
5. Open `Assets/Scenes/Main.unity`, add a Directional Light and Plane/Terrain, then Save
6. Press Play → Host Game to start a local host or Build & Run and **Connect** from another instance.

## Folder Structure
```
Assets/
  Scenes/
    Main.unity
  Readme/
    README.md
  Scripts/
    FieldGenerator.cs
    Unit/
    Network/
Packages/
  manifest.json
  packages-lock.json
```

## Development Roadmap
1. Field generation (tile/obstacle spawner)
2. Unit system (base Unit class + two types)
3. Pathfinding and path preview
4. Attack logic and UI indicators
5. Turn management and timer
6. Networking with Netcode
7. Optional features: draws, IK, shaders
8. Optimization and profiling
9. Version control with Git (branching and commits) 