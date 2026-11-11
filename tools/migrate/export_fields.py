#!/usr/bin/env python3
"""Export DataSetting fields from the Python source into C#-friendly JSON.

This script is intentionally dependency-free (stdlib only) so it can run with
any Python 3.10+. It parses the source with ``ast`` and never imports PySide6
or dolphin-memory-engine.

Output:
    dotnet/src/PoR.Game/Data/fields.json
"""

from __future__ import annotations

import ast
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "src" / "parameter" / "data_setting.py"
OUTPUT = ROOT / "dotnet" / "src" / "PoR.Game" / "Data" / "fields.json"

CORE_IDS: dict[str, str] = {
    "模型": "model",
    "TEMP1": "temp1",
    "TEMP2": "temp2",
    "人物": "person",
    "职业": "class",
    "阵营": "faction",
    "同行": "ally",
    "序号": "slotIndex",
    "部队": "army",
    "等级": "level",
    "经验": "exp",
    "化身": "transform",
    "TEMP3": "temp3",
    "坐标Ｘ": "coordX",
    "坐标Ｙ": "coordY",
    "TEMP4": "temp4",
    "首领": "isLeader",
    "TEMP5": "temp5",
    "行动": "action",
    "救出": "rescuing",
    "被救": "rescued",
    "TEMP6": "temp6",
    "麻痹": "paralysis",
    "TEMP7": "temp7",
    "沉默": "silence",
    "睡眠": "sleep",
    "狂暴": "berserk",
    "中毒": "poison",
    "TEMP8": "temp8",
    "ＨＰ": "hp",
    "S_CON": "stat.con",
    "S_MOV": "stat.mov",
    "S_HP": "stat.hp",
    "S_STR": "stat.str",
    "S_MAG": "stat.mag",
    "S_SKL": "stat.skl",
    "S_SPD": "stat.spd",
    "S_LUK": "stat.luk",
    "S_DEF": "stat.def",
    "S_MDF": "stat.mdf",
    "TEMP9": "temp9",
    "E_HP": "growth.hp",
    "E_STR": "growth.str",
    "E_MAG": "growth.mag",
    "E_SKL": "growth.skl",
    "E_SPD": "growth.spd",
    "E_LUK": "growth.luk",
    "E_DEF": "growth.def",
    "E_MDF": "growth.mdf",
    "剑": "weapon.sword",
    "枪": "weapon.lance",
    "斧": "weapon.axe",
    "弓": "weapon.bow",
    "炎": "weapon.fire",
    "雷": "weapon.thunder",
    "风": "weapon.wind",
    "杖": "weapon.staff",
    "SEQ1": "seq1",
    "SEQ2": "seq2",
    "SEQ3": "seq3",
    "MTYPE": "mtype",
    "所持金": "money",
    "奖励EX": "bonusExp",
}

ENUM_FIELDS: dict[str, str] = {
    "person": "pid",
    "class": "jid",
    "model": "aid",
    "faction": "group",
    "army": "army",
    "ally": "ally",
    "mtype": "mtype",
    "seq1": "seq",
    "seq2": "seq",
    "seq3": "seq",
}

BOOL_FIELDS = {
    "isLeader",
    "rescuing",
    "rescued",
}

TRISTATE_FIELDS = {"action"}

REFRESH_AFTER_WRITE = {
    "person",
    "class",
    "model",
}


def find_class(tree: ast.Module, name: str) -> ast.ClassDef:
    for node in tree.body:
        if isinstance(node, ast.ClassDef) and node.name == name:
            return node
    raise SystemExit(f"class {name!r} not found in {SOURCE}")


def class_assignment(cls: ast.ClassDef, name: str) -> object:
    for node in cls.body:
        if isinstance(node, ast.Assign):
            for target in node.targets:
                if isinstance(target, ast.Name) and target.id == name:
                    return ast.literal_eval(node.value)
    raise SystemExit(f"class attribute {name!r} not found")


def setting_dict(cls: ast.ClassDef) -> dict[str, tuple]:
    for node in cls.body:
        if isinstance(node, ast.FunctionDef) and node.name == "__init__":
            for stmt in node.body:
                if not isinstance(stmt, ast.Assign):
                    continue
                for target in stmt.targets:
                    if (
                        isinstance(target, ast.Attribute)
                        and isinstance(target.value, ast.Name)
                        and target.value.id == "self"
                        and target.attr == "setting"
                    ):
                        value = ast.literal_eval(stmt.value)
                        if not isinstance(value, dict):
                            raise SystemExit("DataSetting.setting is not a dict")
                        return value
    raise SystemExit("DataSetting.setting assignment not found")


def make_field_id(legacy_key: str, index: int) -> str:
    if legacy_key in CORE_IDS:
        return CORE_IDS[legacy_key]

    patterns = (
        (r"道具(\d+)", "item"),
        (r"耐久(\d+)", "durability"),
        (r"装备(\d+)", "equipped"),
        (r"掉落(\d+)", "dropped"),
        (r"支援(\d+)", "support"),
        (r"TEMP(\d+)", "temp"),
    )
    for pattern, prefix in patterns:
        match = re.fullmatch(pattern, legacy_key)
        if match:
            return f"{prefix}{int(match.group(1))}"

    if legacy_key.startswith("SID_"):
        return legacy_key

    safe = re.sub(r"[^0-9A-Za-z_]+", "_", legacy_key).strip("_")
    return safe or f"field_{index}"


def classify(field_id: str, legacy_key: str) -> tuple[str, str | None, bool, bool]:
    """Return (kind, enum_id, read_only, refresh_after_write)."""

    if field_id in TRISTATE_FIELDS or legacy_key in TRISTATE_FIELDS:
        return "tristate", None, False, False

    if field_id in ENUM_FIELDS:
        return "enum", ENUM_FIELDS[field_id], False, field_id in REFRESH_AFTER_WRITE

    if field_id in BOOL_FIELDS:
        return "bool", None, False, False

    if legacy_key.startswith("SID_"):
        return "bool", None, False, False

    if re.fullmatch(r"装备\d+", legacy_key) or re.fullmatch(r"掉落\d+", legacy_key):
        return "bool", None, False, False

    if re.fullmatch(r"道具\d+", legacy_key):
        return "enum", "iid", False, True

    if legacy_key == "序号":
        return "numeric", None, True, False

    return "numeric", None, False, False


def bit_json(raw: object) -> dict[str, int] | None:
    if raw is None:
        return None
    if isinstance(raw, int):
        return {"start": raw, "end": raw + 1}
    if isinstance(raw, tuple) and len(raw) == 2:
        start, end = raw
        return {"start": int(start), "end": int(end)}
    raise SystemExit(f"unsupported bit definition: {raw!r}")


def main() -> None:
    tree = ast.parse(SOURCE.read_text(encoding="utf-8"), filename=str(SOURCE))
    cls = find_class(tree, "DataSetting")

    slot_base = int(class_assignment(cls, "SLOT"))
    slot_step = int(class_assignment(cls, "STEP"))
    slot_count = int(class_assignment(cls, "COUNT"))
    slot_end = slot_base + slot_step * slot_count

    settings = setting_dict(cls)
    fields: list[dict] = []
    seen: set[str] = set()

    for index, (legacy_key, raw) in enumerate(settings.items()):
        if not isinstance(raw, tuple) or len(raw) not in (2, 3):
            raise SystemExit(f"unexpected field definition for {legacy_key!r}: {raw!r}")

        address = int(raw[0])
        length = int(raw[1])
        bit = bit_json(raw[2] if len(raw) == 3 else None)

        field_id = make_field_id(legacy_key, index)
        if field_id in seen:
            field_id = f"{field_id}_{index}"
        seen.add(field_id)

        kind, enum_id, read_only, refresh_after_write = classify(field_id, legacy_key)
        scope = "character" if slot_base <= address < slot_end else "global"

        fields.append(
            {
                "id": field_id,
                "legacyKey": legacy_key,
                "address": f"0x{address:08X}",
                "length": length,
                "bit": bit,
                "scope": scope,
                "kind": kind,
                "enumId": enum_id,
                "readOnly": read_only,
                "refreshAfterWrite": refresh_after_write,
                "displayKey": f"Ui.Field.{field_id}",
            }
        )

    document = {
        "profile": {
            "slotBase": f"0x{slot_base:08X}",
            "slotStep": slot_step,
            "slotCount": slot_count,
            "fieldCount": len(fields),
        },
        "fields": fields,
    }

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(
        json.dumps(document, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(f"wrote {OUTPUT} ({len(fields)} fields)")
    print(
        f"slot base=0x{slot_base:08X} step=0x{slot_step:X} "
        f"count={slot_count} end=0x{slot_end:08X}"
    )


if __name__ == "__main__":
    main()
