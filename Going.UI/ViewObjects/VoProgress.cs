using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 값 기반 진행 막대 컨트롤입니다 (Vo 패밀리, leaf). 트랙 위에 Value/Max 비율만큼 채움을 그립니다.
    /// <para>채움 폭 해석은 OnDraw에서 수행하므로 <see cref="Value"/> setter는 필드 대입만입니다.</para>
    /// </summary>
    public class VoProgress : GoControl
    {
        /// <summary>현재 값.</summary>
        [GoProperty(PCategory.Control, 0)] public float Value { get; set; } = 0F;
        /// <summary>최대치.</summary>
        [GoProperty(PCategory.Control, 5)] public float Max { get; set; } = 100F;
        /// <summary>트랙(배경) 색.</summary>
        [GoProperty(PCategory.Control, 1)] public string TrackColor { get; set; } = "Base3";
        /// <summary>채움 시작 색.</summary>
        [GoProperty(PCategory.Control, 2)] public string FillColor { get; set; } = "Point";
        /// <summary>채움 끝 색 (그라데이션). null이면 단색.</summary>
        [GoProperty(PCategory.Control, 3)] public string? FillColor2 { get; set; }
        /// <summary>모서리 반지름.</summary>
        [GoProperty(PCategory.Control, 4)] public float Radius { get; set; } = 6F;

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var bounds = Areas()["Content"];
            using var track = new SKRoundRect(bounds, Radius);
            using var p = new SKPaint { IsAntialias = true, IsStroke = false, Color = thm.ToColor(TrackColor) };
            canvas.DrawRoundRect(track, p);

            float ratio = Max != 0 ? Math.Clamp(Value / Max, 0F, 1F) : 0F;
            if (ratio > 0)
            {
                var fillRect = new SKRect(bounds.Left, bounds.Top, bounds.Left + bounds.Width * ratio, bounds.Bottom);
                using var fill = new SKRoundRect(fillRect, Radius);
                SKShader? shader = null;
                try
                {
                    if (FillColor2 != null)
                    {
                        shader = SKShader.CreateLinearGradient(
                            new SKPoint(bounds.Left, 0), new SKPoint(bounds.Right, 0),
                            new[] { thm.ToColor(FillColor), thm.ToColor(FillColor2) }, (float[]?)null, SKShaderTileMode.Clamp);
                        p.Shader = shader;
                    }
                    else p.Color = thm.ToColor(FillColor);
                    canvas.DrawRoundRect(fill, p);
                }
                finally { shader?.Dispose(); }
            }
            base.OnDraw(canvas, thm);
        }
    }
}
