#:package System.Reflection.MetadataLoadContext@10.0.0

// 用法: dotnet run Tools/ExtractApi.cs -- <dll路径> [输出文件]
// 从编译产物中提取全部公开 API 清单,作为 API 文档的事实依据。
using System.Reflection;
using System.Text;

if (args.Length < 1)
{
    Console.Error.WriteLine("用法: dotnet run Tools/ExtractApi.cs -- <dll路径> [输出文件]");
    return 1;
}

var dll = Path.GetFullPath(args[0]);
if (!File.Exists(dll))
{
    Console.Error.WriteLine($"找不到文件: {dll}");
    return 1;
}

// 同时解析 .NET 与 WindowsDesktop 两个 shared framework 目录,以覆盖 WPF 类型依赖
var netcoreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
var desktopRoot = Path.GetFullPath(Path.Combine(netcoreDir, "..", "..", "Microsoft.WindowsDesktop.App"));
var desktopDir = Directory.Exists(desktopRoot)
    ? Directory.GetDirectories(desktopRoot).OrderByDescending(x => x).First()
    : null;

var files = Directory.GetFiles(netcoreDir, "*.dll").ToList();
if (desktopDir is not null)
    files.AddRange(Directory.GetFiles(desktopDir, "*.dll"));
files.Add(dll);

var resolver = new PathAssemblyResolver(files);
using var mlc = new MetadataLoadContext(resolver);
var asm = mlc.LoadFromAssemblyPath(dll);

var sb = new StringBuilder();
foreach (var t in asm.GetExportedTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
{
    if (t.IsEnum)
    {
        sb.AppendLine($"== enum {t.FullName}");
        foreach (var name in Enum.GetNames(t))
            sb.AppendLine($"   {name}");
        sb.AppendLine();
        continue;
    }
    sb.AppendLine($"== {(t.IsInterface ? "interface " : t.IsAbstract && t.IsSealed ? "static class " : "class ")}{t.FullName}");
    if (t.BaseType is not null && t.BaseType != typeof(object))
        sb.AppendLine($"   base: {t.BaseType.FullName}");

    // 公开静态字段(DependencyProperty 等)
    foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(f => f.Name))
        sb.AppendLine($"   static field {f.Name} : {TypeName(f.FieldType)}");

    // 公开属性(实例 + 静态,仅本类型声明)
    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(p => p.Name))
        sb.AppendLine($"   prop {(p.GetMethod?.IsStatic == true || p.SetMethod?.IsStatic == true ? "static " : "")}{p.Name} : {TypeName(p.PropertyType)}{ReadWrite(p)}");

    // 公开事件(实例 + 静态,仅本类型声明)
    foreach (var ev in t.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(e => e.Name))
        sb.AppendLine($"   event {(ev.AddMethod?.IsStatic == true ? "static " : "")}{ev.Name}");

    // 公开方法(排除属性/事件访问器;控制类不列继承方法,静态工具类则列静态方法)
    var methodFlags = t.IsAbstract && t.IsSealed
        ? BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly
        : BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
    foreach (var m in t.GetMethods(methodFlags).Where(m => !m.IsSpecialName).OrderBy(m => m.Name))
        sb.AppendLine($"   method {(m.IsStatic ? "static " : "")}{m.Name}({string.Join(", ", m.GetParameters().Select(pa => $"{TypeName(pa.ParameterType)} {pa.Name}"))}) : {TypeName(m.ReturnType)}");

    sb.AppendLine();
}

var text = sb.ToString();
if (args.Length > 1)
{
    File.WriteAllText(Path.GetFullPath(args[1]), text);
    Console.WriteLine($"已写入 {Path.GetFullPath(args[1])} ({text.Length} 字符)");
}
else
{
    Console.Write(text);
}
return 0;

static string TypeName(Type type)
{
    if (type.IsByRef) return TypeName(type.GetElementType()!) + "&";
    if (type.IsArray) return TypeName(type.GetElementType()!) + "[]";
    if (!type.IsGenericType) return type.FullName ?? type.Name;
    var def = (type.FullName ?? type.Name).Split('`')[0];
    return $"{def}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>";
}

static string ReadWrite(PropertyInfo p) =>
    p.CanRead && p.CanWrite ? " { get; set; }" : p.CanRead ? " { get; }" : " { set; }";
