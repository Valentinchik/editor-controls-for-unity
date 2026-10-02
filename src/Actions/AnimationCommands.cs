namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class AnimationPlayCommand : UnityShortcutCommand
    {
        public AnimationPlayCommand() : base("Play Animation", Groups.Animation, UnityShortcuts.AnimationPlay) { }
    }

    public sealed class PreviousFrameCommand : UnityShortcutCommand
    {
        public PreviousFrameCommand() : base("Previous Frame", Groups.Animation, UnityShortcuts.PreviousFrame) { }
    }

    public sealed class NextFrameCommand : UnityShortcutCommand
    {
        public NextFrameCommand() : base("Next Frame", Groups.Animation, UnityShortcuts.NextFrame) { }
    }

    public sealed class PreviousKeyframeCommand : UnityShortcutCommand
    {
        public PreviousKeyframeCommand() : base("Previous Key", Groups.Animation, UnityShortcuts.PreviousKeyframe) { }
    }

    public sealed class NextKeyframeCommand : UnityShortcutCommand
    {
        public NextKeyframeCommand() : base("Next Key", Groups.Animation, UnityShortcuts.NextKeyframe) { }
    }

    public sealed class FirstKeyframeCommand : UnityShortcutCommand
    {
        public FirstKeyframeCommand() : base("First Key", Groups.Animation, UnityShortcuts.FirstKeyframe) { }
    }

    public sealed class LastKeyframeCommand : UnityShortcutCommand
    {
        public LastKeyframeCommand() : base("Last Key", Groups.Animation, UnityShortcuts.LastKeyframe) { }
    }

    public sealed class KeySelectedCommand : UnityShortcutCommand
    {
        public KeySelectedCommand() : base("Add Key", Groups.Animation, UnityShortcuts.KeySelected) { }
    }

    public sealed class KeyModifiedCommand : UnityShortcutCommand
    {
        public KeyModifiedCommand() : base("Key Modified", Groups.Animation, UnityShortcuts.KeyModified) { }
    }
}
