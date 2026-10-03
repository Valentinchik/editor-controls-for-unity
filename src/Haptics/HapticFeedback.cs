namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // Haptic events for MX Master 4. Waveforms are mapped in package/events/extra/eventMapping.yaml and can be
    // changed by the user in Options+. Events come from transitions in the editor state reported by the Unity package.
    internal sealed class HapticFeedback
    {
        public const String CompileSucceeded = "compileSucceeded";
        public const String CompileFailed = "compileFailed";
        public const String PlayModeEntered = "playModeEntered";
        public const String PlayModeExited = "playModeExited";
        public const String ConsoleError = "consoleError";
        public const String TestsPassed = "testsPassed";
        public const String TestsFailed = "testsFailed";
        public const String Test = "testVibration";

        private static readonly TimeSpan ErrorCooldown = TimeSpan.FromSeconds(3);

        private readonly Plugin _plugin;
        private readonly Object _sync = new();
        private EditorView _previousView = EditorView.Disconnected;
        private EditorState _lastState;
        private DateTime _lastErrorUtc = DateTime.MinValue;

        public HapticFeedback(Plugin plugin)
        {
            this._plugin = plugin;
        }

        public void Register()
        {
            var events = this._plugin.PluginEvents;
            events.AddEvent(CompileSucceeded, "Compilation succeeded", "Scripts compiled without errors");
            events.AddEvent(CompileFailed, "Compilation failed", "Scripts have compile errors");
            events.AddEvent(PlayModeEntered, "Entered Play Mode", "The editor entered Play Mode");
            events.AddEvent(PlayModeExited, "Exited Play Mode", "The editor left Play Mode");
            events.AddEvent(ConsoleError, "New console error", "A new error appeared in the Unity console");
            events.AddEvent(TestsPassed, "Tests passed", "A test run finished without failures");
            events.AddEvent(TestsFailed, "Tests failed", "A test run finished with failures");
            events.AddEvent(Test, "Test vibration", "Sent by the Test Vibration action");
        }

        public void Raise(String eventName) => this._plugin.PluginEvents.RaiseEvent(eventName);

        // Called on every bridge change. Compilation is judged view to view; everything else against the last state
        // seen while connected, so transitions across a domain reload (Play Mode, PlayMode tests) are not missed.
        public void OnEditorChanged()
        {
            EditorView previousView, view = UnityBridge.View;
            EditorState last;
            lock (this._sync)
            {
                previousView = this._previousView;
                last = this._lastState;
                this._previousView = view;
                if (view.Connected)
                {
                    this._lastState = view.State;
                }
            }

            if (previousView.Compiling && !view.Compiling && view.Connected)
            {
                this.Raise(view.State.CompileFailed ? CompileFailed : CompileSucceeded);
                return; // compile errors also land in the console — one buzz is enough
            }

            if (!view.Connected || last == null || ReferenceEquals(last, view.State))
            {
                return;
            }

            var now = view.State;
            this.RaiseIfFinished(last.EditModeTests, now.EditModeTests);
            this.RaiseIfFinished(last.PlayModeTests, now.PlayModeTests);

            // PlayMode tests enter and leave Play Mode on their own; the result buzz is the one that matters.
            if (!last.TestsRunning && !now.TestsRunning)
            {
                if (!last.Playing && now.Playing)
                {
                    this.Raise(PlayModeEntered);
                }
                else if (last.Playing && !now.Playing)
                {
                    this.Raise(PlayModeExited);
                }
            }

            if (now.Errors > last.Errors && !now.TestsRunning && DateTime.UtcNow - this._lastErrorUtc > ErrorCooldown)
            {
                this._lastErrorUtc = DateTime.UtcNow;
                this.Raise(ConsoleError);
            }
        }

        private void RaiseIfFinished(TestRun before, TestRun after)
        {
            if (after.Finished && after.Runs > before.Runs)
            {
                this.Raise(after.Failed > 0 ? TestsFailed : TestsPassed);
            }
        }
    }
}
