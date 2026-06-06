using Going.UI.Bindings;
using Going.UI.Containers;
using Going.UI.Controls;
using Xunit;

namespace Going.UI.Tests.Bindings;

public class GudxBindingWireTests
{
    private sealed class Src { public string Title { get; set; } = "hi"; public double Level { get; set; } = 12.345; }
    private sealed class Box { public Src Data1 { get; } = new(); }

    [Fact]
    public void AddPendingBinding_StoresExpr()
    {
        var lbl = new GoLabel();
        lbl.AddPendingBinding("Text", "{Data1.Title}");
        Assert.NotNull(lbl.PendingBindings);
        Assert.Equal("{Data1.Title}", lbl.PendingBindings!["Text"]);
    }

    [Fact]
    public void Wire_OneWay_SourceToControl_OnPump()
    {
        var lbl = new GoLabel();
        var box = new Box();
        GudxBindingExpression.Wire(lbl, "Text", "{Data1.Title}", box);

        lbl.FireUpdate();
        Assert.Equal("hi", lbl.Text);

        box.Data1.Title = "bye";
        lbl.FireUpdate();
        Assert.Equal("bye", lbl.Text);
    }

    [Fact]
    public void Wire_Format_ProducesFormattedString()
    {
        var lbl = new GoLabel();
        var box = new Box();
        GudxBindingExpression.Wire(lbl, "Text", "{Data1.Level:F1}", box);
        lbl.FireUpdate();
        Assert.Equal("12.3", lbl.Text);
    }

    [Fact]
    public void Wire_TwoWay_ControlToSource_WhenSettable()
    {
        // GoLabel.Text는 settable이고 표시형이라 IsBindingSuppressed=false (편집 정지 없음)
        var lbl = new GoLabel();
        var box = new Box();
        GudxBindingExpression.Wire(lbl, "Text", "{Data1.Title}", box);
        lbl.FireUpdate();          // 초기 동기화: source→control ("hi")

        lbl.Text = "edited";
        lbl.FireUpdate();          // control→source
        Assert.Equal("edited", box.Data1.Title);
    }

    [Fact]
    public void WireTree_RecursesContainers_AndBinds()
    {
        var box = new Box();
        var panel = new GoBoxPanel();
        var lbl = new GoLabel();
        lbl.AddPendingBinding("Text", "{Data1.Title}");
        panel.Childrens.Add(lbl);

        GudxBinder.WireTree(panel, box);

        lbl.FireUpdate();
        Assert.Equal("hi", lbl.Text);
    }

    [Fact]
    public void WireTree_BadPath_SkipsAndContinues()
    {
        var box = new Box();
        var good = new GoLabel(); good.AddPendingBinding("Text", "{Data1.Title}");
        var bad = new GoLabel(); bad.AddPendingBinding("Text", "{Nope.Missing}");
        var panel = new GoBoxPanel();
        panel.Childrens.Add(bad);
        panel.Childrens.Add(good);

        GudxBinder.WireTree(panel, box);   // 예외 없이 끝나야 함

        good.FireUpdate();
        Assert.Equal("hi", good.Text);     // 정상 바인딩은 살아있음
    }
}
