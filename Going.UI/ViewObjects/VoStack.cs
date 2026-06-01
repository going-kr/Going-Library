using Going.UI.Enums;
using Going.UI.Themes;
using SkiaSharp;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 자식들을 가로 또는 세로로 균등 분할하여 배치하는 레이아웃 컨테이너입니다 (flexbox-lite).
    /// 자신은 아무것도 그리지 않고 배치만 담당합니다.
    /// </summary>
    public class VoStack : VoObj
    {
        /// <summary>배치 방향. Horizon이면 가로, Vertical이면 세로.</summary>
        [VoProperty("Layout", 2)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Vertical;

        /// <summary>자식 사이 간격.</summary>
        [VoProperty("Layout", 3)] public float Spacing { get; set; } = 0F;

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, SKRect bounds, GoTheme thm) { /* 배치 전용 — 그리지 않음 */ }

        /// <inheritdoc/>
        protected override SKRect[] ArrangeChildren(SKRect bounds)
        {
            int n = Children.Count;
            var rects = new SKRect[n];
            if (n == 0) return rects;

            bool horizon = Direction == GoDirectionHV.Horizon;
            float total = (horizon ? bounds.Width : bounds.Height) - Spacing * (n - 1);
            float each = total / n;

            float pos = horizon ? bounds.Left : bounds.Top;
            for (int i = 0; i < n; i++)
            {
                rects[i] = horizon
                    ? new SKRect(pos, bounds.Top, pos + each, bounds.Bottom)
                    : new SKRect(bounds.Left, pos, bounds.Right, pos + each);
                pos += each + Spacing;
            }
            return rects;
        }
    }
}
