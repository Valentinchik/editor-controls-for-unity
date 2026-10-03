namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;

    // Scenes in Build Profiles first, then the rest of the project; the open scene is highlighted.
    public sealed class ScenesFolder : UnityCatalogFolder
    {
        public ScenesFolder() : base("Scenes") { }

        protected override String ItemIcon => "map-pin";

        protected override String EmptyLabel => "No scenes";

        protected override IReadOnlyList<CatalogItem> ItemsOf(EditorCatalog catalog) => catalog.Get(EditorCatalog.Scenes);

        protected override Boolean IsCurrent(CatalogItem item, EditorState state) => state?.ActiveScene == item.Id;

        protected override void Run(CatalogItem item)
        {
            UnityBridge.TrySend(new { type = "openScene", path = item.Id });
            this.Close();
        }
    }

    // Methods marked [EditorControlsAction] and menu items under the roots set in Preferences → Editor Controls for Unity.
    public sealed class ProjectToolsFolder : UnityCatalogFolder
    {
        public ProjectToolsFolder() : base("Project Tools") { }

        protected override String ItemIcon => "hammer";

        protected override String EmptyLabel => "No tools";

        protected override IReadOnlyList<CatalogItem> ItemsOf(EditorCatalog catalog) => catalog.Get(EditorCatalog.Tools);

        protected override void Run(CatalogItem item) => UnityBridge.TrySend(new { type = "runTool", id = item.Id });
    }

    // Window → Layouts: Unity's layouts and the user's saved ones; the loaded one is highlighted.
    public sealed class LayoutsFolder : UnityCatalogFolder
    {
        public LayoutsFolder() : base("Layouts") { }

        protected override String ItemIcon => "layout";

        protected override String EmptyLabel => "No layouts";

        protected override IReadOnlyList<CatalogItem> ItemsOf(EditorCatalog catalog) => catalog.Get(EditorCatalog.Layouts);

        protected override Boolean IsCurrent(CatalogItem item, EditorState state) =>
            String.Equals(state?.Layout, item.Label, StringComparison.OrdinalIgnoreCase);

        protected override void Run(CatalogItem item)
        {
            UnityBridge.TrySend(new { type = "loadLayout", path = item.Id });
            this.Close();
        }
    }

    // Assets recently selected in the Project window or opened in Prefab Mode. Prefabs open in Prefab Mode,
    // scenes and scripts open, everything else is selected and pinged.
    public sealed class RecentAssetsFolder : UnityCatalogFolder
    {
        public RecentAssetsFolder() : base("Recent Assets") { }

        protected override String ItemIcon => "file-unknown";

        protected override String EmptyLabel => "No recent assets";

        protected override IReadOnlyList<CatalogItem> ItemsOf(EditorCatalog catalog) => catalog.Get(EditorCatalog.Recent);

        protected override void Run(CatalogItem item) => UnityBridge.TrySend(new { type = "openAsset", id = item.Id });
    }

    // Assets added with Assets → Editor Controls → Add to Favorites (per project, not shared through version control).
    public sealed class FavoritesFolder : UnityCatalogFolder
    {
        public FavoritesFolder() : base("Favorites") { }

        protected override String ItemIcon => "file-unknown";

        protected override String EmptyLabel => "No favorites";

        protected override IReadOnlyList<CatalogItem> ItemsOf(EditorCatalog catalog) => catalog.Get(EditorCatalog.Favorites);

        protected override void Run(CatalogItem item) => UnityBridge.TrySend(new { type = "openAsset", id = item.Id });
    }

    // Saved Scene View cameras for the active scene. An empty slot saves the current view, a filled one flies to it;
    // Save turns the next press into "overwrite this slot".
    public sealed class CameraBookmarksFolder : UnityCatalogFolder
    {
        private const String SaveId = "save";
        private const Int32 Slots = 7; // CameraBookmarks.Slots in the Unity package

        private static readonly IReadOnlyList<CatalogItem> Items = CreateItems();

        private Boolean _saveMode;

        public CameraBookmarksFolder() : base("Camera Bookmarks") { }

        protected override String ItemIcon => "bookmark";

        protected override String EmptyLabel => "";

        protected override IReadOnlyList<CatalogItem> ItemsOf(EditorCatalog catalog) =>
            UnityBridge.View.Connected || UnityBridge.View.Reloading ? Items : Array.Empty<CatalogItem>();

        public override Boolean Deactivate()
        {
            this._saveMode = false;
            return base.Deactivate();
        }

        protected override String LabelOf(CatalogItem item) =>
            item.Id == SaveId ? this.Plugin.Localization.LocalizeDisplayName("Save") : item.Label;

        protected override KeyLook LookOf(CatalogItem item, EditorState state)
        {
            if (item.Id == SaveId)
            {
                return new KeyLook(this._saveMode ? KeyRenderer.Active : KeyRenderer.Normal, "device-floppy");
            }

            if (this._saveMode)
            {
                return new KeyLook(KeyRenderer.Busy, "bookmark-plus");
            }

            return IsFilled(item, state)
                ? new KeyLook(KeyRenderer.Normal, "bookmark")
                : new KeyLook(KeyRenderer.Dim, "bookmark-plus");
        }

        protected override void Run(CatalogItem item)
        {
            if (item.Id == SaveId)
            {
                this._saveMode = !this._saveMode;
                this.RedrawItems();
                return;
            }

            var save = this._saveMode || !IsFilled(item, UnityBridge.View.State);
            UnityBridge.TrySend(new { type = "bookmark", index = Int32.Parse(item.Id), save });
            if (this._saveMode)
            {
                this._saveMode = false;
                this.RedrawItems();
            }
        }

        private static Boolean IsFilled(CatalogItem item, EditorState state) =>
            state != null && (state.Bookmarks & (1 << Int32.Parse(item.Id))) != 0;

        private static IReadOnlyList<CatalogItem> CreateItems()
        {
            var items = new List<CatalogItem> { new(SaveId, "Save", null) };
            for (var slot = 0; slot < Slots; slot++)
            {
                items.Add(new CatalogItem(slot.ToString(), (slot + 1).ToString(), null));
            }

            return items;
        }
    }
}
