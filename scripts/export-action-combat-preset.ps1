param(
    [string]$ConfigRoot = "$env:APPDATA\XIVLauncher\pluginConfigs\AetherUI\AetherUI",
    [string]$OutputPath = "$PSScriptRoot\..\DelvUI\Media\Presets\ActionCombat.delvui"
)

& "$PSScriptRoot\export-preset.ps1" -PresetName ActionCombat -ConfigRoot $ConfigRoot -OutputPath $OutputPath
