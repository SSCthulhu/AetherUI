param(
    [string]$BasePath = "$PSScriptRoot\..\DelvUI\Media\Presets\Default.delvui",
    [string]$TemplatePath = "$PSScriptRoot\..\DelvUI\Media\Presets\ActionCombat.delvui",
    [string]$OutputDir = "$PSScriptRoot\..\DelvUI\Media\Presets"
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
    param(
        [string]$Json,
        [bool]$Enabled
    )

    if ($Json -notmatch '"Enabled"') {
        return $Json
    }

    $value = if ($Enabled) { "true" } else { "false" }
    return [regex]::Replace($Json, '"Enabled"\s*:\s*(true|false)', "`"Enabled`": $value", 1)
}

function Set-JsonPosition {
    param(
        [string]$Json,
        [double]$X,
        [double]$Y
    )

    if ($Json -notmatch '"Position"') {
        return $Json
    }

    $pattern = '"Position"\s*:\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}'
    if ($Json -match $pattern) {
        $inner = $Matches[1]
        $newInner = [regex]::Replace($inner, '"X"\s*:\s*-?\d+(?:\.\d+)?', "`"X`": $X")
        $newInner = [regex]::Replace($newInner, '"Y"\s*:\s*-?\d+(?:\.\d+)?', "`"Y`": $Y")
        $replacement = "`"Position`": { $newInner }"
        return [regex]::Replace($Json, $pattern, $replacement, 1)
    }

    return $Json
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

function Set-JsonNumberProperty {
    param(
        [string]$Json,
        [string]$PropertyName,
        [double]$Value
    )

    $pattern = "`"$PropertyName`"\s*:\s*-?\d+(?:\.\d+)?"
    if ($Json -match $pattern) {
        return [regex]::Replace($Json, $pattern, "`"$PropertyName`": $Value", 1)
    }

    return $Json
}

function Apply-MinimalLayoutOverrides {
    param([string]$ShortType, [string]$Json)

    if ($ShortType -eq 'PlayerParameterOrbConfig') {
        return Set-JsonPosition $Json -700 564
    }

    if ($ShortType -eq 'MinimapConfig') {
        $json = Set-JsonPosition $Json 700 564
        $json = Set-JsonBoolProperty $json 'Square' $false
        return Set-JsonNumberProperty $json 'Size' 335
    }

    return $Json
}

function Test-TypeMatch {
    param(
        [string]$ShortType,
        [string[]]$Patterns
    )

    foreach ($pattern in $Patterns) {
        if ($ShortType -like $pattern) {
            return $true
        }
    }

    return $false
}

function Get-PresetEnabled {
    param(
        [string]$ShortType,
        [string]$Preset
    )

    $playerUnit = @('PlayerUnitFrameConfig')
    $playerMana = @('PlayerPrimaryResourceConfig')
    $playerCast = @('PlayerCastbarConfig')
    $targetUnit = @('TargetUnitFrameConfig', 'TargetOfTargetUnitFrameConfig', 'FocusTargetUnitFrameConfig')
    $targetMana = @('TargetPrimaryResourceConfig', 'TargetOfTargetPrimaryResourceConfig', 'FocusTargetPrimaryResourceConfig')
    $targetCast = @('TargetCastbarConfig', 'TargetOfTargetCastbarConfig', 'FocusTargetCastbarConfig')
    $allUnit = $playerUnit + $targetUnit
    $allMana = $playerMana + $targetMana
    $allCast = $playerCast + $targetCast
    $buffs = @(
        'PlayerBuffsListConfig', 'PlayerDebuffsListConfig', 'TargetBuffsListConfig', 'TargetDebuffsListConfig',
        'FocusTargetBuffsListConfig', 'FocusTargetDebuffsListConfig', 'CustomEffectsListConfig'
    )
    $otherElements = @('ExperienceBarConfig', 'GCDIndicatorConfig', 'PullTimerConfig', 'LimitBreakConfig', 'MPTickerConfig')
    $nameplates = @(
        'NameplatesGeneralConfig', 'PlayerNameplateConfig', 'EnemyNameplateConfig', 'PartyMembersNameplateConfig',
        'AllianceMembersNameplateConfig', 'FriendPlayerNameplateConfig', 'OtherPlayerNameplateConfig',
        'PetNameplateConfig', 'NPCNameplateConfig', 'MinionNPCNameplateConfig', 'ObjectsNameplateConfig',
        'BossesNameplateConfig'
    )
    $partyFrames = @(
        'PartyFramesConfig', 'PartyFramesHealthBarsConfig', 'PartyFramesManaBarConfig', 'PartyFramesCastbarConfig',
        'PartyFramesIconsConfig', 'PartyFramesBuffsConfig', 'PartyFramesDebuffsConfig', 'PartyFramesTrackersConfig',
        'PartyFramesCooldownListConfig'
    )
    $partyCooldowns = @('PartyCooldownsConfig', 'PartyCooldownsBarConfig', 'PartyCooldownsDataConfig')
    $enemyList = @(
        'EnemyListConfig', 'EnemyListHealthBarConfig', 'EnemyListEnmityIconConfig', 'EnemyListSignIconConfig',
        'EnemyListCastbarConfig', 'EnemyListBuffsConfig', 'EnemyListDebuffsConfig'
    )

    switch ($Preset) {
        'Minimal' {
            if ($ShortType -eq 'PlayerParameterOrbConfig') { return $true }
            if (Test-TypeMatch $ShortType $playerUnit) { return $false }
            if (Test-TypeMatch $ShortType $playerMana) { return $false }
            if (Test-TypeMatch $ShortType $playerCast) { return $false }
            if (Test-TypeMatch $ShortType $targetUnit) { return $false }
            if (Test-TypeMatch $ShortType $targetMana) { return $false }
            if (Test-TypeMatch $ShortType $targetCast) { return $false }
            if (Test-TypeMatch $ShortType $buffs) { return $false }
            if (Test-TypeMatch $ShortType $otherElements) { return $false }
            if (Test-TypeMatch $ShortType $nameplates) { return $true }
            if (Test-TypeMatch $ShortType $partyFrames) { return $false }
            if ($ShortType -eq 'PartyCooldownsConfig') { return $false }
            if ($ShortType -eq 'EnemyListConfig') { return $false }
            if ($ShortType -eq 'MinimapConfig') { return $true }
            if ($ShortType -eq 'JobBarsGeneralConfig') { return $false }
            if ($ShortType -eq 'ActionCameraConfig') { return $false }
            return $null
        }
        'MmoModern' {
            if ($ShortType -eq 'PlayerParameterOrbConfig') { return $false }
            if (Test-TypeMatch $ShortType $allUnit) { return $true }
            if (Test-TypeMatch $ShortType $allMana) { return $true }
            if (Test-TypeMatch $ShortType $allCast) { return $true }
            if (Test-TypeMatch $ShortType $buffs) { return $true }
            if (Test-TypeMatch $ShortType $nameplates) { return $true }
            if (Test-TypeMatch $ShortType $partyFrames) { return $true }
            if ($ShortType -eq 'PartyCooldownsConfig') { return $false }
            if ($ShortType -eq 'EnemyListConfig') { return $false }
            if ($ShortType -eq 'MinimapConfig') { return $true }
            if ($ShortType -eq 'JobBarsGeneralConfig') { return $true }
            if ($ShortType -eq 'ActionCameraConfig') { return $false }
            if ($ShortType -eq 'ExperienceBarConfig') { return $false }
            if ($ShortType -eq 'GCDIndicatorConfig') { return $true }
            if ($ShortType -eq 'PullTimerConfig') { return $false }
            if ($ShortType -eq 'LimitBreakConfig') { return $false }
            if ($ShortType -eq 'MPTickerConfig') { return $false }
            return $null
        }
        'RaidFocused' {
            if ($ShortType -eq 'PlayerParameterOrbConfig') { return $false }
            if (Test-TypeMatch $ShortType $allUnit) { return $true }
            if (Test-TypeMatch $ShortType $allMana) { return $true }
            if (Test-TypeMatch $ShortType $allCast) { return $true }
            if (Test-TypeMatch $ShortType $buffs) { return $true }
            if (Test-TypeMatch $ShortType $nameplates) { return $true }
            if (Test-TypeMatch $ShortType $partyFrames) { return $true }
            if ($ShortType -eq 'PartyCooldownsConfig') { return $true }
            if ($ShortType -eq 'EnemyListConfig') { return $true }
            if ($ShortType -eq 'MinimapConfig') { return $true }
            if ($ShortType -eq 'JobBarsGeneralConfig') { return $true }
            if ($ShortType -eq 'ActionCameraConfig') { return $false }
            if ($ShortType -eq 'ExperienceBarConfig') { return $false }
            if ($ShortType -eq 'GCDIndicatorConfig') { return $false }
            if ($ShortType -eq 'PullTimerConfig') { return $true }
            if ($ShortType -eq 'LimitBreakConfig') { return $true }
            if ($ShortType -eq 'MPTickerConfig') { return $false }
            return $null
        }
    }

    return $null
}

function Build-Preset {
    param(
        [string]$Preset,
        [string]$OutputPath
    )

    $baseRaw = [IO.File]::ReadAllText($BasePath)
    $baseChunks = $baseRaw.Trim().Split('|', [StringSplitOptions]::RemoveEmptyEntries)
    $byType = @{}

    foreach ($chunk in $baseChunks) {
        $json = Decode-Chunk $chunk
        $shortType = Get-ShortTypeName $json
        if ($null -eq $shortType) { continue }
        if ($shortType -eq 'HomeFeatureSettingsConfig') { continue }

        $enabledOverride = Get-PresetEnabled $shortType $Preset
        if ($null -ne $enabledOverride) {
            $json = Set-JsonEnabled $json $enabledOverride
        }

        if ($Preset -eq 'Minimal') {
            $json = Apply-MinimalLayoutOverrides $shortType $json
        }

        $byType[$shortType] = Encode-Chunk $json
    }

    if (Test-Path $TemplatePath) {
        $templateRaw = [IO.File]::ReadAllText($TemplatePath)
        $templateChunks = $templateRaw.Trim().Split('|', [StringSplitOptions]::RemoveEmptyEntries)
        $extraTypes = @('ActionCameraConfig', 'JobBarsGeneralConfig', 'BossesNameplateConfig')

        foreach ($chunk in $templateChunks) {
            $json = Decode-Chunk $chunk
            $shortType = Get-ShortTypeName $json
            if ($null -eq $shortType) { continue }
            if ($shortType -notin $extraTypes) { continue }
            if ($byType.ContainsKey($shortType)) { continue }

            $enabledOverride = Get-PresetEnabled $shortType $Preset
            if ($null -ne $enabledOverride) {
                $json = Set-JsonEnabled $json $enabledOverride
            }

            if ($Preset -eq 'Minimal') {
                $json = Apply-MinimalLayoutOverrides $shortType $json
            }

            $byType[$shortType] = Encode-Chunk $json
        }
    }

    $combined = [string]::Join('|', @($byType.Values))
    $directory = Split-Path $OutputPath -Parent
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    [IO.File]::WriteAllText($OutputPath, $combined)
    Write-Host "Built $Preset -> $OutputPath ($($byType.Count) chunks)"
}

$presets = @(
    @{ Name = 'Minimal'; File = 'Minimal.delvui' },
    @{ Name = 'MmoModern'; File = 'MmoModern.delvui' },
    @{ Name = 'RaidFocused'; File = 'RaidFocused.delvui' }
)

foreach ($preset in $presets) {
    $outputPath = Join-Path $OutputDir $preset.File
    Build-Preset -Preset $preset.Name -OutputPath $outputPath
}
