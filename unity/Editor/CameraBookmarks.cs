using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Valentinchik.EditorControls
{
    // Saved Scene View cameras, per scene. Personal, so stored under UserSettings/ (not committed).
    [FilePath("UserSettings/EditorControls/CameraBookmarks.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class CameraBookmarks : ScriptableSingleton<CameraBookmarks>
    {
        public const int Slots = 7;

        [Serializable]
        private class Bookmark
        {
            public int slot;
            public Vector3 pivot;
            public Quaternion rotation;
            public float size;
            public bool orthographic;
            public bool in2DMode;
        }

        [Serializable]
        private class SceneBookmarks
        {
            public string scene;
            public List<Bookmark> bookmarks = new List<Bookmark>();
        }

        [SerializeField] private List<SceneBookmarks> scenes = new List<SceneBookmarks>();

        // Bit n is set when slot n is filled for the active scene.
        public static int Mask()
        {
            var scene = instance.Of(SceneKey(), false);
            if (scene == null)
            {
                return 0;
            }

            var mask = 0;
            foreach (var bookmark in scene.bookmarks)
            {
                mask |= 1 << bookmark.slot;
            }

            return mask;
        }

        public static void Use(int slot, bool save)
        {
            var view = SceneView.lastActiveSceneView;
            if (view == null || slot < 0 || slot >= Slots)
            {
                return;
            }

            var scene = instance.Of(SceneKey(), save);
            var bookmark = scene?.bookmarks.Find(b => b.slot == slot);
            if (save)
            {
                if (bookmark == null)
                {
                    bookmark = new Bookmark { slot = slot };
                    scene.bookmarks.Add(bookmark);
                }

                bookmark.pivot = view.pivot;
                bookmark.rotation = view.rotation;
                bookmark.size = view.size;
                bookmark.orthographic = view.orthographic;
                bookmark.in2DMode = view.in2DMode;
                instance.Save(true);
            }
            else if (bookmark != null)
            {
                if (view.in2DMode != bookmark.in2DMode)
                {
                    view.in2DMode = bookmark.in2DMode;
                }

                view.LookAt(bookmark.pivot, bookmark.rotation, bookmark.size, bookmark.orthographic, false);
            }
        }

        private SceneBookmarks Of(string key, bool create)
        {
            var scene = this.scenes.Find(s => s.scene == key);
            if (scene == null && create)
            {
                scene = new SceneBookmarks { scene = key };
                this.scenes.Add(scene);
            }

            return scene;
        }

        // The GUID survives renames and moves; an unsaved scene shares one set of bookmarks.
        private static string SceneKey()
        {
            var path = SceneManager.GetActiveScene().path;
            return string.IsNullOrEmpty(path) ? "untitled" : AssetDatabase.AssetPathToGUID(path);
        }
    }
}
