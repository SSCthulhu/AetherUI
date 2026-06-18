param([string]$Path)

$raw = [IO.File]::ReadAllText($Path)
foreach ($c in $raw.Trim().Split('|')) {
    $bytes = [Convert]::FromBase64String($c)
    $ms = New-Object IO.MemoryStream(,$bytes)
    $def = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Decompress)
    $out = New-Object IO.MemoryStream
    $def.CopyTo($out)
    $json = [Text.Encoding]::UTF8.GetString($out.ToArray())
    if ($json -notmatch 'MinimapConfig') { continue }

    if ($json -match '"Square"\s*:\s*(true|false)') { Write-Host "Square=$($Matches[1])" }
    if ($json -match '"Size"\s*:\s*([\d.]+)') { Write-Host "Size=$($Matches[1])" }
    if ($json -match '"Position"[\s\S]*?"X"\s*:\s*([\d.-]+)[\s\S]*?"Y"\s*:\s*([\d.-]+)') {
        Write-Host "Position=$($Matches[1]),$($Matches[2])"
    }
    break
}
