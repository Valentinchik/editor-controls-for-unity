namespace Loupedeck.UnityEditorControlsPlugin
{
    // Status keys: with the Unity package they show compilation and console errors live;
    // without it they still work as Refresh and Console.
    public sealed class CompileStatusCommand : LiveShortcutCommand
    {
        public CompileStatusCommand() : base("Compile", Groups.Status, UnityShortcuts.Refresh) { }

        protected override KeyLook? GetLook(EditorView view)
        {
            if (view.Compiling)
            {
                return new KeyLook(KeyRenderer.Busy, "loader-2");
            }

            if (!view.Connected)
            {
                return null;
            }

            return view.State.CompileFailed
                ? new KeyLook(KeyRenderer.Failed, "alert-triangle")
                : new KeyLook(KeyRenderer.Good, "circle-check");
        }
    }

    public sealed class ConsoleErrorsCommand : LiveShortcutCommand
    {
        public ConsoleErrorsCommand() : base("Errors", Groups.Status, UnityShortcuts.ConsoleWindow) { }

        protected override KeyLook? GetLook(EditorView view)
        {
            if (!view.Connected)
            {
                return null;
            }

            var errors = view.State.Errors;
            return new KeyLook(errors > 0 ? KeyRenderer.Failed : KeyRenderer.Normal, Label: $"{this.LocalizedName} {errors}");
        }
    }
}
