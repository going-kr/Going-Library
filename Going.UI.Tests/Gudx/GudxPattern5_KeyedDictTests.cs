using Going.UI.Design;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxPattern5_KeyedDictTests
{
    [Fact]
    public void GoDesign_serializesPagesDictAsKeyedChildren()
    {
        var d = new GoDesign { Name = "Test", DesignWidth = 1024, DesignHeight = 600 };
        d.AddPage(new GoPage { Name = "Dashboard" });
        d.AddPage(new GoPage { Name = "Alarm" });

        var xml = GoGudxConverter.WriteAny(d).ToString();

        Assert.Contains("<GoPage Name=\"Dashboard\"", xml);
        Assert.Contains("<GoPage Name=\"Alarm\"", xml);
    }

    [Fact]
    public void GoDesign_roundTripsPagesDict()
    {
        var original = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
        original.AddPage(new GoPage { Name = "P1" });

        var xml = GoGudxConverter.WriteAny(original).ToString();
        var elem = System.Xml.Linq.XElement.Parse(xml);
        var restored = GoGudxConverter.ReadGoDesign(elem);

        Assert.NotNull(restored);
        Assert.True(restored!.Pages.ContainsKey("P1"));
    }

    [Fact]
    public void GoDesign_roundTripsBothPagesAndWindows()
    {
        // Verifies multi-[GoChilds] tag-filter routing — Task 7 review I-3.
        // Note: GoWindow.Width/Height are [JsonIgnore] so not serialized via Gudx.
        // Use Text (string) as the distinguishing property instead.
        var original = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
        original.AddPage(new GoPage { Name = "Dash" });
        original.Windows.Add("Settings", new GoWindow { Name = "Settings", Text = "Settings Window" });

        var xml = GoGudxConverter.WriteAny(original).ToString();
        var elem = System.Xml.Linq.XElement.Parse(xml);
        var restored = GoGudxConverter.ReadGoDesign(elem);

        Assert.NotNull(restored);
        Assert.True(restored!.Pages.ContainsKey("Dash"), "Dash page lost");
        Assert.True(restored.Windows.ContainsKey("Settings"), "Settings window lost — tag filter likely missing");
    }
}
