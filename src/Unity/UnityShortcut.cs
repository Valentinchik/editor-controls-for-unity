namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // One Unity Editor shortcut: its Shortcut Manager id plus the Unity 6 default binding.
    // The user's own rebinding (Edit → Shortcuts) wins over the default at send time.
    public sealed class UnityShortcut
    {
        private readonly UnityBinding _default;
        private readonly UnityBinding? _windowsDefault;
        private readonly String[] _legacyIds;
        private readonly Boolean _unity6Only;

        // legacyIds: the same command under an older Unity's id (2022 renamed some), so a rebinding made there is honoured too.
        // unity6Only: absent, or present without a default binding, before Unity 6 — works there only once the user binds it.
        public UnityShortcut(String id, UnityBinding defaultBinding, UnityBinding? windowsDefault = null, String[] legacyIds = null, Boolean unity6Only = false)
        {
            this.Id = id;
            this._default = defaultBinding;
            this._windowsDefault = windowsDefault;
            this._legacyIds = legacyIds ?? Array.Empty<String>();
            this._unity6Only = unity6Only;
        }

        // Null for raw keys that are not Unity shortcuts (arrow navigation) — those are never rebound.
        public String Id { get; }

        public String Description
        {
            get
            {
                if (this.Id == null)
                {
                    return "";
                }

                var path = (this.Id.StartsWith("Main Menu/", StringComparison.Ordinal) ? this.Id.Substring("Main Menu/".Length) : this.Id).Replace("/", " › ");
                return this._unity6Only ? path + " · Unity 6+" : path;
            }
        }

        public void Send(ClientApplication application)
        {
            // The Unity package runs it directly — works in the background and ignores keyboard layout and rebinding.
            if (this.Id != null && UnityBridge.TrySend(this.Id))
            {
                return;
            }

            var fallback = OperatingSystem.IsWindows() && this._windowsDefault.HasValue ? this._windowsDefault.Value : this._default;
            var binding = this.Id == null ? fallback : UnityShortcutProfile.Resolve(this.Id, this._legacyIds, fallback);

            if (binding is not { } resolved)
            {
                PluginLog.Info($"'{this.Id}' has no shortcut in the active Unity profile");
                return;
            }

            if (!resolved.TryToVirtualKey(out var key))
            {
                PluginLog.Warning($"'{this.Id}' is bound to {resolved}, which cannot be sent as a keystroke");
                return;
            }

            PluginLog.Verbose($"'{this.Id}' → keystroke {resolved}");
            application.SendKeyboardShortcut(key, resolved.ToModifierKey());
        }
    }
}
