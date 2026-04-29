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
        // Stronger coverage: 3 pages + 2 windows ensures tag filter doesn't drop
        // second-or-later occurrences of either type after one match.
        var original = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
        foreach (var name in new[] { "Dash", "Alarm", "Trend" })
            original.AddPage(new GoPage { Name = name });
        foreach (var name in new[] { "Settings", "About" })
            original.Windows.Add(name, new GoWindow { Name = name, Text = "win-" + name });

        var xml = GoGudxConverter.WriteAny(original).ToString();
        var elem = System.Xml.Linq.XElement.Parse(xml);
        var restored = GoGudxConverter.ReadGoDesign(elem);

        Assert.NotNull(restored);
        Assert.Equal(3, restored!.Pages.Count);
        Assert.Equal(2, restored.Windows.Count);
        foreach (var k in new[] { "Dash", "Alarm", "Trend" })
            Assert.True(restored.Pages.ContainsKey(k), $"Missing page: {k}");
        foreach (var k in new[] { "Settings", "About" })
            Assert.True(restored.Windows.ContainsKey(k), $"Missing window: {k}");
    }
}
