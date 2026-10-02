namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // Links the plugin to the Unity Editor, so Options+ offers the plugin's default profile when Unity is added.
    // Every Unity 6.x / 202x editor shares this bundle id.
    public class UnityEditorControlsApplication : ClientApplication
    {
        private Boolean? _installed;

        protected override String GetProcessName() => "Unity";

        protected override String GetBundleName() => UnityEditorLocator.BundleId;

        // Options+ lists the app under "Supported applications" only when this reports Installed.
        public override ClientApplicationStatus GetApplicationStatus() =>
            (this._installed ??= UnityEditorLocator.IsInstalled()) || this.IsRunning()
                ? ClientApplicationStatus.Installed
                : ClientApplicationStatus.NotInstalled;
    }
}
