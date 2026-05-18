# plan.md — Windows Gaming Optimizer: Project Roadmap & Logic

---

## 1. Project Overview

**Application Name:** JaiDee-Optimize
**Goal:** A standalone, lightweight Windows Forms application that applies targeted Windows Registry and INI file tweaks to reduce DPC Latency and Input Lag for gaming.  
**Inspired by:** NEXR8 / NZTS style optimizers  
**Distribution:** Single `.exe` (no installer required)

---

## 2. Tech Stack

| Layer | Technology | Rationale |
|---|---|---|
| Language | C# 9.0 (via .NET Framework 4.8) | Native Windows API access, no runtime install needed on Win10/11 |
| UI Framework | Windows Forms (.NET Framework 4.8) | Zero dependency, ships with OS, extremely lightweight |
| Registry Access | `Microsoft.Win32.Registry` | First-party, no NuGet required |
| File I/O | `System.IO` (StreamReader/Writer) | Native, fast, zero overhead |
| Logging | Custom `Logger` class → `RichTextBox` | Real-time in-app feedback |
| Packaging | Single `.exe` via ILMerge or `/p:PublishSingleFile` | Standalone, no installer |
| Permissions | `app.manifest` (UAC `requireAdministrator`) | Enforced at launch by Windows |
| Build System | MSBuild / Visual Studio 2022 | Standard .NET 4.8 project |

---

## 3. Permissions Handling (UAC / Admin Rights)

### 3.1 Manifest-Based Elevation (Primary Method)
The `app.manifest` file forces Windows to prompt for elevation **before** the application window opens. This is the cleanest, most user-friendly approach.

```xml
<!-- app.manifest -->
<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
```

This manifest must be embedded in the project (`Properties/app.manifest`) and referenced in `AssemblyInfo.cs` or the `.csproj` directly.

### 3.2 Runtime Guard Check (Secondary / Defensive)
Despite the manifest, always perform a runtime check at `Form1_Load` as a defensive layer:

```csharp
// In Program.cs or Form1_Load
using System.Security.Principal;

bool IsAdmin() {
    var identity = WindowsIdentity.GetCurrent();
    var principal = new WindowsPrincipal(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}

if (!IsAdmin()) {
    MessageBox.Show("This application must be run as Administrator.", 
        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
    Application.Exit();
}
```

---

## 4. Core Logic Modules

### 4.1 `RegistryEditor.cs`
**Responsibility:** All Registry read/write/restore operations.

**Key Methods:**

| Method | Signature | Description |
|---|---|---|
| `ApplyTweak` | `bool ApplyTweak(string keyPath, string valueName, object value, RegistryValueKind kind)` | Writes a single registry value |
| `RestoreDefault` | `bool RestoreDefault(string keyPath, string valueName, object originalValue, RegistryValueKind kind)` | Restores a single value to default |
| `BackupValue` | `void BackupValue(string keyPath, string valueName)` | Reads and stores current value before applying tweaks |
| `ReadValue` | `object ReadValue(string keyPath, string valueName)` | Reads current registry value |
| `KeyExists` | `bool KeyExists(string keyPath)` | Validates key existence before write |

**Design Notes:**
- Always call `BackupValue` before `ApplyTweak`.
- Use `RegistryKey.OpenSubKey(path, writable: true)` and wrap in try/catch.
- Target `HKEY_LOCAL_MACHINE` for system-level tweaks (requires Admin).
- Store backups in a static `Dictionary<string, object> _backupStore` for session-lifetime restore.

---

### 4.2 `IniEditor.cs`
**Responsibility:** Parse, modify, and save `SYSTEM.INI` and `WIN.INI`.

**Target Files:**
- `C:\Windows\SYSTEM.INI`
- `C:\Windows\WIN.INI`

**Key Methods:**

| Method | Signature | Description |
|---|---|---|
| `ReadValue` | `string ReadValue(string filePath, string section, string key)` | Returns value for a `[section]\key` |
| `WriteValue` | `bool WriteValue(string filePath, string section, string key, string value)` | Writes or updates a key in section |
| `BackupFile` | `bool BackupFile(string filePath)` | Copies file to `<filename>.bak` before modification |
| `RestoreFromBackup` | `bool RestoreFromBackup(string filePath)` | Overwrites from `.bak` file |
| `SectionExists` | `bool SectionExists(string filePath, string section)` | Validates section presence |

**Design Notes:**
- Use `System.Runtime.InteropServices` → `GetPrivateProfileString` / `WritePrivateProfileString` for Windows-native INI parsing (most reliable for legacy INI files).
- Fallback: manual line-by-line parse with `StreamReader` if P/Invoke is undesirable.
- Always call `BackupFile` on first write per session.

---

### 4.3 `Logger.cs`
**Responsibility:** Thread-safe, timestamped, color-coded log output to the in-app `RichTextBox`.

**Key Methods:**

| Method | Signature | Description |
|---|---|---|
| `Log` | `void Log(string message, LogLevel level)` | Appends a timestamped message |
| `Clear` | `void Clear()` | Clears the log display |
| `Export` | `void Export(string filePath)` | Saves log to `.txt` file |

**LogLevel Enum:**
```csharp
public enum LogLevel { Info, Success, Warning, Error }
```

**Color Mapping:**
| Level | Color |
|---|---|
| Info | `Color.Silver` |
| Success | `Color.LimeGreen` |
| Warning | `Color.Gold` |
| Error | `Color.OrangeRed` |

**Design Notes:**
- Use `RichTextBox.Invoke()` for thread-safe UI updates.
- Auto-scroll to bottom after each log entry.
- Format: `[HH:mm:ss] [LEVEL] Message text`

---

### 4.4 `TweakDefinitions.cs`
**Responsibility:** Static data class holding all tweak definitions as structured objects.

```csharp
public class TweakDefinition {
    public string Id { get; set; }          // e.g., "WinPriority"
    public string DisplayName { get; set; } // e.g., "Optimize Win32 Priority (0x2E)"
    public string Description { get; set; }
    public TweakType Type { get; set; }     // Registry | IniFile
    public string Target { get; set; }      // Registry path or file path
    public string ValueName { get; set; }
    public object TweakValue { get; set; }
    public object DefaultValue { get; set; }
    public RegistryValueKind ValueKind { get; set; }
    public string Category { get; set; }    // "Priority" | "MMCSS" | "System.INI" | "Memory" | "Mitigations"
}

public enum TweakType { Registry, IniFile }
```

---

### 4.5 `TweakEngine.cs`
**Responsibility:** Orchestrates applying and restoring all tweaks. Calls `RegistryEditor` and `IniEditor` based on `TweakDefinition.Type`.

**Key Methods:**
- `ApplyAll(IEnumerable<TweakDefinition> tweaks)` — Apply all checked tweaks, log each result.
- `ApplyOne(TweakDefinition tweak)` — Apply a single tweak.
- `RestoreAll()` — Restore all backed-up defaults.
- `RestoreOne(TweakDefinition tweak)` — Restore a single tweak's backup.

---

## 5. Registry Tweaks Catalogue

| Tweak Name | Registry Path | Value Name | Tweak Value | Default Value | Type |
|---|---|---|---|---|---|
| Win32PrioritySeparation (0x2E) | `HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl` | `Win32PrioritySeparation` | `0x2E` (46) | `0x02` (2) | DWORD |
| Disable OS Mitigations (Spectre/Meltdown) | `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management` | `FeatureSettingsOverride` | `3` | `0` | DWORD |
| Mitigations Override Mask | `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management` | `FeatureSettingsOverrideMask` | `3` | `0` | DWORD |
| MMCSS Game Priority | `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games` | `Priority` | `6` | `2` | DWORD |
| MMCSS Scheduling Category | `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games` | `Scheduling Category` | `"High"` | `"Medium"` | String |
| MMCSS GPU Priority | `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games` | `GPU Priority` | `8` | `2` | DWORD |
| MMCSS System Responsiveness | `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile` | `SystemResponsiveness` | `0` | `20` | DWORD |
| Network Throttling Index (Disable) | `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile` | `NetworkThrottlingIndex` | `0xFFFFFFFF` | `10` | DWORD |
| Zero TimeSlice (IRQ8Priority) | `HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl` | `IRQ8Priority` | `1` | `0` | DWORD |
| Disable Dynamic Tick | `HKLM\SYSTEM\CurrentControlSet\Control\TimeZoneInformation` | `RealTimeIsUniversal` | `1` | `0` | DWORD |
| Large System Cache | `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management` | `LargeSystemCache` | `0` | `0` | DWORD |
| Disable Paging Executive | `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management` | `DisablePagingExecutive` | `1` | `0` | DWORD |

---

## 6. INI File Tweaks Catalogue

| Tweak Name | File | Section | Key | Tweak Value | Default Value |
|---|---|---|---|---|---|
| 386 Enhanced Mode IRQs | `SYSTEM.INI` | `[386Enh]` | `IRQ9=4` | `IRQ9=4` | *(absent)* |
| Disable Virtual Memory Swap | `SYSTEM.INI` | `[386Enh]` | `MinSPs=4` | `4` | *(absent)* |
| Win.INI Load Optimization | `WIN.INI` | `[windows]` | `load=` | *(empty)* | *(varies)* |

---

## 7. Error Handling Strategy

### 7.1 Exception Types & Responses

| Exception | Context | Handler Action |
|---|---|---|
| `UnauthorizedAccessException` | Registry write, INI file write | Log as `Error`, skip tweak, show advisory message |
| `FileNotFoundException` | INI file not found | Log as `Warning`, skip INI tweak gracefully |
| `SecurityException` | Registry key access denied | Log as `Error`, advise re-run as Admin |
| `IOException` | INI file locked or disk error | Log as `Error` with full exception message |
| `NullReferenceException` | Missing registry key/path | Log as `Warning`, create key if appropriate |
| General `Exception` | Catch-all in `TweakEngine` | Log as `Error`, continue to next tweak (never crash app) |

### 7.2 Strategy Principles
- **Never crash silently** — all exceptions are caught and logged.
- **Continue on failure** — one failed tweak must not halt the rest.
- **Backup before write** — always capture current value before modification.
- **Atomic restore** — restore uses the in-memory backup, not re-reading registry (avoids partial state).

---

## 8. Development Roadmap (Milestones)

### Phase 1 — Foundation
- [ ] Create `.NET 4.8` WinForms project in Visual Studio
- [ ] Add `app.manifest` with `requireAdministrator`
- [ ] Scaffold project folder structure
- [ ] Implement `Logger.cs` (no dependencies on other modules)
- [ ] Implement `TweakDefinition.cs` and populate tweak catalogue

### Phase 2 — Core Logic
- [ ] Implement `RegistryEditor.cs` (Read, Write, Backup, Restore)
- [ ] Implement `IniEditor.cs` (Parse, Write, Backup, Restore)
- [ ] Implement `TweakEngine.cs` (orchestration layer)
- [ ] Unit test all logic classes manually (no test framework needed)

### Phase 3 — UI
- [ ] Design `Form1` layout per `design.md`
- [ ] Wire up `btnApplyAll` → `TweakEngine.ApplyAll()`
- [ ] Wire up `btnRestoreDefaults` → `TweakEngine.RestoreAll()`
- [ ] Wire up individual checkboxes to `TweakDefinition` IDs
- [ ] Populate `rtbLog` via `Logger`

### Phase 4 — Polish & Packaging
- [ ] Apply color scheme and font settings from `design.md`
- [ ] Add About panel / version label
- [ ] Test on Windows 10 and Windows 11
- [ ] Build in `Release` mode
- [ ] Verify standalone `.exe` runs without Visual Studio

---

## 9. Folder Structure

```
JaiDee-Optimize/
├── JaiDee-Optimize.sln
├── JaiDee-Optimize/
│   ├── Properties/
│   │   ├── app.manifest          ← UAC requireAdministrator
│   │   └── AssemblyInfo.cs
│   ├── Core/
│   │   ├── RegistryEditor.cs
│   │   ├── IniEditor.cs
│   │   ├── TweakEngine.cs
│   │   ├── TweakDefinitions.cs
│   │   └── Logger.cs
│   ├── Models/
│   │   ├── TweakDefinition.cs
│   │   └── Enums.cs              ← LogLevel, TweakType
│   ├── UI/
│   │   ├── Form1.cs
│   │   └── Form1.Designer.cs
│   ├── Program.cs
│   └── JaiDee-Optimize.csproj
```
