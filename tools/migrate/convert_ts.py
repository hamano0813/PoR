#!/usr/bin/env python3
"""Convert Qt .ts translations into flat JSON dictionaries for Avalonia.

Outputs:
    dotnet/src/PoR.App/Localization/terms.zh.json
    dotnet/src/PoR.App/Localization/terms.jp.json

The JSON shape is ``{ "PID_IKE": "艾克", ... }``. Missing/empty translations are
omitted; the runtime LocalizationService falls back to the source key.
"""

from __future__ import annotations

import json
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
QM_DIR = ROOT / "resource" / "QM"
OUT_DIR = ROOT / "dotnet" / "src" / "PoR.App" / "Localization"

FILES = {
    "zh": QM_DIR / "zh.ts",
    "jp": QM_DIR / "jp.ts",
}


def text_of(node: ET.Element | None) -> str:
    if node is None:
        return ""
    return "".join(node.itertext()).strip()


def convert(path: Path) -> tuple[dict[str, str], dict[str, int]]:
    root = ET.parse(path).getroot()
    result: dict[str, str] = {}
    stats = {
        "messages": 0,
        "translated": 0,
        "empty": 0,
        "unfinished": 0,
        "duplicates": 0,
    }
    seen: set[str] = set()

    for message in root.iter("message"):
        source_node = message.find("source")
        translation_node = message.find("translation")
        source = text_of(source_node)
        translation = text_of(translation_node)
        if not source:
            continue

        stats["messages"] += 1
        if translation_node is not None and translation_node.get("type") == "unfinished":
            stats["unfinished"] += 1

        if not translation:
            stats["empty"] += 1
            continue

        if source in seen:
            stats["duplicates"] += 1
        seen.add(source)
        result[source] = translation
        stats["translated"] += 1

    result = dict(sorted(result.items()))
    return result, stats


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for language, path in FILES.items():
        if not path.exists():
            raise SystemExit(f"missing translation source: {path}")
        result, stats = convert(path)
        output = OUT_DIR / f"terms.{language}.json"
        output.write_text(
            json.dumps(result, ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8",
        )
        print(f"wrote {output} ({len(result)} terms)")
        print(
            f"  messages={stats['messages']} translated={stats['translated']} "
            f"empty={stats['empty']} unfinished={stats['unfinished']} "
            f"duplicates={stats['duplicates']}"
        )


if __name__ == "__main__":
    main()
