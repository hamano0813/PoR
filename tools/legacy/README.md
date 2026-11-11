# Legacy Python Probe

This directory keeps the original Python/DME probe used to verify that the C#
memory layer reads exactly the same values.

## Setup

```bash
uv venv .venv --python 3.13
uv pip install --python .venv/Scripts/python.exe "dolphin-memory-engine==1.3.0"
```

`.venv/` is ignored by git.

## Run

Start Dolphin and boot Path of Radiance, then:

```bash
.venv/Scripts/python.exe -I tools/legacy/probe_python.py
```

The script prints JSON with:

- `person`, `classId`, `level`, `money`
- `visibleSlots` and the first 10 slot samples

## Print character status

```bash
# default slot 0
.venv/Scripts/python.exe -I tools/legacy/print_status.py

# explicit slot
.venv/Scripts/python.exe -I tools/legacy/print_status.py --slot 8
```

It prints the status-tab fields (identity, faction/army, level/EXP/HP,
status effects, coordinates, MTYPE/SEQ, battle/victory). Action-lock state is
maintained by the modifier, not stored in game memory, so the script reports
only the raw action value.

## Parity check

Build the C# solution once, then run:

```bash
dotnet build dotnet/PoR.sln
.venv/Scripts/python.exe -I tools/verify_parity.py
```

The verifier runs both probes and compares the JSON fields. Exit code 0 means
the C# port matches the legacy Python implementation.
