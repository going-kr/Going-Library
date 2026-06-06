using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Going.UI.Controls.Shapes
{
    /// <summary>
    /// 벡터 셰이프의 베이스. 공통 효과(채움/테두리/그림자/글로우/회전/클립)를 일괄 처리하고,
    /// 파생 셰이프는 <see cref="GetPath"/>로 기하만 정의한다. 색은 테마 색 키(<c>"Point"</c>,<c>"#FF8800"</c> 등).
    /// </summary>
    public abstract class GsShape : GoControl
    {
        // ── Fill ──────────────────────────────────────────────
        /// <summary>채움 색 (테마 색 키). null이면 채우지 않습니다.</summary>
        [GoProperty(PCategory.Control, 0)] public string? Fill { get; set; }
        /// <summary>채움 방식 (단색/선형/방사형).</summary>
        [GoProperty(PCategory.Control, 1)] public GsFillType FillType { get; set; } = GsFillType.Solid;
        /// <summary>그라데이션 끝 색 (테마 색 키). null이면 Fill 사용.</summary>
        [GoProperty(PCategory.Control, 2)] public string? FillColor2 { get; set; }
        /// <summary>선형 그라데이션 방향(도). 0=좌→우, 90=상→하.</summary>
        [GoProperty(PCategory.Control, 3)] public float GradientAngle { get; set; } = 90F;

        // ── Stroke ────────────────────────────────────────────
        /// <summary>테두리 색 (테마 색 키). null이면 그리지 않습니다.</summary>
        [GoProperty(PCategory.Control, 4)] public string? StrokeColor { get; set; }
        /// <summary>테두리 두께.</summary>
        [GoProperty(PCategory.Control, 5)] public float StrokeWidth { get; set; } = 1F;

        // ── Shadow ────────────────────────────────────────────
        /// <summary>그림자 색 (테마 색 키). null이면 그림자 없음.</summary>
        [GoProperty(PCategory.Control, 6)] public string? ShadowColor { get; set; }
        /// <summary>그림자 X 오프셋.</summary>
        [GoProperty(PCategory.Control, 7)] public float ShadowX { get; set; } = 0F;
        /// <summary>그림자 Y 오프셋.</summary>
        [GoProperty(PCategory.Control, 8)] public float ShadowY { get; set; } = 2F;
        /// <summary>그림자 흐림 정도.</summary>
        [GoProperty(PCategory.Control, 9)] public float ShadowBlur { get; set; } = 4F;

        // ── Glow ──────────────────────────────────────────────
        /// <summary>글로우 색 (테마 색 키). null이면 글로우 없음. 오프셋 0의 외곽 블러.</summary>
        [GoProperty(PCategory.Control, 10)] public string? GlowColor { get; set; }
        /// <summary>글로우 흐림 정도.</summary>
        [GoProperty(PCategory.Control, 11)] public float GlowBlur { get; set; } = 8F;

        // ── Transform / Clip ──────────────────────────────────
        /// <summary>회전 각도(도). Bounds 중심 기준.</summary>
        [GoProperty(PCategory.Control, 12)] public float Rotation { get; set; } = 0F;
        /// <summary>Bounds로 클리핑할지. false면 효과/회전이 Bounds 밖으로 그려질 수 있습니다.</summary>
        [GoProperty(PCategory.Control, 13)] public bool Clip { get; set; } = true;

        /// <inheritdoc/>
        protected internal override bool ClipToBounds => Clip;

        /// <summary>이 셰이프의 경로. 파생 클래스가 구현하는 유일한 것.</summary>
        protected abstract SKPath GetPath(SKRect bounds);
        /// <summary>닫힌 도형이면 fill 가능(기본 true). Line/열린 도형은 false로 오버라이드.</summary>
        protected virtual bool Fillable => true;

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rt = Areas()["Content"];
            int save = canvas.Save();
            try
            {
                if (Rotation != 0) canvas.RotateDegrees(Rotation, rt.MidX, rt.MidY);
                using var path = GetPath(rt);

                bool fill = Fillable && Fill != null;
                bool stroke = StrokeColor != null && StrokeWidth > 0;

                // 1) Glow / Shadow — 실루엣 뒤에 깔림
                DrawEffect(canvas, path, GlowColor, 0F, 0F, GlowBlur, fill, thm);
                DrawEffect(canvas, path, ShadowColor, ShadowX, ShadowY, ShadowBlur, fill, thm);

                // 2) Fill
                if (fill)
                {
                    using var p = new SKPaint { IsAntialias = true, IsStroke = false };
                    SKShader? shader = null;
                    try
                    {
                        if (FillType != GsFillType.Solid)
                        {
                            var c1 = thm.ToColor(Fill!);
                            var c2 = thm.ToColor(FillColor2 ?? Fill!);
                            var colors = new[] { c1, c2 };
                            shader = FillType == GsFillType.Linear
                                ? SKShader.CreateLinearGradient(AngleA(rt), AngleB(rt), colors, null, SKShaderTileMode.Clamp)
                                : SKShader.CreateRadialGradient(new SKPoint(rt.MidX, rt.MidY), Math.Max(rt.Width, rt.Height) / 2F, colors, null, SKShaderTileMode.Clamp);
                            p.Shader = shader;
                        }
                        else p.Color = thm.ToColor(Fill!);
                        canvas.DrawPath(path, p);
                    }
                    finally { shader?.Dispose(); }
                }

                // 3) Stroke
                if (stroke)
                {
                    using var p = new SKPaint
                    {
                        IsAntialias = true,
                        IsStroke = true,
                        StrokeWidth = StrokeWidth,
                        StrokeCap = SKStrokeCap.Round,
                        StrokeJoin = SKStrokeJoin.Round,
                        Color = thm.ToColor(StrokeColor!),
                    };
                    canvas.DrawPath(path, p);
                }
            }
            finally { canvas.RestoreToCount(save); }

            base.OnDraw(canvas, thm);
        }

        /// <summary>경로 실루엣(fill 또는 stroke)을 DropShadowOnly로 그려 그림자/글로우 레이어를 만든다.</summary>
        private void DrawEffect(SKCanvas canvas, SKPath path, string? colorKey, float dx, float dy, float blur, bool asFill, GoTheme thm)
        {
            if (colorKey == null || blur <= 0) return;
            using var p = new SKPaint
            {
                IsAntialias = true,
                IsStroke = !asFill,
                StrokeWidth = asFill ? 0F : StrokeWidth,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                ImageFilter = SKImageFilter.CreateDropShadowOnly(dx, dy, blur, blur, thm.ToColor(colorKey)),
            };
            canvas.DrawPath(path, p);
        }

        /// <summary>정규화 점("x,y" 0~1)을 bounds 안 실제 좌표로 환산.</summary>
        protected static SKPoint ToPoint(string token, SKRect rt)
        {
            var parts = token.Split(',');
            float x = parts.Length > 0 && float.TryParse(parts[0], out var vx) ? vx : 0F;
            float y = parts.Length > 1 && float.TryParse(parts[1], out var vy) ? vy : 0F;
            return new SKPoint(rt.Left + x * rt.Width, rt.Top + y * rt.Height);
        }

        /// <summary>공백 구분 점 목록("x,y x,y …", 정규화 0~1)을 bounds 안 좌표 배열로 환산.</summary>
        protected static SKPoint[] ParsePoints(string? spec, SKRect rt)
        {
            if (string.IsNullOrWhiteSpace(spec)) return System.Array.Empty<SKPoint>();
            var toks = spec.Split(new[] { ' ', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var pts = new SKPoint[toks.Length];
            for (int i = 0; i < toks.Length; i++) pts[i] = ToPoint(toks[i], rt);
            return pts;
        }

        private SKPoint AngleA(SKRect b) { var r = GradientAngle * Math.PI / 180.0; float dx = (float)Math.Cos(r), dy = (float)Math.Sin(r); float e = Math.Abs(dx) * b.Width / 2F + Math.Abs(dy) * b.Height / 2F; return new SKPoint(b.MidX - dx * e, b.MidY - dy * e); }
        private SKPoint AngleB(SKRect b) { var r = GradientAngle * Math.PI / 180.0; float dx = (float)Math.Cos(r), dy = (float)Math.Sin(r); float e = Math.Abs(dx) * b.Width / 2F + Math.Abs(dy) * b.Height / 2F; return new SKPoint(b.MidX + dx * e, b.MidY + dy * e); }
    }
}
