# Data Migration Tools

These scripts export the original Python game data into JSON consumed by the
C# / Avalonia application. They are stdlib-only and never import PySide6 or
`dolphin_memory_engine`; the source is parsed with `ast`.

## Requirements

- Python 3.10+ (3.14 works)
- Run from any directory; paths are resolved relative to the repository root.

On this machine `python` is not on PATH. Use the uv-managed interpreter:

```bash
uv run --no-project --python 3.14 tools/migrate/export_fields.py
uv run --no-project --python 3.14 tools/migrate/export_enums.py
uv run --no-project --python 3.14 tools/migrate/convert_ts.py
uv run --no-project --python 3.14 tools/migrate/validate_assets.py
```

Alternatively, use the interpreter directly:

```bash
"$USERPROFILE/.local/bin/python3.14.exe" tools/migrate/export_fields.py
```

## Scripts

| Script | Output |
|---|---|
| `export_fields.py` | `dotnet/src/PoR.Game/Data/fields.json` |
| `export_enums.py` | `enums.json`, `skills.json`, `support.json`, `affinity.json`, `group.json`, `army.json`, `mappings.json` |
| `convert_ts.py` | `dotnet/src/PoR.App/Localization/terms.{zh,jp}.json` |
| `validate_assets.py` | `docs/data-report.md`; exits non-zero on structural errors |

## Notes

- Generated JSON is committed so the C# side can start without Python.
- Until the C# `GameDataCatalog` becomes the source of truth, re-run these
  scripts after changing `src/parameter/*.py` or `resource/QM/*.ts`.
- `fields.json` keeps `legacyKey` for comparison with the Python version.
- `terms.*.json` contains only non-empty translations. AID (508), SEQ (75), and
  MTYPE (33) have no translations in the original `.ts`; the app falls back to
  the source key, matching the original behavior.
- Assets are not copied by these scripts. The C# app will reference the files
  under `resource/PID`, `resource/IID`, `resource/SID`, `resource/AFF`, and
  `resource/WP`.
