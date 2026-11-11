#!/usr/bin/env python3
"""Print the status-tab fields of one character slot using the legacy DME.

This is a diagnostic/parity tool, not part of the C# runtime.
Usage:
    .venv/Scripts/python.exe -I tools/legacy/print_status.py [--slot N]
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

from dolphin_memory_engine import hook, is_hooked, read_bytes, read_word

ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = ROOT / "dotnet" / "src" / "PoR.Game" / "Data"
LOCALIZATION_DIR = ROOT / "dotnet" / "src" / "PoR.App" / "Localization"

SLOT_BASE = 0x802AF5E4
SLOT_STEP = 0x280
SLOT_COUNT = 0x9F
PERSON_ADDRESS = 0x802AF76C

STATUS_FIELDS = [
    "person",
    "class",
    "model",
    "faction",
    "army",
    "ally",
    "isLeader",
    "action",
    "rescuing",
    "rescued",
    "slotIndex",
    "level",
    "exp",
    "hp",
    "transform",
    "paralysis",
    "silence",
    "sleep",
    "berserk",
    "poison",
    "coordX",
    "coordY",
    "mtype",
    "seq1",
    "seq2",
    "seq3",
    "battle",
    "victory",
]


def load_json(path: Path) -> object:
    return json.loads(path.read_text(encoding="utf-8"))


FIELDS = {
    field["id"]: field
    for field in load_json(DATA_DIR / "fields.json")["fields"]
}
ENUMS = load_json(DATA_DIR / "enums.json")
MAPPINGS = load_json(DATA_DIR / "mappings.json")
GROUPS = {
    int(entry["value"], 16): entry["term"]
    for entry in load_json(DATA_DIR / "group.json")
}
ARMIES = {
    int(entry["value"]): entry["term"]
    for entry in load_json(DATA_DIR / "army.json")
}
TERMS = load_json(LOCALIZATION_DIR / "terms.zh.json")


def term(term_key: str) -> str:
    return TERMS.get(term_key, term_key)


def read_cstring(address: int, depth: int = 0) -> str:
    if address == 0 or depth > 8:
        return ""
    word = read_word(address)
    if 0x80000000 < word < 0x81800000:
        return read_cstring(word, depth + 1)
    data = bytearray()
    for offset in range(512):
        byte = read_bytes(address + offset, 1)[0]
        if byte == 0:
            break
        data.append(byte)
    return data.decode("utf-8", errors="replace")


def read_raw(field: dict, slot_offset: int) -> int:
    address = int(field["address"], 16) + slot_offset
    length = int(field["length"])
    data = read_bytes(address, abs(length))
    return int.from_bytes(data, "big", signed=length < 0)


def read_field(field_id: str, slot_offset: int) -> int:
    field = FIELDS[field_id]
    value = read_raw(field, slot_offset)
    bit = field.get("bit")
    if bit is None:
        return value
    width = bit["end"] - bit["start"]
    return (value >> bit["start"]) & ((1 << width) - 1)


def enum_entry(enum_id: str, pointer: int) -> dict | None:
    if pointer == 0:
        return None
    text = read_cstring(pointer)
    for entry in ENUMS[enum_id]:
        if entry["key"] == text:
            return entry
    return None


def enum_label(enum_id: str, pointer: int) -> str:
    entry = enum_entry(enum_id, pointer)
    if entry is None:
        return "——" if pointer == 0 else f"0x{pointer:08X}"
    return f"{term(entry['term'])} [{entry['key']}]"


def text_mapping_label(mapping_id: str, value: int) -> str:
    if value == 0:
        return "——"
    mapping = MAPPINGS[mapping_id]
    base = int(mapping["baseAddress"], 16)
    step = int(mapping["step"])
    address_offset = int(mapping["addressOffset"])
    for index, entry in enumerate(ENUMS[mapping["enum"]]):
        candidate = read_word(base + step * index) + address_offset
        if candidate == value:
            return f"{term(entry['term'])} [{entry['key']}]"
    return f"0x{value:08X}"


def slot_person(index: int) -> int:
    return read_word(PERSON_ADDRESS + index * SLOT_STEP)


def slot_label(index: int) -> str:
    person = slot_person(index)
    if not person:
        return "——"
    return enum_label("pid", person)


def main() -> int:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")

    parser = argparse.ArgumentParser()
    parser.add_argument("--slot", type=int, default=0, help="slot index (default: 0)")
    args = parser.parse_args()

    if not 0 <= args.slot < SLOT_COUNT:
        print(f"slot must be 0..{SLOT_COUNT - 1}", file=sys.stderr)
        return 2

    hook()
    if not is_hooked():
        print("Dolphin is not hooked; start the game first.", file=sys.stderr)
        return 1

    slot_offset = args.slot * SLOT_STEP
    person_pointer = read_field("person", slot_offset)
    if person_pointer == 0:
        print(f"槽位 #{args.slot} 为空。")
        return 0

    values = {
        field_id: read_field(field_id, slot_offset)
        for field_id in STATUS_FIELDS
    }

    ally_pointer = values["ally"]
    if ally_pointer:
        ally_index = (ally_pointer - SLOT_BASE) // SLOT_STEP
        ally_text = (
            slot_label(ally_index)
            if 0 <= ally_index < SLOT_COUNT
            else f"0x{ally_pointer:08X}"
        )
    else:
        ally_text = "——"

    faction = GROUPS.get(values["faction"], f"0x{values['faction']:08X}")
    army = ARMIES.get(values["army"], f"0x{values['army']:02X}")

    print(f"槽位 #{args.slot}  人物状态")
    print("-" * 48)
    print(f"人物      {enum_label('pid', values['person'])}")
    print(f"职业      {enum_label('jid', values['class'])}")
    print(f"模型      {enum_label('aid', values['model'])}")
    print(f"阵营      {term(faction)}")
    print(f"部队      {term(army)}")
    print(f"同行      {ally_text}")
    print(f"首领      {'是' if values['isLeader'] else '否'}")
    print(
        "行动      "
        + ("行动完毕" if values["action"] else "未行动")
        + "（内存值；锁定状态由修改器维护）"
    )
    print(f"救出/被救 {values['rescuing']} / {values['rescued']}")
    print(f"序号      {values['slotIndex']}")
    print(f"等级      {values['level']}")
    print(f"经验      {values['exp']}")
    print(f"HP        {values['hp']}")
    print(f"化身      {values['transform']}")
    print(
        "状态      "
        f"中毒={values['poison']} 睡眠={values['sleep']} "
        f"沉默={values['silence']} 狂暴={values['berserk']} "
        f"麻痹={values['paralysis']}"
    )
    print(f"坐标      ({values['coordX']}, {values['coordY']})")
    print(f"模式      {text_mapping_label('mtype', values['mtype'])}")
    print(f"攻击      {text_mapping_label('seq', values['seq1'])}")
    print(f"目标      {text_mapping_label('seq', values['seq2'])}")
    print(f"回复      {text_mapping_label('seq', values['seq3'])}")
    print(f"战斗/胜利 {values['battle']} / {values['victory']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
