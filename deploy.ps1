# =============================================================================
# SETS - IIS Deployment Script
# =============================================================================
# Usage:
#   .\deploy.ps1                          # Deploy to default output folder
#   .\deploy.ps1 -OutputPath "C:\deploy"  # Deploy to custom folder
#   .\deploy.ps1 -SkipFrontend           # Skip Vue build (backend only)
#   .\deploy.ps1 -SkipBackend            # Skip .NET publish (frontend only)
# =============================================================================

param (
    [string]$OutputPath = "$PSScriptRoot\publish",
    [switch]$SkipFrontend,
    [switch]$SkipBackend
)

# --- Config ------------------------------------------------------------------
$SolutionRoot   = $PSScriptRoot
$ServerProject  = "$SolutionRoot\SETS.Server\SETS.Server.csproj"   # was LabStockPlus.Server
$FrontendDir    = "$SolutionRoot\sets.client"                        # was labstockplus.client
$WwwrootTarget  = "$OutputPath\wwwroot"

# --- Helpers -----------------------------------------------------------------
function Write-Step { param([string]$Msg) Write-Host "`n>>> $Msg" -ForegroundColor Cyan }
function Write-OK   { param([string]$Msg) Write-Host "    [OK] $Msg" -ForegroundColor Green }
function Write-Fail { param([string]$Msg) Write-Host "    [FAIL] $Msg" -ForegroundColor Red; exit 1 }

function Require-Command {
    param([string]$Cmd, [string]$InstallHint)
    if (-not (Get-Command $Cmd -ErrorAction SilentlyContinue)) {
        Write-Fail "$Cmd not found. $InstallHint"
    }
}

# --- Prereq checks -----------------------------------------------------------
Write-Step "Checking prerequisites..."

Require-Command "dotnet" "Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0"
$dotnetVersion = dotnet --version
Write-OK ".NET SDK $dotnetVersion found"

if (-not $SkipFrontend) {
    Require-Command "node" "Install Node.js 20+ from https://nodejs.org"
    Require-Command "npm"  "npm is bundled with Node.js"
    $nodeVersion = node --version
    $npmVersion  = npm --version
    Write-OK "Node $nodeVersion / npm $npmVersion found"
}

# --- Clean output ------------------------------------------------------------
Write-Step "Cleaning output folder: $OutputPath"
if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
    Write-OK "Cleaned existing output"
}
New-Item -ItemType Directory -Path $OutputPath | Out-Null
Write-OK "Created fresh output folder"

# --- Build frontend ----------------------------------------------------------
if (-not $SkipFrontend) {
    Write-Step "Installing frontend dependencies (npm install)..."
    Push-Location $FrontendDir
    npm install
    if ($LASTEXITCODE -ne 0) { Write-Fail "npm install failed" }
    Write-OK "Dependencies installed"

    Write-Step "Building Vue 3 frontend (npm run build)..."
    npm run build
    if ($LASTEXITCODE -ne 0) { Write-Fail "npm run build failed" }
    Write-OK "Frontend built successfully"
    Pop-Location

    $DistDir = "$FrontendDir\dist"
    if (-not (Test-Path $DistDir)) {
        Write-Fail "Frontend dist folder not found at $DistDir"
    }
}

# --- Publish backend ---------------------------------------------------------
if (-not $SkipBackend) {
    Write-Step "Publishing .NET 8 backend..."
    dotnet publish $ServerProject `
        --configuration Release `
        --runtime win-x64 `
        --self-contained false `
        --output $OutputPath `
        /p:EnvironmentName=Production

    if ($LASTEXITCODE -ne 0) { Write-Fail "dotnet publish failed" }
    Write-OK "Backend published to $OutputPath"
}

# --- Copy frontend dist -> publish\wwwroot (AFTER dotnet publish) ------------
if (-not $SkipFrontend) {
    Write-Step "Copying frontend dist to wwwroot..."
    New-Item -ItemType Directory -Force -Path $WwwrootTarget | Out-Null
    Copy-Item -Recurse -Force "$DistDir\*" $WwwrootTarget
    Write-OK "Frontend copied to $WwwrootTarget"
}


# --- Create logs folder (required for stdout logging) ------------------------
Write-Step "Creating logs folder..."
New-Item -ItemType Directory -Force -Path "$OutputPath\logs" | Out-Null
Write-OK "Logs folder created"

# --- Summary -----------------------------------------------------------------
Write-Host ""
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host "  Deployment package ready!" -ForegroundColor Yellow
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host "  Output  : $OutputPath" -ForegroundColor White
Write-Host ""
Write-Host "  Next steps:" -ForegroundColor White
Write-Host "  1. Copy '$OutputPath' to your IIS server" -ForegroundColor Gray
Write-Host "  2. Update appsettings.json connection strings for production" -ForegroundColor Gray
Write-Host "  3. In IIS Manager:" -ForegroundColor Gray
Write-Host "       - Create App Pool: No Managed Code, Integrated, max 1 worker" -ForegroundColor Gray
Write-Host "       - Create Site pointing to the copied folder" -ForegroundColor Gray
Write-Host "  4. Grant IIS AppPool identity Read/Execute on the folder:" -ForegroundColor Gray
Write-Host "       icacls <folder> /grant `"IIS AppPool\<PoolName>:(OI)(CI)RX`"" -ForegroundColor Gray
Write-Host "  5. Ensure .NET 8 Hosting Bundle is installed on the server" -ForegroundColor Gray
Write-Host "       https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Gray
Write-Host "============================================================" -ForegroundColor Yellow
