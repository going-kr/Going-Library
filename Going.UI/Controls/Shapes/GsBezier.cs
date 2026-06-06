using Going.UI.Datas;
using SkiaSharp;

namespace Going.UI.Controls.Shapes
{
    /// <summary>
    /// 3차 베지에 곡선 셰이프. 점은 공백 구분 정규화 좌표("x,y x,y", 0~1)이며 [start, c1, c2, end, c1, c2, end, …]
    /// 형태로 4점 이후 3점씩 세그먼트를 잇습니다. Closed면 닫혀서 fill 가능.
    /// </summary>
    public class GsBezier : GsShape
    {
        /// <summary>제어점(정규화 0~1, 공백 구분). 기본 1개 큐빅 세그먼트(U자).</summary>
        [GoProperty(PCategory.Control, 20)] public string Points { get; set; } = "0,1 0.3,0 0.7,0 1,1";
        /// <summary>경로를 닫을지(닫으면 fill 가능).</summary>
        [GoProperty(PCategory.Control, 21)] public bool Closed { get; set; } = false;

        /// <summary>닫힌 곡선만 fill.</summary>
        protected override bool Fillable => Closed;

        /// <inheritdoc/>
        protected override SKPath GetPath(SKRect bounds)
        {
            var path = new SKPath();
            var pts = ParsePoints(Points, bounds);
            if (pts.Length >= 4)
            {
                path.MoveTo(pts[0]);
                for (int i = 1; i + 2 < pts.Length; i += 3)
                    path.CubicTo(pts[i], pts[i + 1], pts[i + 2]);
                if (Closed) path.Close();
            }
            else if (pts.Length >= 2)
            {
                path.MoveTo(pts[0]);
                for (int j = 1; j < pts.Length; j++) path.LineTo(pts[j]);
                if (Closed) path.Close();
            }
            return path;
        }
    }
}
