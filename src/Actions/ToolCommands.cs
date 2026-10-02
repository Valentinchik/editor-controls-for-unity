namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class HandToolCommand : HighlightShortcutCommand
    {
        public HandToolCommand() : base("Hand", Groups.Tools, UnityShortcuts.HandTool) { }

        protected override Boolean IsOn(EditorState state) => state.Tool == "View";
    }

    public sealed class MoveToolCommand : HighlightShortcutCommand
    {
        public MoveToolCommand() : base("Move", Groups.Tools, UnityShortcuts.MoveTool) { }

        protected override Boolean IsOn(EditorState state) => state.Tool == "Move";
    }

    public sealed class RotateToolCommand : HighlightShortcutCommand
    {
        public RotateToolCommand() : base("Rotate", Groups.Tools, UnityShortcuts.RotateTool) { }

        protected override Boolean IsOn(EditorState state) => state.Tool == "Rotate";
    }

    public sealed class ScaleToolCommand : HighlightShortcutCommand
    {
        public ScaleToolCommand() : base("Scale", Groups.Tools, UnityShortcuts.ScaleTool) { }

        protected override Boolean IsOn(EditorState state) => state.Tool == "Scale";
    }

    public sealed class RectToolCommand : HighlightShortcutCommand
    {
        public RectToolCommand() : base("Rect", Groups.Tools, UnityShortcuts.RectTool) { }

        protected override Boolean IsOn(EditorState state) => state.Tool == "Rect";
    }

    public sealed class TransformToolCommand : HighlightShortcutCommand
    {
        public TransformToolCommand() : base("Transform", Groups.Tools, UnityShortcuts.TransformTool) { }

        protected override Boolean IsOn(EditorState state) => state.Tool == "Transform";
    }

    public sealed class PivotPositionCommand : LiveShortcutCommand
    {
        public PivotPositionCommand() : base("Pivot / Center", Groups.Tools, UnityShortcuts.PivotPosition) { }

        // Shows the current handle position like Unity's toolbar: anchor = Pivot, centre mark = Center.
        protected override KeyLook? GetLook(EditorView view) =>
            view.State?.PivotMode == "Center" ? new KeyLook(KeyRenderer.Normal, "focus-centered") : null;
    }

    public sealed class PivotOrientationCommand : LiveShortcutCommand
    {
        public PivotOrientationCommand() : base("Local / Global", Groups.Tools, UnityShortcuts.PivotOrientation) { }

        // Shows the current handle rotation like Unity's toolbar: cube = Local, globe = Global.
        protected override KeyLook? GetLook(EditorView view) =>
            view.State?.PivotRotation == "Local" ? new KeyLook(KeyRenderer.Normal, "cube") : null;
    }
}
