param(
    [string]$PresetPath = "$PSScriptRoot\..\DelvUI\Media\Presets\Minimal.delvui"
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

function Set-JsonBoolProperty {
    param(
        [string]$Json,
        [string]$PropertyName,
        [bool]$Value
    )

    $literal = if ($Value) { "true" } else { "false" }
    $pattern = "`"$PropertyName`"\s*:\s*(true|false)"
    if ($Json -match $pattern) {
        return [regex]::Replace($Json, $pattern, "`"$PropertyName`": $literal", 1)
    }

    return $Json
}

$raw = [IO.File]::ReadAllText($PresetPath)
$chunks = $raw.Trim().Split('|', [StringSplitOptions]::RemoveEmptyEntries)
$updated = New-Object System.Collections.Generic.List[string]
$patched = $false

foreach ($chunk in $chunks) {
    $json = Decode-Chunk $chunk
    $shortType = Get-ShortTypeName $json
    if ($shortType -eq 'HUDOptionsConfig') {
        $json = Set-JsonBoolProperty $json 'HideDefaultJobGauges' $true
        $patched = $true
    }

    $updated.Add((Encode-Chunk $json))
}

[System.IO.File]::WriteAllText($PresetPath, [string]::Join('|', $updated))
if ($patched) {
    Write-Host "Enabled HideDefaultJobGauges in Minimal HUDOptionsConfig"
}
else {
    Write-Warning "HUDOptionsConfig chunk not found in $PresetPath"
}
