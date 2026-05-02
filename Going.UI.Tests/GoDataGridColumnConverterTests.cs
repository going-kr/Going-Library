using System.Text.Json;
using Going.UI.Datas;
using Going.UI.Json;
using Xunit;

namespace Going.UI.Tests;

public class GoDataGridColumnConverterTests
{
    [Fact]
    public void Registry_NonGeneric_Registered()
    {
        Assert.Contains("GoDataGridLabelColumn", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridButtonColumn", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridCheckBoxColumn", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridLampColumn", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridInputTextColumn", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridInputBoolColumn", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridInputTimeColumn", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridInputColorColumn", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridInputComboColumn", GoJsonConverter.ColumnTypes.Keys);
    }

    [Fact]
    public void Registry_NumericGeneric_RegisteredWithBothAliases()
    {
        Assert.Contains("GoDataGridNumberColumn<int>", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridNumberColumn<Int32>", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridNumberColumn<double>", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridInputNumberColumn<float>", GoJsonConverter.ColumnTypes.Keys);
        Assert.Contains("GoDataGridInputNumberColumn<Single>", GoJsonConverter.ColumnTypes.Keys);

        Assert.Same(
            GoJsonConverter.ColumnTypes["GoDataGridNumberColumn<int>"],
            GoJsonConverter.ColumnTypes["GoDataGridNumberColumn<Int32>"]);
    }

    [Fact]
    public void RoundTrip_LabelColumn_Preserved()
    {
        GoDataGridColumn col = new GoDataGridLabelColumn
        {
            Name = "n",
            HeaderText = "헤더",
            Size = "120",
            UseFilter = true,
            UseSort = true,
            Fixed = true,
        };

        var json = JsonSerializer.Serialize(col, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<GoDataGridColumn>(json, GoJsonConverter.Options);

        var t = Assert.IsType<GoDataGridLabelColumn>(back);
        Assert.Equal("n", t.Name);
        Assert.Equal("헤더", t.HeaderText);
        Assert.Equal("120", t.Size);
        Assert.True(t.UseFilter);
        Assert.True(t.UseSort);
        Assert.True(t.Fixed);
        Assert.Contains("GoDataGridLabelColumn", json);
    }

    [Fact]
    public void RoundTrip_NumericGeneric_Preserved()
    {
        GoDataGridColumn col = new GoDataGridNumberColumn<int>
        {
            Name = "score",
            HeaderText = "점수",
            FormatString = "0.00",
        };

        var json = JsonSerializer.Serialize(col, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<GoDataGridColumn>(json, GoJsonConverter.Options);

        var t = Assert.IsType<GoDataGridNumberColumn<int>>(back);
        Assert.Equal("score", t.Name);
        Assert.Equal("0.00", t.FormatString);
        // Type tag는 round-trip으로 검증 (System.Text.Json이 '<','>'를 </>로 escape)
        Assert.Contains("GoDataGridNumberColumn", json);
    }

    [Fact]
    public void RoundTrip_HeterogeneousList_Preserved()
    {
        var list = new List<GoDataGridColumn>
        {
            new GoDataGridLabelColumn { Name = "a" },
            new GoDataGridNumberColumn<double> { Name = "b" },
            new GoDataGridInputBoolColumn { Name = "c" },
            new GoDataGridInputNumberColumn<int> { Name = "d" },
        };

        var json = JsonSerializer.Serialize(list, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<List<GoDataGridColumn>>(json, GoJsonConverter.Options);

        Assert.NotNull(back);
        Assert.Equal(4, back!.Count);
        Assert.IsType<GoDataGridLabelColumn>(back[0]);
        Assert.IsType<GoDataGridNumberColumn<double>>(back[1]);
        Assert.IsType<GoDataGridInputBoolColumn>(back[2]);
        Assert.IsType<GoDataGridInputNumberColumn<int>>(back[3]);
        Assert.Equal("d", back[3].Name);
    }
}
