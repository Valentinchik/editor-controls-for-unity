namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Globalization;
    using System.Text.Json;

    // Live editor state as reported by the Unity package (StateMessage on the Unity side).
    public sealed class EditorState
    {
        public Boolean Focused { get; private init; }
        public Boolean Playing { get; private init; }
        public Boolean Paused { get; private init; }
        public Boolean Compiling { get; private init; }
        public Boolean CompileFailed { get; private init; }
        public Int32 Errors { get; private init; }
        public Int32 Warnings { get; private init; }
        public String Tool { get; private init; }
        public String PivotMode { get; private init; }
        public String PivotRotation { get; private init; }
        public Boolean SceneView2D { get; private init; }
        public String DrawMode { get; private init; }
        public Boolean VertexSnap { get; private init; }
        public Boolean GridSnap { get; private init; }
        public Boolean Isolated { get; private init; }
        public Boolean ViewLocked { get; private init; }
        public Boolean Overlays { get; private init; }
        public Boolean ProfilerRecording { get; private init; }
        public String ActiveScene { get; private init; }
        public Single TimeScale { get; private init; } = 1;
        public String Layout { get; private init; }

        // Bit n is set when camera bookmark n exists for the active scene.
        public Int32 Bookmarks { get; private init; }

        public TestRun EditModeTests { get; private init; } = TestRun.None;
        public TestRun PlayModeTests { get; private init; } = TestRun.None;

        // About the active GameObject: null when nothing is selected.
        public Boolean? SelectionActive { get; private init; }
        public Boolean? SelectionHidden { get; private init; }
        public Boolean? SelectionUnpickable { get; private init; }

        public Boolean TestsRunning => this.EditModeTests.Running || this.PlayModeTests.Running;

        public static EditorState Parse(JsonElement json) => new()
        {
            Focused = Bool(json, "focused"),
            Playing = Bool(json, "playing"),
            Paused = Bool(json, "paused"),
            Compiling = Bool(json, "compiling"),
            CompileFailed = Bool(json, "compileFailed"),
            Errors = Int(json, "errors"),
            Warnings = Int(json, "warnings"),
            Tool = Text(json, "tool"),
            PivotMode = Text(json, "pivotMode"),
            PivotRotation = Text(json, "pivotRotation"),
            SceneView2D = Bool(json, "sceneView2D"),
            DrawMode = Text(json, "drawMode"),
            VertexSnap = Bool(json, "vertexSnap"),
            GridSnap = Bool(json, "gridSnap"),
            Isolated = Bool(json, "isolated"),
            ViewLocked = Bool(json, "viewLocked"),
            Overlays = Bool(json, "overlays"),
            ProfilerRecording = Bool(json, "profilerRecording"),
            ActiveScene = Text(json, "activeScene"),
            TimeScale = json.TryGetProperty("timeScale", out var timeScale) ? timeScale.GetSingle() : 1,
            Layout = Text(json, "layout"),
            Bookmarks = Int(json, "bookmarks"),
            EditModeTests = TestRun.Parse(json, "editModeTests"),
            PlayModeTests = TestRun.Parse(json, "playModeTests"),
            SelectionActive = TriState(json, "selectionActive"),
            SelectionHidden = TriState(json, "selectionHidden"),
            SelectionUnpickable = TriState(json, "selectionUnpickable"),
        };

        // "0.25×", "1×", "1.5×" — the same text on every key and dial that shows the time scale.
        public static String FormatTimeScale(Single value) => value.ToString("0.##", CultureInfo.InvariantCulture) + "×";

        internal static Boolean Bool(JsonElement json, String name) => json.TryGetProperty(name, out var value) && value.GetBoolean();

        internal static Int32 Int(JsonElement json, String name) => json.TryGetProperty(name, out var value) ? value.GetInt32() : 0;

        private static String Text(JsonElement json, String name) => json.TryGetProperty(name, out var value) ? value.GetString() : null;

        // -1 / 0 / 1 on the wire (JsonUtility has no nullable booleans).
        private static Boolean? TriState(JsonElement json, String name) =>
            json.TryGetProperty(name, out var value) && value.GetInt32() >= 0 ? value.GetInt32() == 1 : null;
    }

    // The last EditMode or PlayMode test run (TestRunState on the Unity side).
    public sealed record TestRun(Int32 Status, Int32 Runs, Int32 Total, Int32 Done, Int32 Passed, Int32 Failed)
    {
        public static readonly TestRun None = new(0, 0, 0, 0, 0, 0);

        public Boolean Running => this.Status == 1;
        public Boolean Finished => this.Status == 2;

        public static TestRun Parse(JsonElement json, String name) =>
            json.TryGetProperty(name, out var run) && run.ValueKind == JsonValueKind.Object
                ? new TestRun(EditorState.Int(run, "status"), EditorState.Int(run, "runs"), EditorState.Int(run, "total"), EditorState.Int(run, "done"),
                    EditorState.Int(run, "passed"), EditorState.Int(run, "failed"))
                : None;
    }

    // What the keys draw from: the focused editor's state, or a reload in progress, or nothing (no Unity package).
    public sealed class EditorView
    {
        public static readonly EditorView Disconnected = new(null, false, false);

        public EditorView(EditorState state, Boolean reloading, Boolean reloadingForPlayMode)
        {
            this.State = state;
            this.Reloading = reloading;
            this.ReloadingForPlayMode = reloading && reloadingForPlayMode;
        }

        public EditorState State { get; }

        // Between "about to reload the domain" and the package reconnecting.
        public Boolean Reloading { get; }

        // Entering Play Mode reloads the domain as well; that one is not a compilation.
        public Boolean ReloadingForPlayMode { get; }

        // Scripts are compiling, or the domain reloads with freshly compiled code.
        public Boolean Compiling => (this.Reloading && !this.ReloadingForPlayMode) || this.State?.Compiling == true;

        public Boolean Connected => this.State != null;
    }
}

namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    // The lists the Unity package sends for the dynamic folders and the Run Menu Item editor. Immutable: a new list
    // replaces the old one, so folders can tell "changed" by reference.
    public sealed class EditorCatalog
    {
        public const String Scenes = "scenes";
        public const String Tools = "tools";
        public const String Layouts = "layouts";
        public const String Recent = "recent";
        public const String Favorites = "favorites";
        public const String Menus = "menus";

        public static readonly EditorCatalog Empty = new(new Dictionary<String, IReadOnlyList<CatalogItem>>());

        private readonly IReadOnlyDictionary<String, IReadOnlyList<CatalogItem>> _lists;

        private EditorCatalog(IReadOnlyDictionary<String, IReadOnlyList<CatalogItem>> lists)
        {
            this._lists = lists;
        }

        public IReadOnlyList<CatalogItem> Get(String name) =>
            this._lists.TryGetValue(name, out var items) ? items : Array.Empty<CatalogItem>();

        public EditorCatalog With(String name, IReadOnlyList<CatalogItem> items) =>
            new(new Dictionary<String, IReadOnlyList<CatalogItem>>(this._lists) { [name] = items });

        // A "list" message: { name, items: [{ id, label, icon }] }.
        public static (String Name, IReadOnlyList<CatalogItem> Items) ParseList(JsonElement json)
        {
            var name = json.GetProperty("name").GetString();
            IReadOnlyList<CatalogItem> items = json.TryGetProperty("items", out var array) && array.ValueKind == JsonValueKind.Array
                ? array.EnumerateArray().Select(item => new CatalogItem(
                    item.GetProperty("id").GetString(),
                    item.GetProperty("label").GetString(),
                    item.TryGetProperty("icon", out var icon) && icon.GetString() is { Length: > 0 } iconName ? iconName : null)).ToList()
                : Array.Empty<CatalogItem>();
            return (name, items);
        }
    }

    // Id is what the Unity package needs back (scene path, tool id, layout file, asset GUID, menu path);
    // Icon is an optional Tabler name.
    public sealed record CatalogItem(String Id, String Label, String Icon);
}
