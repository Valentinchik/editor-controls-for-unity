namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
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

        // About the active GameObject: null when nothing is selected.
        public Boolean? SelectionActive { get; private init; }
        public Boolean? SelectionHidden { get; private init; }
        public Boolean? SelectionUnpickable { get; private init; }

        public static EditorState Parse(JsonElement json) => new()
        {
            Focused = Bool(json, "focused"),
            Playing = Bool(json, "playing"),
            Paused = Bool(json, "paused"),
            Compiling = Bool(json, "compiling"),
            CompileFailed = Bool(json, "compileFailed"),
            Errors = json.TryGetProperty("errors", out var errors) ? errors.GetInt32() : 0,
            Warnings = json.TryGetProperty("warnings", out var warnings) ? warnings.GetInt32() : 0,
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
            SelectionActive = TriState(json, "selectionActive"),
            SelectionHidden = TriState(json, "selectionHidden"),
            SelectionUnpickable = TriState(json, "selectionUnpickable"),
        };

        private static Boolean Bool(JsonElement json, String name) => json.TryGetProperty(name, out var value) && value.GetBoolean();

        private static String Text(JsonElement json, String name) => json.TryGetProperty(name, out var value) ? value.GetString() : null;

        // -1 / 0 / 1 on the wire (JsonUtility has no nullable booleans).
        private static Boolean? TriState(JsonElement json, String name) =>
            json.TryGetProperty(name, out var value) && value.GetInt32() >= 0 ? value.GetInt32() == 1 : null;
    }

    // What the keys draw from: the focused editor's state, or a reload in progress, or nothing (no Unity package).
    public sealed class EditorView
    {
        public static readonly EditorView Disconnected = new(null, false);
        public static readonly EditorView ReloadingDomain = new(null, true);

        public EditorView(EditorState state, Boolean reloading)
        {
            this.State = state;
            this.Reloading = reloading;
        }

        public EditorState State { get; }

        // Between "about to reload the domain" and the package reconnecting — scripts are compiling.
        public Boolean Reloading { get; }

        public Boolean Connected => this.State != null;
    }
}
