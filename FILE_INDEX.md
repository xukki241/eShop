# 📑 eShop Agent Automation - File Index

## 🎯 Start Here

### ⭐ First Time? Start with ONE of these:

| Your Goal | Read This | Time |
|-----------|-----------|------|
| **Quick start** | `QUICKSTART.md` | 5 min |
| **Full overview** | `AUTOMATION_README.md` | 10 min |
| **Just setup** | `GEMINI_CLI_SETUP.md` | 5 min |
| **See all tasks** | `AGENT_TASKS.md` | 15 min |

---

## 📚 All Files Explained

### 🔴 Essential Setup Files
These you **MUST** read/run first:

```
1. QUICKSTART.md
   └─ Fastest path to get started
   └─ Follow this if it's your first time
   └─ ⏱️ 5 minutes

2. setup-gemini-auto.ps1
   └─ Automated setup script
   └─ Run: .\setup-gemini-auto.ps1 -InteractiveSetup
   └─ ⏱️ 2 minutes
```

### 🟡 Important Documentation
Read these to understand everything:

```
1. GEMINI_CLI_SETUP.md
   └─ Detailed setup instructions
   └─ Troubleshooting guide
   └─ Security best practices
   └─ For reference: 📖

2. AUTOMATION_README.md
   └─ Complete overview of system
   └─ Architecture & workflows
   └─ Real examples
   └─ FAQ & troubleshooting
   └─ For reference: 📖

3. AGENT_TASKS.md
   └─ All available automation tasks
   └─ Examples & integrations
   └─ Scheduling & CI/CD
   └─ For reference: 📖
```

### 🟢 Automation Scripts
These do the work:

```
1. agent-pipeline.ps1
   └─ Main automation orchestrator
   └─ Usage: .\agent-pipeline.ps1 -Task [build|test|analyze|all]
   └─ 🔧 The heart of automation

2. setup-gemini-auto.ps1
   └─ One-time Gemini CLI setup
   └─ Usage: .\setup-gemini-auto.ps1 -InteractiveSetup
   └─ 🔧 Setup tool
```

### 🔵 Generated/Configuration Files
Auto-created when you run scripts:

```
logs/
└─ agent-pipeline-YYYYMMDD_HHMMSS.log
└─ Build execution logs & details
└─ 📝 For debugging

reports/
└─ build-report-YYYYMMDD_HHMMSS.md
└─ Analysis & summaries
└─ 📊 For tracking progress

artifacts/bin/
└─ Compiled binaries
└─ Build output
└─ 📦 Your builds

.env
└─ Configuration file
└─ Contains GEMINI_API_KEY
└─ ⚠️ Don't commit!
```

---

## 🚀 Quickest Start Path

### If you have 5 minutes:
```powershell
# 1. Get API Key from: https://aistudio.google.com/apikey

# 2. Setup (choose one):
.\setup-gemini-auto.ps1 -InteractiveSetup
# OR
.\setup-gemini-auto.ps1 -ApiKey "YOUR_KEY_HERE"

# 3. Build & test
.\agent-pipeline.ps1 -Task all

# 4. Check results
Get-ChildItem reports/ | Sort-Object LastWriteTime -Descending | Select-Object -First 1
```

---

## 📖 Reading Guide by Role

### 👨‍💻 Developer (Just want to build)
```
1. QUICKSTART.md              → 5 min setup
2. agent-pipeline.ps1         → Run builds
3. Check logs/ & reports/     → See results
```

### 🏗️ DevOps/Automation Engineer
```
1. AUTOMATION_README.md       → Full overview
2. AGENT_TASKS.md            → All tasks available
3. Setup CI/CD integration    → GitHub Actions example in AGENT_TASKS.md
```

### 🔍 Troubleshooter
```
1. Troubleshooting → AUTOMATION_README.md or GEMINI_CLI_SETUP.md
2. Check logs/ → agent-pipeline-*.log files
3. Check reports/ → build-report-*.md files
```

### 📚 Learner/Student
```
1. QUICKSTART.md              → Get overview
2. AUTOMATION_README.md       → Understand architecture
3. AGENT_TASKS.md            → Learn all capabilities
4. Just experiment! Run tasks and see what happens
```

---

## 🔄 Common Workflows

### "I want to build quickly"
```
Read:  QUICKSTART.md (2 min)
Run:   .\agent-pipeline.ps1 -Task quick-build (2 min)
Check: reports/build-report-*.md
```

### "I want to set up CI/CD"
```
Read:  AGENT_TASKS.md → "CI/CD Integration" section
Read:  GitHub Actions example
Copy:  .github/workflows/eshop-build.yml
```

### "Setup is failing"
```
Read:  GEMINI_CLI_SETUP.md → Troubleshooting section
Run:   .\agent-pipeline.ps1 -Task status
Read:  logs/agent-pipeline-*.log for details
```

### "Tests are failing"
```
Run:   .\agent-pipeline.ps1 -Task test -Verbose
Read:  logs/ for details
Read:  reports/ for Gemini analysis
```

---

## ✅ Checklist

### Before you start
- [ ] Have Gemini API Key (get from: https://aistudio.google.com/apikey)
- [ ] Have .NET 10 SDK (check: `dotnet --version`)
- [ ] Have npm (check: `npm --version`)

### Setup phase
- [ ] Read QUICKSTART.md
- [ ] Run setup-gemini-auto.ps1
- [ ] Verify with: `$env:GEMINI_API_KEY`

### Build phase
- [ ] Run: `.\agent-pipeline.ps1 -Task all`
- [ ] Check: logs/ for execution details
- [ ] Review: reports/ for analysis

### Done!
- [ ] Build succeeded ✅
- [ ] Tests passed ✅
- [ ] Analysis complete ✅
- [ ] Ready to deploy! 🚀

---

## 🎯 File Decision Tree

```
START HERE
    │
    ├─→ "What should I read?" 
    │   └─→ Choose by role above
    │
    ├─→ "How do I setup?"
    │   └─→ QUICKSTART.md (5 min)
    │   └─→ Run: setup-gemini-auto.ps1
    │
    ├─→ "How do I build?"
    │   └─→ See "Common Workflows" above
    │   └─→ Run: agent-pipeline.ps1
    │
    ├─→ "Something's broken!"
    │   └─→ Check Troubleshooting section
    │   └─→ Check logs/ for details
    │
    └─→ "I want to learn everything"
        └─→ Read in order:
            1. QUICKSTART.md
            2. AUTOMATION_README.md
            3. AGENT_TASKS.md
            4. GEMINI_CLI_SETUP.md
```

---

## 📞 Quick Help

| Question | Answer |
|----------|--------|
| Where's setup? | `QUICKSTART.md` or `setup-gemini-auto.ps1` |
| How to build? | Run `.\agent-pipeline.ps1 -Task all` |
| Something wrong? | Check `logs/*.log` |
| Need help? | Read `AUTOMATION_README.md` Troubleshooting |
| All details? | Read `AUTOMATION_README.md` |
| All tasks? | Read `AGENT_TASKS.md` |
| Setup issues? | Read `GEMINI_CLI_SETUP.md` |

---

## 🎓 Learn More

### About eShop
- Original: https://github.com/dotnet/eShop
- .NET: https://learn.microsoft.com/dotnet/

### About Automation
- PowerShell: https://learn.microsoft.com/powershell/
- .NET CLI: https://learn.microsoft.com/dotnet/core/tools/

### About AI
- Gemini API: https://ai.google.dev/
- Google AI Studio: https://aistudio.google.com/

---

## 💾 File Organization

```
eShop/
│
├── 📄 QUICKSTART.md                      ⭐ START HERE
├── 📄 AUTOMATION_README.md               📖 Full guide
├── 📄 GEMINI_CLI_SETUP.md               🔧 Setup guide
├── 📄 AGENT_TASKS.md                    📚 All tasks
├── 📄 FILE_INDEX.md                     📑 This file
│
├── 🔧 agent-pipeline.ps1                 ⚙️ Main script
├── 🔧 setup-gemini-auto.ps1              ⚙️ Setup script
│
├── 📁 logs/                              🔍 Build logs
├── 📁 reports/                           📊 Analysis reports
├── 📁 artifacts/                         📦 Build output
│
├── .env                                  🔐 Config (don't commit!)
│
└── [Original eShop files...]
```

---

## 🔥 Pro Tips

1. **First time?** Start with `QUICKSTART.md` - don't miss it!
2. **Stuck?** Check `logs/` folder for error details
3. **Understanding?** Read `AUTOMATION_README.md` for architecture
4. **All tasks?** See `AGENT_TASKS.md` for everything available
5. **Setup issue?** Use `GEMINI_CLI_SETUP.md` troubleshooting
6. **Scheduled builds?** See CI/CD section in `AGENT_TASKS.md`

---

## ⏱️ Time Estimates

| Task | Time |
|------|------|
| Read QUICKSTART.md | 5 min |
| Setup Gemini CLI | 2 min |
| First build | 3-5 min |
| Full pipeline | 8-10 min |
| Read everything | 30 min |

**Total to get running: ~15 minutes** ⚡

---

## 🚀 Ready?

1. Get API Key: https://aistudio.google.com/apikey
2. Read: `QUICKSTART.md`
3. Setup: `.\setup-gemini-auto.ps1 -InteractiveSetup`
4. Build: `.\agent-pipeline.ps1 -Task all`

**Go!** 🎯

---

*Last updated: May 26, 2026*  
*eShop Agent Automation with Gemini AI*

