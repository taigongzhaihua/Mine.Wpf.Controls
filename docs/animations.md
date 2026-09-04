# 动画

命名空间:`Mine.Wpf.Controls.Animations`

## `MotionTokens`(静态类)

Material 3 动效令牌:标准时长与缓动函数。XAML 中通过 `x:Static` 引用:

```xml
Duration="{x:Static mine:MotionTokens.Medium1}"
EasingFunction="{x:Static mine:MotionTokens.EmphasizedDecelerate}"
```

### 时长令牌

| 字段 | 时长 | | 字段 | 时长 |
|---|---|---|---|---|
| `Short1` | 50ms | | `Medium1` | 250ms |
| `Short2` | 100ms | | `Medium2` | 300ms |
| `Short3` | 150ms | | `Long1` | 350ms |
| `Short4` | 200ms | | `Long2` | 450ms |
| | | | `ExtraLong1` | 550ms |
| | | | `ExtraLong2` | 700ms |

### 缓动令牌

| 属性 | 类型 | 说明 |
|---|---|---|
| `Standard` | `IEasingFunction` | 标准缓动(`EaseInOut`),通用过渡 |
| `EmphasizedDecelerate` | `IEasingFunction` | 强调减速(`EaseOut`),进入/显著元素 |
| `StandardAccelerate` | `IEasingFunction` | 标准加速(`EaseIn`),退出元素 |

### 方法

| 签名 | 说明 |
|---|---|
| `Animate(double to, Duration duration, IEasingFunction? easing = null, double? from = null)` | 快捷创建带令牌的 `DoubleAnimation` |

```csharp
var anim = MotionTokens.Animate(to: 1.0, duration: MotionTokens.Medium2, from: 0.0);
element.BeginAnimation(OpacityProperty, anim);
```

## `CornerRadiusAnimation`

针对 `CornerRadius` 属性的动画类(`AnimationTimeline` 子类),用于圆角过渡。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `From` | `CornerRadius?` | `null` | 起始圆角 |
| `To` | `CornerRadius?` | `null` | 目标圆角 |
| `EasingFunction` | `IEasingFunction?` | `null` | 缓动函数 |
| `TargetPropertyType` | `Type` | `CornerRadius` | 目标属性类型(只读) |

```csharp
var anim = new CornerRadiusAnimation
{
    To = new CornerRadius(12),
    Duration = MotionTokens.Medium1,
    EasingFunction = MotionTokens.Standard,
};
border.BeginAnimation(Border.CornerRadiusProperty, anim);
```
