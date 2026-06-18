param([string]$Path)

$raw = [IO.File]::ReadAllText($Path)
$chunks = $raw.Trim().Split('|')
$types = New-Object System.Collections.Generic.List[string]

foreach ($c in $chunks) {
    try {
        $bytes = [Convert]::FromBase64String($c)
        $ms = New-Object IO.MemoryStream(,$bytes)
        $def = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Decompress)
        $out = New-Object IO.MemoryStream
        $def.CopyTo($out)
        $json = [Text.Encoding]::UTF8.GetString($out.ToArray())
        if ($json -match '"\$type":\s*"([^"]+)"') {
            $short = ($Matches[1] -split '\.')[-1] -replace ',.*', ''
            $types.Add($short)
        }
    }
    catch {}
}

$types | Sort-Object
