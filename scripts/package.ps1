#!/usr/bin/env pwsh

<#
.SYNOPSIS
    NuGet Package Creation and Versioning Script

.DESCRIPTION
    Demonstrates different approaches to creating NuGet packages and managing versions.
    Supports multiple versioning strategies and packaging scenarios.

.PARAMETER Strategy
    Versioning strategy to use: Manual, GitVersion, BuildNumber, Timestamp

.PARAMETER PreRelease
    Pre-release label (alpha, beta, rc)

.PARAMETER BuildNumber
    Build number for CI/CD scenarios

.PARAMETER OutputPath
    Output directory for packages (default: ./packages)

.EXAMPLE
    .\scripts\package.ps1 -Strategy Manual
    .\scripts\package.ps1 -Strategy GitVersion -PreRelease alpha
    .\scripts\package.ps1 -Strategy BuildNumber -BuildNumber 123
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Manual", "GitVersion", "BuildNumber", "Timestamp")]
    [string]$Strategy = "Manual",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("alpha", "beta", "rc")]
    [string]$PreRelease,
    
    [Parameter(Mandatory = $false)]
    [int]$BuildNumber,
    
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "./packages"
)

# Ensure output directory exists
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# Define the project path
$ProjectPath = "WideWorldImporters.Domain/WideWorldImporters.Domain.csproj"

Write-Host "🚀 Creating NuGet package using strategy: $Strategy" -ForegroundColor Green

# Function to execute dotnet pack with specific parameters
function Invoke-PackCommand {
    param(
        [hashtable]$Properties,
        [string]$Suffix = ""
    )
    
    $msbuildArgs = @()
    foreach ($prop in $Properties.GetEnumerator()) {
        $msbuildArgs += "-p:$($prop.Key)=$($prop.Value)"
    }
    
    $packageName = if ($Suffix) { "package-$Suffix" } else { "package" }
    
    Write-Host "Building package with properties:" -ForegroundColor Yellow
    foreach ($prop in $Properties.GetEnumerator()) {
        Write-Host "  $($prop.Key): $($prop.Value)" -ForegroundColor Gray
    }
    
    $cmd = "dotnet pack `"$ProjectPath`" --configuration Release --output `"$OutputPath`" $($msbuildArgs -join ' ')"
    Write-Host "Executing: $cmd" -ForegroundColor Cyan
    
    Invoke-Expression $cmd
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Package created successfully" -ForegroundColor Green
    } else {
        Write-Host "Package creation failed" -ForegroundColor Red
        exit 1
    }
}

# Strategy implementations
switch ($Strategy) {
    "Manual" {
        $props = @{
            "VersionPrefix" = "1.0.1"
        }
        
        if ($PreRelease) {
            $props["VersionSuffix"] = $PreRelease
        }
        
        Invoke-PackCommand -Properties $props -Suffix "manual"
    }
    
    "GitVersion" {
        # Check if GitVersion is available
        try {
            $gitVersion = dotnet tool list -g | Select-String "gitversion"
            if (-not $gitVersion) {
                Write-Host "Installing GitVersion tool..." -ForegroundColor Yellow
                dotnet tool install --global GitVersion.Tool
            }
            
            # Get version from Git
            $versionInfo = dotnet gitversion | ConvertFrom-Json
            
            $props = @{
                "Version" = $versionInfo.FullSemVer
                "AssemblyVersion" = $versionInfo.AssemblySemVer
                "FileVersion" = $versionInfo.AssemblySemFileVer
                "PackageVersion" = $versionInfo.FullSemVer
            }
            
            Invoke-PackCommand -Properties $props -Suffix "gitversion"
        }
        catch {
            Write-Host "GitVersion strategy failed: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "Falling back to manual strategy..." -ForegroundColor Yellow
            
            $props = @{
                "VersionPrefix" = "1.0.0"
                "VersionSuffix" = "git-$(git rev-parse --short HEAD)"
            }
            
            Invoke-PackCommand -Properties $props -Suffix "git-fallback"
        }
    }
    
    "BuildNumber" {
        if (-not $BuildNumber) {
            $BuildNumber = (Get-Date).ToString("yyyyMMddHHmm")
        }
        
        $props = @{
            "VersionPrefix" = "1.0.0"
            "BuildNumber" = $BuildNumber
            "FileVersion" = "1.0.0.$BuildNumber"
        }
        
        if ($PreRelease) {
            $props["VersionSuffix"] = "$PreRelease.$BuildNumber"
        } else {
            $props["VersionSuffix"] = "build.$BuildNumber"
        }
        
        Invoke-PackCommand -Properties $props -Suffix "build"
    }
    
    "Timestamp" {
        $timestamp = (Get-Date).ToString("yyyyMMddHHmmss")
        
        $props = @{
            "VersionPrefix" = "1.0.0"
            "VersionSuffix" = "dev.$timestamp"
            "FileVersion" = "1.0.0.0"
        }
        
        Invoke-PackCommand -Properties $props -Suffix "timestamp"
    }
}

# Display created packages
Write-Host "`n📦 Created packages:" -ForegroundColor Green
Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | ForEach-Object {
    Write-Host "  $($_.Name)" -ForegroundColor Cyan
}

# Package validation (if available)
$latestPackage = Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($latestPackage) {
    Write-Host "`n🔍 Package Analysis:" -ForegroundColor Green
    Write-Host "Latest package: $($latestPackage.Name)" -ForegroundColor Cyan
    Write-Host "Size: $([math]::Round($latestPackage.Length / 1KB, 2)) KB" -ForegroundColor Gray
    
    # Try to inspect package contents (requires nuget.exe or unzip)
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $archive = [System.IO.Compression.ZipFile]::OpenRead($latestPackage.FullName)
        
        Write-Host "`nPackage contents:" -ForegroundColor Yellow
        $archive.Entries | ForEach-Object {
            Write-Host "  $($_.FullName)" -ForegroundColor Gray
        }
        
        $archive.Dispose()
    }
    catch {
        Write-Host "Unable to inspect package contents" -ForegroundColor Yellow
    }
}

Write-Host "`n✨ Package creation complete!" -ForegroundColor Green 