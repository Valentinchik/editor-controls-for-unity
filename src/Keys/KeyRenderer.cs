namespace Loupedeck.UnityEditorControlsPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    // Glyphs for live keys. Options+ places a plugin image into the icon area of metadata/DefaultIconTemplate.ict and
    // draws the label itself, so a live key returns only the glyph, coloured by state, on a transparent background.
    // The SVG must be rasterized with the colour (VectorToBitmap): a vector image from FromSvg is drawn as a white mask.
    // Tabler strokes use currentColor, which that rasterizer drops (only filled shapes survive), so it is made explicit.
    internal static class KeyRenderer
    {
        public static readonly BitmapColor Normal = new(0xFF, 0xFF, 0xFF);
        public static readonly BitmapColor Active = new(0x4C, 0x9A, 0xFF);
        public static readonly BitmapColor Busy = new(0xFF, 0xB2, 0x24);
        public static readonly BitmapColor Failed = new(0xFF, 0x5A, 0x52);
        public static readonly BitmapColor Good = new(0x3D, 0xD6, 0x8C);

        private static readonly Assembly Assembly = typeof(KeyRenderer).Assembly;
        private static readonly ConcurrentDictionary<String, BitmapImage> Glyphs = new();
        private static readonly ConcurrentDictionary<String, BitmapImage> Vectors = new();
        private static readonly Lazy<ILookup<String, String>> ActionIcons = new(LoadActionIcons);

        public static BitmapImage Glyph(String icon, BitmapColor color, PluginImageSize size) =>
            Glyphs.GetOrAdd($"{icon}|{color.ARGB}|{size}", _ => Vector(icon).VectorToBitmap(size, color));

        // The Tabler icon an action class uses on its key (icons/map.tsv).
        public static String IconOf(Type action) => ActionIcons.Value[action.Name].FirstOrDefault() ?? "square";

        private static BitmapImage Vector(String icon) => Vectors.GetOrAdd(icon, name =>
        {
            using var stream = Assembly.GetManifestResourceStream($"tabler.{name}.svg");
            using var reader = new StreamReader(stream);
            return BitmapImage.FromSvg(reader.ReadToEnd().Replace("stroke=\"currentColor\"", "stroke=\"#FFFFFF\""));
        });

        private static ILookup<String, String> LoadActionIcons()
        {
            using var stream = Assembly.GetManifestResourceStream("icons.map.tsv");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd()
                .Split('\n')
                .Where(line => line.Length > 0 && !line.StartsWith("#"))
                .Select(line => line.TrimEnd('\r').Split('\t'))
                .ToLookup(parts => parts[0], parts => parts[1]);
        }
    }
}
