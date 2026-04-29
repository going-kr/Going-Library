using System.Reflection;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GoChildsAttributeTests
{
    private class ProbeWithChildren
    {
        [GoChilds] public List<string> Items { get; set; } = new();
        public string Other { get; set; } = "";
    }

    [Fact]
    public void GoChildsAttribute_isDetectedByReflection()
    {
        var prop = typeof(ProbeWithChildren).GetProperty(nameof(ProbeWithChildren.Items))!;
        var attr = prop.GetCustomAttribute<GoChildsAttribute>();
        Assert.NotNull(attr);
    }

    [Fact]
    public void GoChildsAttribute_findsAllMarkedProperties()
    {
        var props = typeof(ProbeWithChildren)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<GoChildsAttribute>() != null)
            .ToList();
        Assert.Single(props);
        Assert.Equal("Items", props[0].Name);
    }
}
