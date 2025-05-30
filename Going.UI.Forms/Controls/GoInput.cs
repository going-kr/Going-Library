﻿using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Dialogs;
using Going.UI.Forms.Input;
using Going.UI.Managers;
using Going.UI.Tools;
using SkiaSharp;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Going.UI.Forms.Controls
{
    public class GoInputString : GoWrapperControl<Going.UI.Controls.GoInputString>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } } 
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        public string Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Member Variable
        WFInputManager IM;
        #endregion

        #region Constructor
        public GoInputString()
        {
            IM = new WFInputManager(this);
            GoInputEventer.Current.InputString += (c, bounds, callback, value) =>
            {
                if (c == Control)
                    IM.InputString(Control, bounds, FontName, FontStyle, FontSize, ValueColor, TextColor, callback, null, value);
            };
        }
        #endregion

        #region Method
        public void Input()
        {
            var rt = Control.Areas()["Value"];
            Control.FireMouseDown(rt.Left + 1, rt.Top + 1, GoMouseButton.Left);
            Control.FireMouseUp(rt.Left + 1, rt.Top + 1, GoMouseButton.Left);
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
    }

    public class GoInputNumber<T> : GoWrapperControl<Going.UI.Controls.GoInputNumber<T>> where T : struct
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        public T Value { get => Control.Value; set { if (!Control.Value.Equals(value)) { Control.Value = value; Invalidate(); } } }
        public T? Minimum { get => Control.Minimum; set => Control.Minimum = value; }
        public T? Maximum { get => Control.Maximum; set => Control.Maximum = value; }
        public string? FormatString { get => Control.FormatString; set { if (Control.FormatString != value) { Control.FormatString = value; Invalidate(); } } }
        public string? Unit { get => Control.Unit; set { if (Control.Unit != value) { Control.Unit = value; Invalidate(); } } }
        public float? UnitSize { get => Control.UnitSize; set { if (Control.UnitSize != value) { Control.UnitSize = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Member Variable
        WFInputManager IM;
        #endregion

        #region Constructor
        public GoInputNumber()
        {
            IM = new WFInputManager(this);

            GoInputEventer.Current.InputNumber += (c, bounds, callback, type, value, min, max) =>
            {
                if (c == Control)
                {
                    IM.InputNumber<T>(Control, bounds, FontName, FontStyle, FontSize, ValueColor, TextColor,
                                                                 (v) => { callback(v); Invalidate(); }, null, type, value, min, max);
                }
            };
        }
        #endregion

        #region Method
        public void Input()
        {
            var rt = Control.Areas()["Value"];
            Control.FireMouseDown(rt.Left + 1, rt.Top + 1, GoMouseButton.Left);
            Control.FireMouseUp(rt.Left + 1, rt.Top + 1, GoMouseButton.Left);
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
    }

    public class GoInputInteger : GoInputNumber<int> { }
    public class GoInputFloat: GoInputNumber<double> { }

    public class GoInputBoolean : GoWrapperControl<Going.UI.Controls.GoInputBoolean>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        public bool Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }

        public string? OnText { get => Control.OnText; set { if (Control.OnText != value) { Control.OnText = value; Invalidate(); } } }
        public string? OffText { get => Control.OffText; set { if (Control.OffText != value) { Control.OffText = value; Invalidate(); } } }
        public string? OnIconString { get => Control.OnIconString; set { if (Control.OnIconString != value) { Control.OnIconString = value; Invalidate(); } } }
        public string? OffIconString { get => Control.OffIconString; set { if (Control.OffIconString != value) { Control.OffIconString = value; Invalidate(); } } }

        #endregion

        #region Event
        public event EventHandler ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoInputBoolean()
        {
           
        }
        #endregion
    }

    public class GoInputCombo : GoWrapperControl<Going.UI.Controls.GoInputCombo>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoListItem> Items { get => Control.Items; set { if (Control.Items != value) { Control.Items = value; Invalidate(); } } }
        public int SelectedIndex { get => Control.SelectedIndex; set { if (Control.SelectedIndex != value) { Control.SelectedIndex = value; Invalidate(); } } }
        public GoListItem? SelectedItem => Control.SelectedItem;

        public int MaximumViewCount { get => Control.MaximumViewCount; set { if (Control.MaximumViewCount != value) { Control.MaximumViewCount = value; Invalidate(); } } }
        public int ItemHeight { get => Control.ItemHeight; set { if (Control.ItemHeight != value) { Control.ItemHeight = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler SelectedIndexChanged { add => Control.SelectedIndexChanged += value; remove => Control.SelectedIndexChanged -= value; }
        #endregion

        #region Constructor
        public GoInputCombo()
        {
            Control.DropDownOpening += (o, s) =>
            {
                s.Cancel = true;

                OpenDropDown(Control.Areas()["Value"]);
            };
        }
        #endregion

        #region DropDown
        // var, Method를 건드리지 말고 GoComboBoxDropDownWindow를 사용해서 드롭박스를 만들어 보라고 이렇게 따로 설정
        GoComboBoxDropDownWindow? dwnd;

        void Opened()
        {
            if(dwnd != null)
            {
                var sel = SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;
                dwnd.Set(FontName, FontStyle, FontSize, ItemHeight, MaximumViewCount, Items, sel, (item) =>
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
            Size inflatedDropSize = new Size(Convert.ToInt32(rtValue.Width), n * ItemHeight + 2);
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

    public class GoInputSelector : GoWrapperControl<Going.UI.Controls.GoInputSelector>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoListItem> Items { get => Control.Items; set { if (Control.Items != value) { Control.Items = value; Invalidate(); } } }
        public int SelectedIndex { get => Control.SelectedIndex; set { if (Control.SelectedIndex != value) { Control.SelectedIndex = value; Invalidate(); } } }
        public GoListItem? SelectedItem => Control.SelectedItem;
        #endregion

        #region Event
        public event EventHandler SelectedIndexChanged { add => Control.SelectedIndexChanged += value; remove => Control.SelectedIndexChanged -= value; }
        #endregion

        #region Constructor
        public GoInputSelector()
        {
            
        }
        #endregion
    }

    public class GoInputColor : GoWrapperControl<Going.UI.Controls.GoInputColor>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        public SKColor Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoInputColor()
        {
            Control.DropDownOpening += (o, s) =>
            {
                s.Cancel = true;

                OpenDropDown(Control.Areas()["Value"]);
            };
        }
        #endregion

        #region DropDown
        GoColorDropDownWindow? dwnd;

        void Opened()
        {
            if (dwnd != null)
            {
                dwnd.Set(FontName, FontStyle, FontSize, Value, (color) =>
                {
                    if (color.HasValue)
                        Value = color.Value;
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

            dwnd = new();
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
            var w = Convert.ToInt32(MathTool.Constrain(rtValue.Width, 280, 360));
            var h = w + 40;

            Point pt = PointToScreen(new Point(Convert.ToInt32(rtValue.Left), Convert.ToInt32(rtValue.Bottom)));
            Size inflatedDropSize = new Size(w, h);
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

    public class GoInputDateTime : GoWrapperControl<Going.UI.Controls.GoInputDateTime>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        public DateTime Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        public GoDateTimeKind DateTimeStyle { get => Control.DateTimeStyle; set { if (Control.DateTimeStyle != value) { Control.DateTimeStyle = value; Invalidate(); } } }
        public string DateFormat { get => Control.DateFormat; set { if (Control.DateFormat != value) { Control.DateFormat = value; Invalidate(); } } }
        public string TimeFormat { get => Control.TimeFormat; set { if (Control.TimeFormat != value) { Control.TimeFormat = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoInputDateTime()
        {
            Control.DropDownOpening += (o, s) =>
            {
                s.Cancel = true;

                OpenDropDown(Control.Areas()["Value"]);
            };
        }
        #endregion

        #region DropDown
        GoDateTimeDropDownWindow? dwnd;

        void Opened()
        {
            if (dwnd != null)
            {
                dwnd.Set(FontName, FontStyle, FontSize, Value, DateTimeStyle, (datetime) =>
                {
                    if (datetime.HasValue)
                        Value = datetime.Value;
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

            dwnd = new();
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
            var w = Convert.ToInt32(MathTool.Constrain(rtValue.Width, 240, 300));
            var h = DateTimeStyle == GoDateTimeKind.Date ? w + 40 : (DateTimeStyle == GoDateTimeKind.Time ? 80 + 10 : w + 40 + 40);

            Point pt = PointToScreen(new Point(Convert.ToInt32(rtValue.Left), Convert.ToInt32(rtValue.Bottom)));
            Size inflatedDropSize = new Size(w, h);
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
