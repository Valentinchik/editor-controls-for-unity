namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class CreateEmptyCommand : UnityShortcutCommand
    {
        public CreateEmptyCommand() : base("Create Empty", Groups.GameObject, UnityShortcuts.CreateEmpty) { }
    }

    public sealed class CreateEmptyChildCommand : UnityShortcutCommand
    {
        public CreateEmptyChildCommand() : base("Empty Child", Groups.GameObject, UnityShortcuts.CreateEmptyChild) { }
    }

    public sealed class CreateEmptyParentCommand : UnityShortcutCommand
    {
        public CreateEmptyParentCommand() : base("Empty Parent", Groups.GameObject, UnityShortcuts.CreateEmptyParent) { }
    }

    public sealed class ToggleActiveCommand : UnityShortcutCommand
    {
        public ToggleActiveCommand() : base("Toggle Active", Groups.GameObject, UnityShortcuts.ToggleActive) { }
    }

    public sealed class DuplicateCommand : UnityShortcutCommand
    {
        public DuplicateCommand() : base("Duplicate", Groups.GameObject, UnityShortcuts.Duplicate) { }
    }

    public sealed class DeleteCommand : UnityShortcutCommand
    {
        public DeleteCommand() : base("Delete", Groups.GameObject, UnityShortcuts.Delete) { }
    }

    public sealed class MoveToViewCommand : UnityShortcutCommand
    {
        public MoveToViewCommand() : base("Move To View", Groups.GameObject, UnityShortcuts.MoveToView) { }
    }

    public sealed class AlignWithViewCommand : UnityShortcutCommand
    {
        public AlignWithViewCommand() : base("Align With View", Groups.GameObject, UnityShortcuts.AlignWithView) { }
    }

    public sealed class AddComponentCommand : UnityShortcutCommand
    {
        public AddComponentCommand() : base("Add Component", Groups.GameObject, UnityShortcuts.AddComponent) { }
    }

    public sealed class FirstSiblingCommand : UnityShortcutCommand
    {
        public FirstSiblingCommand() : base("First Sibling", Groups.GameObject, UnityShortcuts.FirstSibling) { }
    }

    public sealed class LastSiblingCommand : UnityShortcutCommand
    {
        public LastSiblingCommand() : base("Last Sibling", Groups.GameObject, UnityShortcuts.LastSibling) { }
    }
}
