using Going.UI.Collections;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Dialogs;
using Going.UI.Forms.Input;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Going.UI.Extensions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using Going.UI.Tools;
using Windows.AI.MachineLearning;

namespace Going.UI.Forms.Controls
{
    public class GoDataGrid : GoWrapperControl<Going.UI.Controls.GoDataGrid>
    {
        #region Const
        internal const float InputBright = -0.2F;
        internal const float CheckBoxBright = -0.2F;
        internal const float BorderBright = 0.35F;
        #endregion

        #region Properties
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }
        
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string RowColor { get => Control.RowColor; set { if (Control.RowColor != value) { Control.RowColor = value; Invalidate(); } } }
        public string SummaryRowColor { get => Control.SummaryRowColor; set { if (Control.SummaryRowColor != value) { Control.SummaryRowColor = value; Invalidate(); } } }
        public string ColumnColor { get => Control.ColumnColor; set { if (Control.ColumnColor != value) { Control.ColumnColor = value; Invalidate(); } } }
        public string SelectedRowColor { get => Control.SelectedRowColor; set { if (Control.SelectedRowColor != value) { Control.SelectedRowColor = value; Invalidate(); } } }

        public float RowHeight { get => Control.RowHeight; set { if (Control.RowHeight != value) { Control.RowHeight = value; Invalidate(); } } }
        public float ColumnHeight { get => Control.ColumnHeight; set { if (Control.ColumnHeight != value) { Control.ColumnHeight = value; Invalidate(); } } }

        public ScrollMode ScrollMode { get => Control.ScrollMode; set { if (Control.ScrollMode != value) { Control.ScrollMode = value; Invalidate(); } } }
        public GoDataGridSelectionMode SelectionMode { get => Control.SelectionMode; set { if (Control.SelectionMode != value) { Control.SelectionMode = value; Invalidate(); } } }

        public ObservableList<GoDataGridColumn> ColumnGroups => Control.ColumnGroups;
        public ObservableList<GoDataGridColumn> Columns => Control.Columns;
        public List<GoDataGridSummaryRow> SummaryRows => Control.SummaryRows;

        [JsonIgnore] public List<GoDataGridRow> Rows => Control.Rows;
        #endregion

        #region Event
        public event EventHandler? SelectedChanged { add => Control.SelectedChanged += value; remove => Control.SelectedChanged -= value; }
        public event EventHandler? SortChanged { add => Control.SortChanged += value; remove => Control.SortChanged -= value; }

        public event EventHandler<GoDataGridColumnMouseEventArgs>? ColumnMouseClick { add => Control.ColumnMouseClick += value; remove => Control.ColumnMouseClick -= value; }
        public event EventHandler<GoDataGridCellMouseEventArgs>? CellMouseClick { add => Control.CellMouseClick += value; remove => Control.CellMouseClick -= value; }
        public event EventHandler<GoDataGridCellMouseEventArgs>? CellMouseDoubleClick { add => Control.CellMouseDoubleClick += value; remove => Control.CellMouseDoubleClick -= value; }
        public event EventHandler<GoDataGridCellMouseEventArgs>? CellMouseLongClick { add => Control.CellMouseLongClick += value; remove => Control.CellMouseLongClick -= value; }
        public event EventHandler<GoDataGridCellButtonClickEventArgs>? CellButtonClick { add => Control.CellButtonClick += value; remove => Control.CellButtonClick -= value; }
        public event EventHandler<GoDataGridCellValueChangedEventArgs>? ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoDataGrid()
        {
            Control.DateTimeDropDownOpening += (o, s) => { s.Cancel = true; OpenDropDown_DT(s); };
            Control.ColorDropDownOpening += (o, s) => { s.Cancel = true; OpenDropDown_C(s); };
            Control.ComboDropDownOpening += (o, s) => { s.Cancel = true; OpenDropDown_CB(s); };
            Control.GetColorDropDownVisible += (cell) => (dwnd_C?.Visible ?? false) && dwndCell_C == cell;
            Control.GetDateTimeDropDownVisible += (cell) => (dwnd_DT?.Visible ?? false) && dwndCell_DT == cell;
            Control.GetComboDropDownVisible += (cell) => (dwnd_CB?.Visible ?? false) && dwndCell_CB == cell;

            GoInputEventer.Current.InputNumber += (c, bounds, callback, type, value, min, max) =>
            {
                if (c == Control && Control.InputObject is GoDataGridCell cell)
                {
                    var thm = GoTheme.Current;
                    var br = GoTheme.Current.Dark ? 1F : -1F;
                    var cSel = thm.ToColor(SelectedRowColor);
                    var cRow = thm.ToColor(RowColor);
                    var cF = cell.Row.Selected ? cSel : cRow;
                    var cV = cF.BrightnessTransmit(cell.Row.RowIndex % 2 == 0 ? 0.05F : -0.05F).BrightnessTransmit(GoDataGrid.InputBright * br);

                    if (type == typeof(byte)) FormsInputManager.Current.InputNumber<byte>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(sbyte)) FormsInputManager.Current.InputNumber<sbyte>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(ushort)) FormsInputManager.Current.InputNumber<ushort>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(short)) FormsInputManager.Current.InputNumber<short>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(int)) FormsInputManager.Current.InputNumber<int>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(uint)) FormsInputManager.Current.InputNumber<uint>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(long)) FormsInputManager.Current.InputNumber<long>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(ulong)) FormsInputManager.Current.InputNumber<ulong>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(float)) FormsInputManager.Current.InputNumber<float>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(double)) FormsInputManager.Current.InputNumber<double>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(decimal)) FormsInputManager.Current.InputNumber<decimal>(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);

                }
            };

            GoInputEventer.Current.InputString += (c, bounds, callback, value) =>
            {
                if (c == Control)
                {
                    if (Control.InputObject is GoDataGridCell cell)
                    {
                        var thm = GoTheme.Current;
                        var br = GoTheme.Current.Dark ? 1F : -1F;
                        var cSel = thm.ToColor(SelectedRowColor);
                        var cRow = thm.ToColor(RowColor);
                        var cF = cell.Row.Selected ? cSel : cRow;
                        var cV = cF.BrightnessTransmit(cell.Row.RowIndex % 2 == 0 ? 0.05F : -0.05F).BrightnessTransmit(GoDataGrid.InputBright * br);

                        FormsInputManager.Current.InputString(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, callback, spkey, value);
                    }
                    else if(Control.InputObject is GoDataGridColumn col)
                    {
                        var thm = GoTheme.Current;
                        var br = GoTheme.Current.Dark ? 1F : -1F;
                        var cF = thm.ToColor(ColumnColor);
                        var cV = cF.BrightnessTransmit(GoDataGrid.InputBright * br);

                        FormsInputManager.Current.InputString(this, Control, bounds, FontName, FontStyle, FontSize, $"#{cV.Red:X2}{cV.Green:X2}{cV.Blue:X2}", TextColor, callback, spkey, value);
                    }
                }
            };
        }
        #endregion

        #region Override
        protected override void OnResize(EventArgs e)
        {
            FormsInputManager.Current.ClearInput();
            base.OnResize(e);
        }
        #endregion

        #region Method
        public void SetDataSource<T>(IEnumerable<T> values) => Control.SetDataSource<T>(values);
        public void RefreshRows() => Control.RefreshRows();

        void spkey(Keys key, Keys modifier)
        {
            if((key == Keys.Enter || key == Keys.Down) && Control.InputObject is GoDataGridCell cell)
            {
                var ri = Control.ViewRows.IndexOf(cell.Row);
                if (ri + 1 < Control.ViewRows.Count)
                {
                    FormsInputManager.Current.ClearInput();
                    Control.InputCell(Control.ViewRows[ri + 1].Cells[cell.ColumnIndex], "d");
                }
            }
            else if ((key == Keys.Up) && Control.InputObject is GoDataGridCell cell2)
            {
                var ri = Control.ViewRows.IndexOf(cell2.Row);
                if (ri - 1 >= 0)
                {
                    FormsInputManager.Current.ClearInput();
                    Control.InputCell(Control.ViewRows[ri - 1].Cells[cell2.ColumnIndex], "u");
                }
            }
            else if((key == Keys.Left && modifier == Keys.Control) && Control.InputObject is GoDataGridCell cell3)
            {
                var ls = cell3.Row.Cells.Where(x =>
                {
                    var tp = x.GetType();
                    return x is GoDataGridInputTextCell || (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(GoDataGridInputNumberCell<>));
                }).ToList();

                var idx = ls.IndexOf(cell3);
                if (idx >= 0 && idx - 1 >= 0)
                {
                    FormsInputManager.Current.ClearInput();
                    Control.InputCell(ls[idx - 1], "l");
                }
            }
            else if ((key == Keys.Right && modifier == Keys.Control) && Control.InputObject is GoDataGridCell cell4)
            {
                var ls = cell4.Row.Cells.Where(x =>
                {
                    var tp = x.GetType();
                    return x is GoDataGridInputTextCell || (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(GoDataGridInputNumberCell<>));
                }).ToList();

                var idx = ls.IndexOf(cell4);
                if (idx >= 0 && idx + 1 < ls.Count)
                {
                    FormsInputManager.Current.ClearInput();
                    Control.InputCell(ls[idx + 1], "r");
                }
            }
        }
        #endregion

        #region DropDowns
        #region DateTime
        GoDateTimeDropDownWindow? dwnd_DT;
        GoDataGridInputTimeCell? dwndCell_DT;

        void Opened_DT(GoDataGridDateTimeDropDownOpeningEventArgs args)
        {
            if (dwnd_DT != null)
            {
                dwndCell_DT = args.Cell;
                dwnd_DT.Set(FontName, FontStyle, FontSize, args.Value, args.Style, args.Action);
            }
        }

        #region var
        bool closedWhileInControl_DT;

        DropWindowState DropState_DT { get; set; }
        bool CanDrop_DT
        {
            get
            {
                if (dwnd_DT != null) return false;
                if (dwnd_DT == null && closedWhileInControl_DT)
                {
                    closedWhileInControl_DT = false;
                    return false;
                }

                return !closedWhileInControl_DT;
            }
        }
        #endregion
        #region Method
        #region Freeze
        internal void FreezeDropDown_DT(bool remainVisible)
        {
            if (dwnd_DT != null)
            {
                dwnd_DT.Freeze = true;
                if (!remainVisible) dwnd_DT.Visible = false;
            }
        }

        internal void UnFreezeDropDown_DT()
        {
            if (dwnd_DT != null)
            {
                dwnd_DT.Freeze = false;
                if (!dwnd_DT.Visible) dwnd_DT.Visible = true;
            }
        }
        #endregion
        #region Open
        private void OpenDropDown_DT(GoDataGridDateTimeDropDownOpeningEventArgs args)
        {
            this.Move += (o, s) => { if (dwnd_DT != null) dwnd_DT.Bounds = GetDropDownBounds_DT(args); };

            dwnd_DT = new();
            dwnd_DT.Bounds = GetDropDownBounds_DT(args);
            dwnd_DT.DropStateChanged += (o, s) => { DropState_DT = s.DropState; };
            dwnd_DT.FormClosed += (o, s) =>
            {
                if (dwnd_DT != null && !dwnd_DT.IsDisposed) dwnd_DT.Dispose();
                dwnd_DT = null;
                closedWhileInControl_DT = (this.RectangleToScreen(this.ClientRectangle).Contains(Cursor.Position));
                DropState_DT = DropWindowState.Closed;
                this.Invalidate();
            };

            Opened_DT(args);

            DropState_DT = DropWindowState.Dropping;
            dwnd_DT.Show(this);
            DropState_DT = DropWindowState.Dropped;
            this.Invalidate();
        }
        #endregion
        #region Close
        public void CloseDropDown()
        {
            if (dwnd_DT != null)
            {
                DropState_DT = DropWindowState.Closing;
                dwnd_DT.Freeze = false;
                dwnd_DT.Close();
            }
        }
        #endregion
        #region Bounds
        private Rectangle GetDropDownBounds_DT(GoDataGridDateTimeDropDownOpeningEventArgs args)
        {
            var w = Convert.ToInt32(MathTool.Constrain(args.Bounds.Width, 240, 300));
            var h = args.Style == GoDateTimeKind.Date ? w + 40 : (args.Style == GoDateTimeKind.Time ? 80 + 10 : w + 40 + 40);

            Point pt = PointToScreen(new Point(Convert.ToInt32(args.Bounds.Left), Convert.ToInt32(args.Bounds.Bottom + 1)));
            Size inflatedDropSize = new Size(w, h);
            Rectangle screenBounds = new Rectangle(pt, inflatedDropSize);
            Rectangle workingArea = Screen.GetWorkingArea(screenBounds);

            if (screenBounds.X < workingArea.X) screenBounds.X = workingArea.X;
            if (screenBounds.Y < workingArea.Y) screenBounds.Y = workingArea.Y;

            if (screenBounds.Right > workingArea.Right && workingArea.Width > screenBounds.Width) screenBounds.X = workingArea.Right - screenBounds.Width;
            if (screenBounds.Bottom > workingArea.Bottom && workingArea.Height > screenBounds.Height) screenBounds.Y = Convert.ToInt32(pt.Y - args.Bounds.Height - screenBounds.Height - 1);
            return screenBounds;
        }
        #endregion
        #endregion
        #endregion
        #region Color
        GoColorDropDownWindow? dwnd_C;
        GoDataGridInputColorCell? dwndCell_C;

        void Opened_C(GoDataGridColorDropDownOpeningEventArgs args)
        {
            if (dwnd_C != null)
            {
                dwndCell_C = args.Cell;
                dwnd_C.Set(FontName, FontStyle, FontSize, args.Value, args.Action);
            }
        }

        #region var
        bool closedWhileInControl_C;

        DropWindowState DropState_C { get; set; }
        bool CanDrop_C
        {
            get
            {
                if (dwnd_C != null) return false;
                if (dwnd_C == null && closedWhileInControl_C)
                {
                    closedWhileInControl_C = false;
                    return false;
                }

                return !closedWhileInControl_C;
            }
        }
        #endregion
        #region Method
        #region Freeze
        internal void FreezeDropDown_C(bool remainVisible)
        {
            if (dwnd_C != null)
            {
                dwnd_C.Freeze = true;
                if (!remainVisible) dwnd_C.Visible = false;
            }
        }

        internal void UnFreezeDropDown_C()
        {
            if (dwnd_C != null)
            {
                dwnd_C.Freeze = false;
                if (!dwnd_C.Visible) dwnd_C.Visible = true;
            }
        }
        #endregion
        #region Open
        private void OpenDropDown_C(GoDataGridColorDropDownOpeningEventArgs args)
        {
            this.Move += (o, s) => { if (dwnd_C != null) dwnd_C.Bounds = GetDropDownBounds_C(args); };

            dwnd_C = new();
            dwnd_C.Bounds = GetDropDownBounds_C(args);
            dwnd_C.DropStateChanged += (o, s) => { DropState_C = s.DropState; };
            dwnd_C.FormClosed += (o, s) =>
            {
                if (dwnd_C != null && !dwnd_C.IsDisposed) dwnd_C.Dispose();
                dwnd_C = null;
                closedWhileInControl_C = (this.RectangleToScreen(this.ClientRectangle).Contains(Cursor.Position));
                DropState_C = DropWindowState.Closed;
                this.Invalidate();
            };

            Opened_C(args);

            DropState_C = DropWindowState.Dropping;
            dwnd_C.Show(this);
            DropState_C = DropWindowState.Dropped;
            this.Invalidate();
        }
        #endregion
        #region Close
        public void CloseDropDown_C()
        {
            if (dwnd_C != null)
            {
                DropState_C = DropWindowState.Closing;
                dwnd_C.Freeze = false;
                dwnd_C.Close();
            }
        }
        #endregion
        #region Bounds
        private Rectangle GetDropDownBounds_C(GoDataGridColorDropDownOpeningEventArgs args)
        {
            var w = Convert.ToInt32(MathTool.Constrain(args.Bounds.Width, 280, 360));
            var h = w + 40;

            Point pt = PointToScreen(new Point(Convert.ToInt32(args.Bounds.Left), Convert.ToInt32(args.Bounds.Bottom + 1)));
            Size inflatedDropSize = new Size(w, h);
            Rectangle screenBounds = new Rectangle(pt, inflatedDropSize);
            Rectangle workingArea = Screen.GetWorkingArea(screenBounds);

            if (screenBounds.X < workingArea.X) screenBounds.X = workingArea.X;
            if (screenBounds.Y < workingArea.Y) screenBounds.Y = workingArea.Y;

            if (screenBounds.Right > workingArea.Right && workingArea.Width > screenBounds.Width) screenBounds.X = workingArea.Right - screenBounds.Width;
            if (screenBounds.Bottom > workingArea.Bottom && workingArea.Height > screenBounds.Height) screenBounds.Y = Convert.ToInt32(pt.Y - args.Bounds.Height - screenBounds.Height - 1);
            return screenBounds;
        }
        #endregion
        #endregion
        #endregion
        #region Combo
        GoComboBoxDropDownWindow? dwnd_CB;
        GoDataGridInputComboCell? dwndCell_CB;

        void Opened_CB(GoDataGridComboDropDownOpeningEventArgs args)
        {
            if (dwnd_CB != null)
            {
                dwndCell_CB = args.Cell;
                dwnd_CB.Set(FontName, FontStyle, FontSize, args.ItemHeight, args.MaximumViewCount,
                                     args.Items.Cast<GoListItem>().ToList(), args.SelectedItem, (v) => { if (v is GoDataGridInputComboItem c) args.Action(c); });
            }
        }

        #region var
        bool closedWhileInControl_CB;

        DropWindowState DropState_CB { get; set; }
        bool CanDrop_CB
        {
            get
            {
                if (dwnd_CB != null) return false;
                if (dwnd_CB == null && closedWhileInControl_CB)
                {
                    closedWhileInControl_CB = false;
                    return false;
                }

                return !closedWhileInControl_CB;
            }
        }
        #endregion
        #region Method
        #region Freeze
        internal void FreezeDropDown_CB(bool remainVisible)
        {
            if (dwnd_CB != null)
            {
                dwnd_CB.Freeze = true;
                if (!remainVisible) dwnd_CB.Visible = false;
            }
        }

        internal void UnFreezeDropDown_CB()
        {
            if (dwnd_CB != null)
            {
                dwnd_CB.Freeze = false;
                if (!dwnd_CB.Visible) dwnd_CB.Visible = true;
            }
        }
        #endregion
        #region Open
        private void OpenDropDown_CB(GoDataGridComboDropDownOpeningEventArgs args)
        {
            this.Move += (o, s) => { if (dwnd_CB != null) dwnd_CB.Bounds = GetDropDownBounds(args); };

            dwnd_CB = new();
            dwnd_CB.Bounds = GetDropDownBounds(args);
            dwnd_CB.DropStateChanged += (o, s) => { DropState_CB = s.DropState; };
            dwnd_CB.FormClosed += (o, s) =>
            {
                if (dwnd_CB != null && !dwnd_CB.IsDisposed) dwnd_CB.Dispose();
                dwnd_CB = null;
                closedWhileInControl_CB = (this.RectangleToScreen(this.ClientRectangle).Contains(Cursor.Position));
                DropState_CB = DropWindowState.Closed;
                this.Invalidate();
            };

            Opened_CB(args);

            DropState_CB = DropWindowState.Dropping;
            dwnd_CB.Show(this);
            DropState_CB = DropWindowState.Dropped;
            this.Invalidate();
        }
        #endregion
        #region Close
        public void CloseDropDown_CB()
        {
            if (dwnd_CB != null)
            {
                DropState_CB = DropWindowState.Closing;
                dwnd_CB.Freeze = false;
                dwnd_CB.Close();
            }
        }
        #endregion
        #region Bounds
        private Rectangle GetDropDownBounds(GoDataGridComboDropDownOpeningEventArgs args)
        {
            int n = args.Items.Count;
            var items = args.Items;

            Point pt = PointToScreen(new Point(Convert.ToInt32(args.Bounds.Left), Convert.ToInt32(args.Bounds.Bottom + 1)));
            if (args.MaximumViewCount != -1) n = args.Items.Count > args.MaximumViewCount ? args.MaximumViewCount : args.Items.Count;
            Size inflatedDropSize = new Size(Convert.ToInt32(args.Bounds.Width), Convert.ToInt32(n * args.ItemHeight + 2));
            Rectangle screenBounds = new Rectangle(pt, inflatedDropSize);
            Rectangle workingArea = Screen.GetWorkingArea(screenBounds);

            if (screenBounds.X < workingArea.X) screenBounds.X = workingArea.X;
            if (screenBounds.Y < workingArea.Y) screenBounds.Y = workingArea.Y;

            if (screenBounds.Right > workingArea.Right && workingArea.Width > screenBounds.Width) screenBounds.X = workingArea.Right - screenBounds.Width;
            if (screenBounds.Bottom > workingArea.Bottom && workingArea.Height > screenBounds.Height) screenBounds.Y = Convert.ToInt32(pt.Y - args.Bounds.Height - screenBounds.Height - 1);
            return screenBounds;
        }
        #endregion
        #endregion
        #endregion
        #endregion
    }
}
