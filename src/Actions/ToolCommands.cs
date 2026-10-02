namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class HandToolCommand : UnityShortcutCommand
    {
        public HandToolCommand() : base("Hand", Groups.Tools, UnityShortcuts.HandTool) { }
    }

    public sealed class MoveToolCommand : UnityShortcutCommand
    {
        public MoveToolCommand() : base("Move", Groups.Tools, UnityShortcuts.MoveTool) { }
    }

    public sealed class RotateToolCommand : UnityShortcutCommand
    {
        public RotateToolCommand() : base("Rotate", Groups.Tools, UnityShortcuts.RotateTool) { }
    }

    public sealed class ScaleToolCommand : UnityShortcutCommand
    {
        public ScaleToolCommand() : base("Scale", Groups.Tools, UnityShortcuts.ScaleTool) { }
    }

    public sealed class RectToolCommand : UnityShortcutCommand
    {
        public RectToolCommand() : base("Rect", Groups.Tools, UnityShortcuts.RectTool) { }
    }

    public sealed class TransformToolCommand : UnityShortcutCommand
    {
        public TransformToolCommand() : base("Transform", Groups.Tools, UnityShortcuts.TransformTool) { }
    }

    public sealed class PivotPositionCommand : UnityShortcutCommand
    {
        public PivotPositionCommand() : base("Pivot / Center", Groups.Tools, UnityShortcuts.PivotPosition) { }
    }

    public sealed class PivotOrientationCommand : UnityShortcutCommand
    {
        public PivotOrientationCommand() : base("Local / Global", Groups.Tools, UnityShortcuts.PivotOrientation) { }
    }
}
