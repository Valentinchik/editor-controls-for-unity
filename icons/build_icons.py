#!/usr/bin/env python3
"""Builds the plugin's action icons from icons/map.tsv + icons/tabler/*.svg.

actionsymbols/ — black glyphs shown next to the action in the Options+ action picker.
actionicons/   — white glyphs drawn on the key (layout comes from metadata/DefaultIconTemplate.ict).
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
PACKAGE = ROOT.parent / "src" / "package"
NAMESPACE = "Loupedeck.UnityEditorControlsPlugin"


def recolor(svg: str, color: str, size: int) -> str:
    svg = svg.replace('stroke="currentColor"', f'stroke="{color}"')
    svg = re.sub(r'\s+class="[^"]*"', "", svg)
    svg = re.sub(r'width="\d+"', f'width="{size}"', svg, count=1)
    svg = re.sub(r'height="\d+"', f'height="{size}"', svg, count=1)
    return svg


def main() -> int:
    targets = {"actionsymbols": ("#000000", 24), "actionicons": ("#FFFFFF", 100)}
    for folder in targets:
        out = PACKAGE / folder
        out.mkdir(parents=True, exist_ok=True)
        for old in out.glob("*.svg"):
            old.unlink()

    missing = []
    count = 0
    for line in (ROOT / "map.tsv").read_text(encoding="utf-8").splitlines():
        if not line.strip() or line.startswith("#"):
            continue
        action, icon = line.split("\t")
        source = ROOT / "tabler" / f"{icon}.svg"
        if not source.exists():
            missing.append(icon)
            continue
        svg = source.read_text(encoding="utf-8")
        for folder, (color, size) in targets.items():
            (PACKAGE / folder / f"{NAMESPACE}.{action}.svg").write_text(recolor(svg, color, size), encoding="utf-8")
        count += 1

    print(f"{count} actions -> {PACKAGE / 'actionsymbols'} + {PACKAGE / 'actionicons'}")
    if missing:
        print("missing icons: " + ", ".join(missing), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
