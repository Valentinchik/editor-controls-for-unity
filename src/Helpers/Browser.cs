namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Diagnostics;

    internal static class Browser
    {
        public static void Open(String url)
        {
            try
            {
                Process.Start(OperatingSystem.IsWindows()
                    ? new ProcessStartInfo(url) { UseShellExecute = true }
                    : new ProcessStartInfo("open", url));
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex, $"Could not open {url}");
            }
        }
    }
}
