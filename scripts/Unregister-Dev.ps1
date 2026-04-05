#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Unregisters the ThumbnailPreviewer shell extension.
#>

param(
    [string]$Configuration = "Debug",
    [switch]$NoRestartExplorer
)

$ErrorActionPreference = "Stop"

$dllPath = Join-Path $PSScriptRoot "..\src\ThumbnailPreviewer\bin\x64\$Configuration\net48\ThumbnailPreviewer.dll"
$dllPath = [System.IO.Path]::GetFullPath($dllPath)

if (-not (Test-Path $dllPath)) {
    Write-Error "DLL not found at: $dllPath"
    exit 1
}

$regasm = Join-Path $env:windir "Microsoft.NET\Framework64\v4.0.30319\regasm.exe"
if (-not (Test-Path $regasm)) {
    Write-Error "regasm.exe not found at: $regasm"
    exit 1
}

Write-Host "Unregistering: $dllPath" -ForegroundColor Cyan

& $regasm /unregister $dllPath 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Unregistration may have partially failed (exit code $LASTEXITCODE)"
}

Write-Host "Unregistration complete." -ForegroundColor Green

if (-not $NoRestartExplorer) {
    Write-Host "Restarting Explorer..." -ForegroundColor Yellow
    Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Start-Process explorer
    Write-Host "Explorer restarted." -ForegroundColor Green
}
