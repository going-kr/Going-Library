using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 자식들을 CSS 그리드처럼 셀에 배치하는 레이아웃 컨테이너입니다.
    /// <para><see cref="Util.Grid"/>를 재활용합니다. 열/행 크기는 <c>"100px"</c>, <c>"50%"</c>, <c>"*"</c> 토큰을 씁니다.
    /// 각 자식은 <see cref="VoObj.Col"/>/<see cref="VoObj.Row"/>(+Span)로 셀을 지정합니다.</para>
    /// </summary>
    public class VoGrid : VoObj
    {
        /// <summary>열 크기 정의 (예: ["50%", "50%"], ["100px", "*"]).</summary>
        [VoProperty("Grid", 0)] public List<string> Columns { get; set; } = ["*"];
        /// <summary>행 크기 정의.</summary>
        [VoProperty("Grid", 1)] public List<string> Rows { get; set; } = ["*"];

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, SKRect bounds, GoTheme thm) { /* 배치 전용 */ }

        /// <inheritdoc/>
        protected override SKRect[] ArrangeChildren(SKRect bounds)
        {
            int n = Children.Count;
            var rects = new SKRect[n];
            if (n == 0) return rects;

            var cols = Columns is { Count: > 0 } ? Columns.ToArray() : ["*"];
            var rows = Rows is { Count: > 0 } ? Rows.ToArray() : ["*"];
            var grid = Util.Grid(bounds, cols, rows);

            int rowCount = rows.Length, colCount = cols.Length;
            for (int i = 0; i < n; i++)
            {
                var ch = Children[i];
                int c0 = Math.Clamp(ch.Col, 0, colCount - 1);
                int r0 = Math.Clamp(ch.Row, 0, rowCount - 1);
                int c1 = Math.Clamp(c0 + Math.Max(1, ch.ColSpan) - 1, 0, colCount - 1);
                int r1 = Math.Clamp(r0 + Math.Max(1, ch.RowSpan) - 1, 0, rowCount - 1);

                var a = grid[r0, c0];
                var b = grid[r1, c1];
                rects[i] = new SKRect(
                    Math.Min(a.Left, b.Left), Math.Min(a.Top, b.Top),
                    Math.Max(a.Right, b.Right), Math.Max(a.Bottom, b.Bottom));
            }
            return rects;
        }
    }
}
