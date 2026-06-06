using Going.UI.Datas;
using SkiaSharp;

namespace Going.UI.Controls.Shapes
{
    /// <summary>직선 셰이프(stroke 전용). 점은 공백 구분 정규화 좌표("x,y x,y", 0~1)입니다.</summary>
    public class GsLine : GsShape
    {
        /// <summary>두 끝점(정규화 0~1, 공백 구분). 기본 가로 중앙선.</summary>
        [GoProperty(PCategory.Control, 20)] public string Points { get; set; } = "0,0.5 1,0.5";

        /// <summary>직선은 stroke 전용.</summary>
        protected override bool Fillable => false;

        /// <inheritdoc/>
        protected override SKPath GetPath(SKRect bounds)
        {
            var path = new SKPath();
            var pts = ParsePoints(Points, bounds);
            if (pts.Length >= 2)
            {
                path.MoveTo(pts[0]);
                path.LineTo(pts[1]);
            }
            return path;
        }
    }
}
