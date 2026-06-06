using SkiaSharp;

namespace Going.UI.Controls.Shapes
{
    /// <summary>원/타원 셰이프. Bounds에 맞춰 그리므로 정사각이면 원, 아니면 타원입니다.</summary>
    public class GsCircle : GsShape
    {
        /// <inheritdoc/>
        protected override SKPath GetPath(SKRect bounds)
        {
            var path = new SKPath();
            path.AddOval(bounds);
            return path;
        }
    }
}
