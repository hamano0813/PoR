# C# / Avalonia Rewrite

Current status: **AvaloniaFluentUI variant** (branch
`izumipl-avalonia-fluent-ui`, 2026-11-13).

## AvaloniaFluentUI variant

This branch is the C#-only UI version of the completed editor:

- Uses `AvaloniaFluentUI` 2.0.6 with `AvaloniaFluentTheme` and accent `#0E8FA3`;
  the built-in `Avalonia.Themes.Fluent` package/theme is not used.
- Uses `FluentWindow` for the managed title bar; the maximize button is hidden
  with `MaxButtonIsVisible="False"` while minimize and close remain available.
- Keeps the fixed 1440×800 window (no resize/maximize) and the same 280px
  roster, form spacing and bottom action bar as the `avalonia` branch.
- Uses a custom numeric editor: two near-square, borderless/transparent
  Fluent-style buttons (decrease on the left, increase on the right) overlaid
  on a full-width text field; the buttons stay transparent on hover and only
  the arrow changes to the accent color. The text field reserves button-width
  padding on both sides and centers the value. Increment/decrement are
  ViewModel commands with range clamping on the shared
  `NumericFieldViewModel`.
- Status page layout: a 10-column × 108px grid (1080px of content, 40px row
  height). Labels are 108px; person/class/3D model dropdowns span 4 columns
  (432px), ally spans 2 columns (216px), move AI spans 3 columns (324px), and
  the behavior AI row uses one 108px label followed by three 324px dropdowns.
  Ordinary numeric and boolean controls are 108px wide. Faction uses one
  standard cell (108px); army uses two (216px).
- The page is split into separate card panels: status on top, then two
  side-by-side panels (10px apart) for ability (5 × 108px: stat label/value,
  growth value, weapon-proficiency label/value) and equipment. Equipment uses
  four weapon rows and four item rows; the item dropdown spans two 108px cells
  (216px), quantity is 108px, the equipped checkbox column is 72px, and dropped
  is 108px. The equipped column saves 36px, matching the 10px panel gap plus
  12px panel padding and 1px border on each side. The equipment card is
  therefore 530px wide (504px content + 24px padding + 2px border), so the
  lower row is 566 + 10 + 530 = 1106px, exactly matching the 1106px status card
  and aligning all three panel edges and visible control edges.
- A third row (320px high) contains three more cards: support (566px, matching
  the ability panel), active skills (260px), and hidden features (260px). The
  support panel uses 20px affinity icons, a 128px icon+name column, an 88px
  points column, and a 316px cumulative progress bar with C/B threshold ticks.
  The old “support value” header was removed. The page uses an automatic
  vertical scrollbar when the panels exceed the window height. All panels are
  part of the single status page; the old tab UI and separate tab views have
  been removed, while the ability, equipment, skill and support data still
  share their underlying ViewModel instances.
- Shares all ViewModels, localization, memory access and page behavior with the
  C# editor.

## Projects

```text
PoR.sln
├─ src/
│  ├─ PoR.App/                 # Avalonia UI (roster + single status page + language/theme)
│  ├─ PoR.Core/                # memory abstractions, ValueCodec, mapping models
│  ├─ PoR.Game/                # catalog, mapping, slot snapshot, GameSession
│  └─ PoR.Memory.Dolphin/      # Windows/GameCube MEM1 port of DME
├─ tests/
│  └─ PoR.Core.Tests/          # value codec / catalog / mapping / snapshot tests
└─ tools/
   └─ PoR.Tools.Probe/         # console probe for manual verification
```

## Run the Avalonia app

```bash
cd C:\iCode\PoR
dotnet run --project src/PoR.App/PoR.App.csproj
```

From VS Code: open `C:\iCode\PoR`, choose the **PoR.App (Debug)** launch
configuration and press F5. The launch config builds the app first. The C#
extension and Avalonia extension are recommended (see the refactor plan).

Current UI scope:

- left roster with transparent-background avatars, names/classes and selection
- compact status page modeled on the original Qt layout: identity/faction/army,
  flags, level/EXP/HP, status effects, coordinates, MTYPE/SEQ, battle/victory
- the status page also contains ability/equipment and support/skills/features
  panels; all panels share their underlying ViewModel instances
- numeric fields use a custom compact editor: near-square Fluent-style
  decrease/increase buttons around a centered value; the arrows are hidden by
  default and appear in the accent color on hover or keyboard focus. Clicking
  empty space clears text-field focus.
- tri-state action checkbox: unchecked = not acted, indeterminate = 10 ms
  "lock not acted" polling, checked = acted
- skill/feature panels: two scrollable lists with localized names and icons
- equipment panel: eight item/durability/equipped/dropped rows with item icons
- support panel: current affinity, seven support partners, support points and
  C/B/A progress bars; affinity icons are 20px
- the old Skill/Item/Support/Other tabs and the compact tab control have been
  removed; their controls now live directly on the status page
- bottom status bar: left-aligned refresh (accent) followed by editable
  money/EXP text fields; theme and language stay right-aligned. The separate
  reconnect button was removed because refresh re-hooks when disconnected.
- language switching reloads game terms only; UI labels and buttons stay in
  Chinese; the theme button now shows the current mode (深色/浅色)
- action status has its own label; the rescue/rescued checkboxes share a compact
  horizontal group; checkbox text is vertically centered and uses the same
  12px size as other controls
- `tools/process_avatars.py` regenerates the de-fringed PNG portraits used by
  the roster and the character drop-down (requires Pillow)

All editor fields share the application-level editor DataTemplates in
`src/PoR.App/App.axaml`, so numeric/enum/bool controls look and behave the same
throughout the editor.

## Build

```bash
cd C:\iCode\PoR
dotnet build PoR.sln
```

## Publish

Two single-file publish profiles are available:

```bash
cd C:\iCode\PoR

# Self-contained, compressed: about 52 MB, no .NET runtime required.
dotnet publish src/PoR.App/PoR.App.csproj -p:PublishProfile=win-x64-standalone

# Framework-dependent: about 41 MB, requires the .NET 10 Desktop Runtime.
dotnet publish src/PoR.App/PoR.App.csproj -p:PublishProfile=win-x64-framework-dependent
```

Outputs:

- `publish/win-x64/PoR.App.exe` — standalone distribution build
- `publish/win-x64-framework-dependent/PoR.App.exe` — smaller build for machines
  with the .NET 10 Desktop Runtime installed

Publish symbols are removed automatically; each directory contains only the
single EXE. The EXE embeds `Assets/icon.ico` and the following metadata, mapped
in the C# project (the old `version.txt` has been removed):

- File version: 1.9.6.0
- Product version: 1.9.6
- Company: Hamano0813
- Product: Fire Emblem Path of Radiance Modifier V1.9.6
- Description: 火焰纹章 苍炎的轨迹 动态修改器
- Copyright: Copyright © 2021-2026 Hamano0813

Trimming is intentionally disabled: Avalonia/AvaloniaFluentUI plus reflection-based
resource loading make aggressive trimming risky. `EnableCompressionInSingleFile`
and the framework-dependent profile are the safe size reductions used here.

## Probe

Start Dolphin and boot Path of Radiance first, then:

```bash
dotnet run --project tools/PoR.Tools.Probe/PoR.Tools.Probe.csproj
dotnet run --project tools/PoR.Tools.Probe/PoR.Tools.Probe.csproj -- --json

# Safe write round-trip: reads money/level and writes the same bytes back.
dotnet run --project tools/PoR.Tools.Probe/PoR.Tools.Probe.csproj -- --json --write-self-test

# Full GameSession refresh: dynamic mappings + slot 0 status.
dotnet run --project tools/PoR.Tools.Probe/PoR.Tools.Probe.csproj -- --json --session

# GameSession bitfield/global write round-trip.
dotnet run --project tools/PoR.Tools.Probe/PoR.Tools.Probe.csproj -- --json --session --session-write-self-test
```

## Tests

```bash
dotnet test PoR.sln
```

## Verification

The C# probe is the current verification path. Start Dolphin, boot Path of
Radiance, then run the commands in the Probe section above. Parity with the
legacy Python editor was verified before the Python sources were removed.

Expected states:

- `NotRunning`: Dolphin process not found.
- `NoEmu`: Dolphin is running but no GameCube MEM1 (usually no game booted).
- `Hooked`: reads person/class/level/money and the 159-slot snapshot.
- `AccessDenied`: run Dolphin and the probe at the same privilege level.

The probe reads:

| Field | Console address |
|---|---|
| Person | `0x802AF76C` |
| Class | `0x802AF770` |
| Level | `0x802AF77E` |
| Money | `0x8032E7A1` (3 bytes) |
| Slot snapshot | `0x802AF5E4`, 159 × `0x280` |

## Game data

The embedded JSON catalog under `src/PoR.Game/Data/` and the localization files
under `src/PoR.App/Localization/` are the current source of truth.

Validation report: `data-report.md`.

## Prerequisites

- .NET 10 SDK (10.0.404 or later)
- Avalonia templates 12.1.6 when P3 starts
- See `avalonia-refactor-plan.md` section 14 for commands.
