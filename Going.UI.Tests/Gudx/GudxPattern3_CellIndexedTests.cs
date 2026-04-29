using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxPattern3_CellIndexedTests
{
    [Fact]
    public void GoTableLayoutPanel_emitsCellAttributeOnChildren()
    {
        var panel = new GoTableLayoutPanel
        {
            Name = "tbl",
            Columns = { "50%", "50%" },
            Rows = { "100%" }
        };
        var btnA = new GoButton { Name = "btnA" };
        var btnB = new GoButton { Name = "btnB" };
        // Add using the col,row overload (no bare Add(IGoControl) exists)
        panel.Childrens.Add(btnA, 0, 0);
        panel.Childrens.Add(btnB, 1, 0);

        var xml = GoGudxConverter.SerializeControl(panel);

        Assert.Contains("Columns=\"50%,50%\"", xml);
        Assert.Contains("Rows=\"100%\"", xml);
        Assert.Contains("Cell=\"0,0\"", xml);
        Assert.Contains("Cell=\"1,0\"", xml);
    }

    [Fact]
    public void GoTableLayoutPanel_roundTripsCellIndexes()
    {
        var original = new GoTableLayoutPanel
        {
            Name = "tbl",
            Columns = { "50%", "50%" },
            Rows = { "100%" }
        };
        var btnA = new GoButton { Name = "btnA" };
        // GoTableIndex uses Column (not Col)
        original.Childrens.Add(btnA, 1, 0);

        var xml = GoGudxConverter.SerializeControl(original);
        var restored = (GoTableLayoutPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Single(restored.Childrens);
        var restoredBtn = restored.Childrens[0]!; // indexer returns IGoControl?, assert non-null
        Assert.Equal("btnA", ((GoButton)restoredBtn).Name);
        Assert.True(restored.Childrens.Indexes.TryGetValue(restoredBtn.Id, out var idx));
        Assert.NotNull(idx);
        Assert.Equal(1, idx!.Column);
        Assert.Equal(0, idx.Row);
    }
}
