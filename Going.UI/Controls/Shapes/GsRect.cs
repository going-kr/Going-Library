using Going.UI.Datas;
using SkiaSharp;

namespace Going.UI.Controls.Shapes
{
    /// <summary>사각형(라운드 가능) 셰이프.</summary>
    public class GsRect : GsShape
    {
        /// <summary>모서리 반지름.</summary>
        [GoProperty(PCategory.Control, 20)] public float CornerRadius { get; set; } = 0F;

        /// <inheritdoc/>
        protected override SKPath GetPath(SKRect bounds)
        {
            var path = new SKPath();
            if (CornerRadius > 0) path.AddRoundRect(new SKRoundRect(bounds, CornerRadius));
            else path.AddRect(bounds);
            return path;
        }
    }
}
