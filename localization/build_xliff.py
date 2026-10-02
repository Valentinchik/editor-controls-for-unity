#!/usr/bin/env python3
"""Fills localization/UnityEditorControls.template.xliff with translations.json, one file per language,
into src/package/localization/UnityEditorControls_<lang>.xliff (the name Logi Plugin Service expects).

Regenerate the template after adding or renaming actions: build, then `open "loupedeck://plugin/UnityEditorControls/xliff"`
and copy bin/Debug/localization.generated/UnityEditorControls.xliff over localization/UnityEditorControls.template.xliff.
"""
import json
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parent
OUT = ROOT.parent / "src" / "package" / "localization"
PLUGIN = "UnityEditorControls"


def main() -> int:
    translations = {lang: table for lang, table in json.loads((ROOT / "translations.json").read_text(encoding="utf-8")).items() if not lang.startswith("_")}
    translations["en-US"] = {}

    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.xliff"):
        old.unlink()

    problems = 0
    for lang, table in sorted(translations.items()):
        tree = ET.parse(ROOT / f"{PLUGIN}.template.xliff")
        for file in tree.getroot().iter("file"):
            file.set("target-language", lang)
        for unit in tree.getroot().iter("trans-unit"):
            source = unit.find("source").text or ""
            is_description = "###description" in unit.get("id")
            if lang == "en-US" or is_description:
                text = source  # descriptions are Unity menu paths, kept in English like the Unity Editor UI
            elif source in table:
                text = table[source]
            else:
                print(f"{lang}: no translation for '{source}'", file=sys.stderr)
                problems += 1
                text = source
            target = unit.find("target")
            target.text = text
            target.set("state", "translated")
        out = OUT / f"{PLUGIN}_{lang}.xliff"
        tree.write(out, encoding="utf-8", xml_declaration=True)
        print(f"{lang} -> {out.relative_to(ROOT.parent)}")

    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
