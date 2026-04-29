using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Tests.Fixtures;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxT14LazyIdTests
{
    [Fact]
    public void DeserializeGudx_allControlsHaveNonEmptyIds()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tmp);
        try
        {
            var original = MakeFixtureDesign.Build();
            var masterPath = Path.Combine(tmp, "Master.gudx");
            original.SerializeGudx(masterPath);

            var restored = GoDesign.DeserializeGudx(masterPath)!;

            // Walk all controls in all pages and assert non-empty Id
            foreach (var page in restored.Pages.Values)
            {
                AssertAllControlsHaveIds(page.Childrens);
            }
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    private static void AssertAllControlsHaveIds(IEnumerable<IGoControl> controls)
    {
        foreach (var c in controls)
        {
            Assert.NotEqual(Guid.Empty, c.Id);
            if (c is IGoContainer container)
            {
                AssertAllControlsHaveIds(container.Childrens);
            }
        }
    }
}
