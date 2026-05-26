# ============================================================================
# eShop Agent Automation Pipeline - GitHub Copilot & Gemini CLI Integration
# ============================================================================
# Purpose: Automate build, test, analysis, and monitoring for eShop project
# Usage: .\agent-pipeline.ps1 -Task "build|test|analyze|all|deploy"
# ============================================================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("build", "test", "analyze", "all", "quick-build", "deploy", "status")]
    [string]$Task = "all",
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Debug",
    
    [Parameter(Mandatory=$false)]
    [bool]$Verbose = $true,
    
    [Parameter(Mandatory=$false)]
    [bool]$ReportToGemini = $false
)

# ============================================================================
# CONFIGURATION
# ============================================================================
$RootPath = Get-Location
$SrcPath = Join-Path $RootPath "src"
$TestsPath = Join-Path $RootPath "tests"
$AppHostPath = Join-Path $SrcPath "eShop.AppHost"
$LogPath = Join-Path $RootPath "logs"
$ReportPath = Join-Path $RootPath "reports"
$TimestampFormat = "yyyy-MM-dd HH:mm:ss"
$BuildStartTime = Get-Date

# Create log directories
if (!(Test-Path $LogPath)) { New-Item -ItemType Directory -Path $LogPath | Out-Null }
if (!(Test-Path $ReportPath)) { New-Item -ItemType Directory -Path $ReportPath | Out-Null }

$LogFile = Join-Path $LogPath "agent-pipeline-$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
$ReportFile = Join-Path $ReportPath "build-report-$(Get-Date -Format 'yyyyMMdd_HHmmss').md"

# ============================================================================
# LOGGING FUNCTIONS
# ============================================================================
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format $TimestampFormat
    $logMessage = "[$timestamp] [$Level] $Message"
    
    Add-Content -Path $LogFile -Value $logMessage
    
    if ($Verbose) {
        $color = switch($Level) {
            "ERROR" { "Red" }
            "WARN" { "Yellow" }
            "SUCCESS" { "Green" }
            "INFO" { "Cyan" }
            default { "White" }
        }
        Write-Host $logMessage -ForegroundColor $color
    }
}

function Write-Report {
    param([string]$Content)
    Add-Content -Path $ReportFile -Value $Content
}

# ============================================================================
# TASK FUNCTIONS
# ============================================================================

function Task-Clean {
    Write-Log "🧹 Cleaning artifacts..." "INFO"
    
    try {
        $artifactsPath = Join-Path $RootPath "artifacts"
        if (Test-Path $artifactsPath) {
            Remove-Item -Path "$artifactsPath\*" -Recurse -Force -ErrorAction SilentlyContinue
            Write-Log "✓ Artifacts cleaned" "SUCCESS"
        }
    }
    catch {
        Write-Log "✗ Failed to clean artifacts: $_" "ERROR"
        return $false
    }
    return $true
}

function Task-Restore {
    Write-Log "📦 Restoring NuGet packages..." "INFO"
    
    try {
        Push-Location $RootPath
        dotnet restore --no-cache 2>&1 | Tee-Object -FilePath $LogFile -Append
        
        if ($LASTEXITCODE -eq 0) {
            Write-Log "✓ Restore completed successfully" "SUCCESS"
            return $true
        }
        else {
            Write-Log "✗ Restore failed with exit code $LASTEXITCODE" "ERROR"
            return $false
        }
    }
    catch {
        Write-Log "✗ Restore error: $_" "ERROR"
        return $false
    }
    finally {
        Pop-Location
    }
}

function Task-Build {
    Write-Log "🔨 Building solution ($Configuration)..." "INFO"
    
    try {
        Push-Location $RootPath
        $buildOutput = dotnet build eShop.slnx -c $Configuration --no-restore --output artifacts/bin 2>&1
        $buildOutput | Add-Content -Path $LogFile
        
        if ($LASTEXITCODE -eq 0) {
            Write-Log "✓ Build completed successfully" "SUCCESS"
            return @{ Success = $true; Output = $buildOutput }
        }
        else {
            Write-Log "✗ Build failed with exit code $LASTEXITCODE" "ERROR"
            return @{ Success = $false; Output = $buildOutput }
        }
    }
    catch {
        Write-Log "✗ Build error: $_" "ERROR"
        return @{ Success = $false; Output = $_ }
    }
    finally {
        Pop-Location
    }
}

function Task-Test {
    Write-Log "🧪 Running tests..." "INFO"
    
    try {
        Push-Location $RootPath
        
        # Run unit tests
        Write-Log "  → Running Unit Tests..." "INFO"
        dotnet test tests/ -c $Configuration --no-build --logger "console;verbosity=minimal" 2>&1 | Tee-Object -FilePath $LogFile -Append
        
        if ($LASTEXITCODE -eq 0) {
            Write-Log "✓ Tests passed" "SUCCESS"
            return @{ Success = $true }
        }
        else {
            Write-Log "✗ Tests failed with exit code $LASTEXITCODE" "WARN"
            return @{ Success = $false }
        }
    }
    catch {
        Write-Log "✗ Test error: $_" "ERROR"
        return @{ Success = $false }
    }
    finally {
        Pop-Location
    }
}

function Task-Analyze {
    Write-Log "🔍 Analyzing project structure and dependencies..." "INFO"
    
    try {
        Push-Location $RootPath
        
        # Get project structure
        Write-Log "  → Scanning projects..." "INFO"
        $projects = Get-ChildItem -Path $SrcPath -Filter "*.csproj" -Recurse
        $testProjects = Get-ChildItem -Path $TestsPath -Filter "*.csproj" -Recurse
        
        Write-Log "✓ Found $($projects.Count) source projects and $($testProjects.Count) test projects" "SUCCESS"
        
        # List all projects
        Write-Log "Projects:" "INFO"
        $projects | ForEach-Object { Write-Log "  - $($_.Name)" "INFO" }
        
        # Check for warnings
        Write-Log "  → Checking for compiler warnings..." "INFO"
        $buildWarnings = dotnet build eShop.slnx -c $Configuration --no-restore -v minimal 2>&1 | Select-String "warning"
        
        if ($buildWarnings) {
            Write-Log "⚠ Found $($buildWarnings.Count) compiler warnings" "WARN"
        }
        
        return @{ 
            Success = $true
            ProjectCount = $projects.Count
            TestProjectCount = $testProjects.Count
            Warnings = $buildWarnings.Count
        }
    }
    catch {
        Write-Log "✗ Analysis error: $_" "ERROR"
        return @{ Success = $false }
    }
    finally {
        Pop-Location
    }
}

function Task-QuickBuild {
    Write-Log "⚡ Quick Build (skip tests)..." "INFO"
    
    $results = @{}
    
    if (Task-Restore) {
        $results.Restore = $true
        $buildResult = Task-Build
        $results.Build = $buildResult.Success
    }
    else {
        $results.Restore = $false
        $results.Build = $false
    }
    
    return $results
}

function Task-Deploy {
    Write-Log "🚀 Deploy to Docker (if Docker running)..." "INFO"
    
    try {
        $dockerCheck = docker ps --no-trunc 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Log "✓ Docker is running" "SUCCESS"
            Write-Log "  → Ready for container deployment" "INFO"
            return @{ Success = $true; DockerRunning = $true }
        }
        else {
            Write-Log "⚠ Docker is not running - skipping deployment" "WARN"
            return @{ Success = $false; DockerRunning = $false }
        }
    }
    catch {
        Write-Log "✗ Deploy error: $_" "ERROR"
        return @{ Success = $false }
    }
}

function Task-Status {
    Write-Log "📊 System Status Check..." "INFO"
    
    $status = @{}
    
    # Check .NET SDK
    $dotnetVersion = dotnet --version 2>&1
    Write-Log "  → .NET SDK: $dotnetVersion" "INFO"
    $status.DotNetSDK = $dotnetVersion
    
    # Check Node.js (for frontend)
    try {
        $nodeVersion = node --version 2>&1
        Write-Log "  → Node.js: $nodeVersion" "INFO"
        $status.Node = $nodeVersion
    }
    catch {
        Write-Log "  → Node.js: Not found" "WARN"
        $status.Node = "Not installed"
    }
    
    # Check Docker
    try {
        $dockerVersion = docker --version 2>&1
        Write-Log "  → Docker: $dockerVersion" "INFO"
        $status.Docker = $dockerVersion
    }
    catch {
        Write-Log "  → Docker: Not found" "WARN"
        $status.Docker = "Not installed"
    }
    
    return $status
}

# ============================================================================
# REPORT GENERATION
# ============================================================================
function Generate-Report {
    param([hashtable]$Results)
    
    Write-Report "# eShop Agent Automation Report"
    Write-Report ""
    Write-Report "**Generated:** $(Get-Date -Format $TimestampFormat)"
    Write-Report "**Configuration:** $Configuration"
    Write-Report ""
    
    Write-Report "## Build Summary"
    Write-Report ""
    
    if ($Results.Build) {
        Write-Report "- ✅ **Build**: SUCCESS"
    }
    else {
        Write-Report "- ❌ **Build**: FAILED"
    }
    
    if ($Results.ContainsKey("Test")) {
        if ($Results.Test) {
            Write-Report "- ✅ **Tests**: PASSED"
        }
        else {
            Write-Report "- ⚠️ **Tests**: FAILED (check logs)"
        }
    }
    
    Write-Report ""
    Write-Report "## Project Analysis"
    Write-Report ""
    
    if ($Results.ContainsKey("Analysis")) {
        Write-Report "- Source Projects: $($Results.Analysis.ProjectCount)"
        Write-Report "- Test Projects: $($Results.Analysis.TestProjectCount)"
        Write-Report "- Compiler Warnings: $($Results.Analysis.Warnings)"
    }
    
    Write-Report ""
    Write-Report "## Logs"
    Write-Report ""
    Write-Report "Full logs saved to: `logs/agent-pipeline-*.log`"
    
    Write-Log "📄 Report generated: $(Split-Path $ReportFile -Leaf)" "SUCCESS"
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

Write-Log "╔════════════════════════════════════════════════════════════╗" "INFO"
Write-Log "║  eShop Agent Automation Pipeline Started                  ║" "INFO"
Write-Log "╚════════════════════════════════════════════════════════════╝" "INFO"
Write-Log ""
Write-Log "Task: $Task | Configuration: $Configuration" "INFO"
Write-Log "Root Path: $RootPath" "INFO"
Write-Log ""

$results = @{ Timestamp = $BuildStartTime }

# Execute tasks based on parameter
switch($Task) {
    "quick-build" {
        $results += Task-QuickBuild
    }
    
    "build" {
        Task-Clean | Out-Null
        Task-Restore | Out-Null
        $buildResult = Task-Build
        $results.Build = $buildResult.Success
    }
    
    "test" {
        $testResult = Task-Test
        $results.Test = $testResult.Success
    }
    
    "analyze" {
        $analysisResult = Task-Analyze
        $results.Analysis = $analysisResult
    }
    
    "deploy" {
        $deployResult = Task-Deploy
        $results.Deploy = $deployResult
    }
    
    "status" {
        $statusResult = Task-Status
        $results.Status = $statusResult
    }
    
    "all" {
        Task-Clean | Out-Null
        $results.Restore = Task-Restore
        $buildResult = Task-Build
        $results.Build = $buildResult.Success
        $testResult = Task-Test
        $results.Test = $testResult.Success
        $analysisResult = Task-Analyze
        $results.Analysis = $analysisResult
    }
}

# ============================================================================
# SUMMARY
# ============================================================================
Write-Log ""
Write-Log "╔════════════════════════════════════════════════════════════╗" "INFO"
Write-Log "║  Pipeline Execution Summary                               ║" "INFO"
Write-Log "╚════════════════════════════════════════════════════════════╝" "INFO"

$duration = (Get-Date) - $BuildStartTime
Write-Log "Duration: $($duration.ToString('hh\:mm\:ss'))" "INFO"
Write-Log "Status: $Task completed" "SUCCESS"
Write-Log ""

Generate-Report -Results $results

Write-Log ""
Write-Log "📁 Log file: $LogFile" "INFO"
Write-Log "📊 Report: $ReportFile" "INFO"

# Exit with appropriate code
if ($results.ContainsKey("Build") -and -not $results.Build) {
    exit 1
}

exit 0

