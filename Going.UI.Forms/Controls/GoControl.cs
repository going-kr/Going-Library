using Going.UI.Enums;
using Going.UI.Forms.Tools;
using Going.UI.Themes;
using Going.UI.Utils;
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
            var canvas = e.Surface.Canvas;
            var cBack = GoTheme.Current.ToColor(BackgroundColor);
            canvas.Clear(ColorTool.EnableColor(this, cBack));

            using var p = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
            var sp = canvas.SaveLayer(p);

            var args = new ContentDrawEventArgs(canvas);
            OnContentDraw(args);
            ContentDraw?.Invoke(this, args);

            canvas.RestoreToCount(sp);

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

    public class GoWrapperControl<T> : GoControl where T : Going.UI.Controls.GoControl
    {
        #region Properties
        protected T Control { get; set; }
        #endregion

        #region Constructor
        public GoWrapperControl()
        {
            Control = Activator.CreateInstance<T>();
            Control.SetInvalidate(Invalidate);
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            Control.Bounds = Util.FromRect(0, 0, Width, Height);
            Control.FireDraw(e.Canvas);
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Select();
            Control.FireMouseDown(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            Control.FireMouseUp(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Control.FireMouseMove(e.X, e.Y);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            Control.FireMouseDoubleClick(e.X, e.Y, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Control.FireMouseWheel(e.X, e.Y, e.Delta / 120F);
            Invalidate();
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            Control.FireMouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            Control.FireMouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion

        #region OnKeyDown
        protected override void OnKeyDown(KeyEventArgs e)
        {
            Control.FireKeyDown(e.Shift, e.Control, e.Alt, (GoKeys)e.KeyCode);
            base.OnKeyDown(e);
        }
        #endregion
        #region OnKeyUp
        protected override void OnKeyUp(KeyEventArgs e)
        {
            Control.FireKeyDown(e.Shift, e.Control, e.Alt, (GoKeys)e.KeyCode);
            base.OnKeyDown(e);
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            //Control.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            //Control.Visible = Visible;
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }

    public class ContentDrawEventArgs(SKCanvas Canvas)
    {
        public SKCanvas Canvas { get; } = Canvas;
    }
}


