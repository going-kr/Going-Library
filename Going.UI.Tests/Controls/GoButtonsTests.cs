using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Controls;

public class GoButtonsTests
{
    [Fact]
    public void VerticalDirection_UsesRowsForHitTesting()
    {
        var buttons = new GoButtons
        {
            Bounds = new SKRect(0, 0, 90, 90),
            Direction = GoDirectionHV.Vertical,
            Mode = GoButtonsMode.Radio,
        };
        buttons.Buttons.Add(new GoButtonsItem { Name = "a", Text = "A" });
        buttons.Buttons.Add(new GoButtonsItem { Name = "b", Text = "B" });
        buttons.Buttons.Add(new GoButtonsItem { Name = "c", Text = "C" });

        buttons.FireMouseDown(10, 45, GoMouseButton.Left);
        buttons.FireMouseUp(10, 45, GoMouseButton.Left);

        Assert.False(buttons.Buttons[0].Selected);
        Assert.True(buttons.Buttons[1].Selected);
        Assert.False(buttons.Buttons[2].Selected);
    }
}
