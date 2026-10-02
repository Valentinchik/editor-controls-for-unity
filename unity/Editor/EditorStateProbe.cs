using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    internal static class EditorStateProbe
    {
        private const BindingFlags AnyMember = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        // Internal members, present in 2022.3 and 6; each read falls back to a neutral value if one disappears.
        private static readonly MethodInfo GetCountsByType =
            typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries")?.GetMethod("GetCountsByType", BindingFlags.Public | BindingFlags.Static);
        private static readonly FieldInfo VertexDragging = typeof(Tools).GetField("vertexDragging", AnyMember);
        private static readonly PropertyInfo ViewLockedToObject = typeof(SceneView).GetProperty("viewIsLockedToObject", AnyMember);
        private static readonly PropertyInfo OverlayCanvas = typeof(EditorWindow).GetProperty("overlayCanvas", AnyMember);
        private static PropertyInfo _overlaysEnabled;

        public static StateMessage Capture()
        {
            var view = SceneView.lastActiveSceneView;
            var selected = Selection.activeGameObject;
            var visibility = SceneVisibilityManager.instance;

            var state = new StateMessage
            {
                focused = InternalEditorUtility.isApplicationActive,
                playing = EditorApplication.isPlaying,
                paused = EditorApplication.isPaused,
                compiling = EditorApplication.isCompiling,
                compileFailed = EditorUtility.scriptCompilationFailed,
                tool = Tools.current.ToString(),
                pivotMode = Tools.pivotMode.ToString(),
                pivotRotation = Tools.pivotRotation.ToString(),
                sceneView2D = view != null && view.in2DMode,
                drawMode = view != null ? view.cameraMode.drawMode.ToString() : null,
                vertexSnap = Read(() => (bool)VertexDragging.GetValue(null)),
                gridSnap = EditorSnapSettings.gridSnapEnabled,
                isolated = visibility.IsCurrentStageIsolated(),
                viewLocked = view != null && Read(() => (bool)ViewLockedToObject.GetValue(view)),
                overlays = view != null && Read(() => OverlaysEnabled(view), true),
                profilerRecording = ProfilerDriver.enabled,
                selectionActive = selected == null ? -1 : selected.activeSelf ? 1 : 0,
                selectionHidden = selected == null ? -1 : visibility.IsHidden(selected) ? 1 : 0,
                selectionUnpickable = selected == null ? -1 : visibility.IsPickingDisabled(selected) ? 1 : 0,
            };

            if (GetCountsByType != null)
            {
                var args = new object[] { 0, 0, 0 };
                if (Read(() => { GetCountsByType.Invoke(null, args); return true; }))
                {
                    state.errors = (int)args[0];
                    state.warnings = (int)args[1];
                }
            }

            return state;
        }

        private static bool OverlaysEnabled(SceneView view)
        {
            var canvas = OverlayCanvas.GetValue(view);
            _overlaysEnabled ??= canvas.GetType().GetProperty("overlaysEnabled", AnyMember);
            return (bool)_overlaysEnabled.GetValue(canvas);
        }

        private static T Read<T>(Func<T> read, T fallback = default)
        {
            try
            {
                return read();
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}
