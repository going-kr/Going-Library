using System.IO;
using System.Xml.Linq;
using Going.UI.Design;
using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxT12MasterSplitTests
{
    private static string CreateTempDir()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tmp);
        return tmp;
    }

    [Fact]
    public void SerializeGudx_writesMasterAndPageFiles()
    {
        var tmp = CreateTempDir();
        try
        {
            var d = new GoDesign { Name = "T", DesignWidth = 1024, DesignHeight = 600 };
            d.AddPage(new GoPage { Name = "Dashboard" });
            d.AddPage(new GoPage { Name = "Alarm" });

            var masterPath = Path.Combine(tmp, "Master.gudx");
            d.SerializeGudx(masterPath);

            // Master file exists with refs
            Assert.True(File.Exists(masterPath));
            var master = File.ReadAllText(masterPath);
            Assert.Contains("<GoPageRef", master);
            Assert.Contains("File=\"Pages/Dashboard.gudx\"", master);
            Assert.Contains("File=\"Pages/Alarm.gudx\"", master);
            // Master should NOT contain Pages dict inline (GoPageRef is OK, GoPage element is not)
            Assert.DoesNotContain("<GoPage ", master);
            Assert.DoesNotContain("<GoPage>", master);

            // Page files exist
            Assert.True(File.Exists(Path.Combine(tmp, "Pages", "Dashboard.gudx")));
            Assert.True(File.Exists(Path.Combine(tmp, "Pages", "Alarm.gudx")));
        }
        finally { Directory.Delete(tmp, recursive: true); }
    }

    [Fact]
    public void DeserializeGudx_loadsMasterFollowsPageRefs()
    {
        var tmp = CreateTempDir();
        try
        {
            var original = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
            original.AddPage(new GoPage { Name = "P1" });
            var masterPath = Path.Combine(tmp, "Master.gudx");
            original.SerializeGudx(masterPath);

            var restored = GoDesign.DeserializeGudx(masterPath);

            Assert.NotNull(restored);
            Assert.Equal("T", restored!.Name);
            Assert.Equal(800, restored.DesignWidth);
            Assert.Equal(480, restored.DesignHeight);
            Assert.True(restored.Pages.ContainsKey("P1"));
        }
        finally { Directory.Delete(tmp, recursive: true); }
    }

    [Fact]
    public void SerializeGudx_writesWindowsToSeparateFiles()
    {
        var tmp = CreateTempDir();
        try
        {
            var d = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 480 };
            d.Windows.Add("Settings", new GoWindow { Name = "Settings" });

            var masterPath = Path.Combine(tmp, "Master.gudx");
            d.SerializeGudx(masterPath);

            Assert.True(File.Exists(Path.Combine(tmp, "Windows", "Settings.gudx")));
            var master = File.ReadAllText(masterPath);
            Assert.Contains("<GoWindowRef", master);
            Assert.Contains("File=\"Windows/Settings.gudx\"", master);
        }
        finally { Directory.Delete(tmp, recursive: true); }
    }

    [Fact]
    public void RoundTrip_preservesPagesWindowsAndTheme()
    {
        var tmp = CreateTempDir();
        try
        {
            var original = new GoDesign { Name = "Full", DesignWidth = 1024, DesignHeight = 600 };
            original.UseTitleBar = true;
            original.CustomTheme = new GoTheme { Dark = true, Fore = SKColors.White };
            original.AddPage(new GoPage { Name = "Dashboard" });
            original.AddPage(new GoPage { Name = "Settings" });
            original.Windows.Add("HelpWindow", new GoWindow { Name = "HelpWindow" });

            var masterPath = Path.Combine(tmp, "Master.gudx");
            original.SerializeGudx(masterPath);

            var restored = GoDesign.DeserializeGudx(masterPath)!;

            Assert.Equal("Full", restored.Name);
            Assert.Equal(2, restored.Pages.Count);
            Assert.True(restored.Pages.ContainsKey("Dashboard"));
            Assert.True(restored.Pages.ContainsKey("Settings"));
            Assert.Single(restored.Windows);
            Assert.True(restored.Windows.ContainsKey("HelpWindow"));
            Assert.True(restored.UseTitleBar);
            Assert.NotNull(restored.CustomTheme);
            Assert.True(restored.CustomTheme!.Dark);
            Assert.Equal(SKColors.White, restored.CustomTheme.Fore);
        }
        finally { Directory.Delete(tmp, recursive: true); }
    }

    [Fact]
    public void DeserializeGudx_returnsNullForMissingFile()
    {
        var result = GoDesign.DeserializeGudx(Path.Combine(Path.GetTempPath(), "definitely-does-not-exist-" + System.Guid.NewGuid() + ".gudx"));
        Assert.Null(result);
    }
}
