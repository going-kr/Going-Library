using Going.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.ImageCanvas
{
    public class IcOnOff : SKControl
    {
        #region Properties
        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value)
                {
                    bOnOff = value;
                    Invalidate();
                }
            }
        }
        #endregion

        #region Member Variable
        private bool bOnOff = false;
        #endregion

        #region Override
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            if (Parent is IcCanvas c && c.On != null && c.Off != null)
            {
                var sx = c.On.Width / Parent.Width;
                var sy = c.On.Height / Parent.Height;
                canvas.DrawBitmap(OnOff ? c.On : c.Off, Util.FromRect(Left, Top, Width, Height), Util.FromRect(0, 0, Width, Height));
            }

            if (DesignMode)
            {
                using var p = new SkiaSharp.SKPaint { IsAntialias = false };
                using var pe = SKPathEffect.CreateDash([2F, 2F], 2);
                p.IsStroke = true;
                p.Color = SKColors.Red;
                p.PathEffect = pe;
                canvas.DrawRect(Util.FromRect(0, 0, Width - 1, Height - 1), p);
                p.PathEffect = null;
            }

            base.OnPaintSurface(e);
        }
        #endregion
    }
}
