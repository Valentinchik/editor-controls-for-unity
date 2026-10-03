using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    // Recent and favourite assets for the plugin's folders. Personal, so stored under UserSettings/ (not committed).
    // Recent = assets selected in the Project window or opened in Prefab Mode; favourites are added from the Assets menu.
    [InitializeOnLoad]
    [FilePath("UserSettings/EditorControls/AssetShortcuts.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class AssetShortcuts : ScriptableSingleton<AssetShortcuts>
    {
        private const int MaxRecent = 8;
        private const int MaxFavorites = 32;
        private const string AddMenu = "Assets/Editor Controls/Add to Favorites";
        private const string RemoveMenu = "Assets/Editor Controls/Remove from Favorites";

        [SerializeField] private List<string> recent = new List<string>();
        [SerializeField] private List<string> favorites = new List<string>();

        // An asset opened from the keypad must not jump to the top of Recent while its folder is on screen.
        private static string _openedGuid;
        private static double _openedUntil;
        private static bool _saveScheduled;

        static AssetShortcuts()
        {
            Selection.selectionChanged += OnSelectionChanged;
            PrefabStage.prefabStageOpened += stage => Remember(AssetDatabase.AssetPathToGUID(stage.assetPath));
        }

        public static ListItem[] Recent() => Items(instance.recent);

        public static ListItem[] Favorites() => Items(instance.favorites);

        public static void Open(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                Forget(guid);
                return;
            }

            _openedGuid = guid;
            _openedUntil = EditorApplication.timeSinceStartup + 1;

            if (asset is SceneAsset)
            {
                EditorCatalog.OpenScene(path);
            }
            else if (asset is MonoScript || asset is Shader || IsPrefab(asset, path))
            {
                AssetDatabase.OpenAsset(asset);
            }
            else
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }

        [MenuItem(AddMenu, false, 2000)]
        private static void AddSelectedToFavorites()
        {
            var list = instance.favorites;
            foreach (var guid in Selection.assetGUIDs.Where(guid => !list.Contains(guid)))
            {
                list.Add(guid);
            }

            if (list.Count > MaxFavorites)
            {
                list.RemoveRange(0, list.Count - MaxFavorites);
            }

            Changed(EditorCatalog.FavoritesList);
        }

        [MenuItem(AddMenu, true)]
        private static bool CanAddSelectedToFavorites() => Selection.assetGUIDs.Any(guid => !instance.favorites.Contains(guid));

        [MenuItem(RemoveMenu, false, 2001)]
        private static void RemoveSelectedFromFavorites()
        {
            instance.favorites.RemoveAll(guid => Selection.assetGUIDs.Contains(guid));
            Changed(EditorCatalog.FavoritesList);
        }

        [MenuItem(RemoveMenu, true)]
        private static bool CanRemoveSelectedFromFavorites() => Selection.assetGUIDs.Any(guid => instance.favorites.Contains(guid));

        private static void OnSelectionChanged()
        {
            var asset = Selection.activeObject;
            if (asset == null || !EditorUtility.IsPersistent(asset) || asset is SceneAsset)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(path) && !AssetDatabase.IsValidFolder(path))
            {
                Remember(AssetDatabase.AssetPathToGUID(path));
            }
        }

        private static void Remember(string guid)
        {
            if (string.IsNullOrEmpty(guid) || (guid == _openedGuid && EditorApplication.timeSinceStartup < _openedUntil))
            {
                return;
            }

            var list = instance.recent;
            if (list.Count > 0 && list[0] == guid)
            {
                return;
            }

            list.Remove(guid);
            list.Insert(0, guid);
            if (list.Count > MaxRecent)
            {
                list.RemoveRange(MaxRecent, list.Count - MaxRecent);
            }

            Changed(EditorCatalog.RecentList);
        }

        private static void Forget(string guid)
        {
            instance.recent.Remove(guid);
            instance.favorites.Remove(guid);
            Changed(EditorCatalog.RecentList);
            Changed(EditorCatalog.FavoritesList);
        }

        private static void Changed(string list)
        {
            EditorCatalog.MarkDirty(list);
            if (_saveScheduled)
            {
                return;
            }

            // Selection changes come in bursts; one write per burst is enough.
            _saveScheduled = true;
            EditorApplication.delayCall += () =>
            {
                _saveScheduled = false;
                instance.Save(true);
            };
        }

        private static ListItem[] Items(IEnumerable<string> guids) => guids
            .Select(guid => (guid, path: AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => !string.IsNullOrEmpty(asset.path))
            .Select(asset => new ListItem
            {
                id = asset.guid,
                label = System.IO.Path.GetFileNameWithoutExtension(asset.path),
                icon = IconFor(asset.path),
            })
            .ToArray();

        private static bool IsPrefab(UnityEngine.Object asset, string path) =>
            asset is GameObject && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);

        // Tabler icon names; the plugin draws them.
        private static string IconFor(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return "folder";
            }

            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (type == null)
            {
                return "file-unknown";
            }

            if (type == typeof(SceneAsset)) return "map-pin";
            if (type == typeof(GameObject)) return path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ? "box" : "cube";
            if (typeof(Material).IsAssignableFrom(type)) return "palette";
            if (typeof(Texture).IsAssignableFrom(type) || typeof(Sprite).IsAssignableFrom(type)) return "photo";
            if (typeof(AudioClip).IsAssignableFrom(type)) return "music";
            if (typeof(AnimationClip).IsAssignableFrom(type) || typeof(RuntimeAnimatorController).IsAssignableFrom(type)) return "movie";
            if (type == typeof(MonoScript) || typeof(Shader).IsAssignableFrom(type)) return "file-code";
            if (typeof(ScriptableObject).IsAssignableFrom(type)) return "file-settings";
            return "file-unknown";
        }
    }
}
