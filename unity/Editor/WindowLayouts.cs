using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    // Window → Layouts: Unity's own layouts plus the ones the user saved, all kept in the preferences folder.
    internal static class WindowLayouts
    {
        private const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        // Internal; on Toolbar in 2022.3, on WindowLayout in Unity 6.
        private static readonly PropertyInfo LastLoadedName =
            typeof(EditorWindow).Assembly.GetType("UnityEditor.Toolbar")?.GetProperty("lastLoadedLayoutName", AnyStatic)
            ?? typeof(EditorWindow).Assembly.GetType("UnityEditor.WindowLayout")?.GetProperty("lastLoadedLayoutName", AnyStatic);

        private static string Folder => Path.Combine(InternalEditorUtility.unityPreferencesFolder, "Layouts", "default");

        public static string Current
        {
            get
            {
                try
                {
                    return LastLoadedName?.GetValue(null) as string;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static ListItem[] List()
        {
            if (!Directory.Exists(Folder))
            {
                return Array.Empty<ListItem>();
            }

            return Directory.GetFiles(Folder, "*.wlt")
                .OrderBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
                .Select(path => new ListItem { id = path, label = Path.GetFileNameWithoutExtension(path) })
                .ToArray();
        }

        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                EditorCatalog.MarkDirty(EditorCatalog.LayoutsList);
                return;
            }

            // Loading a layout rebuilds every window; doing it from inside the bridge's update tick is not safe.
            EditorApplication.delayCall += () =>
            {
                if (!EditorUtility.LoadWindowLayout(path))
                {
                    Debug.LogWarning($"[Editor Controls] Could not load the layout '{Path.GetFileNameWithoutExtension(path)}'.");
                }
            };
        }
    }
}
