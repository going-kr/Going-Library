using System.IO;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>GoChip: 알약 칩(상태 dot + 텍스트). s1 LIVE 형식 렌더 검증.</summary>
public class GoChipTests
{
    private static readonly string ArtifactDir =
        Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");

    private static void Draw(SKCanvas canvas, GoChip chip, float x, float y, float w, float h)
    {
        chip.Bounds = new SKRect(x, y, x + w, y + h);
        using (new SKAutoCanvasRestore(canvas))
        {
            canvas.Translate(x, y);
            chip.Bounds = new SKRect(0, 0, w, h);
            chip.FireDraw(canvas, GoTheme.DarkTheme);
        }
    }

    [Fact]
    public void Chips_Render()
    {
        var bmp = new SKBitmap(560, 220);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(new SKColor(0x0E, 0x11, 0x17));

            // s1 LIVE 형식
            Draw(canvas, new GoChip { Text = "LIVE", DotColor = "#39D98A", TextColor = "#39D98A", ChipColor = "#10231A", BorderColor = "#1E5B3F" }, 20, 24, 110, 34);
            // 상태 칩들
            Draw(canvas, new GoChip { Text = "RUNNING", DotColor = "#39D98A", TextColor = "#39D98A", ChipColor = "#10231A", BorderColor = "#1E5B3F" }, 150, 24, 130, 34);
            Draw(canvas, new GoChip { Text = "WARNING", DotColor = "#F59E0B", TextColor = "#F59E0B", ChipColor = "#2A2110", BorderColor = "#5B4620" }, 300, 24, 140, 34);
            Draw(canvas, new GoChip { Text = "FAULT", DotColor = "#EF4444", TextColor = "#EF4444", ChipColor = "#2A1414", BorderColor = "#5B2424" }, 460, 24, 90, 34);

            // dot 없는 태그 칩
            Draw(canvas, new GoChip { Text = "v1.2.0", DotColor = null, TextColor = "#8B97A8", ChipColor = "#1B212B", BorderColor = "#2A323F" }, 20, 80, 100, 30);
            Draw(canvas, new GoChip { Text = "BETA", DotColor = null, TextColor = "#FFFFFF", ChipColor = "#5B8DEF", BorderColor = null }, 130, 80, 80, 30);
            // 각진(Corner) 칩
            Draw(canvas, new GoChip { Text = "SQUARE", DotColor = "#34C3FF", TextColor = "#34C3FF", ChipColor = "#10222B", BorderColor = "#1E4B5B", Corner = 6 }, 220, 80, 130, 30);
            // 아이콘 칩
            Draw(canvas, new GoChip { Text = "LIVE", IconString = "fa-bolt", IconColor = "#39D98A", TextColor = "#39D98A", ChipColor = "#10231A", BorderColor = "#1E5B3F" }, 20, 130, 120, 34);
            Draw(canvas, new GoChip { Text = "Verified", IconString = "fa-circle-check", IconColor = "#34C3FF", TextColor = "#CFE9FF", ChipColor = "#10222B", BorderColor = "#1E4B5B" }, 160, 130, 140, 34);
            // 닫기 버튼 칩 (필터/제거 칩)
            Draw(canvas, new GoChip { Text = "kim", IconString = "fa-user", IconColor = "#9B6BEF", TextColor = "#C7B6F5", ChipColor = "#1B1430", BorderColor = "#3A2A5B", Closable = true }, 320, 130, 120, 34);
            Draw(canvas, new GoChip { Text = "Filter: Line 7", TextColor = "#C7D0DC", ChipColor = "#1B212B", BorderColor = "#2A323F", Closable = true }, 20, 175, 180, 32);

            canvas.Flush();
        }
        Directory.CreateDirectory(ArtifactDir);
        using (var img = SKImage.FromBitmap(bmp))
        using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
        using (var fs = File.Create(Path.Combine(ArtifactDir, "gochip.png")))
            data.SaveTo(fs);
        bmp.Dispose();
    }

    [Fact]
    public void Chip_AutoRegisters()
        => Assert.True(Going.UI.Json.GoJsonConverter.ControlTypes.ContainsKey("GoChip"));
}
