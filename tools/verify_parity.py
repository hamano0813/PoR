#!/usr/bin/env python3
"""Run the C# probe and the legacy Python probe, then compare their output.

Prerequisite: build ``dotnet/PoR.sln`` first and create ``.venv`` as described
in ``tools/legacy/README.md``. Dolphin must be running with the game booted.
"""

from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "dotnet" / "tools" / "PoR.Tools.Probe" / "PoR.Tools.Probe.csproj"
PYTHON = (
    ROOT / ".venv" / "Scripts" / "python.exe"
    if (ROOT / ".venv" / "Scripts" / "python.exe").exists()
    else ROOT / ".venv" / "bin" / "python"
)
PYTHON_PROBE = ROOT / "tools" / "legacy" / "probe_python.py"

COMPARE_KEYS = [
    "status",
    "person",
    "classId",
    "level",
    "money",
    "visibleSlots",
    "sample",
]


def run(command: list[str]) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        command,
        cwd=ROOT,
        text=True,
        encoding="utf-8",
        capture_output=True,
        check=False,
    )


def parse_json(label: str, process: subprocess.CompletedProcess[str]) -> dict:
    try:
        return json.loads(process.stdout)
    except json.JSONDecodeError as exception:
        print(f"{label}: could not parse JSON ({exception})", file=sys.stderr)
        print(f"{label} stdout:\n{process.stdout}", file=sys.stderr)
        print(f"{label} stderr:\n{process.stderr}", file=sys.stderr)
        raise SystemExit(2) from exception


def main() -> int:
    if not PYTHON.exists():
        print(
            "Legacy venv not found. Run:\n"
            "  uv venv .venv --python 3.13\n"
            '  uv pip install --python .venv/Scripts/python.exe "dolphin-memory-engine==1.3.0"',
            file=sys.stderr,
        )
        return 2

    csharp_process = run(
        [
            "dotnet",
            "run",
            "--project",
            str(PROJECT),
            "--no-build",
            "--",
            "--json",
        ]
    )
    python_process = run([str(PYTHON), "-I", str(PYTHON_PROBE)])

    csharp = parse_json("C# probe", csharp_process)
    python = parse_json("Python probe", python_process)

    print("C#:    ", json.dumps(csharp, ensure_ascii=False))
    print("Python:", json.dumps(python, ensure_ascii=False))

    differences = []
    for key in COMPARE_KEYS:
        if csharp.get(key) != python.get(key):
            differences.append(
                f"{key}: C#={csharp.get(key)!r} Python={python.get(key)!r}"
            )

    if differences:
        print("\nPARITY FAILED", file=sys.stderr)
        for difference in differences:
            print(f"  {difference}", file=sys.stderr)
        return 1

    print("\nPARITY OK")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
