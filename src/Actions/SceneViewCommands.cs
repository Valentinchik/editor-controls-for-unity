namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class FrameSelectedCommand : UnityShortcutCommand
    {
        public FrameSelectedCommand() : base("Frame Selected", Groups.SceneView, UnityShortcuts.FrameSelected) { }
    }

    public sealed class LockViewCommand : UnityShortcutCommand
    {
        public LockViewCommand() : base("Lock View", Groups.SceneView, UnityShortcuts.LockView) { }
    }

    public sealed class MaximizeViewCommand : UnityShortcutCommand
    {
        public MaximizeViewCommand() : base("Maximize View", Groups.SceneView, UnityShortcuts.MaximizeView) { }
    }

    public sealed class Toggle2DCommand : UnityShortcutCommand
    {
        public Toggle2DCommand() : base("2D / 3D", Groups.SceneView, UnityShortcuts.Toggle2D) { }
    }

    public sealed class ShadedCommand : UnityShortcutCommand
    {
        public ShadedCommand() : base("Shaded", Groups.SceneView, UnityShortcuts.Shaded) { }
    }

    public sealed class WireframeCommand : UnityShortcutCommand
    {
        public WireframeCommand() : base("Wireframe", Groups.SceneView, UnityShortcuts.Wireframe) { }
    }

    public sealed class ShadedWireframeCommand : UnityShortcutCommand
    {
        public ShadedWireframeCommand() : base("Shaded Wireframe", Groups.SceneView, UnityShortcuts.ShadedWireframe) { }
    }

    public sealed class UnlitCommand : UnityShortcutCommand
    {
        public UnlitCommand() : base("Unlit", Groups.SceneView, UnityShortcuts.Unlit) { }
    }

    public sealed class VertexSnapCommand : UnityShortcutCommand
    {
        public VertexSnapCommand() : base("Vertex Snap", Groups.SceneView, UnityShortcuts.VertexSnap) { }
    }

    public sealed class ToggleOverlaysCommand : UnityShortcutCommand
    {
        public ToggleOverlaysCommand() : base("Overlays", Groups.SceneView, UnityShortcuts.ToggleOverlays) { }
    }

    public sealed class IsolateCommand : UnityShortcutCommand
    {
        public IsolateCommand() : base("Isolate", Groups.SceneView, UnityShortcuts.Isolate) { }
    }

    public sealed class ToggleVisibilityCommand : UnityShortcutCommand
    {
        public ToggleVisibilityCommand() : base("Hide / Show", Groups.SceneView, UnityShortcuts.ToggleVisibility) { }
    }

    public sealed class TogglePickingCommand : UnityShortcutCommand
    {
        public TogglePickingCommand() : base("Picking", Groups.SceneView, UnityShortcuts.TogglePicking) { }
    }
}
