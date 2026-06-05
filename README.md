# FFXIV Hud Reimagined

A Dalamud plugin that replaces the default FFXIV HUD with a custom layout: center HP orb, MP ring, mirrored hotbars, status lanes, class gauge, limit break display, and an optional custom minimap.

## Requirements

- Final Fantasy XIV with [Dalamud](https://github.com/goatcorp/Dalamud) installed
- .NET SDK matching the project target (see `FFXIVHudPlugin.csproj`)

## Build

```bash
dotnet build -c Debug
```

Copy or symlink the output from `bin/Debug/` into your Dalamud dev plugins folder, then reload in-game with `/xlreload`.

## Configuration

Open the plugin config from the Dalamud plugin installer or use the in-game command defined in `PluginCommands.cs`.

## Experimental Repo Install (No Build Needed)

For friends/testers, you can install this plugin directly from Dalamud's Experimental Repos without cloning or building.

- Add this URL in `/xlsettings` -> `Experimental` -> `Custom Plugin Repositories`:
  - `https://raw.githubusercontent.com/SSCthulhu/FFXIVHudReimagined/main/pluginmaster.json`
- Open Dalamud plugin installer and install `FFXIV Hud Reimagined`.

### Publishing updates for testers

1. Bump `<Version>` in `FFXIVHudPlugin.csproj`.
2. Commit and push to `main`.
3. Create and push a matching git tag (example: `v0.0.66`):
   - `git tag v0.0.66`
   - `git push origin v0.0.66`
4. GitHub Actions publishes `FFXIVHudPlugin.zip` to Releases automatically.
5. Dalamud users get install/update from the same experimental repo URL.

## Action Camera Plugin (Standalone Module)

This repository now includes an isolated Action Camera feature that does not alter existing HUD logic.

- **Command**: `/actioncam` toggles action camera mode.
- **Settings tab**: `Action Camera` tab in the existing config window.
- **Direct camera path**: uses `FFXIVClientStructs` scene camera look-vector writes through `ICameraProvider`.
- **Fallback strategy**: camera access is abstracted behind `ICameraProvider` for game-version-safe replacement.

### Behavior

- Locks and hides cursor while action camera is active.
- Reads raw mouse delta each frame and applies yaw/pitch updates.
- Holding Alt temporarily releases cursor (configurable).
- UI visibility can auto-release cursor (configurable).
- Pressing Escape disables action camera until reactivated (right click by default, configurable).
- Separate horizontal and vertical sensitivity (`0.1` to `5.0`).
- Optional center reticle and debug overlay.

### Architecture

- `ActionCameraPlugin`: lifecycle/state orchestration.
- `InputManager`: key and mouse delta capture.
- `UiStateService`: common UI-focus detection.
- `CursorManager`: show/hide, lock/unlock, recenter.
- `CameraController`: yaw/pitch integration and clamping.
- `ICameraProvider` / `FfxivClientStructsCameraProvider`: direct camera reads/writes.
- `ActionCameraOverlay`: optional reticle/debug rendering.

## Version history

See git commits and tags. Bump the version in `FFXIVHudPlugin.csproj` for each release you test in-game.

## License

Add a license file when you choose one for this repository.
