namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class UndoCommand : UnityShortcutCommand
    {
        public UndoCommand() : base("Undo", Groups.Edit, UnityShortcuts.Undo) { }
    }

    public sealed class RedoCommand : UnityShortcutCommand
    {
        public RedoCommand() : base("Redo", Groups.Edit, UnityShortcuts.Redo) { }
    }

    public sealed class CutCommand : UnityShortcutCommand
    {
        public CutCommand() : base("Cut", Groups.Edit, UnityShortcuts.Cut) { }
    }

    public sealed class CopyCommand : UnityShortcutCommand
    {
        public CopyCommand() : base("Copy", Groups.Edit, UnityShortcuts.Copy) { }
    }

    public sealed class PasteCommand : UnityShortcutCommand
    {
        public PasteCommand() : base("Paste", Groups.Edit, UnityShortcuts.Paste) { }
    }

    public sealed class PasteAsChildCommand : UnityShortcutCommand
    {
        public PasteAsChildCommand() : base("Paste As Child", Groups.Edit, UnityShortcuts.PasteAsChild) { }
    }

    public sealed class SaveCommand : UnityShortcutCommand
    {
        public SaveCommand() : base("Save", Groups.Edit, UnityShortcuts.Save) { }
    }

    public sealed class RefreshCommand : UnityShortcutCommand
    {
        public RefreshCommand() : base("Refresh", Groups.Edit, UnityShortcuts.Refresh) { }
    }

    public sealed class SearchCommand : UnityShortcutCommand
    {
        public SearchCommand() : base("Search", Groups.Edit, UnityShortcuts.Search) { }
    }

    public sealed class FindCommand : UnityShortcutCommand
    {
        public FindCommand() : base("Find", Groups.Edit, UnityShortcuts.Find) { }
    }
}
