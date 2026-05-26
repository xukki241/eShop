# ============================================================================
# Gemini CLI Auto-Setup Script
# ============================================================================
# Purpose: Automatically setup Gemini CLI with API Key for automation
# Usage: .\setup-gemini-auto.ps1 -ApiKey "YOUR_KEY_HERE"
# ============================================================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiKey,
    
    [Parameter(Mandatory=$false)]
    [switch]$InteractiveSetup
)

# ============================================================================
# CONFIGURATION
# ============================================================================
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Gemini CLI Auto-Setup for eShop Agent Automation          ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# ============================================================================
# STEP 1: Check if Gemini CLI is installed
# ============================================================================
Write-Host "🔍 Step 1: Checking Gemini CLI installation..." -ForegroundColor Yellow

try {
    $geminiVersion = gemini --version 2>&1
    Write-Host "✓ Gemini CLI found: $geminiVersion" -ForegroundColor Green
}
catch {
    Write-Host "✗ Gemini CLI not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Installing Gemini CLI..." -ForegroundColor Yellow
    
    # Check if npm is installed
    try {
        $npmVersion = npm --version 2>&1
        Write-Host "  → npm found: $npmVersion" -ForegroundColor Cyan
    }
    catch {
        Write-Host "✗ npm not found! Please install Node.js first." -ForegroundColor Red
        Write-Host "  Download from: https://nodejs.org/" -ForegroundColor Cyan
        exit 1
    }
    
    # Install Gemini CLI
    Write-Host "  → Installing @google/generative-ai..." -ForegroundColor Yellow
    npm install -g @google/generative-ai
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Gemini CLI installed successfully" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Failed to install Gemini CLI" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""

# ============================================================================
# STEP 2: Get API Key
# ============================================================================
Write-Host "🔑 Step 2: Gemini API Key Setup" -ForegroundColor Yellow

if (-not $ApiKey -and $InteractiveSetup) {
    Write-Host ""
    Write-Host "  How to get your Gemini API Key:" -ForegroundColor Cyan
    Write-Host "  1. Go to: https://aistudio.google.com/apikey" -ForegroundColor Cyan
    Write-Host "  2. Click 'Get API Key'" -ForegroundColor Cyan
    Write-Host "  3. Select or create a Google Cloud project" -ForegroundColor Cyan
    Write-Host "  4. Copy the API Key" -ForegroundColor Cyan
    Write-Host ""
    
    $ApiKey = Read-Host "  Enter your Gemini API Key (or press Ctrl+C to cancel)"
    
    if (-not $ApiKey) {
        Write-Host "✗ No API Key provided" -ForegroundColor Red
        exit 1
    }
}
elseif (-not $ApiKey) {
    Write-Host "  ℹ Use -ApiKey parameter or -InteractiveSetup for manual entry" -ForegroundColor Yellow
    exit 1
}

# Validate API Key format (should be at least 20 chars)
if ($ApiKey.Length -lt 20) {
    Write-Host "✗ API Key seems too short. Please verify it's correct." -ForegroundColor Red
    exit 1
}

Write-Host "✓ API Key received (length: $($ApiKey.Length) chars)" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 3: Set Environment Variables
# ============================================================================
Write-Host "⚙️  Step 3: Setting Environment Variables..." -ForegroundColor Yellow

try {
    # Set for current session
    $env:GEMINI_API_KEY = $ApiKey
    Write-Host "  ✓ Session variable set: GEMINI_API_KEY" -ForegroundColor Green
    
    # Set for user (permanent)
    [System.Environment]::SetEnvironmentVariable("GEMINI_API_KEY", $ApiKey, "User")
    Write-Host "  ✓ User environment variable set (permanent)" -ForegroundColor Green
    
    # Set additional environment variables
    [System.Environment]::SetEnvironmentVariable("GEMINI_MODEL", "gemini-2.0-flash", "User")
    Write-Host "  ✓ GEMINI_MODEL set to: gemini-2.0-flash" -ForegroundColor Green
}
catch {
    Write-Host "✗ Failed to set environment variables: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ============================================================================
# STEP 4: Create .env file (for reference)
# ============================================================================
Write-Host "📝 Step 4: Creating .env file..." -ForegroundColor Yellow

$envContent = @"
# Gemini API Configuration
# Generated: $timestamp
GEMINI_API_KEY=$ApiKey
GEMINI_MODEL=gemini-2.0-flash
GEMINI_TEMPERATURE=0.7
GEMINI_MAX_TOKENS=2000

# Agent Settings
AGENT_LOG_LEVEL=INFO
AGENT_TIMEOUT_SECONDS=300
"@

$envFile = Join-Path (Get-Location) ".env"

try {
    # Check if .env already exists
    if (Test-Path $envFile) {
        Write-Host "  ⚠️ .env file already exists" -ForegroundColor Yellow
        $overwrite = Read-Host "  Overwrite? (y/n)"
        if ($overwrite -ne "y") {
            Write-Host "  → Skipping .env update" -ForegroundColor Yellow
        }
        else {
            Set-Content -Path $envFile -Value $envContent -Force
            Write-Host "  ✓ .env file updated" -ForegroundColor Green
        }
    }
    else {
        Set-Content -Path $envFile -Value $envContent
        Write-Host "  ✓ .env file created: $envFile" -ForegroundColor Green
    }
}
catch {
    Write-Host "  ⚠️ Could not create .env file: $_" -ForegroundColor Yellow
}

# Add to .gitignore
$gitIgnorePath = Join-Path (Get-Location) ".gitignore"
if (Test-Path $gitIgnorePath) {
    if (-not (Get-Content $gitIgnorePath | Select-String "^\.env$")) {
        Add-Content -Path $gitIgnorePath -Value ".env"
        Write-Host "  ✓ Added .env to .gitignore" -ForegroundColor Green
    }
}

Write-Host ""

# ============================================================================
# STEP 5: Test Authentication
# ============================================================================
Write-Host "🧪 Step 5: Testing Gemini API Connection..." -ForegroundColor Yellow

try {
    # Create a simple test call using PowerShell and curl
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    $body = @{
        contents = @(
            @{
                parts = @(
                    @{ text = "Say 'hello' and nothing else" }
                )
            }
        )
    } | ConvertTo-Json
    
    $uri = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=$ApiKey"
    
    Write-Host "  → Sending test request to Gemini API..." -ForegroundColor Cyan
    
    $response = Invoke-WebRequest -Uri $uri -Method POST -Headers $headers -Body $body -ErrorAction Stop
    
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Authentication successful!" -ForegroundColor Green
        Write-Host "  Status Code: $($response.StatusCode)" -ForegroundColor Green
        
        # Parse response
        $responseBody = $response.Content | ConvertFrom-Json
        if ($responseBody.candidates) {
            $message = $responseBody.candidates[0].content.parts[0].text
            Write-Host "  API Response: '$message'" -ForegroundColor Cyan
        }
    }
}
catch {
    Write-Host "✗ Authentication failed: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Verify API Key is correct" -ForegroundColor Yellow
    Write-Host "  2. Check if API is enabled in Google Cloud project" -ForegroundColor Yellow
    Write-Host "  3. Check your internet connection" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# ============================================================================
# STEP 6: Verify Setup
# ============================================================================
Write-Host "✅ Step 6: Verification" -ForegroundColor Yellow

Write-Host ""
Write-Host "  Environment Variables:" -ForegroundColor Cyan
Write-Host "  • GEMINI_API_KEY: $($env:GEMINI_API_KEY.Substring(0, 10))...***" -ForegroundColor Cyan
Write-Host "  • GEMINI_MODEL: $env:GEMINI_MODEL" -ForegroundColor Cyan

Write-Host ""
Write-Host "  Files:" -ForegroundColor Cyan
Write-Host "  • .env: $(if (Test-Path $envFile) { "✓ Created" } else { "✗ Not found" })" -ForegroundColor Cyan
Write-Host "  • .gitignore: $(if (Test-Path $gitIgnorePath) { "✓ Updated" } else { "ℹ Not found" })" -ForegroundColor Cyan

Write-Host ""

# ============================================================================
# SUMMARY & NEXT STEPS
# ============================================================================
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  ✅ Gemini CLI Setup Complete!                            ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host ""
Write-Host "📌 Next Steps:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Close and reopen PowerShell for permanent env vars to take effect" -ForegroundColor White
Write-Host ""
Write-Host "2. Run a quick test:" -ForegroundColor White
Write-Host "   powershell" -ForegroundColor Gray
Write-Host "   `$env:GEMINI_API_KEY  # Verify it's set" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Integrate into agent pipeline:" -ForegroundColor White
Write-Host "   .\agent-pipeline.ps1 -Task all" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Start building with automation:" -ForegroundColor White
Write-Host "   .\agent-pipeline.ps1 -Task quick-build" -ForegroundColor Gray
Write-Host ""

Write-Host "📚 Documentation:" -ForegroundColor Cyan
Write-Host "   • Setup Guide: GEMINI_CLI_SETUP.md" -ForegroundColor Gray
Write-Host "   • Pipeline: agent-pipeline.ps1" -ForegroundColor Gray
Write-Host "   • Agent Tasks: AGENT_TASKS.md" -ForegroundColor Gray
Write-Host ""

Write-Host "💡 Tip: You can now use Gemini CLI in your automation scripts!" -ForegroundColor Yellow
Write-Host ""

