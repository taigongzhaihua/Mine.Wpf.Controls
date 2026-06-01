using System.Windows;

namespace Mine.Wpf.Controls.Controls;

/// <summary>Assist Chip — 触发单次操作，不保留选中态。</summary>
public class AssistChip : Chip
{
    static AssistChip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AssistChip), new FrameworkPropertyMetadata(typeof(AssistChip)));
    }

    public AssistChip() => Variant = ChipVariant.Assist;
}

/// <summary>Filter Chip — 可切换选中/取消，选中时显示勾选图标。</summary>
public class FilterChip : Chip
{
    static FilterChip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FilterChip), new FrameworkPropertyMetadata(typeof(FilterChip)));
    }

    public FilterChip() => Variant = ChipVariant.Filter;
}

/// <summary>Input Chip — 表示已输入的标签，带删除按钮。</summary>
public class InputChip : Chip
{
    static InputChip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(InputChip), new FrameworkPropertyMetadata(typeof(InputChip)));
    }

    public InputChip() => Variant = ChipVariant.Input;
}

/// <summary>Suggestion Chip — 智能建议快捷入口，点击即触发。</summary>
public class SuggestionChip : Chip
{
    static SuggestionChip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SuggestionChip), new FrameworkPropertyMetadata(typeof(SuggestionChip)));
    }

    public SuggestionChip() => Variant = ChipVariant.Suggestion;
}
