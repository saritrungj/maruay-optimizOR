# build.ps1 — JaiDee-Optimize build helper
# Usage: .\build.ps1 [Debug|Release]

param([string]$Configuration = "Debug")

$candidates = @(
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
)

$msbuild = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $msbuild) {
    Write-Error "MSBuild.exe not found. Install Visual Studio or Build Tools."
    exit 1
}

Write-Host "Using MSBuild: $msbuild" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan

& $msbuild "$PSScriptRoot\optimizOR\optimizOR.csproj" `
    /p:Configuration=$Configuration `
    /t:Rebuild `
    /verbosity:minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild SUCCEEDED -> optimizOR\bin\$Configuration\JaiDee-Optimize.exe" -ForegroundColor Green
} else {
    Write-Host "`nBuild FAILED." -ForegroundColor Red
    exit 1
}
