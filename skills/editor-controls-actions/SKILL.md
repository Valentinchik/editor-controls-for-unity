---
name: editor-controls-actions
description: Add or change Unity editor tools, cheats and tests that the "Editor Controls for Unity" plugin puts on Logitech keys (Logi Options+ — MX Keypad, MX Creative Console, Actions Ring) through the com.valentinchik.editor-controls package. Use whenever the user wants a tool, cheat or test runnable from their keypad, asks to group or reorder keypad tools, or asks why a tool or test does not show up in the plugin's Project Tools folder or side panel.
---

# Keypad actions with Editor Controls for Unity

The Unity package `com.valentinchik.editor-controls` connects the editor to the Logi Options+ plugin *Editor Controls
for Unity*. Static methods marked `[EditorControlsAction]` become keys: they fill the plugin's **Project Tools** folder
and appear one by one in the Options+ side panel, ready to drag onto a key. Full documentation:
<https://github.com/Valentinchik/editor-controls-for-unity/blob/main/docs/project-tools.md>.

## Before writing code

1. **Which package version is installed?** Find `com.valentinchik.editor-controls` in `Packages/manifest.json`. It can
   be a git URL (`…editor-controls-for-unity.git?path=/unity#v0.6.1` — the tag is the version), a local folder
   (`file:…`) or a registry version. For anything but a tagged URL, read `version` from the package's own
   `package.json` (the local folder, or `Library/PackageCache/com.valentinchik.editor-controls@*/`). `Group` needs
   **0.6.0+**, `Order` **0.5.3+**. If the package is missing or too old, tell the user — adding or upgrading a package
   is their call, not a side effect of writing a tool.
2. **Is the dependency already there?** If other code in the project already uses `[EditorControlsAction]`, it is —
   carry on. Otherwise the attribute adds a compile-time dependency: in a project under version control (look for
   `.git`, `.plastic`, `.svn`, `.p4config` in the project folder or its parents), the manifest line must be committed
   together with the code, or teammates and CI stop compiling. A `file:` package with an absolute path works only on
   that machine — say so. If the user cannot share the dependency, offer a plain `[MenuItem]` (see the end).
3. **Is it already built in?** The plugin ships ~100 actions; don't write a tool for something a key already does:
   Play / Pause / Step; time scale 0.25× / 1× / 2×, Slow Down / Speed Up and a dial; Reload Domain and Clean Play;
   Run EditMode / PlayMode Tests; Scene View, GameObject, Tools, Windows, Edit, Animation shortcuts; folders for
   Scenes, Layouts, Camera Bookmarks, Recent Assets and Favorites; and *Run Menu Item*, which puts any existing menu
   item on a key. List: <https://github.com/Valentinchik/editor-controls-for-unity#features>. If a request is close to
   a built-in but different (e.g. "pause the game" via `Time.timeScale` vs. the editor's Pause), build it and tell the
   user how it differs.
4. **Where does editor code live?** The package's assembly `Valentinchik.EditorControls.Editor` is editor-only and
   auto-referenced:
   - **No asmdefs** (code in `Assembly-CSharp` / `Assembly-CSharp-Editor`): put tools in any `Editor` folder; cheats
     that use runtime types or belong next to gameplay can live in runtime code, with the attribute *and* its `using`
     inside `#if UNITY_EDITOR`. Follow whatever the project already does.
   - **Code in asmdefs**: use an editor-only asmdef (`includePlatforms: ["Editor"]`) that references
     `Valentinchik.EditorControls.Editor` and the runtime assembly it drives. Do not reference the package from a runtime asmdef.

## The attribute

```csharp
using Valentinchik.EditorControls;

public static class LevelTools
{
    [EditorControlsAction("Snap To Ground", Icon = "magnet", Group = "Level Design", Order = 1)]
    private static void SnapToGround() { /* ... */ }
}
```

| Part | Meaning |
|---|---|
| method | `static`, **no parameters**, any access level. Anything else is skipped silently. |
| `Label` (ctor) | Text on the key. Defaults to the nicified method name. Keep it short — about two lines of a dozen characters. |
| `Icon` | A Tabler icon the plugin ships — [references/icons.md](references/icons.md). An unknown name shows a tool glyph. |
| `Group` | Subfolder of Project Tools and a group in the side panel (*Project Tools › Group*). No group: the tool sits at the top level after the groups. Reuse existing group names exactly — a typo makes a second subfolder. |
| `Order` | Position, lower first, then by label. A group is placed where its lowest `Order` puts it; equal groups sort by name. |

The package rebuilds the list on every domain reload and sends it to the plugin: the **Project Tools** folder shows
groups as subfolders (with a back key inside), and the side panel lists each tool under *Project Tools › Group*.

- **Renaming breaks keys.** A tool's id is `method:<Namespace.Type>.<Method>`. Renaming or moving the method, its
  class or namespace disconnects keys the user bound to that tool in the side panel (the folder adapts by itself).
  Changing `Label`, `Icon`, `Group` or `Order` is safe. Before such a rename, tell the user which keys need re-binding.
- If the project already has keypad tools without a `Group` and you add a group next to them, mention it and offer to
  group them too.

## Making a tool keypad-friendly

- **Context comes from the editor**, not parameters: `Selection`, the active scene, `SceneView.lastActiveSceneView`.
- **Record Edit Mode changes with `Undo`** (`Undo.RecordObject(s)`, `Undo.RegisterCreatedObjectUndo`, ...). The package
  (0.6.1+) wraps every key press in its own undo group named after the tool, so all recorded changes of one press undo
  together with one Ctrl/Cmd+Z. Play Mode changes need no Undo — they vanish when Play Mode ends.
- **Report the result in the console** with a short prefix — the user is looking at the game, not at a window. When the
  tool cannot run (nothing selected, not in Play Mode), say so instead of failing silently: the keypad has no menu
  validators to grey the key out.
- **No modal dialogs on the main path.** The key press arrives while the user may be in another window, and agents
  that drive the editor block on modals.
- Calls arrive on the editor main thread, so Unity APIs are safe. Keep the work short; long jobs should show a
  progress bar or run asynchronously.

## Cheats (Play Mode tools)

```csharp
#if UNITY_EDITOR
using Valentinchik.EditorControls;
#endif

public static class EconomyCheats
{
#if UNITY_EDITOR
    [EditorControlsAction("Add 1000 Coins", Icon = "star", Group = "Cheats", Order = 1)]
#endif
    private static void AddCoins()
    {
        if (!UnityEngine.Application.isPlaying)
        {
            UnityEngine.Debug.Log("[Cheats] Cheats work in Play Mode.");
            return;
        }

        // Reach the running game the way the project already does (DI container, service locator, scene lookup)
        // and call existing gameplay code. Log what changed.
    }
}
```

- **Guard with `Application.isPlaying`** — in Edit Mode a cheat must not touch saves, assets or scenes.
- **Reach game state the project's way** (its DI container, services, managers) — don't add statics or singletons as
  a shortcut to the game. A cheat's own small state (say, the time scale before a pause) may live in a static field,
  but reset it per Play session with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`:
  projects that enter Play Mode without a domain reload keep statics between sessions.
- **Never write the player's real save data** unless the user asked for exactly that.
- Group cheats (`Group = "Cheats"`) so they share one subfolder and one side-panel group.

## Tests the Run Tests keys run

The *Run EditMode Tests* / *Run PlayMode Tests* keys use the Unity Test Framework (`com.unity.test-framework`) and run
every test of that mode — or only the assemblies ticked in *Preferences → Editor Controls for Unity → Run Tests keys*.
If the project has plugin-bundled tests (e.g. a DI framework's own suite), suggest the user ticks only their
assemblies there; it is a personal editor preference, so don't try to set it from code.

- Tests must be discoverable by the Test Framework: an assembly that references `nunit.framework` and the TestRunner
  assemblies — a test asmdef, or (in projects without asmdefs) any `Editor` folder if `Assembly-CSharp-Editor` already
  references them. Verify with the Test Runner window or by listing tests before claiming they are picked up.
- Keep the EditMode suite fast — the key runs it in one go. Mark slow tests `[Explicit]`.
- The keys show progress, then `passed / total`; MX Master 4 vibrates on the result. Runs started from the Test Runner
  window show on the keys too.

## Verifying

What you can check:

1. The project compiles — through the editor if you can drive it, otherwise at least by checking the APIs you used
   for the project's Unity version. Say plainly what you could not compile.
2. The console shows no errors after the domain reload.

What only the user can check — list it for them at the end:

3. The tool appears in the **Project Tools** folder (inside its group) and in the Options+ side panel under
   *Project Tools › Group*.
4. Pressing the key does what it says, logs to the console, and (for Edit Mode tools) one Ctrl/Cmd+Z undoes it.

If a tool does not show up: is it static and parameterless, is the attribute inside an `#if` that is off, is the
package older than a property you used, did the editor finish recompiling?

## When a menu item is the better fit

A plain `[MenuItem("MyGame/…")]` needs no package dependency, appears in Unity's menu, and can still go on a key: the
plugin's *Run Menu Item* action lets the user pick any menu item. It does not appear in Project Tools (unless the user
lists that top-level menu in the package's Preferences). A method may carry both attributes.
