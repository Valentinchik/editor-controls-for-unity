namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    public class UnityEditorControlsPlugin : Plugin
    {
        // Actions are keyboard shortcuts, so Unity has to be the frontmost app when they run.
        public override Boolean UsesApplicationApiOnly => false;

        public override Boolean HasNoApplication => false;

        public UnityEditorControlsPlugin()
        {
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
        }

        public override void Load()
        {
            this.ClientApplication.ApplicationActivated += this.OnUnityActivated;
            this.ClientApplication.ApplicationDeactivated += this.OnUnityDeactivated;
        }

        public override void Unload()
        {
            this.ClientApplication.ApplicationActivated -= this.OnUnityActivated;
            this.ClientApplication.ApplicationDeactivated -= this.OnUnityDeactivated;
        }

        private void OnUnityActivated(Object sender, EventArgs e) => PluginLog.Verbose("Unity Editor activated");

        private void OnUnityDeactivated(Object sender, EventArgs e) => PluginLog.Verbose("Unity Editor deactivated");
    }
}
