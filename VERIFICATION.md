# Verification

This file records the current evidence boundary for the Tower Defense Lab portfolio prototype.

## Previously recorded evidence (not rerun for this export)

- Unity version: `2022.3.22f1`
- `ProjectBuilder.BuildProject`: passed; scene marker `TOWER_DEFENSE_SCENE_CREATED`
- `ProjectBuilder.BuildStandalone`: passed; marker `TOWER_DEFENSE_BUILD_RESULT:Succeeded`
- Standalone output: `Builds/Linux/TowerDefenseLab.x86_64` (ignored by Git)
- MIT license present with Christian Busca attribution
- Standalone player configured for a 1024×768 window

## Not yet verified

- Interactive gameplay in the standalone player. The 2026-08-15 build starts but
  `TowerDefenseApp.Start()` fails while creating its first material because
  `Shader.Find("Standard")` returns null in the player. This is an
  `ArgumentNullException` in `Material.CreateWithShader`; no wave or placement
  gameplay was reached.
- Automated play-mode coverage for pathfinding and wave transitions
- WebGL build
- Unity Profiler baseline

## 2026-10-07 test attempt

- Launched the existing `Builds/Linux/TowerDefenseLab.x86_64` on Linux. The
  runtime log confirms the missing `Standard` shader failure above.
- Updated `ProjectBuilder.BuildStandalone` to create a Resources material that
  references the shader so Unity includes it in the next build. The builder
  also accepts `TOWER_DEFENSE_BUILD_DIR` to preserve the previous player.
- A new build and gameplay retest remain pending. Unity 2022.3.22f1 exits
  before opening the project because no active Editor license is available on
  this machine (`No valid Unity Editor license found`).

## Reproduce

Set `UNITY_EDITOR` to your Unity 2022.3.22f1 executable, then run from the project root. An active Unity license is required.

```bash
"$UNITY_EDITOR" \
  -batchmode -nographics \
  -projectPath "$PWD" \
  -executeMethod ProjectBuilder.BuildProject -quit

"$UNITY_EDITOR" \
  -batchmode -nographics \
  -projectPath "$PWD" \
  -executeMethod ProjectBuilder.BuildStandalone -quit
```

## Source export review — 2026-11-11

Assets (including .meta files), Packages, ProjectSettings and documentation were selected. Generated caches, diagnostics, player binaries and prior Git history are excluded. Static JSON, meta-pair and scene GUID checks passed. Unity compilation, builds and gameplay were not rerun for this export.
