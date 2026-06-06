using System;
using System.Xml.Linq;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

[Collection("GudxComponentRegistry")]   // 정적 Registry 공유 — 병렬 race 방지
public class GudxComponentTests
{
    // 고유 타입명 — simple-name 스캔이 다른 테스트의 동명 타입에 걸리지 않도록
    public sealed class CompTestVm { public double Rpm { get; set; } = 100; }

    [Theory]
    [InlineData("string", typeof(string))]
    [InlineData("int", typeof(int))]
    [InlineData("double", typeof(double))]
    [InlineData("bool", typeof(bool))]
    public void Param_ResolvesAliasTypes(string name, Type expected)
        => Assert.Equal(expected, GoComponentParam.ResolveType(name));

    [Fact]
    public void Param_ResolvesUserTypeBySimpleName()
        => Assert.Equal(typeof(CompTestVm), GoComponentParam.ResolveType("CompTestVm"));

    [Fact]
    public void Param_UnknownType_ReturnsNull()
        => Assert.Null(GoComponentParam.ResolveType("NoSuchTypeXyz123"));

    [Fact]
    public void Registry_RegisterAndLookup()
    {
        GoComponentTemplate.Registry.Clear();
        var tpl = new GoComponentTemplate
        {
            Name = "RegCard",
            Params = new() { new GoComponentParam { Name = "Title", TypeName = "string" } },
            Roots = new() { XElement.Parse("<GoLabel Text=\"{Title}\"/>") },
        };
        GoComponentTemplate.Registry.Register(tpl);

        Assert.True(GoComponentTemplate.Registry.TryGet("RegCard", out var got));
        Assert.Same(tpl, got);
        Assert.False(GoComponentTemplate.Registry.TryGet("Nope", out _));
    }

    [Fact]
    public void Instance_HoldsNameParamsChildren()
    {
        var ci = new GoComponentInstance { ComponentName = "MotorCard" };
        ci.ParamValues["Title"] = "펌프 A";
        ci.Childrens.Add(new GoLabel());

        Assert.Equal("MotorCard", ci.ComponentName);
        Assert.Equal("펌프 A", ci.ParamValues["Title"]);
        Assert.Single(ci.Childrens);
    }

    [Fact]
    public void Deserialize_RegistersComponents_AndInstantiatesUsage()
    {
        var xml = @"<GoDesign>
          <Components>
            <GoComponent name='MotorCard'>
              <Param name='Title' type='string' default=''/>
              <GoBoxPanel><Childrens><GoLabel Text='{Title}'/></Childrens></GoBoxPanel>
            </GoComponent>
          </Components>
          <Pages>
            <GoPage Name='Main'><Childrens><MotorCard Title='펌프 A'/></Childrens></GoPage>
          </Pages>
        </GoDesign>";

        var d = GoGudxConverter.ReadGoDesign(XElement.Parse(xml));
        Assert.NotNull(d);
        Assert.True(GoComponentTemplate.Registry.TryGet("MotorCard", out _));

        var page = d!.Pages["Main"];
        var inst = Assert.IsType<GoComponentInstance>(page.Childrens[0]);
        Assert.Equal("MotorCard", inst.ComponentName);
        Assert.Equal("펌프 A", inst.ParamValues["Title"]);

        var box = Assert.IsType<GoBoxPanel>(inst.Childrens[0]);
        var lbl = Assert.IsType<GoLabel>(box.Childrens[0]);
        Assert.Equal("{Title}", lbl.PendingBindings!["Text"]);
    }

    [Fact]
    public void RoundTrip_Instance_EmitsNameAndParams_NoChildren()
    {
        GoComponentTemplate.Registry.Clear();
        GoComponentTemplate.Registry.Register(new GoComponentTemplate
        {
            Name = "MotorCard",
            Params = new() { new GoComponentParam { Name = "Title", TypeName = "string" } },
            Roots = new() { XElement.Parse("<GoBoxPanel><Childrens><GoLabel Text='{Title}'/></Childrens></GoBoxPanel>") },
        });
        var ci = (GoComponentInstance)GoGudxConverter.ReadElement(
            XElement.Parse("<MotorCard Title='펌프 A' Bounds='0,0,100,60'/>"));

        var e = GoGudxConverter.WriteAny(ci);
        Assert.Equal("MotorCard", e.Name.LocalName);
        Assert.Equal("펌프 A", e.Attribute("Title")?.Value);
        Assert.Equal("0,0,100,60", e.Attribute("Bounds")?.Value);
        Assert.Empty(e.Elements());   // 자식 트리 미출력
    }
}
