param(
    [string]$Path
)

function Decode-Chunk([string]$Chunk) {
    $bytes = [Convert]::FromBase64String($Chunk)
    $ms = New-Object IO.MemoryStream(,$bytes)
    $def = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Decompress)
    $out = New-Object IO.MemoryStream
    $def.CopyTo($out)
    return [Text.Encoding]::UTF8.GetString($out.ToArray())
}

$raw = [IO.File]::ReadAllText($Path)
$types = @{}
$chunks = $raw.Trim().Split('|')

foreach ($chunk in $chunks) {
    if ([string]::IsNullOrWhiteSpace($chunk)) { continue }
    try {
        $json = Decode-Chunk $chunk
        if ($json -notmatch '"\$type"\s*:\s*"([^"]+)"') { continue }
        $short = ($Matches[1] -split '\.')[-1] -replace ',.*', ''
        $obj = $json | ConvertFrom-Json
        $enabled = if ($null -ne $obj.PSObject.Properties['Enabled']) { $obj.Enabled.ToString().ToLower() } else { 'n/a' }
        $pos = if ($null -ne $obj.PSObject.Properties['Position']) { "($($obj.Position.X), $($obj.Position.Y))" } else { '' }
        $types[$short] = @{ Enabled = $enabled; Position = $pos }
    }
    catch {}
}

Write-Host "=== $(Split-Path $Path -Leaf) ==="
Write-Host "Chunk count: $($types.Count)"
$keyTypes = @(
    'PlayerParameterOrbConfig','PlayerUnitFrameConfig','PartyFramesConfig','EnemyListConfig',
    'MinimapConfig','ActionCameraConfig','JobBarsGeneralConfig','PlayerBuffsListConfig',
    'PlayerDebuffsListConfig','NameplatesGeneralConfig','HUDOptionsConfig','BossesNameplateConfig'
)
foreach ($k in $keyTypes) {
    if ($types.ContainsKey($k)) {
        $entry = $types[$k]
        $posText = if ($entry.Position) { " pos=$($entry.Position)" } else { '' }
        Write-Host "  $k Enabled=$($entry.Enabled)$posText"
    }
}
