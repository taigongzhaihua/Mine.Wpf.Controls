using System.Windows;
using System.Windows.Markup;
namespace Mine.Wpf.Controls.Markup;
/// <summary>
/// MarkupExtension，用于按键名获取主题资源，
/// 并在 ThemeManager.ThemeChanged 触发时自动刷新绑定。
/// 用法：{mine:ThemeResource Brush.Primary}
/// </summary>
[MarkupExtensionReturnType(typeof(object))]
public sealed class ThemeResourceExtension : MarkupExtension
{
    public ThemeResourceExtension() { }
    public ThemeResourceExtension(string resourceKey) => ResourceKey = resourceKey;
    /// <summary>不含 "Mine." 前缀的资源键，例如 "Brush.Primary"。</summary>
    [ConstructorArgument("resourceKey")]
    public string? ResourceKey { get; set; }
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (ResourceKey is null) return null;
        var fullKey = ResourceKey.StartsWith("Mine.", StringComparison.Ordinal)
                          ? ResourceKey
                          : $"Mine.{ResourceKey}";
        // 返回 DynamicResourceExtension 以保持绑定持续有效
        var dynRes = new DynamicResourceExtension(fullKey);
        return dynRes.ProvideValue(serviceProvider);
    }
}