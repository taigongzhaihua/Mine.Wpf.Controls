using System.Windows.Media;

namespace Mine.Wpf.Controls.Theming;

/// <summary>
/// 官方预设主题目录。为常见色系提供稳定、可复用的种子色入口。
/// </summary>
public static class ThemePresetCatalog
{
    public static IReadOnlyList<ThemePreset> All { get; } =
    [
        ThemePreset.Violet,
        ThemePreset.Iris,
        ThemePreset.Purple,
        ThemePreset.Indigo,
        ThemePreset.Cobalt,
        ThemePreset.Blue,
        ThemePreset.Sky,
        ThemePreset.Cyan,
        ThemePreset.Teal,
        ThemePreset.Jade,
        ThemePreset.Green,
        ThemePreset.Forest,
        ThemePreset.Lime,
        ThemePreset.Olive,
        ThemePreset.Gold,
        ThemePreset.Amber,
        ThemePreset.Orange,
        ThemePreset.DeepOrange,
        ThemePreset.Coral,
        ThemePreset.Red,
        ThemePreset.Crimson,
        ThemePreset.Pink,
        ThemePreset.Rose,
        ThemePreset.Magenta,
        ThemePreset.Brown,
        ThemePreset.Cocoa,
        ThemePreset.Slate,
        ThemePreset.Gray
    ];

    public static string GetDisplayName(ThemePreset preset) => preset switch
    {
        ThemePreset.None => "Custom",
        ThemePreset.Violet => "Violet",
        ThemePreset.Iris => "Iris",
        ThemePreset.Purple => "Purple",
        ThemePreset.Indigo => "Indigo",
        ThemePreset.Cobalt => "Cobalt",
        ThemePreset.Blue => "Blue",
        ThemePreset.Sky => "Sky",
        ThemePreset.Cyan => "Cyan",
        ThemePreset.Teal => "Teal",
        ThemePreset.Jade => "Jade",
        ThemePreset.Green => "Green",
        ThemePreset.Forest => "Forest",
        ThemePreset.Lime => "Lime",
        ThemePreset.Olive => "Olive",
        ThemePreset.Gold => "Gold",
        ThemePreset.Amber => "Amber",
        ThemePreset.Orange => "Orange",
        ThemePreset.DeepOrange => "Deep Orange",
        ThemePreset.Coral => "Coral",
        ThemePreset.Red => "Red",
        ThemePreset.Crimson => "Crimson",
        ThemePreset.Pink => "Pink",
        ThemePreset.Rose => "Rose",
        ThemePreset.Magenta => "Magenta",
        ThemePreset.Brown => "Brown",
        ThemePreset.Cocoa => "Cocoa",
        ThemePreset.Slate => "Slate",
        ThemePreset.Gray => "Gray",
        _ => preset.ToString()
    };

    public static Color GetSeedColor(ThemePreset preset) => preset switch
    {
        ThemePreset.Violet => Color.FromRgb(0x6E, 0x56, 0xCF),
        ThemePreset.Iris => Color.FromRgb(0x5A, 0x67, 0xD8),
        ThemePreset.Purple => Color.FromRgb(0x67, 0x50, 0xA4),
        ThemePreset.Indigo => Color.FromRgb(0x39, 0x4A, 0xB6),
        ThemePreset.Cobalt => Color.FromRgb(0x00, 0x57, 0xB8),
        ThemePreset.Blue => Color.FromRgb(0x00, 0x5F, 0xAF),
        ThemePreset.Sky => Color.FromRgb(0x00, 0x7C, 0xC7),
        ThemePreset.Cyan => Color.FromRgb(0x00, 0x76, 0xA3),
        ThemePreset.Teal => Color.FromRgb(0x00, 0x68, 0x74),
        ThemePreset.Jade => Color.FromRgb(0x00, 0x7A, 0x5A),
        ThemePreset.Green => Color.FromRgb(0x00, 0x6C, 0x4E),
        ThemePreset.Forest => Color.FromRgb(0x2E, 0x7D, 0x32),
        ThemePreset.Lime => Color.FromRgb(0x75, 0x8F, 0x00),
        ThemePreset.Olive => Color.FromRgb(0x6B, 0x73, 0x31),
        ThemePreset.Gold => Color.FromRgb(0x8A, 0x63, 0x00),
        ThemePreset.Amber => Color.FromRgb(0x9A, 0x67, 0x00),
        ThemePreset.Orange => Color.FromRgb(0xB0, 0x5A, 0x00),
        ThemePreset.DeepOrange => Color.FromRgb(0xB5, 0x27, 0x0F),
        ThemePreset.Coral => Color.FromRgb(0xC2, 0x4F, 0x3D),
        ThemePreset.Red => Color.FromRgb(0xBA, 0x1A, 0x1A),
        ThemePreset.Crimson => Color.FromRgb(0xA6, 0x1D, 0x48),
        ThemePreset.Pink => Color.FromRgb(0xB0, 0x2F, 0x60),
        ThemePreset.Rose => Color.FromRgb(0xC2, 0x18, 0x5B),
        ThemePreset.Magenta => Color.FromRgb(0x9C, 0x27, 0xB0),
        ThemePreset.Brown => Color.FromRgb(0x7D, 0x52, 0x46),
        ThemePreset.Cocoa => Color.FromRgb(0x6D, 0x4C, 0x41),
        ThemePreset.Slate => Color.FromRgb(0x52, 0x63, 0x73),
        ThemePreset.Gray => Color.FromRgb(0x5F, 0x63, 0x69),
        _ => Color.FromRgb(0x67, 0x50, 0xA4)
    };
}

