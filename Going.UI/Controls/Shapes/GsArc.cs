using Going.UI.Datas;
using SkiaSharp;
using System;

namespace Going.UI.Controls.Shapes
{
    /// <summary>
    /// 호(arc) 셰이프. 원형 게이지/장식에 사용합니다. 두께/색은 베이스의 Stroke(<see cref="GsShape.StrokeWidth"/>/
    /// <see cref="GsShape.StrokeColor"/>)를 씁니다. 각도는 Skia 기준(3시=0도, 시계방향 +).
    /// </summary>
    public class GsArc : GsShape
    {
        /// <summary>시작 각도(도). 3시=0, 시계방향 +.</summary>
        [GoProperty(PCategory.Control, 20)] public float StartAngle { get; set; } = 135F;
        /// <summary>스윕 각도(도).</summary>
        [GoProperty(PCategory.Control, 21)] public float SweepAngle { get; set; } = 270F;

        /// <summary>호는 stroke 전용.</summary>
        protected override bool Fillable => false;

        /// <inheritdoc/>
        protected override SKPath GetPath(SKRect bounds)
        {
            float d = Math.Min(bounds.Width, bounds.Height);
            var sq = new SKRect(bounds.MidX - d / 2F, bounds.MidY - d / 2F, bounds.MidX + d / 2F, bounds.MidY + d / 2F);
            sq.Inflate(-StrokeWidth / 2F, -StrokeWidth / 2F);   // stroke 외경이 bounds 안에 들어오도록
            var path = new SKPath();
            if (SweepAngle != 0) path.AddArc(sq, StartAngle, SweepAngle);
            return path;
        }
    }
}
