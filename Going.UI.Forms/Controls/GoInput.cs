using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Going.UI.Themes;
using Going.UI.Forms.Input;
using Going.UI.Managers;
using Going.UI.Controls;
using System.Windows.Forms;
using Going.UI.Forms.Dialogs;
using Going.UI.Tools;
using SkiaSharp;

namespace Going.UI.Forms.Controls
{
    public class GoInputString : GoControl
    {
        #region Properties
        public string? IconString { get => control.IconString; set { if (control.IconString != value) { control.IconString = value; Invalidate(); } } }
        public float IconSize { get => control.IconSize; set { if (control.IconSize != value) { control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => control.IconGap; set { if (control.IconGap != value) { control.IconGap = value; Invalidate(); } } }

        public string FontName { get => control.FontName; set { if (control.FontName != value) { control.FontName = value; Invalidate(); } } }
        public float FontSize { get => control.FontSize; set { if (control.FontSize != value) { control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => control.Direction; set { if (control.Direction != value) { control.Direction = value; Invalidate(); } } }

        public string TextColor { get => control.TextColor; set { if (control.TextColor != value) { control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => control.BorderColor; set { if (control.BorderColor != value) { control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => control.FillColor; set { if (control.FillColor != value) { control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => control.ValueColor; set { if (control.ValueColor != value) { control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => control.Round; set { if (control.Round != value) { control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => control.TitleSize; set { if (control.TitleSize != value) { control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => control.Title; set { if (control.Title != value) { control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonInfo> Buttons { get => control.Buttons; set { if (control.Buttons != value) { control.Buttons = value; Invalidate(); } } } 
        public float? ButtonSize { get => control.ButtonSize; set { if (control.ButtonSize != value) { control.ButtonSize = value; Invalidate(); } } }

        public string Value { get => control.Value; set { if (control.Value != value) { control.Value = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        Going.UI.Controls.GoInputString control = new Going.UI.Controls.GoInputString();
        #endregion

        #region Event
        public event EventHandler ValueChanged { add => control.ValueChanged += value; remove => control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoInputString()
        {
            GoInputEventer.Current.InputString += (c, bounds, callback, value) =>
            {
                if (c == control)
                    FormsInputManager.Current.InputString(this, control, bounds, FontName, FontSize, ValueColor, TextColor, callback, value);
            };
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
            control.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            control.Visible = Visible;
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }

    public class GoInputNumber<T> : GoControl where T : struct
    {
        #region Properties
        public string? IconString { get => control.IconString; set { if (control.IconString != value) { control.IconString = value; Invalidate(); } } }
        public float IconSize { get => control.IconSize; set { if (control.IconSize != value) { control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => control.IconGap; set { if (control.IconGap != value) { control.IconGap = value; Invalidate(); } } }

        public string FontName { get => control.FontName; set { if (control.FontName != value) { control.FontName = value; Invalidate(); } } }
        public float FontSize { get => control.FontSize; set { if (control.FontSize != value) { control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => control.Direction; set { if (control.Direction != value) { control.Direction = value; Invalidate(); } } }

        public string TextColor { get => control.TextColor; set { if (control.TextColor != value) { control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => control.BorderColor; set { if (control.BorderColor != value) { control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => control.FillColor; set { if (control.FillColor != value) { control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => control.ValueColor; set { if (control.ValueColor != value) { control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => control.Round; set { if (control.Round != value) { control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => control.TitleSize; set { if (control.TitleSize != value) { control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => control.Title; set { if (control.Title != value) { control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonInfo> Buttons { get => control.Buttons; set { if (control.Buttons != value) { control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => control.ButtonSize; set { if (control.ButtonSize != value) { control.ButtonSize = value; Invalidate(); } } }

        public T Value { get => control.Value; set { if (!control.Value.Equals(value)) { control.Value = value; Invalidate(); } } }
        public T? Minimum { get => control.Minimum; set => control.Minimum = value; }
        public T? Maximum { get => control.Maximum; set => control.Maximum = value; }
        public string? FormatString { get => control.FormatString; set { if (control.FormatString != value) { control.FormatString = value; Invalidate(); } } }
        public string? Unit { get => control.Unit; set { if (control.Unit != value) { control.Unit = value; Invalidate(); } } }
        public float? UnitSize { get => control.UnitSize; set { if (control.UnitSize != value) { control.UnitSize = value; Invalidate(); } } }
        #endregion
        #region Member Variable
        Going.UI.Controls.GoInputNumber<T> control = new Going.UI.Controls.GoInputNumber<T>();
        #endregion

        #region Event
        public event EventHandler ValueChanged { add => control.ValueChanged += value; remove => control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoInputNumber()
        {
            GoInputEventer.Current.InputNumber += (c, bounds, callback, type, value, min, max) =>
            {
                if (c == control)
                {
                    FormsInputManager.Current.InputNumber<T>(this, control, bounds, FontName, FontSize, ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, type, value, min, max);
                }
            };
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
            control.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            control.Visible = Visible;
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }

    public class GoInputInteger : GoInputNumber<int> { }
    public class GoInputFloat: GoInputNumber<double> { }

    public class GoInputBoolean : GoControl
    {
        #region Properties
        public string? IconString { get => control.IconString; set { if (control.IconString != value) { control.IconString = value; Invalidate(); } } }
        public float IconSize { get => control.IconSize; set { if (control.IconSize != value) { control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => control.IconGap; set { if (control.IconGap != value) { control.IconGap = value; Invalidate(); } } }

        public string FontName { get => control.FontName; set { if (control.FontName != value) { control.FontName = value; Invalidate(); } } }
        public float FontSize { get => control.FontSize; set { if (control.FontSize != value) { control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => control.Direction; set { if (control.Direction != value) { control.Direction = value; Invalidate(); } } }

        public string TextColor { get => control.TextColor; set { if (control.TextColor != value) { control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => control.BorderColor; set { if (control.BorderColor != value) { control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => control.FillColor; set { if (control.FillColor != value) { control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => control.ValueColor; set { if (control.ValueColor != value) { control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => control.Round; set { if (control.Round != value) { control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => control.TitleSize; set { if (control.TitleSize != value) { control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => control.Title; set { if (control.Title != value) { control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonInfo> Buttons { get => control.Buttons; set { if (control.Buttons != value) { control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => control.ButtonSize; set { if (control.ButtonSize != value) { control.ButtonSize = value; Invalidate(); } } }

        public bool Value { get => control.Value; set { if (control.Value != value) { control.Value = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        Going.UI.Controls.GoInputBoolean control = new Going.UI.Controls.GoInputBoolean();
        #endregion

        #region Event
        public event EventHandler ValueChanged { add => control.ValueChanged += value; remove => control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoInputBoolean()
        {
           
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
            control.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            control.Visible = Visible;
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }

    public class GoInputCombo : GoControl
    {
        #region Properties
        public string? IconString { get => control.IconString; set { if (control.IconString != value) { control.IconString = value; Invalidate(); } } }
        public float IconSize { get => control.IconSize; set { if (control.IconSize != value) { control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => control.IconGap; set { if (control.IconGap != value) { control.IconGap = value; Invalidate(); } } }

        public string FontName { get => control.FontName; set { if (control.FontName != value) { control.FontName = value; Invalidate(); } } }
        public float FontSize { get => control.FontSize; set { if (control.FontSize != value) { control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => control.Direction; set { if (control.Direction != value) { control.Direction = value; Invalidate(); } } }

        public string TextColor { get => control.TextColor; set { if (control.TextColor != value) { control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => control.BorderColor; set { if (control.BorderColor != value) { control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => control.FillColor; set { if (control.FillColor != value) { control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => control.ValueColor; set { if (control.ValueColor != value) { control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => control.Round; set { if (control.Round != value) { control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => control.TitleSize; set { if (control.TitleSize != value) { control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => control.Title; set { if (control.Title != value) { control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonInfo> Buttons { get => control.Buttons; set { if (control.Buttons != value) { control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => control.ButtonSize; set { if (control.ButtonSize != value) { control.ButtonSize = value; Invalidate(); } } }

        public List<GoListItem> Items { get => control.Items; set { if (control.Items != value) { control.Items = value; Invalidate(); } } }
        public int SelectedIndex { get => control.SelectedIndex; set { if (control.SelectedIndex != value) { control.SelectedIndex = value; Invalidate(); } } }
        public int MaximumViewCount { get => control.MaximumViewCount; set { if (control.MaximumViewCount != value) { control.MaximumViewCount = value; Invalidate(); } } }
        public int ItemHeight { get => control.ItemHeight; set { if (control.ItemHeight != value) { control.ItemHeight = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        Going.UI.Controls.GoInputCombo control = new Going.UI.Controls.GoInputCombo();
        #endregion

        #region Event
        public event EventHandler SelectedIndexChanged { add => control.SelectedIndexChanged += value; remove => control.SelectedIndexChanged -= value; }
        #endregion

        #region Constructor
        public GoInputCombo()
        {
            control.DropDownOpening += (o, s) =>
            {
                s.Cancel = true;

                OpenDropDown(control.Areas()["Value"]);
            };
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
            control.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            control.Visible = Visible;
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion

        #region DropDown
        GoComboBoxDropDownWindow? dwnd;

        void Opened()
        {
            if(dwnd != null)
            {
                var sel = SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;
                dwnd.Set(FontName, FontSize, ItemHeight, MaximumViewCount, Items, sel, (item) =>
                {
                    if (item != null)
                        SelectedIndex = Items.IndexOf(item);
                });
            }
        }

        #region var
        bool closedWhileInControl;

        DropWindowState DropState { get; set; }
        bool CanDrop
        {
            get
            {
                if (dwnd != null) return false;
                if (dwnd == null && closedWhileInControl)
                {
                    closedWhileInControl = false;
                    return false;
                }

                return !closedWhileInControl;
            }
        }
        #endregion
        #region Method
        #region Freeze
        internal void FreezeDropDown(bool remainVisible)
        {
            if (dwnd != null)
            {
                dwnd.Freeze = true;
                if (!remainVisible) dwnd.Visible = false;
            }
        }
        
        internal void UnFreezeDropDown()
        {
            if (dwnd != null)
            {
                dwnd.Freeze = false;
                if (!dwnd.Visible) dwnd.Visible = true;
            }
        }
        #endregion
        #region Open
        private void OpenDropDown(SKRect rtValue)
        {
            this.Move += (o, s) => { if (dwnd != null) dwnd.Bounds = GetDropDownBounds(rtValue); };

            dwnd = new ();
            dwnd.Bounds = GetDropDownBounds(rtValue);
            dwnd.DropStateChanged += (o, s) => { DropState = s.DropState; };
            dwnd.FormClosed += (o, s) =>
            {
                if (dwnd != null && !dwnd.IsDisposed) dwnd.Dispose();
                dwnd = null;
                closedWhileInControl = (this.RectangleToScreen(this.ClientRectangle).Contains(Cursor.Position));
                DropState = DropWindowState.Closed;
                this.Invalidate();
            };
            
            Opened();

            DropState = DropWindowState.Dropping;
            dwnd.Show(this);
            DropState = DropWindowState.Dropped;
            this.Invalidate();
        }
        #endregion
        #region Close
        public void CloseDropDown()
        {
            if (dwnd != null)
            {
                DropState = DropWindowState.Closing;
                dwnd.Freeze = false;
                dwnd.Close();
            }
        }
        #endregion
        #region Bounds
        private Rectangle GetDropDownBounds(SKRect rtValue)
        {
            int n = Items.Count;
            Point pt = PointToScreen(new Point(Convert.ToInt32(rtValue.Left), Convert.ToInt32(rtValue.Bottom)));
            if (MaximumViewCount != -1) n = Items.Count > MaximumViewCount ? MaximumViewCount : Items.Count;
            Size inflatedDropSize = new Size(this.Width, n * ItemHeight + 2);
            Rectangle screenBounds = new Rectangle(pt, inflatedDropSize);
            Rectangle workingArea = Screen.GetWorkingArea(screenBounds);

            if (screenBounds.X < workingArea.X) screenBounds.X = workingArea.X;
            if (screenBounds.Y < workingArea.Y) screenBounds.Y = workingArea.Y;

            if (screenBounds.Right > workingArea.Right && workingArea.Width > screenBounds.Width) screenBounds.X = workingArea.Right - screenBounds.Width;
            if (screenBounds.Bottom > workingArea.Bottom && workingArea.Height > screenBounds.Height) screenBounds.Y = pt.Y - this.Height - screenBounds.Height;
            return screenBounds;
        }
        #endregion
        #endregion
        #endregion
    }
}
