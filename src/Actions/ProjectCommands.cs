namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class NewSceneCommand : UnityShortcutCommand
    {
        public NewSceneCommand() : base("New Scene", Groups.Project, UnityShortcuts.NewScene) { }
    }

    public sealed class OpenSceneCommand : UnityShortcutCommand
    {
        public OpenSceneCommand() : base("Open Scene", Groups.Project, UnityShortcuts.OpenScene) { }
    }

    public sealed class BuildProfilesCommand : UnityShortcutCommand
    {
        public BuildProfilesCommand() : base("Build Profiles", Groups.Project, UnityShortcuts.BuildProfiles) { }
    }

    public sealed class BuildAndRunCommand : UnityShortcutCommand
    {
        public BuildAndRunCommand() : base("Build And Run", Groups.Project, UnityShortcuts.BuildAndRun) { }
    }

    public sealed class GenerateLightingCommand : UnityShortcutCommand
    {
        public GenerateLightingCommand() : base("Generate Lighting", Groups.Project, UnityShortcuts.GenerateLighting) { }
    }

    public sealed class ProfilerRecordCommand : LiveShortcutCommand
    {
        public ProfilerRecordCommand() : base("Profiler Record", Groups.Project, UnityShortcuts.ProfilerRecord) { }

        protected override KeyLook? GetLook(EditorView view) =>
            view.State?.ProfilerRecording == true ? new KeyLook(KeyRenderer.Failed) : null;
    }
}
