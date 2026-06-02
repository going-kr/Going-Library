using System.IO;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>GoBoxPanel 좌측 액센트 바 렌더 검증.</summary>
public class GoBoxPanelAccentTests
{
    private static readonly string ArtifactDir =
        Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");

    private static void Draw(SKCanvas canvas, GoBoxPanel p, float x, float y, float w, float h)
    {
        using (new SKAutoCanvasRestore(canvas))
        {
            canvas.Translate(x, y);
            p.Bounds = new SKRect(0, 0, w, h);
            p.FireDraw(canvas, GoTheme.DarkTheme);
        }
    }

    private static GoBoxPanel Card(string accent, string label, string body)
    {
        var p = new GoBoxPanel
        {
            BoxColor = "#161B22", BoxColor2 = "#10141B", BorderColor = "#222A35", BorderWidth = 1,
            Round = GoRoundType.All, DrawAccentStrip = true, AccentColor = accent, AccentWidth = 5, Padding = new(16, 12, 14, 12),
        };
        p.Childrens.Add(new GoLabel { Text = label, FontSize = 13, FontStyle = GoFontStyle.Bold, TextColor = accent, BackgroundDraw = false, ContentAlignment = GoContentAlignment.TopLeft, Dock = GoDockStyle.Top, Height = 20 });
        p.Childrens.Add(new GoLabel { Text = body, FontSize = 12, TextColor = "#C7D0DC", BackgroundDraw = false, ContentAlignment = GoContentAlignment.TopLeft, Dock = GoDockStyle.Fill });
        return p;
    }

    [Fact]
    public void Accent_Renders()
    {
        var bmp = new SKBitmap(720, 240);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(new SKColor(0x0E, 0x11, 0x17));
            Draw(canvas, Card("#39D98A", "OPERATIONAL", "All systems nominal."), 20, 20, 210, 90);
            Draw(canvas, Card("#F59E0B", "WARNING", "Cache latency rising."), 255, 20, 210, 90);
            Draw(canvas, Card("#EF4444", "FAULT", "Vision AI offline."), 490, 20, 210, 90);

            // 각진(Rect) + 큰 액센트
            var sq = new GoBoxPanel { BoxColor = "#1B212B", BorderColor = "#2A323F", BorderWidth = 1, Round = GoRoundType.Rect, DrawAccentStrip = true, AccentColor = "#34C3FF", AccentWidth = 6, Padding = new(16, 12, 14, 12) };
            sq.Childrens.Add(new GoLabel { Text = "Square accent (Rect)", FontSize = 13, TextColor = "#CFE9FF", BackgroundDraw = false, ContentAlignment = GoContentAlignment.MiddleLeft, Dock = GoDockStyle.Fill });
            Draw(canvas, sq, 20, 130, 330, 80);

            // 액센트 없음 (대조)
            var none = new GoBoxPanel { BoxColor = "#1B212B", BorderColor = "#2A323F", BorderWidth = 1, Round = GoRoundType.All, Padding = new(16, 12, 14, 12) };
            none.Childrens.Add(new GoLabel { Text = "No accent", FontSize = 13, TextColor = "#8B97A8", BackgroundDraw = false, ContentAlignment = GoContentAlignment.MiddleLeft, Dock = GoDockStyle.Fill });
            Draw(canvas, none, 370, 130, 330, 80);

            canvas.Flush();
        }
        Directory.CreateDirectory(ArtifactDir);
        using (var img = SKImage.FromBitmap(bmp))
        using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
        using (var fs = File.Create(Path.Combine(ArtifactDir, "goboxpanel-accent.png")))
            data.SaveTo(fs);
        bmp.Dispose();
    }
}
