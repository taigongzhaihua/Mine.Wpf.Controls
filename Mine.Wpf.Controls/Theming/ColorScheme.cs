using System.Windows.Media;
namespace Mine.Wpf.Controls.Theming;
/// <summary>
/// 保存单个主题（亮色或暗色）所有 Material 3 颜色角色的值。
/// </summary>
public sealed class ColorScheme
{
    // 主色
    public Color Primary            { get; init; }
    public Color OnPrimary          { get; init; }
    public Color PrimaryContainer   { get; init; }
    public Color OnPrimaryContainer { get; init; }
    // 次要色
    public Color Secondary             { get; init; }
    public Color OnSecondary           { get; init; }
    public Color SecondaryContainer    { get; init; }
    public Color OnSecondaryContainer  { get; init; }
    // 第三色
    public Color Tertiary             { get; init; }
    public Color OnTertiary           { get; init; }
    public Color TertiaryContainer    { get; init; }
    public Color OnTertiaryContainer  { get; init; }
    // 错误色
    public Color Error             { get; init; }
    public Color OnError           { get; init; }
    public Color ErrorContainer    { get; init; }
    public Color OnErrorContainer  { get; init; }
    // 表面色
    public Color Surface                  { get; init; }
    public Color OnSurface                { get; init; }
    public Color SurfaceVariant           { get; init; }
    public Color OnSurfaceVariant         { get; init; }
    public Color SurfaceTint              { get; init; }
    public Color SurfaceContainerLowest   { get; init; }
    public Color SurfaceContainerLow      { get; init; }
    public Color SurfaceContainer         { get; init; }
    public Color SurfaceContainerHigh     { get; init; }
    public Color SurfaceContainerHighest  { get; init; }
    // 背景色
    public Color Background   { get; init; }
    public Color OnBackground { get; init; }
    // 轮廓色
    public Color Outline        { get; init; }
    public Color OutlineVariant { get; init; }
    // 反转色
    public Color InverseSurface   { get; init; }
    public Color InverseOnSurface { get; init; }
    public Color InversePrimary   { get; init; }
    // 其他
    public Color Shadow { get; init; }
    public Color Scrim  { get; init; }
    // 语义色
    public Color Warning            { get; init; }
    public Color OnWarning          { get; init; }
    public Color WarningContainer   { get; init; }
    public Color OnWarningContainer { get; init; }
    public Color Success            { get; init; }
    public Color OnSuccess          { get; init; }
    public Color SuccessContainer   { get; init; }
    public Color OnSuccessContainer { get; init; }
    public Color Info               { get; init; }
    public Color OnInfo             { get; init; }
    public Color InfoContainer      { get; init; }
    public Color OnInfoContainer    { get; init; }
}