# Publish Checklist (Aether UI)

Use this checklist every time you publish an update for testers via Dalamud Custom Plugin Repositories.

## One-time setup (already done)

- `pluginmaster.json` exists at repo root and points to `AetherUI.zip`.
- GitHub Actions workflow exists at `.github/workflows/release.yml`.
- Testers use:
  - `https://raw.githubusercontent.com/SSCthulhu/FFXIVHudReimagined/main/pluginmaster.json`

## Per-release checklist

1. **Pick the next version number**
   - Example: `2.7.0.2`
   - Tag format example: `v2.7.0.2-aether.0`

2. **Build the source plugin release package**
   - Build from the Aether UI source repository.
   - Ensure the release package is named `AetherUI.zip`.

3. **Update distribution metadata**
   - In `pluginmaster.json`, set:
     - `"AssemblyVersion"` to the released Aether UI version.
   - Confirm install/update links point to:
     - `.../releases/latest/download/AetherUI.zip`

4. **Commit and push `main`**
   - `git add "pluginmaster.json" "README.md" ".github/workflows/release.yml" "AetherUI.zip"`
   - `git commit -m "Publish Aether UI <version> release artifacts."`
   - `git push origin main`

5. **Create and push release tag**
   - `git tag v2.7.0.2-aether.0`
   - `git push origin v2.7.0.2-aether.0`

6. **Wait for GitHub Actions**
   - Open GitHub -> `Actions` -> `Build and Release Plugin`
   - Confirm latest run is green

7. **Verify release asset**
   - Open GitHub -> `Releases` -> latest tag
   - Confirm asset exists:
     - `AetherUI.zip`
   - Run package validation before tagging:
     - `pwsh -ExecutionPolicy Bypass -File .\scripts\validate-package.ps1`

8. **Smoke test install path**
   - In-game `/xlsettings` -> `Experimental` -> ensure repo URL is present
   - Plugin installer can install/update `Aether UI`

## Quick command template

Replace `v2.7.0.2-aether.0` with your next tag.

```powershell
cd "F:\Game Development\FFXIV Plugins\ffxiv-dalamud-hud"
pwsh -ExecutionPolicy Bypass -File .\scripts\validate-package.ps1
git add "pluginmaster.json" "README.md" ".github/workflows/release.yml" "AetherUI.zip"
git commit -m "Publish Aether UI release."
git push origin main
git tag v2.7.0.2-aether.0
git push origin v2.7.0.2-aether.0
```

## If release fails

- Check `Actions` logs for the failed step.
- Fix on `main`, push, then create a **new** tag (do not reuse failed tag).

## Packaging guardrails

Always ensure `AetherUI.zip` contains all required runtime files:

- `AetherUI.dll`
- `AetherUI.deps.json`
- `AetherUI.json`
- `Colourful.dll`
- `changelog.md`
- `LICENSE`
- `Media\...`
