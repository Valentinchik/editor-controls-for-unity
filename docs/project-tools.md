# Project tools on your keys

With the Unity package installed, any static method marked `[EditorControlsAction]` becomes a key. Use it for the
editor tools and cheats you reach for all day: they show up in the plugin's **Project Tools** folder and, one by one,
in the Logi Options+ side panel.

- [Requirements](#requirements)
- [The attribute](#the-attribute)
- [Groups and order](#groups-and-order)
- [Where tools appear](#where-tools-appear)
- [Editor code, runtime code and asmdefs](#editor-code-runtime-code-and-asmdefs)
- [Cheats](#cheats)
- [Good keypad tools](#good-keypad-tools)
- [Menus instead of the attribute](#menus-instead-of-the-attribute)
- [Test keys](#test-keys)
- [Working with AI coding agents](#working-with-ai-coding-agents)

## Requirements

- The plugin from [Releases](https://github.com/Valentinchik/editor-controls-for-unity/releases) and the Unity package
  `https://github.com/Valentinchik/editor-controls-for-unity.git?path=/unity#v0.6.1` (Unity 2022.3+).
- `Group` needs package 0.6.0+, `Order` 0.5.3+.
- The attribute is a compile-time dependency: in a team project, commit the `Packages/manifest.json` line together
  with the code that uses it.

## The attribute

```csharp
using Valentinchik.EditorControls;

public static class LevelTools
{
    [EditorControlsAction("Snap To Ground", Icon = "magnet", Group = "Level Design", Order = 1)]
    private static void SnapToGround() { /* ... */ }
}
```

| Property | Meaning |
|---|---|
| `Label` (constructor) | Text on the key. Defaults to the method name, nicified. |
| `Icon` | Name of a [Tabler](https://tabler.io/icons) outline icon the plugin ships — see the [list](../skills/editor-controls-actions/references/icons.md). Unknown names show a tool glyph. |
| `Group` | Puts the tool into a subfolder of Project Tools and a group of the side panel. |
| `Order` | Position: lower first, then by label. |

The method must be `static` and take no parameters; anything else is skipped. It runs on the editor main thread.

## Groups and order

- Tools with a `Group` are collected into subfolders. A subfolder sits where the lowest `Order` among its tools puts
  it; groups with equal orders sort by name.
- Tools without a group follow the groups at the top level of the folder.
- Inside a group, the first key — *‹ Group name* — goes back up.

## Where tools appear

| Where | How |
|---|---|
| **Project Tools** folder (Folders group of the side panel) | One key opens all your tools; groups are subfolders. Put it on a Keypad page or in the Actions Ring. |
| **Side panel → Project Tools → Group → tool** | Each tool as its own action — drag the ones you use most straight onto keys. |

The list comes from the Unity project in front (or the one you used last). The plugin remembers the tools it has seen,
so keys keep their names and icons while Unity is closed, and the side panel keeps the entries; a project's entries
are refreshed whenever it reports its list.

A key bound to a single tool remembers it by `Namespace.Type.Method`. Renaming or moving the method, its class or its
namespace disconnects that key — bind it again from the side panel. Changing the label, icon, group or order is safe.

## Editor code, runtime code and asmdefs

The package's assembly is editor-only and referenced automatically by Unity's predefined assemblies.

- **Projects without asmdefs:** put tools in any `Editor` folder. A cheat may also live in runtime code next to the
  gameplay it touches — wrap the attribute and its `using` in `#if UNITY_EDITOR`, so player builds compile.
- **Projects with asmdefs:** put tools in an editor-only asmdef (`includePlatforms: ["Editor"]`) that references
  `Valentinchik.EditorControls.Editor` and the runtime assemblies it drives.

## Cheats

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

        // Find the running game through your DI container / services and change it; log what happened.
    }
}
```

Check `Application.isPlaying` first and reach game state the way your project already does. With *Enter Play Mode
Options* static fields survive from one Play session to the next, so a cheat's own state (say, the time scale before a
pause) needs a reset in `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` — or use
the *Clean Play* key to start from a fresh domain.

## Good keypad tools

- A short label: it has to fit on a key.
- Context from the editor (`Selection`, the active scene, the Scene View), not parameters.
- Undo for every scene or asset change. Each key press is its own undo step (the package opens an undo group named
  after the tool), so one Ctrl/Cmd+Z takes the whole press back.
- A console message saying what happened — or why nothing did (the keypad cannot grey a key out the way a menu
  validator does).
- No modal dialogs on the main path.

## Menus instead of the attribute

Every menu item can go on a key without any code: the plugin's **Run Menu Item** action lets you pick one from a list
of all menu items of the open project. To list whole menus in Project Tools as well, type their top-level names in
*Preferences → Editor Controls for Unity* (empty by default). A method can carry both `[MenuItem]` and
`[EditorControlsAction]`.

## Test keys

*Run EditMode Tests* and *Run PlayMode Tests* run the project's tests through the Unity Test Framework and show
progress, then `passed / total` in green or red; MX Master 4 vibrates on the result. To leave out tests bundled with
plugins, open *Preferences → Editor Controls for Unity → Run Tests keys*, turn on *Run only the selected test
assemblies* and tick your own.

## Working with AI coding agents

The repository ships a skill that teaches coding agents (Claude Code and others that read `SKILL.md` skills) how to
add keypad tools, cheats and tests the right way: [`skills/editor-controls-actions`](../skills/editor-controls-actions/SKILL.md).

To use it with Claude Code, copy the folder into your Unity project:

```
<your project>/.claude/skills/editor-controls-actions/
```

Then ask for things like *"add a cheat that refills ammo to my keypad, in the Cheats group"* — the agent follows the
rules above: correct attribute and placement, Play Mode guard, undo, console messages, no package-dependency surprises.

## License

The code examples on this page, the Unity package and the agent skill are MIT-licensed — see [unity/LICENSE.md](../unity/LICENSE.md).
