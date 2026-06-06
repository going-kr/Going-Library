using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Containers;

public class GoStackLayoutTests
{
    private static void Layout(GoStackLayout s)
    {
        using var bmp = new SKBitmap(1, 1);
        using var canvas = new SKCanvas(bmp);
        s.FireDraw(canvas, GoTheme.DarkTheme);   // OnDraw → OnLayout
    }

    private static GoLabel Child(float w, float h)
        => new() { Width = w, Height = h, Margin = new GoPadding(0) };

    [Fact]
    public void Vertical_StacksAtNaturalHeight_WithSpacing()
    {
        var s = new GoStackLayout { Left = 0, Top = 0, Width = 200, Height = 400, Spacing = 10 };
        var a = Child(50, 20); var b = Child(50, 30); var c = Child(50, 40);
        s.Childrens.Add(a); s.Childrens.Add(b); s.Childrens.Add(c);

        Layout(s);

        // 자식 높이는 각자 그대로 (B: 자식 크기대로)
        Assert.Equal(20f, a.Height);
        Assert.Equal(30f, b.Height);
        Assert.Equal(40f, c.Height);
        // 순차 적층 + Spacing(10)
        Assert.Equal(a.Bottom + 10f, b.Top);
        Assert.Equal(b.Bottom + 10f, c.Top);
    }

    [Fact]
    public void Vertical_Near_KeepsChildWidth()
    {
        var s = new GoStackLayout { Left = 0, Top = 0, Width = 200, Height = 400, Alignment = GoStackAlignment.Near };
        var a = Child(50, 20);
        s.Childrens.Add(a);

        Layout(s);

        Assert.Equal(50f, a.Width);     // 교차축 크기 유지
    }

    [Fact]
    public void Vertical_Fill_StretchesChildWidthToContent()
    {
        var s = new GoStackLayout { Left = 0, Top = 0, Width = 200, Height = 400, Alignment = GoStackAlignment.Fill };
        var a = Child(50, 20);
        s.Childrens.Add(a);

        Layout(s);

        Assert.True(a.Width > 50f);      // 교차축을 content 너비로 채움
    }

    [Fact]
    public void Horizontal_StacksAtNaturalWidth_WithSpacing()
    {
        var s = new GoStackLayout { Left = 0, Top = 0, Width = 400, Height = 100, Direction = GoDirectionHV.Horizon, Spacing = 8 };
        var a = Child(40, 50); var b = Child(60, 50);
        s.Childrens.Add(a); s.Childrens.Add(b);

        Layout(s);

        Assert.Equal(40f, a.Width);
        Assert.Equal(60f, b.Width);
        Assert.Equal(a.Right + 8f, b.Left);
    }

    [Fact]
    public void GudxRoundTrips()
    {
        var s = new GoStackLayout { Name = "stk", Direction = GoDirectionHV.Horizon, Spacing = 12, Alignment = GoStackAlignment.Center };
        s.Childrens.Add(new GoLabel { Text = "a" });

        var xml = GoGudxConverter.SerializeControl(s);
        Assert.Contains("<GoStackLayout", xml);
        var r = (GoStackLayout)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(GoDirectionHV.Horizon, r.Direction);
        Assert.Equal(12f, r.Spacing);
        Assert.Equal(GoStackAlignment.Center, r.Alignment);
        Assert.Single(r.Childrens);
    }
}
