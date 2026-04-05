#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Registers the ThumbnailPreviewer shell extension for development/testing.
.DESCRIPTION
    Uses regasm to register the COM DLL with the /codebase flag.
    Restarts Explorer to apply changes.
#>

param(
    [string]$Configuration = "Debug",
    [switch]$NoRestartExplorer
)

$ErrorActionPreference = "Stop"

$dllPath = Join-Path $PSScriptRoot "..\src\ThumbnailPreviewer\bin\x64\$Configuration\net48\ThumbnailPreviewer.dll"
$dllPath = [System.IO.Path]::GetFullPath($dllPath)

if (-not (Test-Path $dllPath)) {
    Write-Error "DLL not found at: $dllPath`nBuild the project first with: dotnet build -c $Configuration"
    exit 1
}

# Find regasm.exe for .NET Framework 4.x (64-bit)
$regasm = Join-Path $env:windir "Microsoft.NET\Framework64\v4.0.30319\regasm.exe"
if (-not (Test-Path $regasm)) {
    Write-Error "regasm.exe not found at: $regasm"
    exit 1
}

Write-Host "Registering: $dllPath" -ForegroundColor Cyan
Write-Host "Using regasm: $regasm" -ForegroundColor Gray

& $regasm $dllPath /codebase /tlb 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Registration failed with exit code $LASTEXITCODE"
    exit 1
}

Write-Host "Registration successful!" -ForegroundColor Green

if (-not $NoRestartExplorer) {
    Write-Host "Restarting Explorer..." -ForegroundColor Yellow
    Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Start-Process explorer
    Write-Host "Explorer restarted." -ForegroundColor Green
}

Write-Host "`nDone. Open a folder with supported files and switch to 'Large icons' view to test." -ForegroundColor Cyan
