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
$csprojPath = Join-Path $repoRoot "FFXIVHudPlugin.csproj"
$pluginmasterPath = Join-Path $repoRoot "pluginmaster.json"
$pluginManifestPath = Join-Path $repoRoot "FFXIVHudPlugin.json"
$reimaginedManifestPath = Join-Path $repoRoot "FFXIVHudReimagined.json"
$releaseZipPath = Join-Path $repoRoot "bin\Release\FFXIVHudPlugin\latest.zip"

if (!(Test-Path $csprojPath)) { Fail "Missing csproj: $csprojPath" }
if (!(Test-Path $pluginmasterPath)) { Fail "Missing pluginmaster.json: $pluginmasterPath" }
if (!(Test-Path $pluginManifestPath)) { Fail "Missing FFXIVHudPlugin.json: $pluginManifestPath" }
if (!(Test-Path $reimaginedManifestPath)) { Fail "Missing FFXIVHudReimagined.json: $reimaginedManifestPath" }

[xml]$csprojXml = Get-Content $csprojPath
$csprojVersion = $csprojXml.Project.PropertyGroup.Version | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($csprojVersion)) {
    Fail "Could not resolve <Version> from FFXIVHudPlugin.csproj."
}

$pluginManifest = Get-Content $pluginManifestPath | ConvertFrom-Json
$reimaginedManifest = Get-Content $reimaginedManifestPath | ConvertFrom-Json
$pluginmaster = Get-Content $pluginmasterPath | ConvertFrom-Json

if ($pluginmaster.Count -lt 1) {
    Fail "pluginmaster.json does not contain at least one plugin entry."
}

$masterEntry = $pluginmaster[0]
$expectedInternalName = "FFXIVHudReimagined"
$expectedRepoUrl = "https://github.com/SSCthulhu/FFXIVHudReimagined"
$expectedReleaseZipUrl = "https://github.com/SSCthulhu/FFXIVHudReimagined/releases/latest/download/FFXIVHudPlugin.zip"

Assert-Equal "FFXIVHudPlugin.json InternalName" $pluginManifest.InternalName $expectedInternalName
Assert-Equal "FFXIVHudReimagined.json InternalName" $reimaginedManifest.InternalName $expectedInternalName
Assert-Equal "pluginmaster InternalName" $masterEntry.InternalName $expectedInternalName

Assert-Equal "FFXIVHudPlugin.json AssemblyVersion" $pluginManifest.AssemblyVersion $csprojVersion
Assert-Equal "FFXIVHudReimagined.json AssemblyVersion" $reimaginedManifest.AssemblyVersion $csprojVersion
Assert-Equal "pluginmaster AssemblyVersion" $masterEntry.AssemblyVersion $csprojVersion

Assert-Equal "FFXIVHudPlugin.json RepoUrl" $pluginManifest.RepoUrl $expectedRepoUrl
Assert-Equal "FFXIVHudReimagined.json RepoUrl" $reimaginedManifest.RepoUrl $expectedRepoUrl
Assert-Equal "pluginmaster RepoUrl" $masterEntry.RepoUrl $expectedRepoUrl

Assert-Equal "pluginmaster DownloadLinkInstall" $masterEntry.DownloadLinkInstall $expectedReleaseZipUrl
Assert-Equal "pluginmaster DownloadLinkUpdate" $masterEntry.DownloadLinkUpdate $expectedReleaseZipUrl

if (!(Test-Path $releaseZipPath)) {
    Fail "Release artifact not found at $releaseZipPath. Build Release first."
}

$requiredZipEntries = @(
    "FFXIVHudPlugin.dll",
    "FFXIVHudPlugin.json",
    "FFXIVHudReimagined.dll",
    "FFXIVHudReimagined.deps.json",
    "FFXIVHudReimagined.json"
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
Write-Host "Version: $csprojVersion"
Write-Host "InternalName: $expectedInternalName"
