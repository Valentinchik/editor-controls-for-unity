namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class PlayCommand : LiveShortcutCommand
    {
        public PlayCommand() : base("Play / Stop", Groups.PlayMode, UnityShortcuts.Play) { }

        protected override KeyLook? GetLook(EditorView view) =>
            view.State?.Playing == true ? new KeyLook(KeyRenderer.Active, "player-stop") : null;
    }

    public sealed class PauseCommand : LiveShortcutCommand
    {
        public PauseCommand() : base("Pause", Groups.PlayMode, UnityShortcuts.Pause) { }

        protected override KeyLook? GetLook(EditorView view) =>
            view.State?.Paused == true ? new KeyLook(KeyRenderer.Busy) : null;
    }

    public sealed class StepCommand : UnityShortcutCommand
    {
        public StepCommand() : base("Step", Groups.PlayMode, UnityShortcuts.Step) { }
    }
}
