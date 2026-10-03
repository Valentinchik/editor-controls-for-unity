namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    // A dynamic folder filled from a list the Unity package sends. Buttons are keyed by a short hash of the item id,
    // because scene paths and menu paths contain characters Options+ uses as separators in action names.
    public abstract class UnityCatalogFolder : PluginDynamicFolder
    {
        private const String NoPackageKey = "no-package";
        private const String HelpUrl = "https://github.com/Valentinchik/editor-controls-for-unity#unity-package";

        private Dictionary<String, CatalogItem> _items = new();
        private IReadOnlyList<CatalogItem> _shown;

        protected UnityCatalogFolder(String displayName)
        {
            this.DisplayName = displayName;
            this.GroupName = Groups.Folders;
        }

        protected String LocalizedName => this.Plugin.Localization.LocalizeDisplayName(this.DisplayName);

        protected abstract String ItemIcon { get; }

        protected abstract String EmptyLabel { get; }

        public override Boolean Load()
        {
            UnityBridge.Changed += this.OnBridgeChanged;
            return true;
        }

        public override Boolean Unload()
        {
            UnityBridge.Changed -= this.OnBridgeChanged;
            return true;
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _) => PluginDynamicFolderNavigation.ButtonArea;

        public override String GetButtonDisplayName(PluginImageSize imageSize) => this.LocalizedName;

        public override BitmapImage GetButtonImage(PluginImageSize imageSize) =>
            KeyRenderer.Glyph(KeyRenderer.IconOf(this.GetType()), KeyRenderer.Normal, imageSize);

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            this.Refresh();
            return this._items.Count > 0
                ? this._items.Keys.Select(this.CreateCommandName)
                : new[] { this.CreateCommandName(NoPackageKey) };
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            actionParameter == NoPackageKey
                ? (UnityBridge.View.Connected ? this.EmptyLabel : "Unity package")
                : this._items.TryGetValue(actionParameter, out var item) ? this.LabelOf(item) : null;

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (!this._items.TryGetValue(actionParameter, out var item))
            {
                return KeyRenderer.Glyph(UnityBridge.View.Connected ? "info-circle" : "plug-x", KeyRenderer.Normal, imageSize);
            }

            var look = this.LookOf(item, UnityBridge.View.State);
            return KeyRenderer.Glyph(look.Icon ?? item.Icon ?? this.ItemIcon, look.IconColor, imageSize);
        }

        public override void RunCommand(String actionParameter)
        {
            if (this._items.TryGetValue(actionParameter, out var item))
            {
                this.Run(item);
            }
            else if (actionParameter == NoPackageKey && !UnityBridge.View.Connected)
            {
                Browser.Open(HelpUrl);
            }
        }

        protected abstract IReadOnlyList<CatalogItem> ItemsOf(EditorCatalog catalog);

        protected abstract void Run(CatalogItem item);

        // Blue for the current item (open scene, loaded layout); a null Icon keeps the item's own.
        protected virtual KeyLook LookOf(CatalogItem item, EditorState state) =>
            new(this.IsCurrent(item, state) ? KeyRenderer.Active : KeyRenderer.Normal);

        protected virtual Boolean IsCurrent(CatalogItem item, EditorState state) => false;

        protected virtual String LabelOf(CatalogItem item) => item.Label;

        // Redraws every button, e.g. after a mode inside the folder changed.
        protected void RedrawItems()
        {
            foreach (var key in this._items.Keys)
            {
                this.CommandImageChanged(key);
            }
        }

        private void OnBridgeChanged()
        {
            if (this.Refresh())
            {
                this.ButtonActionNamesChanged();
            }

            this.RedrawItems();
        }

        // True when the list of buttons changed.
        private Boolean Refresh()
        {
            var items = this.ItemsOf(UnityBridge.ActiveCatalog);
            if (ReferenceEquals(items, this._shown))
            {
                return false;
            }

            this._shown = items;
            this._items = items.GroupBy(Key).ToDictionary(group => group.Key, group => group.First());
            return true;
        }

        private static String Key(CatalogItem item) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(item.Id)), 0, 6).ToLowerInvariant();
    }
}
