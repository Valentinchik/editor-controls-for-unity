namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    public abstract class UnityShortcutCommand : PluginDynamicCommand
    {
        private readonly UnityShortcut _shortcut;

        protected UnityShortcutCommand(String displayName, String groupName, UnityShortcut shortcut)
            : base(displayName: displayName, description: shortcut.Description, groupName: groupName)
        {
            this._shortcut = shortcut;
        }

        protected override void RunCommand(String actionParameter) => this._shortcut.Send(this.Plugin.ClientApplication);
    }

    internal static class Groups
    {
        public const String PlayMode = "Play Mode";
        public const String SceneView = "Scene View";
        public const String GridSnap = "Grid & Snap";
        public const String GameObject = "GameObject";
        public const String Selection = "Selection";
        public const String Tools = "Tools";
        public const String Windows = "Windows";
        public const String Edit = "Edit";
        public const String Project = "Project";
        public const String Animation = "Animation";
        public const String Dials = "Dials";
    }
}
