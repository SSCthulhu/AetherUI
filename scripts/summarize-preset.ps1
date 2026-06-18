param([string]$Path)

$raw = [IO.File]::ReadAllText($Path)
$chunks = $raw.Trim().Split('|')
Write-Host "=== Minimal Preset Summary ($($chunks.Count) chunks) ==="

$keyTypes = @(
    @{ Name = 'FontsConfig'; Props = @('GlobalFontKey','GlobalNumericFontKey') },
    @{ Name = 'PlayerParameterOrbConfig'; Props = @('Enabled') },
    @{ Name = 'MinimapConfig'; Props = @('Enabled','Square','Size') },
    @{ Name = 'NameplatesGeneralConfig'; Props = @('Enabled') },
    @{ Name = 'PartyFramesConfig'; Props = @('Enabled') },
    @{ Name = 'PartyCooldownsConfig'; Props = @('Enabled') },
    @{ Name = 'EnemyListConfig'; Props = @('Enabled') },
    @{ Name = 'JobBarsGeneralConfig'; Props = @('Enabled') },
    @{ Name = 'ActionCameraConfig'; Props = @('Enabled') },
    @{ Name = 'GridConfig'; Props = @('ShowGrid') },
    @{ Name = 'TooltipsConfig'; Props = @('Enabled') },
    @{ Name = 'WindowClippingConfig'; Props = @('Enabled') },
    @{ Name = 'HUDOptionsConfig'; Props = @('Enabled') }
)

function Get-JsonRootEnabled {
    param([string]$Json)
    # Last root-level Enabled before Version is typical in these configs
    if ($Json -match '"Enabled"\s*:\s*(true|false)\s*,\s*"Version"') {
        return $Matches[1]
    }
    if ($Json -match '"Enabled"\s*:\s*(true|false)') {
        return $Matches[1]
    }
    return 'n/a'
}

foreach ($c in $chunks) {
    $bytes = [Convert]::FromBase64String($c)
    $ms = New-Object IO.MemoryStream(,$bytes)
    $def = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Decompress)
    $out = New-Object IO.MemoryStream
    $def.CopyTo($out)
    $json = [Text.Encoding]::UTF8.GetString($out.ToArray())
    if ($json -notmatch '"\$type":\s*"([^"]+)"') { continue }
    $short = ($Matches[1] -split '\.')[-1] -replace ',.*', ''

    foreach ($key in $keyTypes) {
        if ($short -ne $key.Name) { continue }
        Write-Host "`n[$($key.Name)]"
        foreach ($prop in $key.Props) {
            if ($prop -eq 'Enabled') {
                Write-Host "  Enabled=$(Get-JsonRootEnabled $json)"
            }
            elseif ($json -match "`"$prop`"\s*:\s*([^,\}\]]+)") {
                Write-Host "  $prop=$($Matches[1].Trim())"
            }
        }
        if ($short -eq 'PlayerParameterOrbConfig' -and $json -match '"Position"[\s\S]*?"X"\s*:\s*([\d.-]+)[\s\S]*?"Y"\s*:\s*([\d.-]+)') {
            Write-Host "  Position=$($Matches[1]),$($Matches[2])"
        }
        if ($short -eq 'MinimapConfig' -and $json -match '"Position"[\s\S]*?"X"\s*:\s*([\d.-]+)[\s\S]*?"Y"\s*:\s*([\d.-]+)') {
            Write-Host "  Position=$($Matches[1]),$($Matches[2])"
        }
    }
}
