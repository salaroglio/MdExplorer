# MdExplorer -- start container + open browser (Windows)
# Usage: .\start.ps1

$ErrorActionPreference = "Stop"
$url = "http://127.0.0.1:5000/client2/index.html"
$probe = "http://127.0.0.1:5000/"   # any 200/404/302 means Kestrel is up
$timeoutSec = 90

Write-Host "Starting MdExplorer container..." -ForegroundColor Cyan
docker compose up -d
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Waiting for MdExplorer to respond on $probe (timeout: ${timeoutSec}s)..." -ForegroundColor Cyan
$elapsed = 0
$serverUp = $false
while ($elapsed -lt $timeoutSec) {
    try {
        Invoke-WebRequest -Uri $probe -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop | Out-Null
        $serverUp = $true
        break
    } catch {
        # Any HTTP response (incl. 404/500) means Kestrel is alive. Both PS 5.1
        # (WebException) and PS 7+ (HttpResponseException) expose .Response on
        # the inner exception when the server replied. We only keep polling if
        # there's truly no response (connection refused -> no .Response).
        if ($_.Exception.Response) { $serverUp = $true; break }
    }
    Start-Sleep -Seconds 2
    $elapsed += 2
    Write-Host "." -NoNewline
}
Write-Host ""

if (-not $serverUp) {
    Write-Warning "Timeout -- MdExplorer didn't respond within $timeoutSec s. Opening URL anyway."
} else {
    Write-Host "MdExplorer ready (${elapsed}s)." -ForegroundColor Green
}

Start-Process $url
