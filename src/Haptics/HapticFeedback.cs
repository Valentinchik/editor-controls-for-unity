namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;

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

        // Last state per editor process. Kept while an editor reloads its domain (no session then), so the compile
        // result after the reload and Play Mode entered through a reload are still caught.
        private readonly Dictionary<Int32, Seen> _seen = new();
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

        // Called on every bridge change. Each editor is compared with its own previous state, so switching between
        // projects is not mistaken for a transition. Compilation and test results come from any open editor (you
        // switch away while waiting for them); Play Mode and console errors only from the editor the keys act on.
        public void OnEditorChanged()
        {
            var editors = UnityBridge.Editors;
            var target = UnityBridge.TargetPid;
            var events = new List<String>();

            lock (this._sync)
            {
                foreach (var editor in editors)
                {
                    var now = editor.State;
                    var compiling = editor.CompileReload || now.Compiling;
                    this._seen.TryGetValue(editor.Pid, out var before);
                    this._seen[editor.Pid] = new Seen(now, compiling);
                    if (before == null || (ReferenceEquals(before.State, now) && before.Compiling == compiling))
                    {
                        continue;
                    }

                    if (before.Compiling && !compiling)
                    {
                        events.Add(now.CompileFailed ? CompileFailed : CompileSucceeded);
                        continue; // compile errors also land in the console — one buzz is enough
                    }

                    AddIfFinished(events, before.State.EditModeTests, now.EditModeTests);
                    AddIfFinished(events, before.State.PlayModeTests, now.PlayModeTests);

                    if (editor.Pid != target)
                    {
                        continue;
                    }

                    // PlayMode tests enter and leave Play Mode on their own; the result buzz is the one that matters.
                    if (!before.State.TestsRunning && !now.TestsRunning && before.State.Playing != now.Playing)
                    {
                        events.Add(now.Playing ? PlayModeEntered : PlayModeExited);
                    }

                    if (now.Errors > before.State.Errors && !now.TestsRunning && DateTime.UtcNow - this._lastErrorUtc > ErrorCooldown)
                    {
                        this._lastErrorUtc = DateTime.UtcNow;
                        events.Add(ConsoleError);
                    }
                }
            }

            events.ForEach(this.Raise);
        }

        private static void AddIfFinished(List<String> events, TestRun before, TestRun after)
        {
            if (after.Finished && after.Runs > before.Runs)
            {
                events.Add(after.Failed > 0 ? TestsFailed : TestsPassed);
            }
        }

        private sealed record Seen(EditorState State, Boolean Compiling);
    }
}
