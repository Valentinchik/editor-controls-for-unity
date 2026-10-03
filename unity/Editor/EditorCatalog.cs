using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    // The lists behind the plugin's dynamic folders. Each list is rebuilt only when marked dirty
    // and sent only when its content changed.
    internal static class EditorCatalog
    {
        public const string ScenesList = "scenes";
        public const string ToolsList = "tools";
        public const string LayoutsList = "layouts";
        public const string RecentList = "recent";
        public const string FavoritesList = "favorites";
        public const string MenusList = "menus";

        private const int MaxScenes = 100;
        private const int MaxTools = 100;
        private const string MethodPrefix = "method:";
        private const string MenuPrefix = "menu:";

        // " %#t", " _g", " &k" — the hotkey suffix of a [MenuItem] path, not part of the menu name.
        private static readonly Regex HotkeySuffix = new Regex(@"\s+[%#&_^]\S*$");
        private static readonly string[] BuiltInMenuRoots = { "File", "Edit", "Assets", "GameObject", "Component", "Window", "Help" };
        private static readonly Dictionary<string, MethodInfo> Methods = new Dictionary<string, MethodInfo>();

        private static readonly Dictionary<string, Func<ListItem[]>> Builders = new Dictionary<string, Func<ListItem[]>>
        {
            [ScenesList] = Scenes,
            [ToolsList] = Tools,
            [LayoutsList] = WindowLayouts.List,
            [RecentList] = AssetShortcuts.Recent,
            [FavoritesList] = AssetShortcuts.Favorites,
        };

        private static readonly HashSet<string> Dirty = new HashSet<string>(Builders.Keys);
        private static readonly Dictionary<string, string> LastSent = new Dictionary<string, string>();

        public static void MarkDirty(string list) => Dirty.Add(list);

        // After a (re)connect the plugin has nothing, so every list goes out again.
        public static void ResendAll()
        {
            LastSent.Clear();
            Dirty.UnionWith(Builders.Keys);
        }

        // JSON of the lists that changed since they were last sent.
        public static List<string> TakeChanged()
        {
            var changed = new List<string>();
            foreach (var name in Dirty.ToList())
            {
                var json = JsonUtility.ToJson(new ListMessage { name = name, items = Builders[name]() });
                if (!LastSent.TryGetValue(name, out var last) || last != json)
                {
                    LastSent[name] = json;
                    changed.Add(json);
                }
            }

            Dirty.Clear();
            return changed;
        }

        // Every menu item, for the plugin's "Run Menu Item" action. Sent on request only — it is large.
        public static string Menus()
        {
            var roots = BuiltInMenuRoots.Concat(TypeCache.GetMethodsWithAttribute<MenuItem>()
                    .SelectMany(method => method.GetCustomAttributes<MenuItem>())
                    .Select(item => item.menuItem.Split('/')[0])
                    .Where(root => root != "CONTEXT" && !root.StartsWith("internal:", StringComparison.Ordinal)))
                .Distinct();

            var items = roots
                .SelectMany(root => Unsupported.GetSubmenus(root) ?? Array.Empty<string>())
                .Distinct()
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => new ListItem { id = path, label = path })
                .ToArray();

            return JsonUtility.ToJson(new ListMessage { name = MenusList, items = items });
        }

        public static void OpenScene(string path)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Editor Controls] Leave Play Mode to open another scene.");
                return;
            }

            if (File.Exists(path) && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
            }
        }

        public static void RunTool(string id)
        {
            if (id.StartsWith(MenuPrefix, StringComparison.Ordinal))
            {
                if (!EditorApplication.ExecuteMenuItem(id.Substring(MenuPrefix.Length)))
                {
                    Debug.LogWarning($"[Editor Controls] Menu item '{id.Substring(MenuPrefix.Length)}' was not found or is disabled.");
                }
            }
            else if (Methods.TryGetValue(id, out var method))
            {
                method.Invoke(null, null);
            }
        }

        private static ListItem[] Scenes()
        {
            var inBuild = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToList();
            var others = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !inBuild.Contains(path))
                .OrderBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase);

            return inBuild.Concat(others)
                .Take(MaxScenes)
                .Select(path => new ListItem { id = path, label = Path.GetFileNameWithoutExtension(path) })
                .ToArray();
        }

        private static ListItem[] Tools()
        {
            Methods.Clear();
            var tools = new List<ListItem>();

            foreach (var method in TypeCache.GetMethodsWithAttribute<EditorControlsActionAttribute>())
            {
                if (!method.IsStatic || method.GetParameters().Length > 0)
                {
                    continue;
                }

                var attribute = method.GetCustomAttribute<EditorControlsActionAttribute>();
                var id = $"{MethodPrefix}{method.DeclaringType?.FullName}.{method.Name}";
                Methods[id] = method;
                tools.Add(new ListItem { id = id, label = attribute.Label ?? ObjectNames.NicifyVariableName(method.Name), icon = attribute.Icon });
            }

            var roots = EditorControlsSettings.MenuRoots;
            var menuPaths = TypeCache.GetMethodsWithAttribute<MenuItem>()
                .SelectMany(method => method.GetCustomAttributes<MenuItem>())
                .Where(item => !item.validate)
                .Select(item => HotkeySuffix.Replace(item.menuItem, ""))
                .Where(path => roots.Any(root => path.StartsWith(root + "/", StringComparison.Ordinal)))
                .Distinct()
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

            tools.AddRange(menuPaths.Select(path => new ListItem { id = MenuPrefix + path, label = path.Substring(path.LastIndexOf('/') + 1) }));
            return tools.Take(MaxTools).ToArray();
        }
    }
}
