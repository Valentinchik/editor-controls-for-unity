#!/usr/bin/env python3
"""Builds the default Options+ profiles (.lp5) shipped with the plugin, one per profiles/<layout>.json.

An .lp5 is a zip: ApplicationInfo.json + ProfileInfo.json (the layout) + metadata/ (package yaml,
key previews for the profile picker) + ActionIcons/ (icons of the folders it defines).
Device types: Loupedeck70 = MX Keypad / MX Creative Keypad, Loupedeck71 = MX Creative Dialpad,
Loupedeck72 = Actions Ring. GUIDs are derived from names, so a rebuild updates the same profile.
"""
import base64
import functools
import io
import json
import re
import subprocess
import sys
import tempfile
import uuid
import zipfile
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "src"
PACKAGE = SRC / "package"
PLUGIN = "UnityEditorControls"
NAMESPACE = "Loupedeck.UnityEditorControlsPlugin"
APP_NAME = "@_" + PLUGIN.lower()
APP_DISPLAY_NAME = "Unity Editor"
PROFILE_DISPLAY_NAME = "Editor Controls for Unity"
BUNDLE_ID = "com.unity3d.UnityEditor5.x"
LAYOUT7 = "Loupedeck.Service.Devices.Loupedeck7Devices."
PROFILE_ACTION = "$@Generic___@ProfileAction___"
FOLDER_DESCRIPTION = "Group and nest multiple actions"

KEY_SIZE = 116
FONT = "/System/Library/Fonts/Helvetica.ttc"
BLACK, WHITE = 0xFF000000, 0xFFFFFFFF


def guid(*parts: str) -> str:
    return uuid.uuid5(uuid.NAMESPACE_URL, "/".join((PLUGIN,) + parts)).hex.upper()


def action_name(cls: str) -> str:
    if cls.endswith("Folder"):  # PluginDynamicFolder classes are referenced through the folder action
        return f"${PLUGIN}___#DynamicFolder___DynamicFolder#{NAMESPACE}.{cls}"
    return f"${PLUGIN}___{NAMESPACE}.{cls}"


def plugin_version() -> str:
    yaml = (PACKAGE / "metadata" / "LoupedeckPackage.yaml").read_text(encoding="utf-8")
    return re.search(r"^version:\s*(\S+)", yaml, re.M).group(1)


@functools.cache
def display_names() -> dict:
    names = {}
    for cs in [*(SRC / "Actions").glob("*.cs"), *(SRC / "Folders").glob("*.cs")]:
        text = cs.read_text(encoding="utf-8")
        names.update(re.findall(r'class (\w+) : \w+\s*\{(?:(?!\bclass\b).)*?public \1\(\) : base\("([^"]+)"', text, re.S))
        names.update(re.findall(r'class (\w+) : PluginDynamic\w+(?:(?!\bclass\b).)*?displayName: "([^"]+)"', text, re.S))
        # TimeScalePresetCommand builds its name from the value: base(0.25f) -> "Time Scale 0.25×".
        names.update((cls, f"Time Scale {float(value):g}×")
                     for cls, value in re.findall(r'class (\w+) : TimeScalePresetCommand\s*\{\s*public \1\(\) : base\(([\d.]+)f\)', text))
    return names


@functools.cache
def icon_map() -> dict:
    lines = (ROOT / "icons" / "map.tsv").read_text(encoding="utf-8").splitlines()
    return dict(line.split("\t") for line in lines if line.strip() and not line.startswith("#"))


def tabler_paths(icon: str) -> str:
    svg = (ROOT / "icons" / "tabler" / f"{icon}.svg").read_text(encoding="utf-8")
    return "".join(re.findall(r"<(?:path|circle|rect|line|polyline|polygon|ellipse)[^>]*/>", svg))


def white_svg(icon: str) -> str:
    return (
        '<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" viewBox="0 0 24 24" fill="none" '
        'stroke="#FFFFFF" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">'
        f"{tabler_paths(icon)}</svg>"
    )


def render_glyph(icon: str, size: int) -> Image.Image:
    """White glyph on black, rasterized with Quick Look (the only SVG renderer macOS ships)."""
    wrapper = (
        '<svg xmlns="http://www.w3.org/2000/svg" width="240" height="240" viewBox="0 0 240 240">'
        '<rect width="240" height="240" fill="#000"/>'
        '<g transform="scale(10)" fill="none" stroke="#FFF" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">'
        f"{tabler_paths(icon)}</g></svg>"
    )
    with tempfile.TemporaryDirectory() as tmp:
        src = Path(tmp) / f"{icon}.svg"
        src.write_text(wrapper, encoding="utf-8")
        subprocess.run(["qlmanage", "-t", "-s", "512", "-o", tmp, str(src)], check=True, capture_output=True)
        img = Image.open(Path(tmp) / f"{icon}.svg.png").convert("RGB")
    # Quick Look pads the render with white; the black square marks the real bounds.
    left, top, right, bottom = ImageChops.difference(img, Image.new("RGB", img.size, "white")).convert("L").point(lambda v: 255 if v > 40 else 0).getbbox()
    inset = 3  # drop the anti-aliased edge of the square, or it shows as a faint frame
    return img.crop((left + inset, top + inset, right - inset, bottom - inset)).resize((size, size), Image.LANCZOS)


def render_key(icon: str, label: str) -> str:
    out = io.BytesIO()
    render_key_image(icon, label).save(out, format="PNG")
    return base64.b64encode(out.getvalue()).decode("ascii")


def render_key_image(icon: str, label: str) -> Image.Image:
    # Mirrors metadata/DefaultIconTemplate.ict: glyph at (20,6) 60x60, label in the bottom 30 of a 100-unit key.
    scale = KEY_SIZE / 100
    key = Image.new("RGB", (KEY_SIZE, KEY_SIZE), "black")
    key.paste(render_glyph(icon, round(60 * scale)), (round(20 * scale), round(6 * scale)))

    draw = ImageDraw.Draw(key)
    font = ImageFont.truetype(FONT, 14)
    while draw.textlength(label, font=font) > KEY_SIZE - 6 and font.size > 9:
        font = ImageFont.truetype(FONT, font.size - 1)
    draw.text(((KEY_SIZE - draw.textlength(label, font=font)) / 2, round(85 * scale)), label, fill="white", font=font, anchor="lm")
    return key.convert("RGBA")


def folder_icon_template(icon: str, label: str) -> dict:
    return {
        "backgroundColor": BLACK,
        "items": [
            {
                "$type": "Loupedeck.Service.ActionIconImageItem, LoupedeckShared",
                "image": base64.b64encode(white_svg(icon).encode("utf-8")).decode("ascii"),
                "imageFileName": f"{icon}.svg",
                "imageColor": WHITE,
                "imageRotation": "None",
                "isVisible": True,
                "itemType": "Image",
                "area": {"x": 20, "y": 6, "width": 60, "height": 60, "isFullScreen": False},
            },
            {
                "$type": "Loupedeck.Service.ActionIconTextItem, LoupedeckShared",
                "text": label,
                "textColor": WHITE,
                "fontSize": 5,
                "fontName": "Brown Logitech Pan Light",
                "isVisible": True,
                "itemType": "Text",
                "area": {"x": 0, "y": 70, "width": 100, "height": 30, "isFullScreen": False},
            },
        ],
    }


def control(i: int, press=None, rotate=None) -> dict:
    return {"$type": LAYOUT7 + "ProfileLayoutControl7, LoupedeckService", "controlId": i, "pressAction": press, "rotateAction": rotate}


def page(name: str, display_name: str, controls: list, description=None) -> dict:
    return {"$type": LAYOUT7 + "ProfileLayoutPage7, LoupedeckService", "name": name, "displayName": display_name, "description": description, "controls": controls}


class Builder:
    def __init__(self, layout: dict):
        self.device = layout["device"]
        self.layout = layout
        self.profile_actions = []
        self.folder_pages = []
        self.folder_icons = {}
        self.folder_info = {}

    def key_action(self, key) -> str:
        if isinstance(key, str):
            if key not in display_names():
                raise SystemExit(f"{self.layout['file']}: unknown action {key}")
            return action_name(key)

        folder_id = guid(self.device, "folder", key["folder"])
        name = PROFILE_ACTION + folder_id
        self.profile_actions.append({
            "$type": "Loupedeck.Service.ApplicationProfileCommand, LoupedeckService",
            "isCommand": True,
            "name": name,
            "templateActionName": "$@Generic___@OpenFolder",
            "actionParameters": {
                "$type": "Loupedeck.ActionEditorActionParameters, PluginApi",
                "parameters": {"$type": "Loupedeck.StringDictionaryNoCase, PluginApi", "folderName": ""},
                "count": 1,
            },
            "displayName": key["folder"],
            "description": FOLDER_DESCRIPTION,
            "groupName": "Folders",
            "superGroupName": "@navigation",
            "isProfileAction": True,
            "isMultiState": False,
            "isResetCommand": False,
            "adjustmentName": None,
            "states": None,
        })
        self.folder_pages.append(page(folder_id, "Folder", [control(i, self.key_action(k)) for i, k in enumerate(key["keys"])], FOLDER_DESCRIPTION))
        self.folder_icons[name] = folder_icon_template(key["icon"], key["folder"])
        self.folder_info[name] = (key["icon"], key["folder"])
        return name

    def preview_entry(self, i: int, action: str):
        if action is None:
            return None
        if action in self.folder_info:
            icon, label = self.folder_info[action]
        else:
            cls = action.rsplit(".", 1)[1]
            icon, label = icon_map()[cls], display_names()[cls]
        return {"controlId": i, "actionName": action, "displayName": label, "description": "", "image": render_key(icon, label)}

    def build(self) -> Path:
        device = self.device
        press_pages = [
            page(guid(device, "page", p["name"]), p["name"], [control(i, self.key_action(k)) for i, k in enumerate(p["keys"])])
            for p in self.layout["pages"]
        ]
        dials = [action_name(d) if d else None for d in self.layout.get("dials", [])]
        rotate_pages = [page(guid(device, "dials"), "Dial Page", [control(i, rotate=d) for i, d in enumerate(dials)])] if dials else []

        profile_id = guid(device, "profile")
        workspace_id = guid(device, "workspace")
        profile = {
            "$type": "Loupedeck.Service.ApplicationProfile, LoupedeckService",
            "name": profile_id,
            "profileFlags": "None",
            "displayName": PROFILE_DISPLAY_NAME,
            "description": None,
            "deviceType": device,
            "applicationName": APP_NAME,
            "nativePluginName": PLUGIN,
            "hasNativePlugin": True,
            "additionalNativePluginNames": ["DefaultMac"],
            "lastModifiedTimeUtc": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%f0Z"),
            "profileSettings": {"$type": "Loupedeck.DictionaryNoCase`1[[System.String, System.Private.CoreLib]], PluginApi"},
            "actionImages90": None,
            "actionImages60": None,
            "wheelImages": None,
            "actionColors": None,
            "layout": {
                "$type": LAYOUT7 + "ProfileLayout7, LoupedeckService",
                "deviceType": device,
                "profileFlags": "None",
                "layoutModes": [{
                    "$type": LAYOUT7 + "ProfileLayoutMode7, LoupedeckService",
                    "deviceType": device,
                    "modeName": "main",
                    "parentModeName": None,
                    "actions": None,
                    "dynamicButtonPages": None,
                    "dynamicEncoderPages": None,
                    "workspaces": [{
                        "$type": LAYOUT7 + "ProfileLayoutWorkspace7, LoupedeckService",
                        "name": workspace_id,
                        "displayName": "Workspace 1",
                        "description": None,
                        "pressPages": press_pages,
                        "rotatePages": rotate_pages,
                    }],
                    "homeWorkspaceName": workspace_id,
                }],
                "folderPages": self.folder_pages,
            },
            "macroCommands": [],
            "macroAdjustments": [],
            "profileCommands": [],
            "profileAdjustments": [],
            "conversionHistory": None,
            "packageName": None,
            "packageVersion": None,
            "profileActions": self.profile_actions,
        }

        app_info = {
            "$type": "Loupedeck.Service.SupportedApplicationInfo, LoupedeckService",
            "name": APP_NAME,
            "displayName": APP_DISPLAY_NAME,
            "description": None,
            "deviceType": device,
            "nativePluginName": PLUGIN,
            "hasNativePlugin": True,
            "processOrBundleName": BUNDLE_ID,
            "modes": [{"$type": "Loupedeck.Service.ApplicationMode, LoupedeckService", "name": "main", "parentModeName": None, "displayName": "Main"}],
            "defaultProfileName": profile_id,
            "isEnabled": True,
        }

        first_page = press_pages[0]["controls"]
        preview = {
            "buttonPages": [self.preview_entry(c["controlId"], c["pressAction"]) for c in first_page],
            "encoderPages": [self.preview_entry(i, d) for i, d in enumerate(dials)],
        }

        out = PACKAGE / "profiles" / self.layout["file"]
        out.parent.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
            z.write(PACKAGE / "metadata" / "Icon256x256.png", "ApplicationIcon.png")
            z.writestr("ApplicationInfo.json", json.dumps(app_info, indent=4, ensure_ascii=False))
            z.writestr("ProfileInfo.json", json.dumps(profile, indent=4, ensure_ascii=False))
            for name, template in self.folder_icons.items():
                z.writestr(f"ActionIcons/{name}.ict", json.dumps(template, indent=2, ensure_ascii=False))
            z.writestr("metadata/AdvancedInfo.json", json.dumps({"additionalPluginNames": [PLUGIN]}, indent=4))
            z.writestr("metadata/LoupedeckPackage.yaml", f"type: Profile5\nname: {profile_id}\ndisplayName: {PROFILE_DISPLAY_NAME}\nversion: {plugin_version()}\n")
            z.writestr("metadata/ProfilePreview.json", json.dumps(preview, indent=4, ensure_ascii=False))
        return out


DEVICE_NAMES = {"Loupedeck70": "MX Keypad / MX Creative Keypad", "Loupedeck71": "MX Creative Dialpad", "Loupedeck72": "Actions Ring"}


def write_docs_sheet(layouts: list) -> Path:
    """docs/default-profiles.png: every page of every default profile, for the README."""
    cell, label_width, header = KEY_SIZE + 6, 190, 40
    rows = []
    for layout in layouts:
        rows.append(("header", DEVICE_NAMES.get(layout["device"], layout["device"])))
        for p in layout["pages"]:
            rows.append((p["name"], p["keys"]))
            rows += [(f"{k['folder']} (folder)", k["keys"]) for k in p["keys"] if isinstance(k, dict)]
        dials = [d for d in layout.get("dials", []) if d]
        if dials:
            rows.append(("Dials", dials))

    width = label_width + 9 * cell + 10
    height = sum(header if kind == "header" else cell for kind, _ in rows) + 10
    sheet = Image.new("RGB", (width, height), (30, 30, 30))
    draw = ImageDraw.Draw(sheet)
    title_font, label_font = ImageFont.truetype(FONT, 20), ImageFont.truetype(FONT, 15)
    y = 5
    for kind, keys in rows:
        if kind == "header":
            draw.text((10, y + header / 2), keys, fill="white", font=title_font, anchor="lm")
            y += header
            continue
        draw.text((10, y + cell / 2), kind, fill=(170, 170, 170), font=label_font, anchor="lm")
        for i, key in enumerate(keys):
            if isinstance(key, dict):
                icon, label = key["icon"], key["folder"]
            else:
                icon, label = icon_map()[key], display_names()[key]
            sheet.paste(render_key_image(icon, label), (label_width + i * cell, y + 3))
        y += cell

    out = ROOT / "docs" / "default-profiles.png"
    out.parent.mkdir(exist_ok=True)
    sheet.save(out)
    return out


def main() -> int:
    layouts = []
    for layout_path in sorted((ROOT / "profiles").glob("*.json")):
        layout = json.loads(layout_path.read_text(encoding="utf-8"))
        layouts.append(layout)
        out = Builder(layout).build()
        print(f"{layout_path.name} -> {out.relative_to(ROOT)}")
    if "--docs" in sys.argv:
        order = ["Loupedeck70", "Loupedeck72", "Loupedeck71"]
        print("docs ->", write_docs_sheet(sorted(layouts, key=lambda l: order.index(l["device"]))).relative_to(ROOT))
    return 0


if __name__ == "__main__":
    sys.exit(main())
