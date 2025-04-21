using Going.UI.Controls;
using Going.UI.Forms.Controls;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using Windows.Devices.Gpio.Provider;

namespace Going.UI.Forms.ImageCanvas
{
    [Designer(typeof(ControlDesigner))]
    public class IcContainer : UserControl
    {
        #region Properties
        public string? OffImage { get => sOffImage; set { if (sOffImage != value) { sOffImage = value; Invalidate(); } } }
        public string? OnImage { get => sOnImage; set { if (sOnImage != value) { sOnImage = value; Invalidate(); } } }

        [Editor("System.Windows.Forms.Design.FolderNameEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        public string? ImageFolder
        {
            get => sImageFolder;
            set
            {
                if (sImageFolder != value)
                {
                    sImageFolder = value;
                    if (sImageFolder != null) IcResources.Load(sImageFolder);
                    Invalidate();
                }
            }
        }

        public string BackgroundColor
        {
            get => sBackgroundColor;
            set
            {
                if (sBackgroundColor != value)
                {
                    sBackgroundColor = value;
                    Invalidate();
                }
            }
        }
        #endregion

        #region Member Variable
        private string? sImageFolder;
        private string? sOffImage, sOnImage;
        private string sBackgroundColor = "Back";
        #endregion

        #region Constructor
        public IcContainer()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            DoubleBuffered = true;
            ResizeRedraw = true;
        }
        #endregion

        #region Override
        #region OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var bitmap = new SKBitmap(this.Width, this.Height))
            using (var canvas = new SKCanvas(bitmap))
            using (var surface = SKSurface.Create(bitmap.Info))
            {
                var cBack = GoTheme.Current.ToColor(BackgroundColor);
                canvas.Clear(cBack);

                using var p = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                var sp = canvas.SaveLayer(p);

                OnPaintSurface(canvas);

                canvas.RestoreToCount(sp);

                using (var bmp = bitmap.ToBitmap())
                    e.Graphics.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
            }
        }
        #endregion

        #region OnPaintSurface
        protected virtual void OnPaintSurface(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);

            var ip = IcResources.Get(ImageFolder);
            if (ip != null && ip.GetImage(OffImage)?.FirstOrDefault() is SKImage off && ip.GetImage(OnImage)?.FirstOrDefault() is SKImage on)
                canvas.DrawImage(off, rtBox, Util.Sampling);
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #endregion

      
    }
}
