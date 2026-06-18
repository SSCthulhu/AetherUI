param(
    [string]$PresetPath = "$PSScriptRoot\..\DelvUI\Media\Presets\Minimal.delvui",
    [double]$OrbX = -730,
    [double]$OrbY = 564,
    [double]$MinimapX = 729.5,
    [double]$MinimapY = 563.5,
    [double]$PartyFramesX = -1230,
    [double]$PartyFramesY = 375,
    [double]$EnemyListX = 1005,
    [double]$EnemyListY = 395
)

function Decode-Chunk {
    param([string]$Chunk)
    $bytes = [Convert]::FromBase64String($Chunk)
    $ms = New-Object IO.MemoryStream(,$bytes)
    $def = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Decompress)
    $out = New-Object IO.MemoryStream
    $def.CopyTo($out)
    return [Text.Encoding]::UTF8.GetString($out.ToArray())
}

function Encode-Chunk {
    param([string]$Json)
    $inputBytes = [Text.Encoding]::UTF8.GetBytes($Json)
    $ms = New-Object IO.MemoryStream
    $def = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Compress, $true)
    $def.Write($inputBytes, 0, $inputBytes.Length)
    $def.Close()
    return [Convert]::ToBase64String($ms.ToArray())
}

function Get-ShortTypeName {
    param([string]$Json)
    if ($Json -match '"\$type":\s*"([^"]+)"') {
        return ($Matches[1] -split '\.')[-1] -replace ',.*', ''
    }
    return $null
}

function Set-RootJsonPosition {
    param(
        [string]$Json,
        [double]$X,
        [double]$Y
    )

    $pattern = '"Position"\s*:\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}'
    $matches = [regex]::Matches($Json, $pattern)
    if ($matches.Count -eq 0) {
        return $Json
    }

    $last = $matches[$matches.Count - 1]
    $inner = $last.Groups[1].Value
    $newInner = [regex]::Replace($inner, '"X"\s*:\s*-?\d+(?:\.\d+)?', "`"X`": $X")
    $newInner = [regex]::Replace($newInner, '"Y"\s*:\s*-?\d+(?:\.\d+)?', "`"Y`": $Y")
    $newBlock = "`"Position`": { $newInner }"
    return $Json.Substring(0, $last.Index) + $newBlock + $Json.Substring($last.Index + $last.Length)
}

$layoutTypes = @{
    'PlayerParameterOrbConfig' = @{ X = $OrbX; Y = $OrbY }
    'MinimapConfig' = @{ X = $MinimapX; Y = $MinimapY }
    'PartyFramesConfig' = @{ X = $PartyFramesX; Y = $PartyFramesY }
    'EnemyListConfig' = @{ X = $EnemyListX; Y = $EnemyListY }
}

$raw = [IO.File]::ReadAllText($PresetPath)
$chunks = $raw.Trim().Split('|', [StringSplitOptions]::RemoveEmptyEntries)
$updated = New-Object System.Collections.Generic.List[string]
$patched = 0

foreach ($chunk in $chunks) {
    $json = Decode-Chunk $chunk
    $shortType = Get-ShortTypeName $json
    if ($null -ne $shortType -and $layoutTypes.ContainsKey($shortType)) {
        $coords = $layoutTypes[$shortType]
        $json = Set-RootJsonPosition $json $coords.X $coords.Y
        $patched++
    }

    $updated.Add((Encode-Chunk $json))
}

[System.IO.File]::WriteAllText($PresetPath, [string]::Join('|', $updated))
Write-Host "Patched $patched Minimal HUD layout positions in $PresetPath"
