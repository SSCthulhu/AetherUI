param(
    [string]$PresetPath = "$PSScriptRoot\..\DelvUI\Media\Presets\Minimal.delvui",
    [double]$BuffX = -450,
    [double]$BuffY = 500,
    [double]$DebuffX = 450,
    [double]$DebuffY = 500
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

function Set-JsonEnabled {
    param([string]$Json, [bool]$Enabled)
    if ($Json -notmatch '"Enabled"') { return $Json }
    $value = if ($Enabled) { "true" } else { "false" }
    return [regex]::Replace($Json, '"Enabled"\s*:\s*(true|false)\s*,\s*"(StrataLevel|Version)"', "`"Enabled`": $value, `"`$2`"", 1)
}

function Set-RootJsonPosition {
    param([string]$Json, [double]$X, [double]$Y)
    $pattern = '"Position"\s*:\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}'
    $matches = [regex]::Matches($Json, $pattern)
    if ($matches.Count -eq 0) { return $Json }

    $last = $matches[$matches.Count - 1]
    $inner = $last.Groups[1].Value
    $newInner = [regex]::Replace($inner, '"X"\s*:\s*-?\d+(?:\.\d+)?', "`"X`": $X")
    $newInner = [regex]::Replace($newInner, '"Y"\s*:\s*-?\d+(?:\.\d+)?', "`"Y`": $Y")
    $newBlock = "`"Position`": { $newInner }"
    return $Json.Substring(0, $last.Index) + $newBlock + $Json.Substring($last.Index + $last.Length)
}

$enabledTypes = @{
    'PlayerBuffsListConfig' = @{ X = $BuffX; Y = $BuffY }
    'PlayerDebuffsListConfig' = @{ X = $DebuffX; Y = $DebuffY }
}

$disabledTypes = @(
    'TargetBuffsListConfig',
    'TargetDebuffsListConfig',
    'FocusTargetBuffsListConfig',
    'FocusTargetDebuffsListConfig',
    'CustomEffectsListConfig'
)

$raw = [IO.File]::ReadAllText($PresetPath)
$chunks = $raw.Trim().Split('|', [StringSplitOptions]::RemoveEmptyEntries)
$updated = New-Object System.Collections.Generic.List[string]
$enabledCount = 0
$disabledCount = 0

foreach ($chunk in $chunks) {
    $json = Decode-Chunk $chunk
    $shortType = Get-ShortTypeName $json
    if ($null -eq $shortType) {
        $updated.Add($chunk)
        continue
    }

    if ($enabledTypes.ContainsKey($shortType)) {
        $coords = $enabledTypes[$shortType]
        $json = Set-JsonEnabled $json $true
        $json = Set-RootJsonPosition $json $coords.X $coords.Y
        $enabledCount++
    }
    elseif ($disabledTypes -contains $shortType) {
        $json = Set-JsonEnabled $json $false
        $disabledCount++
    }

    $updated.Add((Encode-Chunk $json))
}

[System.IO.File]::WriteAllText($PresetPath, [string]::Join('|', $updated))
Write-Host "Minimal buff/debuff patch: $enabledCount enabled, $disabledCount disabled in $PresetPath"
