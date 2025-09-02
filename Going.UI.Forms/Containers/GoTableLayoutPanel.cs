using Going.UI.Enums;
using Going.UI.Forms.Controls;
using Going.UI.Forms.Tools;
using Going.UI.Themes;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Containers
{
    public class GoTableLayoutPanel : TableLayoutPanel
    {
        #region Properties
        private string sBackgroundColor = "Back";
        public string BackgroundColor
        {
            get => sBackgroundColor;
            set
            {
                if (sBackgroundColor != value)
                {
                    sBackgroundColor = value;

                    var c = GoThemeW.Current.ToColor(BackgroundColor);
                    this.BackColor = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);

                    Invalidate();
                }
            }
        }
        #endregion

        #region Event
        public event EventHandler<ContentDrawEventArgs>? ContentDraw;
        #endregion

        #region Constructor
        public GoTableLayoutPanel()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, false);
            this.UpdateStyles();

            DoubleBuffered = true;
            ResizeRedraw = true;

            this.TabStop = false;

            var c = GoThemeW.Current.ToColor(BackgroundColor); ;
            this.BackColor = Color.FromArgb(c.Red, c.Green, c.Blue);
        }
        #endregion

        #region Override
        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        #endregion
        #region OnPaintSurface
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var bitmap = new SKBitmap(this.Width, this.Height))
            using (var canvas = new SKCanvas(bitmap))
            using (var surface = SKSurface.Create(bitmap.Info))
            {
                var cBack = GoThemeW.Current.ToColor(BackgroundColor);
                canvas.Clear(ColorTool.EnableColor(this, cBack));

                using var p = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                var sp = canvas.SaveLayer(p);

                OnContentDraw(new ContentDrawEventArgs(canvas));

                canvas.RestoreToCount(sp);

                using (var bmp = bitmap.ToBitmap())
                    e.Graphics.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
            }
        }
        #endregion
        #region OnContentDraw
        protected virtual void OnContentDraw(ContentDrawEventArgs e) { }
        #endregion
        #region OnGotFocus
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }
        #endregion
        #region OnLostFocus
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }
        #endregion
        #endregion

        #region Method
        #region ToGoMouseButton
        protected GoMouseButton ToGoMouseButton(MouseButtons btn)
        {
            GoMouseButton ret = GoMouseButton.Left;
            if (btn == MouseButtons.Left) ret = GoMouseButton.Left;
            else if (btn == MouseButtons.Right) ret = GoMouseButton.Right;
            else if (btn == MouseButtons.Middle) ret = GoMouseButton.Middle;
            return ret;
        }
        #endregion
        #endregion
    }
}
