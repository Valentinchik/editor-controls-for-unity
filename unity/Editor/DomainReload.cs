using UnityEditor;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    // Reload Domain and Clean Play keys, for projects that enter Play Mode without a domain reload
    // (Project Settings → Editor → Enter Play Mode Settings): static state survives from one Play session to the next,
    // and these keys start from a fresh domain without touching that project setting.
    [InitializeOnLoad]
    internal static class DomainReload
    {
        public const string ReloadCommand = "Editor Controls/Reload Domain";
        public const string CleanPlayCommand = "Editor Controls/Clean Play";

        private const string PlayAfterReloadKey = "Valentinchik.EditorControls.PlayAfterReload";
        private const string ReloadAfterStopKey = "Valentinchik.EditorControls.ReloadAfterStop";

        static DomainReload()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (PlayPending)
            {
                SessionState.EraseBool(PlayAfterReloadKey);
                // Too early inside the domain-load callback; let the editor finish loading first.
                EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
            }
        }

        // A Clean Play is under way: the coming domain reload belongs to entering Play Mode, not to a compilation.
        public static bool PlayPending => SessionState.GetBool(PlayAfterReloadKey, false);

        private static bool PlayModeReloadsDomain =>
            !EditorSettings.enterPlayModeOptionsEnabled || (EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableDomainReload) == 0;

        public static bool Reload()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Editor Controls] Leave Play Mode to reload the domain.");
                return false;
            }

            EditorUtility.RequestScriptReload();
            return true;
        }

        // Fresh domain, then Play. In Play Mode it stops first, so it doubles as "restart clean".
        public static bool CleanPlay()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                return false; // already switching modes
            }

            if (EditorApplication.isPlaying)
            {
                SessionState.SetBool(ReloadAfterStopKey, true);
                EditorApplication.isPlaying = false;
                return true;
            }

            StartFromEditMode();
            return true;
        }

        private static void StartFromEditMode()
        {
            if (PlayModeReloadsDomain)
            {
                EditorApplication.isPlaying = true; // entering Play Mode reloads the domain anyway
                return;
            }

            SessionState.SetBool(PlayAfterReloadKey, true);
            EditorUtility.RequestScriptReload();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(ReloadAfterStopKey, false))
            {
                SessionState.EraseBool(ReloadAfterStopKey);
                StartFromEditMode();
            }
        }
    }
}
