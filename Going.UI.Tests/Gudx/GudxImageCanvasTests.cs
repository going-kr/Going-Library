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

    [Fact]
    public void IcState_roundTripsStateImages()
    {
        var st = new IcState { Name = "stateLamp", State = 2 };
        st.StateImages.Add(new StateImage { State = 0, Image = "off" });
        st.StateImages.Add(new StateImage { State = 1, Image = "warn" });
        st.StateImages.Add(new StateImage { State = 2, Image = "on" });

        var xml = GoGudxConverter.SerializeControl(st);
        var restored = (IcState)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(2, restored.State);
        Assert.Equal(3, restored.StateImages.Count);
        Assert.Equal(0, restored.StateImages[0].State);
        Assert.Equal("off", restored.StateImages[0].Image);
        Assert.Equal(1, restored.StateImages[1].State);
        Assert.Equal("warn", restored.StateImages[1].Image);
        Assert.Equal(2, restored.StateImages[2].State);
        Assert.Equal("on", restored.StateImages[2].Image);
    }
}
