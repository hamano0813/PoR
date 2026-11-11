#!/usr/bin/env python3
"""Validate the generated C# game data against source assets and layouts.

Checks:
    - field IDs/addresses/bits/lengths
    - enum key uniqueness
    - PID/IID icons and skill/mastery SID icons
    - affinity and weapon icons
    - support partner references and threshold ordering
    - mapping definitions
    - translation coverage (reported, not fatal)

Writes docs/data-report.md and returns non-zero on structural errors.
"""

from __future__ import annotations

import json
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = ROOT / "dotnet" / "src" / "PoR.Game" / "Data"
LOCALIZATION_DIR = ROOT / "dotnet" / "src" / "PoR.App" / "Localization"
RESOURCE_DIR = ROOT / "resource"
REPORT = ROOT / "docs" / "data-report.md"

WEAPON_KEYS = ["AXE", "BOW", "FIRE", "LANCE", "LIGHT", "STAFF", "SWORD", "THUNDER", "WIND"]
ENUM_IDS = {"aid", "pid", "jid", "iid", "sid", "seq", "mtype"}
SPECIAL_ENUM_IDS = {"group", "army", "ally"}


def load(name: str) -> object:
    path = DATA_DIR / name
    if not path.exists():
        raise SystemExit(f"missing generated data: {path}")
    return json.loads(path.read_text(encoding="utf-8"))


def parse_hex(value: str) -> int:
    return int(value, 16) if isinstance(value, str) else int(value)


def main() -> int:
    fields_doc = load("fields.json")
    enums = load("enums.json")
    skills = load("skills.json")
    support = load("support.json")
    affinity = load("affinity.json")
    group = load("group.json")
    army = load("army.json")
    mappings = load("mappings.json")

    profile = fields_doc["profile"]
    fields = fields_doc["fields"]
    slot_base = parse_hex(profile["slotBase"])
    slot_step = int(profile["slotStep"])
    slot_count = int(profile["slotCount"])
    slot_end = slot_base + slot_step * slot_count

    errors: list[str] = []
    warnings: list[str] = []

    # ---- fields ---------------------------------------------------------
    ids: set[str] = set()
    field_kinds: Counter[str] = Counter()
    field_scopes: Counter[str] = Counter()
    enum_ids: set[str] = set(enums) | SPECIAL_ENUM_IDS

    for field in fields:
        fid = field["id"]
        legacy = field["legacyKey"]
        address = parse_hex(field["address"])
        length = int(field["length"])
        kind = field["kind"]
        scope = field["scope"]
        field_kinds[kind] += 1
        field_scopes[scope] += 1

        if fid in ids:
            errors.append(f"duplicate field id: {fid}")
        ids.add(fid)

        if length == 0:
            errors.append(f"{fid}: length must not be zero")

        bit = field.get("bit")
        if bit is not None:
            start, end = int(bit["start"]), int(bit["end"])
            if start < 0 or end <= start or end > abs(length) * 8:
                errors.append(f"{fid}: invalid bit range [{start}, {end}) for length {length}")

        if scope == "character":
            offset = address - slot_base
            if offset < 0 or offset + abs(length) > slot_step:
                errors.append(
                    f"{fid}: character field offset 0x{offset:X} + {abs(length)} "
                    f"exceeds slot step 0x{slot_step:X}"
                )
        elif scope == "global":
            if slot_base <= address < slot_end:
                errors.append(f"{fid}: global field address is inside the slot array")
        else:
            errors.append(f"{fid}: unknown scope {scope!r}")

        enum_id = field.get("enumId")
        if kind == "enum":
            if enum_id not in enum_ids:
                errors.append(f"{fid}: unknown enumId {enum_id!r}")
        elif enum_id is not None:
            errors.append(f"{fid}: non-enum field has enumId {enum_id!r}")

        if kind not in {"numeric", "enum", "bool", "tristate"}:
            errors.append(f"{fid}: unknown kind {kind!r}")

    if len(fields) != int(profile["fieldCount"]):
        errors.append("profile.fieldCount does not match fields length")

    # ---- enum keys ------------------------------------------------------
    enum_sets: dict[str, set[str]] = {}
    for enum_id, entries in enums.items():
        keys = [entry["key"] for entry in entries]
        if len(keys) != len(set(keys)):
            errors.append(f"enum {enum_id}: duplicate keys")
        for entry in entries:
            if not entry.get("key") or not entry.get("term"):
                errors.append(f"enum {enum_id}: empty key/term")
        enum_sets[enum_id] = set(keys)

    pid_keys = enum_sets["pid"]
    iid_keys = enum_sets["iid"]
    sid_keys = enum_sets["sid"]

    # ---- icons ----------------------------------------------------------
    def missing_icons(kind: str, keys: list[str] | set[str], suffix: str) -> list[str]:
        directory = RESOURCE_DIR / kind
        return sorted(key for key in keys if not (directory / f"{key}{suffix}").exists())

    pid_missing = missing_icons("PID", pid_keys, ".gif")
    iid_missing = missing_icons("IID", iid_keys, ".gif")
    icon_skill_keys = set(skills["skills"]) | set(skills["masteries"])
    sid_missing = missing_icons("SID", icon_skill_keys, ".gif")

    if pid_missing:
        errors.append(f"PID icons missing ({len(pid_missing)}): {', '.join(pid_missing[:20])}")
    if iid_missing:
        errors.append(f"IID icons missing ({len(iid_missing)}): {', '.join(iid_missing[:20])}")
    if sid_missing:
        errors.append(f"SID icons missing ({len(sid_missing)}): {', '.join(sid_missing[:20])}")

    affinity_values = {entry["affinity"] for entry in affinity}
    aff_missing = sorted(
        value for value in affinity_values
        if not (RESOURCE_DIR / "AFF" / f"{value}.gif").exists()
    )
    if aff_missing:
        errors.append(f"AFF icons missing: {', '.join(aff_missing)}")

    wp_missing = sorted(
        key for key in WEAPON_KEYS
        if not (RESOURCE_DIR / "WP" / f"WP_{key}.png").exists()
    )
    if wp_missing:
        errors.append(f"WP icons missing: {', '.join(wp_missing)}")

    # ---- support / affinity ---------------------------------------------
    support_pids: set[str] = set()
    for row in support:
        pid = row["pid"]
        support_pids.add(pid)
        if pid not in pid_keys:
            errors.append(f"support: unknown PID {pid!r}")
        entries = row["entries"]
        if len(entries) != 7:
            warnings.append(f"support {pid}: expected 7 rows, got {len(entries)}")
        for index, entry in enumerate(entries):
            if entry is None:
                continue
            partner = entry["partner"]
            if partner not in pid_keys:
                errors.append(f"support {pid}[{index}]: unknown partner {partner!r}")
            thresholds = entry["thresholds"]
            if len(thresholds) != 3:
                errors.append(f"support {pid}[{index}]: thresholds must have 3 values")
            elif thresholds != sorted(thresholds):
                errors.append(f"support {pid}[{index}]: thresholds not ascending")

    for entry in affinity:
        if entry["pid"] not in pid_keys:
            errors.append(f"affinity: unknown PID {entry['pid']!r}")

    support_without_affinity = sorted(support_pids - {entry["pid"] for entry in affinity})
    if support_without_affinity:
        warnings.append(
            "support characters without affinity: " + ", ".join(support_without_affinity)
        )

    # ---- group / army ----------------------------------------------------
    for collection, name in ((group, "group"), (army, "army")):
        values = [entry["value"] for entry in collection]
        if len(values) != len(set(values)):
            errors.append(f"{name}: duplicate values")
        for entry in collection:
            if not entry.get("term"):
                errors.append(f"{name}: empty term for {entry!r}")

    # ---- mappings --------------------------------------------------------
    field_by_address = {parse_hex(field["address"]): field for field in fields}
    for mapping_id, mapping in mappings.items():
        enum_id = mapping.get("enum")
        if enum_id not in enum_sets:
            errors.append(f"mapping {mapping_id}: unknown enum {enum_id!r}")
        if mapping["type"] == "pointer":
            address = parse_hex(mapping["pointerAddress"])
            if address not in field_by_address:
                errors.append(
                    f"mapping {mapping_id}: pointer address 0x{address:08X} "
                    "does not match a field"
                )
            if int(mapping["step"]) <= 0:
                errors.append(f"mapping {mapping_id}: step must be positive")
        elif mapping["type"] == "text":
            if int(mapping["step"]) <= 0:
                errors.append(f"mapping {mapping_id}: step must be positive")
        else:
            errors.append(f"mapping {mapping_id}: unknown type {mapping['type']!r}")

    # ---- translation coverage -------------------------------------------
    terms: dict[str, dict[str, str]] = {}
    for language in ("zh", "jp"):
        path = LOCALIZATION_DIR / f"terms.{language}.json"
        terms[language] = json.loads(path.read_text(encoding="utf-8")) if path.exists() else {}

    term_keys = set().union(*(set(values) for values in terms.values())) if terms else set()
    game_keys = set().union(*enum_sets.values()) if enum_sets else set()
    missing_terms = sorted(game_keys - term_keys)
    if missing_terms:
        warnings.append(
            f"{len(missing_terms)} game terms have no translation "
            "(mostly AID/SEQ/MTYPE; runtime falls back to key)"
        )

    coverage: dict[str, tuple[int, int]] = {}
    for enum_id, entries in enums.items():
        keys = {entry["key"] for entry in entries}
        coverage[enum_id] = (len(keys) - len(keys - term_keys), len(keys))

    # ---- report ----------------------------------------------------------
    lines = [
        "# PoR 数据迁移报告",
        "",
        f"- fields: {len(fields)}",
        f"- slot: base=0x{slot_base:08X}, step=0x{slot_step:X}, count={slot_count}, end=0x{slot_end:08X}",
        "- kinds: " + ", ".join(f"{key}={value}" for key, value in sorted(field_kinds.items())),
        "- scopes: " + ", ".join(f"{key}={value}" for key, value in sorted(field_scopes.items())),
        "",
        "## 枚举",
        "",
    ]
    for enum_id, entries in enums.items():
        lines.append(f"- {enum_id}: {len(entries)}")
    lines += [
        f"- support: {len(support)}",
        f"- affinity: {len(affinity)}",
        f"- group: {len(group)}",
        f"- army: {len(army)}",
        f"- skills/masteries/features: {len(skills['skills'])}/{len(skills['masteries'])}/{len(skills['features'])}",
        "",
        "## 资源",
        "",
        f"- PID missing: {len(pid_missing)}",
        f"- IID missing: {len(iid_missing)}",
        f"- SID (skill/mastery) missing: {len(sid_missing)}",
        f"- AFF missing: {len(aff_missing)}",
        f"- WP missing: {len(wp_missing)}",
        "",
        "## 翻译",
        "",
        f"- game keys: {len(game_keys)}",
        f"- translated keys: {len(term_keys)}",
        f"- missing translation keys: {len(missing_terms)}",
        "",
        "| enum | translated | total |",
        "|---|---:|---:|",
    ]
    lines += [
        f"| {enum_id} | {translated} | {total} |"
        for enum_id, (translated, total) in coverage.items()
    ]
    lines += [
        "",
        "## 错误",
        "",
    ]
    lines += [f"- {item}" for item in errors] or ["- 无"]
    lines += ["", "## 警告", ""]
    lines += [f"- {item}" for item in warnings] or ["- 无"]
    lines.append("")

    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text("\n".join(lines), encoding="utf-8")
    print(f"wrote {REPORT}")
    print(f"fields={len(fields)} errors={len(errors)} warnings={len(warnings)}")
    for item in errors:
        print(f"ERROR: {item}")
    for item in warnings:
        print(f"WARN: {item}")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
