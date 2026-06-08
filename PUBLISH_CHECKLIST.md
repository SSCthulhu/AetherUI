# Publish Checklist

Use this checklist every time you publish an update for testers via Dalamud Experimental Repos.

## One-time setup (already done)

- `pluginmaster.json` exists at repo root.
- GitHub Actions workflow exists at `.github/workflows/release.yml`.
- Testers use:
  - `https://raw.githubusercontent.com/SSCthulhu/FFXIVHudReimagined/main/pluginmaster.json`

## Per-release checklist

1. **Pick the next version number**
   - Example: `0.0.66`
   - Tag format must match: `v0.0.66`

2. **Update version fields**
   - In `FFXIVHudPlugin.csproj`, set:
     - `<Version>0.0.66</Version>`
   - In `pluginmaster.json`, set:
     - `"AssemblyVersion": "0.0.66.0"`

3. **Commit and push `main`**
   - `git add "FFXIVHudPlugin.csproj" "pluginmaster.json"`
   - `git commit -m "Bump plugin version to 0.0.66 for release."`
   - `git push origin main`

4. **Create and push release tag**
   - `git tag v0.0.66`
   - `git push origin v0.0.66`

5. **Wait for GitHub Actions**
   - Open GitHub -> `Actions` -> `Build and Release Plugin`
   - Confirm latest run is green

6. **Verify release asset**
   - Open GitHub -> `Releases` -> `v0.0.66`
   - Confirm asset exists:
     - `FFXIVHudPlugin.zip`
   - Run package validation before publishing:
     - `pwsh -ExecutionPolicy Bypass -File .\scripts\validate-package.ps1`

7. **Smoke test install path**
   - In-game `/xlsettings` -> `Experimental` -> ensure repo URL is present
   - Plugin installer can install/update `FFXIV Hud Reimagined`

## Quick command template

Replace `0.0.66` with your next version.

```powershell
cd "F:\Game Development\FFXIV Plugins\ffxiv-dalamud-hud"
pwsh -ExecutionPolicy Bypass -File .\scripts\validate-package.ps1
git add "FFXIVHudPlugin.csproj" "pluginmaster.json"
git commit -m "Bump plugin version to 0.0.66 for release."
git push origin main
git tag v0.0.66
git push origin v0.0.66
```

## If release fails

- Check `Actions` logs for the failed step.
- Fix on `main`, push, then create a **new** tag (do not reuse failed tag), e.g.:
  - `v0.0.66.1`

## Packaging guardrails

Always ensure the release zip contains all of the following files:

- `FFXIVHudPlugin.dll` (legacy assembly still emitted)
- `FFXIVHudPlugin.json` (legacy manifest still emitted)
- `FFXIVHudReimagined.dll` (Dalamud loader target)
- `FFXIVHudReimagined.deps.json` (Dalamud dependency graph for alias)
- `FFXIVHudReimagined.json` (manifest with `InternalName: FFXIVHudReimagined`)

The validation script checks:

- csproj `<Version>` matches manifest `AssemblyVersion`
- `FFXIVHudPlugin.json` and `FFXIVHudReimagined.json` stay in sync
- both manifests keep `InternalName` set to `FFXIVHudReimagined`
- `pluginmaster.json` continues to point at the same GitHub release zip path
- required files are present in `bin\Release\FFXIVHudPlugin\latest.zip`
