#!/usr/bin/env python3
"""Export EnumData game catalog data from Python source into JSON.

The script parses ``src/parameter/enum_data.py`` with ``ast`` and evaluates only
literal containers plus ``self.tr("KEY")`` calls. It does not import PySide6 or
touch Dolphin memory.

Outputs (under dotnet/src/PoR.Game/Data):
    enums.json
    skills.json
    support.json
    affinity.json
    group.json
    army.json
    mappings.json
"""

from __future__ import annotations

import ast
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "src" / "parameter" / "enum_data.py"
DATA_DIR = ROOT / "dotnet" / "src" / "PoR.Game" / "Data"

ENUM_ATTRS = {
    "aid": "AID_ENUM",
    "pid": "PID_ENUM",
    "jid": "JID_ENUM",
    "iid": "IID_ENUM",
    "sid": "SID_ENUM",
    "seq": "SEQ_ENUM",
    "mtype": "MTYPE_ENUM",
}

SETTING_ATTRS = {
    "aid": "AID_SETTING",
    "pid": "PID_SETTING",
    "jid": "JID_SETTING",
    "iid": "IID_SETTING",
    "seq": "SEQ_SETTING",
    "mtype": "MTYPE_SETTING",
}


def find_class(tree: ast.Module, name: str) -> ast.ClassDef:
    for node in tree.body:
        if isinstance(node, ast.ClassDef) and node.name == name:
            return node
    raise SystemExit(f"class {name!r} not found in {SOURCE}")


def eval_node(node: ast.AST) -> object:
    """Evaluate the small subset of expressions used by enum_data.py."""

    if isinstance(node, ast.Constant):
        return node.value

    if isinstance(node, (ast.Tuple, ast.List)):
        values = [eval_node(item) for item in node.elts]
        return tuple(values) if isinstance(node, ast.Tuple) else values

    if isinstance(node, ast.Dict):
        result = {}
        for key, value in zip(node.keys, node.values):
            if key is None:  # ``**mapping``; not used in this file
                raise SystemExit("dict unpacking is not supported")
            result[eval_node(key)] = eval_node(value)
        return result

    if isinstance(node, ast.UnaryOp) and isinstance(node.op, ast.USub):
        return -eval_node(node.operand)

    if isinstance(node, ast.Name):
        if node.id == "None":
            return None
        if node.id == "True":
            return True
        if node.id == "False":
            return False
        raise SystemExit(f"unsupported name: {node.id}")

    if isinstance(node, ast.Call):
        func = node.func
        if isinstance(func, ast.Attribute) and func.attr == "tr":
            if len(node.args) != 1:
                raise SystemExit("self.tr() must have exactly one argument")
            return eval_node(node.args[0])
        raise SystemExit(
            f"unsupported call at line {getattr(node, 'lineno', '?')}: "
            f"{ast.unparse(node)}"
        )

    raise SystemExit(
        f"unsupported expression at line {getattr(node, 'lineno', '?')}: "
        f"{ast.unparse(node)}"
    )


def class_assignment(cls: ast.ClassDef, name: str) -> object:
    for node in cls.body:
        if isinstance(node, ast.Assign):
            for target in node.targets:
                if isinstance(target, ast.Name) and target.id == name:
                    return eval_node(node.value)
    raise SystemExit(f"class attribute {name!r} not found")


def self_assignment(cls: ast.ClassDef, attr: str) -> object:
    for node in ast.walk(cls):
        if isinstance(node, ast.Assign):
            for target in node.targets:
                if (
                    isinstance(target, ast.Attribute)
                    and isinstance(target.value, ast.Name)
                    and target.value.id == "self"
                    and target.attr == attr
                ):
                    return eval_node(node.value)
    raise SystemExit(f"self.{attr} assignment not found")


def function_return(cls: ast.ClassDef, name: str) -> ast.AST:
    for node in cls.body:
        if isinstance(node, ast.FunctionDef) and node.name == name:
            for stmt in node.body:
                if isinstance(stmt, ast.Return):
                    if stmt.value is None:
                        raise SystemExit(f"{name}() returns None")
                    return stmt.value
    raise SystemExit(f"function {name!r} not found")


def hex32(value: int) -> str:
    return f"0x{value:08X}"


def enum_list(mapping: dict) -> list[dict]:
    """Convert an ordered enum dict into [{key, term}]."""
    result = []
    for key, term in mapping.items():
        result.append({"key": str(key), "term": str(term)})
    return result


def write_json(path: Path, value: object) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        json.dumps(value, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(f"wrote {path}")


def main() -> None:
    tree = ast.parse(SOURCE.read_text(encoding="utf-8"), filename=str(SOURCE))
    cls = find_class(tree, "EnumData")

    enums: dict[str, list[dict]] = {}
    enum_maps: dict[str, dict] = {}
    for enum_id, attr in ENUM_ATTRS.items():
        mapping = self_assignment(cls, attr)
        if not isinstance(mapping, dict):
            raise SystemExit(f"{attr} is not a dict")
        enum_maps[enum_id] = mapping
        enums[enum_id] = enum_list(mapping)

    skills = {
        "skills": list(class_assignment(cls, "SKILL_ENUM")),
        "masteries": list(class_assignment(cls, "MASTERY_ENUM")),
        "features": list(class_assignment(cls, "FEATURE_ENUM")),
    }

    support_raw = self_assignment(cls, "SUPPORT_ENUM")
    if not isinstance(support_raw, dict):
        raise SystemExit("SUPPORT_ENUM is not a dict")

    support = []
    for pid, entries in support_raw.items():
        if not isinstance(entries, tuple):
            raise SystemExit(f"SUPPORT_ENUM[{pid!r}] is not a tuple")
        row = []
        for entry in entries:
            if entry is None:
                row.append(None)
                continue
            if not isinstance(entry, tuple) or len(entry) != 2:
                raise SystemExit(f"unexpected support entry for {pid!r}: {entry!r}")
            partner, thresholds = entry
            if partner is None:
                row.append(None)
                continue
            row.append(
                {
                    "partner": str(partner),
                    "thresholds": [int(value) for value in thresholds],
                }
            )
        support.append({"pid": str(pid), "entries": row})

    affinity_raw = self_assignment(cls, "AFF_ENUM")
    if not isinstance(affinity_raw, dict):
        raise SystemExit("AFF_ENUM is not a dict")
    affinity = [
        {"pid": str(pid), "affinity": str(aff)}
        for pid, aff in affinity_raw.items()
    ]

    group_raw = eval_node(function_return(cls, "GROUP_MAPPING"))
    if not isinstance(group_raw, dict):
        raise SystemExit("GROUP_MAPPING did not return a dict")
    group = [
        {"value": hex32(int(address)), "term": str(term)}
        for address, term in group_raw.items()
    ]

    army_raw = eval_node(function_return(cls, "ARMY_MAPPING"))
    if not isinstance(army_raw, dict):
        raise SystemExit("ARMY_MAPPING did not return a dict")
    army = [
        {"value": int(value), "term": str(term)}
        for value, term in army_raw.items()
    ]

    settings: dict[str, tuple] = {}
    for mapping_id, attr in SETTING_ATTRS.items():
        setting = class_assignment(cls, attr)
        if not isinstance(setting, tuple) or len(setting) != 2:
            raise SystemExit(f"{attr} has unexpected value: {setting!r}")
        settings[mapping_id] = setting

    mappings: dict[str, dict] = {}
    for mapping_id in ("aid", "pid", "jid", "iid"):
        pointer_address, step = settings[mapping_id]
        mappings[mapping_id] = {
            "type": "pointer",
            "pointerAddress": hex32(int(pointer_address)),
            "step": int(step),
            "enum": mapping_id,
        }

    for mapping_id, function_name in (("seq", "SEQ_MAPPING"), ("mtype", "MTYPE_MAPPING")):
        call = function_return(cls, function_name)
        if not isinstance(call, ast.Call):
            raise SystemExit(f"{function_name} does not return a call")
        if len(call.args) != 3:
            raise SystemExit(f"{function_name} call has unexpected args")
        base_address, step = settings[mapping_id]
        base_address = int(base_address)
        step = int(step)
        address_offset = int(eval_node(call.args[2]))
        mappings[mapping_id] = {
            "type": "text",
            "baseAddress": hex32(base_address),
            "step": step,
            "addressOffset": address_offset,
            "enum": mapping_id,
        }

    write_json(DATA_DIR / "enums.json", enums)
    write_json(DATA_DIR / "skills.json", skills)
    write_json(DATA_DIR / "support.json", support)
    write_json(DATA_DIR / "affinity.json", affinity)
    write_json(DATA_DIR / "group.json", group)
    write_json(DATA_DIR / "army.json", army)
    write_json(DATA_DIR / "mappings.json", mappings)

    print(
        "counts: "
        + ", ".join(f"{name}={len(items)}" for name, items in enums.items())
    )
    print(
        f"support={len(support)}, affinity={len(affinity)}, "
        f"group={len(group)}, army={len(army)}"
    )
    print(
        f"skills={len(skills['skills'])}, masteries={len(skills['masteries'])}, "
        f"features={len(skills['features'])}"
    )


if __name__ == "__main__":
    main()
