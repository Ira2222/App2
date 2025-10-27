Param(
    [string]$BaseUrl = "http://localhost:5080"
)

$ErrorActionPreference = "Stop"

$live = Invoke-WebRequest -Uri "$BaseUrl/healthz/live" -UseBasicParsing
Write-Host "Live health:" $live.Content

$ready = Invoke-WebRequest -Uri "$BaseUrl/healthz/ready" -UseBasicParsing
Write-Host "Ready health:" $ready.Content

$todos = Invoke-WebRequest -Uri "$BaseUrl/api/todos" -UseBasicParsing -Headers @{ Accept = "application/json" }
Write-Host "Todos:" $todos.Content

try {
    $docs = Invoke-WebRequest -Uri "$BaseUrl/reference" -UseBasicParsing
    Write-Host "Scalar UI available at $BaseUrl/reference"
} catch {
    Write-Warning "Scalar UI not reachable."
}
