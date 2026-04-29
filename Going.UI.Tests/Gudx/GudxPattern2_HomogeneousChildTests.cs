using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxPattern2_HomogeneousChildTests
{
    [Fact]
    public void GoBoxPanel_serializesChildrenAsXmlChildren()
    {
        var panel = new GoBoxPanel { Name = "pnl" };
        panel.Childrens.Add(new GoButton { Name = "btnA", Text = "A" });
        panel.Childrens.Add(new GoButton { Name = "btnB", Text = "B" });

        var xml = GoGudxConverter.SerializeControl(panel);

        Assert.Contains("<GoButton", xml);
        Assert.Contains("Name=\"btnA\"", xml);
        Assert.Contains("Name=\"btnB\"", xml);
    }

    [Fact]
    public void GoBoxPanel_roundTripsChildren()
    {
        var original = new GoBoxPanel { Name = "pnl" };
        original.Childrens.Add(new GoButton { Name = "btnA", Text = "A" });
        original.Childrens.Add(new GoButton { Name = "btnB", Text = "B" });

        var xml = GoGudxConverter.SerializeControl(original);
        var restored = (GoBoxPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(2, restored.Childrens.Count);
        Assert.Equal("btnA", ((GoButton)restored.Childrens[0]).Name);
        Assert.Equal("btnB", ((GoButton)restored.Childrens[1]).Name);
        Assert.Equal("pnl", restored.Name);
    }
}
