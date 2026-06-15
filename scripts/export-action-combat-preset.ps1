param(
    [string]$ConfigRoot = "$env:APPDATA\XIVLauncher\pluginConfigs\AetherUI\AetherUI",
    [string]$OutputPath = "$PSScriptRoot\..\DelvUI\Media\Presets\ActionCombat.delvui"
)

$chunks = New-Object System.Collections.Generic.List[string]
$skipped = New-Object System.Collections.Generic.List[string]

Get-ChildItem -Path $ConfigRoot -Recurse -Filter "*.json" | Where-Object {
    ($_.FullName -notmatch "\\Profiles\\") -and
    ($_.FullName -notmatch "\\Backups\\") -and
    ($_.FullName -notmatch "\\Import\\") -and
    ($_.Name -ne "Profiles.json") -and
    ($_.Name -ne "version")
} | ForEach-Object {
    $json = [System.IO.File]::ReadAllText($_.FullName)
    if ($json -notmatch '"\$type":\s*"([^"]+)"') {
        $skipped.Add($_.FullName + " (no type)")
        return
    }

    $typeString = $Matches[1]
    if ($typeString -match "NameplatesPreviewConfig") {
        $skipped.Add($_.FullName + " (removed type)")
        return
    }

    $inputBytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $ms = New-Object System.IO.MemoryStream
    $deflate = New-Object System.IO.Compression.DeflateStream($ms, [System.IO.Compression.CompressionMode]::Compress, $true)
    $deflate.Write($inputBytes, 0, $inputBytes.Length)
    $deflate.Close()
    $chunks.Add([Convert]::ToBase64String($ms.ToArray()))
}

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
