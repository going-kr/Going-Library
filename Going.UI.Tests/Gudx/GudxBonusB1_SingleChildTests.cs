using Going.UI.Design;
using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxBonusB1_SingleChildTests
{
    [Fact]
    public void GoDesign_serializesSingleChildPropertiesAsPropertyNamedTags()
    {
        var d = new GoDesign { Name = "T", DesignWidth = 1024, DesignHeight = 600 };
        d.UseTitleBar = true;

        var xml = GoGudxConverter.WriteAny(d).ToString();

        Assert.Contains("<TitleBar", xml);
        Assert.Contains("<LeftSideBar", xml);
        Assert.Contains("<RightSideBar", xml);
        Assert.Contains("<Footer", xml);
    }

    [Fact]
    public void GoDesign_emitsThemeTagViaGudxTagNameOverride()
    {
        var d = new GoDesign { Name = "T", DesignWidth = 1024, DesignHeight = 600 };
        d.CustomTheme = new GoTheme { Dark = true };

        var xml = GoGudxConverter.WriteAny(d).ToString();

        // Theme is emitted via [GudxTagName("Theme")] override on CustomTheme.
        // Class name is GoTheme; property name is CustomTheme; explicit tag is Theme.
        Assert.Contains("<Theme", xml);
        Assert.DoesNotContain("<CustomTheme", xml);
        Assert.DoesNotContain("<GoTheme", xml);
    }

    [Fact]
    public void GoDesign_roundTripsSingleChildBars()
    {
        // Verifies B1 emission + read by setting a TitleBar/Footer property NOT proxied by GoDesign.
        // BarSize is on GoTitleBar/GoFooter (not a P1 proxy on GoDesign), so its round-trip
        // requires B1 to actually emit <TitleBar BarSize="..."/> and <Footer BarSize="..."/>
        // and parse them back. UseTitleBar/UseFooter are P1 proxies and would round-trip
        // via P1 attribute alone, so they don't exercise B1.
        var original = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
        original.UseTitleBar = true;
        original.TitleBar.BarSize = 42;  // B1-only property: not a P1 proxy
        original.UseFooter = true;
        original.Footer.BarSize = 24;    // B1-only property: not a P1 proxy

        var xml = GoGudxConverter.WriteAny(original).ToString();
        var elem = System.Xml.Linq.XElement.Parse(xml);
        var restored = GoGudxConverter.ReadGoDesign(elem);

        Assert.NotNull(restored);
        // Proxy properties should still round-trip via P1 attributes
        Assert.True(restored!.UseTitleBar, "TitleBar Visible round-trip lost");
        Assert.True(restored.UseFooter, "Footer Visible round-trip lost");
        // BarSize values require actual B1 emission/parse (not just P1)
        Assert.Equal(42, restored.TitleBar.BarSize);
        Assert.Equal(24, restored.Footer.BarSize);
    }

    [Fact]
    public void GoDesign_roundTripsCustomThemeViaThemeTag()
    {
        var original = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
        original.CustomTheme = new GoTheme { Dark = true };

        var xml = GoGudxConverter.WriteAny(original).ToString();
        var elem = System.Xml.Linq.XElement.Parse(xml);
        var restored = GoGudxConverter.ReadGoDesign(elem);

        Assert.NotNull(restored);
        Assert.NotNull(restored!.CustomTheme);
        Assert.True(restored.CustomTheme!.Dark);
    }
}
