namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class ToggleGridSnapCommand : HighlightShortcutCommand
    {
        public ToggleGridSnapCommand() : base("Grid Snap", Groups.GridSnap, UnityShortcuts.ToggleGridSnap) { }

        protected override Boolean IsOn(EditorState state) => state.GridSnap;
    }

    public sealed class PushToGridCommand : UnityShortcutCommand
    {
        public PushToGridCommand() : base("Push To Grid", Groups.GridSnap, UnityShortcuts.PushToGrid) { }
    }

    public sealed class IncreaseGridCommand : UnityShortcutCommand
    {
        public IncreaseGridCommand() : base("Grid Size +", Groups.GridSnap, UnityShortcuts.IncreaseGrid) { }
    }

    public sealed class DecreaseGridCommand : UnityShortcutCommand
    {
        public DecreaseGridCommand() : base("Grid Size −", Groups.GridSnap, UnityShortcuts.DecreaseGrid) { }
    }
}
