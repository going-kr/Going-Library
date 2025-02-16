using Going.UI.Enums;
using Going.UI.Themes;
using OpenTK.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UI.Forms.Controls
{
    public class GoControl : SKControl
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

                    var c = GoTheme.Current.ToColor(BackgroundColor);
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
        public GoControl()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, false);
            this.UpdateStyles();

            DoubleBuffered = true;
            ResizeRedraw = true;

            this.TabStop = false;

            var c = GoTheme.Current.ToColor(BackgroundColor); ;
            this.BackColor = Color.FromArgb(c.Red, c.Green, c.Blue);
        }
        #endregion

        #region Override
        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        #endregion
        #region OnPaintSurface
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var cBack = GoTheme.Current.ToColor(BackgroundColor);
            e.Surface.Canvas.Clear(cBack);

            OnContentDraw(new ContentDrawEventArgs(e.Surface.Canvas));

            base.OnPaintSurface(e);
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

    public class ContentDrawEventArgs(SKCanvas Canvas)
    {
        public SKCanvas Canvas { get; } = Canvas;
    }
}


