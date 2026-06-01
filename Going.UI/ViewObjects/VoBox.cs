using Going.UI.Themes;
using SkiaSharp;
using System;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 가장 기본적인 시각 노드입니다. 사각형(라운드 가능) 영역에 배경/테두리/그림자를 그립니다.
    /// HTML의 <c>div</c>에 해당하며 자식을 가질 수 있습니다.
    /// <para>색은 모두 테마 색 문자열(<c>"Back"</c>, <c>"Point-50"</c>, <c>"#FF8800"</c> 등)로 지정하며
    /// 그릴 때 <see cref="GoTheme.ToColor"/>로 해석됩니다.</para>
    /// </summary>
    public class VoBox : VoObj
    {
        // ── Fill ──────────────────────────────────────────────
        /// <summary>배경/그라데이션 시작 색 (테마 색 키). null이면 칠하지 않습니다.</summary>
        [VoProperty("Fill", 0)] public string? Background { get; set; }
        /// <summary>채우기 방식 (단색/선형/방사형).</summary>
        [VoProperty("Fill", 1)] public VoFillType FillType { get; set; } = VoFillType.Solid;
        /// <summary>그라데이션 끝 색 (테마 색 키). null이면 Background를 사용.</summary>
        [VoProperty("Fill", 2)] public string? FillColor2 { get; set; }
        /// <summary>선형 그라데이션 방향(도). 0=좌→우, 90=상→하.</summary>
        [VoProperty("Fill", 3)] public float GradientAngle { get; set; } = 90F;

        // ── Border ────────────────────────────────────────────
        /// <summary>테두리 색 (테마 색 키). null이면 그리지 않습니다.</summary>
        [VoProperty("Border", 0)] public string? BorderColor { get; set; }
        /// <summary>테두리 두께.</summary>
        [VoProperty("Border", 1)] public float BorderWidth { get; set; } = 1F;
        /// <summary>모서리 반지름.</summary>
        [VoProperty("Border", 2)] public float BorderRadius { get; set; } = 0F;

        // ── Shadow ────────────────────────────────────────────
        /// <summary>그림자 색 (테마 색 키). null이면 그림자 없음.</summary>
        [VoProperty("Shadow", 0)] public string? ShadowColor { get; set; }
        /// <summary>그림자 X 오프셋.</summary>
        [VoProperty("Shadow", 1)] public float ShadowX { get; set; } = 0F;
        /// <summary>그림자 Y 오프셋.</summary>
        [VoProperty("Shadow", 2)] public float ShadowY { get; set; } = 2F;
        /// <summary>그림자 흐림 정도.</summary>
        [VoProperty("Shadow", 3)] public float ShadowBlur { get; set; } = 4F;

        // ── Effect ────────────────────────────────────────────
        /// <summary>불투명도 (0~1).</summary>
        [VoProperty("Effect", 0)] public float Opacity { get; set; } = 1F;

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, SKRect bounds, GoTheme thm)
        {
            var op = Math.Clamp(Opacity, 0F, 1F);
            bool hasFill = Background != null;
            bool hasBorder = BorderColor != null && BorderWidth > 0;
            if (!hasFill && !hasBorder) return;

            if (hasFill)
            {
                using var rr = new SKRoundRect(bounds, BorderRadius);
                using var p = new SKPaint { IsAntialias = true, IsStroke = false };
                var c1 = Apply(thm.ToColor(Background!), op);

                SKShader? shader = null;
                SKImageFilter? shadow = null;
                try
                {
                    if (FillType != VoFillType.Solid)
                    {
                        var c2 = Apply(thm.ToColor(FillColor2 ?? Background!), op);
                        var colors = new SKColor[] { c1, c2 };
                        shader = FillType == VoFillType.Linear
                            ? SKShader.CreateLinearGradient(AngleA(bounds), AngleB(bounds), colors, (float[]?)null, SKShaderTileMode.Clamp)
                            : SKShader.CreateRadialGradient(new SKPoint(bounds.MidX, bounds.MidY), Math.Max(bounds.Width, bounds.Height) / 2F, colors, (float[]?)null, SKShaderTileMode.Clamp);
                        p.Shader = shader;
                    }
                    else p.Color = c1;

                    if (ShadowColor != null)
                    {
                        shadow = SKImageFilter.CreateDropShadow(ShadowX, ShadowY, ShadowBlur, ShadowBlur, Apply(thm.ToColor(ShadowColor), op));
                        p.ImageFilter = shadow;
                    }

                    canvas.DrawRoundRect(rr, p);
                }
                finally { shader?.Dispose(); shadow?.Dispose(); }
            }

            if (hasBorder)
            {
                var brt = bounds;
                brt.Inflate(-BorderWidth / 2F, -BorderWidth / 2F);
                using var rb = new SKRoundRect(brt, BorderRadius);
                using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = BorderWidth, Color = Apply(thm.ToColor(BorderColor!), op) };
                canvas.DrawRoundRect(rb, p);
            }
        }

        private SKPoint AngleA(SKRect b)
        {
            var rad = GradientAngle * Math.PI / 180.0;
            float dx = (float)Math.Cos(rad), dy = (float)Math.Sin(rad);
            float ext = Math.Abs(dx) * b.Width / 2F + Math.Abs(dy) * b.Height / 2F;
            return new SKPoint(b.MidX - dx * ext, b.MidY - dy * ext);
        }

        private SKPoint AngleB(SKRect b)
        {
            var rad = GradientAngle * Math.PI / 180.0;
            float dx = (float)Math.Cos(rad), dy = (float)Math.Sin(rad);
            float ext = Math.Abs(dx) * b.Width / 2F + Math.Abs(dy) * b.Height / 2F;
            return new SKPoint(b.MidX + dx * ext, b.MidY + dy * ext);
        }

        private static SKColor Apply(SKColor c, float opacity)
            => opacity >= 1F ? c : c.WithAlpha((byte)(c.Alpha * opacity));
    }
}
