using System.Linq;
using System.Xml.Linq;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxR7TabControlTests
{
    [Fact]
    public void GoTabControl_serializesTabPagesAsP4Wrappers()
    {
        var tab = new GoTabControl { Name = "tabs" };
        var pageA = new GoTabPage { Name = "TabA" };
        pageA.Childrens.Add(new GoLabel { Name = "lblA", Text = "A" });
        tab.TabPages.Add(pageA);

        var pageB = new GoTabPage { Name = "TabB" };
        pageB.Childrens.Add(new GoButton { Name = "btnB", Text = "B" });
        tab.TabPages.Add(pageB);

        var xml = GoGudxConverter.SerializeControl(tab);
        var root = XElement.Parse(xml);

        Assert.Equal("GoTabControl", root.Name.LocalName);
        var tabs = root.Elements("GoTabPage").ToList();
        Assert.Equal(2, tabs.Count);
        Assert.Equal("TabA", tabs[0].Attribute("Name")?.Value);
        Assert.Equal("TabB", tabs[1].Attribute("Name")?.Value);
        Assert.Single(tabs[0].Elements("GoLabel"));
        Assert.Single(tabs[1].Elements("GoButton"));
    }

    [Fact]
    public void GoTabControl_roundTripsTabPagesAndChildren()
    {
        var original = new GoTabControl { Name = "tabs" };
        var pa = new GoTabPage { Name = "X" };
        pa.Childrens.Add(new GoLabel { Name = "lblX" });
        original.TabPages.Add(pa);

        var xml = GoGudxConverter.SerializeControl(original);
        var restored = (GoTabControl)GoGudxConverter.DeserializeControl(xml);

        Assert.Single(restored.TabPages);
        Assert.Equal("X", restored.TabPages[0].Name);
        Assert.Single(restored.TabPages[0].Childrens);
        Assert.Equal("lblX", ((GoLabel)restored.TabPages[0].Childrens[0]).Name);
    }
}
