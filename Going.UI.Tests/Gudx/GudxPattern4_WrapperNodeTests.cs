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
    public void GoGridLayoutPanel_emitsRowsAndChildrenAsSeparateGroups()
    {
        // v1.2.1: NestInto removed (XAML-style). Rows is independent metadata <Rows> group;
        // children sit flat in <Childrens> with attached Cell="col,row" attributes.
        var grid = new GoGridLayoutPanel { Name = "grid" };
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "33%", "33%", "34%" } });
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "25%", "25%", "25%", "25%" } });
        grid.Childrens.Add(new GoButton { Name = "btnA" }, 0, 0);
        grid.Childrens.Add(new GoButton { Name = "btnD" }, 0, 1);

        var xml = GoGudxConverter.SerializeControl(grid);
        var root = System.Xml.Linq.XElement.Parse(xml);

        Assert.Equal("GoGridLayoutPanel", root.Name.LocalName);

        // <Rows> group with row metadata only (no nested controls)
        var rowsGroup = root.Element("Rows");
        Assert.NotNull(rowsGroup);
        var rows = rowsGroup.Elements("GoGridLayoutPanelRow").ToList();
        Assert.Equal(2, rows.Count);
        Assert.Empty(rows[0].Elements("GoButton"));  // row metadata has no nested controls
        Assert.Empty(rows[1].Elements("GoButton"));

        // <Childrens> group with controls flat + attached Cell attributes
        var childrensGroup = root.Element("Childrens");
        Assert.NotNull(childrensGroup);
        var btnA = childrensGroup.Elements("GoButton").FirstOrDefault(e => e.Attribute("Name")?.Value == "btnA");
        Assert.NotNull(btnA);
        Assert.Equal("0,0", btnA.Attribute("Cell")?.Value);
        var btnD = childrensGroup.Elements("GoButton").FirstOrDefault(e => e.Attribute("Name")?.Value == "btnD");
        Assert.NotNull(btnD);
        Assert.Equal("0,1", btnD.Attribute("Cell")?.Value);
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
    public void GoGridLayoutPanel_RowsAndChildrensFlat_NoNesting()
    {
        // v1.2.1: ensures NestInto special case is gone — Rows and Childrens are independent groups.
        var grid = new GoGridLayoutPanel { Name = "grid" };
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "50%", "50%" } });
        grid.Childrens.Add(new GoButton { Name = "btn" }, 0, 0);

        var xml = GoGudxConverter.SerializeControl(grid);
        var root = XElement.Parse(xml);

        var rowsGroup = root.Element("Rows");
        Assert.NotNull(rowsGroup);
        var rows = rowsGroup.Elements("GoGridLayoutPanelRow").ToList();
        Assert.Single(rows);
        Assert.Empty(rows[0].Elements("GoButton"));  // no nested controls in row metadata

        var childrensGroup = root.Element("Childrens");
        Assert.NotNull(childrensGroup);
        Assert.Single(childrensGroup.Elements("GoButton"));
        Assert.Equal("0,0", childrensGroup.Element("GoButton")?.Attribute("Cell")?.Value);
    }

    [Fact]
    public void GoGridLayoutPanel_multiRowMultiCell_roundTrip()
    {
        // v1.2.1: cell positions encoded purely in attached Cell attributes; Rows is independent metadata.
        var grid = new GoGridLayoutPanel { Name = "grid" };
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "33%", "33%", "34%" } });
        grid.Rows.Add(new GoGridLayoutPanelRow { Height = "50%", Columns = { "25%", "25%", "25%", "25%" } });
        grid.Childrens.Add(new GoButton { Name = "r0c0" }, 0, 0);
        grid.Childrens.Add(new GoButton { Name = "r0c1" }, 1, 0);
        grid.Childrens.Add(new GoButton { Name = "r0c2" }, 2, 0);
        grid.Childrens.Add(new GoButton { Name = "r1c0" }, 0, 1);
        grid.Childrens.Add(new GoButton { Name = "r1c1" }, 1, 1);
        grid.Childrens.Add(new GoButton { Name = "r1c2" }, 2, 1);
        grid.Childrens.Add(new GoButton { Name = "r1c3" }, 3, 1);

        var xml = GoGudxConverter.SerializeControl(grid);
        var root = XElement.Parse(xml);

        // Structural: Rows group has 2 row metadata, Childrens group has 7 buttons flat
        var rowsGroup = root.Element("Rows");
        Assert.NotNull(rowsGroup);
        Assert.Equal(2, rowsGroup.Elements("GoGridLayoutPanelRow").Count());

        var childrensGroup = root.Element("Childrens");
        Assert.NotNull(childrensGroup);
        Assert.Equal(7, childrensGroup.Elements("GoButton").Count());

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
