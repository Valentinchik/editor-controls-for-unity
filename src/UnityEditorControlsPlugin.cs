namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Threading;

    public class UnityEditorControlsPlugin : Plugin
    {
        private const String PackageHelpUrl = "https://github.com/Valentinchik/editor-controls-for-unity#unity-package";
        private static readonly TimeSpan PackageHintDelay = TimeSpan.FromSeconds(20);

        private Timer _statusTimer;
        private HapticFeedback _haptics;
        private DateTime? _unityWithoutPackageSinceUtc;
        private String _lastStatus;

        // Without the Unity package, actions are keyboard shortcuts, so Unity has to be the frontmost app.
        public override Boolean UsesApplicationApiOnly => false;

        public override Boolean HasNoApplication => false;

        public UnityEditorControlsPlugin()
        {
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
        }

        public override void Load()
        {
            this._haptics = new HapticFeedback(this);
            this._haptics.Register();
            UnityBridge.Changed += this._haptics.OnEditorChanged;
            UnityBridge.Changed += this.UpdateStatus;
            UnityBridge.Start();
            this._statusTimer = new Timer(_ => this.UpdateStatus(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public override void Unload()
        {
            this._statusTimer?.Dispose();
            UnityBridge.Changed -= this.UpdateStatus;
            UnityBridge.Changed -= this._haptics.OnEditorChanged;
            UnityBridge.Stop();
        }

        // Shown in Options+ next to the plugin: which project is connected, or a hint about the optional Unity package.
        private void UpdateStatus()
        {
            var view = UnityBridge.View;
            if (view.Connected || view.Reloading || !this.ClientApplication.IsRunning())
            {
                this._unityWithoutPackageSinceUtc = null;
                var project = UnityBridge.ActiveProject;
                this.SetStatus(Loupedeck.PluginStatus.Normal, project != null ? $"Connected to Unity: {project}" : null);
                return;
            }

            this._unityWithoutPackageSinceUtc ??= DateTime.UtcNow;
            if (DateTime.UtcNow - this._unityWithoutPackageSinceUtc > PackageHintDelay)
            {
                this.SetStatus(Loupedeck.PluginStatus.Warning,
                    "Keys work as Unity shortcuts. Add the com.valentinchik.editor-controls package to your project for live state and background control.",
                    PackageHelpUrl);
            }
        }

        private void SetStatus(Loupedeck.PluginStatus status, String message, String url = null)
        {
            var key = $"{status}|{message}";
            if (key == this._lastStatus)
            {
                return;
            }

            this._lastStatus = key;
            if (url == null)
            {
                this.OnPluginStatusChanged(status, message);
            }
            else
            {
                this.OnPluginStatusChanged(status, message, url, "How to install");
            }
        }
    }
}
