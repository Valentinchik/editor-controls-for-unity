# Changelog

## 0.5.1 — 2026-10-03

- Test keys can run only selected test assemblies (*Preferences → Editor Controls for Unity → Run Tests keys*), so
  tests bundled with plugins such as Zenject stay out of the run.
- The Project Tools menu list in Preferences accepts new lines again (Enter was swallowed while typing).
- Several Unity projects open: keys act on the project in front. A project without the Unity package in front gets
  keyboard shortcuts again instead of commands going to another project that has the package; with Unity in the
  background, keys still control the project used last. Live keys and folders follow the same project.
- Haptics follow each open project separately, so switching projects no longer fakes Play Mode or error events.

## 0.5.0 — 2026-10-03

- Haptic feedback on MX Master 4: compilation succeeded / failed, Play Mode entered / exited, new console error,
  tests passed / failed — each with its own waveform, changeable in Options+. *Test Vibration* action to check it.
- Dynamic folders filled from the open project: *Scenes*, *Project Tools*, *Layouts*, *Camera Bookmarks*,
  *Recent Assets*, *Favorites*.
- *Project Tools* lists methods marked `[EditorControlsAction]` and menu items under the menus set in
  *Preferences → Editor Controls for Unity*.
- Camera bookmarks: 7 saved Scene View cameras per scene; favorites via *Assets → Editor Controls → Add to Favorites*.
- Time scale in Play Mode: 0.25× / 1× / 2×, *Slow Down* / *Speed Up* and a *Time Scale* dial; keys show the value.
- *Run EditMode Tests* / *Run PlayMode Tests* with progress and the result on the key (needs the Test Framework).
- *Run Menu Item*: a configurable action — pick any Unity menu item in the action's settings.
- New *Workflow* page in the MX Keypad default profile.
- Entering Play Mode with domain reload no longer counts as a compilation.

## 0.4.0 — 2026-10-02

- Unity companion package `com.valentinchik.editor-controls` (Unity 2022.3+), installed from git URL.
- Live keys: Play shows Stop while playing, Pause lights up, the active tool and render mode are highlighted.
- Toggle keys show their state: 2D / 3D, Pivot / Center, Local / Global, Vertex Snap, Grid Snap, Lock View, Isolate,
  Overlays, Profiler Record, and — for the selected GameObject — active, hidden and pickable.
- New tool icons (Hand, Rotate, Scale, Rect, Transform, Pivot).
- New *Status* group: *Compile* (compiling / done / failed) and *Errors* (console error count).
- With the package, actions run through the editor API — in the background, independent of keyboard layout and
  shortcut bindings; render modes and grid snapping now work in Unity 2022.3 as well.
- Options+ shows which Unity project is connected, or a hint about the package when Unity runs without it.

## 0.3.0 — 2026-10-02

First public version.

- 83 Unity Editor shortcut actions in 10 groups and 8 dial actions, each with an icon.
- Follows the user's own Unity shortcut bindings (active Shortcut Manager profile, parent profiles included).
- Default profiles for MX Keypad / MX Creative Keypad, MX Creative Dialpad and the Actions Ring.
- Linked to the Unity Editor app, so the profiles switch on when Unity is in front.
- Localized into English, Russian, German, French, Spanish, Brazilian Portuguese, Japanese, Korean and Simplified Chinese.
- macOS and Windows; Unity 6, with Unity 2022.3 differences marked as "Unity 6+".
