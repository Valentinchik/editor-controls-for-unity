namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // The optional Unity package com.valentinchik.editor-controls, from the plugin's side.
    internal static class UnityPackage
    {
        public const String HelpUrl = "https://github.com/Valentinchik/editor-controls-for-unity#unity-package";

        // Appended to the description of every action only the package can run, like " · Unity 6+".
        public const String Required = " · needs the Unity package";

        private static readonly TimeSpan HelpCooldown = TimeSpan.FromSeconds(10);
        private static DateTime _helpOpenedUtc = DateTime.MinValue;

        // A package-only key was pressed without the package: with Unity open, show how to install it — once in a while,
        // not on every press, and never while the editor is only reloading its domain.
        public static void ExplainMissing(ClientApplication unity)
        {
            var view = UnityBridge.View;
            if (view.Connected || view.Reloading || !unity.IsRunning() || DateTime.UtcNow - _helpOpenedUtc < HelpCooldown)
            {
                return;
            }

            _helpOpenedUtc = DateTime.UtcNow;
            Browser.Open(HelpUrl);
        }
    }
}
