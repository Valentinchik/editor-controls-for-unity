namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class FrameSelectedCommand : UnityShortcutCommand
    {
        public FrameSelectedCommand() : base("Frame Selected", Groups.SceneView, UnityShortcuts.FrameSelected) { }
    }

    public sealed class LockViewCommand : LiveShortcutCommand
    {
        public LockViewCommand() : base("Lock View", Groups.SceneView, UnityShortcuts.LockView) { }

        protected override KeyLook? GetLook(EditorView view) => view.State == null ? null
            : view.State.ViewLocked ? new KeyLook(KeyRenderer.Active) : new KeyLook(KeyRenderer.Normal, "lock-open");
    }

    public sealed class MaximizeViewCommand : UnityShortcutCommand
    {
        public MaximizeViewCommand() : base("Maximize View", Groups.SceneView, UnityShortcuts.MaximizeView) { }
    }

    public sealed class Toggle2DCommand : LiveShortcutCommand
    {
        public Toggle2DCommand() : base("2D / 3D", Groups.SceneView, UnityShortcuts.Toggle2D) { }

        // Shows the current mode like the 2D button of the Scene view.
        protected override KeyLook? GetLook(EditorView view) =>
            view.State?.SceneView2D == true ? new KeyLook(KeyRenderer.Normal, "badge-2d") : null;
    }

    public sealed class ShadedCommand : HighlightShortcutCommand
    {
        public ShadedCommand() : base("Shaded", Groups.SceneView, UnityShortcuts.Shaded) { }

        protected override Boolean IsOn(EditorState state) => state.DrawMode == "Textured";
    }

    public sealed class WireframeCommand : HighlightShortcutCommand
    {
        public WireframeCommand() : base("Wireframe", Groups.SceneView, UnityShortcuts.Wireframe) { }

        protected override Boolean IsOn(EditorState state) => state.DrawMode == "Wireframe";
    }

    public sealed class ShadedWireframeCommand : HighlightShortcutCommand
    {
        public ShadedWireframeCommand() : base("Shaded Wireframe", Groups.SceneView, UnityShortcuts.ShadedWireframe) { }

        protected override Boolean IsOn(EditorState state) => state.DrawMode == "TexturedWire";
    }

    public sealed class UnlitCommand : UnityShortcutCommand
    {
        public UnlitCommand() : base("Unlit", Groups.SceneView, UnityShortcuts.Unlit) { }
    }

    public sealed class VertexSnapCommand : LiveShortcutCommand
    {
        public VertexSnapCommand() : base("Vertex Snap", Groups.SceneView, UnityShortcuts.VertexSnap) { }

        protected override KeyLook? GetLook(EditorView view) => view.State == null ? null
            : view.State.VertexSnap ? new KeyLook(KeyRenderer.Active) : new KeyLook(KeyRenderer.Normal, "magnet-off");
    }

    public sealed class ToggleOverlaysCommand : HighlightShortcutCommand
    {
        public ToggleOverlaysCommand() : base("Overlays", Groups.SceneView, UnityShortcuts.ToggleOverlays) { }

        protected override Boolean IsOn(EditorState state) => state.Overlays;
    }

    public sealed class IsolateCommand : HighlightShortcutCommand
    {
        public IsolateCommand() : base("Isolate", Groups.SceneView, UnityShortcuts.Isolate) { }

        protected override Boolean IsOn(EditorState state) => state.Isolated;
    }

    public sealed class ToggleVisibilityCommand : LiveShortcutCommand
    {
        public ToggleVisibilityCommand() : base("Hide / Show", Groups.SceneView, UnityShortcuts.ToggleVisibility) { }

        // Eye while the selected object is visible in the Scene view, crossed eye while hidden.
        protected override KeyLook? GetLook(EditorView view) =>
            view.State?.SelectionHidden == true ? new KeyLook(KeyRenderer.Normal, "eye-off") : null;
    }

    public sealed class TogglePickingCommand : LiveShortcutCommand
    {
        public TogglePickingCommand() : base("Picking", Groups.SceneView, UnityShortcuts.TogglePicking) { }

        protected override KeyLook? GetLook(EditorView view) =>
            view.State?.SelectionUnpickable == true ? new KeyLook(KeyRenderer.Normal, "hand-finger-off") : null;
    }
}
