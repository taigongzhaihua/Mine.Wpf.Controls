# 控件:输入

命名空间:`Mine.Wpf.Controls.Controls`

## `TextBox`

Material 3 文本框:浮动标签、辅助文本、前后缀、清空按钮、字符计数器。

```xml
<mine:TextBox Hint="用户名" HelperText="4–16 个字符" IsClearable="True"
              LeadingIcon="person" MaxLength="16" ShowCharacterCounter="True"/>

<mine:TextBox Hint="密码" Variant="Outlined" HasError="True"
              ErrorMessage="密码不能为空"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `TextFieldVariant` | `Filled` | 变体 |
| `Hint` | `string?` | `null` | 浮动标签文字 |
| `HelperText` | `string?` | `null` | 辅助文本(标签下方) |
| `PrefixText` | `string?` | `null` | 前缀文本 |
| `SuffixText` | `string?` | `null` | 后缀文本 |
| `LeadingIcon` | `object?` | `null` | 前导图标 |
| `TrailingIcon` | `object?` | `null` | 尾部图标 |
| `IsClearable` | `bool` | `false` | 显示清空按钮 |
| `HasError` | `bool` | `false` | 错误状态(红色主题) |
| `ErrorMessage` | `string?` | `null` | 错误提示(替代 HelperText 显示) |
| `ShowCharacterCounter` | `bool` | `false` | 显示字符计数器(需配合 `MaxLength`) |
| `CornerRadius` | `CornerRadius` | `(4,4,0,0)` | 圆角 |
| `HasText` | `bool`(只读) | `false` | 是否有文本 |

### `TextFieldVariant` 枚举

| 值 | 说明 |
|---|---|
| `Filled` | 填充式(底色 + 顶部圆角,默认) |
| `Outlined` | 描边式(缺口边框 + 全圆角) |

---

## `AutoCompleteBox`

自动完成输入框,继承 `TextBox`:输入时自动过滤建议列表,支持键盘导航。

```xml
<mine:AutoCompleteBox Hint="城市"
                      SuggestionsSource="{Binding Cities}"
                      SuggestionDisplayMemberPath="Name"/>
```

### 属性(在 `TextBox` 基础上新增)

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `TextFieldVariant` | `Outlined` | 变体(覆写默认值) |
| `SuggestionsSource` | `IEnumerable?` | `null` | 建议数据源 |
| `AutoFilter` | `bool` | `true` | 按输入自动过滤 |
| `SuggestionDisplayMemberPath` | `string?` | `null` | 显示/过滤的成员路径 |
| `SuggestionItemTemplate` | `DataTemplate?` | `null` | 建议项模板 |
| `MaxSuggestions` | `int` | `8` | 最多显示条数 |
| `MinimumPrefixLength` | `int` | `0` | 触发过滤的最小输入长度 |
| `IsDropDownOpen` | `bool` | `false` | 下拉是否打开 |
| `SelectedSuggestion` | `object?` | `null` | 当前选中建议项 |
| `FilteredSuggestions` | `IEnumerable`(只读) | `null` | 过滤结果,供模板绑定 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `SuggestionSelected` | `SuggestionSelectedEventHandler` | 建议被选中。参数 `SuggestionSelectedEventArgs.SelectedItem` 为选中项 |

---

## `ComboBox`

Material 3 下拉选择框,支持单选与多选。

```xml
<mine:ComboBox Hint="国家/地区" ItemsSource="{Binding Countries}"/>

<mine:ComboBox Hint="标签" IsMultiSelect="True" ItemsSource="{Binding Tags}"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `TextFieldVariant` | `Outlined` | 变体 |
| `Hint` | `string?` | `null` | 浮动标签文字 |
| `HelperText` | `string?` | `null` | 辅助文本 |
| `HasError` | `bool` | `false` | 错误状态 |
| `IsMultiSelect` | `bool` | `false` | 多选模式 |
| `SelectionText` | `string`(只读) | `""` | 多选结果的文本表示 |
| `SelectedObjects` | `IReadOnlyList<object>`(只读) | 空集合 | 多选结果集合 |

---

## `NumericUpDown`

数字输入框:上下步进按钮、范围钳制、格式化显示。

```xml
<mine:NumericUpDown Hint="数量" Value="10" Minimum="0" Maximum="100"
                    SmallChange="5" StringFormat="F0"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `TextFieldVariant` | `Filled` | 变体 |
| `Value` | `double` | `0` | 当前值(强制钳制到 `[Minimum, Maximum]`) |
| `Minimum` | `double` | `double.MinValue` | 最小值 |
| `Maximum` | `double` | `double.MaxValue` | 最大值 |
| `SmallChange` | `double` | `1` | 步进量 |
| `StringFormat` | `string` | `"G"` | 数值格式(如 `"F2"`) |
| `Hint` | `string?` | `null` | 浮动标签文字 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `ValueChanged` | `RoutedPropertyChangedEventHandler<double>` | 数值变化 |

---

## `SearchBox`

搜索框:内置搜索图标、清空按钮、建议列表,支持 `Bar`(胶囊搜索栏)与 `Field`(紧凑字段)两种变体。

```xml
<mine:SearchBox SearchCommand="{Binding SearchCommand}" SuggestionsSource="{Binding History}"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `SearchBoxVariant` | `Bar` | 变体 |
| `Placeholder` | `string` | `"搜索"` | 占位符 |
| `IsClearable` | `bool` | `true` | 允许清空 |
| `ShowSearchButton` | `bool` | `false` | 显示右侧搜索按钮 |
| `LeadingContent` | `object?` | `null` | 前置内容(默认搜索图标) |
| `SearchCommand` | `ICommand?` | `null` | 搜索命令(Enter / 点击触发) |
| `SearchCommandParameter` | `object?` | `null` | 搜索命令参数 |
| `SuggestionsSource` | `IEnumerable?` | `null` | 建议数据源 |
| `AutoFilter` | `bool` | `true` | 按输入自动过滤 |
| `SuggestionDisplayMemberPath` | `string?` | `null` | 显示/过滤的成员路径 |
| `SuggestionItemTemplate` | `DataTemplate?` | `null` | 建议项模板 |
| `MaxSuggestions` | `int` | `8` | 最多显示条数 |
| `IsDropDownOpen` | `bool` | `false` | 下拉是否打开 |
| `SelectedSuggestion` | `object?` | `null` | 当前选中建议项 |
| `FilteredSuggestions` | `IEnumerable`(只读) | `null` | 过滤结果 |
| `HasText` | `bool`(只读) | `false` | 是否有文本 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `Search` | `RoutedEventHandler` | 触发搜索 |
| `SuggestionSelected` | `SuggestionSelectedEventHandler` | 建议被选中 |

### `SearchBoxVariant`

| 值 | 说明 |
|---|---|
| `Bar` | 胶囊型搜索栏(MD3 Search Bar) |
| `Field` | 紧凑边框型字段 |

---

## `TagBox`

标签输入框(继承 `TextBox`):回车添加标签,退格删除。

```xml
<mine:TagBox Tags="{Binding Tags}" MaxTags="5" Placeholder="输入标签后回车"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Tags` | `IList?` | `null` | 标签集合 |
| `Placeholder` | `string` | `"输入标签..."` | 占位符 |
| `AllowDuplicates` | `bool` | `false` | 允许重复标签 |
| `MaxTags` | `int` | `0` | 最大标签数(0 = 无限制) |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `TagAdded` | `TagEventHandler` | 添加标签(`TagEventArgs.Tag` 为标签对象) |
| `TagRemoved` | `TagEventHandler` | 移除标签 |
