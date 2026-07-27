using Going.UI.Controls;
using Going.UI.ImageCanvas;
using Going.UI.Json;
using System.Text.Json;
using Xunit;

namespace Going.UI.Tests.Json;

public class JsonImageCanvasTests
{
    [Fact]
    public void IcContainer_json_roundTripsChildren()
    {
        var container = new IcContainer { Name = "ic", OffImage = "bg_off", OnImage = "bg_on" };
        container.Childrens.Add(new IcButton { Name = "btn1", Text = "go" });
        container.Childrens.Add(new IcOnOff { Name = "sw1" });

        var json = JsonSerializer.Serialize<IGoControl>(container, GoJsonConverter.Options);
        var restored = (IcContainer)JsonSerializer.Deserialize<IGoControl>(json, GoJsonConverter.Options)!;

        Assert.Equal(2, restored.Childrens.Count);
        Assert.IsType<IcButton>(restored.Childrens[0]);
        Assert.Equal("btn1", restored.Childrens[0].Name);
        Assert.Equal("go", ((IcButton)restored.Childrens[0]).Text);
        Assert.IsType<IcOnOff>(restored.Childrens[1]);
        Assert.Equal("sw1", restored.Childrens[1].Name);
        Assert.Equal("bg_off", restored.OffImage);
        Assert.Equal("bg_on", restored.OnImage);
    }

    [Fact]
    public void IcContainer_json_roundTripsNestedContainer()
    {
        var outer = new IcContainer { Name = "outer" };
        var inner = new IcContainer { Name = "inner" };
        inner.Childrens.Add(new IcOnOff { Name = "deep" });
        outer.Childrens.Add(inner);

        var json = JsonSerializer.Serialize<IGoControl>(outer, GoJsonConverter.Options);
        var restored = (IcContainer)JsonSerializer.Deserialize<IGoControl>(json, GoJsonConverter.Options)!;

        var restoredInner = Assert.IsType<IcContainer>(Assert.Single(restored.Childrens));
        Assert.Equal("inner", restoredInner.Name);
        var deep = Assert.IsType<IcOnOff>(Assert.Single(restoredInner.Childrens));
        Assert.Equal("deep", deep.Name);
    }
}
