using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxPattern4_WrapperNodeTests
{
    [Fact]
    public void GoSwitchPanel_serializesPagesAsGoSubPageElements()
    {
        var sw = new GoSwitchPanel { Name = "tblSwitch" };
        var pageA = new GoSubPage { Name = "ModeA" };
        pageA.Childrens.Add(new GoLabel { Name = "lblA" });
        sw.Pages.Add(pageA);

        var xml = GoGudxConverter.SerializeControl(sw);

        Assert.Contains("<GoSwitchPanel", xml);
        Assert.Contains("<GoSubPage", xml);
        Assert.Contains("Name=\"ModeA\"", xml);
        Assert.Contains("<GoLabel", xml);
        Assert.Contains("Name=\"lblA\"", xml);
    }

    [Fact]
    public void GoSwitchPanel_roundTripsPagesAndChildren()
    {
        var original = new GoSwitchPanel { Name = "sw" };
        var p = new GoSubPage { Name = "M" };
        p.Childrens.Add(new GoLabel { Name = "lbl" });
        original.Pages.Add(p);

        var xml = GoGudxConverter.SerializeControl(original);
        var restored = (GoSwitchPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Single(restored.Pages);
        Assert.Equal("M", restored.Pages[0].Name);
        Assert.Single(restored.Pages[0].Childrens);
        Assert.Equal("lbl", ((GoLabel)restored.Pages[0].Childrens[0]).Name);
    }

    [Fact]
    public void GoGridLayoutPanel_emitsRowsWithChildrenNested()
    {
        var grid = new GoGridLayoutPanel { Name = "grid" };
        // AddRow helper: height + columns array
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "33%", "33%", "34%" } });
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "25%", "25%", "25%", "25%" } });
        // GoGridLayoutControlCollection.Add(control, col, row) — no ColSpan/RowSpan in GoGridIndex
        grid.Childrens.Add(new GoButton { Name = "btnA" }, 0, 0);  // row=0 child
        grid.Childrens.Add(new GoButton { Name = "btnD" }, 0, 1);  // row=1 child

        var xml = GoGudxConverter.SerializeControl(grid);

        Assert.Contains("<GoGridLayoutPanelRow", xml);
        Assert.Contains("Height=\"50%\"", xml);
        Assert.Contains("Name=\"btnA\"", xml);
        Assert.Contains("Name=\"btnD\"", xml);
    }

    [Fact]
    public void GoGridLayoutPanel_roundTripsRowsAndChildren()
    {
        var original = new GoGridLayoutPanel { Name = "grid" };
        original.Rows.Add(new GoGridLayoutPanelRow { Height = "60%", Columns = { "50%", "50%" } });
        original.Rows.Add(new GoGridLayoutPanelRow { Height = "40%", Columns = { "100%" } });
        var btnA = new GoButton { Name = "btnA" };
        var btnB = new GoButton { Name = "btnB" };
        var btnC = new GoButton { Name = "btnC" };
        original.Childrens.Add(btnA, 0, 0);
        original.Childrens.Add(btnB, 1, 0);
        original.Childrens.Add(btnC, 0, 1);

        var xml = GoGudxConverter.SerializeControl(original);
        var restored = (GoGridLayoutPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(2, restored.Rows.Count);
        Assert.Equal("60%", restored.Rows[0].Height);
        Assert.Equal(2, restored.Rows[0].Columns.Count);
        Assert.Equal(3, restored.Childrens.Controls.Count);

        // Verify cell positions preserved
        var restoredA = restored.Childrens.Controls.First(c => ((GoButton)c).Name == "btnA");
        Assert.True(restored.Childrens.Indexes.TryGetValue(restoredA.Id, out var idxA));
        Assert.Equal(0, idxA!.Column);
        Assert.Equal(0, idxA.Row);

        var restoredC = restored.Childrens.Controls.First(c => ((GoButton)c).Name == "btnC");
        Assert.True(restored.Childrens.Indexes.TryGetValue(restoredC.Id, out var idxC));
        Assert.Equal(0, idxC!.Column);
        Assert.Equal(1, idxC.Row);
    }
}
