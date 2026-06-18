param([string]$Path, [string]$TypeFilter)

$raw = [IO.File]::ReadAllText($Path)
foreach ($c in $raw.Trim().Split('|')) {
    $bytes = [Convert]::FromBase64String($c)
    $ms = New-Object IO.MemoryStream(,$bytes)
    $def = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Decompress)
    $out = New-Object IO.MemoryStream
    $def.CopyTo($out)
    $json = [Text.Encoding]::UTF8.GetString($out.ToArray())
    if ($json -notmatch $TypeFilter) { continue }
    if ($json -match '"Position"[\s\S]*?"X"\s*:\s*([\d.-]+)[\s\S]*?"Y"\s*:\s*([\d.-]+)\s*\}\s*,\s*"Enabled"') {
        Write-Host "$TypeFilter root Position=$($Matches[1]),$($Matches[2])"
    }
    break
}
