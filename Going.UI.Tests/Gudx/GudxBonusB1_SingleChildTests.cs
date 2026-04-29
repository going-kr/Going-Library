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
        var original = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
        original.UseTitleBar = true;
        original.UseFooter = true;

        var xml = GoGudxConverter.WriteAny(original).ToString();
        var elem = System.Xml.Linq.XElement.Parse(xml);
        var restored = GoGudxConverter.ReadGoDesign(elem);

        Assert.NotNull(restored);
        // The restored bars should be present (reset to non-default Visible=true via P1 attrs)
        Assert.True(restored!.UseTitleBar, "TitleBar Visible round-trip lost");
        Assert.True(restored.UseFooter, "Footer Visible round-trip lost");
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
