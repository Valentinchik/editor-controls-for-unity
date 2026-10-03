namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Threading;

    // Runs a test mode through the Unity Test Framework: progress while running, then passed / (passed + failed)
    // in green or red for a moment, then the normal key again. Needs the Unity package and the Test Framework package.
    public abstract class RunTestsCommand : BridgeCommand
    {
        private static readonly TimeSpan ResultTime = TimeSpan.FromSeconds(2);

        private readonly String _commandId;
        private readonly Boolean _playMode;
        private readonly Object _sync = new();
        private Int32? _seenRuns; // null until the first state: a result from before that is not news
        private TestRun _result;
        private DateTime _resultUntilUtc;
        private TestRun _logged;
        private Timer _revert;

        protected RunTestsCommand(String displayName, String commandId, Boolean playMode)
            : base(displayName, "Runs the project's tests with the Unity Test Framework", Groups.Tests)
        {
            this._commandId = commandId;
            this._playMode = playMode;
        }

        // Subscribed before the base redraw handler, so the redraw already sees the updated result.
        protected override Boolean OnLoad()
        {
            UnityBridge.Changed += this.OnTestsChanged;
            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            UnityBridge.Changed -= this.OnTestsChanged;
            this._revert?.Dispose();
            return base.OnUnload();
        }

        // Not blocked while a result is on the key — only Unity refuses a second run while one is in progress.
        protected override void Run() => UnityBridge.TrySend(this._commandId);

        protected override KeyLook? GetLook(EditorView view)
        {
            if (!view.Connected)
            {
                return null;
            }

            var run = this.RunOf(view.State);
            if (run.Running)
            {
                return new KeyLook(KeyRenderer.Busy, "loader-2", $"{run.Done}/{run.Total}");
            }

            TestRun result;
            lock (this._sync)
            {
                result = DateTime.UtcNow < this._resultUntilUtc ? this._result : null;
            }

            if (result == null)
            {
                return null;
            }

            var score = $"{result.Passed}/{result.Passed + result.Failed}";
            return result.Failed > 0
                ? new KeyLook(KeyRenderer.Failed, "circle-x", score)
                : new KeyLook(KeyRenderer.Good, "circle-check", score);
        }

        private TestRun RunOf(EditorState state) => this._playMode ? state.PlayModeTests : state.EditModeTests;

        private void OnTestsChanged()
        {
            var state = UnityBridge.View.State;
            if (state == null)
            {
                return;
            }

            var run = this.RunOf(state);
            lock (this._sync)
            {
                if (this._logged == null || this._logged.Status != run.Status || this._logged.Runs != run.Runs)
                {
                    this._logged = run;
                    PluginLog.Verbose($"{(this._playMode ? "PlayMode" : "EditMode")} tests: status {run.Status}, run {run.Runs}, " +
                        $"{run.Done}/{run.Total} done, {run.Passed} passed, {run.Failed} failed");
                }

                var isNew = this._seenRuns != null && this._seenRuns != run.Runs;
                this._seenRuns = run.Runs;
                if (!isNew || !run.Finished)
                {
                    return;
                }

                this._result = run;
                this._resultUntilUtc = DateTime.UtcNow + ResultTime;
                this._revert?.Dispose();
                this._revert = new Timer(_ => this.ActionImageChanged(), null, ResultTime + TimeSpan.FromMilliseconds(50), Timeout.InfiniteTimeSpan);
            }
        }
    }

    // Ids must match TestRuns.RunEditModeCommand / RunPlayModeCommand in the Unity package.
    public sealed class RunEditModeTestsCommand : RunTestsCommand
    {
        public RunEditModeTestsCommand() : base("Run EditMode Tests", "Editor Controls/Run EditMode Tests", false) { }
    }

    public sealed class RunPlayModeTestsCommand : RunTestsCommand
    {
        public RunPlayModeTestsCommand() : base("Run PlayMode Tests", "Editor Controls/Run PlayMode Tests", true) { }
    }
}
