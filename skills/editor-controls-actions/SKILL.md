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

1. **Is the package installed?** `Packages/manifest.json` must list `com.valentinchik.editor-controls`
   (git URL `https://github.com/Valentinchik/editor-controls-for-unity.git?path=/unity#v<version>`). `Group` needs
   **0.6.0+**, `Order` **0.5.3+**. If it is missing or older, tell the user — adding or upgrading a package is their
   call, not a side effect of writing a tool.
2. **Is the dependency shared?** The attribute is a compile-time dependency. In a project under version control, the
   manifest line has to be committed together with code that uses the attribute, or teammates and CI stop compiling.
   If the user cannot commit the manifest yet, offer a plain `[MenuItem]` instead (see the end).
3. **Where does editor code live?** The package's assembly `Valentinchik.EditorControls.Editor` is editor-only and
   auto-referenced:
   - **No asmdefs** (code in `Assembly-CSharp` / `Assembly-CSharp-Editor`): put the method in any `Editor` folder; or,
     in runtime code, wrap the attribute *and* its `using` in `#if UNITY_EDITOR` so player builds compile.
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
| `Group` | Subfolder of Project Tools and a group in the side panel (*Project Tools › Group*). No group: the tool sits at the top level after the groups. |
| `Order` | Position, lower first, then by label. A group is placed where its lowest `Order` puts it. |

How it reaches the keys: the package rebuilds the list on every domain reload and sends it to the plugin; the
**Project Tools** folder shows groups as subfolders (with a back button inside), and the side panel lists each tool
under *Project Tools › Group*. Keys that hold a single tool keep working across restarts.

## Making a tool keypad-friendly

- **Context comes from the editor**, not parameters: `Selection`, the active scene, `SceneView.lastActiveSceneView`.
- **Undo every scene or asset change** (`Undo.RecordObject`, `Undo.RegisterCreatedObjectUndo`, ...): a key press is
  blind, Ctrl/Cmd+Z must take it back.
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
- **Use the project's own way to reach game state** (its DI container, services, managers). Do not add new statics or
  singletons just for cheats: projects that enter Play Mode without a domain reload keep static state between sessions.
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

1. The project compiles (a fresh domain reload also refreshes the keypad list).
2. The tool shows up in the **Project Tools** folder, in its group, and in the Options+ side panel under
   *Project Tools › Group*. If not: is it static and parameterless, is the attribute inside a `#if` that is off, is the
   package older than the property you used?
3. Pressing it does what it says and logs to the console.

## When a menu item is the better fit

A plain `[MenuItem("MyGame/…")]` needs no package dependency, appears in Unity's menu, and can still go on a key: the
plugin's *Run Menu Item* action lets the user pick any menu item. It does not appear in Project Tools (unless the user
lists that top-level menu in the package's Preferences). A method may carry both attributes.
