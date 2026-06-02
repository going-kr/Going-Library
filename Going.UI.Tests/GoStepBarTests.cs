using System.Text.Json;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Gudx;
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

    // gudx 역직렬화 시 Step 속성이 Steps 자식보다 먼저 적용 → 비어있을 때 0으로 클램프되던 버그 회귀 방지
    [Fact]
    public void Step_SurvivesGudxRoundTrip()
    {
        var s = new GoStepBar();
        foreach (var t in new[] { "Charge", "Mix", "React", "Cool", "Discharge" })
            s.Steps.Add(new GoStepItem { Text = t });
        s.Step = 3;

        var xml = GoGudxConverter.SerializeControl(s);
        Assert.Contains("Step=\"3\"", xml);

        var back = Assert.IsType<GoStepBar>(GoGudxConverter.DeserializeControl(xml));
        Assert.Equal(5, back.Steps.Count);
        Assert.Equal(3, back.Step);   // 클램프 버그면 0
    }

    [Fact]
    public void DotFillColor_GudxRoundTrips()
    {
        var s = new GoStepBar { DotFillColor = "#10151C" };
        s.Steps.Add(new GoStepItem { Text = "A" });
        var xml = GoGudxConverter.SerializeControl(s);
        Assert.Contains("DotFillColor=\"#10151C\"", xml);
        var back = Assert.IsType<GoStepBar>(GoGudxConverter.DeserializeControl(xml));
        Assert.Equal("#10151C", back.DotFillColor);
    }
}
