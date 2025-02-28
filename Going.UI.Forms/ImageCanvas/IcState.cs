using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
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
    public class IcState : SKControl
    {
        #region Properties
        public string? StateName { get => sStateName; set { if (sStateName != value) { sStateName = value; Invalidate(); } } }
        public int State { get => nState; set { if (nState != value) { nState = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        private string? sStateName;
        private int nState = 0;
        #endregion

        #region Constructor
        public IcState()
        {
 
        }
        #endregion

        #region Event
        public event EventHandler? ButtonClicked;
        #endregion

        #region Override
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var thm = GoTheme.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);

            var ic = GetCanvas();
            if (ic != null && Parent is TabPage tab && ic.PageImages.TryGetValue(tab.Name, out var img) && img.On != null && img.Off != null)
            {
                var sx = (double)img.On.Width / Parent.Width;
                var sy = (double)img.On.Height / Parent.Height;
                canvas.DrawBitmap(img.Off, Util.FromRect(Convert.ToInt16(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);

                if (StateName != null && ic.StateImages.TryGetValue(StateName, out var dic) && dic.TryGetValue(State, out var bm) && bm != null)
                    canvas.DrawBitmap(bm, rtBox);
            }
            else if (Parent is IcContainer con && con.On != null && con.Off != null)
            {
                var sx = (double)con.On.Width / con.Width;
                var sy = (double)con.On.Height / con.Height;
                canvas.DrawBitmap(con.Off, Util.FromRect(Convert.ToInt16(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);

                if (ic != null && StateName != null && ic.StateImages.TryGetValue(StateName, out var dic) && dic.TryGetValue(State, out var bm) && bm != null)
                    canvas.DrawBitmap(bm, rtBox);
            }

            if (!Enabled)
            {
                using var p = new SKPaint { IsAntialias = true };

                p.IsStroke = false;
                p.Color = thm.ToColor(ic.BackgroundColor).WithAlpha(180);
                canvas.DrawRect(rtBox, p);
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

        #region Method
        IcCanvas? GetCanvas()
        {
            var p = Parent;
            while (p is not IcCanvas && p != null) p = p.Parent;
            return p as IcCanvas;
        }
        #endregion
    }
}
