<#
.SYNOPSIS
  Serve um build WebGL descomprimido em http://localhost:<porta>/ para testar no navegador.

.EXAMPLE
  .\tools\serve-webgl.ps1 -Root Builds\WebGL
  # abra http://localhost:8080/ ; Ctrl+C para parar
#>
[CmdletBinding()]
param(
    [string]$Root = (Join-Path $PSScriptRoot '..\Builds\WebGL'),
    [int]$Port = 8080
)

$ErrorActionPreference = 'Stop'
$Root = (Resolve-Path $Root).Path

$types = @{
    '.html' = 'text/html; charset=utf-8'; '.js' = 'application/javascript'; '.wasm' = 'application/wasm'
    '.data' = 'application/octet-stream'; '.json' = 'application/json'; '.css' = 'text/css'
    '.png' = 'image/png'; '.jpg' = 'image/jpeg'; '.ico' = 'image/x-icon'; '.svg' = 'image/svg+xml'; '.txt' = 'text/plain'
}

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$Port/")
$listener.Start()
Write-Host "Servindo $Root em http://localhost:$Port/  (Ctrl+C para parar)"

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $path = [Uri]::UnescapeDataString($context.Request.Url.AbsolutePath).TrimStart('/')
        if ([string]::IsNullOrEmpty($path)) { $path = 'index.html' }

        $file = [System.IO.Path]::GetFullPath((Join-Path $Root $path))
        $response = $context.Response

        # Never leave the build folder.
        if (-not $file.StartsWith($Root, [StringComparison]::OrdinalIgnoreCase) -or -not (Test-Path $file -PathType Leaf)) {
            $response.StatusCode = 404
            $response.Close()
            continue
        }

        $ext = [System.IO.Path]::GetExtension($file).ToLowerInvariant()
        $response.ContentType = if ($types.ContainsKey($ext)) { $types[$ext] } else { 'application/octet-stream' }
        $bytes = [System.IO.File]::ReadAllBytes($file)
        $response.ContentLength64 = $bytes.Length
        $response.OutputStream.Write($bytes, 0, $bytes.Length)
        $response.Close()
    }
}
finally {
    $listener.Stop()
}
