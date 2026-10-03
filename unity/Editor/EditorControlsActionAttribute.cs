using System;

namespace Valentinchik.EditorControls
{
    /// <summary>
    /// Puts a static, parameterless editor method into the plugin's Project Tools folder in Logi Options+.
    /// Icon: a Tabler icon name the plugin ships ("hammer", "bug", "rocket"; unknown names show a tool icon).
    /// Order: position in the folder, lower first; equal orders sort by label. Editor-only: in runtime code wrap it in #if UNITY_EDITOR.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EditorControlsActionAttribute : Attribute
    {
        public EditorControlsActionAttribute(string label = null)
        {
            Label = label;
        }

        public string Label { get; }

        public string Icon { get; set; }

        public int Order { get; set; }
    }
}
