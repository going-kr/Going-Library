using System.Xml.Linq;
using Going.UI.Bindings;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Bindings;

[Collection("GudxComponentRegistry")]   // 정적 Registry 공유 — 병렬 race 방지
public class GudxComponentScopeTests
{
    // 고유 타입명 — simple-name 스캔 충돌 방지
    public sealed class ScopeMotorVM { public double Rpm { get; set; } = 100; }
    public sealed class ScopeOuter { public ScopeMotorVM PumpA { get; } = new(); }
    public sealed class ScopeTwoMotors { public ScopeMotorVM PumpA { get; } = new(); public ScopeMotorVM PumpB { get; } = new(); }

    private static GoComponentInstance MakeInstance(string usageXml)
    {
        GoComponentTemplate.Registry.Clear();
        GoComponentTemplate.Registry.Register(new GoComponentTemplate
        {
            Name = "MotorCard",
            Params = new()
            {
                new GoComponentParam { Name = "Title", TypeName = "string", Default = "" },
                new GoComponentParam { Name = "motor", TypeName = "ScopeMotorVM" },
            },
            Roots = new() { XElement.Parse("<GoBoxPanel><Childrens><GoLabel Text='{Title}'/><GoLabel Text='{motor.Rpm:F0}'/></Childrens></GoBoxPanel>") },
        });
        return (GoComponentInstance)GoGudxConverter.ReadElement(XElement.Parse(usageXml));
    }

    [Fact]
    public void Scope_Build_ResolvesLiteralAndBindingParams()
    {
        var ci = MakeInstance("<MotorCard Title='펌프 A' motor='{PumpA}'/>");
        var outer = new ScopeOuter();
        var scope = GoComponentScope.Build(ci, outer);

        Assert.Equal("펌프 A", scope.Values["Title"]);
        Assert.Same(outer.PumpA, scope.Values["motor"]);   // 객체 참조 스냅샷
        Assert.Equal(typeof(ScopeMotorVM), scope.Types["motor"]);
    }

    [Fact]
    public void WireScoped_ValueParam_OneWay()
    {
        var ci = MakeInstance("<MotorCard Title='펌프 A' motor='{PumpA}'/>");
        var scope = GoComponentScope.Build(ci, new ScopeOuter());
        var lbl = new GoLabel();
        GudxBindingExpression.WireScoped(lbl, "Text", "{Title}", scope);
        lbl.FireUpdate();
        Assert.Equal("펌프 A", lbl.Text);
    }

    [Fact]
    public void WireScoped_ObjectParamPath_TracksLiveValue()
    {
        var ci = MakeInstance("<MotorCard Title='펌프 A' motor='{PumpA}'/>");
        var outer = new ScopeOuter();
        var scope = GoComponentScope.Build(ci, outer);
        var lbl = new GoLabel();
        GudxBindingExpression.WireScoped(lbl, "Text", "{motor.Rpm:F0}", scope);

        lbl.FireUpdate();
        Assert.Equal("100", lbl.Text);
        outer.PumpA.Rpm = 250;
        lbl.FireUpdate();
        Assert.Equal("250", lbl.Text);   // 참조 통해 live
    }

    [Fact]
    public void WireTree_Component_BindsAgainstScope_PerInstance()
    {
        GoComponentTemplate.Registry.Clear();
        GoComponentTemplate.Registry.Register(new GoComponentTemplate
        {
            Name = "MotorCard",
            Params = new() { new GoComponentParam { Name = "motor", TypeName = "ScopeMotorVM" } },
            Roots = new() { XElement.Parse("<GoBoxPanel><Childrens><GoLabel Text='{motor.Rpm:F0}'/></Childrens></GoBoxPanel>") },
        });

        var outer = new ScopeTwoMotors();
        var a = (GoComponentInstance)GoGudxConverter.ReadElement(XElement.Parse("<MotorCard motor='{PumpA}'/>"));
        var b = (GoComponentInstance)GoGudxConverter.ReadElement(XElement.Parse("<MotorCard motor='{PumpB}'/>"));

        GudxBinder.WireTree(a, outer);
        GudxBinder.WireTree(b, outer);

        outer.PumpA.Rpm = 11; outer.PumpB.Rpm = 22;
        var lblA = (GoLabel)((GoBoxPanel)a.Childrens[0]).Childrens[0];
        var lblB = (GoLabel)((GoBoxPanel)b.Childrens[0]).Childrens[0];
        lblA.FireUpdate();
        lblB.FireUpdate();

        Assert.Equal("11", lblA.Text);
        Assert.Equal("22", lblB.Text);
    }

    [Fact]
    public void WireTree_DepthDoesNotThrow_OnNormalNesting()
    {
        GoComponentTemplate.Registry.Clear();
        GoComponentTemplate.Registry.Register(new GoComponentTemplate
        {
            Name = "Inner",
            Params = new() { new GoComponentParam { Name = "motor", TypeName = "ScopeMotorVM" } },
            Roots = new() { XElement.Parse("<GoLabel Text='{motor.Rpm:F0}'/>") },
        });
        var outer = new ScopeTwoMotors();
        var inst = (GoComponentInstance)GoGudxConverter.ReadElement(XElement.Parse("<Inner motor='{PumpA}'/>"));
        GudxBinder.WireTree(inst, outer);   // 예외 없이 종료
    }
}
