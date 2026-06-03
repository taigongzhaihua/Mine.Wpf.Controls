using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 TagBox 控件。
/// 支持添加、删除和显示多个标签的输入控件。
/// </summary>
[TemplatePart(Name = "PART_TagsPanel", Type = typeof(Panel))]
public class TagBox : TextBox
{
    static TagBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TagBox), new FrameworkPropertyMetadata(typeof(TagBox)));
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[TagBox] OnPreviewKeyDown - Key: {e.Key}");

        if (e.Key == Key.Enter)
        {
            System.Diagnostics.Debug.WriteLine("[TagBox] Enter detected, adding tag");
            AddTagFromInput();
            e.Handled = true;
            return;
        }
        else if (e.Key == Key.Back && string.IsNullOrEmpty(Text) && CaretIndex == 0)
        {
            System.Diagnostics.Debug.WriteLine("[TagBox] Backspace with empty text, removing last tag");
            RemoveLastTag();
            e.Handled = true;
            return;
        }

        base.OnPreviewKeyDown(e);
    }

    // ── Tags ─────────────────────────────────────────────────────
    public static readonly DependencyProperty TagsProperty =
        DependencyProperty.Register(nameof(Tags), typeof(IList), typeof(TagBox),
            new PropertyMetadata(null, OnTagsChanged));

    public IList? Tags
    {
        get => (IList?)GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    private static void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TagBox tagBox) return;

        // 取消订阅旧集合
        if (e.OldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= tagBox.OnTagsCollectionChanged;

        // 订阅新集合
        if (e.NewValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += tagBox.OnTagsCollectionChanged;

        tagBox.RefreshTags();
    }

    private void OnTagsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshTags();
    }

    // ── Placeholder ──────────────────────────────────────────────
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(TagBox),
            new PropertyMetadata("输入标签..."));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // ── AllowDuplicates ──────────────────────────────────────────
    public static readonly DependencyProperty AllowDuplicatesProperty =
        DependencyProperty.Register(nameof(AllowDuplicates), typeof(bool), typeof(TagBox),
            new PropertyMetadata(false));

    public bool AllowDuplicates
    {
        get => (bool)GetValue(AllowDuplicatesProperty);
        set => SetValue(AllowDuplicatesProperty, value);
    }

    // ── MaxTags ──────────────────────────────────────────────────
    public static readonly DependencyProperty MaxTagsProperty =
        DependencyProperty.Register(nameof(MaxTags), typeof(int), typeof(TagBox),
            new PropertyMetadata(0)); // 0 = 无限制

    public int MaxTags
    {
        get => (int)GetValue(MaxTagsProperty);
        set => SetValue(MaxTagsProperty, value);
    }

    // ── TagAdded 路由事件 ─────────────────────────────────────────
    public static readonly RoutedEvent TagAddedEvent =
        EventManager.RegisterRoutedEvent(nameof(TagAdded), RoutingStrategy.Bubble,
            typeof(TagEventHandler), typeof(TagBox));

    public event TagEventHandler TagAdded
    {
        add    => AddHandler(TagAddedEvent, value);
        remove => RemoveHandler(TagAddedEvent, value);
    }

    // ── TagRemoved 路由事件 ───────────────────────────────────────
    public static readonly RoutedEvent TagRemovedEvent =
        EventManager.RegisterRoutedEvent(nameof(TagRemoved), RoutingStrategy.Bubble,
            typeof(TagEventHandler), typeof(TagBox));

    public event TagEventHandler TagRemoved
    {
        add    => AddHandler(TagRemovedEvent, value);
        remove => RemoveHandler(TagRemovedEvent, value);
    }

    // ── Template parts ────────────────────────────────────────────
    private ItemsControl? _tagsPanel;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _tagsPanel = GetTemplateChild("PART_TagsPanel") as ItemsControl;

        RefreshTags();
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        // TextBox 自身会处理 placeholder 的显示逻辑
    }

    private void AddTagFromInput()
    {
        System.Diagnostics.Debug.WriteLine($"[TagBox] AddTagFromInput called, Text: '{Text}'");

        if (string.IsNullOrWhiteSpace(Text))
        {
            System.Diagnostics.Debug.WriteLine("[TagBox] Text is null or whitespace, aborting");
            return;
        }

        var tagText = Text.Trim();
        System.Diagnostics.Debug.WriteLine($"[TagBox] Trimmed tag text: '{tagText}'");

        // 检查是否超过最大标签数
        if (MaxTags > 0 && Tags != null && Tags.Count >= MaxTags)
        {
            System.Diagnostics.Debug.WriteLine($"[TagBox] Max tags limit reached: {MaxTags}");
            return;
        }

        // 检查是否允许重复
        if (!AllowDuplicates && Tags != null && Tags.Contains(tagText))
        {
            System.Diagnostics.Debug.WriteLine($"[TagBox] Duplicate tag not allowed: '{tagText}'");
            return;
        }

        // 添加标签
        if (Tags == null)
        {
            System.Diagnostics.Debug.WriteLine("[TagBox] Tags collection is null, creating new ObservableCollection");
            Tags = new ObservableCollection<string>();
        }

        System.Diagnostics.Debug.WriteLine($"[TagBox] Adding tag: '{tagText}'");
        Tags.Add(tagText);
        Text = string.Empty;

        // 触发事件
        System.Diagnostics.Debug.WriteLine($"[TagBox] Raising TagAdded event for: '{tagText}'");
        RaiseEvent(new TagEventArgs(TagAddedEvent, tagText));
    }

    private void RemoveLastTag()
    {
        if (Tags == null || Tags.Count == 0)
            return;

        var lastTag = Tags[Tags.Count - 1];
        Tags.RemoveAt(Tags.Count - 1);

        // 触发事件
        if (lastTag != null)
            RaiseEvent(new TagEventArgs(TagRemovedEvent, lastTag));
    }

    private void RemoveTag(object tag)
    {
        if (Tags == null || tag == null)
            return;

        Tags.Remove(tag);

        // 触发事件
        RaiseEvent(new TagEventArgs(TagRemovedEvent, tag));
    }

    private void RefreshTags()
    {
        if (_tagsPanel == null)
            return;

        _tagsPanel.Items.Clear();

        if (Tags == null)
            return;

        foreach (var tag in Tags)
        {
            var chip = new Chip
            {
                Content = tag,
                Variant = ChipVariant.Input,
                Margin = new Thickness(0, 0, 4, 4)
            };

            chip.Deleted += (_, _) => RemoveTag(tag);

            _tagsPanel.Items.Add(chip);
        }
    }
}

/// <summary>
/// TagBox 事件参数
/// </summary>
public class TagEventArgs : RoutedEventArgs
{
    public TagEventArgs(RoutedEvent routedEvent, object tag)
        : base(routedEvent)
    {
        Tag = tag;
    }

    public object Tag { get; }
}

/// <summary>
/// TagBox 事件处理器
/// </summary>
public delegate void TagEventHandler(object sender, TagEventArgs e);
