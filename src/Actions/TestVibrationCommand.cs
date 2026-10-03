namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // Checks the haptics chain plugin → Options+ → MX Master 4 without Unity.
    public sealed class TestVibrationCommand : PluginDynamicCommand
    {
        public TestVibrationCommand()
            : base(displayName: "Test Vibration", description: "Vibrates an MX Master 4 mouse", groupName: Groups.Status)
        {
        }

        protected override void RunCommand(String actionParameter) => this.Plugin.PluginEvents.RaiseEvent(HapticFeedback.Test);
    }
}
