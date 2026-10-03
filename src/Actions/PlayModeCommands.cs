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

    // For projects that enter Play Mode without a domain reload. Unity has no shortcuts for these; ids must match
    // DomainReload.ReloadCommand / CleanPlayCommand in the Unity package.
    public sealed class ReloadDomainCommand : BridgeCommand
    {
        public ReloadDomainCommand() : base("Reload Domain", "Reloads scripts without recompiling, resetting static state", Groups.PlayMode) { }

        protected override void Run() => UnityBridge.TrySend("Editor Controls/Reload Domain");

        protected override KeyLook? GetLook(EditorView view) => view.Compiling ? new KeyLook(KeyRenderer.Busy, "loader-2") : null;
    }

    public sealed class CleanPlayCommand : BridgeCommand
    {
        public CleanPlayCommand() : base("Clean Play", "Reloads the domain, then enters Play Mode; restarts the game if it is running", Groups.PlayMode) { }

        protected override void Run() => UnityBridge.TrySend("Editor Controls/Clean Play");

        protected override KeyLook? GetLook(EditorView view) => view.ReloadingForPlayMode
            ? new KeyLook(KeyRenderer.Busy, "loader-2")
            : view.State?.Playing == true ? new KeyLook(KeyRenderer.Active) : null;
    }
}
