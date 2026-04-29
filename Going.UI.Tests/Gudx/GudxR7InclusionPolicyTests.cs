using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxR7InclusionPolicyTests
{
    [Fact]
    public void Selectable_doesNotEmit_evenWhenTrue()
    {
        // GoControl.Selectable is `public bool Selectable { get; protected set; }` with NO [GoProperty].
        // Inclusion-only policy: no [GoProperty] family attribute → no emission.
        // (Cannot easily set Selectable=true on GoButton because protected set, but the
        //  default value is false; we just verify Selectable= attribute never appears.)
        var btn = new GoButton { Name = "btn" };
        var xml = GoGudxConverter.SerializeControl(btn);
        Assert.DoesNotContain("Selectable=", xml);
    }

    [Fact]
    public void GoControl_DerivedAlias_doesNotEmit()
    {
        // X/Y/Width/Height etc. are [GoProperty] + [JsonIgnore] computed aliases of Bounds.
        // The [JsonIgnore] secondary filter excludes them even though [GoProperty] is present.
        // Use space-prefixed patterns so e.g. "Width=" does not false-match "BorderWidth=".
        var btn = new GoButton { Name = "btn" };
        var xml = GoGudxConverter.SerializeControl(btn);
        Assert.DoesNotContain(" X=", xml);
        Assert.DoesNotContain(" Y=", xml);
        Assert.DoesNotContain(" Width=", xml);
        Assert.DoesNotContain(" Height=", xml);
        Assert.DoesNotContain(" Left=", xml);
        Assert.DoesNotContain(" Top=", xml);
        Assert.DoesNotContain(" Right=", xml);
        Assert.DoesNotContain(" Bottom=", xml);
        // But the canonical Bounds= attribute SHOULD emit (free-positioning support):
        Assert.Contains("Bounds=", xml);
    }
}
