#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Rename App2 template to a new project name.

.DESCRIPTION
    This script performs cross-platform renaming:
    1. Replaces "App2" with the new name in file contents
    2. Renames files and directories containing "App2"

    Works on Windows, macOS, and Linux PowerShell.

.PARAMETER Name
    The new project name (e.g., "LocationForm")

.EXAMPLE
    pwsh ./scripts/rename-from-template.ps1 -Name LocationForm

.EXAMPLE
    ./scripts/rename-from-template.ps1 LocationForm
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Name
)

$ErrorActionPreference = "Stop"
$Old = "App2"
$New = $Name

Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host " Renaming $Old → $New" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""

# Step 1: Replace contents in files
Write-Host "→ Replacing '$Old' with '$New' in file contents..." -ForegroundColor Yellow

$FilePatterns = @(
    "*.sln",
    "*.csproj",
    "*.cs",
    "*.ts",
    "*.tsx",
    "*.json",
    "*.yml",
    "*.yaml",
    "*.md",
    "*.txt"
)

$ExcludePaths = @("node_modules", ".git", "bin", "obj")

Get-ChildItem -Recurse -File -Include $FilePatterns | Where-Object {
    $path = $_.FullName
    $exclude = $false
    foreach ($excludePath in $ExcludePaths) {
        if ($path -like "*\$excludePath\*" -or $path -like "*/$excludePath/*") {
            $exclude = $true
            break
        }
    }
    -not $exclude
} | ForEach-Object {
    # Read, replace, and write back with UTF8 encoding (preserves line endings)
    $content = Get-Content $_.FullName -Raw
    $newContent = $content -replace $Old, $New

    if ($content -ne $newContent) {
        # Use UTF8 without BOM, preserve original line endings
        [System.IO.File]::WriteAllText($_.FullName, $newContent, [System.Text.UTF8Encoding]::new($false))
    }
}

Write-Host "  ✓ Content replacement complete" -ForegroundColor Green

# Step 2: Rename files and directories
Write-Host ""
Write-Host "→ Renaming files and directories containing '$Old'..." -ForegroundColor Yellow

# Find all paths containing Old, sort by depth (longest first)
Get-ChildItem -Recurse -Filter "*$Old*" | Where-Object {
    $path = $_.FullName
    $exclude = $false
    foreach ($excludePath in $ExcludePaths) {
        if ($path -like "*\$excludePath\*" -or $path -like "*/$excludePath/*") {
            $exclude = $true
            break
        }
    }
    -not $exclude
} | Sort-Object { $_.FullName.Length } -Descending | ForEach-Object {
    $oldPath = $_.FullName
    $newName = $_.Name -replace $Old, $New

    if ($newName -ne $_.Name) {
        $newPath = Join-Path $_.Directory.FullName $newName
        Move-Item -LiteralPath $oldPath -Destination $newPath
        Write-Host "  ✓ Renamed: $oldPath → $newPath" -ForegroundColor Green
    }
}

Write-Host "  ✓ Path renaming complete" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " ✓ Renaming Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Review changes: git status"
Write-Host "  2. Test build: dotnet build && cd apps/web && npm run build"
Write-Host "  3. Commit changes: git add -A && git commit -m 'chore: rename $Old → $New'"
Write-Host ""
