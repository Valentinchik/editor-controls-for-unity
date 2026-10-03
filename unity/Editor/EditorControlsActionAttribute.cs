using System;

namespace Valentinchik.EditorControls
{
    /// <summary>
    /// Puts a static, parameterless editor method into the plugin's Project Tools folder in Logi Options+.
    /// Icon is a Tabler icon name the plugin ships (for example "hammer", "bug", "rocket"); unknown names fall back to a tool icon.
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
    }
}
