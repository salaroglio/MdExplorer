# MdExplorer — release helper: build → tag → push to GHCR
#
# Usage:
#   .\release-docker.ps1 0.1.2-preview
#   .\release-docker.ps1 0.1.2-preview -NoBuild     # reuse current mdexplorer:dev image
#   .\release-docker.ps1 0.1.2-preview -DryRun      # show what would happen
#
# Authentication: assumes you've already run `docker login ghcr.io`. If you
# haven't, the push step will fail with 401 — log in and rerun.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0,
        HelpMessage = "Version tag, e.g. 0.1.2-preview")]
    [ValidatePattern('^\d+\.\d+\.\d+(-[a-zA-Z0-9]+)?$')]
    [string] $Version,

    [switch] $NoBuild,
    [switch] $NoLatest,
    [switch] $DryRun
)

$ErrorActionPreference = "Stop"

$image      = "mdexplorer:dev"
$repo       = "ghcr.io/salaroglio/mdexplorer"
$versionTag = "${repo}:${Version}"
$latestTag  = "${repo}:latest"

function Step($msg) { Write-Host ""; Write-Host ">>> $msg" -ForegroundColor Cyan }
function Run($cmd, [string[]]$cmdArgs) {
    Write-Host "    $cmd $($cmdArgs -join ' ')" -ForegroundColor DarkGray
    if (-not $DryRun) {
        & $cmd @cmdArgs
        if ($LASTEXITCODE -ne 0) { throw "Command failed (exit $LASTEXITCODE)" }
    }
}

# Locate the docker/ folder so we can call `docker compose build` against
# its docker-compose.yml regardless of where the script is invoked from.
$dockerDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Step "Sanity checks"
docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker daemon not reachable. Open Docker Desktop and retry."
    exit 1
}
Write-Host "    docker daemon: OK"

# Refuse to overwrite an existing remote tag (latest is always safe to move).
$tok = (Invoke-RestMethod "https://ghcr.io/token?scope=repository:salaroglio/mdexplorer:pull").token
$h   = @{ Authorization = "Bearer $tok"; Accept = "application/vnd.docker.distribution.manifest.v2+json" }
try {
    Invoke-WebRequest "https://ghcr.io/v2/salaroglio/mdexplorer/manifests/$Version" `
        -Headers $h -Method Head -UseBasicParsing -ErrorAction Stop | Out-Null
    Write-Error "Tag '$Version' already exists on GHCR. Pick a different version (immutable tags policy)."
    exit 1
} catch {
    if ($_.Exception.Response.StatusCode.value__ -ne 404) {
        Write-Warning "Couldn't probe remote tag (HTTP $($_.Exception.Response.StatusCode.value__)). Proceeding."
    } else {
        Write-Host "    remote tag '$Version': free"
    }
}

if (-not $NoBuild) {
    Step "Build $image (multi-stage docker compose build)"
    Push-Location $dockerDir
    try {
        Run "docker" @("compose","build")
    } finally { Pop-Location }
} else {
    Step "Skipping build (-NoBuild). Using existing $image"
    docker image inspect $image 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Local image '$image' not found. Drop -NoBuild or run a build first."
        exit 1
    }
}

Step "Tag $image as $versionTag"
Run "docker" @("tag", $image, $versionTag)

if (-not $NoLatest) {
    Step "Tag $image as $latestTag"
    Run "docker" @("tag", $image, $latestTag)
}

Step "Push $versionTag to GHCR"
Run "docker" @("push", $versionTag)

if (-not $NoLatest) {
    Step "Push $latestTag to GHCR"
    Run "docker" @("push", $latestTag)
}

Step "Verify pushed digest"
if (-not $DryRun) {
    $manifest = (docker manifest inspect $versionTag 2>&1) -join "`n"
    Write-Host "    Local view of $versionTag config digest:"
    ($manifest | Select-String '"digest"' | Select-Object -First 1).Line.Trim()

    $remoteDigest = (Invoke-WebRequest "https://ghcr.io/v2/salaroglio/mdexplorer/manifests/$Version" `
        -Headers $h -Method Head -UseBasicParsing).Headers."Docker-Content-Digest"
    Write-Host "    Anonymous pull manifest digest: $remoteDigest"
}

Step "Done"
Write-Host "    Pushed: $versionTag" -ForegroundColor Green
if (-not $NoLatest) {
    Write-Host "    Pushed: $latestTag" -ForegroundColor Green
}
Write-Host ""
Write-Host "Next: bump version numbers on the website if you want users to see the new tag." -ForegroundColor Yellow
Write-Host "      - mdExplorerWebSite/.../index.html         (download card)"
Write-Host "      - mdExplorerWebSite/.../docs/docker.html   (quick-fact tag)"
