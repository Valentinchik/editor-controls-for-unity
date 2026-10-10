#!/usr/bin/env python3
"""Builds the plugin's action icons from icons/map.tsv + icons/tabler/*.svg.

actionsymbols/ — black glyphs on a transparent background, shown next to the action in the Options+ action list.
actionicons/   — white glyphs drawn on the key (layout comes from metadata/DefaultIconTemplate.ict).
Both are written as filled outlines: Options+ draws action-list symbols from fills only (strokes vanish in the dark
theme, closed shapes fill solid in the light one), so Tabler's strokes are expanded with picosvg (pip install picosvg).
metadata/Icon256x256.png — the plugin icon: transparent, dark glyph with a white outline (reads on light and dark).
"""
import re
import subprocess
import sys
import tempfile
from pathlib import Path

from PIL import Image, ImageChops
from picosvg.svg import SVG

ROOT = Path(__file__).resolve().parent
PACKAGE = ROOT.parent / "src" / "package"
NAMESPACE = "Loupedeck.UnityEditorControlsPlugin"


# Tabler starts every icon with an invisible 24x24 bounding box. Options+ draws it as a solid tile in the action list
# (a black square in the light theme, white in the dark one), so it never leaves this folder.
BOUNDING_BOX = re.compile(r'\s*<path[^>]*d="M0 0h24v24H0z"[^>]*/>')


def normalize_sources() -> None:
    for svg in (ROOT / "tabler").glob("*.svg"):
        text = svg.read_text(encoding="utf-8")
        clean = BOUNDING_BOX.sub("", text)
        if clean != text:
            svg.write_text(clean, encoding="utf-8")


def filled(svg: str, color: str, size: int) -> str:
    """The icon with every stroke expanded into a filled outline, in one colour, on a transparent background."""
    svg = BOUNDING_BOX.sub("", svg)
    svg = svg.replace('stroke="currentColor"', 'stroke="#000000"')
    svg = re.sub(r'\s+class="[^"]*"', "", svg)
    svg = SVG.fromstring(svg).topicosvg().tostring()
    svg = re.sub(r'fill="[^"]*"', f'fill="{color}"', svg)
    svg = svg.replace("<defs/>", "")
    svg = re.sub(r'width="\d+"', f'width="{size}"', svg, count=1)
    return re.sub(r'height="\d+"', f'height="{size}"', svg, count=1)


# Options+ looks dynamic-folder images up under this prefix (as Logi's own plugins name them).
def file_names(action: str) -> list:
    names = [f"{NAMESPACE}.{action}.svg"]
    if action.endswith("Folder"):
        names.append(f"#DynamicFolder___DynamicFolder#{NAMESPACE}.{action}.svg")
    return names


def main() -> int:
    targets = {"actionsymbols": ("#000000", 24), "actionicons": ("#FFFFFF", 100)}
    for folder in targets:
        out = PACKAGE / folder
        out.mkdir(parents=True, exist_ok=True)
        for old in out.glob("*.svg"):
            old.unlink()

    normalize_sources()
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
            image = filled(svg, color, size)
            for name in file_names(action):
                (PACKAGE / folder / name).write_text(image, encoding="utf-8")
        count += 1

    print(f"{count} actions -> {PACKAGE / 'actionsymbols'} + {PACKAGE / 'actionicons'}")
    write_icon_reference()
    write_plugin_icon()
    if missing:
        print("missing icons: " + ", ".join(missing), file=sys.stderr)
        return 1
    return 0


def stroke_mask(icon: str, width: float) -> Image.Image:
    """The icon's strokes as an 8-bit mask, 240x240, rasterized with Quick Look (the only SVG renderer macOS ships)."""
    paths = "".join(re.findall(r"<(?:path|circle|rect|line|polyline|polygon|ellipse)[^>]*/>", (ROOT / "tabler" / f"{icon}.svg").read_text(encoding="utf-8")))
    wrapper = (
        '<svg xmlns="http://www.w3.org/2000/svg" width="240" height="240" viewBox="0 0 240 240">'
        '<rect width="240" height="240" fill="#000"/>'
        f'<g transform="scale(10)" fill="none" stroke="#FFF" stroke-width="{width}" stroke-linecap="round" stroke-linejoin="round">'
        f"{paths}</g></svg>"
    )
    with tempfile.TemporaryDirectory() as tmp:
        src = Path(tmp) / f"{icon}.svg"
        src.write_text(wrapper, encoding="utf-8")
        subprocess.run(["qlmanage", "-t", "-s", "512", "-o", tmp, str(src)], check=True, capture_output=True)
        img = Image.open(Path(tmp) / f"{icon}.svg.png").convert("RGB")
    # Quick Look pads the render with white; the black square marks the real bounds.
    left, top, right, bottom = ImageChops.difference(img, Image.new("RGB", img.size, "white")).convert("L").point(lambda v: 255 if v > 40 else 0).getbbox()
    inset = 3
    return img.crop((left + inset, top + inset, right - inset, bottom - inset)).resize((240, 240), Image.LANCZOS).convert("L")


def write_plugin_icon() -> None:
    """256x256, transparent outside the glyph, glyph inside the central 192x192 (Logi's plugin-icon rules)."""
    glyph, outline = (34, 36, 37, 255), (255, 255, 255, 255)
    size, area = 256, 192
    icon = Image.new("RGBA", (240, 240), (0, 0, 0, 0))
    icon.paste(Image.new("RGBA", icon.size, outline), mask=stroke_mask("brand-unity", 3.6))
    icon.paste(Image.new("RGBA", icon.size, glyph), mask=stroke_mask("brand-unity", 1.9))
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    offset = (size - area) // 2
    canvas.paste(icon.resize((area, area), Image.LANCZOS), (offset, offset))
    canvas.save(PACKAGE / "metadata" / "Icon256x256.png")


def write_icon_reference() -> None:
    """The names [EditorControlsAction(Icon = ...)] can use: every icon the plugin ships, for the agent skill."""
    names = sorted(svg.stem for svg in (ROOT / "tabler").glob("*.svg"))
    lines = [
        "# Icons for `[EditorControlsAction(Icon = \"…\")]`",
        "",
        f"The plugin draws only these {len(names)} [Tabler](https://tabler.io/icons) outline icons; any other name shows",
        "the `tool` glyph. Generated by `icons/build_icons.py` — do not edit by hand.",
        "",
        "```",
    ]
    line = ""
    for name in names:
        if len(line) + len(name) + 1 > 110:
            lines.append(line.rstrip())
            line = ""
        line += name + " "
    lines += [line.rstrip(), "```", ""]
    target = ROOT.parent / "skills" / "editor-controls-actions" / "references" / "icons.md"
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text("\n".join(lines), encoding="utf-8")


if __name__ == "__main__":
    sys.exit(main())
