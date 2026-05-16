using Going.UI.Gudx;
using Going.UI.ImageCanvas;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxImageCanvasTests
{
    [Fact]
    public void IcContainer_roundTripsChildren()
    {
        var container = new IcContainer { Name = "ic", OffImage = "bg_off", OnImage = "bg_on" };
        container.Childrens.Add(new IcButton { Name = "btn1", Text = "go" });
        container.Childrens.Add(new IcLabel { Name = "lbl1" });

        var xml = GoGudxConverter.SerializeControl(container);
        var restored = (IcContainer)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(2, restored.Childrens.Count);
        Assert.IsType<IcButton>(restored.Childrens[0]);
        Assert.Equal("btn1", restored.Childrens[0].Name);
        Assert.Equal("go", ((IcButton)restored.Childrens[0]).Text);
        Assert.IsType<IcLabel>(restored.Childrens[1]);
        Assert.Equal("bg_off", restored.OffImage);
        Assert.Equal("bg_on", restored.OnImage);
    }
}
