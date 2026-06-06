using Going.UI.Datas;
using SkiaSharp;

namespace Going.UI.Controls.Shapes
{
    /// <summary>다각형/폴리라인 셰이프. 점은 공백 구분 정규화 좌표("x,y x,y", 0~1). Closed면 닫혀서 fill 가능.</summary>
    public class GsPolygon : GsShape
    {
        /// <summary>꼭짓점(정규화 0~1, 공백 구분). 기본 삼각형.</summary>
        [GoProperty(PCategory.Control, 20)] public string Points { get; set; } = "0.5,0 1,1 0,1";
        /// <summary>경로를 닫을지(닫으면 fill 가능).</summary>
        [GoProperty(PCategory.Control, 21)] public bool Closed { get; set; } = true;

        /// <summary>닫힌 다각형만 fill.</summary>
        protected override bool Fillable => Closed;

        /// <inheritdoc/>
        protected override SKPath GetPath(SKRect bounds)
        {
            var path = new SKPath();
            var pts = ParsePoints(Points, bounds);
            if (pts.Length >= 2)
            {
                path.MoveTo(pts[0]);
                for (int i = 1; i < pts.Length; i++) path.LineTo(pts[i]);
                if (Closed) path.Close();
            }
            return path;
        }
    }
}
