namespace Loupedeck.UnityEditorControlsPlugin
{
    public sealed class SceneWindowCommand : UnityShortcutCommand
    {
        public SceneWindowCommand() : base("Scene", Groups.Windows, UnityShortcuts.SceneWindow) { }
    }

    public sealed class GameWindowCommand : UnityShortcutCommand
    {
        public GameWindowCommand() : base("Game", Groups.Windows, UnityShortcuts.GameWindow) { }
    }

    public sealed class InspectorWindowCommand : UnityShortcutCommand
    {
        public InspectorWindowCommand() : base("Inspector", Groups.Windows, UnityShortcuts.InspectorWindow) { }
    }

    public sealed class HierarchyWindowCommand : UnityShortcutCommand
    {
        public HierarchyWindowCommand() : base("Hierarchy", Groups.Windows, UnityShortcuts.HierarchyWindow) { }
    }

    public sealed class ProjectWindowCommand : UnityShortcutCommand
    {
        public ProjectWindowCommand() : base("Project", Groups.Windows, UnityShortcuts.ProjectWindow) { }
    }

    public sealed class ConsoleWindowCommand : UnityShortcutCommand
    {
        public ConsoleWindowCommand() : base("Console", Groups.Windows, UnityShortcuts.ConsoleWindow) { }
    }

    public sealed class AnimationWindowCommand : UnityShortcutCommand
    {
        public AnimationWindowCommand() : base("Animation", Groups.Windows, UnityShortcuts.AnimationWindow) { }
    }

    public sealed class ProfilerWindowCommand : UnityShortcutCommand
    {
        public ProfilerWindowCommand() : base("Profiler", Groups.Windows, UnityShortcuts.ProfilerWindow) { }
    }

    public sealed class AudioMixerWindowCommand : UnityShortcutCommand
    {
        public AudioMixerWindowCommand() : base("Audio Mixer", Groups.Windows, UnityShortcuts.AudioMixerWindow) { }
    }

    public sealed class LightingWindowCommand : UnityShortcutCommand
    {
        public LightingWindowCommand() : base("Lighting", Groups.Windows, UnityShortcuts.LightingWindow) { }
    }

    public sealed class PackageManagerWindowCommand : UnityShortcutCommand
    {
        public PackageManagerWindowCommand() : base("Package Manager", Groups.Windows, UnityShortcuts.PackageManagerWindow) { }
    }

    public sealed class UndoHistoryWindowCommand : UnityShortcutCommand
    {
        public UndoHistoryWindowCommand() : base("Undo History", Groups.Windows, UnityShortcuts.UndoHistoryWindow) { }
    }
}
