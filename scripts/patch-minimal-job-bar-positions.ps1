param(
    [string]$PresetPath = "$PSScriptRoot\..\DelvUI\Media\Presets\Minimal.delvui",
    [double]$X = 0,
    [double]$Y = 499.5
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

function Test-IsJobBarConfigType {
    param([string]$Json)

    if ($Json -notmatch '"\$type":\s*"DelvUI\.Interface\.Jobs\.([^"]+)"') {
        return $false
    }

    $typeName = $Matches[1] -replace ',.*', ''
    if ($typeName -eq 'JobBarsGeneralConfig') {
        return $false
    }

    return $typeName -like '*Config'
}

$raw = [IO.File]::ReadAllText($PresetPath)
$chunks = $raw.Trim().Split('|', [StringSplitOptions]::RemoveEmptyEntries)
$updated = New-Object System.Collections.Generic.List[string]
$jobBarCount = 0

foreach ($chunk in $chunks) {
    $json = Decode-Chunk $chunk
    if (Test-IsJobBarConfigType $json) {
        $json = Set-RootJsonPosition $json $X $Y
        $jobBarCount++
    }

    $updated.Add((Encode-Chunk $json))
}

[System.IO.File]::WriteAllText($PresetPath, [string]::Join('|', $updated))
Write-Host "Updated $jobBarCount job bar config positions to ($X, $Y) in $PresetPath"
