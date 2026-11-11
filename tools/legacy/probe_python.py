#!/usr/bin/env python3
"""Legacy Python/DME probe used to verify the C# memory layer.

Reads the same fields as dotnet/tools/PoR.Tools.Probe and prints JSON.
Requires the legacy virtual environment with dolphin-memory-engine==1.3.0.
"""

from __future__ import annotations

import json
import sys

from dolphin_memory_engine import hook, is_hooked, read_byte, read_bytes, read_word

PERSON = 0x802AF76C
CLASS = 0x802AF770
LEVEL = 0x802AF77E
MONEY = 0x8032E7A1

SLOT = 0x802AF5E4
STEP = 0x280
COUNT = 0x9F


def main() -> int:
    hook()
    if not is_hooked():
        print(json.dumps({"status": "Unhooked"}, indent=2))
        return 1

    try:
        person = read_word(PERSON)
        class_id = read_word(CLASS)
        level = read_byte(LEVEL)
        money = int.from_bytes(read_bytes(MONEY, 3), "big")

        snapshot = read_bytes(SLOT, COUNT * STEP)
        person_offset = PERSON - SLOT
        class_offset = CLASS - SLOT

        visible = 0
        sample = []
        for index in range(COUNT):
            base = index * STEP
            slot_person = int.from_bytes(
                snapshot[base + person_offset:base + person_offset + 4], "big"
            )
            slot_class = int.from_bytes(
                snapshot[base + class_offset:base + class_offset + 4], "big"
            )
            if not slot_person:
                continue
            visible += 1
            if len(sample) < 10:
                sample.append(
                    {"Index": index, "Person": slot_person, "ClassId": slot_class}
                )

        print(
            json.dumps(
                {
                    "status": "Hooked",
                    "person": f"0x{person:08X}",
                    "classId": f"0x{class_id:08X}",
                    "level": level,
                    "money": money,
                    "visibleSlots": visible,
                    "sample": sample,
                },
                indent=2,
                ensure_ascii=False,
            )
        )
        return 0
    except Exception as exception:  # noqa: BLE001 - probe is a diagnostic tool
        print(
            json.dumps(
                {"status": "Error", "error": f"{type(exception).__name__}: {exception}"},
                indent=2,
            )
        )
        return 2


if __name__ == "__main__":
    sys.exit(main())
