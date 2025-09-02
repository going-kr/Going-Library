using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Forms.Controls;
using Going.UI.Forms.Input;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenTK.Graphics.OpenGL.GL;

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
                var cBack = GoThemeW.Current.ToColor(WindowColor);
                var cBorder = GoThemeW.Current.ToColor(BorderColor);
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
            control = new Going.UI.Controls.GoListBox { BackgroundDraw = false, Round = GoRoundType.Rect };

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
            control.FireDraw(e.Canvas, GoThemeW.Current);
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

    public class GoColorDropDownWindow : GoDropDownWindow
    {
        #region Member Variable
        private Going.UI.Controls.GoColorSelector color;
        private Going.UI.Controls.GoButton btnOK;
        private Going.UI.Controls.GoButton btnCancel;
        private Action<SKColor?>? feedback;

        private IGoControl[] cs => [color, btnOK, btnCancel];

        private WFInputManager IM;
        #endregion

        #region Constructor
        public GoColorDropDownWindow()
        {
            color = new Going.UI.Controls.GoColorSelector { };

            IM = new WFInputManager(this);
            GoInputEventer.Current.InputNumber += (c, bounds, callback, type, value, min, max) =>
            {
                if (c == color)
                {
                    var rt = bounds;
                    rt.Offset(color.Left, color.Top);
                    IM.InputNumber<byte>(color, rt, color.FontName, color.FontStyle, color.FontSize, color.InputColor, color.TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                }
            };

            btnOK = new UI.Controls.GoButton { Text = "선택" };
            btnCancel = new UI.Controls.GoButton { Text = "취소" };

            btnOK.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(color.Value);
                Hide();
            };

            btnCancel.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(null);
                Hide();
            };

            color.SetInvalidate(Invalidate);
            btnOK.SetInvalidate(Invalidate);
            btnCancel.SetInvalidate(Invalidate);
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            var rts = Util.Grid(Util.FromRect(Util.FromRect(0, 0, Width, Height), new GoPadding(5)), ["100%", "80px", "80px"], ["100%", "40px"]);

            color.Bounds = Util.FromRect( Util.Merge(rts, 0, 0, 3, 1), new GoPadding(5));
            btnOK.Bounds = Util.FromRect(Util.Merge(rts, 1, 1, 1, 1), new GoPadding(5));
            btnCancel.Bounds = Util.FromRect(Util.Merge(rts, 2, 1, 1, 1), new GoPadding(5));

            foreach (IGoControl control in cs)
            {
                using (new SKAutoCanvasRestore(e.Canvas))
                {
                    e.Canvas.Translate(control.Left, control.Top);
                    control.FireDraw(e.Canvas, GoThemeW.Current);
                }
            }
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Select();
            foreach (IGoControl control in cs) control.FireMouseDown(e.X - control.Left, e.Y - control.Top, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseUp(e.X - control.Left, e.Y - control.Top, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseMove(e.X - control.Left, e.Y - control.Top);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseDoubleClick(e.X - control.Left, e.Y - control.Top, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseWheel(e.X - control.Left, e.Y - control.Top, e.Delta / 120F);
            Invalidate();
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseMove(-1, -1);
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

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            IM.ClearInput();
            IM.Dispose();
            base.Dispose(disposing);
        }
        #endregion
        #endregion

        #region Set
        public void Set(string fontName, GoFontStyle fontStyle, float fontSize, SKColor value, Action<SKColor?> result)
        {
            feedback = result;

            btnOK.FontName = btnCancel.FontName = color.FontName = fontName;
            btnOK.FontStyle = btnCancel.FontStyle = color.FontStyle = fontStyle;
            btnOK.FontSize = btnCancel.FontSize = color.FontSize = fontSize;
            color.Value = value;
        }
        #endregion

        #region spkey
        void spkey(Keys key, Keys modifier)
        {

        }
        #endregion
    }

    public class GoDateTimeDropDownWindow : GoDropDownWindow
    {
        #region Member Variable
        private Going.UI.Controls.GoCalendar cal;
        private Going.UI.Controls.GoButton btnOK;
        private Going.UI.Controls.GoButton btnCancel;
        private Going.UI.Controls.GoInputNumber<int> inH, inM, inS;
        private Action<DateTime?>? feedback;
        private GoDateTimeKind style = GoDateTimeKind.DateTime;

        private IGoControl[] cs
        {
            get
            {
                if (style == GoDateTimeKind.Date) return [cal, btnOK, btnCancel];
                else if(style == GoDateTimeKind.Time) return [btnOK, btnCancel, inH, inM, inS];
                else return [cal, btnOK, btnCancel, inH, inM, inS];
            }
        }

        private WFInputManager IM;
        #endregion

        #region Constructor
        public GoDateTimeDropDownWindow()
        {
            IM = new WFInputManager(this);

            GoInputEventer.Current.InputNumber += (c, bounds, callback, type, value, min, max) =>
            {
                if ((c == inH || c == inM || c == inS) && c is Going.UI.Controls.GoInputNumber<int> vc)
                {
                    var rt = bounds;
                    rt.Offset(c.Left, c.Top);
                    IM.InputNumber<byte>(c, rt, vc.FontName, vc.FontStyle, vc.FontSize, vc.ValueColor, vc.TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                }
            };

            cal = new Going.UI.Controls.GoCalendar { BackgroundDraw = false, MultiSelect = false };
            btnOK = new UI.Controls.GoButton { Text = "선택" };
            btnCancel = new UI.Controls.GoButton { Text = "취소" };
            inH = new UI.Controls.GoInputNumber<int> { Minimum = 0, Maximum = 23, UnitSize = 30, Unit = "시" };
            inM = new UI.Controls.GoInputNumber<int> { Minimum = 0, Maximum = 59, UnitSize = 30, Unit = "분" };
            inS = new UI.Controls.GoInputNumber<int> { Minimum = 0, Maximum = 59, UnitSize = 30, Unit = "초" };

            btnOK.ButtonClicked += (o, s) =>
            {
                if (cal.SelectedDays.Count > 0)
                {
                    var dt = cal.SelectedDays.First().Date + new TimeSpan(inH.Value, inM.Value, inS.Value);

                    feedback?.Invoke(dt);
                    Hide();
                }
            };

            btnCancel.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(null);
                Hide();
            };

            cal.SetInvalidate(Invalidate);
            inH.SetInvalidate(Invalidate);
            inM.SetInvalidate(Invalidate);
            inS.SetInvalidate(Invalidate);
            btnOK.SetInvalidate(Invalidate);
            btnCancel.SetInvalidate(Invalidate);
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            SKRect[,] rts;

            if (style == GoDateTimeKind.Date)
            {
                rts = Util.Grid(Util.FromRect(Util.FromRect(0, 0, Width, Height), new GoPadding(5)), ["33.34%", "33.33%", "33.33%"], ["100%", "40px"]);

                cal.Bounds = Util.FromRect(Util.Merge(rts, 0, 0, 3, 1), new GoPadding(5));
                btnOK.Bounds = Util.FromRect(Util.Merge(rts, 1, 1, 1, 1), new GoPadding(5));
                btnCancel.Bounds = Util.FromRect(Util.Merge(rts, 2, 1, 1, 1), new GoPadding(5));
            }
            else if (style == GoDateTimeKind.Time)
            {
                rts = Util.Grid(Util.FromRect(Util.FromRect(0, 0, Width, Height), new GoPadding(5)), ["33.34%", "33.33%", "33.33%"], ["40px", "40px"]);

                inH.Bounds = Util.FromRect(Util.Merge(rts, 0, 0, 1, 1), new GoPadding(5));
                inM.Bounds = Util.FromRect(Util.Merge(rts, 1, 0, 1, 1), new GoPadding(5));
                inS.Bounds = Util.FromRect(Util.Merge(rts, 2, 0, 1, 1), new GoPadding(5));
                btnOK.Bounds = Util.FromRect(Util.Merge(rts, 1, 1, 1, 1), new GoPadding(5));
                btnCancel.Bounds = Util.FromRect(Util.Merge(rts, 2, 1, 1, 1), new GoPadding(5));
            }
            else
            {
                rts = Util.Grid(Util.FromRect(Util.FromRect(0, 0, Width, Height), new GoPadding(5)), ["33.34%", "33.33%", "33.33%"], ["100%", "40px", "40px"]);

                cal.Bounds = Util.FromRect(Util.Merge(rts, 0, 0, 3, 1), new GoPadding(5));
                inH.Bounds = Util.FromRect(Util.Merge(rts, 0, 1, 1, 1), new GoPadding(5));
                inM.Bounds = Util.FromRect(Util.Merge(rts, 1, 1, 1, 1), new GoPadding(5));
                inS.Bounds = Util.FromRect(Util.Merge(rts, 2, 1, 1, 1), new GoPadding(5));
                btnOK.Bounds = Util.FromRect(Util.Merge(rts, 1, 2, 1, 1), new GoPadding(5));
                btnCancel.Bounds = Util.FromRect(Util.Merge(rts, 2, 2, 1, 1), new GoPadding(5));
            }
                       
            foreach (IGoControl control in cs)
            {
                using (new SKAutoCanvasRestore(e.Canvas))
                {
                    e.Canvas.Translate(control.Left, control.Top);
                    control.FireDraw(e.Canvas, GoThemeW.Current);
                }
            }
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Select();
            foreach (IGoControl control in cs) control.FireMouseDown(e.X - control.Left, e.Y - control.Top, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseUp(e.X - control.Left, e.Y - control.Top, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseMove(e.X - control.Left, e.Y - control.Top);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseDoubleClick(e.X - control.Left, e.Y - control.Top, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseWheel(e.X - control.Left, e.Y - control.Top, e.Delta / 120F);
            Invalidate();
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            foreach (IGoControl control in cs) control.FireMouseMove(-1, -1);
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

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            IM.ClearInput();
            IM.Dispose();
            base.Dispose(disposing);
        }
        #endregion
        #endregion

        #region Set
        public void Set(string fontName, GoFontStyle fontStyle, float fontSize, DateTime value, GoDateTimeKind style, Action<DateTime?> result)
        {
            feedback = result;
            this.style = style;

            #region set
            IGoControl[] cls = [cal, inH, inM, inS, btnOK, btnCancel];
            foreach (var v in cls)
            {
                if (v is UI.Controls.GoLabel c1) { c1.FontName = fontName; c1.FontSize = fontSize; c1.FontStyle = fontStyle; }
                else if (v is UI.Controls.GoInputNumber<int> c2) { c2.FontName = fontName; c2.FontSize = fontSize; c2.FontStyle = fontStyle; }
                else if (v is UI.Controls.GoCalendar c3) { c3.FontName = fontName; c3.FontSize = fontSize; c3.FontStyle = fontStyle; }
                else if (v is UI.Controls.GoButton c4) { c4.FontName = fontName; c4.FontSize = fontSize; c4.FontStyle = fontStyle; }
            }

            cal.SelectedDays.Clear();
            cal.SelectedDays.Add(value.Date);
            inH.Value = style != GoDateTimeKind.Date ? value.Hour : 0;
            inM.Value = style != GoDateTimeKind.Date ? value.Minute : 0;
            inS.Value = style != GoDateTimeKind.Date ? value.Second : 0;
            #endregion
        }
        #endregion

        #region spkey
        void spkey(Keys key, Keys modifier)
        {

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
