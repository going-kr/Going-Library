using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

/// <summary>
/// Regression tests for the P2+P4 mix latent bug discovered during v1.2.0 review.
///
/// GoGroupBox / GoPanel hold both [GoChildList] Childrens (P2) and [GoChildWrappers]
/// Buttons (P4) on the same class. Under the v1.2.0 emit format, P4 wrappers (GoButtonItem)
/// emitted as direct siblings of P2 IGoControl children. ReadP2_HomogeneousList walked
/// elem.Elements() unfiltered — when Buttons had data, ReadElement received GoButtonItem
/// (not in _gudxControlTypes registry, since GoButtonItem doesn't implement IGoControl)
/// and threw "Unknown Gudx tag: GoButtonItem".
///
/// Test129 round-trip 0 diffs passed despite this bug because Buttons was empty in all
/// real-world fixtures. These tests force the mix scenario and assert clean round-trip.
///
/// Expected before T-G3/T-G4: throws InvalidOperationException.
/// Expected after T-G4: passes (P2/P4 grouped under property-name elements).
/// </summary>
public class GudxP2P4MixTests
{
    [Fact]
    public void GoGroupBox_WithButtonsAndChildrens_RoundTripsCleanly()
    {
        var src = new GoGroupBox { Name = "grpSettings", Text = "Settings" };
        src.Buttons.Add(new GoButtonItem { Text = "Save" });
        src.Buttons.Add(new GoButtonItem { Text = "Cancel" });
        src.Childrens.Add(new GoButton { Name = "btnOk", Text = "OK" });
        src.Childrens.Add(new GoLabel { Name = "lblStatus", Text = "Status: ready" });

        var xml = GoGudxConverter.SerializeControl(src);
        var dst = (GoGroupBox)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(2, dst.Buttons.Count);
        Assert.Equal(2, dst.Childrens.Count);
        Assert.Equal("Save", dst.Buttons[0].Text);
        Assert.Equal("Cancel", dst.Buttons[1].Text);
        Assert.Equal("btnOk", ((GoButton)dst.Childrens[0]).Name);
        Assert.Equal("lblStatus", ((GoLabel)dst.Childrens[1]).Name);
    }

    [Fact]
    public void GoPanel_WithButtonsAndChildrens_RoundTripsCleanly()
    {
        var src = new GoPanel { Name = "pnl1", Text = "Panel1" };
        src.Buttons.Add(new GoButtonItem { Text = "Edit" });
        src.Childrens.Add(new GoLabel { Name = "lblHello", Text = "Hello" });

        var xml = GoGudxConverter.SerializeControl(src);
        var dst = (GoPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Single(dst.Buttons);
        Assert.Single(dst.Childrens);
        Assert.Equal("Edit", dst.Buttons[0].Text);
        Assert.Equal("lblHello", ((GoLabel)dst.Childrens[0]).Name);
    }

    [Fact]
    public void GoGroupBox_OnlyButtons_NoChildrens_RoundTripsCleanly()
    {
        // Sanity: P4-only (no P2 mix) should always work
        var src = new GoGroupBox { Name = "grpA" };
        src.Buttons.Add(new GoButtonItem { Text = "Save" });

        var xml = GoGudxConverter.SerializeControl(src);
        var dst = (GoGroupBox)GoGudxConverter.DeserializeControl(xml);

        Assert.Single(dst.Buttons);
        Assert.Empty(dst.Childrens);
    }

    [Fact]
    public void GoGroupBox_OnlyChildrens_NoButtons_RoundTripsCleanly()
    {
        // Sanity: P2-only (no P4 mix) — this is the path Test129 fixtures actually exercised
        var src = new GoGroupBox { Name = "grpB" };
        src.Childrens.Add(new GoButton { Name = "btnA", Text = "A" });

        var xml = GoGudxConverter.SerializeControl(src);
        var dst = (GoGroupBox)GoGudxConverter.DeserializeControl(xml);

        Assert.Empty(dst.Buttons);
        Assert.Single(dst.Childrens);
    }
}
