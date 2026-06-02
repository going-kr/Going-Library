using System.IO;
using System.Linq;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Json;
using Going.UI.Themes;
using Going.UI.ViewObjects;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>Vo* 가 GoControl 패밀리로 동작하는지: 조립→렌더, gudx 왕복, 값 노드, 자동 등록.</summary>
public class VoControlsTests
{
    private static readonly string ArtifactDir =
        Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");

    // CPU 카드: VoBox(styled) > VoGrid > [VoText, VoText, VoProgress]
    private static VoBox Card(float pct = 48)
    {
        var card = new VoBox
        {
            Background = "#1B212B", FillType = VoFillType.Linear, FillColor2 = "#141921", GradientAngle = 90,
            BorderRadius = 16, ShadowColor = "#000000", ShadowY = 6, ShadowBlur = 16,
            Padding = new(20),
            Bounds = new SKRect(0, 0, 300, 160),
        };
        var grid = new VoGrid { Columns = ["*"], Rows = ["22px", "6px", "46px", "12px", "14px", "*"], Dock = GoDockStyle.Fill };
        grid.Childrens.Add(new VoText { Text = "CPU LOAD", FontSize = 13, TextColor = "#8FA1B3", Alignment = GoContentAlignment.MiddleLeft }, 0, 0);
        grid.Childrens.Add(new VoText { Name = "value", Text = $"{pct:0}%", FontSize = 36, FontStyle = GoFontStyle.Bold, TextColor = "#FFFFFF", Alignment = GoContentAlignment.MiddleLeft }, 0, 2);
        grid.Childrens.Add(new VoProgress { Name = "bar", Value = pct, Max = 100, TrackColor = "#2A323F", FillColor = "#34C3FF", FillColor2 = "#39D98A", Radius = 7 }, 0, 4);
        card.Childrens.Add(grid);
        return card;
    }

    private static SKBitmap Render(VoBox card, int w, int h)
    {
        card.Bounds = new SKRect(0, 0, w, h);
        var bmp = new SKBitmap(w, h);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(new SKColor(0x0E, 0x11, 0x16));
        card.FireDraw(canvas, GoTheme.DarkTheme);
        canvas.Flush();
        return bmp;
    }

    [Fact]
    public void Card_Renders_And_RoundTripsIdentically()
    {
        var card = Card();
        using var b1 = Render(card, 300, 160);

        Directory.CreateDirectory(ArtifactDir);
        using (var img = SKImage.FromBitmap(b1))
        using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
        using (var fs = File.Create(Path.Combine(ArtifactDir, "voctrl-card.png")))
            data.SaveTo(fs);

        // 뭔가 그려짐
        Assert.NotEqual(new SKColor(0x0E, 0x11, 0x16), b1.GetPixel(150, 80));

        // gudx 왕복 — VoObj 전용 P4 없이 표준 P2/P3 로
        var xml = GoGudxConverter.SerializeControl(card);
        Assert.Contains("<VoBox", xml);
        Assert.Contains("<VoGrid", xml);
        Assert.Contains("<VoText", xml);
        Assert.Contains("<VoProgress", xml);

        var back = Assert.IsType<VoBox>(GoGudxConverter.DeserializeControl(xml));
        var grid = Assert.IsType<VoGrid>(Assert.Single(back.Childrens));
        Assert.Equal(3, grid.Childrens.Count());

        // 복원본 렌더가 원본과 픽셀 동일
        using var b2 = Render(back, 300, 160);
        foreach (var (x, y) in new[] { (40, 40), (60, 70), (150, 130), (40, 130) })
            Assert.Equal(b1.GetPixel(x, y), b2.GetPixel(x, y));
    }

    [Fact]
    public void ValueNode_DrivesRendering()
    {
        var lo = Card(20);
        var hi = Card(85);
        using var blo = Render(lo, 300, 160);
        using var bhi = Render(hi, 300, 160);
        // 값이 다르면 막대 채움 폭 + 값 텍스트가 달라짐 → 전체 차분 픽셀이 충분히 존재해야 함
        int diff = 0;
        for (int y = 0; y < 160; y += 2)
            for (int x = 0; x < 300; x += 2)
                if (blo.GetPixel(x, y) != bhi.GetPixel(x, y)) diff++;
        Assert.True(diff > 50, $"diff={diff}");
    }

    [Fact]
    public void VoFamily_AutoRegisters_InControlTypes()
    {
        foreach (var name in new[] { "VoBox", "VoText", "VoArc", "VoProgress", "VoStack", "VoGrid" })
            Assert.True(GoJsonConverter.ControlTypes.ContainsKey(name), $"{name} not registered");
    }
}
