namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // Time.timeScale in Play Mode. Unity has no shortcuts for it, so these keys need the Unity package.
    public abstract class TimeScalePresetCommand : BridgeCommand
    {
        private readonly Single _value;

        protected TimeScalePresetCommand(Single value)
            : base($"Time Scale {EditorState.FormatTimeScale(value)}", "Sets Time.timeScale in Play Mode", Groups.PlayMode)
        {
            this._value = value;
        }

        protected override void Run() => UnityBridge.TrySend(new { type = "timeScale", value = this._value });

        protected override KeyLook? GetLook(EditorView view)
        {
            if (!view.Connected)
            {
                return null;
            }

            var current = view.State.Playing && Math.Abs(view.State.TimeScale - this._value) < 0.001f;
            return new KeyLook(current ? KeyRenderer.Active : KeyRenderer.Normal, Label: EditorState.FormatTimeScale(this._value));
        }
    }

    public sealed class TimeScaleQuarterCommand : TimeScalePresetCommand
    {
        public TimeScaleQuarterCommand() : base(0.25f) { }
    }

    public sealed class TimeScaleNormalCommand : TimeScalePresetCommand
    {
        public TimeScaleNormalCommand() : base(1f) { }
    }

    public sealed class TimeScaleDoubleCommand : TimeScalePresetCommand
    {
        public TimeScaleDoubleCommand() : base(2f) { }
    }

    // One step along 0.05 · 0.1 · 0.25 · 0.5 · 0.75 · 1 · 1.5 · 2 · 4 · 8; in Play Mode the label shows the current value.
    public abstract class TimeScaleStepCommand : BridgeCommand
    {
        private readonly Int32 _direction;

        protected TimeScaleStepCommand(String displayName, Int32 direction)
            : base(displayName, "Changes Time.timeScale in Play Mode by one step", Groups.PlayMode)
        {
            this._direction = direction;
        }

        protected override void Run() => UnityBridge.TrySend(new { type = "timeScaleStep", index = this._direction });

        protected override KeyLook? GetLook(EditorView view) => view.State?.Playing == true
            ? new KeyLook(Math.Abs(view.State.TimeScale - 1) < 0.001f ? KeyRenderer.Normal : KeyRenderer.Active,
                Label: EditorState.FormatTimeScale(view.State.TimeScale))
            : null;
    }

    public sealed class TimeScaleSlowerCommand : TimeScaleStepCommand
    {
        public TimeScaleSlowerCommand() : base("Slow Down", -1) { }
    }

    public sealed class TimeScaleFasterCommand : TimeScaleStepCommand
    {
        public TimeScaleFasterCommand() : base("Speed Up", 1) { }
    }

    // Dial: one step per tick, press to reset to 1×.
    public sealed class TimeScaleAdjustment : PluginDynamicAdjustment
    {
        private const Int32 MaxStepsPerEvent = 3;

        public TimeScaleAdjustment()
            : base(displayName: "Time Scale", description: "Changes Time.timeScale in Play Mode; press to reset to 1×", groupName: Groups.Dials, hasReset: true)
        {
        }

        protected override Boolean OnLoad()
        {
            UnityBridge.Changed += this.OnBridgeChanged;
            return true;
        }

        protected override Boolean OnUnload()
        {
            UnityBridge.Changed -= this.OnBridgeChanged;
            return true;
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            for (var i = 0; i < Math.Min(Math.Abs(diff), MaxStepsPerEvent); i++)
            {
                UnityBridge.TrySend(new { type = "timeScaleStep", index = Math.Sign(diff) });
            }
        }

        protected override void RunCommand(String actionParameter) => UnityBridge.TrySend(new { type = "timeScale", value = 1f });

        protected override String GetAdjustmentValue(String actionParameter) =>
            UnityBridge.View.State is { } state ? EditorState.FormatTimeScale(state.TimeScale) : null;

        private void OnBridgeChanged() => this.AdjustmentValueChanged();
    }
}
