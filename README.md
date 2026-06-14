# Aether UI

Aether UI is a Dalamud plugin for Final Fantasy XIV that provides a standalone, highly customizable HUD and UI framework focused on profile safety, consistent behavior, and parity with modern utility features.

This repository hosts both the plugin source and the custom-repo distribution metadata used by testers.

## What Aether UI Includes

- Full HUD replacement and configuration framework
- Advanced job HUD support from the DelvUI lineage
- Standalone Action Camera module
- Nameplate systems and extension points
- Profile import/export and safer configuration handling
- Custom media support (fonts, textures, profile templates)

## Commands

- `/aetherui` - Open the Aether UI configuration window
- `/aui` - Alias for opening configuration
- `/aetherui toggle` - Toggle HUD visibility
- `/aetherui show` / `/aetherui hide` - Explicit HUD visibility control
- `/aetherui actioncam <on/off/toggle>` - Control Action Camera

## Installation (Dalamud Custom Repo)

1. Open `/xlsettings`
2. Navigate to `Experimental` -> `Custom Plugin Repositories`
3. Add:
   - `https://raw.githubusercontent.com/SSCthulhu/AetherUI/main/pluginmaster.json`
4. Refresh plugin lists in Dalamud Plugin Installer
5. Install `Aether UI`

## Repository Layout

- `DelvUI/` - main plugin source (Aether UI forked codebase)
- `AetherUI.json` - distribution manifest aligned with release
- `pluginmaster.json` - Dalamud custom repo entry
- `AetherUI.zip` - release package used for install/update flow
- `scripts/validate-package.ps1` - release metadata/package validation

## Build (Local)

```bash
dotnet build ./DelvUI/DelvUI.csproj -c Release
```

For release sanity checks:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\validate-package.ps1
```

## Fork and Attribution

Aether UI is a forked derivative of DelvUI and contains modified code from that upstream project and other credited open-source components.

## License

This project is distributed under the GNU Affero General Public License v3.0 (AGPL-3.0-or-later).  
See `LICENSE` for full terms.

In accordance with AGPL requirements, the complete corresponding source for distributed builds is provided in this repository.

## Notices

- This project includes code and assets that retain original notices and attributions where required.
- Final Fantasy XIV and related marks are trademarks of Square Enix.
