param(
    [string]$ConfigRoot = "$env:APPDATA\XIVLauncher\pluginConfigs\AetherUI\AetherUI",
    [string]$OutputPath = "$PSScriptRoot\..\DelvUI\Media\Presets\ActionCombat.delvui"
)

function Get-ConfigVersion {
    param([string]$Json)

    if ($Json -match '"Version"\s*:\s*"([^"]+)"') {
        return $Matches[1]
    }

    return "0.0.0.0"
}

function Test-VersionGreater {
    param(
        [string]$Left,
        [string]$Right
    )

    try {
        return [Version]$Left -gt [Version]$Right
    }
    catch {
        return $false
    }
}

function Get-ConfigPriorityScore {
    param([string]$FullPath)

    $score = 0

    if ($FullPath -notmatch "\\Misc\\") {
        $score += 10
    }

    if ($FullPath -notmatch "\\Player Parameter Orb\\General\.json$") {
        $score += 5
    }

    return $score
}

$skipped = New-Object System.Collections.Generic.List[string]
$byType = @{}

Get-ChildItem -Path $ConfigRoot -Recurse -Filter "*.json" | Where-Object {
    ($_.FullName -notmatch "\\Profiles\\") -and
    ($_.FullName -notmatch "\\Backups\\") -and
    ($_.FullName -notmatch "\\Import\\") -and
    ($_.Name -ne "Profiles.json") -and
    ($_.Name -ne "version")
} | ForEach-Object {
    $fullPath = $_.FullName

    if ($fullPath -match "\\Misc\\Action Camera\.json$") {
        $skipped.Add($fullPath + " (legacy duplicate path)")
        return
    }

    if ($fullPath -match "\\Player Parameter Orb\\General\.json$") {
        $skipped.Add($fullPath + " (legacy duplicate path)")
        return
    }

    $json = [System.IO.File]::ReadAllText($fullPath)
    if ($json -notmatch '"\$type":\s*"([^"]+)"') {
        $skipped.Add($fullPath + " (no type)")
        return
    }

    $typeString = $Matches[1]
    if ($typeString -match "NameplatesPreviewConfig") {
        $skipped.Add($fullPath + " (removed type)")
        return
    }

    $version = Get-ConfigVersion $json
    $priorityScore = Get-ConfigPriorityScore $fullPath

    $inputBytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $ms = New-Object System.IO.MemoryStream
    $deflate = New-Object System.IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Compress, $true)
    $deflate.Write($inputBytes, 0, $inputBytes.Length)
    $deflate.Close()
    $chunk = [Convert]::ToBase64String($ms.ToArray())

    if (-not $byType.ContainsKey($typeString)) {
        $byType[$typeString] = @{
            Chunk = $chunk
            Version = $version
            PriorityScore = $priorityScore
            Path = $fullPath
        }
        return
    }

    $existing = $byType[$typeString]
    $replace = $false

    if (Test-VersionGreater $version $existing.Version) {
        $replace = $true
    }
    elseif ($version -eq $existing.Version -and $priorityScore -gt $existing.PriorityScore) {
        $replace = $true
    }

    if ($replace) {
        $skipped.Add($existing.Path + " (duplicate type, replaced by newer copy)")
        $byType[$typeString] = @{
            Chunk = $chunk
            Version = $version
            PriorityScore = $priorityScore
            Path = $fullPath
        }
    }
    else {
        $skipped.Add($fullPath + " (duplicate type)")
    }
}

$chunks = @($byType.Values | ForEach-Object { $_.Chunk })
$combined = [string]::Join("|", $chunks)
$directory = Split-Path $OutputPath -Parent
if (-not (Test-Path $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}

[System.IO.File]::WriteAllText($OutputPath, $combined)
Write-Host "Exported $($chunks.Count) config chunks to $OutputPath"
if ($skipped.Count -gt 0) {
    Write-Host "Skipped $($skipped.Count) entries:"
    $skipped | ForEach-Object { Write-Host "  $_" }
}
