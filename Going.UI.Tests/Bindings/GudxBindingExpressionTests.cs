using System;
using Going.UI.Bindings;
using Xunit;

namespace Going.UI.Tests.Bindings;

public class GudxBindingExpressionTests
{
    [Theory]
    [InlineData("{Data1.Title}", true)]
    [InlineData("{X}", true)]
    [InlineData("{X:F1}", true)]
    [InlineData("Hello", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("{{literal}}", false)]   // escape — 바인딩 아님
    [InlineData("{unclosed", false)]
    [InlineData("trailing}", false)]
    public void IsBinding_Detects(string? value, bool expected)
        => Assert.Equal(expected, GudxBindingExpression.IsBinding(value));

    [Theory]
    [InlineData("{Data1.Title}", "Data1.Title", null)]
    [InlineData("{DataMgr.Tank1.Level:F1}", "DataMgr.Tank1.Level", "F1")]
    [InlineData("{X : 0.0 }", "X", "0.0")]   // 공백 trim
    public void Parse_SplitsPathAndFormat(string value, string path, string? fmt)
    {
        var (p, f) = GudxBindingExpression.Parse(value);
        Assert.Equal(path, p);
        Assert.Equal(fmt, f);
    }

    public sealed class Leaf { public string Title { get; set; } = "init"; public int RO { get; } = 7; }
    public sealed class Root { public Leaf Data1 { get; } = new(); public double Speed { get; set; } = 1.5; }

    [Fact]
    public void Compile_Getter_WalksPath()
    {
        var root = new Root();
        var cp = GudxBindingExpression.CompilePath(typeof(Root), "Data1.Title");
        Assert.Equal("init", cp.Getter(root));
        root.Data1.Title = "changed";
        Assert.Equal("changed", cp.Getter(root));   // 매번 현재값
        Assert.Equal(typeof(string), cp.LeafType);
    }

    [Fact]
    public void Compile_Setter_WhenLeafWritable()
    {
        var root = new Root();
        var cp = GudxBindingExpression.CompilePath(typeof(Root), "Data1.Title");
        Assert.NotNull(cp.Setter);
        cp.Setter!(root, "viaSetter");
        Assert.Equal("viaSetter", root.Data1.Title);
    }

    [Fact]
    public void Compile_NoSetter_WhenLeafReadOnly()
    {
        var cp = GudxBindingExpression.CompilePath(typeof(Root), "Data1.RO");
        Assert.Null(cp.Setter);
    }

    [Fact]
    public void Compile_Caches_SameInstance()
    {
        var a = GudxBindingExpression.CompilePath(typeof(Root), "Speed");
        var b = GudxBindingExpression.CompilePath(typeof(Root), "Speed");
        Assert.Same(a, b);
    }

    [Fact]
    public void Compile_Throws_OnMissingMember()
        => Assert.Throws<ArgumentException>(() => GudxBindingExpression.CompilePath(typeof(Root), "Nope.Missing"));
}
