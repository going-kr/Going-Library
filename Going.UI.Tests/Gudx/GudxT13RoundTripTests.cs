using System.IO;
using System.Linq;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Tests.Fixtures;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxT13RoundTripTests
{
    [Fact]
    public void FullGoDesign_serializeDeserializeSerialize_isIdempotent()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tmp);
        try
        {
            // 1) Build fixture and serialize to disk
            var original = MakeFixtureDesign.Build();
            var masterPath = Path.Combine(tmp, "Master.gudx");
            original.SerializeGudx(masterPath);

            // Capture first-pass file contents
            var firstMaster = File.ReadAllText(masterPath);
            var firstPageFiles = Directory.GetFiles(Path.Combine(tmp, "Pages"))
                .OrderBy(f => f)
                .ToDictionary(Path.GetFileName, File.ReadAllText);

            // 2) Deserialize
            var restored = GoDesign.DeserializeGudx(masterPath);
            Assert.NotNull(restored);

            // 3) Re-serialize to a separate directory
            var rtDir = Path.Combine(tmp, "rt");
            Directory.CreateDirectory(rtDir);
            var rtMasterPath = Path.Combine(rtDir, "Master.gudx");
            restored!.SerializeGudx(rtMasterPath);

            // 4) Compare master files byte-for-byte
            var rtMaster = File.ReadAllText(rtMasterPath);
            Assert.Equal(firstMaster, rtMaster);

            // 5) Compare each page file byte-for-byte
            var rtPageFiles = Directory.GetFiles(Path.Combine(rtDir, "Pages"))
                .OrderBy(f => f)
                .ToDictionary(Path.GetFileName, File.ReadAllText);
            Assert.Equal(firstPageFiles.Count, rtPageFiles.Count);
            foreach (var kvp in firstPageFiles)
            {
                Assert.True(rtPageFiles.TryGetValue(kvp.Key, out var rtContent),
                    $"Round-trip is missing page file {kvp.Key}");
                Assert.Equal(kvp.Value, rtContent);
            }
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void FullGoDesign_deserialize_preservesAllStructure()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tmp);
        try
        {
            var original = MakeFixtureDesign.Build();
            var masterPath = Path.Combine(tmp, "Master.gudx");
            original.SerializeGudx(masterPath);

            var restored = GoDesign.DeserializeGudx(masterPath)!;

            // Top-level scalars (P1)
            Assert.Equal(original.Name, restored.Name);
            Assert.Equal(original.DesignWidth, restored.DesignWidth);
            Assert.Equal(original.DesignHeight, restored.DesignHeight);
            Assert.Equal(original.BarColor, restored.BarColor);
            Assert.Equal(original.UseTitleBar, restored.UseTitleBar);

            // B1: Theme round-trip
            Assert.NotNull(restored.CustomTheme);
            Assert.Equal(original.CustomTheme!.Dark, restored.CustomTheme!.Dark);
            Assert.Equal(original.CustomTheme.Fore, restored.CustomTheme.Fore);
            Assert.Equal(original.CustomTheme.Back, restored.CustomTheme.Back);

            // P5: Pages count and keys
            Assert.Equal(original.Pages.Count, restored.Pages.Count);
            foreach (var key in original.Pages.Keys)
                Assert.True(restored.Pages.ContainsKey(key), $"Missing page: {key}");

            // P2 + P3 + P4 nested in Dashboard page
            var dash = restored.Pages["Dashboard"];
            Assert.Equal(3, dash.Childrens.Count);  // tableLayout + switchPanel + boxPanel

            // P3: TableLayoutPanel preserves Cell indices
            var tbl = dash.Childrens.OfType<GoTableLayoutPanel>().First();
            Assert.Equal(2, tbl.Childrens.Controls.Count);
            Assert.Contains(tbl.Childrens.Indexes.Values, idx => idx.Column == 0 && idx.Row == 0);
            Assert.Contains(tbl.Childrens.Indexes.Values, idx => idx.Column == 1 && idx.Row == 0);

            // P4: SwitchPanel.Pages preserves order and inner Childrens
            var sw = dash.Childrens.OfType<GoSwitchPanel>().First();
            Assert.Equal(2, sw.Pages.Count);
            Assert.Equal("ModeA", sw.Pages[0].Name);
            Assert.Equal("ModeB", sw.Pages[1].Name);
            Assert.Single(sw.Pages[0].Childrens);

            // P2: BoxPanel children
            var box = dash.Childrens.OfType<GoBoxPanel>().First();
            Assert.Single(box.Childrens);
            Assert.Equal("btnAction", ((GoButton)box.Childrens[0]).Name);
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }
}
