namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;

    // Mirrors UnityEditor.ShortcutManagement.ShortcutModifiers; Action is ⌘ on macOS and Ctrl on Windows.
    [Flags]
    public enum UnityModifiers
    {
        None = 0,
        Alt = 1,
        Action = 2,
        Shift = 4,
        Control = 8,
    }

    // A key combination in Unity's own terms (UnityEngine.KeyCode + ShortcutModifiers), as stored in .shortcut profiles.
    public readonly record struct UnityBinding(Int32 KeyCode, UnityModifiers Modifiers)
    {
        public ModifierKey ToModifierKey()
        {
            var keys = ModifierKey.None;
            if (this.Modifiers.HasFlag(UnityModifiers.Action)) keys |= ModifierKey.ControlOrCommand;
            if (this.Modifiers.HasFlag(UnityModifiers.Alt)) keys |= ModifierKey.AltOrOption;
            if (this.Modifiers.HasFlag(UnityModifiers.Shift)) keys |= ModifierKey.Shift;
            if (this.Modifiers.HasFlag(UnityModifiers.Control)) keys |= ModifierKey.Control;
            return keys;
        }

        public Boolean TryToVirtualKey(out VirtualKeyCode key) => UnityKeys.TryToVirtualKey(this.KeyCode, out key);

        public override String ToString()
        {
            var text = "";
            if (this.Modifiers.HasFlag(UnityModifiers.Control)) text += "Ctrl+";
            if (this.Modifiers.HasFlag(UnityModifiers.Alt)) text += "Alt+";
            if (this.Modifiers.HasFlag(UnityModifiers.Shift)) text += "Shift+";
            if (this.Modifiers.HasFlag(UnityModifiers.Action)) text += "Cmd/Ctrl+";
            return text + this.KeyCode;
        }
    }

    // UnityEngine.KeyCode values used by the catalog, and their mapping to Logi virtual keys.
    public static class UnityKeys
    {
        public const Int32 Backspace = 8, Tab = 9, Return = 13, Escape = 27, Space = 32, Quote = 39;
        public const Int32 Comma = 44, Minus = 45, Period = 46, Slash = 47, Semicolon = 59, EqualsKey = 61;
        public const Int32 LeftBracket = 91, Backslash = 92, RightBracket = 93, BackQuote = 96, Delete = 127;
        public const Int32 Alpha0 = 48, Alpha1 = 49, Alpha2 = 50, Alpha3 = 51, Alpha4 = 52;
        public const Int32 Alpha5 = 53, Alpha6 = 54, Alpha7 = 55, Alpha8 = 56, Alpha9 = 57;
        public const Int32 A = 97, B = 98, C = 99, D = 100, E = 101, F = 102, G = 103, H = 104, I = 105;
        public const Int32 K = 107, L = 108, N = 110, O = 111, P = 112, Q = 113, R = 114, S = 115, T = 116;
        public const Int32 U = 117, V = 118, W = 119, X = 120, Y = 121, Z = 122;
        public const Int32 UpArrow = 273, DownArrow = 274, RightArrow = 275, LeftArrow = 276;
        public const Int32 F1 = 282, F9 = 290;

        public static Boolean TryToVirtualKey(Int32 unityKeyCode, out VirtualKeyCode key)
        {
            key = unityKeyCode switch
            {
                >= A and <= Z => Offset(VirtualKeyCode.KeyA, unityKeyCode - A),
                >= Alpha0 and <= Alpha9 => Offset(VirtualKeyCode.Key0, unityKeyCode - Alpha0),
                >= F1 and <= 296 => Offset(VirtualKeyCode.F1, unityKeyCode - F1),
                >= 256 and <= 265 => Offset(VirtualKeyCode.NumPad0, unityKeyCode - 256),
                266 => VirtualKeyCode.Decimal,
                267 => VirtualKeyCode.Divide,
                268 => VirtualKeyCode.Multiply,
                269 => VirtualKeyCode.Subtract,
                270 => VirtualKeyCode.Add,
                271 => VirtualKeyCode.Return,
                Backspace => VirtualKeyCode.Back,
                Tab => VirtualKeyCode.Tab,
                Return => VirtualKeyCode.Return,
                Escape => VirtualKeyCode.Escape,
                Space => VirtualKeyCode.Space,
                Delete => VirtualKeyCode.Delete,
                Comma => VirtualKeyCode.Comma,
                Period => VirtualKeyCode.Period,
                Minus => VirtualKeyCode.Minus,
                EqualsKey => VirtualKeyCode.Equals,
                Semicolon => VirtualKeyCode.Oem1,
                Slash => VirtualKeyCode.Oem2,
                BackQuote => VirtualKeyCode.Oem3,
                LeftBracket => VirtualKeyCode.Oem4,
                Backslash => VirtualKeyCode.Oem5,
                RightBracket => VirtualKeyCode.Oem6,
                Quote => VirtualKeyCode.Oem7,
                UpArrow => VirtualKeyCode.ArrowUp,
                DownArrow => VirtualKeyCode.ArrowDown,
                RightArrow => VirtualKeyCode.ArrowRight,
                LeftArrow => VirtualKeyCode.ArrowLeft,
                277 => VirtualKeyCode.Insert,
                278 => VirtualKeyCode.Home,
                279 => VirtualKeyCode.End,
                280 => VirtualKeyCode.PageUp,
                281 => VirtualKeyCode.PageDown,
                _ => VirtualKeyCode.None,
            };
            return key != VirtualKeyCode.None;
        }

        private static VirtualKeyCode Offset(VirtualKeyCode first, Int32 offset) => (VirtualKeyCode)(Convert.ToInt32(first) + offset);
    }
}
