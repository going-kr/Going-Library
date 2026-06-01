using Going.UI.Themes;
using SkiaSharp;
using System;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 호(arc)를 그리는 시각 노드입니다. 원형 게이지/진행 표시에 사용합니다.
    /// <para>각도는 Skia 기준(3시 방향=0도, 시계방향 +). 게이지는 보통 트랙 호 + 값 호 두 개를 겹쳐 만듭니다.</para>
    /// </summary>
    public class VoArc : VoObj
    {
        /// <summary>시작 각도(도). 3시=0, 시계방향 +. (예: 135 = 좌하단)</summary>
        [VoProperty("Arc", 0)] public float StartAngle { get; set; } = 135F;
        /// <summary>스윕 각도(도). (예: 270 = 3/4 바퀴)</summary>
        [VoProperty("Arc", 1)] public float SweepAngle { get; set; } = 270F;
        /// <summary>호 두께.</summary>
        [VoProperty("Arc", 2)] public float Thickness { get; set; } = 12F;
        /// <summary>호 색 (테마 색 키).</summary>
        [VoProperty("Arc", 3)] public string Color { get; set; } = "Point";
        /// <summary>스윕 그라데이션 끝 색 (테마 색 키). null이면 단색.</summary>
        [VoProperty("Arc", 4)] public string? Color2 { get; set; }
        /// <summary>끝을 둥글게 처리할지.</summary>
        [VoProperty("Arc", 5)] public bool RoundCap { get; set; } = true;

        /// <summary>현재 값. 실제 그려지는 스윕 = SweepAngle × (Value/Max). (기본 Max=full)</summary>
        [VoProperty("Arc", 6)] public float Value { get; set; } = 100F;
        /// <summary>값의 최대치.</summary>
        [VoProperty("Arc", 7)] public float Max { get; set; } = 100F;

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, SKRect bounds, GoTheme thm)
        {
            if (Thickness <= 0 || SweepAngle == 0) return;

            // 값 → 스윕 (해석은 여기서. setter는 필드 대입만이라 OnUpdate 반복 호출이 싸다)
            float ratio = Max != 0 ? Math.Clamp(Value / Max, 0F, 1F) : 1F;
            float sweep = SweepAngle * ratio;
            if (sweep == 0) return;

            // 정사각형 영역 중앙 정렬 후 두께만큼 안쪽으로
            float d = Math.Min(bounds.Width, bounds.Height);
            var sq = new SKRect(
                bounds.MidX - d / 2F, bounds.MidY - d / 2F,
                bounds.MidX + d / 2F, bounds.MidY + d / 2F);
            sq.Inflate(-Thickness / 2F, -Thickness / 2F);

            using var p = new SKPaint
            {
                IsAntialias = true,
                IsStroke = true,
                StrokeWidth = Thickness,
                StrokeCap = RoundCap ? SKStrokeCap.Round : SKStrokeCap.Butt,
                Color = thm.ToColor(Color),
            };

            SKShader? shader = null;
            try
            {
                if (Color2 != null)
                {
                    var center = new SKPoint(sq.MidX, sq.MidY);
                    shader = SKShader.CreateSweepGradient(center,
                        new[] { thm.ToColor(Color), thm.ToColor(Color2) },
                        new[] { 0F, 1F }, SKShaderTileMode.Clamp, StartAngle, StartAngle + sweep);
                    p.Shader = shader;
                }

                using var path = new SKPath();
                path.AddArc(sq, StartAngle, sweep);
                canvas.DrawPath(path, p);
            }
            finally { shader?.Dispose(); }
        }
    }
}
