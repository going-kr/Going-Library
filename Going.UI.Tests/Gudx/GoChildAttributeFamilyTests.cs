using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GoChildAttributeFamilyTests
{
    private class Probe
    {
        [GoChildList]    public List<int> AsList { get; set; } = new();
        [GoChildCells]   public List<int> AsCells { get; set; } = new();
        [GoChildWrappers] public List<int> AsWrappers { get; set; } = new();
        [GoChildMap]     public Dictionary<string, int> AsMap { get; set; } = new();
        [GoChildSingle]  public object? AsSingle { get; set; }

        [GoChildResource("Image", Folder = "resources", Ext = ".png")]
        public Dictionary<string, byte[]>? AsResource { get; set; }

        public string PlainScalar { get; set; } = "";
    }

    [Fact]
    public void EachDerivedAttribute_isDetectedByItsOwnType()
    {
        Assert.NotNull(typeof(Probe).GetProperty("AsList")!.GetCustomAttribute<GoChildListAttribute>());
        Assert.NotNull(typeof(Probe).GetProperty("AsCells")!.GetCustomAttribute<GoChildCellsAttribute>());
        Assert.NotNull(typeof(Probe).GetProperty("AsWrappers")!.GetCustomAttribute<GoChildWrappersAttribute>());
        Assert.NotNull(typeof(Probe).GetProperty("AsMap")!.GetCustomAttribute<GoChildMapAttribute>());
        Assert.NotNull(typeof(Probe).GetProperty("AsSingle")!.GetCustomAttribute<GoChildSingleAttribute>());
        Assert.NotNull(typeof(Probe).GetProperty("AsResource")!.GetCustomAttribute<GoChildResourceAttribute>());
    }

    [Fact]
    public void AllDerivedAttributes_areDetectedViaAbstractBase()
    {
        var marked = typeof(Probe).GetProperties()
            .Where(p => p.IsDefined(typeof(GoChildAttribute), inherit: true))
            .Select(p => p.Name)
            .ToList();

        Assert.Equal(6, marked.Count);
        Assert.Contains("AsList", marked);
        Assert.Contains("AsCells", marked);
        Assert.Contains("AsWrappers", marked);
        Assert.Contains("AsMap", marked);
        Assert.Contains("AsSingle", marked);
        Assert.Contains("AsResource", marked);
    }

    [Fact]
    public void PlainScalarProperty_isNotDetected()
    {
        Assert.False(typeof(Probe).GetProperty("PlainScalar")!.IsDefined(typeof(GoChildAttribute), inherit: true));
    }

    [Fact]
    public void GoChildResource_carriesTagNameFolderExt()
    {
        var attr = typeof(Probe).GetProperty("AsResource")!.GetCustomAttribute<GoChildResourceAttribute>()!;
        Assert.Equal("Image", attr.TagName);
        Assert.Equal("resources", attr.Folder);
        Assert.Equal(".png", attr.Ext);
    }

    [Fact]
    public void GoChildResource_optionalFolderExt_canBeOmitted()
    {
        var attr = new GoChildResourceAttribute("Whatever");
        Assert.Equal("Whatever", attr.TagName);
        Assert.Null(attr.Folder);
        Assert.Null(attr.Ext);
    }

    [Fact]
    public void GoChildAttribute_isAbstract()
    {
        Assert.True(typeof(GoChildAttribute).IsAbstract);
    }
}
