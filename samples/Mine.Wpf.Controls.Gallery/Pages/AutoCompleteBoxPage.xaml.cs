using System.Collections.ObjectModel;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class AutoCompleteBoxPage
{
    // ── 城市列表 ────────────────────────────────────────────────
    public IReadOnlyList<string> Cities { get; } =
    [
        "北京", "上海", "广州", "深圳", "杭州", "南京", "成都", "武汉",
        "西安", "重庆", "苏州", "天津", "长沙", "郑州", "青岛", "大连",
        "厦门", "宁波", "合肥", "昆明", "沈阳", "哈尔滨", "济南", "福州",
        "石家庄", "太原", "兰州", "银川", "西宁", "乌鲁木齐", "拉萨", "呼和浩特"
    ];

    // ── 国家列表 ────────────────────────────────────────────────
    public IReadOnlyList<string> Countries { get; } =
    [
        "中国", "美国", "日本", "德国", "英国", "法国", "意大利", "加拿大",
        "澳大利亚", "韩国", "西班牙", "墨西哥", "印度尼西亚", "荷兰", "沙特阿拉伯",
        "土耳其", "瑞士", "波兰", "比利时", "瑞典", "阿根廷", "奥地利"
    ];

    // ── 产品列表 ────────────────────────────────────────────────
    public IReadOnlyList<string> Products { get; } =
    [
        "iPhone 15 Pro", "MacBook Pro", "iPad Air", "Apple Watch",
        "Surface Laptop", "Surface Pro", "Xbox Series X", "Surface Studio",
        "Galaxy S24", "Galaxy Tab", "Galaxy Watch", "Galaxy Buds",
        "ThinkPad X1", "ThinkPad T14", "ThinkCentre", "ThinkStation",
        "Dell XPS 13", "Dell Inspiron", "Alienware", "Dell Precision"
    ];

    // ── 邮箱域名列表 ────────────────────────────────────────────
    public IReadOnlyList<string> EmailDomains { get; } =
    [
        "@gmail.com", "@outlook.com", "@hotmail.com", "@yahoo.com",
        "@qq.com", "@163.com", "@126.com", "@sina.com",
        "@icloud.com", "@proton.me", "@zoho.com"
    ];

    // ── 标签列表 ────────────────────────────────────────────────
    public IReadOnlyList<string> Tags { get; } =
    [
        "编程", "设计", "音乐", "摄影", "旅行", "美食", "运动", "阅读",
        "电影", "游戏", "科技", "艺术", "时尚", "健身", "瑜伽", "冥想"
    ];

    // ── 用户列表 ────────────────────────────────────────────────
    public IReadOnlyList<UserItem> Users { get; } =
    [
        new("张三", "zhangsan@example.com"),
        new("李四", "lisi@example.com"),
        new("王五", "wangwu@example.com"),
        new("赵六", "zhaoliu@example.com"),
        new("孙七", "sunqi@example.com"),
        new("周八", "zhouba@example.com"),
        new("吴九", "wujiu@example.com"),
        new("郑十", "zhengshi@example.com"),
        new("Alice Johnson", "alice.johnson@example.com"),
        new("Bob Smith", "bob.smith@example.com"),
        new("Carol White", "carol.white@example.com"),
        new("David Brown", "david.brown@example.com")
    ];

    public AutoCompleteBoxPage() => InitializeComponent();

    private void OnSuggestionSelected(object sender, SuggestionSelectedEventArgs e)
    {
        BasicResult.Text = $"已选中：{e.SelectedItem}";
    }
}

/// <summary>用户演示数据项。</summary>
public record UserItem(string Name, string Email);
