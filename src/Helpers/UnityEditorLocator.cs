namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    // Unity Hub keeps editors in <install root>/<version>/ — too deep for Options+ to find on its own.
    internal static class UnityEditorLocator
    {
        public const String BundleId = "com.unity3d.UnityEditor5.x";

        public static Boolean IsInstalled()
        {
            var installed = HubInstallRoots().Any(HasEditor) || (OperatingSystem.IsMacOS() && IsKnownToSpotlight());
            PluginLog.Info($"Unity Editor installed: {installed}");
            return installed;
        }

        private static IEnumerable<String> HubInstallRoots()
        {
            String hubSettings;
            if (OperatingSystem.IsWindows())
            {
                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Unity\Hub\Editor");
                hubSettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UnityHub");
            }
            else
            {
                yield return "/Applications/Unity/Hub/Editor";
                hubSettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Application Support/UnityHub");
            }

            var secondary = Path.Combine(hubSettings, "secondaryInstallPath.json");
            if (File.Exists(secondary))
            {
                var path = File.ReadAllText(secondary).Trim().Trim('"');
                if (path.Length > 0)
                {
                    yield return path;
                }
            }
        }

        private static Boolean HasEditor(String root) =>
            Directory.Exists(root) && Directory.EnumerateDirectories(root).Any(dir => OperatingSystem.IsWindows()
                ? File.Exists(Path.Combine(dir, @"Editor\Unity.exe"))
                : Directory.Exists(Path.Combine(dir, "Unity.app")));

        // Catches editors installed outside Unity Hub.
        private static Boolean IsKnownToSpotlight()
        {
            try
            {
                var startInfo = new ProcessStartInfo("/usr/bin/mdfind") { RedirectStandardOutput = true, UseShellExecute = false };
                startInfo.ArgumentList.Add($"kMDItemCFBundleIdentifier == '{BundleId}'");
                using var process = Process.Start(startInfo);
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(3000);
                return output.Trim().Length > 0;
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex, "mdfind lookup for Unity Editor failed");
                return false;
            }
        }
    }
}
