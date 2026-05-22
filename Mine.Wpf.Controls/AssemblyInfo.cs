using System.Windows;
using System.Windows.Markup;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]

// Register https://schemas.mine.io/wpf namespace
[assembly: XmlnsDefinition("https://schemas.mine.io/wpf", "Mine.Wpf.Controls")]
[assembly: XmlnsDefinition("https://schemas.mine.io/wpf", "Mine.Wpf.Controls.Controls")]
[assembly: XmlnsDefinition("https://schemas.mine.io/wpf", "Mine.Wpf.Controls.Theming")]
[assembly: XmlnsDefinition("https://schemas.mine.io/wpf", "Mine.Wpf.Controls.Attached")]
[assembly: XmlnsDefinition("https://schemas.mine.io/wpf", "Mine.Wpf.Controls.Primitives")]
[assembly: XmlnsDefinition("https://schemas.mine.io/wpf", "Mine.Wpf.Controls.Markup")]
[assembly: XmlnsDefinition("https://schemas.mine.io/wpf", "Mine.Wpf.Controls.Animations")]
[assembly: XmlnsDefinition("https://schemas.mine.io/wpf", "Mine.Wpf.Controls.Converters")]
[assembly: XmlnsPrefix("https://schemas.mine.io/wpf", "mine")]

