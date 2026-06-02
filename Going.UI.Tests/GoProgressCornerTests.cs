using Going.UI.Controls;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>GoProgress.Corner: null = 완전 둥근(pill), 값 = 해당 반경(0이면 각짐).</summary>
public class GoProgressCornerTests
{
    private static SKBitmap Render(float? corner, bool drawEmpty = true)
    {
        var pg = new GoProgress
        {
            Minimum = 0, Maximum = 100, Value = 50,
            EmptyColor = "Base3", FillColor = "Good", BorderWidth = 0,
            Corner = corner, DrawEmpty = drawEmpty,
            Bounds = new SKRect(0, 0, 200, 40),
        };
        var bmp = new SKBitmap(200, 40);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(new SKColor(0xFF, 0x00, 0xFF));   // 배경: 마젠타 (트랙/배경 구분용)
        pg.FireDraw(canvas, GoTheme.DarkTheme);
        canvas.Flush();
        return bmp;
    }

    [Fact]
    public void Corner_Null_IsPill_Corner0_IsSquare()
    {
        var magenta = new SKColor(0xFF, 0x00, 0xFF);
        using var pill = Render(null);   // 모서리 잘림 → 코너는 배경(마젠타)
        using var square = Render(0);    // 각짐 → 코너는 트랙색으로 채워짐

        var cPill = pill.GetPixel(2, 2);
        var cSquare = square.GetPixel(2, 2);

        Assert.Equal(magenta, cPill);        // pill: 코너 비어 배경 보임
        Assert.NotEqual(magenta, cSquare);   // square: 코너가 트랙으로 채워짐
    }

    [Fact]
    public void DrawEmpty_False_OmitsTrackBackground()
    {
        var magenta = new SKColor(0xFF, 0x00, 0xFF);
        // Value=50% 이므로 오른쪽 끝(x=190)은 빈(트랙) 영역. DrawEmpty=false면 거기 배경이 안 그려져 마젠타가 보임.
        using var withEmpty = Render(0, drawEmpty: true);
        using var noEmpty = Render(0, drawEmpty: false);

        Assert.NotEqual(magenta, withEmpty.GetPixel(190, 20));  // 트랙 그려짐
        Assert.Equal(magenta, noEmpty.GetPixel(190, 20));       // 트랙 생략 → 배경 보임
    }

    [Fact]
    public void Corner_Value_ClampedToPill()
    {
        // 두께(40)/2=20 을 초과하는 큰 값은 pill 로 클램프 → null 과 동일 렌더
        using var big = Render(9999);
        using var pill = Render(null);
        foreach (var (x, y) in new[] { (2, 2), (100, 20), (198, 38) })
            Assert.Equal(pill.GetPixel(x, y), big.GetPixel(x, y));
    }
}
