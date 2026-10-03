# Editor Controls for Unity

A Logi Options+ plugin that puts the Unity Editor on your Logitech keys, dials and Actions Ring:
**103 ready-made actions with icons**, **6 dynamic folders** and **default profiles for every device**, so there is
nothing to set up by hand. Add the optional [Unity package](#unity-package) and the keys show live editor state,
folders fill with your project's scenes, layouts and assets, and an MX Master 4 vibrates when the editor needs you.

![Default profiles](docs/default-profiles.png)

## Features

- **103 actions** in 14 groups — Play Mode, Status, Tests, Scene View, Grid & Snap, GameObject, Selection, Tools,
  Windows, Edit, Project, Animation, Folders, and 9 dial actions (Undo / Redo, tool switching, time scale, animation
  frames and keys, selection history, grid size, list navigation, frame-by-frame stepping).
- **Follows your Unity shortcuts.** If you rebound a command in Unity (*Edit → Shortcuts*), the plugin sends your
  binding, including bindings inherited from a parent profile. A command you unbound in Unity does nothing.
- **Default profiles** for MX Keypad / MX Creative Keypad (6 pages), MX Creative Dialpad (buttons + dial + roller)
  and the Actions Ring (with *Create* and *Windows* folders). They install with the plugin and switch on
  automatically when Unity is in front.
- **9 languages:** English, Russian, German, French, Spanish, Brazilian Portuguese, Japanese, Korean,
  Simplified Chinese.
- Works on **macOS and Windows**, with any keyboard layout.

With the [Unity package](#unity-package):

- **Live keys.** Play turns into Stop while the game runs, Pause lights up, the active tool and render mode are
  highlighted, toggles (2D / 3D, Pivot / Center, Local / Global, snapping, Lock View, Isolate, Overlays,
  Profiler Record, the selected object's active / hidden / pickable state) show their current state, *Compile* shows
  compiling / done / failed and *Errors* counts console errors. Actions run even while Unity is in the background.
- **Dynamic folders** that fill from the open project:
  *Scenes* (the open one highlighted), *Project Tools* (your own editor tools — see below), *Layouts* (Unity's and
  your saved window layouts), *Camera Bookmarks* (7 saved Scene View cameras per scene — an empty slot saves the
  view, a filled one flies back to it), *Recent Assets* and *Favorites* (prefabs open in Prefab Mode, scenes and
  scripts open, everything else is selected).
- **Time scale** in Play Mode: 0.25× / 1× / 2× keys, *Slow Down* / *Speed Up* along
  0.05 · 0.1 · 0.25 · 0.5 · 0.75 · 1 · 1.5 · 2 · 4 · 8, and a dial (press to reset). The keys show the current value.
- **Tests:** *Run EditMode Tests* and *Run PlayMode Tests* show progress while running, then `passed / total` in green
  or red for two seconds. Needs the Unity Test Framework package. To leave out tests that come with plugins, pick the
  assemblies the keys run in *Preferences → Editor Controls for Unity*.
- **Run Menu Item:** a configurable action — pick any Unity menu item from a list in the action's settings in Options+.
- **Haptics on MX Master 4:** compilation succeeded / failed, Play Mode entered / exited, a new console error, tests
  passed / failed. Each event has its own waveform, changeable in Options+; *Test Vibration* (Status group) checks
  that the mouse buzzes.

## Supported devices

| Device | Default profile |
|---|---|
| MX Keypad, MX Creative Keypad | 6 pages of 9 keys: Play & Scene, GameObject, Tools, Windows, Project, Workflow |
| MX Creative Dialpad | Play / Stop, Pause, Frame Selected, Save; dial = Undo / Redo, roller = tool |
| Actions Ring (Logi Options+ mice and keyboards) | 6 actions + *Create* and *Windows* folders |
| MX Master 4 | Haptic feedback for editor events |
| Loupedeck CT / Live / Live S | All actions are available; no default profile |

## Compatibility

- **Unity 6** (6000.x): every action.
- **Unity 2022.3**: everything except the actions marked *Unity 6+* in their description — Shaded / Wireframe /
  Shaded Wireframe (they exist in 2022 but have no default shortcut; bind them in Unity and the keys work), Unlit,
  Grid & Snap, and Previous / Next Selection.
- **Logi Options+** with Logi Plugin Service 6.4 or newer.
- Without the Unity package, actions are Unity keyboard shortcuts, so the Unity Editor must be the active window.
  With the package (Unity 2022.3+), most actions run directly through the editor API, and the render-mode and
  grid-snap actions work in Unity 2022.3 too. Folders, time scale, tests, *Run Menu Item* and haptics need the package.

## Install

- **Logitech Marketplace** — search for *Editor Controls for Unity* in Logi Options+ *(coming soon)*.
- **Manually** — download `UnityEditorControls_<version>.lplug4` from [Releases](../../releases) and double-click it.

Options+ lists apps installed in the top levels of `/Applications`, and Unity Hub installs editors deeper than that, so
Unity may be missing from Options+' *Add application* list. You don't need it: the plugin adds **Unity Editor** with its
profiles on install. A profile you already have is never replaced by a newer default — new actions appear in the
side panel, ready to drag onto a key.

## Unity package

The optional companion package `com.valentinchik.editor-controls` connects the editor to the plugin
(over a local connection on `127.0.0.1` only) for live key states, folders, haptics and background control.
It is editor-only and never ends up in a player build.

Install it in *Window → Package Manager → + → Install (Add) package from git URL…*:

```
https://github.com/Valentinchik/editor-controls-for-unity.git?path=/unity#v0.5.1
```

or add it to `Packages/manifest.json`:

```json
"com.valentinchik.editor-controls": "https://github.com/Valentinchik/editor-controls-for-unity.git?path=/unity#v0.5.1"
```

With several Unity projects open, the keys act on the one in front — through the package if it has it, as keyboard
shortcuts if it does not. While Unity is in the background, they control the project you used last.

When the editor connects, the Unity console shows *[Editor Controls] Connected to Logi Options+* and the plugin's
status in Options+ reads *Connected to Unity: &lt;project&gt;*. The package runs only a fixed set of editor commands
and accepts connections from the plugin alone (a per-session token stored in a file only your user can read).

### Project Tools

The *Project Tools* folder lists your own editor tools:

- static parameterless methods marked with `[EditorControlsAction]`:

  ```csharp
  using Valentinchik.EditorControls;

  public static class MyTools
  {
      [EditorControlsAction("Rebuild Atlas", Icon = "photo")] // Icon: any Tabler icon name the plugin ships
      private static void RebuildAtlas() { /* ... */ }
  }
  ```

- menu items under the top-level menus listed in *Preferences → Editor Controls for Unity* (`Tools` by default;
  add your own, e.g. `MyGame`). The list is stored per project in EditorPrefs.

### Test keys

*Run EditMode Tests* / *Run PlayMode Tests* run every test of that mode, including tests bundled with plugins
(Zenject alone brings several hundred). In *Preferences → Editor Controls for Unity → Run Tests keys*, turn on
*Run only the selected test assemblies* and tick the assemblies with your own tests. Runs started from the Test Runner
window show on the keys as well.

### Favorites, recent assets and camera bookmarks

Add assets to *Favorites* with *Assets → Editor Controls → Add to Favorites* (also in the Project window's context
menu). *Recent Assets* collects assets you select in the Project window or open in Prefab Mode. Favorites, recent
assets and camera bookmarks are personal: they are stored in `UserSettings/EditorControls/`, which Unity's standard
version-control ignore files exclude.

## Building from source

Requirements: .NET 10 SDK, Logi Options+ (Logi Plugin Service ≥ 6.4), Python 3 with Pillow, and
`LogiPluginTool` (`dotnet tool install --global LogiPluginTool`). The build scripts are for macOS.

```bash
./build.sh   # icons + profiles + localization + Debug build; installs a dev link and reloads the plugin
./pack.sh    # Release build → dist/UnityEditorControls_<version>.lplug4, checked with `logiplugintool verify`
```

`build.sh` uses the .NET SDK in `~/.dotnet` (`dotnet-install.sh --channel 10.0 --install-dir ~/.dotnet`).
Changed icons or profiles need a Logi Plugin Service restart. A profile already installed for a device is never
overwritten by a newer default. `python3 profiles/build_profile.py --docs` re-renders `docs/default-profiles.png`.

### Layout

| Path | What |
|---|---|
| `src/Unity/UnityShortcuts.cs` | The catalog: Unity Shortcut Manager id + Unity 6 default binding (+ Windows / Unity 2022 differences) |
| `src/Unity/UnityShortcutProfile.cs` | Reads the active Unity shortcut profile (EditorPrefs + `.shortcut` files, parent chain) |
| `src/Unity/UnityBinding.cs` | Unity `KeyCode` / `ShortcutModifiers` → Logi virtual keys |
| `src/Actions/` | One-line action classes per group; `Adjustments.cs` for dials; live and bridge-only key base classes |
| `src/Folders/` | Dynamic folders filled from lists the Unity package sends |
| `src/Bridge/` | Loopback server for the Unity package, editor state and lists |
| `src/Haptics/` | MX Master 4 haptic events; waveforms in `src/package/events/extra/eventMapping.yaml` |
| `unity/` | The Unity package (`com.valentinchik.editor-controls`) |
| `icons/` | `map.tsv` (action → Tabler icon) and `build_icons.py` → `actionsymbols/` (picker) + `actionicons/` (keys) |
| `profiles/` | Default profile layouts (`Loupedeck70` Keypad, `71` Dialpad, `72` Actions Ring) and their builder |
| `localization/` | `translations.json` + `build_xliff.py` over the XLIFF template generated by Logi Plugin Service |

### Adding an action

1. Add the shortcut to `UnityShortcuts.cs` — id and default binding from Unity's Shortcut Manager.
2. Add a one-line class next to its group in `src/Actions/*Commands.cs`.
3. Add `<ClassName>\t<tabler-icon>` to `icons/map.tsv`; put the [Tabler](https://tabler.io/icons) outline icon
   into `icons/tabler/`.
4. `./build.sh`, then refresh the XLIFF template (`open "loupedeck://plugin/UnityEditorControls/xliff"`, copy
   `bin/Debug/localization.generated/UnityEditorControls.xliff` over `localization/UnityEditorControls.template.xliff`)
   and add the new strings to `translations.json`; `build_xliff.py` fails on any missing translation.

## License

[End User License Agreement](EULA.md). Third-party components: [Tabler Icons](https://github.com/tabler/tabler-icons),
MIT — see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

Unity is a trademark of Unity Technologies; Logitech, Logi, MX and Options+ are trademarks of Logitech.
This is an independent project, not affiliated with or endorsed by either.
