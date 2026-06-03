using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 가장 기본적인 시각 컨테이너입니다 (Vo 패밀리). 사각형(라운드 가능) 영역에 배경/그라데이션/테두리/그림자를
    /// 그리고 자식 컨트롤을 담습니다. HTML의 <c>div</c>에 해당. 색은 테마 색 키(<c>"Back"</c>,<c>"#FF8800"</c> 등).
    /// <para>자식 배치는 표준 GoControl Dock/Bounds 모델을 따릅니다(에디터 자유 배치). 카드 내부를 행/열로
    /// 나누려면 자식으로 <see cref="VoGrid"/>/<see cref="VoStack"/>(Dock=Fill)를 넣으세요.</para>
    /// </summary>
    public class VoBox : GoContainer
    {
        // ── Fill ──────────────────────────────────────────────
        /// <summary>배경/그라데이션 시작 색 (테마 색 키). null이면 칠하지 않습니다.</summary>
        [GoProperty(PCategory.Control, 0)] public string? Background { get; set; }
        /// <summary>채우기 방식 (단색/선형/방사형).</summary>
        [GoProperty(PCategory.Control, 6)] public VoFillType FillType { get; set; } = VoFillType.Solid;
        /// <summary>그라데이션 끝 색 (테마 색 키). null이면 Background를 사용.</summary>
        [GoProperty(PCategory.Control, 1)] public string? FillColor2 { get; set; }
        /// <summary>선형 그라데이션 방향(도). 0=좌→우, 90=상→하.</summary>
        [GoProperty(PCategory.Control, 7)] public float GradientAngle { get; set; } = 90F;

        // ── Border ────────────────────────────────────────────
        /// <summary>테두리 색 (테마 색 키). null이면 그리지 않습니다.</summary>
        [GoProperty(PCategory.Control, 2)] public string? BorderColor { get; set; }
        /// <summary>테두리 두께.</summary>
        [GoProperty(PCategory.Control, 4)] public float BorderWidth { get; set; } = 1F;
        /// <summary>모서리 반지름.</summary>
        [GoProperty(PCategory.Control, 5)] public float BorderRadius { get; set; } = 0F;

        // ── Shadow ────────────────────────────────────────────
        /// <summary>그림자 색 (테마 색 키). null이면 그림자 없음.</summary>
        [GoProperty(PCategory.Control, 3)] public string? ShadowColor { get; set; }
        /// <summary>그림자 X 오프셋.</summary>
        [GoProperty(PCategory.Control, 8)] public float ShadowX { get; set; } = 0F;
        /// <summary>그림자 Y 오프셋.</summary>
        [GoProperty(PCategory.Control, 9)] public float ShadowY { get; set; } = 2F;
        /// <summary>그림자 흐림 정도.</summary>
        [GoProperty(PCategory.Control, 10)] public float ShadowBlur { get; set; } = 4F;

        // ── Effect ────────────────────────────────────────────
        /// <summary>불투명도 (0~1).</summary>
        [GoProperty(PCategory.Control, 11)] public float Opacity { get; set; } = 1F;

        /// <summary>자식 영역의 내부 여백. 자식들은 이만큼 인셋된 영역에 배치됩니다.</summary>
        [GoProperty(PCategory.Bounds, 11)] public GoPadding Padding { get; set; } = new(0);

        /// <summary>자식 컨트롤 목록.</summary>
        [GoChildList]
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        /// <summary>자식 배치 영역 = Content 에서 Padding 인셋.</summary>
        [JsonIgnore] public override SKRect PanelBounds => Util.FromRect(Areas()["Content"], Padding);

        /// <summary>JSON 역직렬화용 생성자.</summary>
        [JsonConstructor] public VoBox(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary><see cref="VoBox"/>의 새 인스턴스를 초기화합니다.</summary>
        public VoBox() { }

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var bounds = Areas()["Content"];
            var op = Math.Clamp(Opacity, 0F, 1F);
            bool hasFill = Background != null;
            bool hasBorder = BorderColor != null && BorderWidth > 0;

            if (hasFill)
            {
                using var p = new SKPaint { IsAntialias = true, IsStroke = false };
                var c1 = Apply(thm.ToColor(Background!), op);
                SKShader? shader = null; SKImageFilter? shadow = null;
                try
                {
                    if (FillType != VoFillType.Solid)
                    {
                        var c2 = Apply(thm.ToColor(FillColor2 ?? Background!), op);
                        var colors = new[] { c1, c2 };
                        shader = FillType == VoFillType.Linear
                            ? SKShader.CreateLinearGradient(AngleA(bounds), AngleB(bounds), colors, null, SKShaderTileMode.Clamp)
                            : SKShader.CreateRadialGradient(new SKPoint(bounds.MidX, bounds.MidY), Math.Max(bounds.Width, bounds.Height) / 2F, colors, null, SKShaderTileMode.Clamp);
                        p.Shader = shader;
                    }
                    else p.Color = c1;

                    if (ShadowColor != null)
                    {
                        shadow = SKImageFilter.CreateDropShadow(ShadowX, ShadowY, ShadowBlur, ShadowBlur, Apply(thm.ToColor(ShadowColor), op));
                        p.ImageFilter = shadow;
                    }
                    using var rr = new SKRoundRect(bounds, BorderRadius);
                    canvas.DrawRoundRect(rr, p);
                }
                finally { shader?.Dispose(); shadow?.Dispose(); }
            }

            if (hasBorder)
            {
                var brt = bounds; brt.Inflate(-BorderWidth / 2F, -BorderWidth / 2F);
                using var rb = new SKRoundRect(brt, BorderRadius);
                using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = BorderWidth, Color = Apply(thm.ToColor(BorderColor!), op) };
                canvas.DrawRoundRect(rb, p);
            }

            base.OnDraw(canvas, thm);   // GoContainer: OnLayout + 자식 그리기
        }

        /// <inheritdoc/>
        protected override void OnLayout()
        {
            var rtPanel = PanelBounds;
            var nofills = Childrens.Where(c => c.Dock != GoDockStyle.Fill).ToList();
            var fills = Childrens.Where(c => c.Dock == GoDockStyle.Fill).ToList();
            var bnds = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height);
            foreach (var control in nofills) bnds = ApplyDocking(control, bnds);
            foreach (var c in fills)
            {
                c.Left = bnds.Left + c.Margin.Left;
                c.Top = bnds.Top + c.Margin.Top;
                c.Right = bnds.Right - c.Margin.Right;
                c.Bottom = bnds.Bottom - c.Margin.Bottom;
            }
        }

        private SKPoint AngleA(SKRect b) { var r = GradientAngle * Math.PI / 180.0; float dx = (float)Math.Cos(r), dy = (float)Math.Sin(r); float e = Math.Abs(dx) * b.Width / 2F + Math.Abs(dy) * b.Height / 2F; return new SKPoint(b.MidX - dx * e, b.MidY - dy * e); }
        private SKPoint AngleB(SKRect b) { var r = GradientAngle * Math.PI / 180.0; float dx = (float)Math.Cos(r), dy = (float)Math.Sin(r); float e = Math.Abs(dx) * b.Width / 2F + Math.Abs(dy) * b.Height / 2F; return new SKPoint(b.MidX + dx * e, b.MidY + dy * e); }
        private static SKColor Apply(SKColor c, float o) => o >= 1F ? c : c.WithAlpha((byte)(c.Alpha * o));
    }
}
