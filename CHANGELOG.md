# Changelog

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
