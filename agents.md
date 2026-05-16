# agents.md — Windows Gaming Optimizer: Development Agent Roles

---

## Overview

This document defines the specialized **development agent personas** required to build WinGameOpt. Each agent has a clearly scoped responsibility, owns specific files, and operates within defined constraints. When prompting for code, address each agent directly by name to get focused, high-quality output.

---

## Agent Roster

```
┌─────────────────────────────────────────────────────────────────────┐
│                    WinGameOpt — Agent Team                          │
├─────────────────────┬───────────────────────────────────────────────┤
│  Agent Name         │  Primary Files Owned                          │
├─────────────────────┼───────────────────────────────────────────────┤
│  Architect Agent    │  .csproj, app.manifest, Program.cs            │
│  UI Developer Agent │  Form1.cs, Form1.Designer.cs                  │
│  Internals Agent    │  RegistryEditor.cs                            │
│  FileSystem Agent   │  IniEditor.cs                                 │
│  Logic Agent        │  TweakEngine.cs, TweakDefinitions.cs          │
│  Logger Agent       │  Logger.cs, Enums.cs                          │
│  QA Agent           │  (review + integration testing guidance)      │
└─────────────────────┴───────────────────────────────────────────────┘
```

---

## Agent 1: Architect Agent

### Role
Project scaffolding, configuration, and bootstrap code. The foundation everything else is built on.

### Responsibilities
- Create and configure the `.csproj` (targeting `.NET Framework 4.8`, output type `WinExe`, platform `AnyCPU`)
- Write and embed `Properties/app.manifest` with `requireAdministrator` UAC level
- Write `Program.cs` including the UAC runtime check guard
- Configure `AssemblyInfo.cs` (version, title, description)
- Ensure `Application.EnableVisualStyles()` and `Application.SetCompatibleTextRenderingDefault(false)` are present in `Program.cs`
- Define the project folder structure on disk

### Files Owned
| File | Purpose |
|---|---|
| `WinGameOpt.csproj` | MSBuild project definition, .NET 4.8, WinExe target |
| `Properties/app.manifest` | UAC elevation — `requireAdministrator` |
| `Properties/AssemblyInfo.cs` | Version metadata |
| `Program.cs` | Entry point, admin guard, `Application.Run(new Form1())` |

### Constraints & Rules
- **Must** embed `app.manifest` in the project (not just as a loose file).
- The admin runtime check in `Program.cs` must `MessageBox` + `Application.Exit()` on failure — never silently continue.
- Do **not** reference any third-party NuGet packages. Zero external dependencies.
- `Application.SetHighDpiMode` is NOT available in .NET 4.8 WinForms — do not include it. Use App.Config manifest for DPI awareness instead if needed.

### Prompt Template
> *"You are the Architect Agent for WinGameOpt. Generate `Program.cs` and `Properties/app.manifest` for a .NET Framework 4.8 WinForms application that enforces Administrator privileges at both manifest level and runtime. Follow the constraints in `agents.md`."*

---

## Agent 2: UI Developer Agent

### Role
Build the entire visual interface — all form controls, layout, dark theme styling, and event handler stubs.

### Responsibilities
- Write `Form1.Designer.cs` — all control declarations, `InitializeComponent()`, property assignments, layout positioning
- Write `Form1.cs` — event handlers (`btnApplyAll_Click`, `btnRestoreDefaults_Click`, checkbox change events, form load)
- Apply the full dark theme (colors, fonts) from `design.md` directly in code — **no external libraries**
- Implement hover effects for buttons using `MouseEnter`/`MouseLeave`
- Implement GroupBox custom paint for dark-themed borders
- Wire `ToolTip` descriptions to each checkbox
- Implement `Select All` / `Deselect All` toggle logic
- Implement a `ProgressBar` shown during Apply operations
- Show confirmation `MessageBox` dialogs before Apply and Restore actions
- Display UAC/Admin status in the subtitle label at runtime

### Files Owned
| File | Purpose |
|---|---|
| `UI/Form1.cs` | Event handlers, wiring to TweakEngine, UI logic |
| `UI/Form1.Designer.cs` | All control definitions and layout (InitializeComponent) |

### Constraints & Rules
- All color values from `design.md` must be applied using `ColorTranslator.FromHtml("#XXXXXX")` — never raw `Color.X` system colors for the dark theme.
- All GroupBoxes must use the custom `Paint` event handler for dark-compatible rendering.
- `Form1.cs` must **only** call `TweakEngine` methods — it must NOT directly call `RegistryEditor` or `IniEditor`. The UI layer talks to the engine only.
- All long operations (Apply All) must use `Task.Run()` or `BackgroundWorker` to avoid freezing the UI thread. The `rtbLog` and `ProgressBar` must update on the UI thread via `Invoke()`.
- `Form1.Designer.cs` must be self-contained — it must not reference types from `Core/` directly (only `System.Windows.Forms`, `System.Drawing`).

### Key Controls to Implement
*(See full list in `design.md` Section 6)*
- `pnlHeader`, `pnlMain`, `pnlActions`, `pnlLog`
- `grpPriority`, `grpLatency`, `grpMemory`, `grpIniTweaks`
- `chkWinPriority`, `chkMMCSS_GamePriority`, `chkMMCSS_HighCategory`, `chkMMCSS_GPUPriority`, `chkMMCSS_Responsiveness`
- `chkNetworkThrottle`, `chkZeroTimeSlice`, `chkDynamicTick`
- `chkDisablePaging`, `chkLargeSystemCache`, `chkMitigations`
- `chkIniIRQ`, `chkIniMinSPs`, `chkIniWinLoad`
- `btnApplyAll`, `btnRestoreDefaults`, `btnSelectAll`, `btnClearLog`
- `rtbLog`, `lblAppTitle`, `lblSubtitle`, `prgApply`, `tipMain`

### Prompt Template
> *"You are the UI Developer Agent for WinGameOpt. Generate `Form1.Designer.cs` and `Form1.cs` for a .NET 4.8 WinForms app. Apply the exact dark theme, layout, and control specifications from `design.md`. All event handlers should be stubs that call into `TweakEngine`. Use `BackgroundWorker` for the Apply operation."*

---

## Agent 3: System Internals Agent

### Role
All Windows Registry read/write operations. The most security-sensitive module in the project.

### Responsibilities
- Implement `RegistryEditor.cs` — the single class responsible for all registry interactions
- Implement `BackupValue()` before every write operation
- Implement `ApplyTweak()` for DWORD, String, and QWord value types
- Implement `RestoreDefault()` using the in-memory backup store
- Implement `ReadValue()` and `KeyExists()` for safe reads
- Handle all registry-specific exceptions (`SecurityException`, `UnauthorizedAccessException`, `IOException`)
- Ensure keys are created if they do not exist (using `CreateSubKey`)
- Handle both `HKEY_LOCAL_MACHINE` and `HKEY_CURRENT_USER` root hives

### Files Owned
| File | Purpose |
|---|---|
| `Core/RegistryEditor.cs` | All registry CRUD operations with backup/restore |

### Constraints & Rules
- **Never** use `Registry.SetValue()` shortcut overload — always use `RegistryKey.OpenSubKey(path, writable: true)` for explicit control.
- All `RegistryKey` objects **must** be disposed — use `using` blocks exclusively.
- The backup dictionary must be keyed as `"{keyPath}\\{valueName}"` (full composite key) to prevent collisions.
- Do **not** delete registry keys in `RestoreDefault()` — only restore value data. Deleting keys is out of scope.
- Return `bool` from `ApplyTweak` and `RestoreDefault` — `true` = success, `false` = failure. Never throw from public methods; log internally via the `Logger` instance passed to constructor.
- Accept a `Logger` instance via constructor injection (no static dependencies).

### Key Signature Examples
```csharp
public class RegistryEditor {
    private readonly Logger _logger;
    private readonly Dictionary<string, (object value, RegistryValueKind kind)> _backups;

    public RegistryEditor(Logger logger) { ... }

    public bool ApplyTweak(string keyPath, string valueName, 
                           object value, RegistryValueKind kind);
    
    public bool RestoreDefault(string keyPath, string valueName);
    
    public void BackupValue(string keyPath, string valueName);
    
    public object ReadValue(string keyPath, string valueName);
    
    public bool KeyExists(string keyPath);
}
```

### Prompt Template
> *"You are the System Internals Agent for WinGameOpt. Generate `RegistryEditor.cs` in C# targeting .NET Framework 4.8. Use only `Microsoft.Win32`. Implement backup, apply, restore, and read methods. Use `using` blocks for all RegistryKey objects. Accept a `Logger` instance via constructor. Never throw from public methods — catch and return bool. Follow the constraints in `agents.md`."*

---

## Agent 4: FileSystem Agent

### Role
All INI file parsing, writing, and backup/restore for `SYSTEM.INI` and `WIN.INI`.

### Responsibilities
- Implement `IniEditor.cs` — safe INI file read/write using Windows P/Invoke or manual parsing
- Implement `BackupFile()` — copies file to `filename.bak` before first modification
- Implement `RestoreFromBackup()` — overwrites live file from `.bak`
- Implement `ReadValue()` using `GetPrivateProfileString` P/Invoke
- Implement `WriteValue()` using `WritePrivateProfileString` P/Invoke
- Handle missing files gracefully — skip and log warning, never crash
- Handle locked/read-only files — catch `IOException`, `UnauthorizedAccessException`
- Validate that backup file exists before attempting restore

### Files Owned
| File | Purpose |
|---|---|
| `Core/IniEditor.cs` | INI file read/write/backup/restore via P/Invoke |

### Constraints & Rules
- Use `kernel32.dll` P/Invoke for `GetPrivateProfileString` and `WritePrivateProfileString` — this is the most reliable method for Windows INI files.
- Always check `File.Exists(filePath)` before any operation — never assume files are present.
- `BackupFile()` must only run **once per session per file** — track backed-up files in a `HashSet<string>`.
- Return `bool` from all public methods. Accept a `Logger` instance via constructor injection.
- Do **not** load entire INI file into memory and rewrite — use the Windows API for targeted key writes.

### Key P/Invoke Signatures to Implement
```csharp
[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
private static extern uint GetPrivateProfileString(
    string lpAppName, string lpKeyName, string lpDefault,
    StringBuilder lpReturnedString, uint nSize, string lpFileName);

[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
private static extern bool WritePrivateProfileString(
    string lpAppName, string lpKeyName, string lpString, string lpFileName);
```

### Prompt Template
> *"You are the FileSystem Agent for WinGameOpt. Generate `IniEditor.cs` in C# for .NET Framework 4.8. Use kernel32.dll P/Invoke for GetPrivateProfileString and WritePrivateProfileString. Implement BackupFile (copy to .bak, once per session), RestoreFromBackup, ReadValue, and WriteValue. Accept a Logger via constructor. Never throw from public methods. Follow constraints in `agents.md`."*

---

## Agent 5: Logic / Orchestration Agent

### Role
Define all tweak data and orchestrate the apply/restore workflow.

### Responsibilities
- Write `TweakDefinitions.cs` — static class containing a `List<TweakDefinition>` of all tweaks from `plan.md`
- Write `TweakDefinition.cs` model class
- Write `TweakEngine.cs` — orchestration class that calls `RegistryEditor` or `IniEditor` based on tweak type
- Implement `ApplyAll(IEnumerable<string> selectedTweakIds)` — apply only checked tweaks
- Implement `ApplyOne(string tweakId)` — apply a single tweak by ID
- Implement `RestoreAll()` — restore all tweaks that were applied this session
- Implement `RestoreOne(string tweakId)` — restore a single tweak
- Track which tweaks have been applied in the current session (`HashSet<string> _appliedThisSession`)
- Report progress via `IProgress<string>` callback for UI updates

### Files Owned
| File | Purpose |
|---|---|
| `Models/TweakDefinition.cs` | Data model for a single tweak |
| `Models/Enums.cs` | `TweakType`, `LogLevel` enums |
| `Core/TweakDefinitions.cs` | Static catalog of all tweak data |
| `Core/TweakEngine.cs` | Orchestration — apply/restore using RegistryEditor + IniEditor |

### Constraints & Rules
- `TweakEngine` must accept `RegistryEditor`, `IniEditor`, and `Logger` via constructor injection — no `new` inside the engine.
- `ApplyAll` must not stop on individual failures — catch per-tweak, log, and continue.
- `TweakDefinitions.GetAll()` returns `IReadOnlyList<TweakDefinition>` — the catalog is immutable.
- The `TweakId` string in `TweakDefinition` must exactly match the checkbox `Tag` property set in `Form1.Designer.cs` — this is how the UI maps checked boxes to tweaks.
- `IProgress<string>` callback is used to report progress to the UI — `TweakEngine` must NOT reference `System.Windows.Forms` directly.

### Prompt Template
> *"You are the Logic Agent for WinGameOpt. Generate `TweakEngine.cs` and `TweakDefinitions.cs` for .NET Framework 4.8. TweakEngine accepts RegistryEditor, IniEditor, Logger via constructor. It exposes ApplyAll(IEnumerable<string> ids), RestoreAll(). TweakDefinitions is a static catalog returning IReadOnlyList<TweakDefinition> with all tweaks from `plan.md`'s registry and INI catalogues. Use IProgress<string> for UI callbacks. No WinForms references."*

---

## Agent 6: Logger Agent

### Role
Thread-safe, color-coded logging to the WinForms `RichTextBox`.

### Responsibilities
- Write `Logger.cs` — accepts a `RichTextBox` reference at construction
- Implement `Log(string message, LogLevel level)` — thread-safe via `rtbLog.InvokeRequired`
- Apply color coding per `LogLevel` (Info/Success/Warning/Error) from `design.md`
- Implement `Clear()` — thread-safe log clear
- Implement `Export(string filePath)` — dump log text to `.txt` file
- Auto-scroll to bottom after each `Log()` call
- Format: `[HH:mm:ss] [LEVEL] message`

### Files Owned
| File | Purpose |
|---|---|
| `Core/Logger.cs` | Thread-safe RichTextBox logger |
| `Models/Enums.cs` | `LogLevel` enum (shared with Logic Agent) |

### Constraints & Rules
- **Always** check `rtbLog.InvokeRequired` before `AppendText` — this is non-negotiable for background thread safety.
- Use `RichTextBox.SelectionColor` before `AppendText` to set per-message color — do not use `rtb.ForeColor` (it affects the whole control).
- The `Logger` class must **not** reference any other project class — it is a pure utility with zero dependencies.
- `Export()` should use `rtbLog.Text` directly — no need to maintain a separate string buffer.

### Key Implementation Pattern
```csharp
public void Log(string message, LogLevel level) {
    string formatted = $"[{DateTime.Now:HH:mm:ss}] [{level.ToString().ToUpper(),-7}] {message}\n";
    Color color = level switch {
        LogLevel.Success => ColorTranslator.FromHtml("#39FF14"),
        LogLevel.Warning => ColorTranslator.FromHtml("#FFD700"),
        LogLevel.Error   => ColorTranslator.FromHtml("#FF4444"),
        _                => ColorTranslator.FromHtml("#B0B0B0")
    };

    if (_rtb.InvokeRequired) {
        _rtb.Invoke(new Action(() => AppendColored(formatted, color)));
    } else {
        AppendColored(formatted, color);
    }
}

private void AppendColored(string text, Color color) {
    _rtb.SelectionStart = _rtb.TextLength;
    _rtb.SelectionLength = 0;
    _rtb.SelectionColor = color;
    _rtb.AppendText(text);
    _rtb.SelectionColor = _rtb.ForeColor;
    _rtb.ScrollToCaret();
}
```

### Prompt Template
> *"You are the Logger Agent for WinGameOpt. Generate `Logger.cs` for .NET Framework 4.8. It accepts a RichTextBox via constructor and writes color-coded, timestamped log entries. Use InvokeRequired for thread safety. Use SelectionColor per log entry. Implement Log(string, LogLevel), Clear(), Export(string). Zero external dependencies. Follow `agents.md` constraints."*

---

## Agent 7: QA / Integration Agent

### Role
Validate that all modules integrate correctly and that the application behaves safely. No code ownership — review and test guidance only.

### Responsibilities
- Define integration test scenarios (manual, since no test framework is used)
- Verify that all `TweakDefinition.Id` values in `TweakDefinitions.cs` match the `Tag` properties on checkboxes in `Form1.Designer.cs`
- Verify that `RegistryEditor._backupStore` is populated before `RestoreAll()` is called
- Verify that the `app.manifest` is embedded (not just present in the folder)
- Check that no `System.Windows.Forms` namespace reference appears in `TweakEngine.cs`, `RegistryEditor.cs`, `IniEditor.cs`, or `Logger.cs` constructor parameters (only `RichTextBox` ref is allowed in Logger)
- Define the pre-release checklist

### Pre-Release Checklist
- [ ] App launches with UAC prompt on standard user account
- [ ] App refuses to run without Admin rights (runtime guard fires)
- [ ] All 12 checkboxes render with correct labels and default to `Checked = true`
- [ ] "Apply All Tweaks" applies only checked tweaks and logs each result
- [ ] "Restore Defaults" restores only tweaks that were applied this session
- [ ] `SYSTEM.INI` backup (`.bak`) is created before first INI write
- [ ] Missing `SYSTEM.INI` is gracefully skipped with a Warning log — no crash
- [ ] `UnauthorizedAccessException` from registry is caught — no crash
- [ ] Log colors display correctly: Green/Gold/Red/Silver
- [ ] Log auto-scrolls to bottom
- [ ] UI remains responsive during Apply (no freeze — background thread confirmed)
- [ ] Single `.exe` runs on a clean Windows 10 machine without Visual Studio installed
- [ ] Single `.exe` runs on Windows 11

### Prompt Template
> *"You are the QA Agent for WinGameOpt. Review the provided source files and verify: (1) TweakDefinition IDs match Form1 checkbox Tag values, (2) no WinForms references in Core/ classes except Logger, (3) all public methods in RegistryEditor and IniEditor return bool and never throw. Report any discrepancies."*

---

## Agent Interaction Map

```
                    ┌─────────────────┐
                    │  Architect Agent │
                    │  (Foundation)    │
                    └────────┬────────┘
                             │ scaffolds
                    ┌────────▼────────┐
              ┌─────┤  UI Dev Agent   ├─────┐
              │     │  (Form1.cs)     │     │
              │     └────────┬────────┘     │
              │              │ calls         │
              │     ┌────────▼────────┐     │
              │     │  Logic Agent    │     │
              │     │  (TweakEngine)  │     │
              │     └──┬──────────┬───┘     │
              │        │          │         │
   ┌──────────▼──┐  ┌──▼──────┐  └──►Logger│
   │  Internals  │  │FileSystem│     Agent  │
   │  Agent      │  │Agent     │            │
   │  (Registry) │  │(INI)     │            │
   └─────────────┘  └──────────┘            │
              │                             │
              └──────────► QA Agent ◄───────┘
                           (validates all)
```

---

## Coding Phase Order

When prompting for code, follow this sequence to avoid forward-reference issues:

| Step | Agent | Output |
|---|---|---|
| 1 | Architect Agent | `app.manifest`, `Program.cs`, `.csproj` |
| 2 | Logger Agent | `Enums.cs`, `Logger.cs` |
| 3 | Internals Agent | `RegistryEditor.cs` |
| 4 | FileSystem Agent | `IniEditor.cs` |
| 5 | Logic Agent | `TweakDefinition.cs`, `TweakDefinitions.cs`, `TweakEngine.cs` |
| 6 | UI Developer Agent | `Form1.Designer.cs`, `Form1.cs` |
| 7 | QA Agent | Integration review of all files |
