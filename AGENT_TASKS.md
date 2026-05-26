# eShop Agent Automation Tasks

## Overview

This document defines all automated tasks that Gemini Agent can perform in the eShop pipeline.

**Setup:** See `GEMINI_CLI_SETUP.md`

---

## Quick Start

### Option 1: Interactive Setup (Recommended for First Time)
```powershell
.\setup-gemini-auto.ps1 -InteractiveSetup
```

### Option 2: Direct Setup
```powershell
.\setup-gemini-auto.ps1 -ApiKey "YOUR_API_KEY_HERE"
```

### Option 3: Manual Setup
```powershell
[System.Environment]::SetEnvironmentVariable("GEMINI_API_KEY", "YOUR_KEY", "User")
$env:GEMINI_API_KEY = "YOUR_KEY"
```

---

## Tasks Available

### 1. ✅ BUILD ANALYSIS
**Purpose:** Analyze build output and identify issues

```powershell
# Command
.\agent-pipeline.ps1 -Task build

# What it does:
# - Cleans artifacts
# - Restores packages
# - Builds solution
# - Generates build report
```

**Gemini can analyze:**
- Build warnings and errors
- Compilation issues
- Performance bottlenecks
- Dependency conflicts

---

### 2. 🧪 TEST EXECUTION
**Purpose:** Run tests and report failures

```powershell
# Command
.\agent-pipeline.ps1 -Task test

# What it does:
# - Runs all unit tests
# - Runs functional tests
# - Generates test report
# - Identifies failed tests
```

**Gemini can analyze:**
```powershell
# Example: Get test failure analysis
$logFile = Get-ChildItem logs/*.log | Sort-Object LastWriteTime | Select-Object -Last 1
$testOutput = Get-Content $logFile.FullName
Invoke-GeminiAnalysis -Content $testOutput -Task "AnalyzeTestFailures"
```

---

### 3. 🔍 CODE ANALYSIS
**Purpose:** Analyze project structure and dependencies

```powershell
# Command
.\agent-pipeline.ps1 -Task analyze

# What it does:
# - Scans all projects
# - Counts files and classes
# - Detects warnings
# - Generates analysis report
```

**Gemini can analyze:**
- Code quality metrics
- Architecture patterns
- Dependency health
- Potential issues

---

### 4. ⚡ QUICK BUILD
**Purpose:** Fast build without tests

```powershell
# Command
.\agent-pipeline.ps1 -Task quick-build

# What it does:
# - Skips test execution
# - 2-3x faster than full build
# - Good for rapid iterations
```

---

### 5. 🚀 DEPLOY
**Purpose:** Prepare for deployment

```powershell
# Command
.\agent-pipeline.ps1 -Task deploy

# What it does:
# - Checks Docker status
# - Verifies deployment ready
# - Generates deployment report
```

---

### 6. 📊 ALL (Full Pipeline)
**Purpose:** Complete build-test-analyze cycle

```powershell
# Command
.\agent-pipeline.ps1 -Task all

# Executes in order:
# 1. Clean
# 2. Restore
# 3. Build
# 4. Test
# 5. Analyze
```

---

### 7. 📈 STATUS CHECK
**Purpose:** System environment verification

```powershell
# Command
.\agent-pipeline.ps1 -Task status

# Reports:
# - .NET SDK version
# - Node.js status
# - Docker status
# - Environment readiness
```

---

## Integration Examples

### Example 1: Build → Analyze with Gemini

```powershell
# Run build
$buildResult = .\agent-pipeline.ps1 -Task build

# Get latest log
$logFile = Get-ChildItem logs/*.log -File | Sort-Object LastWriteTime | Select-Object -Last 1
$buildLog = Get-Content $logFile.FullName

# Ask Gemini for analysis
$analysis = Invoke-GeminiChat -Message @"
Analyze this build log and provide:
1. Any errors found
2. Warnings that should be addressed
3. Suggestions for improvement
4. Build time optimization tips

Build Log:
$buildLog
"@

Write-Host $analysis
```

### Example 2: Test → Report Generation

```powershell
# Run tests
.\agent-pipeline.ps1 -Task test

# Get test report
$reportFile = Get-ChildItem reports/*.md -File | Sort-Object LastWriteTime | Select-Object -Last 1

# Ask Gemini for executive summary
$summary = Invoke-GeminiChat -Message @"
Based on this test report, create an executive summary:
- Tests passed/failed
- Critical issues to fix
- Next priorities

Report:
$(Get-Content $reportFile.FullName)
"@

# Save summary
$summary | Add-Content -Path $reportFile
```

### Example 3: Continuous Monitoring

```powershell
# Script: continuous-build-monitor.ps1

while ($true) {
    # Run pipeline
    Write-Host "Running pipeline at $(Get-Date)..."
    .\agent-pipeline.ps1 -Task all
    
    # Get latest report
    $report = Get-ChildItem reports/*.md | Sort-Object LastWriteTime | Select-Object -Last 1
    $reportContent = Get-Content $report.FullName
    
    # Ask Gemini for health status
    $health = Invoke-GeminiChat -Message @"
Current build report status: HEALTHY or NEEDS_ATTENTION?
Critical issues: YES or NO?
Recommendation: What should we do next?

$reportContent
"@
    
    Write-Host "📊 Health: $health"
    
    # Check every 30 minutes
    Start-Sleep -Minutes 30
}
```

---

## Advanced: Custom Gemini Tasks

### Create Custom Task Handler

```powershell
# Add to agent-pipeline.ps1

function Task-GeminiAnalyzeErrors {
    param([string]$LogFilePath)
    
    Write-Log "🤖 Gemini: Analyzing errors..." "INFO"
    
    $apiKey = $env:GEMINI_API_KEY
    $logContent = Get-Content $LogFilePath
    
    $payload = @{
        contents = @(
            @{
                parts = @(
                    @{ 
                        text = @"
You are an expert .NET developer. Analyze these build errors and:
1. Identify the root causes
2. Suggest specific fixes
3. Prioritize by severity

Errors:
$logContent
"@
                    }
                )
            }
        )
    } | ConvertTo-Json -Depth 5
    
    $response = Invoke-WebRequest `
        -Uri "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=$apiKey" `
        -Method POST `
        -Headers @{ "Content-Type" = "application/json" } `
        -Body $payload
    
    $result = $response.Content | ConvertFrom-Json
    $analysis = $result.candidates[0].content.parts[0].text
    
    Write-Log "🤖 Analysis: $analysis" "INFO"
    return $analysis
}

# Usage in pipeline:
# $analysis = Task-GeminiAnalyzeErrors -LogFilePath $LogFile
```

---

## Scheduling (Task Scheduler)

### Run Pipeline Daily at 2 AM

```powershell
# Create scheduled task
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
$action = New-ScheduledTaskAction `
    -Execute "powershell.exe" `
    -Argument "-NoProfile -File D:\OJT\RikkeiSoft\DotNet\eShop\agent-pipeline.ps1 -Task all"
$settings = New-ScheduledTaskSettingsSet -RunOnlyIfNetworkAvailable
Register-ScheduledTask `
    -TaskName "eShop-Agent-Pipeline" `
    -Trigger $trigger `
    -Action $action `
    -Settings $settings `
    -RunLevel Highest `
    -Force

# View scheduled tasks
Get-ScheduledTask | Where-Object TaskName -like "*eShop*"

# Test run
Start-ScheduledTask -TaskName "eShop-Agent-Pipeline"
```

---

## Environment Variables

After setup, these are available:

| Variable | Value | Purpose |
|----------|-------|---------|
| `GEMINI_API_KEY` | Your API Key | Authentication |
| `GEMINI_MODEL` | `gemini-2.0-flash` | Model to use (fast) |
| `GEMINI_TEMPERATURE` | `0.7` | Response creativity |
| `GEMINI_MAX_TOKENS` | `2000` | Response length |

Access in scripts:
```powershell
$apiKey = $env:GEMINI_API_KEY
$model = $env:GEMINI_MODEL
```

---

## CI/CD Integration

### GitHub Actions Example

```yaml
# .github/workflows/eshop-build.yml
name: eShop Agent Pipeline

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Setup Gemini CLI
        run: npm install -g @google/generative-ai
      
      - name: Configure Gemini
        env:
          GEMINI_API_KEY: ${{ secrets.GEMINI_API_KEY }}
        run: |
          [System.Environment]::SetEnvironmentVariable("GEMINI_API_KEY", "${{ secrets.GEMINI_API_KEY }}", "User")
      
      - name: Run Pipeline
        run: .\agent-pipeline.ps1 -Task all
      
      - name: Upload Reports
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: build-reports
          path: reports/
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "GEMINI_API_KEY not set" | Run `setup-gemini-auto.ps1` |
| "Gemini not responding" | Check internet connection, verify API key |
| "Tests failing" | Run `.\agent-pipeline.ps1 -Task test -Verbose` |
| "Build taking too long" | Use `quick-build` task, check for large projects |

---

## Success Metrics

Track these metrics in your reports:

- ✅ Build Success Rate
- ✅ Test Pass Rate
- ✅ Average Build Time
- ✅ Code Coverage
- ✅ Number of Warnings
- ✅ Deployment Readiness

---

## Support

For issues, check:
1. `GEMINI_CLI_SETUP.md` - Setup guide
2. `agent-pipeline.ps1` - Implementation
3. `logs/` - Detailed logs
4. `reports/` - Analysis reports

Good luck with your automation! 🚀

