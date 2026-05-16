# design.md — Windows Gaming Optimizer: UI/UX Architecture

---

## 1. Design Philosophy

**Goal:** Make standard WinForms look sharp, dark, and functional — like a professional system utility — **without any external UI libraries** (no DevExpress, Telerik, MetroFramework, etc.).

**Aesthetic Direction:** *Industrial / Dark Utility* — Dark charcoal background, sharp neon-green accent, monospace log font. Think system-level tool, not a consumer app. Clean, dense, purposeful.

---

## 2. Form Properties (`Form1`)

| Property | Value | Rationale |
|---|---|---|
| `Name` | `Form1` | Standard |
| `Text` | `WinGameOpt v1.0 — Windows Gaming Optimizer` | Clear, versioned title |
| `Size` | `820, 640` | Comfortable without being oversized |
| `MinimumSize` | `820, 640` | Prevent UI collapse |
| `MaximizeBox` | `false` | Fixed layout, no maximize needed |
| `FormBorderStyle` | `FixedSingle` | Prevents resize, cleaner look |
| `StartPosition` | `CenterScreen` | Professional default |
| `BackColor` | `#1A1A1A` (Color from Hex) | Deep charcoal base |
| `ForeColor` | `#E0E0E0` | Soft white text |
| `Font` | `Segoe UI, 9pt, Regular` | Crisp, Windows-native |
| `Icon` | Embedded `.ico` resource | Professional branding |
| `ShowIcon` | `true` | |

**Applying Dark Background in WinForms (no library needed):**
```csharp
this.BackColor = ColorTranslator.FromHtml("#1A1A1A");
this.ForeColor = ColorTranslator.FromHtml("#E0E0E0");
```

---

## 3. Color Scheme

| Role | Hex | Usage |
|---|---|---|
| Background (Deep) | `#1A1A1A` | Form background, GroupBox background |
| Background (Surface) | `#242424` | Panel backgrounds, GroupBox inner |
| Background (Raised) | `#2E2E2E` | Button normal state, CheckBox area |
| Accent (Primary) | `#39FF14` | Apply All button, success log, active checkbox tick |
| Accent (Secondary) | `#FF6B00` | Warning states, Restore button |
| Border / Separator | `#3A3A3A` | GroupBox borders, panel edges |
| Text (Primary) | `#E8E8E8` | Labels, checkbox text |
| Text (Muted) | `#888888` | Descriptions, hints |
| Text (Log Info) | `#B0B0B0` | Log info messages |
| Text (Log Success) | `#39FF14` | Log success messages |
| Text (Log Warning) | `#FFD700` | Log warning messages |
| Text (Log Error) | `#FF4444` | Log error messages |

---

## 4. Font Definitions

| Usage | Font | Size | Style |
|---|---|---|---|
| Form base / labels | `Segoe UI` | `9pt` | Regular |
| GroupBox titles | `Segoe UI` | `9pt` | Bold |
| Button text | `Segoe UI` | `9.5pt` | Bold |
| Log output (`rtbLog`) | `Consolas` | `8.5pt` | Regular |
| Header / Title label | `Segoe UI` | `13pt` | Bold |
| Version / subtitle | `Segoe UI` | `8pt` | Regular (Italic) |

---

## 5. Full Control Hierarchy & Layout

```
Form1 (820 x 640)
│
├── pnlHeader (Panel) — Top strip, 820 x 60
│   ├── lblAppTitle (Label)         "⚡ WinGameOpt"
│   └── lblSubtitle (Label)         "DPC Latency & Input Lag Optimizer | v1.0"
│
├── pnlMain (Panel) — Left column, 500 x 540
│   │
│   ├── grpPriority (GroupBox)      "⚙  Priority & Scheduling Tweaks"
│   │   ├── chkWinPriority          "Optimize Win32PrioritySeparation (0x2E)"
│   │   ├── chkMMCSS_GamePriority   "Boost MMCSS Game Task Priority"
│   │   ├── chkMMCSS_HighCategory   "Set MMCSS Scheduling Category: High"
│   │   ├── chkMMCSS_GPUPriority    "Set MMCSS GPU Priority (8)"
│   │   └── chkMMCSS_Responsiveness "Set SystemResponsiveness to 0"
│   │
│   ├── grpLatency (GroupBox)       "⏱  Latency & Timer Tweaks"
│   │   ├── chkNetworkThrottle      "Disable Network Throttling Index"
│   │   ├── chkZeroTimeSlice        "Zero TimeSlice (IRQ8 Priority Boost)"
│   │   └── chkDynamicTick          "Set RealTimeIsUniversal (Timer Fix)"
│   │
│   ├── grpMemory (GroupBox)        "🧠  Memory & Paging Tweaks"
│   │   ├── chkDisablePaging        "Disable Paging Executive (Keep in RAM)"
│   │   ├── chkLargeSystemCache     "Optimize Large System Cache (Gaming)"
│   │   └── chkMitigations          "Disable OS Mitigations (Spectre/Meltdown)"
│   │
│   └── grpIniTweaks (GroupBox)     "📄  System.INI & Win.INI Tweaks"
│       ├── chkIniIRQ               "Inject IRQ9=4 in [386Enh] (SYSTEM.INI)"
│       ├── chkIniMinSPs            "Set MinSPs=4 in [386Enh] (SYSTEM.INI)"
│       └── chkIniWinLoad           "Clear Win.INI Load= Entry"
│
├── pnlActions (Panel) — Below left column, 500 x 60
│   ├── btnApplyAll (Button)        "✔  APPLY ALL TWEAKS"
│   ├── btnRestoreDefaults (Button) "↩  RESTORE DEFAULTS"
│   └── btnSelectAll (Button)       "☑  Select All"
│
└── pnlLog (Panel) — Right column, 300 x 540
    ├── lblLogTitle (Label)         "📋 Operation Log"
    ├── rtbLog (RichTextBox)        [Scrollable log output]
    └── btnClearLog (Button)        "Clear Log"
```

---

## 6. Control Specifications

### 6.1 Header Panel (`pnlHeader`)

```
pnlHeader
  Dock:    Top
  Height:  60
  BackColor: #242424
  BorderStyle: None
```

```
lblAppTitle
  Text:      "⚡ WinGameOpt"
  Font:      Segoe UI, 14pt, Bold
  ForeColor: #39FF14
  Location:  12, 14
  AutoSize:  true

lblSubtitle
  Text:      "DPC Latency & Input Lag Optimizer  |  v1.0  |  Running as Administrator"
  Font:      Segoe UI, 8pt, Italic
  ForeColor: #888888
  Location:  12, 38
  AutoSize:  true
```

---

### 6.2 GroupBox Styling (Applied to All GroupBoxes)

Standard WinForms `GroupBox` does not natively support dark theming on its border/title. Use this approach:

```csharp
// In Form1.cs — custom paint GroupBox borders
private void PaintGroupBox(object sender, PaintEventArgs e) {
    GroupBox box = sender as GroupBox;
    e.Graphics.Clear(ColorTranslator.FromHtml("#1A1A1A"));

    // Draw border
    using (Pen pen = new Pen(ColorTranslator.FromHtml("#3A3A3A")))
        e.Graphics.DrawRectangle(pen, 0, 10, box.Width - 1, box.Height - 11);

    // Draw title background + text
    using (Brush brush = new SolidBrush(ColorTranslator.FromHtml("#1A1A1A")))
        e.Graphics.FillRectangle(brush, 8, 0, box.Text.Length * 7 + 8, 16);

    using (Font f = new Font("Segoe UI", 9f, FontStyle.Bold))
    using (Brush textBrush = new SolidBrush(ColorTranslator.FromHtml("#39FF14")))
        e.Graphics.DrawString(box.Text, f, textBrush, 10, 1);
}
// Register: grpPriority.Paint += PaintGroupBox; (for each group)
```

**GroupBox Layout Properties (all):**
```
BackColor:   #1A1A1A
ForeColor:   #E0E0E0
Font:        Segoe UI, 9pt, Bold
Padding:     new Padding(10, 18, 10, 10)
```

---

### 6.3 CheckBox Controls (All Tweak Checkboxes)

**Naming Convention:** `chk` + `CategoryPrefix` + `TweakName`  
Examples: `chkWinPriority`, `chkMMCSS_GamePriority`, `chkIniIRQ`

**Properties (applied uniformly):**
```
FlatStyle:    Flat
BackColor:    #1A1A1A
ForeColor:    #E0E0E0
Font:         Segoe UI, 9pt
Checked:      true (default — all tweaks pre-selected)
Cursor:       Hand
AutoSize:     true
```

**Override checkbox check-mark color (owner-draw):**
```csharp
// The neon green tick requires owner-drawn checkboxes:
// Set Appearance = Appearance.Button with FlatStyle for full control,
// OR use a CheckedListBox with DrawMode.OwnerDrawFixed for consistency.
// Simpler approach: Accept system default tick and rely on the dark
// background + ForeColor contrast for a clean look.
```

---

### 6.4 Buttons

#### `btnApplyAll` — Primary Action
```
Text:         "✔  APPLY ALL TWEAKS"
Size:         220, 42
BackColor:    #39FF14
ForeColor:    #0A0A0A
Font:         Segoe UI, 10pt, Bold
FlatStyle:    Flat
FlatAppearance.BorderSize: 0
Cursor:       Hand
```

Hover effect (manual, in `MouseEnter`/`MouseLeave`):
```csharp
btnApplyAll.MouseEnter += (s,e) => btnApplyAll.BackColor = ColorTranslator.FromHtml("#5FFF3A");
btnApplyAll.MouseLeave += (s,e) => btnApplyAll.BackColor = ColorTranslator.FromHtml("#39FF14");
```

#### `btnRestoreDefaults` — Destructive / Warning Action
```
Text:         "↩  RESTORE DEFAULTS"
Size:         180, 42
BackColor:    #2E2E2E
ForeColor:    #FF6B00
Font:         Segoe UI, 9.5pt, Bold
FlatStyle:    Flat
FlatAppearance.BorderColor:  #FF6B00
FlatAppearance.BorderSize:   1
Cursor:       Hand
```

#### `btnSelectAll` — Utility
```
Text:         "☑  Select All"
Size:         110, 42
BackColor:    #2E2E2E
ForeColor:    #B0B0B0
Font:         Segoe UI, 9pt
FlatStyle:    Flat
FlatAppearance.BorderColor: #3A3A3A
FlatAppearance.BorderSize:  1
Cursor:       Hand
```

#### `btnClearLog` — Log Utility
```
Text:         "Clear Log"
Size:         90, 26
BackColor:    #2E2E2E
ForeColor:    #888888
Font:         Segoe UI, 8pt
FlatStyle:    Flat
FlatAppearance.BorderColor: #3A3A3A
FlatAppearance.BorderSize:  1
```

---

### 6.5 Log Panel (`pnlLog` + `rtbLog`)

```
pnlLog
  Width:      290
  Dock:       Right
  BackColor:  #1A1A1A
  Padding:    8

lblLogTitle
  Text:       "📋  Operation Log"
  Font:       Segoe UI, 9pt, Bold
  ForeColor:  #888888

rtbLog
  Name:       rtbLog
  Dock:       Fill (within pnlLog, minus title area)
  BackColor:  #0F0F0F
  ForeColor:  #B0B0B0
  Font:       Consolas, 8.5pt
  ReadOnly:   true
  BorderStyle: None
  ScrollBars: Vertical
  WordWrap:   true
```

---

## 7. Layout Diagram (ASCII)

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│  ⚡ WinGameOpt                                            [Dark Header #242424]  │
│  DPC Latency & Input Lag Optimizer  |  v1.0  |  Running as Administrator        │
├────────────────────────────────────────────────┬────────────────────────────────┤
│  ┌──────────────────────────────────────────┐  │  📋 Operation Log         [Clr]│
│  │ ⚙  Priority & Scheduling Tweaks          │  │                                │
│  │  ☑ Optimize Win32PrioritySeparation      │  │ [HH:mm:ss] [INFO] App started  │
│  │  ☑ Boost MMCSS Game Task Priority        │  │ [HH:mm:ss] [OK]   WinPriority  │
│  │  ☑ Set MMCSS Scheduling Category: High  │  │ [HH:mm:ss] [WARN] INI missing  │
│  │  ☑ Set MMCSS GPU Priority (8)           │  │ [HH:mm:ss] [ERR] Access denied │
│  │  ☑ Set SystemResponsiveness to 0        │  │                                │
│  └──────────────────────────────────────────┘  │                                │
│  ┌──────────────────────────────────────────┐  │                                │
│  │ ⏱  Latency & Timer Tweaks               │  │                                │
│  │  ☑ Disable Network Throttling Index     │  │                                │
│  │  ☑ Zero TimeSlice (IRQ8 Priority)       │  │                                │
│  │  ☑ Set RealTimeIsUniversal              │  │                                │
│  └──────────────────────────────────────────┘  │                                │
│  ┌──────────────────────────────────────────┐  │                                │
│  │ 🧠  Memory & Paging Tweaks              │  │                                │
│  │  ☑ Disable Paging Executive             │  │                                │
│  │  ☑ Optimize Large System Cache          │  │                                │
│  │  ☑ Disable OS Mitigations               │  │                                │
│  └──────────────────────────────────────────┘  │                                │
│  ┌──────────────────────────────────────────┐  │                                │
│  │ 📄  System.INI & Win.INI Tweaks         │  │                                │
│  │  ☑ Inject IRQ9=4 (SYSTEM.INI)          │  │                                │
│  │  ☑ Set MinSPs=4   (SYSTEM.INI)          │  │                                │
│  │  ☑ Clear Win.INI Load= Entry            │  │                                │
│  └──────────────────────────────────────────┘  │                                │
├────────────────────────────────────────────────┤                                │
│  [✔ APPLY ALL TWEAKS]  [↩ RESTORE DEFAULTS]  [☑ Select All]                  │
└─────────────────────────────────────────────────┴───────────────────────────────┘
```

---

## 8. Naming Conventions Summary

| Control Type | Prefix | Example |
|---|---|---|
| Form | (none) | `Form1` |
| Panel | `pnl` | `pnlHeader`, `pnlMain`, `pnlLog` |
| GroupBox | `grp` | `grpPriority`, `grpMemory`, `grpIniTweaks` |
| CheckBox | `chk` | `chkWinPriority`, `chkMitigations` |
| Button | `btn` | `btnApplyAll`, `btnRestoreDefaults` |
| Label | `lbl` | `lblAppTitle`, `lblSubtitle` |
| RichTextBox | `rtb` | `rtbLog` |
| ToolTip | `tip` | `tipMain` |

---

## 9. ToolTip Descriptions

Attach a single `ToolTip` component (`tipMain`) and set descriptions on each checkbox:

```csharp
tipMain.SetToolTip(chkWinPriority,
    "Sets Win32PrioritySeparation to 0x2E.\nBoosts foreground process scheduling for games.");
tipMain.SetToolTip(chkMitigations,
    "Disables Spectre/Meltdown mitigations.\nIncreases performance. Use only on trusted hardware.");
```

**ToolTip Styling:**
```csharp
tipMain.BackColor = ColorTranslator.FromHtml("#242424");
tipMain.ForeColor = ColorTranslator.FromHtml("#E0E0E0");
tipMain.IsBalloon = false;
tipMain.AutoPopDelay = 5000;
```

---

## 10. Accessibility & UX Details

- All buttons have `TabIndex` set in logical flow order (top-to-bottom, left-to-right).
- `btnApplyAll` is set as the `AcceptButton` for Enter-key support.
- `rtbLog` auto-scrolls: call `rtbLog.ScrollToCaret()` after each log append.
- A `ProgressBar` (`prgApply`) can optionally be shown during `ApplyAll` beneath the action buttons (hidden by default, shown only during operation).
- Confirmation `MessageBox` before applying: `"Apply all selected tweaks? A restart may be required for some changes."` 
- Confirmation `MessageBox` before restoring: `"This will restore all registry and INI values to their defaults. Continue?"` with `MessageBoxIcon.Warning`.
