# 原语控件

命名空间:`Mine.Wpf.Controls.Primitives`

模板级基础控件,供自绘控件模板与高级自定义使用。

## `RippleDecorator`

水波纹装饰器:给任意内容包一层,点击时播放 Material 3 水波纹动画。库内 `Button` 等控件的模板已内置;也可自行包裹任意控件。

```xml
<mine:RippleDecorator CornerRadius="12" RippleColor="#334F7DFF">
    <Border Padding="16">
        <TextBlock Text="点击我"/>
    </Border>
</mine:RippleDecorator>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `RippleColor` | `Color` | `#1EFFFFFF`(白色 30 透明度) | 波纹颜色 |
| `RippleBrush` | `Brush?` | `null` | 设置后从画笔提取颜色覆盖 `RippleColor` |
| `IsCentered` | `bool` | `false` | 波纹从控件中心扩散 |
| `CornerRadius` | `CornerRadius` | `0` | 圆角(变更时同步更新裁剪几何) |

| 方法 | 说明 |
|---|---|
| `StartRipple(Point origin)` | 在指定原点播放波纹(优先 `RippleBrush` 颜色) |

## `NotchedOutlineBorder`

顶部带缺口的描边边框(`Decorator` 子类):缺口区域留给浮动标签,用于 `TextFieldVariant.Outlined` 文本框的浮动标签效果——标签直接浮在缺口上,无需背景遮挡。

```xml
<mine:NotchedOutlineBorder BorderThickness="1" CornerRadius="4"
                          NotchStart="12" NotchWidth="40">
    <TextBlock Text="内容"/>
</mine:NotchedOutlineBorder>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `BorderBrush` | `Brush` | `Gray` | 描边画刷 |
| `BorderThickness` | `double` | `1` | 描边宽度 |
| `CornerRadius` | `double` | `4` | 圆角半径 |
| `NotchStart` | `double` | `12` | 缺口起始 X(相对控件左边) |
| `NotchWidth` | `double` | `0` | 缺口宽度;为 `0` 时不绘制缺口 |
