using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    // Preferences → Editor Controls for Unity. Stored per project in EditorPrefs (personal, not committed).
    internal static class EditorControlsSettings
    {
        private const string DefaultMenuRoots = "Tools";

        public static event Action Changed;

        private static string MenuRootsKey => "Valentinchik.EditorControls.MenuRoots." + PlayerSettings.productGUID;
        private static string OnlySelectedTestsKey => "Valentinchik.EditorControls.OnlySelectedTests." + PlayerSettings.productGUID;
        private static string TestAssembliesKey => "Valentinchik.EditorControls.TestAssemblies." + PlayerSettings.productGUID;

        // Top-level menus whose items appear in the Project Tools folder.
        public static IReadOnlyList<string> MenuRoots => Lines(EditorPrefs.GetString(MenuRootsKey, DefaultMenuRoots))
            .Select(root => root.Trim('/'))
            .Where(root => root.Length > 0)
            .ToList();

        // When on, the Run Tests keys run only TestAssemblies — e.g. to leave out tests that ship with plugins.
        public static bool OnlySelectedTestAssemblies
        {
            get => EditorPrefs.GetBool(OnlySelectedTestsKey, false);
            private set => EditorPrefs.SetBool(OnlySelectedTestsKey, value);
        }

        public static IReadOnlyList<string> TestAssemblies => Lines(EditorPrefs.GetString(TestAssembliesKey, ""));

        [SettingsProvider]
        private static SettingsProvider CreateProvider()
        {
            SettingsProvider provider = null;
            provider = new SettingsProvider("Preferences/Editor Controls for Unity", SettingsScope.User)
            {
                keywords = new[] { "Logitech", "Logi", "Options+", "MX", "Keypad", "Project Tools", "Tests" },
                activateHandler = (_, __) => TestRuns.RefreshAssemblies(() => provider.Repaint()),
                guiHandler = _ =>
                {
                    DrawProjectTools();
                    EditorGUILayout.Space(12);
                    DrawTests();
                },
            };
            return provider;
        }

        private static void DrawProjectTools()
        {
            EditorGUILayout.LabelField("Project Tools folder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Menu items under these top-level menus appear in the Project Tools folder of the Logi Options+ plugin, " +
                "together with methods marked [EditorControlsAction]. One menu per line, e.g. \"Tools\" or \"MyGame\".",
                MessageType.None);

            // The raw text, not the parsed list: re-joining the list would swallow a newline the moment it is typed.
            var current = EditorPrefs.GetString(MenuRootsKey, DefaultMenuRoots);
            var edited = EditorGUILayout.TextArea(current, GUILayout.MinHeight(60));
            if (edited != current)
            {
                EditorPrefs.SetString(MenuRootsKey, edited);
                Changed?.Invoke();
            }
        }

        private static void DrawTests()
        {
            EditorGUILayout.LabelField("Run Tests keys", EditorStyles.boldLabel);
            if (!TestRuns.Available)
            {
                EditorGUILayout.HelpBox("The Run Tests keys need the Test Framework package (com.unity.test-framework).", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(
                "By default the keys run every EditMode / PlayMode test in the project, including tests that come with plugins. " +
                "Turn on the filter and tick the assemblies with your own tests.",
                MessageType.None);

            var only = EditorGUILayout.ToggleLeft("Run only the selected test assemblies", OnlySelectedTestAssemblies);
            if (only != OnlySelectedTestAssemblies)
            {
                OnlySelectedTestAssemblies = only;
            }

            var selected = new HashSet<string>(TestAssemblies, StringComparer.OrdinalIgnoreCase);
            var changed = false;
            using (new EditorGUI.DisabledScope(!only))
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var assembly in TestRuns.Assemblies)
                {
                    var label = $"{assembly.Name}   ({(assembly.PlayMode ? "PlayMode" : "EditMode")}, {assembly.Tests} tests)";
                    changed |= Toggle(selected, assembly.Name, label);
                }

                // Selected earlier but gone now (renamed, deleted or not compiled yet) — still shown so it can be unticked.
                foreach (var name in selected.Where(name => TestRuns.Assemblies.All(a => !a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))).ToList())
                {
                    changed |= Toggle(selected, name, $"{name}   (not found)");
                }

                if (TestRuns.Assemblies.Count == 0)
                {
                    EditorGUILayout.LabelField("No test assemblies found yet.", EditorStyles.miniLabel);
                }
            }

            if (changed)
            {
                EditorPrefs.SetString(TestAssembliesKey, string.Join("\n", selected.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)));
            }

            if (GUILayout.Button("Refresh list", GUILayout.Width(110)))
            {
                TestRuns.RefreshAssemblies(SettingsService.RepaintAllSettingsWindow);
            }
        }

        private static bool Toggle(HashSet<string> selected, string name, string label)
        {
            var on = selected.Contains(name);
            if (EditorGUILayout.ToggleLeft(label, on) == on)
            {
                return false;
            }

            if (on)
            {
                selected.Remove(name);
            }
            else
            {
                selected.Add(name);
            }

            return true;
        }

        private static IReadOnlyList<string> Lines(string text) => text
            .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();
    }
}
