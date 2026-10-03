namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;

    // A configurable action: the user picks any Unity menu item in the Options+ action editor. The list comes from the
    // focused editor through the Unity package, which also runs the item, so it works with Unity in the background.
    public sealed class RunMenuItemCommand : ActionEditorCommand
    {
        private const String MenuControl = "menu";

        // Kept across editor reloads and disconnects, so the list is there even when Unity is busy.
        private static IReadOnlyList<CatalogItem> _menus = Array.Empty<CatalogItem>();

        public RunMenuItemCommand()
        {
            this.DisplayName = "Run Menu Item";
            this.Description = "Runs any Unity menu item chosen in the action settings (needs the Unity package)";
            this.GroupName = Groups.Project;

            this.ActionEditor.AddControlEx(new ActionEditorListbox(MenuControl, "Menu item").SetRequired());
            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;
            this.ActionEditor.Started += (_, _) => UnityBridge.TrySend(new { type = "listMenus" });
        }

        protected override Boolean OnLoad()
        {
            UnityBridge.Changed += this.OnBridgeChanged;
            return true;
        }

        protected override Boolean OnUnload()
        {
            UnityBridge.Changed -= this.OnBridgeChanged;
            return true;
        }

        protected override String GetCommandDisplayName(ActionEditorActionParameters actionParameters) =>
            actionParameters.TryGetString(MenuControl, out var path) && !String.IsNullOrEmpty(path)
                ? path.Substring(path.LastIndexOf('/') + 1)
                : this.DisplayName;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters) =>
            actionParameters.TryGetString(MenuControl, out var path) && !String.IsNullOrEmpty(path)
            && UnityBridge.TrySend(new { type = "runTool", id = "menu:" + path });

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            if (!String.Equals(e.ControlName, MenuControl, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (var menu in _menus)
            {
                e.AddItem(menu.Id, menu.Label, null);
            }
        }

        private void OnBridgeChanged()
        {
            var menus = UnityBridge.ActiveCatalog.Get(EditorCatalog.Menus);
            if (menus.Count > 0 && !ReferenceEquals(menus, _menus))
            {
                _menus = menus;
                this.ActionEditor.ListboxItemsChanged(MenuControl);
            }
        }
    }
}
