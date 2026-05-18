# design.md - JaiDee-Optimize UI/UX Architecture

## 1. Direction

JaiDee-Optimize uses a minimal system-utility layout with a liquid glass aesthetic. The UI should feel quiet, precise, and readable: soft translucent panels, rounded controls, restrained blue accents, and no all-caps button text.

The app supports light and dark themes. Every visual color must come from the theme palette in `Form1.ThemePalette`, with automatic adaptation when the theme toggle changes.

## 2. Window

| Property | Value |
|---|---|
| Size | `820 x 620` client area |
| Resize | Fixed single window |
| Sidebar | `64px` icon-only navigation |
| Topbar | `48px` with page title, subtitle, OS profile badge |
| Content | Scrollable page area with `20px` padding |

## 3. Palette

| Token | Light | Dark |
|---|---|---|
| Base | `#F8F9FC` | `#0F1117` |
| Surface | `#FFFFFF` | `#161922` |
| Raised | `#F1F4F9` | `#1E2230` |
| Glass | `rgba(0,0,0,0.04)` | `rgba(255,255,255,0.06)` |
| Glass border | `rgba(0,0,0,0.08)` | `rgba(255,255,255,0.12)` |
| Glass hover | `rgba(0,0,0,0.06)` | `rgba(255,255,255,0.10)` |
| Accent | `#185FA5` | `#2D8EFF` |
| Accent dim | `#0C3D73` | `#1A5FA8` |
| Accent tint | `rgba(24,95,165,0.08)` | `rgba(45,142,255,0.12)` |
| Warning | `#FF6B00` | `#FF6B00` |
| Text | `#111318` | `#F0F0F0` |
| Muted | `#6B7280` | `#8A8FA8` |
| Log bg | `#F4F5F7` | `#08090D` |

Log levels:

- OK: `#2D8EFF`
- WARN: `#FFD700`
- ERR: `#FF4444`
- INFO: `#6A9FD8`

## 4. Typography

Use the system stack where available: `Segoe UI`, `Inter`, `-apple-system`, sans-serif. Use `Consolas` or `Fira Code` for logs.

| Role | Size | Weight |
|---|---:|---:|
| Meta/label | 11px | 400 |
| Description | 12px | 400 |
| Body | 13px | 400 |
| Button | 12-14px | 500-equivalent |
| Heading | 18-20px | 500-equivalent |

Avoid heavy bold weights. In WinForms, prefer `FontStyle.Regular` when a true medium weight is unavailable.

## 5. Layout

```text
Sidebar 64px
  Logo mark 36x36, radius 10
  Nav: Home, Tweaks, Log
  Utility nav: Settings, About

Main
  Topbar 48px
    Page title + subtitle
    OS profile badge
  Content
    HOME
    TWEAKS
    LOG
    SETTINGS
    ABOUT
```

Navigation swaps pages instantly with no page-transition animation.

## 6. Pages

HOME:

- Hero glass panel with 72x72 blue-tinted lightning mark
- Title: `Optimize your system`
- Subtitle: `Tweaks ready, [OS profile] loaded`
- Primary CTA: `Optimize now`
- Stats row: active tweaks, OS profile, last run
- Quick status panel for tweak groups

TWEAKS:

- Glass cards for Priority & Scheduling, Latency & Timers, Memory & Paging, INI Tweaks, and Debloat
- Each row uses label text plus a toggle switch
- Bottom action row: `Apply selected`, `Restore defaults`, `Turn all on/off`

LOG:

- Glass log panel
- Monospace log area
- Actions: `Clear`, `Export .txt`

SETTINGS:

- OS profile selector: Windows 11 / Windows 10
- Selected profile uses accent border and tint
- Windows 10 uses orange badge styling
- Preferences include language and theme toggles

ABOUT:

- App identity block
- Version, platform, requirements, backup behavior, active profile
- Safety advisory

## 7. Components

Panels:

- Cards and panels use 12-14px radius
- Glass fill plus 0.5-1px translucent border

Buttons:

- Radius: 10px
- Primary: accent blue background, white text
- Ghost: raised surface, glass border
- Danger ghost: orange text, orange-tinted border
- Text is sentence case

Toggle:

- Track: 36x20px, 10px radius
- Thumb: 14x14px white circle
- Off: raised bg with glass border
- On: accent-dim bg with accent border behavior where available
- Transition: short and snappy

OS badge:

- Pill-shaped rounded button in the topbar
- Windows 11: blue tint
- Windows 10: orange tint
- Click navigates to Settings

## 8. Implementation Notes

WinForms does not support CSS variables or true blur-backed glass. The implementation approximates the spec with owner-drawn rounded panels, alpha-blended glass colors, theme palette tokens, and custom rounded button/toggle controls.
