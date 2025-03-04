using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Forms.Controls;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Dialogs
{
    // DropDownWindow 이거 상속받으면됨
    public class GoDropDownWindow : Form, IMessageFilter
    {
        #region Properties
        internal bool Freeze { get; set; }

        public string WindowColor { get; set; } = "Window";
        public string BorderColor { get; set; } = "WindowBorder";
        public GoRoundType Round { get; set; } = GoRoundType.All;
        #endregion

        #region Event
        internal event EventHandler<DropWindowEventArgs>? DropStateChanged;
        #endregion

        #region Constructor
        public GoDropDownWindow()
        {
            #region Init
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.ControlBox = false;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.AutoSize = false;
            this.AutoScroll = false;
            this.MinimumSize = new Size(10, 10);
            this.Padding = new Padding(0, 0, 0, 0);

            Application.AddMessageFilter(this);
            #endregion
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
                var cBack = GoTheme.Current.ToColor(WindowColor);
                var cBorder = GoTheme.Current.ToColor(BorderColor);
                canvas.Clear(cBack);

                using var p = new SKPaint { IsAntialias = false };
                p.IsStroke = true;
                p.StrokeWidth = 1;
                p.Color = cBorder;
                canvas.DrawRect(Util.FromRect(0, 0, Width - 1, Height - 1), p);

                using var p2 = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                var sp = canvas.SaveLayer(p2);

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

        #region Implements
        #region PreFilterMessage
        public bool PreFilterMessage(ref Message m)
        {
            if (!Freeze && this.Visible && (Form.ActiveForm == null || !Form.ActiveForm.Equals(this)))
            {
                if (DropStateChanged != null) DropStateChanged.Invoke(this, new DropWindowEventArgs(DropWindowState.Closing));
                this.Close();
            }
            return false;
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

    public class GoComboBoxDropDownWindow : GoDropDownWindow
    {
        #region Member Variable
        private Going.UI.Controls.GoListBox control;
        private Action<GoListItem?>? feedback;
        #endregion

        #region Constructor
        public GoComboBoxDropDownWindow()
        {
            control = new Going.UI.Controls.GoListBox { BackgroundDraw = false };

            control.ItemClicked += (o, s) =>
            {
                feedback?.Invoke(s.Item);
                Hide();
            };

            control.SetInvalidate(Invalidate);
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            control.Bounds = Util.FromRect(0, 0, Width, Height);
            control.FireDraw(e.Canvas);
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Select();
            control.FireMouseDown(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            control.FireMouseUp(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            control.FireMouseMove(e.X, e.Y);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            control.FireMouseDoubleClick(e.X, e.Y, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            control.FireMouseWheel(e.X, e.Y, e.Delta / 120F);
            Invalidate();
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            control.FireMouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            control.FireMouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion

        #region Set
        public void Set(string fontName, GoFontStyle fontStyle, float fontSize, float itemHeight, int maximumViewCount,
                      List<GoListItem> items, GoListItem? selectedItem,
                      Action<GoListItem?> result)
        {
            feedback = result;

            control.FontName = fontName;
            control.FontStyle = fontStyle;
            control.FontSize = fontSize;
            control.IconSize = fontSize + 2;
            control.IconGap = 3;
            control.ItemHeight = itemHeight;

            control.Items.Clear();
            control.SelectedItems.Clear();
            control.Items.AddRange(items);

            if (selectedItem != null)
            {
                control.SelectedItems.Add(selectedItem);
                control.ScrollPosition = control.Items.IndexOf(selectedItem) * itemHeight;
            }
        }
        #endregion
    }


    #region DropWindowEventArgs
    internal enum DropWindowState { Closed, Closing, Dropping, Dropped }
    internal class DropWindowEventArgs : EventArgs
    {
        internal DropWindowState DropState { get; private set; }
        public DropWindowEventArgs(DropWindowState DropState)
        {
            this.DropState = DropState;
        }
    }
    #endregion

}
