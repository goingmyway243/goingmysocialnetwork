#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Run GoingMy Social Network - Web, Services, or Both

.DESCRIPTION
    Manages running the Angular web application and/or .NET services.
    - 'web': Runs Angular development server (npm start)
    - 'services': Runs .NET Aspire AppHost (dotnet watch run) in a new terminal window
    - No argument or 'all': Runs both web and services

.PARAMETER Target
    The target to run: 'web', 'services', or 'all' (default)

.EXAMPLE
    .\run.ps1 web
    .\run.ps1 services
    .\run.ps1
    .\run.ps1 all
#>

param(
    [string]$Target = "all"
)

function Start-WebApp {
    Write-Host "`n======================================" -ForegroundColor Cyan
    Write-Host "Starting Angular Web Application" -ForegroundColor Green
    Write-Host "======================================`n" -ForegroundColor Cyan
    
    $webPath = "src/GoingMy.Web"
    if (-not (Test-Path $webPath)) {
        Write-Host "ERROR: Web project not found at $webPath" -ForegroundColor Red
        return $false
    }
    
    Push-Location $webPath
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
    
    # Check if node_modules doesn't exist
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing npm dependencies..." -ForegroundColor Yellow
        npm install
        if (-not $?) {
            Write-Host "ERROR: npm install failed" -ForegroundColor Red
            Pop-Location
            return $false
        }
    }
    
    Write-Host "Running: npm start`n" -ForegroundColor Yellow
    npm start
    Pop-Location
    return $?
}

function Start-Services {
    Write-Host "`n======================================" -ForegroundColor Cyan
    Write-Host "Starting .NET Aspire Services" -ForegroundColor Green
    Write-Host "======================================`n" -ForegroundColor Cyan
    
    $appHostPath = "src/GoingMy.AppHost"
    if (-not (Test-Path $appHostPath)) {
        Write-Host "ERROR: AppHost project not found at $appHostPath" -ForegroundColor Red
        return $false
    }
    
    Write-Host "Launching Services in a new terminal window...`n" -ForegroundColor Yellow
    
    $serviceScript = @"
    Set-Location "$appHostPath"
    Write-Host "Current directory: `$(Get-Location)" -ForegroundColor Yellow
    Write-Host "Running: dotnet watch run`n" -ForegroundColor Yellow
    dotnet run
    Read-Host "Press Enter to close this window"
"@
    
    Start-Process PowerShell -ArgumentList "-NoExit -Command $serviceScript"
    return $true
}

# Validate target parameter
$validTargets = @("web", "services", "all")
if ($Target -notin $validTargets) {
    Write-Host "ERROR: Invalid target: '$Target'" -ForegroundColor Red
    Write-Host "Valid options: web, services, all" -ForegroundColor Yellow
    exit 1
}

Write-Host @"
=====================================
   GoingMy Social Network - Launcher
=====================================
Target: $Target

"@ -ForegroundColor Cyan

switch ($Target) {
    "web" {
        Start-WebApp
    }
    "services" {
        Start-Services
    }
    "all" {
        Write-Host "Starting both Web App and Services...`n" -ForegroundColor Cyan
        
        # Start services in new terminal window
        Start-Services
        
        Write-Host "Waiting 3 seconds for services to initialize...`n" -ForegroundColor Yellow
        Start-Sleep -Seconds 3
        
        # Start web in foreground
        Start-WebApp
    }
}

Write-Host "`nCompleted" -ForegroundColor Green
