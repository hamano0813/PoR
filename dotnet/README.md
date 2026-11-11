# C# / Avalonia Rewrite

Current status: **FluentAvalonia UI variant** (branch `avalonia-fluent-ui`,
2026-11-12).

## FluentAvalonia variant

This branch is a parallel UI version of the completed editor:

- Uses `FluentAvaloniaUI` 3.1.0 with `FluentAvaloniaTheme` and accent `#0E8FA3`;
  the built-in `Avalonia.Themes.Fluent` package/theme is not used.
- Uses `FAAppWindow` for the managed title bar and `FAInfoBar` for the warning
  card on the Other tab.
- Keeps the fixed 960×800 window (no resize/maximize) and the same compact
  280px roster, form spacing and bottom action bar as the `avalonia` branch.
- Uses a custom numeric editor: two near-square Fluent-style buttons (decrease
  on the left, increase on the right) around a centered text field. Increment
  and decrement are ViewModel commands, with range clamping on the shared
  `NumericFieldViewModel`.
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

- left roster with avatars/names/classes and selection
- compact status form modeled on the original Qt layout: identity/faction/army,
  flags, level/EXP/HP, status effects, coordinates, MTYPE/SEQ, battle/victory
- numeric fields use a custom compact editor: near-square Fluent-style
  decrease/increase buttons around a centered value; enum fields show
  portraits/items where available
- tri-state action checkbox: unchecked = not acted, indeterminate = 10 ms
  "lock not acted" polling, checked = acted
- ability tab: base stats, growth rates, weapon proficiency icons/values
- skill tab: two scrollable skill/feature lists with localized names and icons
- item tab: eight item/durability/equipped/dropped rows with item icons
- support tab: current affinity, seven support partners, support points and
  C/B/A progress bars driven by the original thresholds
- other tab: global money/bonus EXP plus the original modification notes
- bottom status bar order: refresh (accent), reconnect, theme (neutral),
  language; all four buttons have fixed 96px width
- action status has its own label; the rescue/rescued checkboxes share a compact
  horizontal group
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
