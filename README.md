# Tower Defense Lab — Unity/C# Portfolio Prototype

I built this Unity prototype to experiment with tower placement, pathfinding, and enemy waves. I used Chapter 26, “Tower Defense Design and Outline,” from Casey Hardman’s *Game Programming with Unity and C#* as my starting point.

## Current vertical slice

- Build mode and wave-based play mode.
- Grid-snapped placement for barricades, arrow towers, cannon towers, and hot plates.
- Breadth-first pathfinding that rejects placements which block the spawn-to-goal route.
- Ground enemies that follow the computed route and leak at the goal.
- Gold, lives, round progression, wave rewards, target selection, direct damage, and area damage.
- Unity UI for selection, status, resources, and wave control.

## Run it

Open the project in Unity **2022.3.22f1** and open `Assets/Scenes/Main.unity`. The standalone build is configured for a 1024×768 window so it can be reviewed and recorded without taking over the desktop.

Select a tower with the UI or `B`, `A`, `C`, or `H`, then click a grid tile to place it. Press `SPACE` or `START WAVE` to begin a round. Placements that would block the route are rejected.

## Portfolio evidence

| Role requirement | Demonstrated by |
| --- | --- |
| Unity/C# | Runtime scene construction, component-based app, reusable tower/enemy classes |
| OOP and inheritance path | Separate tower and enemy domain types with shared behaviors and specialized effects |
| Pathfinding | BFS route search and placement validation |
| Collision and interaction | Grid placement, ray-plane selection, primitive colliders, enemy goal leaks |
| UI | Canvas status bar, tower selection, start-wave control, live resources |
| Maintainability | Small vertical slice with explicit states, data types, and focused methods |

## Source attribution

Planning concepts are informed by Casey Hardman, *Game Programming with Unity and C#*, Chapter 26, “Tower Defense Design and Outline” (2020), DOI: https://doi.org/10.1007/978-1-4842-5656-5_26. This project is an original portfolio adaptation and does not copy chapter code or assets.

## Verification status

This source snapshot was checked for JSON validity, Unity asset/meta pairs and scene GUID references. Unity compilation, standalone builds and gameplay were not rerun during export preparation. Historical build results and remaining checks are recorded in [VERIFICATION.md](VERIFICATION.md).

**Known issue:** the previously built player fails at startup because the Standard shader is missing. The included builder now creates a Resources material to retain that shader, but a fresh build and gameplay retest are still pending. The last recorded rebuild attempt was blocked by the Unity Editor license. Treat this as a development prototype.

## Next steps

1. Add an automated test assembly for pathfinding, placement validation, and wave transitions.
2. Add sell mode and persistent tower data.
3. Add a WebGL build and profiler evidence.
4. Replace primitive visuals with original or properly licensed art.
