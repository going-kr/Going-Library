using System.Text.Json;
using Going.UI.Controls;
using Going.UI.Json;
using Xunit;

namespace Going.UI.Tests;

public class GoStepBarTests
{
    [Fact]
    public void Defaults_NoSteps()
    {
        var b = new GoStepBar();
        Assert.Equal(0, b.Step);
        Assert.Equal(GoStepBarMode.Story, b.Mode);
        Assert.False(b.UseClick);
        Assert.Empty(b.Steps);
    }

    [Fact]
    public void Step_ConstrainsToStepsCount()
    {
        var b = new GoStepBar();
        b.Steps.Add(new GoStepItem { Text = "A" });
        b.Steps.Add(new GoStepItem { Text = "B" });

        b.Step = 5;
        Assert.Equal(2, b.Step); // Steps.Count

        b.Step = -3;
        Assert.Equal(0, b.Step); // 0=none

        b.Step = 1;
        Assert.Equal(1, b.Step);
    }

    [Fact]
    public void StepChanged_FiresOnlyOnDelta()
    {
        var b = new GoStepBar();
        b.Steps.Add(new GoStepItem { Text = "A" });
        int fires = 0;
        b.StepChanged += (s, e) => fires++;

        b.Step = 1;
        b.Step = 1; // 동일값 — 발화 안 함
        b.Step = 0;

        Assert.Equal(2, fires);
    }

    [Fact]
    public void Json_RoundTrip_PreservesAllProps()
    {
        var src = new GoStepBar
        {
            Mode = GoStepBarMode.Point,
            UseClick = true,
            DotSize = 22F,
            FontSize = 14F,
        };
        src.Steps.Add(new GoStepItem { Text = "인터뷰", IconString = "fa-comment" });
        src.Steps.Add(new GoStepItem { Text = "계획" });
        src.Steps.Add(new GoStepItem { Text = "구현", IconString = "fa-code" });
        src.Step = 2;

        var json = JsonSerializer.Serialize(src, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<GoStepBar>(json, GoJsonConverter.Options);

        Assert.NotNull(back);
        Assert.Equal(GoStepBarMode.Point, back!.Mode);
        Assert.True(back.UseClick);
        Assert.Equal(22F, back.DotSize);
        Assert.Equal(14F, back.FontSize);
        Assert.Equal(3, back.Steps.Count);
        Assert.Equal("인터뷰", back.Steps[0].Text);
        Assert.Equal("fa-comment", back.Steps[0].IconString);
        Assert.Null(back.Steps[1].IconString);
        Assert.Equal(2, back.Step);
    }

    [Fact]
    public void ControlTypes_Registers_GoStepGauge_And_GoStepBar()
    {
        Assert.Contains("GoStepGauge", GoJsonConverter.ControlTypes.Keys);
        Assert.Contains("GoStepBar", GoJsonConverter.ControlTypes.Keys);
        Assert.DoesNotContain("GoStep", GoJsonConverter.ControlTypes.Keys);
    }
}
