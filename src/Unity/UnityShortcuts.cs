namespace Loupedeck.UnityEditorControlsPlugin
{
    using static UnityKeys;
    using static UnityModifiers;

    // Unity 6 default bindings, read from the Shortcut Manager of 6000.6 (Edit → Shortcuts). Action = ⌘ on macOS, Ctrl on Windows.
    // Windows-only differences are passed as windowsDefault; they could not be verified on a Windows machine.
    // Checked against 2022.3 too: all defaults match except the ones marked unity6Only / legacyIds.
    public static class UnityShortcuts
    {
        // Play Mode
        public static readonly UnityShortcut Play = new("Main Menu/Edit/Play Mode/Play", new(P, Action));
        public static readonly UnityShortcut Pause = new("Main Menu/Edit/Play Mode/Pause", new(P, Action | Shift));
        public static readonly UnityShortcut Step = new("Main Menu/Edit/Play Mode/Step", new(P, Action | Alt));

        // Scene View
        public static readonly UnityShortcut FrameSelected = new("Main Menu/Edit/Frame Selected in Window under Cursor", new(F, None));
        public static readonly UnityShortcut LockView = new("Main Menu/Edit/Lock View to Selected", new(F, Shift));
        public static readonly UnityShortcut MaximizeView = new("Window/Maximize View", new(Space, Shift));
        public static readonly UnityShortcut Toggle2D = new("Scene View/Toggle 2D Mode", new(Alpha2, None));
        public static readonly UnityShortcut Shaded = new("Scene View/Render Mode/Shaded", new(Alpha4, Alt), unity6Only: true);
        public static readonly UnityShortcut Wireframe = new("Scene View/Render Mode/Wireframe", new(Alpha1, Alt), unity6Only: true);
        public static readonly UnityShortcut ShadedWireframe = new("Scene View/Render Mode/Shaded Wireframe", new(Alpha2, Alt), unity6Only: true);
        public static readonly UnityShortcut Unlit = new("Scene View/Render Mode/Unlit", new(Alpha3, Alt), unity6Only: true);
        public static readonly UnityShortcut VertexSnap = new("Scene View/Toggle Vertex Snapping", new(V, Shift));
        public static readonly UnityShortcut ToggleOverlays = new("Overlays/Toggle All Overlays", new(BackQuote, Shift));
        public static readonly UnityShortcut Isolate = new("Scene Visibility/Toggle Isolation On Selection And Descendants", new(H, Shift));
        public static readonly UnityShortcut ToggleVisibility = new("Scene Visibility/Toggle Selection And Descendants Visibility", new(H, None));
        public static readonly UnityShortcut TogglePicking = new("Scene Picking/Toggle Picking On Selection And Descendants", new(L, None));

        // Grid & Snap
        public static readonly UnityShortcut ToggleGridSnap = new("Grid and Snap/Toggle Snapping", new(Backslash, None), unity6Only: true);
        public static readonly UnityShortcut PushToGrid = new("Grid and Snap/Push To Grid", new(Backslash, Action), unity6Only: true);
        public static readonly UnityShortcut IncreaseGrid = new("Grid and Snap/Increase Grid Size", new(RightBracket, Action), unity6Only: true);
        public static readonly UnityShortcut DecreaseGrid = new("Grid and Snap/Decrease Grid Size", new(LeftBracket, Action), unity6Only: true);

        // GameObject
        public static readonly UnityShortcut CreateEmpty = new("Main Menu/GameObject/Create Empty", new(N, Action | Shift));
        public static readonly UnityShortcut CreateEmptyChild = new("Main Menu/GameObject/Create Empty Child", new(N, Alt | Shift));
        public static readonly UnityShortcut CreateEmptyParent = new("Main Menu/GameObject/Create Empty Parent", new(G, Action | Shift));
        public static readonly UnityShortcut ToggleActive = new("Main Menu/GameObject/Toggle Active State", new(A, Alt | Shift));
        public static readonly UnityShortcut Duplicate = new("Main Menu/Edit/Duplicate", new(D, Action));
        public static readonly UnityShortcut Delete = new("Main Menu/Edit/Delete", new(UnityKeys.Delete, Action), new(UnityKeys.Delete, None));
        public static readonly UnityShortcut MoveToView = new("Main Menu/GameObject/Move To View", new(F, Action | Alt));
        public static readonly UnityShortcut AlignWithView = new("Main Menu/GameObject/Align With View", new(F, Action | Shift));
        public static readonly UnityShortcut AddComponent = new("Main Menu/Component/Add...", new(A, Action | Shift));
        public static readonly UnityShortcut FirstSibling = new("Main Menu/GameObject/Set as first sibling", new(EqualsKey, Action));
        public static readonly UnityShortcut LastSibling = new("Main Menu/GameObject/Set as last sibling", new(Minus, Action));

        // Selection
        public static readonly UnityShortcut SelectAll = new("Main Menu/Edit/Select All", new(A, Action));
        public static readonly UnityShortcut DeselectAll = new("Main Menu/Edit/Deselect All", new(D, Shift));
        public static readonly UnityShortcut InvertSelection = new("Main Menu/Edit/Invert Selection", new(I, Action));
        public static readonly UnityShortcut SelectChildren = new("Main Menu/Edit/Select Children", new(C, Shift));
        public static readonly UnityShortcut SelectPrefabRoot = new("Main Menu/Edit/Select Prefab Root", new(R, Action | Shift));
        public static readonly UnityShortcut PreviousSelection = new("Main Menu/Edit/Previous Selection", new(LeftBracket, Action | Shift), unity6Only: true);
        public static readonly UnityShortcut NextSelection = new("Main Menu/Edit/Next Selection", new(RightBracket, Action | Shift), unity6Only: true);

        // Tools
        public static readonly UnityShortcut HandTool = new("Tools/View", new(Q, None));
        public static readonly UnityShortcut MoveTool = new("Tools/Move", new(W, None));
        public static readonly UnityShortcut RotateTool = new("Tools/Rotate", new(E, None));
        public static readonly UnityShortcut ScaleTool = new("Tools/Scale", new(R, None));
        public static readonly UnityShortcut RectTool = new("Tools/Rect", new(T, None));
        public static readonly UnityShortcut TransformTool = new("Tools/Transform", new(Y, None));
        public static readonly UnityShortcut PivotPosition = new("Tools/Toggle Pivot Position", new(Z, None));
        public static readonly UnityShortcut PivotOrientation = new("Tools/Toggle Pivot Orientation", new(X, None));

        // Windows
        public static readonly UnityShortcut SceneWindow = new("Main Menu/Window/General/Scene", new(Alpha1, Action));
        public static readonly UnityShortcut GameWindow = new("Main Menu/Window/General/Game", new(Alpha2, Action));
        public static readonly UnityShortcut InspectorWindow = new("Main Menu/Window/General/Inspector", new(Alpha3, Action));
        public static readonly UnityShortcut HierarchyWindow = new("Main Menu/Window/General/Hierarchy", new(Alpha4, Action));
        public static readonly UnityShortcut ProjectWindow = new("Main Menu/Window/General/Project", new(Alpha5, Action));
        public static readonly UnityShortcut ConsoleWindow = new("Main Menu/Window/General/Console", new(C, Action | Shift));
        public static readonly UnityShortcut AnimationWindow = new("Main Menu/Window/Animation/Animation", new(Alpha6, Action));
        public static readonly UnityShortcut ProfilerWindow = new("Main Menu/Window/Analysis/Profiler", new(Alpha7, Action));
        public static readonly UnityShortcut AudioMixerWindow = new("Main Menu/Window/Audio/Audio Mixer", new(Alpha8, Action));
        public static readonly UnityShortcut LightingWindow = new("Main Menu/Window/Rendering/Lighting", new(Alpha9, Action));

        // Unity 6 names this item "Services", but it opens the Package Manager window.
        public static readonly UnityShortcut PackageManagerWindow = new("Main Menu/Window/Package Management/Services", new(Alpha0, Action));
        public static readonly UnityShortcut UndoHistoryWindow = new("Main Menu/Window/General/Undo History", new(U, Action), legacyIds: new[] { "Main Menu/Edit/Undo History" });

        // Edit
        public static readonly UnityShortcut Undo = new("Main Menu/Edit/Undo", new(Z, Action));
        public static readonly UnityShortcut Redo = new("Main Menu/Edit/Redo", new(Z, Action | Shift), new(Y, Action));
        public static readonly UnityShortcut Cut = new("Main Menu/Edit/Cut", new(X, Action));
        public static readonly UnityShortcut Copy = new("Main Menu/Edit/Copy", new(C, Action));
        public static readonly UnityShortcut Paste = new("Main Menu/Edit/Paste", new(V, Action));
        public static readonly UnityShortcut PasteAsChild = new("Main Menu/Edit/Paste Special/Paste as Child (Keep Local Transform)", new(V, Action | Shift), legacyIds: new[] { "Main Menu/Edit/Paste As Child" });
        public static readonly UnityShortcut Save = new("Main Menu/File/Save", new(S, Action));
        public static readonly UnityShortcut Refresh = new("Main Menu/Assets/Refresh", new(R, Action));
        public static readonly UnityShortcut Search = new("Main Menu/Edit/Search/Search All...", new(K, Action));
        public static readonly UnityShortcut Find = new("Main Menu/Edit/Search/Find", new(F, Action));

        // Project
        public static readonly UnityShortcut NewScene = new("Main Menu/File/New Scene", new(N, Action));
        public static readonly UnityShortcut OpenScene = new("Main Menu/File/Open Scene", new(O, Action));
        public static readonly UnityShortcut BuildProfiles = new("Main Menu/File/Build Profiles", new(B, Action | Shift));
        public static readonly UnityShortcut BuildAndRun = new("Main Menu/File/Build And Run", new(B, Action));
        public static readonly UnityShortcut GenerateLighting = new("Main Menu/Edit/Lighting/Generate Lighting", new(L, Action | Shift));
        public static readonly UnityShortcut ProfilerRecord = new("Profiling/Profiler/RecordToggle", new(F9, None));

        // Animation window (work while it has focus)
        public static readonly UnityShortcut AnimationPlay = new("Animation/Play Animation", new(Space, None));
        public static readonly UnityShortcut PreviousFrame = new("Animation/Previous Frame", new(Comma, None));
        public static readonly UnityShortcut NextFrame = new("Animation/Next Frame", new(Period, None));
        public static readonly UnityShortcut PreviousKeyframe = new("Animation/Previous Keyframe", new(Comma, Alt));
        public static readonly UnityShortcut NextKeyframe = new("Animation/Next Keyframe", new(Period, Alt));
        public static readonly UnityShortcut FirstKeyframe = new("Animation/First Keyframe", new(Comma, Shift));
        public static readonly UnityShortcut LastKeyframe = new("Animation/Last Keyframe", new(Period, Shift));
        public static readonly UnityShortcut KeySelected = new("Animation/Key Selected", new(K, None));
        public static readonly UnityShortcut KeyModified = new("Animation/Key Modified", new(K, Shift));

        // Raw navigation keys for Hierarchy / Project lists
        public static readonly UnityShortcut ArrowUp = new(null, new(UpArrow, None));
        public static readonly UnityShortcut ArrowDown = new(null, new(DownArrow, None));
    }
}
