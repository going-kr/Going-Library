using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using System.Linq;
using System.Xml.Linq;
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
        var root = System.Xml.Linq.XElement.Parse(xml);

        // Root is GoGridLayoutPanel
        Assert.Equal("GoGridLayoutPanel", root.Name.LocalName);

        // Two row wrappers as direct children
        var rows = root.Elements("GoGridLayoutPanelRow").ToList();
        Assert.Equal(2, rows.Count);

        // btnA is direct child of first row, NOT of root
        Assert.Empty(root.Elements("GoButton"));  // no buttons at root level
        var btnAInRow0 = rows[0].Elements("GoButton").FirstOrDefault(e => e.Attribute("Name")?.Value == "btnA");
        Assert.NotNull(btnAInRow0);

        // btnD is direct child of second row
        var btnDInRow1 = rows[1].Elements("GoButton").FirstOrDefault(e => e.Attribute("Name")?.Value == "btnD");
        Assert.NotNull(btnDInRow1);
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

    [Fact]
    public void GoGridLayoutPanel_emitsViaNestIntoAttributeOnly()
    {
        // Verifies the special case is gone and nested emission is attribute-driven.
        var grid = new GoGridLayoutPanel { Name = "grid" };
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "50%", "50%" } });
        grid.Childrens.Add(new GoButton { Name = "btn" }, 0, 0);

        var xml = GoGudxConverter.SerializeControl(grid);
        var root = XElement.Parse(xml);

        var rows = root.Elements("GoGridLayoutPanelRow").ToList();
        Assert.Single(rows);
        Assert.Single(rows[0].Elements("GoButton"));
        Assert.Empty(root.Elements("GoButton"));  // not at root
    }

    [Fact]
    public void GoGridLayoutPanel_NestInto_multiRowMultiCell_roundTrip()
    {
        // 2 rows × 3+4 cells, ensures bucketing by Row index works for non-trivial layouts.
        var grid = new GoGridLayoutPanel { Name = "grid" };
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "33%", "33%", "34%" } });
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "25%", "25%", "25%", "25%" } });
        // Row 0 — 3 buttons
        grid.Childrens.Add(new GoButton { Name = "r0c0" }, 0, 0);
        grid.Childrens.Add(new GoButton { Name = "r0c1" }, 1, 0);
        grid.Childrens.Add(new GoButton { Name = "r0c2" }, 2, 0);
        // Row 1 — 4 buttons
        grid.Childrens.Add(new GoButton { Name = "r1c0" }, 0, 1);
        grid.Childrens.Add(new GoButton { Name = "r1c1" }, 1, 1);
        grid.Childrens.Add(new GoButton { Name = "r1c2" }, 2, 1);
        grid.Childrens.Add(new GoButton { Name = "r1c3" }, 3, 1);

        var xml = GoGudxConverter.SerializeControl(grid);
        var root = XElement.Parse(xml);

        // Structural: 2 rows, each containing the right number of children
        var rows = root.Elements("GoGridLayoutPanelRow").ToList();
        Assert.Equal(2, rows.Count);
        Assert.Equal(3, rows[0].Elements("GoButton").Count());
        Assert.Equal(4, rows[1].Elements("GoButton").Count());
        Assert.Empty(root.Elements("GoButton")); // no buttons at root

        // Round-trip: deserialize and verify Indexes are populated
        var restored = (GoGridLayoutPanel)GoGudxConverter.DeserializeControl(xml);
        Assert.Equal(2, restored.Rows.Count);
        Assert.Equal(7, restored.Childrens.Controls.Count);
        foreach (var c in restored.Childrens.Controls)
        {
            Assert.True(restored.Childrens.Indexes.TryGetValue(c.Id, out var idx));
            var name = ((GoButton)c).Name;
            // names are "r{row}c{col}"
            var expectedRow = int.Parse(name!.Substring(1, 1));
            var expectedCol = int.Parse(name.Substring(3, 1));
            Assert.Equal(expectedRow, idx.Row);
            Assert.Equal(expectedCol, idx.Column);
        }
    }
}
