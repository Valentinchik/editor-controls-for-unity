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

        // Top-level menus whose items appear in the Project Tools folder.
        public static IReadOnlyList<string> MenuRoots =>
            EditorPrefs.GetString(MenuRootsKey, DefaultMenuRoots)
                .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(root => root.Trim().Trim('/'))
                .Where(root => root.Length > 0)
                .ToList();

        [SettingsProvider]
        private static SettingsProvider CreateProvider() => new SettingsProvider("Preferences/Editor Controls for Unity", SettingsScope.User)
        {
            keywords = new[] { "Logitech", "Logi", "Options+", "MX", "Keypad", "Project Tools" },
            guiHandler = _ =>
            {
                EditorGUILayout.LabelField("Project Tools folder", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Menu items under these top-level menus appear in the Project Tools folder of the Logi Options+ plugin, " +
                    "together with methods marked [EditorControlsAction]. One menu per line, e.g. \"Tools\" or \"MyGame\".",
                    MessageType.None);

                var current = string.Join("\n", MenuRoots);
                var edited = EditorGUILayout.TextArea(current, GUILayout.MinHeight(60));
                if (edited != current)
                {
                    EditorPrefs.SetString(MenuRootsKey, edited);
                    Changed?.Invoke();
                }
            },
        };
    }
}
