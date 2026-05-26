# Gemini CLI Setup Guide - For Agent Automation

## Step 1: Get Gemini API Key

### Option A: Google AI Studio (Recommended for quick setup)
1. Go to [Google AI Studio](https://aistudio.google.com/apikey)
2. Click "Get API Key"
3. Select or create a Google Cloud project
4. Copy your API Key

### Option B: Google Cloud Console
1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Create new project or select existing
3. Enable "Generative Language API"
4. Go to "Credentials" → Create "API Key"
5. Copy your API Key

---

## Step 2: Set Up Environment Variables (PowerShell)

### Windows PowerShell - Permanent Setup

```powershell
# 1. Open PowerShell as Administrator
# 2. Run this command:
[System.Environment]::SetEnvironmentVariable("GEMINI_API_KEY", "YOUR_API_KEY_HERE", "User")

# 3. Verify it's set:
$env:GEMINI_API_KEY
# Output: YOUR_API_KEY_HERE

# 4. Close and reopen PowerShell for changes to take effect
```

### Temporary (Current Session Only)
```powershell
$env:GEMINI_API_KEY = "YOUR_API_KEY_HERE"
```

---

## Step 3: Verify Gemini CLI Installation & Authentication

```powershell
# Check if gemini CLI is installed
gemini --version

# Authenticate with API Key
gemini auth --api-key $env:GEMINI_API_KEY

# Or directly:
gemini auth --api-key "YOUR_API_KEY_HERE"

# Test connection
gemini test-connection
# Output should show: ✓ Authenticated successfully
```

---

## Step 4: Create Environment Setup Script

Create `setup-gemini.ps1`:

```powershell
# setup-gemini.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey
)

# Set environment variable
[System.Environment]::SetEnvironmentVariable("GEMINI_API_KEY", $ApiKey, "User")

# Verify
$env:GEMINI_API_KEY = $ApiKey
Write-Host "✓ Gemini API Key set successfully" -ForegroundColor Green
Write-Host "  Variable: GEMINI_API_KEY" -ForegroundColor Cyan

# Test authentication
gemini auth --api-key $ApiKey
gemini test-connection

Write-Host "✓ Authentication verified!" -ForegroundColor Green
```

**Usage:**
```powershell
.\setup-gemini.ps1 -ApiKey "YOUR_API_KEY_HERE"
```

---

## Step 5: Create .env File (For Local Development)

Create `.env` file in project root:

```env
GEMINI_API_KEY=YOUR_API_KEY_HERE
GEMINI_MODEL=gemini-pro  # or gemini-2.0-flash for faster responses

# Optional
GEMINI_TEMPERATURE=0.7
GEMINI_MAX_TOKENS=2000
```

Load in PowerShell scripts:
```powershell
# Load environment variables from .env
if (Test-Path ".env") {
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^\w+\=") {
            $parts = $_ -split "="
            [System.Environment]::SetEnvironmentVariable($parts[0], $parts[1], "Process")
        }
    }
}

$apiKey = $env:GEMINI_API_KEY
```

---

## Step 6: Integrate with Agent Pipeline Script

See `agent-pipeline.ps1` for integration examples.

### Example: Call Gemini from PowerShell Script

```powershell
function Invoke-GeminiTask {
    param(
        [string]$Prompt,
        [string]$Model = "gemini-2.0-flash"  # Faster
    )
    
    $apiKey = $env:GEMINI_API_KEY
    
    if (-not $apiKey) {
        Write-Error "GEMINI_API_KEY not set!"
        return $null
    }
    
    # Example using gemini CLI
    gemini analyze --model $Model --prompt $Prompt
}

# Usage in pipeline
$analysis = Invoke-GeminiTask -Prompt "Analyze build errors and suggest fixes"
```

---

## Security Best Practices

⚠️ **Important:**

1. **Never commit API Key to Git**
   ```powershell
   # Add to .gitignore
   .env
   *.apikey
   secrets.ps1
   ```

2. **Use GitHub Secrets (if on GitHub Actions)**
   ```yaml
   - name: Setup Gemini
     env:
       GEMINI_API_KEY: ${{ secrets.GEMINI_API_KEY }}
     run: gemini auth --api-key $env:GEMINI_API_KEY
   ```

3. **Rotate API Keys regularly** (monthly recommended)

4. **Monitor API Key usage** in Google Cloud Console

---

## Verify Setup Works

```powershell
# Test 1: Check env variable
Write-Host "GEMINI_API_KEY: $($env:GEMINI_API_KEY.Substring(0, 10))...***"

# Test 2: Authenticate
gemini auth --api-key $env:GEMINI_API_KEY

# Test 3: Simple query
gemini prompt "Say hello and confirm you're working"

# Test 4: Check model availability
gemini models list

# If all pass: ✓ You're ready for automation!
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "gemini: command not found" | Install Gemini CLI: `npm install -g @google/generative-ai` |
| "Authentication failed" | Check API Key is correct: `$env:GEMINI_API_KEY` |
| "API quota exceeded" | Wait for quota reset or upgrade API plan |
| "Model not found" | List available: `gemini models list` |

---

## Next Steps

Once Gemini CLI is set up:

1. ✅ Verify API working
2. ✅ Integrate into `agent-pipeline.ps1`
3. ✅ Create Gemini task handlers
4. ✅ Set up automated reporting

See: `AGENT_TASKS.md` for detailed task definitions

