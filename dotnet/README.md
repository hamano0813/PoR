# C# / Avalonia Rewrite

Current status: **FluentAvalonia UI variant** (branch `avalonia-fluent-ui`,
2026-11-12).

## FluentAvalonia variant

This branch is a parallel UI version of the completed editor:

- Uses `FluentAvaloniaUI` 3.1.0 with `FluentAvaloniaTheme` and accent `#0E8FA3`;
  the built-in `Avalonia.Themes.Fluent` package/theme is not used.
- Uses `FAAppWindow` for the managed title bar and `FAInfoBar` for the warning
  card on the Other tab.
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
- A third row (426px high) contains three more cards: support (566px, matching
  the ability panel), active skills (260px), and hidden features (260px). The
  support panel uses 20px affinity icons, a 128px icon+name column, an 88px
  points column, and 108px progress-bar columns; the old “support value” header
  was removed. The page uses an automatic vertical scrollbar when the panels
  exceed the window height. Ability/equipment/skill/support tabs share the same
  ViewModel instances as the status page, so edits stay in sync.
- Shares all ViewModels, localization, memory access and page behavior with the
  `avalonia` branch.

## Projects

```text
dotnet/
├─ PoR.sln
├─ src/
│  ├─ PoR.App/                 # Avalonia UI (roster + six editor tabs + language/theme)
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
cd C:\iCode\PoR\dotnet
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
  panels; the skill and support tabs reuse the same ViewModel instances
- numeric fields use a custom compact editor: near-square Fluent-style
  decrease/increase buttons around a centered value; the arrows are hidden by
  default and appear in the accent color on hover or keyboard focus. Clicking
  empty space clears text-field focus.
- tri-state action checkbox: unchecked = not acted, indeterminate = 10 ms
  "lock not acted" polling, checked = acted
- skill tab: two scrollable skill/feature lists with localized names and icons
- item tab: eight item/durability/equipped/dropped rows with item icons
- support tab/panel: current affinity, seven support partners, support points
  and C/B/A progress bars; affinity icons are 20px
- other tab: the original modification notes (money/bonus EXP moved to the
  bottom status bar)
- bottom status bar: editable money/bonus-EXP text fields, refresh (accent),
  theme, language; the separate reconnect button was removed because refresh
  re-hooks when disconnected
- language switching reloads game terms only; UI labels and buttons stay in
  Chinese
- action status has its own label; the rescue/rescued checkboxes share a compact
  horizontal group; checkbox text is vertically centered and uses the same
  12px size as other controls
- tab pages use `CompactTabControl` to remove the FluentAvalonia 12px selected
  content margin and tighten the outer page spacing
- `tools/process_avatars.py` regenerates the de-fringed PNG portraits used by
  the roster and the character drop-down (requires Pillow)

All editor fields share the application-level editor DataTemplates in
`src/PoR.App/App.axaml`, so numeric/enum/bool controls look and behave the same
across tabs.

## Build

```bash
cd dotnet
dotnet build PoR.sln
```

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

## Parity check

With the legacy venv from `../tools/legacy/README.md` and Dolphin running the
game:

```bash
# from the repository root
.venv/Scripts/python.exe -I tools/verify_parity.py
```

Verified on 2026-11-11 against Dolphin 2609b + Path of Radiance:

- status `Hooked`
- person/class/level/money/visibleSlots and slot samples match Python exactly
- write round-trip self-test passes

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

## Generated data

`src/PoR.Game/Data/*.json` and `src/PoR.App/Localization/terms.*.json` are
generated by the scripts in `../tools/migrate/`. Do not hand-edit them until the
C# data catalog becomes the source of truth.

Validation report: `../docs/data-report.md`.

## Prerequisites

- .NET 10 SDK (10.0.404 or later)
- Avalonia templates 12.1.6 when P3 starts
- See `../docs/avalonia-refactor-plan.md` section 14 for commands.
