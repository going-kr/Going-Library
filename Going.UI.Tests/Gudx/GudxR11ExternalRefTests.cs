using System.Linq;
using System.Xml.Linq;
using Going.UI.Design;
using Going.UI.Gudx;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxR11ExternalRefTests
{
    [Fact]
    public void GoDesign_emitsNoBase64ForImagesOrFonts()
    {
        // Phase 0 invariant: Images/Fonts (private dicts) MUST NOT inline as base64.
        // [JsonInclude] would normally include them, but Gudx inclusion-only via [GoProperty]
        // (and ChildProperties Public-only filter) excludes them.
        var d = new GoDesign { Name = "T", DesignWidth = 1024, DesignHeight = 600 };

        var xml = GoGudxConverter.WriteAny(d).ToString();

        // Hard guard: no base64 anywhere
        Assert.DoesNotContain("base64", xml, System.StringComparison.OrdinalIgnoreCase);
        // Sanity: GoDesign root tag is present
        Assert.Contains("<GoDesign", xml);
    }

    [Fact]
    public void GoPageRef_canBeInstantiated()
    {
        // Placeholder marker class — Task 12 will use it in Master file split.
        var r = new GoPageRef { File = "Pages/Dashboard.gudx" };
        Assert.Equal("Pages/Dashboard.gudx", r.File);
    }

    [Fact]
    public void GoWindowRef_canBeInstantiated()
    {
        var r = new GoWindowRef { File = "Windows/Settings.gudx" };
        Assert.Equal("Windows/Settings.gudx", r.File);
    }
}
