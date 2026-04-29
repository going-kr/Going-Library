using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxGoPaddingTests
{
    [Fact]
    public void GoPadding_formatsAsCommaSeparated()
    {
        var s = GudxSpecialConverters.FormatGoPadding(new GoPadding(1, 2, 3, 4));
        Assert.Equal("1,2,3,4", s);
    }

    [Fact]
    public void GoPadding_parsesCommaSeparated()
    {
        var p = GudxSpecialConverters.ParseGoPadding("1,2,3,4");
        Assert.Equal(1f, p.Left);
        Assert.Equal(2f, p.Top);
        Assert.Equal(3f, p.Right);
        Assert.Equal(4f, p.Bottom);
    }

    [Fact]
    public void GoButton_emitsMarginAsScalar()
    {
        // GoButton inherits GoControl.Margin
        var btn = new GoButton { Name = "btn" };
        btn.Margin = new GoPadding(5, 6, 7, 8);

        var xml = GoGudxConverter.SerializeControl(btn);

        Assert.Contains("Margin=\"5,6,7,8\"", xml);
    }

    [Fact]
    public void GoButton_roundTripsMargin()
    {
        var original = new GoButton { Name = "btn", Margin = new GoPadding(5, 6, 7, 8) };
        var xml = GoGudxConverter.SerializeControl(original);
        var restored = (GoButton)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(5f, restored.Margin.Left);
        Assert.Equal(6f, restored.Margin.Top);
        Assert.Equal(7f, restored.Margin.Right);
        Assert.Equal(8f, restored.Margin.Bottom);
    }
}
