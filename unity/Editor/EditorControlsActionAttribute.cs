using System;

namespace Valentinchik.EditorControls
{
    /// <summary>
    /// Puts a static, parameterless editor method into the plugin's Project Tools folder in Logi Options+.
    /// Icon: a Tabler icon name the plugin ships ("hammer", "bug", "rocket"; unknown names show a tool icon).
    /// Group: a subfolder of Project Tools and a group in the Options+ side panel. Order: position, lower first, then by label;
    /// a group sits where its lowest Order puts it. Editor-only: in runtime code wrap it in #if UNITY_EDITOR.
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

        public string Group { get; set; }
    }
}
