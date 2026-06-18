param([string[]]$Presets = @('ActionCombat', 'Minimal'))

function Decode-Chunk {
    param([string]$Chunk)
    $bytes = [Convert]::FromBase64String($Chunk)
    $ms = New-Object IO.MemoryStream(,$bytes)
    $def = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Decompress)
    $out = New-Object IO.MemoryStream
    $def.CopyTo($out)
    return [Text.Encoding]::UTF8.GetString($out.ToArray())
}

function Get-ShortTypeName {
    param([string]$Json)
    if ($Json -match '"\$type":\s*"([^"]+)"') {
        return ($Matches[1] -split '\.')[-1] -replace ',.*', ''
    }
    return $null
}

$watchTypes = @(
    'PlayerParameterOrbConfig', 'MinimapConfig', 'JobBarsGeneralConfig',
    'ActionCameraConfig', 'SageConfig', 'PaladinConfig',
    'PlayerUnitFrameConfig', 'TargetUnitFrameConfig', 'HUDOptionsConfig',
    'PlayerBuffsListConfig', 'PlayerDebuffsListConfig',
    'NameplatesGeneralConfig', 'PartyFramesConfig', 'EnemyListConfig'
)

foreach ($preset in $Presets) {
    Write-Host "=== $preset ==="
    $path = Join-Path $PSScriptRoot "..\DelvUI\Media\Presets\$preset.delvui"
    $raw = [IO.File]::ReadAllText($path)
    foreach ($chunk in $raw.Trim().Split('|', [StringSplitOptions]::RemoveEmptyEntries)) {
        $json = Decode-Chunk $chunk
        $shortType = Get-ShortTypeName $json
        if ($watchTypes -notcontains $shortType) { continue }

        $enabled = if ($json -match '"Enabled"\s*:\s*(true|false)') { $Matches[1] } else { '?' }
        $pos = '?'
        if ($json -match '"Position"\s*:\s*\{[^{}]*"X"\s*:\s*(-?\d+(?:\.\d+)?)[^{}]*"Y"\s*:\s*(-?\d+(?:\.\d+)?)') {
            $pos = "$($Matches[1]), $($Matches[2])"
        }
        Write-Host "  $shortType Enabled=$enabled Position=$pos"
    }
    Write-Host ""
}
