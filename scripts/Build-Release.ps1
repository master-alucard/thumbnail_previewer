<#
.SYNOPSIS
    Builds the ThumbnailPreviewer in Release mode and optionally creates the installer.
#>

param(
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"
$rootDir = Split-Path $PSScriptRoot -Parent

Write-Host "=== Building ThumbnailPreviewer ===" -ForegroundColor Cyan

# Build
$slnPath = Join-Path $rootDir "ThumbnailPreviewer.sln"
dotnet build $slnPath -c Release -p:Platform=x64
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Copy to dist
$distDir = Join-Path $rootDir "dist"
$binDir = Join-Path $rootDir "src\ThumbnailPreviewer\bin\x64\Release\net48"

if (Test-Path $distDir) { Remove-Item $distDir -Recurse -Force }
New-Item $distDir -ItemType Directory | Out-Null

Copy-Item "$binDir\*" $distDir -Recurse
Write-Host "Output copied to: $distDir" -ForegroundColor Green

# Clean non-Windows platform files (SkiaSharp/PDFium ship native libs for all platforms)
Write-Host "Cleaning non-Windows native libraries..." -ForegroundColor Gray
$removeDirs = @("arm", "arm64", "x86", "loongarch64", "riscv64",
    "musl-arm", "musl-arm64", "musl-x64", "musl-x86",
    "musl-loongarch64", "musl-riscv64")
foreach ($dir in $removeDirs) {
    $path = Join-Path $distDir $dir
    if (Test-Path $path) { Remove-Item $path -Recurse -Force }
}
# Remove macOS .dylib and Linux .so files
Get-ChildItem $distDir -Filter "*.dylib" -Recurse | Remove-Item -Force
Get-ChildItem $distDir -Filter "*.so" -Recurse | Remove-Item -Force
Write-Host "Cleaned — Windows x64 only." -ForegroundColor Green

# Build installer
if (-not $SkipInstaller) {
    $issPath = Join-Path $rootDir "src\ThumbnailPreviewer.Installer\installer.iss"
    if (Test-Path $issPath) {
        # Search common Inno Setup locations
        $isccPaths = @(
            "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
            "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
            "C:\Program Files\Inno Setup 6\ISCC.exe"
        )
        $iscc = $isccPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

        if ($iscc) {
            Write-Host "`n=== Building Installer ===" -ForegroundColor Cyan
            & $iscc $issPath
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Installer build failed!"
                exit 1
            }
            Write-Host "Installer built successfully!" -ForegroundColor Green
        } else {
            Write-Warning "Inno Setup not found. Searched:`n  $($isccPaths -join "`n  ")`nSkipping installer build."
        }
    } else {
        Write-Warning "Installer script not found. Skipping."
    }
}

Write-Host "`n=== Build Complete ===" -ForegroundColor Cyan
Write-Host "DLL: $distDir\ThumbnailPreviewer.dll"
$setupExe = Join-Path $rootDir "src\ThumbnailPreviewer.Installer\Output\ThumbnailPreviewerSetup-*.exe"
if (Test-Path $setupExe) {
    Write-Host "Installer: $(Resolve-Path $setupExe)" -ForegroundColor Green
}
