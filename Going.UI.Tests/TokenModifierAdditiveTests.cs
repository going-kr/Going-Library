using Going.UI.Themes;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace Going.UI.Tests;

// 토큰 명도 modifier 가산화 검증: 어두운 베이스에서도 -light-N 단계가 균등/가시적으로 벌어지는지.
public class TokenModifierAdditiveTests
{
    private readonly ITestOutputHelper _out;
    public TokenModifierAdditiveTests(ITestOutputHelper o) => _out = o;

    private static float L(SKColor c) { c.ToHsl(out _, out _, out var l); return l; }

    [Fact]
    public void DarkBase_LightSteps_AreEvenlyAndVisiblySpaced()
    {
        var thm = new DarkTheme();
        // back 을 아주 어두운 색으로 강제(다크 HMI 바닥 가정)
        thm.Back = new SKColor(0x0A, 0x0E, 0x13); // L ≈ 6%

        float l0 = L(thm.ToColor("back"));
        float l1 = L(thm.ToColor("back-light-1"));
        float l2 = L(thm.ToColor("back-light-2"));
        float l3 = L(thm.ToColor("back-light-3"));
        _out.WriteLine($"back L={l0:0.0}  l1={l1:0.0}  l2={l2:0.0}  l3={l3:0.0}");

        // 균등: 각 단계 간격이 ~8L (가산), 어두워도 동일 폭
        Assert.InRange(l1 - l0, 7f, 9f);
        Assert.InRange(l2 - l1, 7f, 9f);
        Assert.InRange(l3 - l2, 7f, 9f);
    }

    [Fact]
    public void LegacyWords_MapToSteps()
    {
        var thm = new DarkTheme();
        thm.Back = new SKColor(0x0A, 0x0E, 0x13);
        Assert.Equal(L(thm.ToColor("back-light-1")), L(thm.ToColor("back-light")), 1);
        Assert.Equal(L(thm.ToColor("back-light-2")), L(thm.ToColor("back-lightlight")), 1);
        Assert.Equal(L(thm.ToColor("back-dark-2")), L(thm.ToColor("back-darkdark")), 1);
    }

    [Fact]
    public void NumberClampedTo1Through9()
    {
        var thm = new DarkTheme();
        thm.Back = new SKColor(0x0A, 0x0E, 0x13);
        // N=9 가 최대; N=15 는 9 로 클램프되어 동일
        Assert.Equal(L(thm.ToColor("back-light-9")), L(thm.ToColor("back-light-15")), 1);
    }

    [Fact]
    public void PointGradientEnds_DeriveFromSameToken()
    {
        var thm = new DarkTheme();
        thm.Point = new SKColor(0x2D, 0xD4, 0xBF); // teal accent
        var start = thm.ToColor("point");
        var end = thm.ToColor("point-light-2");
        // 같은 hue(8bit RGB 왕복 양자화로 ~1° 오차 허용), 끝색이 더 밝음
        start.ToHsl(out var hs, out _, out _);
        end.ToHsl(out var he, out _, out _);
        Assert.InRange(System.Math.Abs(hs - he), 0f, 1f);
        Assert.True(L(end) > L(start));
    }
}
