using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxSpecialTypeTests
{
    [Fact]
    public void SKColor_formatsAsHashARGBHex()
    {
        var s = GudxSpecialConverters.FormatSKColor(new SKColor(0x1A, 0x2B, 0x3C, 0xFF));
        Assert.Equal("#FF1A2B3C", s);
    }

    [Fact]
    public void SKColor_parsesHashARGBHex()
    {
        var c = GudxSpecialConverters.ParseSKColor("#FF1A2B3C");
        Assert.Equal((byte)0xFF, c.Alpha);
        Assert.Equal((byte)0x1A, c.Red);
        Assert.Equal((byte)0x2B, c.Green);
        Assert.Equal((byte)0x3C, c.Blue);
    }

    [Fact]
    public void SKColor_acceptsRGBOnlyHex()
    {
        // 6-char hex without alpha → assumes opaque
        var c = GudxSpecialConverters.ParseSKColor("#1A2B3C");
        Assert.Equal((byte)0xFF, c.Alpha);
        Assert.Equal((byte)0x1A, c.Red);
    }

    [Fact]
    public void SKRect_formatsAsCommaSeparated()
    {
        var s = GudxSpecialConverters.FormatSKRect(new SKRect(1, 2, 3, 4));
        Assert.Equal("1,2,3,4", s);
    }

    [Fact]
    public void SKRect_parsesCommaSeparated()
    {
        var r = GudxSpecialConverters.ParseSKRect("1,2,3,4");
        Assert.Equal(1f, r.Left);
        Assert.Equal(2f, r.Top);
        Assert.Equal(3f, r.Right);
        Assert.Equal(4f, r.Bottom);
    }

    [Fact]
    public void GoTheme_roundTripsColorsViaB1()
    {
        // Integration: SKColor properties on GoTheme survive B1 round-trip via Gudx attributes.
        var d = new Going.UI.Design.GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
        d.CustomTheme = new GoTheme
        {
            Dark = true,
            Fore = SKColors.White,
            Back = new SKColor(0x1A, 0x1A, 0x1A, 0xFF)
        };

        var xml = GoGudxConverter.WriteAny(d).ToString();
        var elem = System.Xml.Linq.XElement.Parse(xml);
        var restored = GoGudxConverter.ReadGoDesign(elem);

        Assert.NotNull(restored);
        Assert.NotNull(restored!.CustomTheme);
        Assert.True(restored.CustomTheme!.Dark);
        Assert.Equal(SKColors.White, restored.CustomTheme.Fore);
        Assert.Equal(new SKColor(0x1A, 0x1A, 0x1A, 0xFF), restored.CustomTheme.Back);
    }
}
