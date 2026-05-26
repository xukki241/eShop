# ⚡ Quick Start: eShop Agent Automation

> **Dùng Gemini API Key để tự động hóa build, test, và phân tích dự án eShop**

---

## 🎯 What You'll Get

✅ Automated builds & tests  
✅ AI-powered error analysis  
✅ Continuous monitoring  
✅ Detailed reports  
✅ GitHub Copilot integration  

---

## 🚀 Step 1: Get Gemini API Key (2 minutes)

### Option A: Using Google AI Studio (Easiest)
```
1. Go to: https://aistudio.google.com/apikey
2. Click "Get API Key"
3. Select/create Google Cloud project
4. Copy the key → You're done! ✓
```

### Option B: Using Google Cloud Console
```
1. Go to: https://console.cloud.google.com
2. Create new project
3. Enable "Generative Language API"
4. Credentials → Create API Key
5. Copy and save
```

---

## ⚙️ Step 2: Setup Gemini CLI (2 minutes)

**One-line setup:**
```powershell
.\setup-gemini-auto.ps1 -ApiKey "YOUR_API_KEY_HERE"
```

**Or interactive:**
```powershell
.\setup-gemini-auto.ps1 -InteractiveSetup
```

**What it does:**
- ✓ Installs Gemini CLI
- ✓ Sets environment variables
- ✓ Creates .env file
- ✓ Tests authentication
- ✓ Verifies everything works

---

## 🏗️ Step 3: Build & Test

**Quick build (no tests):**
```powershell
.\agent-pipeline.ps1 -Task quick-build
```

**Full pipeline (build + test + analyze):**
```powershell
.\agent-pipeline.ps1 -Task all
```

**Run specific task:**
```powershell
# Build only
.\agent-pipeline.ps1 -Task build

# Test only
.\agent-pipeline.ps1 -Task test

# Analyze only
.\agent-pipeline.ps1 -Task analyze

# Check system status
.\agent-pipeline.ps1 -Task status
```

---

## 📊 What Happens Automatically

### Build Process
```
1. Clean artifacts
   ↓
2. Restore NuGet packages
   ↓
3. Build solution (.NET 10)
   ↓
4. Generate build report
   ↓
5. Gemini analyzes results
   ↓
6. Save logs & reports
```

### Test Process
```
1. Run unit tests
   ↓
2. Run functional tests
   ↓
3. Generate test report
   ↓
4. Gemini identifies failures
   ↓
5. Suggests fixes
```

### Analysis
```
1. Scan project structure
   ↓
2. Count projects & files
   ↓
3. Detect warnings
   ↓
4. Analyze dependencies
   ↓
5. Generate analysis report
```

---

## 📁 Output Files

After running pipeline:

```
eShop/
├── logs/
│   └── agent-pipeline-YYYYMMDD_HHMMSS.log    # Build logs
├── reports/
│   └── build-report-YYYYMMDD_HHMMSS.md       # Analysis report
├── artifacts/
│   └── bin/                                    # Build output
└── .env                                        # Configuration (⚠️ Don't commit!)
```

**View latest report:**
```powershell
# Open latest report
Get-ChildItem reports/*.md | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Invoke-Item
```

---

## 🤖 How GitHub Copilot + Gemini Works

### I (GitHub Copilot) do:
✓ Orchestrate the pipeline  
✓ Run commands  
✓ Parse output  
✓ Identify issues  
✓ Generate reports  

### Gemini CLI does:
✓ Analyze build output  
✓ Suggest fixes  
✓ Write recommendations  
✓ Generate summaries  

### Together:
```
You (Terminal Command)
        ↓
GitHub Copilot (Orchestration)
        ↓
PowerShell Scripts
        ↓
├── Build .NET project
├── Run tests
└── Parse logs
        ↓
Gemini AI (Analysis)
        ↓
├── Analyze errors
├── Suggest fixes
└── Generate report
        ↓
Reports + Logs (Output)
```

---

## 💡 Common Commands

```powershell
# Setup
.\setup-gemini-auto.ps1 -InteractiveSetup

# Quick build
.\agent-pipeline.ps1 -Task quick-build

# Full pipeline
.\agent-pipeline.ps1 -Task all

# Just build
.\agent-pipeline.ps1 -Task build

# Just test
.\agent-pipeline.ps1 -Task test

# Just analyze
.\agent-pipeline.ps1 -Task analyze

# Check status
.\agent-pipeline.ps1 -Task status

# View latest log
Get-ChildItem logs/*.log | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content | less

# View latest report
Get-ChildItem reports/*.md | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content | less
```

---

## 🛠️ Troubleshooting

| Issue | Fix |
|-------|-----|
| `gemini not found` | Run: `npm install -g @google/generative-ai` |
| `API Key error` | Re-run: `.\setup-gemini-auto.ps1 -InteractiveSetup` |
| `Build fails` | Check: `.\agent-pipeline.ps1 -Task status` |
| `Tests fail` | Run: `.\agent-pipeline.ps1 -Task test -Verbose` |
| `Docker needed` | Install [Docker Desktop](https://www.docker.com/products/docker-desktop) |

---

## 📚 Documentation

- **Setup Details**: `GEMINI_CLI_SETUP.md`
- **All Tasks**: `AGENT_TASKS.md`
- **Pipeline Script**: `agent-pipeline.ps1`

---

## ✨ Next Steps

1. ✅ Run setup: `.\setup-gemini-auto.ps1 -InteractiveSetup`
2. ✅ Test build: `.\agent-pipeline.ps1 -Task quick-build`
3. ✅ Full pipeline: `.\agent-pipeline.ps1 -Task all`
4. ✅ Check reports in `reports/` folder
5. ✅ Integrate into CI/CD (GitHub Actions, etc.)

---

## 🎓 Learning Resources

- [Gemini API Documentation](https://ai.google.dev/tutorials/python_quickstart)
- [.NET Build Documentation](https://learn.microsoft.com/dotnet/core/tools/)
- [PowerShell Scripting](https://learn.microsoft.com/powershell/)
- [eShop Project](https://github.com/dotnet/eShop)

---

## 🔒 Security Notes

⚠️ **Important:**
- Never commit `.env` file (already in `.gitignore`)
- API Keys are embedded → Don't share logs publicly
- Rotate keys monthly
- Monitor API usage in [Google Cloud Console](https://console.cloud.google.com)

---

## 🎉 You're Ready!

Everything is set up. Just run:

```powershell
.\agent-pipeline.ps1 -Task all
```

And let the automation work for you! 🚀

---

**Questions?** Check the documentation files or review the logs in `logs/` folder.

Good luck! 💪

