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

namespace Going.UI.Forms.ImageCanvas
{
    [Designer(typeof(ControlDesigner))]
    public class IcContainer : UserControl
    {
        #region Properties
        public string? ContainerName
        {
            get => sContainerName; 
            set
            {
                if (sContainerName != value)
                {
                    sContainerName = value;
                    load();
                    Invalidate();
                }
            }
        }

        [Editor("System.Windows.Forms.Design.FolderNameEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        public string? ImageFolder
        {
            get => sImageFolder;
            set
            {
                if (sImageFolder != value)
                {
                    sImageFolder = value;
                    load();
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

        public SKBitmap? On => imgOn;
        public SKBitmap? Off => imgOff;
        #endregion

        #region Member Variable
        private string? sImageFolder;
        private string? sContainerName;
        private string sBackgroundColor = "Back";
        private SKBitmap? imgOn, imgOff;
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

        #region Event
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

            if (On != null && Off != null)
                canvas.DrawBitmap(Off, rtBox);

            if (!Enabled)
            {
                using var p = new SKPaint { IsAntialias = true };

                p.IsStroke = false;
                p.Color = thm.ToColor(BackgroundColor).WithAlpha(180);
                canvas.DrawRect(rtBox, p);
            }
        }
        #endregion
        #endregion

        #region Method
        void load()
        {
            try
            {
                if (ImageFolder != null && ContainerName != null && Directory.Exists(ImageFolder) )
                {
                    var pattern = @"^(?i)(.+)\.Container\.(On|Off)\.(bmp|png|gif)$";
                    var regex = new Regex(pattern);
                    var imgs = new Dictionary<string, Dictionary<string, string>>();

                    #region group
                    foreach (var path in Directory.GetFiles(ImageFolder))
                    {
                        string fileName = Path.GetFileName(path.ToLower());

                        if (regex.IsMatch(fileName))
                        {
                            var vs = fileName.Split('.');
                            if (vs.Length == 4)
                            {
                                imgs.TryAdd(vs[0], []);
                                imgs[vs[0]].TryAdd(vs[2], path);
                            }
                        }
                    }
                    #endregion

                    foreach (var imgName in imgs.Keys)
                    {
                        if (imgs[imgName].TryGetValue("on", out var pOn) && imgs[imgName].TryGetValue("off", out var pOff))
                        {
                            imgOn = Util.FromBitmap(pOn);
                            imgOff = Util.FromBitmap(pOff);
                        }
                    }
                }
            }
            catch { }
        }
        #endregion
    }
}
