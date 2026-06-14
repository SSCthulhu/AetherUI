$ErrorActionPreference = "Stop"

function Fail([string]$Message) {
    Write-Error $Message
    exit 1
}

function Assert-Equal([string]$Name, $Actual, $Expected) {
    if ($Actual -ne $Expected) {
        Fail "$Name mismatch. Expected '$Expected' but found '$Actual'."
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$pluginmasterPath = Join-Path $repoRoot "pluginmaster.json"
$aetherManifestPath = Join-Path $repoRoot "AetherUI.json"
$releaseZipPath = Join-Path $repoRoot "AetherUI.zip"

if (!(Test-Path $pluginmasterPath)) { Fail "Missing pluginmaster.json: $pluginmasterPath" }
if (!(Test-Path $aetherManifestPath)) { Fail "Missing AetherUI.json: $aetherManifestPath" }

$aetherManifest = Get-Content $aetherManifestPath | ConvertFrom-Json
$pluginmaster = Get-Content $pluginmasterPath | ConvertFrom-Json

if ($pluginmaster.Count -lt 1) {
    Fail "pluginmaster.json does not contain at least one plugin entry."
}

$masterEntry = $pluginmaster[0]
$expectedInternalName = "AetherUI"
$expectedRepoUrl = "https://github.com/SSCthulhu/FFXIVHudReimagined"
$expectedReleaseZipUrl = "https://github.com/SSCthulhu/FFXIVHudReimagined/releases/latest/download/AetherUI.zip"

Assert-Equal "AetherUI.json InternalName" $aetherManifest.InternalName $expectedInternalName
Assert-Equal "pluginmaster InternalName" $masterEntry.InternalName $expectedInternalName

Assert-Equal "pluginmaster AssemblyVersion" $masterEntry.AssemblyVersion $aetherManifest.AssemblyVersion

Assert-Equal "AetherUI.json RepoUrl" $aetherManifest.RepoUrl $expectedRepoUrl
Assert-Equal "pluginmaster RepoUrl" $masterEntry.RepoUrl $expectedRepoUrl

Assert-Equal "pluginmaster DownloadLinkInstall" $masterEntry.DownloadLinkInstall $expectedReleaseZipUrl
Assert-Equal "pluginmaster DownloadLinkUpdate" $masterEntry.DownloadLinkUpdate $expectedReleaseZipUrl

if (!(Test-Path $releaseZipPath)) {
    Fail "Release artifact not found at $releaseZipPath. Build Release first."
}

$requiredZipEntries = @(
    "AetherUI.dll",
    "AetherUI.deps.json",
    "AetherUI.json",
    "Colourful.dll",
    "changelog.md",
    "LICENSE"
)

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($releaseZipPath)
try {
    $entryNames = @{}
    foreach ($entry in $zip.Entries) {
        if ([string]::IsNullOrWhiteSpace($entry.Name)) { continue }
        $entryNames[$entry.Name] = $true
    }

    foreach ($required in $requiredZipEntries) {
        if (-not $entryNames.ContainsKey($required)) {
            Fail "Release zip missing required entry: $required"
        }
    }
}
finally {
    $zip.Dispose()
}

Write-Host "Package validation passed."
Write-Host "Version: $($aetherManifest.AssemblyVersion)"
Write-Host "InternalName: $expectedInternalName"
