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

            var (ip, img, cBack) = vars();
            if (ip != null && img != null && Parent != null && img.On != null && img.Off != null)
            {
                var sx = (double)img.On.Width / Parent.Width;
                var sy = (double)img.On.Height / Parent.Height;
                canvas.DrawBitmap(img.Off, Util.FromRect(Convert.ToInt16(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);

                if (StateName != null && ip.States.TryGetValue(StateName, out var dic) && dic.TryGetValue(State, out var bm) && bm != null)
                    canvas.DrawBitmap(bm, rtBox);
            }

            #region Enabled
            if (!Enabled)
            {
                using var p = new SKPaint { IsAntialias = true };

                p.IsStroke = false;
                p.Color = cBack.WithAlpha(180);
                canvas.DrawRect(rtBox, p);
            }
            #endregion
            #region DesignMode
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
            #endregion

            base.OnPaintSurface(e);
        }
        #endregion

        #region Method
        #region vars
        (IcImageFolder? ip, IcOnOffImage? img, SKColor cBack) vars()
        {
            var thm = GoTheme.Current;
            SKColor cBack = thm.Back;
            IcImageFolder? ip = null;
            IcOnOffImage? img = null;

            if (Parent is IcContainer con)
            {
                ip = IcResources.Get(con.ImageFolder);
                cBack = thm.ToColor(con.BackgroundColor);
                img = ip != null && con.ContainerName != null && ip.Containers.TryGetValue(con.ContainerName, out var v) ? v : null;
            }
            else if (Parent is TabPage tab && tab.Parent is IcCanvas ic)
            {
                ip = IcResources.Get(ic.ImageFolder);
                cBack = thm.ToColor(ic.BackgroundColor);
                img = ip != null && tab.Name != null && ip.Pages.TryGetValue(tab.Name, out var v) ? v : null;
            }

            return (ip, img, cBack);
        }
        #endregion
        #endregion
    }
}
