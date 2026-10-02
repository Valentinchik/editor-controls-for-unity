namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // A dial / roller / Actions Ring rotation mapped to a pair of Unity shortcuts: one per tick, by direction.
    public abstract class UnityShortcutAdjustment : PluginDynamicAdjustment
    {
        // A fast spin can report dozens of ticks at once; Unity cannot keep up with that many keystrokes.
        private const Int32 MaxTicksPerEvent = 8;

        private readonly UnityShortcut _decrease;
        private readonly UnityShortcut _increase;

        protected UnityShortcutAdjustment(String displayName, UnityShortcut decrease, UnityShortcut increase)
            : base(displayName: displayName, description: "", groupName: Groups.Dials, hasReset: false)
        {
            this._decrease = decrease;
            this._increase = increase;
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            var shortcut = diff < 0 ? this._decrease : this._increase;
            if (shortcut == null)
            {
                return;
            }

            for (var i = 0; i < Math.Min(Math.Abs(diff), MaxTicksPerEvent); i++)
            {
                shortcut.Send(this.Plugin.ClientApplication);
            }
        }
    }

    public sealed class UndoRedoAdjustment : UnityShortcutAdjustment
    {
        public UndoRedoAdjustment() : base("Undo / Redo", UnityShortcuts.Undo, UnityShortcuts.Redo) { }
    }

    public sealed class AnimationFrameAdjustment : UnityShortcutAdjustment
    {
        public AnimationFrameAdjustment() : base("Animation Frames", UnityShortcuts.PreviousFrame, UnityShortcuts.NextFrame) { }
    }

    public sealed class AnimationKeyframeAdjustment : UnityShortcutAdjustment
    {
        public AnimationKeyframeAdjustment() : base("Animation Keys", UnityShortcuts.PreviousKeyframe, UnityShortcuts.NextKeyframe) { }
    }

    public sealed class SelectionHistoryAdjustment : UnityShortcutAdjustment
    {
        public SelectionHistoryAdjustment() : base("Selection History", UnityShortcuts.PreviousSelection, UnityShortcuts.NextSelection) { }
    }

    public sealed class GridSizeAdjustment : UnityShortcutAdjustment
    {
        public GridSizeAdjustment() : base("Grid Size", UnityShortcuts.DecreaseGrid, UnityShortcuts.IncreaseGrid) { }
    }

    public sealed class NavigateAdjustment : UnityShortcutAdjustment
    {
        public NavigateAdjustment() : base("Navigate List", UnityShortcuts.ArrowUp, UnityShortcuts.ArrowDown) { }
    }

    public sealed class FrameStepAdjustment : UnityShortcutAdjustment
    {
        public FrameStepAdjustment() : base("Frame Step", null, UnityShortcuts.Step) { }
    }

    // Steps through the scene tools in toolbar order. Unity does not report the active tool,
    // so the position is the plugin's own and can drift if the tool is changed by keyboard.
    public sealed class ToolCycleAdjustment : PluginDynamicAdjustment
    {
        private static readonly (String Name, UnityShortcut Shortcut)[] Tools =
        {
            ("Hand", UnityShortcuts.HandTool),
            ("Move", UnityShortcuts.MoveTool),
            ("Rotate", UnityShortcuts.RotateTool),
            ("Scale", UnityShortcuts.ScaleTool),
            ("Rect", UnityShortcuts.RectTool),
            ("Transform", UnityShortcuts.TransformTool),
        };

        private Int32 _index = 1;

        public ToolCycleAdjustment()
            : base(displayName: "Tool", description: "", groupName: Groups.Dials, hasReset: false)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            this._index = ((this._index + Math.Sign(diff)) % Tools.Length + Tools.Length) % Tools.Length;
            Tools[this._index].Shortcut.Send(this.Plugin.ClientApplication);
            this.AdjustmentValueChanged();
        }

        protected override String GetAdjustmentValue(String actionParameter) => Tools[this._index].Name;
    }
}
