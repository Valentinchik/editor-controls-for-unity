namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    // The user's active Unity shortcut profile: only the bindings they changed (Edit → Shortcuts), parent profiles included.
    // A binding explicitly cleared in Unity resolves to null — the action then does nothing rather than send a stale key.
    public static class UnityShortcutProfile
    {
        private const String ActiveProfilePrefKey = "ShortcutManagement_LastUsedShortcutProfileId_default";
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(2);

        private static readonly Object Sync = new();
        private static Dictionary<String, UnityBinding?> _overrides = new();
        private static DateTime _loadedAt = DateTime.MinValue;

        public static UnityBinding? Resolve(String shortcutId, String[] legacyIds, UnityBinding fallback)
        {
            lock (Sync)
            {
                if (DateTime.UtcNow - _loadedAt > CacheLifetime)
                {
                    _overrides = Load();
                    _loadedAt = DateTime.UtcNow;
                }

                if (_overrides.TryGetValue(shortcutId, out var binding))
                {
                    return binding;
                }

                foreach (var legacyId in legacyIds)
                {
                    if (_overrides.TryGetValue(legacyId, out binding))
                    {
                        return binding;
                    }
                }

                return fallback;
            }
        }

        private static Dictionary<String, UnityBinding?> Load()
        {
            try
            {
                return ReadOverrides(ProfilesFolder(), ReadActiveProfileId());
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex, "Could not read the Unity shortcut profile; using Unity defaults");
                return new Dictionary<String, UnityBinding?>();
            }
        }

        public static Dictionary<String, UnityBinding?> ReadOverrides(String profilesFolder, String activeProfileId)
        {
            var chain = new List<JsonElement>();
            var visited = new HashSet<String>();
            for (var id = activeProfileId; !String.IsNullOrEmpty(id) && visited.Add(id);)
            {
                var path = Path.Combine(profilesFolder, id + ".shortcut");
                if (!File.Exists(path))
                {
                    break;
                }

                var profile = JsonDocument.Parse(File.ReadAllText(path)).RootElement.Clone();
                chain.Add(profile);
                id = profile.TryGetProperty("m_ParentId", out var parent) ? parent.GetString() : null;
            }

            // Apply the root parent first so the active profile wins.
            var overrides = new Dictionary<String, UnityBinding?>();
            foreach (var profile in Enumerable.Reverse(chain))
            {
                foreach (var entry in profile.GetProperty("m_Entries").EnumerateArray())
                {
                    var shortcutId = entry.GetProperty("identifier").GetProperty("path").GetString();
                    var combination = entry.GetProperty("combinations").EnumerateArray().FirstOrDefault();
                    overrides[shortcutId] = combination.ValueKind == JsonValueKind.Object
                        ? new UnityBinding(combination.GetProperty("m_KeyCode").GetInt32(), (UnityModifiers)combination.GetProperty("m_Modifiers").GetInt32())
                        : null;
                }
            }

            return overrides;
        }

        private static String ProfilesFolder() => OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Unity\Editor-5.x\Preferences\shortcuts\default")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Preferences/Unity/Editor-5.x/shortcuts/default");

        // EditorPrefs: a plist on macOS, the registry on Windows (value name gets a "_h<hash>" suffix, data is UTF-8).
        private static String ReadActiveProfileId()
        {
            if (OperatingSystem.IsWindows())
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Unity Technologies\Unity Editor 5.x");
                var name = key?.GetValueNames().FirstOrDefault(n => n.StartsWith(ActiveProfilePrefKey + "_h", StringComparison.Ordinal));
                return name != null && key.GetValue(name) is Byte[] data ? Encoding.UTF8.GetString(data).TrimEnd('\0') : null;
            }

            var startInfo = new ProcessStartInfo("/usr/bin/defaults") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            startInfo.ArgumentList.Add("read");
            startInfo.ArgumentList.Add("com.unity3d.UnityEditor5.x");
            startInfo.ArgumentList.Add(ActiveProfilePrefKey);
            using var process = Process.Start(startInfo);
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(2000);
            return process.ExitCode == 0 ? output : null;
        }
    }
}
