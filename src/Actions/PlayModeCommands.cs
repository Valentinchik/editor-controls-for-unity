namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class PlayCommand : UnityShortcutCommand
    {
        public PlayCommand() : base("Play / Stop", Groups.PlayMode, UnityShortcuts.Play) { }
    }

    public sealed class PauseCommand : UnityShortcutCommand
    {
        public PauseCommand() : base("Pause", Groups.PlayMode, UnityShortcuts.Pause) { }
    }

    public sealed class StepCommand : UnityShortcutCommand
    {
        public StepCommand() : base("Step", Groups.PlayMode, UnityShortcuts.Step) { }
    }
}
