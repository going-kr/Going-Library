using Going.UI.Containers;
using Going.UI.Controls.Shapes;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Controls;

public class GsShapeTests
{
    private static void Render(GoBoxPanel panel)
    {
        using var bmp = new SKBitmap(120, 120);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.Black);
        panel.FireDraw(canvas, GoTheme.DarkTheme);
    }

    // ── Clip ────────────────────────────────────────────────
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClipToBounds_ReflectsClip(bool clip)
    {
        var r = new GsRect { Clip = clip };
        Assert.Equal(clip, r.ClipToBounds);
    }

    /// <summary>Clip=false면 GUI.Draw가 ClipRect를 생략 → 자식 OnDraw 시점의 clip 영역이 컨테이너 전체.</summary>
    private sealed class ClipProbe : GsRect
    {
        public SKRect SeenClip;
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            SeenClip = canvas.LocalClipBounds;
            base.OnDraw(canvas, thm);
        }
    }

    [Fact]
    public void Gui_ClipsChild_ByDefault_AndSkips_WhenClipFalse()
    {
        SKRect Run(bool clip)
        {
            var panel = new GoBoxPanel { Left = 0, Top = 0, Width = 120, Height = 120 };
            var probe = new ClipProbe { Left = 30, Top = 30, Width = 40, Height = 40, Clip = clip, Margin = new GoPadding(0) };
            panel.Childrens.Add(probe);

            using var bmp = new SKBitmap(120, 120);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.Black);
            panel.FireDraw(canvas, GoTheme.DarkTheme);
            return probe.SeenClip;
        }

        var clipped = Run(true);     // child 크기(≈40)로 제한
        var unclipped = Run(false);  // 컨테이너 전체(≈120)

        Assert.True(clipped.Width <= 45f, $"clipped width={clipped.Width}");
        Assert.True(unclipped.Width >= 80f, $"unclipped width={unclipped.Width}");
    }

    // ── 렌더 무크래시 (효과 조합) ──────────────────────────
    [Fact]
    public void AllShapes_RenderWithEffects_NoCrash()
    {
        var panel = new GoBoxPanel { Left = 0, Top = 0, Width = 120, Height = 120 };
        panel.Childrens.Add(new GsRect { Left = 0, Top = 0, Width = 50, Height = 50, Fill = "Point", FillType = GsFillType.Linear, FillColor2 = "Base3", CornerRadius = 8, ShadowColor = "Base1", GlowColor = "Point", Rotation = 15, Clip = false });
        panel.Childrens.Add(new GsCircle { Left = 50, Top = 0, Width = 50, Height = 50, Fill = "Point", FillType = GsFillType.Radial, FillColor2 = "Base2", StrokeColor = "Fore", StrokeWidth = 2 });
        panel.Childrens.Add(new GsArc { Left = 0, Top = 50, Width = 50, Height = 50, StrokeColor = "Point", StrokeWidth = 8 });
        panel.Childrens.Add(new GsLine { Left = 50, Top = 50, Width = 50, Height = 50, StrokeColor = "Fore", StrokeWidth = 3 });
        panel.Childrens.Add(new GsPolygon { Left = 0, Top = 100, Width = 50, Height = 20, Fill = "Point", Closed = true });
        panel.Childrens.Add(new GsBezier { Left = 50, Top = 100, Width = 50, Height = 20, StrokeColor = "Point", StrokeWidth = 2 });

        Render(panel);   // 예외 없이 그려지면 통과
    }

    // ── gudx 라운드트립 ────────────────────────────────────
    [Fact]
    public void GsRect_GudxRoundTrips()
    {
        var r = new GsRect { Name = "r", Fill = "Point", FillType = GsFillType.Linear, FillColor2 = "Base3", CornerRadius = 12, StrokeColor = "Fore", StrokeWidth = 2, ShadowColor = "Base1", GlowColor = "Point", Rotation = 30, Clip = false };
        var xml = GoGudxConverter.SerializeControl(r);
        Assert.Contains("<GsRect", xml);
        var d = (GsRect)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal("Point", d.Fill);
        Assert.Equal(GsFillType.Linear, d.FillType);
        Assert.Equal(12f, d.CornerRadius);
        Assert.Equal(30f, d.Rotation);
        Assert.False(d.Clip);
    }

    [Fact]
    public void GsPolygon_Points_RoundTrip()
    {
        var p = new GsPolygon { Points = "0,0 1,0 0.5,1", Closed = true, Fill = "Point" };
        var xml = GoGudxConverter.SerializeControl(p);
        var d = (GsPolygon)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal("0,0 1,0 0.5,1", d.Points);   // 콤마 포함 점이 그대로 보존(공백 구분)
        Assert.True(d.Closed);
    }
}
