using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Valentinchik.EditorControls
{
    // Time.timeScale from the keypad. Play Mode only: in Edit Mode the value is the project's Time setting,
    // and changing it there would edit ProjectSettings/TimeManager.asset.
    internal static class TimeScale
    {
        private static readonly float[] Steps = { 0.05f, 0.1f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 4f, 8f };
        private const string HintShownKey = "Valentinchik.EditorControls.TimeScaleHint";

        public static void Set(float value)
        {
            if (!EditorApplication.isPlaying)
            {
                if (!SessionState.GetBool(HintShownKey, false))
                {
                    SessionState.SetBool(HintShownKey, true);
                    Debug.Log("[Editor Controls] Time scale keys work in Play Mode.");
                }

                return;
            }

            Time.timeScale = Mathf.Clamp(value, 0f, 100f);
        }

        // To the next value on the ladder; from a value in between, to the nearest one in that direction.
        public static void Step(int direction)
        {
            var current = Time.timeScale;
            var next = direction > 0
                ? Steps.Where(step => step > current + 0.001f).DefaultIfEmpty(Steps[Steps.Length - 1]).First()
                : Steps.Where(step => step < current - 0.001f).DefaultIfEmpty(Steps[0]).Last();
            Set(next);
        }
    }
}
