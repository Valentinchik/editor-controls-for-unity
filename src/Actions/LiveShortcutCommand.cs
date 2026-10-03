namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // A key that redraws itself from the live editor state reported by the Unity package: the glyph's colour (and
    // optionally the glyph or label) follows the state.
    public abstract class LiveCommand : PluginDynamicCommand
    {
        protected LiveCommand(String displayName, String description, String groupName)
            : base(displayName: displayName, description: description, groupName: groupName)
        {
        }

        protected String LocalizedName => this.Plugin.Localization.LocalizeDisplayName(this.DisplayName);

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

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var look = this.GetLook(UnityBridge.View);
            return KeyRenderer.Glyph(look?.Icon ?? KeyRenderer.IconOf(this.GetType()), look?.IconColor ?? KeyRenderer.Normal, imageSize);
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            this.GetLook(UnityBridge.View)?.Label ?? base.GetCommandDisplayName(actionParameter, imageSize);

        // Null keeps the normal look.
        protected abstract KeyLook? GetLook(EditorView view);

        private void OnBridgeChanged() => this.ActionImageChanged();
    }

    public readonly record struct KeyLook(BitmapColor IconColor, String Icon = null, String Label = null);

    // A Unity shortcut with a live look. Without the package it looks and works like any other shortcut key.
    public abstract class LiveShortcutCommand : LiveCommand
    {
        private readonly UnityShortcut _shortcut;

        protected LiveShortcutCommand(String displayName, String groupName, UnityShortcut shortcut)
            : base(displayName, shortcut.Description, groupName)
        {
            this._shortcut = shortcut;
        }

        protected override void RunCommand(String actionParameter) => this._shortcut.Send(this.Plugin.ClientApplication);
    }

    // Highlights the key while a boolean editor state is on (active tool, 2D mode, ...).
    public abstract class HighlightShortcutCommand : LiveShortcutCommand
    {
        protected HighlightShortcutCommand(String displayName, String groupName, UnityShortcut shortcut)
            : base(displayName, groupName, shortcut)
        {
        }

        protected abstract Boolean IsOn(EditorState state);

        protected override KeyLook? GetLook(EditorView view) =>
            view.Connected && this.IsOn(view.State) ? new KeyLook(KeyRenderer.Active) : null;
    }

    // An action only the Unity package can perform (there is no Unity shortcut behind it).
    // Without the package the key shows a crossed-out plug and does nothing.
    public abstract class BridgeCommand : LiveCommand
    {
        protected BridgeCommand(String displayName, String description, String groupName)
            : base(displayName, description, groupName)
        {
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var view = UnityBridge.View;
            return view.Connected || view.Reloading
                ? base.GetCommandImage(actionParameter, imageSize)
                : KeyRenderer.Glyph("plug-x", KeyRenderer.Normal, imageSize);
        }

        protected override void RunCommand(String actionParameter)
        {
            if (UnityBridge.View.Connected)
            {
                this.Run();
            }
        }

        protected abstract void Run();
    }
}
