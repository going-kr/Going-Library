using Going.UI.Enums;
using Going.UI.ImageCanvas;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Controls;

namespace Going.UI.Forms.ImageCanvas
{
    public class IcState : SKControl
    {
        #region Properties
        public string? StateName { get => sStateName; set { if (sStateName != value) { sStateName = value; Invalidate(); } } }
        public int State { get => nState; set { if (nState != value) { nState = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<IcStateImage> StateImages { get; } = [];
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

        #region Override
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var thm = GoThemeW.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);

            var (ip, off, on, cBack) = vars();
            if (ip != null && Parent != null && on != null && off != null)
            {
                var sx = on.Width / Parent.Width;
                var sy = on.Height / Parent.Height;
                canvas.DrawImage(off, Util.FromRect(Left * sx, Top * sy, Width * sx, Height * sy), rtBox, Util.Sampling);

                var simg = StateImages.FirstOrDefault(x => x.State == State);
                var img = ip.GetImage(simg?.Image)?.FirstOrDefault();
                if (img != null) canvas.DrawImage(img, rtBox, Util.Sampling);
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
        (IcFolder? ip, SKImage? on, SKImage? off, SKColor cBack) vars()
        {
            var thm = GoThemeW.Current;
            IcFolder? ip = null;
            SKImage? on = null, off = null;
            SKColor cBack = thm.Back;

            if (Parent is IcContainer con)
            {
                ip = IcResources.Get(con.ImageFolder);
                cBack = thm.ToColor(con.BackgroundColor);
                if (ip != null && ip.GetImage(con.OffImage)?.FirstOrDefault() is SKImage ioff && ip.GetImage(con.OnImage)?.FirstOrDefault() is SKImage ion) { on = ion; off = ioff; }
            }
            else if (Parent is TabPage tab && tab.Parent is IcCanvas ic)
            {
                ip = IcResources.Get(ic.ImageFolder);
                cBack = thm.ToColor(ic.BackgroundColor);
                var nm = ic.TabPageImage.FirstOrDefault(x => x.TabPage == ic.SelectedTab);
                if (ip != null && nm != null && ip.GetImage(nm.OffImage)?.FirstOrDefault() is SKImage ioff && ip.GetImage(nm.OnImage)?.FirstOrDefault() is SKImage ion) { on = ion; off = ioff; }
            }

            return (ip, on, off, cBack);
        }
        #endregion
        #endregion
    }

    public class IcStateImage : Component
    {
        public string? Image { get; set; }
        public int State { get; set; }
    }
}
