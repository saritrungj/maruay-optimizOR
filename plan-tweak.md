# plan-tweak.md - JaiDee-Optimize UI Tweak Plan

## Goal

Upgrade the JaiDee-Optimize interface into a cleaner, more modern, easier-to-use system utility while syncing the practical layout logic from the reference workspace `nztsapp-main`.

This plan is written for the current WinForms project, not a web app. Web-specific ideas such as Tailwind `max-w-5xl`, Bootstrap `.modal-xl`, `80vw`, and `80vh` should be translated into WinForms sizing, layout panels, and owner-drawn rounded controls.

## Implementation Status

Completed in the current project:

- Added a reusable large modal flow for apply confirmation, restore confirmation, no-tweak guidance, and tweak details.
- Replaced cramped confirmation `MessageBox` usage with themed large dialogs where more context is needed.
- Converted the debloat profile switch from native tabs into a cleaner Win11/Win10 segmented selector.
- Kept Windows 10 and Windows 11 debloat groups separated while sharing the same row/toggle structure.
- Verified the project builds successfully with MSBuild using a temporary `bin\DebugCheck\` output path.

Still blocked:

- Exact functional synchronization with `nztsapp-main`, because that workspace path is not currently present on `d:\github-clone` or `d:\`.

## Reference Workspace Status

`nztsapp-main` was not found under `d:\github-clone` or `d:\` during the initial lookup. Before implementation, confirm the real path and inspect it.

Required audit commands once the path is available:

```powershell
rg --files <path-to-nztsapp-main>
rg -n "modal|dialog|max-w|modal-lg|modal-xl|grid|flex|tabs|sidebar|settings|layout" <path-to-nztsapp-main>
```

Extract only the structural and functional ideas:

- Modal sizing and content grouping.
- Navigation hierarchy.
- Settings/profile selection flow.
- Tweak grouping logic.
- Apply/restore/export action placement.
- Any guardrails or confirmation flows.

Do not copy visual clutter, excessive nesting, or framework-specific code that does not fit WinForms.

## Implementation Scope

Owned files:

- `optimizOR/UI/Form1.cs`
- `optimizOR/UI/Form1.Designer.cs`
- `design.md`

Potential supporting files:

- `optimizOR/Core/TweakDefinitions.cs`
- `optimizOR/Core/TweakEngine.cs`
- `readme.md`

No third-party UI libraries. Keep the app on .NET Framework 4.8 WinForms.

## Large Modal Strategy

The current app uses `MessageBox` and page panels rather than a reusable modal. Add a custom large modal form or overlay panel for flows that need more context, especially tweak details, debloat warnings, and apply confirmations.

Target size:

- Desktop modal: `720 x 500`
- Maximum: `80%` of owner form width and height
- Minimum: `560 x 360`
- Border radius: `14px`
- Padding: `24px`
- Header height: `56px`
- Footer height: `64px`

WinForms mapping:

- Use a custom `LargeDialogForm : Form` or an in-form overlay panel.
- `StartPosition = CenterParent`
- `FormBorderStyle = None` if custom chrome is stable, otherwise `FixedDialog`.
- `ShowInTaskbar = false`
- `MaximizeBox = false`
- `MinimizeBox = false`
- Use current `ThemePalette` tokens for all colors.

Recommended modal anatomy:

```text
LargeDialogForm
  Header
    Title
    Subtitle
    Close button
  Body
    Scrollable content area
    Grouped sections
  Footer
    Secondary action
    Primary action
```

Replace cramped `MessageBox` flows where extra clarity matters:

- Debloat confirmation.
- Tweak information/details.
- Restore warning.
- Export result/error.

Keep simple one-line alerts as `MessageBox` only when no extra action choice is needed.

## Layout And Grouping

Use the reference app as a hierarchy guide, then simplify for this project.

Target structure:

```text
Sidebar
  Logo
  Home
  Tweaks
  Log
  Settings
  About

Topbar
  Page title
  Page subtitle
  OS profile badge

Page content
  One primary workflow per page
  Action row near the workflow
  Secondary details below or inside a large modal
```

Tweaks page:

- Keep groups as readable sections, not deeply nested cards.
- Use one row pattern everywhere: icon/status, title, short description, toggle, info button.
- Keep Windows 10 and Windows 11 debloat separated.
- Replace native `TabControl` if it looks dated with a custom segmented selector:
  - `Windows 11`
  - `Windows 10`
- Show unavailable profile tweaks as disabled rows with a small profile chip.

Settings page:

- Keep OS profile selection at the top.
- Use two large selectable cards: Windows 11 and Windows 10.
- Show the Win10 warning inline only when Win10 is selected.
- Group preferences below profile selection.

Log page:

- Make the log panel wider and calmer.
- Use monospace text, color-coded levels, and clear/export actions in the same footer row.
- Avoid extra explanatory text.

Home page:

- Show one clear primary action: `Optimize now`.
- Show three compact stats: selected tweaks, OS profile, last run.
- Show quick group status using small chips.

## UX/UI Enhancement Rules

Modernize beyond the reference app with these choices:

- Increase negative space around page content and section headers.
- Use 12-14px panel radii and 10px button radii consistently.
- Avoid nested cards.
- Prefer sentence case labels.
- Use lighter font weights, mostly regular and medium-equivalent.
- Keep blue accent usage purposeful, not everywhere.
- Use orange only for destructive or Windows 10 caution states.
- Keep every button rounded.
- Keep all theme colors routed through `ThemePalette`.
- Ensure text never overlaps controls at Thai and English lengths.

Theme requirements:

- Dark base: `#0F1117`
- Dark surface: `#161922`
- Dark raised: `#1E2230`
- Dark accent: `#2D8EFF`
- Light base: `#F8F9FC`
- Light surface: `#FFFFFF`
- Light raised: `#F1F4F9`
- Light accent: `#185FA5`

## Functional Sync Checklist

After the reference workspace is available, map its behavior to JaiDee-Optimize:

- [ ] Identify all modal open/close triggers.
- [ ] Identify any modal validation or confirmation logic.
- [ ] Identify how action buttons are grouped.
- [ ] Identify how grouped settings are collapsed, filtered, or switched.
- [ ] Identify any state persistence behavior.
- [ ] Map matching behavior into WinForms without adding framework dependencies.
- [ ] Remove redundant steps that make the workflow slower.

## Implementation Steps

1. Reference audit
   - Locate `nztsapp-main`.
   - Record relevant modal/layout files.
   - Summarize reusable behavior before editing code.

2. Modal foundation
   - Add reusable large modal form or overlay.
   - Wire theme palette, rounded panel painting, close behavior, and footer buttons.
   - Move tweak info and debloat warning into the large modal.

3. Layout simplification
   - Reduce nested panels where possible.
   - Normalize page padding and section spacing.
   - Replace dated controls, especially native debloat tabs if needed.

4. Grouping polish
   - Align all tweak rows to one row system.
   - Add profile chips for Win10/Win11-specific rows.
   - Keep action rows sticky enough to be easy to find inside the fixed app window.

5. Visual polish
   - Tune radii, colors, hover states, and disabled states.
   - Check dark and light themes.
   - Verify Thai and English copy widths.

6. QA
   - Build with MSBuild.
   - Verify no new third-party packages.
   - Verify UI remains responsive during apply/restore.
   - Verify debloat warnings clearly explain irreversible Appx removal behavior.

## Acceptance Criteria

- The large modal feels like a standard large modal, not a small alert box.
- The UI hierarchy is clearer than the original reference.
- Tweaks are grouped by task and OS profile.
- The workflow is simple: choose profile, review tweaks, apply, inspect log.
- Dark and light themes both look complete.
- All buttons are rounded and visually consistent.
- No UI text overlap in Thai or English.
- Build succeeds under .NET Framework 4.8.

## Open Dependency

Implementation should not claim exact sync with `nztsapp-main` until the reference workspace path is provided or restored. If the reference cannot be found, proceed with the current `design.md` structure and this plan as the source of truth.
