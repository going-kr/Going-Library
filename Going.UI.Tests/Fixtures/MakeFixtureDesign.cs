using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Themes;
using SkiaSharp;

namespace Going.UI.Tests.Fixtures;

internal static class MakeFixtureDesign
{
    /// <summary>
    /// Builds a comprehensive GoDesign exercising P1 (scalars), P2 (homogeneous list),
    /// P3 (cell-indexed), P4 (wrapper list), P5 (keyed dict), and B1 (single child + Theme).
    /// Used by T13 round-trip stress test.
    /// </summary>
    public static GoDesign Build()
    {
        var d = new GoDesign
        {
            Name = "FixtureDesign",
            DesignWidth = 1024,
            DesignHeight = 600,
            BarColor = "Base2"
        };

        // B1: TitleBar visible
        d.UseTitleBar = true;

        // B1: CustomTheme via [GudxTagName("Theme")]
        d.CustomTheme = new GoTheme
        {
            Dark = true,
            Fore = SKColors.White,
            Back = new SKColor(0x1A, 0x1A, 0x1A, 0xFF)
        };

        // P5: 2 Pages
        var dashboardPage = new GoPage { Name = "Dashboard" };

        // P3: GoTableLayoutPanel with 2 IGoControl children carrying Cell="c,r"
        var tablePanel = new GoTableLayoutPanel
        {
            Name = "rootGrid",
            Columns = { "50%", "50%" },
            Rows = { "100%" }
        };
        tablePanel.Childrens.Add(new GoButton { Name = "btnSave", Text = "Save" }, 0, 0);
        tablePanel.Childrens.Add(new GoButton { Name = "btnCancel", Text = "Cancel" }, 1, 0);
        dashboardPage.Childrens.Add(tablePanel);

        // P4: GoSwitchPanel.Pages : List<GoSubPage>
        var switchPanel = new GoSwitchPanel { Name = "modeSwitch" };
        var modeA = new GoSubPage { Name = "ModeA" };
        modeA.Childrens.Add(new GoLabel { Name = "lblA", Text = "Mode A" });
        var modeB = new GoSubPage { Name = "ModeB" };
        modeB.Childrens.Add(new GoLabel { Name = "lblB", Text = "Mode B" });
        switchPanel.Pages.Add(modeA);
        switchPanel.Pages.Add(modeB);
        dashboardPage.Childrens.Add(switchPanel);

        // P2: GoBoxPanel.Childrens : List<IGoControl>
        var boxPanel = new GoBoxPanel { Name = "rightPanel" };
        boxPanel.Childrens.Add(new GoButton { Name = "btnAction", Text = "Run", ButtonColor = "Good" });
        dashboardPage.Childrens.Add(boxPanel);

        d.AddPage(dashboardPage);

        // Second page (lighter content)
        var settingsPage = new GoPage { Name = "Settings" };
        settingsPage.Childrens.Add(new GoLabel { Name = "lblSettings", Text = "Settings" });
        d.AddPage(settingsPage);

        return d;
    }
}
