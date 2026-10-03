using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    // Plugin actions the bridge runs directly, keyed by the Unity Shortcut Manager id the plugin uses.
    // Only ids listed in SupportedIds() are routed here; everything else stays a keyboard shortcut.
    internal static class EditorCommands
    {
        // Internal, but present in every supported version; lets a menu path differ between Unity versions without console errors.
        private static readonly MethodInfo MenuItemExists =
            typeof(Menu).GetMethod("MenuItemExists", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly Dictionary<string, Func<bool>> Commands = new Dictionary<string, Func<bool>>();

        static EditorCommands()
        {
            // Play Mode
            Add("Main Menu/Edit/Play Mode/Play", () => { EditorApplication.isPlaying = !EditorApplication.isPlaying; return true; });
            Add("Main Menu/Edit/Play Mode/Pause", () => { EditorApplication.isPaused = !EditorApplication.isPaused; return true; });
            Add("Main Menu/Edit/Play Mode/Step", () => { EditorApplication.Step(); return true; });

            // Scene View
            Add("Main Menu/Edit/Frame Selected in Window under Cursor", () => SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.FrameSelected());
            AddMenu("Main Menu/Edit/Lock View to Selected", "Edit/Lock View to Selected");
            Add("Scene View/Toggle 2D Mode", () => WithSceneView(view => view.in2DMode = !view.in2DMode));
            Add("Scene View/Render Mode/Shaded", () => SetDrawMode(DrawCameraMode.Textured));
            Add("Scene View/Render Mode/Wireframe", () => SetDrawMode(DrawCameraMode.Wireframe));
            Add("Scene View/Render Mode/Shaded Wireframe", () => SetDrawMode(DrawCameraMode.TexturedWire));
            Add("Scene Visibility/Toggle Selection And Descendants Visibility", () => ForEachSelected(go => SceneVisibilityManager.instance.ToggleVisibility(go, true)));
            Add("Scene Picking/Toggle Picking On Selection And Descendants", () => ForEachSelected(go => SceneVisibilityManager.instance.TogglePicking(go, true)));
            Add("Scene Visibility/Toggle Isolation On Selection And Descendants", ToggleIsolation);

            // Grid & Snap
            Add("Grid and Snap/Toggle Snapping", () => { EditorSnapSettings.gridSnapEnabled = !EditorSnapSettings.gridSnapEnabled; return true; });

            // GameObject
            AddMenu("Main Menu/GameObject/Create Empty", "GameObject/Create Empty");
            AddMenu("Main Menu/GameObject/Create Empty Child", "GameObject/Create Empty Child");
            AddMenu("Main Menu/GameObject/Create Empty Parent", "GameObject/Create Empty Parent");
            AddMenu("Main Menu/GameObject/Toggle Active State", "GameObject/Toggle Active State");
            AddMenu("Main Menu/GameObject/Move To View", "GameObject/Move To View");
            AddMenu("Main Menu/GameObject/Align With View", "GameObject/Align With View");
            AddMenu("Main Menu/GameObject/Set as first sibling", "GameObject/Set as first sibling");
            AddMenu("Main Menu/GameObject/Set as last sibling", "GameObject/Set as last sibling");
            AddMenu("Main Menu/Edit/Duplicate", "Edit/Duplicate");
            AddMenu("Main Menu/Edit/Delete", "Edit/Delete");

            // Selection
            AddMenu("Main Menu/Edit/Select All", "Edit/Select All");
            AddMenu("Main Menu/Edit/Deselect All", "Edit/Deselect All");
            AddMenu("Main Menu/Edit/Invert Selection", "Edit/Invert Selection");
            AddMenu("Main Menu/Edit/Select Children", "Edit/Select Children");
            AddMenu("Main Menu/Edit/Select Prefab Root", "Edit/Select Prefab Root");
            AddMenu("Main Menu/Edit/Previous Selection", "Edit/Previous Selection");
            AddMenu("Main Menu/Edit/Next Selection", "Edit/Next Selection");

            // Tools
            Add("Tools/View", () => SetTool(Tool.View));
            Add("Tools/Move", () => SetTool(Tool.Move));
            Add("Tools/Rotate", () => SetTool(Tool.Rotate));
            Add("Tools/Scale", () => SetTool(Tool.Scale));
            Add("Tools/Rect", () => SetTool(Tool.Rect));
            Add("Tools/Transform", () => SetTool(Tool.Transform));
            Add("Tools/Toggle Pivot Position", () => { Tools.pivotMode = Tools.pivotMode == PivotMode.Pivot ? PivotMode.Center : PivotMode.Pivot; return true; });
            Add("Tools/Toggle Pivot Orientation", () => { Tools.pivotRotation = Tools.pivotRotation == PivotRotation.Local ? PivotRotation.Global : PivotRotation.Local; return true; });

            // Windows
            AddMenu("Main Menu/Window/General/Scene", "Window/General/Scene");
            AddMenu("Main Menu/Window/General/Game", "Window/General/Game");
            AddMenu("Main Menu/Window/General/Inspector", "Window/General/Inspector");
            AddMenu("Main Menu/Window/General/Hierarchy", "Window/General/Hierarchy");
            AddMenu("Main Menu/Window/General/Project", "Window/General/Project");
            AddMenu("Main Menu/Window/General/Console", "Window/General/Console");
            AddMenu("Main Menu/Window/Animation/Animation", "Window/Animation/Animation");
            AddMenu("Main Menu/Window/Analysis/Profiler", "Window/Analysis/Profiler");
            AddMenu("Main Menu/Window/Audio/Audio Mixer", "Window/Audio/Audio Mixer");
            AddMenu("Main Menu/Window/Rendering/Lighting", "Window/Rendering/Lighting");
            AddMenu("Main Menu/Window/Package Management/Services", "Window/Package Management/Package Manager", "Window/Package Manager");
            AddMenu("Main Menu/Window/General/Undo History", "Window/General/Undo History", "Edit/Undo History");

            // Edit
            Add("Main Menu/Edit/Undo", () => { Undo.PerformUndo(); return true; });
            Add("Main Menu/Edit/Redo", () => { Undo.PerformRedo(); return true; });
            AddMenu("Main Menu/File/Save", "File/Save");
            Add("Main Menu/Assets/Refresh", () => { AssetDatabase.Refresh(); return true; });
            AddMenu("Main Menu/Edit/Search/Search All...", "Edit/Search/Search All...", "Edit/Search All...");

            // Project
            AddMenu("Main Menu/File/New Scene", "File/New Scene");
            AddMenu("Main Menu/File/Open Scene", "File/Open Scene");
            AddMenu("Main Menu/File/Build Profiles", "File/Build Profiles", "File/Build Settings...");
            AddMenu("Main Menu/File/Build And Run", "File/Build And Run");
            Add("Main Menu/Edit/Lighting/Generate Lighting", () => Lightmapping.BakeAsync());
            Add("Profiling/Profiler/RecordToggle", () => { ProfilerDriver.enabled = !ProfilerDriver.enabled; return true; });

            // Bridge-only actions (no Unity shortcut behind them); listed only when this editor can run them.
            if (TestRuns.Available)
            {
                Add(TestRuns.RunEditModeCommand, () => { TestRuns.Run(false); return true; });
                Add(TestRuns.RunPlayModeCommand, () => { TestRuns.Run(true); return true; });
            }
        }

        public static string[] SupportedIds() => Commands.Keys.ToArray();

        public static void Run(string id)
        {
            if (id == null || !Commands.TryGetValue(id, out var command))
            {
                return;
            }

            try
            {
                command();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Editor Controls] '{id}' failed: {ex.Message}");
            }
        }

        private static void Add(string id, Func<bool> command) => Commands[id] = command;

        // Registers the first menu path that exists in this Unity version; none existing leaves the id to the keyboard shortcut.
        private static void AddMenu(string id, params string[] menuPaths)
        {
            var path = menuPaths.FirstOrDefault(MenuExists);
            if (path != null)
            {
                Commands[id] = () => EditorApplication.ExecuteMenuItem(path);
            }
        }

        private static bool MenuExists(string path)
        {
            if (MenuItemExists == null)
            {
                return true;
            }

            try
            {
                return (bool)MenuItemExists.Invoke(null, new object[] { path });
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static bool WithSceneView(Action<SceneView> action)
        {
            var view = SceneView.lastActiveSceneView;
            if (view == null)
            {
                return false;
            }

            action(view);
            view.Repaint();
            return true;
        }

        private static bool SetDrawMode(DrawCameraMode mode) =>
            WithSceneView(view => view.cameraMode = SceneView.GetBuiltinCameraMode(mode));

        private static bool SetTool(Tool tool)
        {
            Tools.current = tool;
            return true;
        }

        private static bool ForEachSelected(Action<GameObject> action)
        {
            foreach (var go in Selection.gameObjects)
            {
                action(go);
            }

            return Selection.gameObjects.Length > 0;
        }

        private static bool ToggleIsolation()
        {
            var visibility = SceneVisibilityManager.instance;
            if (visibility.IsCurrentStageIsolated())
            {
                visibility.ExitIsolation();
            }
            else
            {
                visibility.Isolate(Selection.gameObjects, true);
            }

            return true;
        }
    }
}
