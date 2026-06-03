using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;

namespace Going.UI.Controls
{
    /// <summary>
    /// 칩(chip / 배지 / 태그) 컨트롤. 알약(pill) 모양 영역에 선행 요소(아이콘 또는 상태 dot) + 텍스트
    /// + 선택적 닫기(×) 버튼을 표시합니다. 상태 표시("● LIVE"), 아이콘 태그, 필터/제거 칩 등에 사용합니다.
    /// <para>색은 모두 테마 색 키(<c>"Good"</c>, <c>"Point"</c>, <c>"#10231A"</c> 등)로 지정합니다.
    /// 선행 요소는 <see cref="IconString"/>이 우선이며, 없으면 <see cref="DotColor"/>를 씁니다.</para>
    /// </summary>
    public class GoChip : GoControl
    {
        #region Properties
        /// <summary>표시할 텍스트.</summary>
        [GoProperty(PCategory.Control, 0)] public string Text { get; set; } = "Chip";
        /// <summary>글꼴 이름.</summary>
        [GoFontNameProperty(PCategory.Control, 2)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일.</summary>
        [GoProperty(PCategory.Control, 3)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Bold;
        /// <summary>글꼴 크기.</summary>
        [GoProperty(PCategory.Control, 4)] public float FontSize { get; set; } = 12;
        /// <summary>텍스트 색 (테마 색 키).</summary>
        [GoProperty(PCategory.Control, 9)] public string TextColor { get; set; } = "Fore";

        /// <summary>칩 배경(채움) 색 (테마 색 키). null이면 채우지 않습니다.</summary>
        [GoProperty(PCategory.Control, 10)] public string? ChipColor { get; set; } = "Base2";
        /// <summary>테두리 색 (테마 색 키). null이면 그리지 않습니다.</summary>
        [GoProperty(PCategory.Control, 11)] public string? BorderColor { get; set; } = "Base3";
        /// <summary>테두리 두께.</summary>
        [GoProperty(PCategory.Control, 15)] public float BorderWidth { get; set; } = 1F;

        /// <summary>선행 아이콘(FontAwesome 등). 설정되면 dot 대신 아이콘을 그립니다.</summary>
        [GoProperty(PCategory.Control, 1)] public string? IconString { get; set; } = null;
        /// <summary>아이콘 색 (테마 색 키). null이면 <see cref="TextColor"/>를 사용.</summary>
        [GoProperty(PCategory.Control, 12)] public string? IconColor { get; set; } = null;
        /// <summary>아이콘 크기.</summary>
        [GoProperty(PCategory.Control, 5)] public float IconSize { get; set; } = 13F;

        /// <summary>선행 상태 dot 색 (테마 색 키). <see cref="IconString"/>이 없고 이 값이 있으면 dot을 그립니다.</summary>
        [GoProperty(PCategory.Control, 13)] public string? DotColor { get; set; } = null;
        /// <summary>상태 dot 지름.</summary>
        [GoProperty(PCategory.Control, 6)] public float DotSize { get; set; } = 8F;
        /// <summary>선행 요소와 텍스트 사이 간격.</summary>
        [GoProperty(PCategory.Control, 7)] public float Gap { get; set; } = 7F;

        /// <summary>닫기(×) 버튼 표시 여부. true이면 텍스트 뒤에 닫기 아이콘을 그리고 클릭 시 <see cref="CloseClicked"/>를 발생시킵니다.</summary>
        [GoProperty(PCategory.Control, 19)] public bool Closable { get; set; } = false;
        /// <summary>닫기 아이콘(FontAwesome).</summary>
        [GoProperty(PCategory.Control, 20)] public string CloseIcon { get; set; } = "fa-xmark";
        /// <summary>닫기 아이콘 색 (테마 색 키). null이면 <see cref="TextColor"/>를 사용.</summary>
        [GoProperty(PCategory.Control, 14)] public string? CloseColor { get; set; } = null;
        /// <summary>닫기 아이콘 크기.</summary>
        [GoProperty(PCategory.Control, 8)] public float CloseSize { get; set; } = 11F;

        /// <summary>모서리 반경. null이면 완전한 알약(pill, 높이/2) 모양, 값이 있으면 해당 반경.</summary>
        [GoProperty(PCategory.Control, 16)] public float? Corner { get; set; } = null;
        /// <summary>칩 내용의 정렬. 세로는 항상 가운데로 처리합니다.</summary>
        [GoProperty(PCategory.Control, 17)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        /// <summary>좌우 내부 여백.</summary>
        [GoProperty(PCategory.Control, 18)] public float SidePadding { get; set; } = 12F;
        #endregion

        #region Event
        /// <summary>닫기(×) 버튼이 클릭되었을 때 발생합니다.</summary>
        public event EventHandler? CloseClicked;
        #endregion

        #region Member
        private SKRect _closeRect = SKRect.Empty;
        private bool _closeHover;
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rt = Areas()["Content"];
            float r = Corner.HasValue ? Math.Clamp(Corner.Value, 0F, Math.Min(rt.Width, rt.Height) / 2F)
                                      : Math.Min(rt.Width, rt.Height) / 2F;

            using var p = new SKPaint { IsAntialias = true };

            // 배경
            if (ChipColor != null)
            {
                p.IsStroke = false;
                p.Color = thm.ToColor(ChipColor);
                using var rr = new SKRoundRect(rt, r);
                canvas.DrawRoundRect(rr, p);
            }
            // 테두리
            if (BorderColor != null && BorderWidth > 0)
            {
                var brt = rt; brt.Inflate(-BorderWidth / 2F, -BorderWidth / 2F);
                p.IsStroke = true;
                p.StrokeWidth = BorderWidth;
                p.Color = thm.ToColor(BorderColor);
                using var rb = new SKRoundRect(brt, Math.Max(0, r - BorderWidth / 2F));
                canvas.DrawRoundRect(rb, p);
            }

            // 그룹: [선행(아이콘|dot)] + 텍스트 + [닫기]
            bool hasIcon = !string.IsNullOrEmpty(IconString);
            bool hasDot = !hasIcon && DotColor != null && DotSize > 0;
            var tsz = Util.MeasureText(Text, FontName, FontStyle, FontSize);
            float leadW = hasIcon ? IconSize : (hasDot ? DotSize : 0F);
            float leadGap = (hasIcon || hasDot) ? Gap : 0F;
            float closeW = Closable ? CloseSize : 0F;
            float closeGap = Closable ? Gap : 0F;
            float groupW = leadW + leadGap + tsz.Width + closeGap + closeW;

            bool left = ContentAlignment is GoContentAlignment.TopLeft or GoContentAlignment.MiddleLeft or GoContentAlignment.BottomLeft;
            bool right = ContentAlignment is GoContentAlignment.TopRight or GoContentAlignment.MiddleRight or GoContentAlignment.BottomRight;
            float x = left ? rt.Left + SidePadding
                    : right ? rt.Right - SidePadding - groupW
                    : rt.MidX - groupW / 2F;
            float cy = rt.MidY;

            // 선행 요소
            if (hasIcon)
            {
                var irt = new SKRect(x, cy - IconSize / 2F, x + IconSize, cy + IconSize / 2F);
                Util.DrawIcon(canvas, IconString, IconSize, irt, thm.ToColor(IconColor ?? TextColor));
                x += IconSize + leadGap;
            }
            else if (hasDot)
            {
                p.IsStroke = false;
                p.Color = thm.ToColor(DotColor!);
                canvas.DrawCircle(x + DotSize / 2F, cy, DotSize / 2F, p);
                x += DotSize + leadGap;
            }

            // 텍스트
            var rtText = new SKRect(x, rt.Top, x + tsz.Width + 1, rt.Bottom);
            Util.DrawText(canvas, Text, FontName, FontStyle, FontSize, rtText, thm.ToColor(TextColor), GoContentAlignment.MiddleLeft);
            x += tsz.Width + closeGap;

            // 닫기 버튼
            if (Closable)
            {
                var crt = new SKRect(x, cy - CloseSize / 2F, x + CloseSize, cy + CloseSize / 2F);
                _closeRect = crt; _closeRect.Inflate(4, 4);   // 히트 영역은 살짝 넓게
                if (_closeHover)
                {
                    p.IsStroke = false;
                    p.Color = thm.ToColor(TextColor).WithAlpha(40);
                    canvas.DrawCircle(crt.MidX, crt.MidY, CloseSize / 2F + 4F, p);
                }
                Util.DrawIcon(canvas, CloseIcon, CloseSize, crt, thm.ToColor(CloseColor ?? TextColor));
            }
            else _closeRect = SKRect.Empty;

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);
            if (Closable)
            {
                bool h = _closeRect.Contains(x, y);
                if (h != _closeHover) { _closeHover = h; Design?.Invalidate(); }
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            base.OnMouseUp(x, y, button);
            if (Closable && _closeRect.Contains(x, y))
                CloseClicked?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
