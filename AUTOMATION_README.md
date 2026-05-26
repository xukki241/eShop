# eShop Agent Automation Pipeline

> **Automate your .NET eShop builds with GitHub Copilot + Gemini AI**

## 📋 Overview

This setup provides **complete automation** for building, testing, and analyzing the eShop project using:

- 🤖 **GitHub Copilot** - Orchestration & code analysis
- 🧠 **Gemini AI** - Error analysis & smart recommendations  
- 🔧 **PowerShell Scripts** - Workflow automation
- 📊 **Automated Reports** - Build & test results

## 🚀 Quick Start (5 minutes)

### 1. Get Gemini API Key
Go to → [Google AI Studio](https://aistudio.google.com/apikey) → Get API Key

### 2. Run Setup
```powershell
.\setup-gemini-auto.ps1 -ApiKey "YOUR_API_KEY_HERE"
```

### 3. Start Building
```powershell
.\agent-pipeline.ps1 -Task all
```

**Done!** Check `reports/` for results. ✅

---

## 📁 Files & What They Do

### 🎯 Core Scripts

| File | Purpose | Usage |
|------|---------|-------|
| **`agent-pipeline.ps1`** | Main automation orchestrator | `.\agent-pipeline.ps1 -Task [build\|test\|analyze\|all]` |
| **`setup-gemini-auto.ps1`** | One-time Gemini CLI setup | `.\setup-gemini-auto.ps1 -InteractiveSetup` |

### 📚 Documentation

| File | Contains |
|------|----------|
| **`QUICKSTART.md`** | Try this first! 5-minute guide |
| **`GEMINI_CLI_SETUP.md`** | Detailed setup instructions |
| **`AGENT_TASKS.md`** | All available tasks & examples |
| **`README.md`** | Original eShop documentation |

### 📊 Generated Files

| Folder | Contains |
|--------|----------|
| **`logs/`** | Build & execution logs |
| **`reports/`** | Analysis & summary reports |
| **`artifacts/`** | Compiled binaries |
| **`.env`** | Configuration (don't commit!) |

---

## 🎯 Available Tasks

### Quick References

```powershell
# ⚡ Fast build (skip tests)
.\agent-pipeline.ps1 -Task quick-build

# 🔨 Full build
.\agent-pipeline.ps1 -Task build

# 🧪 Run tests
.\agent-pipeline.ps1 -Task test

# 🔍 Analyze project
.\agent-pipeline.ps1 -Task analyze

# 📊 Check system status
.\agent-pipeline.ps1 -Task status

# 🚀 Deploy check
.\agent-pipeline.ps1 -Task deploy

# ⭐ Everything (build+test+analyze)
.\agent-pipeline.ps1 -Task all
```

---

## 🏗️ Architecture

```
┌─────────────────────┐
│   You (Terminal)    │
└──────────┬──────────┘
           │
    ┌──────▼──────┐
    │ GitHub      │
    │ Copilot     │ ◄── I manage & orchestrate
    └──────┬──────┘
           │
    ┌──────▼──────────────────┐
    │  PowerShell Pipeline    │
    │  ├─ Clean               │
    │  ├─ Restore packages    │
    │  ├─ Build solution      │
    │  ├─ Run tests           │
    │  └─ Analyze results     │
    └──────┬──────────────────┘
           │
      ┌────┴────┐
      │          │
   ┌──▼──┐   ┌──▼──────┐
   │ Logs │   │ Gemini  │ ◄── AI analysis
   └─────┘   │ (Cloud) │
             └──┬───┬──┘
                │   │
          ┌─────▼─┴─▼────┐
          │  Analysis &   │
          │  Suggestions  │
          └──────────────┘
                │
           ┌────▼────┐
           │ Reports  │ ◄── You see results here
           │ & Logs   │
           └──────────┘
```

---

## 💻 Prerequisites

- ✅ .NET 10 SDK (for eShop)
- ✅ Node.js + npm (for Gemini CLI)
- ✅ PowerShell 5.1+ (Windows)
- ✅ Docker (optional, for deployment)
- ✅ Gemini API Key (free tier available)

**Check system:**
```powershell
.\agent-pipeline.ps1 -Task status
```

---

## 🔐 Security

### API Key Management

```powershell
# Set up once (permanent)
.\setup-gemini-auto.ps1 -InteractiveSetup

# Verify it's set
$env:GEMINI_API_KEY  # Should show your key

# Files with secrets
.env                 # ⚠️ Already in .gitignore
```

**Best Practices:**
- ✅ Use `-InteractiveSetup` (not command line history)
- ✅ Rotate keys monthly
- ✅ Never commit `.env` file
- ✅ Monitor quota in Google Cloud Console

---

## 📊 Outputs

### Logs
```
logs/agent-pipeline-20260526_143022.log

[2026-05-26 14:30:22] [INFO] Building solution...
[2026-05-26 14:31:15] [SUCCESS] Build completed
[2026-05-26 14:31:20] [INFO] Running tests...
```

### Reports
```markdown
# eShop Agent Automation Report

**Generated:** 2026-05-26 14:35:00

## Build Summary
- ✅ Build: SUCCESS
- ✅ Tests: PASSED (127/127)
- ⚠️ Warnings: 3

## Analysis
- Source Projects: 18
- Test Projects: 5
- Build Time: 2m 45s
```

---

## 🧪 Examples

### Example 1: Quick Build
```powershell
# Just build, skip tests (2 min)
.\agent-pipeline.ps1 -Task quick-build

# ✓ Output: artifacts/bin/*
# ✓ Report: reports/build-report-*.md
```

### Example 2: Full Pipeline
```powershell
# Complete: build+test+analyze (5-8 min)
.\agent-pipeline.ps1 -Task all

# ✓ Logs: logs/agent-pipeline-*.log
# ✓ Report: reports/build-report-*.md
# ✓ Everything analyzed by Gemini AI
```

### Example 3: Just Tests
```powershell
# Run test suite (2 min)
.\agent-pipeline.ps1 -Task test

# ✓ Unit tests from tests/
# ✓ Functional tests
# ✓ Detailed results
```

### Example 4: System Check
```powershell
# Verify everything is ready
.\agent-pipeline.ps1 -Task status

# Output:
# - .NET SDK: 10.0.100
# - Node.js: v18.16.0
# - Docker: Docker version 24.0.0
# - Status: ✓ Ready for build!
```

---

## 🔥 Common Workflows

### Continuous Development
```powershell
# In a terminal loop - builds every 30 min
while ($true) {
    .\agent-pipeline.ps1 -Task quick-build
    Start-Sleep -Minutes 30
}
```

### Before Commit
```powershell
# Make sure everything passes
.\agent-pipeline.ps1 -Task all

# Check reports/
Get-ChildItem reports/*.md | Select-Object -Last 1 | Get-Content
```

### Pre-Deployment
```powershell
# Full validation
.\agent-pipeline.ps1 -Task all

# Check Docker
.\agent-pipeline.ps1 -Task deploy

# If all green → Ready to deploy! 🚀
```

---

## 🐛 Troubleshooting

### Setup Issues

**"gemini: command not found"**
```powershell
npm install -g @google/generative-ai
```

**"GEMINI_API_KEY not set"**
```powershell
.\setup-gemini-auto.ps1 -InteractiveSetup
```

**"API authentication failed"**
```powershell
# Verify key is correct:
$env:GEMINI_API_KEY  # Should show your key

# Re-setup:
.\setup-gemini-auto.ps1 -ApiKey "YOUR_KEY"
```

### Build Issues

**"Build failed"**
```powershell
# Check detailed output:
.\agent-pipeline.ps1 -Task build -Verbose

# View full log:
Get-ChildItem logs/*.log | Select-Object -Last 1 | Get-Content
```

**".NET SDK not found"**
```powershell
# Install from: https://dot.net/download
# Then verify:
dotnet --version
```

**"Tests failing"**
```powershell
# Run with verbose output:
.\agent-pipeline.ps1 -Task test -Verbose

# Check logs for details:
.\agent-pipeline.ps1 -Task test
```

---

## 📈 Monitoring

### View Latest Report
```powershell
# Open latest report
Get-ChildItem reports/*.md | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content
```

### Track Build Times
```powershell
# See all reports
Get-ChildItem reports/
# Output: 
#   build-report-20260526_143022.md  (3 min build)
#   build-report-20260526_122011.md  (2:45 min build)
#   build-report-20260526_101533.md  (3:15 min build)
```

### Check Success Rate
```powershell
# Count successful builds
(Get-ChildItem logs/*.log | Select-String "SUCCESS" | Measure-Object).Count
```

---

## 🚀 Next Steps

1. ✅ Read: `QUICKSTART.md` (5 min)
2. ✅ Run: `.\setup-gemini-auto.ps1 -InteractiveSetup` (2 min)
3. ✅ Test: `.\agent-pipeline.ps1 -Task quick-build` (2 min)
4. ✅ Analyze: `.\agent-pipeline.ps1 -Task all` (8 min)
5. ✅ Review: Check `reports/` folder
6. ✅ Integrate: Add to CI/CD pipeline

---

## 📚 References

- **Gemini API**: https://ai.google.dev/
- **.NET Build**: https://learn.microsoft.com/dotnet/core/tools/
- **PowerShell**: https://learn.microsoft.com/powershell/
- **eShop**: https://github.com/dotnet/eShop

---

## ❓ FAQ

**Q: Do I need to pay for Gemini API?**  
A: No! Free tier includes 15 requests/minute. Perfect for CI/CD automation.

**Q: Can I use this without Copilot?**  
A: Yes! This is just automation. Copilot helps orchestrate & analyze.

**Q: How long does a full pipeline take?**  
A: ~5-8 minutes depending on solution size.

**Q: Can I schedule this to run daily?**  
A: Yes! See `AGENT_TASKS.md` for Windows Task Scheduler setup.

**Q: Is my API Key secure?**  
A: Yes! It's stored in Windows User environment variables, not in code.

---

## 📞 Support

- 📖 Check: `GEMINI_CLI_SETUP.md` for setup issues
- 📋 Check: `AGENT_TASKS.md` for task details
- 📊 Check: `logs/` and `reports/` for errors
- 🆘 Read: Troubleshooting section above

---

## 🎉 Ready to Automate!

Everything is set up and ready to go. Start with:

```powershell
.\agent-pipeline.ps1 -Task all
```

Watch as your build, tests, and analysis run automatically. Gemini AI will analyze the results and provide smart recommendations.

**Happy automating!** 🚀

---

*Last updated: May 26, 2026*  
*For .NET 10 eShop with GitHub Copilot & Gemini AI*

