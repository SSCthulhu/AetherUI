param(
    [string]$ConfigRoot = "$env:APPDATA\XIVLauncher\pluginConfigs\AetherUI\AetherUI"
)

function Read-ConfigJson {
    param([string]$RelativePath)
    $path = Join-Path $ConfigRoot $RelativePath
    if (-not (Test-Path $path)) { return $null }
    return Get-Content $path -Raw | ConvertFrom-Json
}

$keyFiles = @{
    'PlayerParameterOrbConfig' = 'Player Parameter Orb\Player Parameter Orb.json'
    'MinimapConfig' = 'Minimap\General.json'
    'PartyFramesConfig' = 'Party Frames\General.json'
    'EnemyListConfig' = 'Enemy List\General.json'
    'PlayerBuffsListConfig' = 'Buffs and Debuffs\Player Buffs.json'
    'PlayerDebuffsListConfig' = 'Buffs and Debuffs\Player Debuffs.json'
    'JobBarsGeneralConfig' = 'Job Specific Bars\General.json'
    'HUDOptionsConfig' = 'Misc\HUD Options.json'
    'NameplatesGeneralConfig' = 'Nameplates\General.json'
}

Write-Host "=== Live config summary ==="
Write-Host "ConfigRoot: $ConfigRoot"
foreach ($entry in $keyFiles.GetEnumerator()) {
    $obj = Read-ConfigJson $entry.Value
    if ($null -eq $obj) {
        Write-Host "  $($entry.Key): MISSING"
        continue
    }
    $enabled = if ($null -ne $obj.PSObject.Properties['Enabled']) { $obj.Enabled.ToString().ToLower() } else { 'n/a' }
    $pos = if ($null -ne $obj.PSObject.Properties['Position']) { "($($obj.Position.X), $($obj.Position.Y))" } else { '' }
    $extra = ''
    if ($entry.Key -eq 'HUDOptionsConfig' -and $null -ne $obj.PSObject.Properties['HideDefaultJobGauges']) {
        $extra = " HideDefaultJobGauges=$($obj.HideDefaultJobGauges.ToString().ToLower())"
    }
    Write-Host "  $($entry.Key) Enabled=$enabled pos=$pos$extra"
}

$sage = Read-ConfigJson 'Job Specific Bars\Healer\Sage.json'
if ($null -ne $sage -and $null -ne $sage.PSObject.Properties['Position']) {
    Write-Host "  SageConfig (sample job) pos=($($sage.Position.X), $($sage.Position.Y)) Enabled=$($sage.Enabled.ToString().ToLower())"
}
