using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxBindingRoundTripTests
{
    [Fact]
    public void Deserialize_BindingAttr_BecomesPendingBinding_NotLiteral()
    {
        var xml = "<GoLabel Text=\"{Data1.Title}\" />";
        var ctrl = (GoLabel)GoGudxConverter.DeserializeControl(xml);

        Assert.NotNull(ctrl.PendingBindings);
        Assert.Equal("{Data1.Title}", ctrl.PendingBindings!["Text"]);
        // 리터럴로 새지 않음: 바인딩 식이 Text에 그대로 박히지 않음
        Assert.NotEqual("{Data1.Title}", ctrl.Text);
    }

    [Fact]
    public void Deserialize_LiteralAttr_StillParsesNormally()
    {
        var ctrl = (GoLabel)GoGudxConverter.DeserializeControl("<GoLabel Text=\"Hello\" />");
        Assert.Equal("Hello", ctrl.Text);
        Assert.Null(ctrl.PendingBindings);
    }

    [Fact]
    public void RoundTrip_BindingExpr_Preserved()
    {
        var ctrl = (GoLabel)GoGudxConverter.DeserializeControl("<GoLabel Text=\"{Data1.Title}\" />");
        var elem = GoGudxConverter.WriteAny(ctrl);
        Assert.Equal("{Data1.Title}", elem.Attribute("Text")?.Value);   // 리터럴화 안 됨
    }
}
