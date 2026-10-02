namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class SelectAllCommand : UnityShortcutCommand
    {
        public SelectAllCommand() : base("Select All", Groups.Selection, UnityShortcuts.SelectAll) { }
    }

    public sealed class DeselectAllCommand : UnityShortcutCommand
    {
        public DeselectAllCommand() : base("Deselect All", Groups.Selection, UnityShortcuts.DeselectAll) { }
    }

    public sealed class InvertSelectionCommand : UnityShortcutCommand
    {
        public InvertSelectionCommand() : base("Invert Selection", Groups.Selection, UnityShortcuts.InvertSelection) { }
    }

    public sealed class SelectChildrenCommand : UnityShortcutCommand
    {
        public SelectChildrenCommand() : base("Select Children", Groups.Selection, UnityShortcuts.SelectChildren) { }
    }

    public sealed class SelectPrefabRootCommand : UnityShortcutCommand
    {
        public SelectPrefabRootCommand() : base("Prefab Root", Groups.Selection, UnityShortcuts.SelectPrefabRoot) { }
    }

    public sealed class PreviousSelectionCommand : UnityShortcutCommand
    {
        public PreviousSelectionCommand() : base("Previous Selection", Groups.Selection, UnityShortcuts.PreviousSelection) { }
    }

    public sealed class NextSelectionCommand : UnityShortcutCommand
    {
        public NextSelectionCommand() : base("Next Selection", Groups.Selection, UnityShortcuts.NextSelection) { }
    }
}
