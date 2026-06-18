param(
    [string]$ConfigRoot = "$env:APPDATA\XIVLauncher\pluginConfigs\AetherUI\AetherUI",
    [string]$OutputPath = "$PSScriptRoot\..\DelvUI\Media\Presets\Minimal.delvui"
)

& "$PSScriptRoot\export-preset.ps1" -PresetName Minimal -ConfigRoot $ConfigRoot -OutputPath $OutputPath
& "$PSScriptRoot\patch-minimal-hud-layout.ps1" -PresetPath $OutputPath
& "$PSScriptRoot\patch-minimal-misc.ps1" -PresetPath $OutputPath
& "$PSScriptRoot\patch-minimal-job-bar-positions.ps1" -PresetPath $OutputPath
& "$PSScriptRoot\patch-minimal-buffs-debuffs.ps1" -PresetPath $OutputPath
