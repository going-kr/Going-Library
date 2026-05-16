using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Containers;

public class PaddingTests
{
    [Fact]
    public void GoBoxPanel_PanelBounds_insetByPadding()
    {
        var p = new GoBoxPanel { Left = 0, Top = 0, Width = 200, Height = 150 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;

        Assert.Equal(10f, pb.Left);
        Assert.Equal(5f, pb.Top);
        Assert.Equal(200f - 20f, pb.Right);
        Assert.Equal(150f - 15f, pb.Bottom);
    }

    [Fact]
    public void GoBoxPanel_Padding_gudxRoundTrips()
    {
        var p = new GoBoxPanel { Name = "bp" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (GoBoxPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }

    [Fact]
    public void GoBoxPanel_DockFillChild_respectsPadding()
    {
        var p = new GoBoxPanel { Left = 0, Top = 0, Width = 200, Height = 150 };
        p.Padding = new GoPadding(10, 10, 10, 10);

        var child = new GoButton
        {
            Dock = GoDockStyle.Fill,
            Margin = new GoPadding(0, 0, 0, 0)
        };
        p.Childrens.Add(child);

        // FireDraw가 OnDraw → OnLayout 트리거. 실제 픽셀은 안 그리지만 레이아웃 계산은 일어남.
        using var bmp = new SKBitmap(1, 1);
        using var canvas = new SKCanvas(bmp);
        p.FireDraw(canvas, GoTheme.DarkTheme);

        Assert.Equal(0f, child.Left);
        Assert.Equal(0f, child.Top);
        Assert.Equal(180f, child.Right);   // PanelBounds.Width = 200 - 10 - 10 = 180
        Assert.Equal(130f, child.Bottom);  // PanelBounds.Height = 150 - 10 - 10 = 130
    }

    [Fact]
    public void GoPage_PanelBounds_insetByPadding()
    {
        var p = new Going.UI.Design.GoPage { Left = 0, Top = 0, Width = 800, Height = 600 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;

        Assert.Equal(10f, pb.Left);
        Assert.Equal(5f, pb.Top);
        Assert.Equal(800f - 20f, pb.Right);
        Assert.Equal(600f - 15f, pb.Bottom);
    }

    [Fact]
    public void GoPage_Padding_gudxRoundTrips()
    {
        var p = new Going.UI.Design.GoPage { Name = "pg" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (Going.UI.Design.GoPage)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }

    [Fact]
    public void GoSideBar_PanelBounds_insetByPadding()
    {
        var p = new Going.UI.Design.GoSideBar { Left = 0, Top = 0, Width = 150, Height = 600 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;

        Assert.Equal(10f, pb.Left);
        Assert.Equal(5f, pb.Top);
        Assert.Equal(150f - 20f, pb.Right);
        Assert.Equal(600f - 15f, pb.Bottom);
    }

    [Fact]
    public void GoSideBar_Padding_gudxRoundTrips()
    {
        var p = new Going.UI.Design.GoSideBar { Name = "sb" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (Going.UI.Design.GoSideBar)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }

    [Fact]
    public void GoPanel_PanelBounds_insetByPadding()
    {
        var p = new GoPanel { Left = 0, Top = 0, Width = 200, Height = 150 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;
        var panelArea = p.Areas()["Panel"];

        Assert.Equal(panelArea.Left + 10f, pb.Left);
        Assert.Equal(panelArea.Top + 5f, pb.Top);
        Assert.Equal(panelArea.Right - 20f, pb.Right);
        Assert.Equal(panelArea.Bottom - 15f, pb.Bottom);
    }

    [Fact]
    public void GoPanel_Padding_gudxRoundTrips()
    {
        var p = new GoPanel { Name = "pnl" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (GoPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }

    [Fact]
    public void GoGroupBox_PanelBounds_insetByPadding()
    {
        var p = new GoGroupBox { Left = 0, Top = 0, Width = 200, Height = 150 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;
        var panelArea = p.Areas()["Panel"];

        Assert.Equal(panelArea.Left + 10f, pb.Left);
        Assert.Equal(panelArea.Top + 5f, pb.Top);
        Assert.Equal(panelArea.Right - 20f, pb.Right);
        Assert.Equal(panelArea.Bottom - 15f, pb.Bottom);
    }

    [Fact]
    public void GoGroupBox_Padding_gudxRoundTrips()
    {
        var p = new GoGroupBox { Name = "gb" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (GoGroupBox)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }
}
