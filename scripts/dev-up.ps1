Param(
    [string]$ApiUrl = "http://localhost:5080"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$root = Resolve-Path "$root\.."
$apiDir = Join-Path $root "src/App2.Api"
$webDir = Join-Path $root "apps/web"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "dotnet SDK is required"
}

if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Error "npm is required"
}

Push-Location $webDir
if (-not (Test-Path node_modules)) {
    Write-Host "Installing web dependencies..."
    npm install
}
Pop-Location

$apiProcess = Start-Process dotnet -ArgumentList "watch run --no-hot-reload --urls $ApiUrl" -WorkingDirectory $apiDir -PassThru
$webProcess = Start-Process npm -ArgumentList "run dev" -WorkingDirectory $webDir -PassThru

Write-Host "API PID: $($apiProcess.Id)"
Write-Host "Web PID: $($webProcess.Id)"
Write-Host "Press Ctrl+C to stop."

try {
    Wait-Process -Id $apiProcess.Id, $webProcess.Id
}
finally {
    if (-not $apiProcess.HasExited) { $apiProcess.Kill() }
    if (-not $webProcess.HasExited) { $webProcess.Kill() }
}
