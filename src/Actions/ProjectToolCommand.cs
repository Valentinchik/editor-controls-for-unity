namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;

    // Every [EditorControlsAction] tool as its own action in the Options+ side panel, under Project Tools › <Group>, so a
    // single tool can go straight onto a key. Tools are remembered in a plugin setting: keys keep their name and icon,
    // and the panel keeps its entries, while Unity is closed. A project's entries are replaced whenever it reports its list.
    // Options+ profiles are per app, not per project, so a key may hold a tool of another project: it is drawn dimmed when
    // the project in front lacks the tool, and with a crossed-out plug when that project has no package.
    public sealed class ProjectToolCommand : PluginDynamicCommand
    {
        private const String SuperGroup = "Project Tools";
        private const String SettingName = "ProjectTools";

        private readonly Object _sync = new();
        private Dictionary<String, KnownTool> _known = new();
        private IReadOnlyList<CatalogItem> _lastList;
        private (Boolean Connected, Boolean Reloading, IReadOnlyList<CatalogItem> Tools) _drawnFor;

        public ProjectToolCommand()
            : base(displayName: "Project Tool", description: "Runs a method marked [EditorControlsAction] in the Unity project" + UnityPackage.Required, groupName: SuperGroup)
        {
        }

        protected override Boolean OnLoad()
        {
            this._known = Load(this.Plugin);
            this.PublishParameters();
            UnityBridge.Changed += this.OnBridgeChanged;
            return true;
        }

        protected override Boolean OnUnload()
        {
            UnityBridge.Changed -= this.OnBridgeChanged;
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            KnownTool tool;
            lock (this._sync)
            {
                this._known.TryGetValue(actionParameter ?? "", out tool);
            }

            // Without a connected package the key already shows a crossed-out plug; a project's own tool is no reason to
            // send the user to the package's install page. A tool missing in this project is reported by Unity's console.
            if (tool != null && UnityBridge.View.Connected)
            {
                UnityBridge.TrySend(new { type = "runTool", id = tool.Id });
            }
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            lock (this._sync)
            {
                return this._known.TryGetValue(actionParameter ?? "", out var tool) ? tool.Label : base.GetCommandDisplayName(actionParameter, imageSize);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            KnownTool tool;
            lock (this._sync)
            {
                this._known.TryGetValue(actionParameter ?? "", out tool);
            }

            var view = UnityBridge.View;
            if (!view.Connected && !view.Reloading)
            {
                return KeyRenderer.Glyph("plug-x", KeyRenderer.Normal, imageSize);
            }

            var available = tool == null || !view.Connected || UnityBridge.ActiveCatalog.Get(EditorCatalog.Tools).Any(item => item.Id == tool.Id);
            return KeyRenderer.Glyph(tool?.Icon ?? "hammer", available ? KeyRenderer.Normal : KeyRenderer.Dim, imageSize);
        }

        private void OnBridgeChanged()
        {
            var tools = UnityBridge.ActiveCatalog.Get(EditorCatalog.Tools);
            this.RedrawIfTargetChanged(tools);
            var project = UnityBridge.ActiveProject;
            if (project == null || ReferenceEquals(tools, this._lastList))
            {
                return;
            }

            lock (this._sync)
            {
                this._lastList = tools;
                var known = this._known.Values.Where(tool => tool.Project != project).ToDictionary(tool => Key(tool.Id));
                foreach (var tool in tools)
                {
                    known[Key(tool.Id)] = new KnownTool(tool.Id, tool.Label, tool.Icon, tool.Group, project);
                }

                if (Same(known, this._known))
                {
                    return;
                }

                this._known = known;
                this.Plugin.SetPluginSetting(SettingName, JsonSerializer.Serialize(known.Values.ToList()), false);
            }

            this.PublishParameters();
        }

        // Keys depend on which project is in front and on its tool list; redraw only when one of those changes.
        private void RedrawIfTargetChanged(IReadOnlyList<CatalogItem> tools)
        {
            var view = UnityBridge.View;
            var target = (view.Connected, view.Reloading, tools);
            lock (this._sync)
            {
                if (target.Connected == this._drawnFor.Connected && target.Reloading == this._drawnFor.Reloading
                    && ReferenceEquals(target.tools, this._drawnFor.Tools))
                {
                    return;
                }

                this._drawnFor = target;
            }

            this.ActionImageChanged();
        }

        private void PublishParameters()
        {
            lock (this._sync)
            {
                this.RemoveAllParameters();
                foreach (var (key, tool) in this._known)
                {
                    this.AddParameter(key, tool.Label, tool.Group ?? SuperGroup).SuperGroupName = SuperGroup;
                }
            }

            this.ParametersChanged();
            this.ActionImageChanged();
        }

        private static Dictionary<String, KnownTool> Load(Plugin plugin)
        {
            try
            {
                if (plugin.TryGetPluginSetting(SettingName, out var json) && !String.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<List<KnownTool>>(json).ToDictionary(tool => Key(tool.Id));
                }
            }
            catch (Exception ex) when (ex is JsonException or ArgumentException)
            {
                PluginLog.Warning(ex, "Ignoring an unreadable list of project tools");
            }

            return new Dictionary<String, KnownTool>();
        }

        private static Boolean Same(Dictionary<String, KnownTool> a, Dictionary<String, KnownTool> b) =>
            a.Count == b.Count && a.All(pair => b.TryGetValue(pair.Key, out var other) && other == pair.Value);

        // Tool ids hold ':', '.' and '/', which Options+ action names do not take; a short hash keys the parameter instead.
        private static String Key(String id) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(id)), 0, 6).ToLowerInvariant();

        public sealed record KnownTool(String Id, String Label, String Icon, String Group, String Project);
    }
}
