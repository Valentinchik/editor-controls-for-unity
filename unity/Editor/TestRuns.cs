using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if EDITOR_CONTROLS_TEST_FRAMEWORK
using UnityEditor.TestTools.TestRunner.Api;
#endif

namespace Valentinchik.EditorControls
{
    // Test runs for the plugin's test keys. The Test Framework package is optional: without it the keys stay inactive.
    // Progress is kept in SessionState because PlayMode tests reload the domain mid-run.
    [InitializeOnLoad]
    internal static class TestRuns
    {
        public const string RunEditModeCommand = "Editor Controls/Run EditMode Tests";
        public const string RunPlayModeCommand = "Editor Controls/Run PlayMode Tests";

        private const string EditModeKey = "Valentinchik.EditorControls.Tests.EditMode";
        private const string PlayModeKey = "Valentinchik.EditorControls.Tests.PlayMode";
        private const string CurrentRunKey = "Valentinchik.EditorControls.Tests.CurrentRun";
        private const string RequestedRunKey = "Valentinchik.EditorControls.Tests.Requested";

        public static TestRunState EditMode => Load(EditModeKey);

        public static TestRunState PlayMode => Load(PlayModeKey);

        // Test assemblies found by the last RefreshAssemblies(), for the Preferences page.
        public static IReadOnlyList<TestAssembly> Assemblies { get; private set; } = Array.Empty<TestAssembly>();

#if EDITOR_CONTROLS_TEST_FRAMEWORK
        public static bool Available => true;

        private static readonly TestRunnerApi Api;

        static TestRuns()
        {
            // Registered on every domain load, so results of runs started anywhere (Test Runner window included) arrive.
            Api = ScriptableObject.CreateInstance<TestRunnerApi>();
            Api.RegisterCallbacks(new Callbacks());

            // A slot left "running" without a run in progress (an aborted run) would block the keys until restart.
            if (string.IsNullOrEmpty(SessionState.GetString(CurrentRunKey, null)))
            {
                ClearStaleRun(EditModeKey);
                ClearStaleRun(PlayModeKey);
            }
        }

        public static void Run(bool playMode)
        {
            if (!string.IsNullOrEmpty(SessionState.GetString(CurrentRunKey, null)))
            {
                Debug.LogWarning("[Editor Controls] Tests are already running.");
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Editor Controls] Leave Play Mode to run tests.");
                return;
            }

            var filter = new Filter { testMode = playMode ? TestMode.PlayMode : TestMode.EditMode };
            if (EditorControlsSettings.OnlySelectedTestAssemblies)
            {
                // An empty assemblyNames would mean "every assembly" to the Test Framework.
                var selected = EditorControlsSettings.TestAssemblies;
                if (selected.Count == 0)
                {
                    Debug.LogWarning("[Editor Controls] No test assemblies are selected in Preferences → Editor Controls for Unity.");
                    return;
                }

                filter.assemblyNames = selected.ToArray();
            }

            SessionState.SetString(RequestedRunKey, playMode ? PlayModeKey : EditModeKey);
            Api.Execute(new ExecutionSettings(filter));
        }

        public static void RefreshAssemblies(Action done)
        {
            var found = new List<TestAssembly>();
            var pending = 2;

            void Collect(ITestAdaptor root, bool playMode)
            {
                found.AddRange(AssembliesUnder(root).Select(assembly => new TestAssembly(NameOf(assembly), playMode, assembly.TestCaseCount)));
                if (--pending == 0)
                {
                    Assemblies = found.OrderBy(a => a.PlayMode).ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase).ToList();
                    done?.Invoke();
                }
            }

            Api.RetrieveTestList(TestMode.EditMode, root => Collect(root, false));
            Api.RetrieveTestList(TestMode.PlayMode, root => Collect(root, true));
        }

        private static IEnumerable<ITestAdaptor> AssembliesUnder(ITestAdaptor test)
        {
            if (test == null)
            {
                return Enumerable.Empty<ITestAdaptor>();
            }

            return test.IsTestAssembly ? new[] { test } : test.Children.SelectMany(AssembliesUnder);
        }

        // The tree names assemblies "Name.dll"; the run filter wants the bare assembly name.
        private static string NameOf(ITestAdaptor assembly) =>
            assembly.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? assembly.Name.Substring(0, assembly.Name.Length - 4) : assembly.Name;

        private static void ClearStaleRun(string key)
        {
            var state = Load(key);
            if (state.status == TestRunState.Running)
            {
                state.status = TestRunState.None;
                Store(key, state);
            }
        }

        private static string KeyOf(TestMode mode) => (mode & TestMode.PlayMode) != 0 ? PlayModeKey : EditModeKey;

        // The root of a run has no mode (0); the test assemblies right under it carry it.
        private static TestMode ModeOf(ITestAdaptor test)
        {
            if (test == null || test.TestMode != 0)
            {
                return test?.TestMode ?? 0;
            }

            foreach (var child in test.Children)
            {
                var mode = ModeOf(child);
                if (mode != 0)
                {
                    return mode;
                }
            }

            return 0;
        }

        // Which slot the run in progress reports to. In SessionState because PlayMode tests reload the domain mid-run.
        private sealed class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                var mode = ModeOf(testsToRun);
                var key = mode != 0 ? KeyOf(mode) : SessionState.GetString(RequestedRunKey, EditModeKey);
                SessionState.EraseString(RequestedRunKey);
                SessionState.SetString(CurrentRunKey, key);
                Store(key, new TestRunState { status = TestRunState.Running, runs = Load(key).runs, total = testsToRun.TestCaseCount });
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var key = SessionState.GetString(CurrentRunKey, null) ?? KeyOf(ModeOf(result.Test));
                SessionState.EraseString(CurrentRunKey);
                Store(key, new TestRunState
                {
                    status = TestRunState.Finished,
                    runs = Load(key).runs + 1,
                    passed = result.PassCount,
                    failed = result.FailCount,
                    skipped = result.SkipCount + result.InconclusiveCount,
                    total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount,
                    done = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount,
                });
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.HasChildren)
                {
                    return;
                }

                var key = SessionState.GetString(CurrentRunKey, null);
                if (key == null)
                {
                    return;
                }

                var state = Load(key);
                state.done++;
                switch (result.TestStatus)
                {
                    case TestStatus.Passed:
                        state.passed++;
                        break;
                    case TestStatus.Failed:
                        state.failed++;
                        break;
                    default:
                        state.skipped++;
                        break;
                }

                Store(key, state);
            }
        }
#else
        public static bool Available => false;

        public static void Run(bool playMode)
        {
        }

        public static void RefreshAssemblies(Action done)
        {
        }
#endif

        private static TestRunState Load(string key)
        {
            var json = SessionState.GetString(key, null);
            return string.IsNullOrEmpty(json) ? new TestRunState() : JsonUtility.FromJson<TestRunState>(json);
        }

        private static void Store(string key, TestRunState state) => SessionState.SetString(key, JsonUtility.ToJson(state));
    }

    internal sealed class TestAssembly
    {
        public TestAssembly(string name, bool playMode, int tests)
        {
            Name = name;
            PlayMode = playMode;
            Tests = tests;
        }

        public string Name { get; }
        public bool PlayMode { get; }
        public int Tests { get; }
    }
}
