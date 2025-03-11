using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    #region class : GoDataGridCell
    public abstract class GoDataGridCell
    {
        #region Properties
        public string? Name { get; set; }

        public int ColSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
        public int ColumnIndex => Row.Cells.IndexOf(this);
        public int RowIndex => Row.RowIndex;

        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public object? Tag { get; set; }

        public string CellBackColor { get; set; }
        public string SelectedCellBackColor { get; set; }
        public string CellTextColor { get; set; }

        public GoDataGrid Grid { get; private set; }
        public GoDataGridRow Row { get; private set; }
        public GoDataGridColumn Column { get; private set; }

        protected PropertyInfo? ValueInfo { get; }
        public object? Value
        {
            get => Row.Source != null && ValueInfo != null ? ValueInfo.GetValue(Row.Source) : null;
            set
            {
                if (ValueInfo != null && ValueInfo.CanWrite)
                    ValueInfo.SetValue(Row.Source, value);
            }
        }

        internal SKRect Bounds
        {
            get
            {
                var ci = ColumnIndex;
                var l = Grid.Columns[ci].Bounds.Left;
                var r = Grid.Columns[ci + ColSpan - 1].Bounds.Right;

                var ri = RowIndex;
                var t = Grid.ViewRows[ri].Bounds.Top;
                var b = Grid.ViewRows[ri + RowSpan- 1].Bounds.Bottom;

                return new SKRect(l, t, r, b);
            }
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        #endregion

        #region Constructor
        public GoDataGridCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column)
        {
            this.Grid = Grid;
            this.Row = Row;
            this.Column = Column;

            if (Grid.DataType != null && !(Column is GoDataGridButtonColumn) && Column?.Name != null)
                ValueInfo = Grid.DataType.GetProperty(Column.Name);

            CellBackColor = Grid.RowColor;
            SelectedCellBackColor = Grid.SelectedRowColor;
            CellTextColor = Grid.TextColor;
        }
        #endregion

        #region Method
        #region Fire
        #region Draw
        internal void Draw(SKCanvas canvas)
        {
            if (Grid != null)
            {
                var thm = GoTheme.Current;
                var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
                var cBack = thm.ToColor(CellBackColor ?? Grid.RowColor);
                var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
                var cF = Row.Selected ? cSel : cBack;
                var cB = cF.BrightnessTransmit(thm.Dark ? GoDataGrid.BorderBright : -GoDataGrid.BorderBright);
                
                Util.DrawBox(canvas, Bounds, cF, cB, GoRoundType.Rect, thm.Corner);

                OnDraw(canvas);
            }
        }
        #endregion

        #region Mouse
        internal void MouseDown(float x, float y, GoMouseButton button)
        {
            if (CollisionTool.Check(Bounds, x, y))
            {
                bDown = true;
                OnMouseDown(x, y, button);
            }
        }

        internal void MouseUp(float x, float y, GoMouseButton button)
        {
            if (bDown)
            {
                bDown = false;
                OnMouseUp(x, y, button);
            }
        }

        internal void MouseMove(float x, float y)
        {
            if (bDown)
            {
                OnMouseMove(x, y);
            }
        }
        #endregion
        #endregion

        #region Virtual
        protected virtual void OnDraw(SKCanvas canvas) { }
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseMove(float x, float y) { }
        #endregion
        #endregion
    }
    #endregion
    #region class : GoDataGridColumn
    public abstract class GoDataGridColumn
    {
        #region Properties
        public string? Name { get; set; }
        public string? GroupName { get; set; }
        public string? HeaderText { get; set; }
        public string? Size { get; set; } = "100%";

        public bool UseFilter { get; set; }
        public string? FilterText { get; set; }

        public bool UseSort { get; set; }
        public GoDataGridColumnSortState SortState { get; set; } = GoDataGridColumnSortState.None;
        public int SortOrder { get; }

        public string? TextColor { get; set; }
        public bool Fixed { get; set; }

        [JsonIgnore] public Type? CellType { get; set; }
        [JsonIgnore] public GoDataGrid? Grid { get; internal set; }
        [JsonIgnore] internal SKRect Bounds { get; set; }
        [JsonIgnore] internal SKRect FilterBounds { get; set; }
        [JsonIgnore] internal int Depth { get; set; }
        #endregion

        #region Member Variable
        private bool bDown = false;
        #endregion

        #region Method
        #region Fire
        #region Draw
        internal void Draw(SKCanvas canvas)
        {
            if (Grid != null)
            {
                var thm = GoTheme.Current;
                var cText = thm.ToColor(TextColor ?? Grid.TextColor);
                var cBack = thm.ToColor(Grid.ColumnColor);
                var cBorder = cBack.BrightnessTransmit(thm.Dark ? GoDataGrid.BorderBright : -GoDataGrid.BorderBright);
                var cIF = cBack.BrightnessTransmit(thm.Dark ? GoDataGrid.InputBright : -GoDataGrid.InputBright);
                var cIB = GoInputEventer.Current.InputControl == Grid && Grid.InputObject == this ? thm.Hignlight : cBorder;
                var isCol = Grid.Columns.Contains(this);
                var (rtText, rtSort) = bounds();

                Util.DrawBox(canvas, Bounds, SKColors.Transparent, cBorder, GoRoundType.Rect, thm.Corner);
                Util.DrawText(canvas, HeaderText, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtText, cText);

                if (isCol)
                {
                    if (FilterBounds.Height > 0) Util.DrawBox(canvas, FilterBounds, SKColors.Transparent, cBorder, GoRoundType.Rect, thm.Corner);
                    if (UseFilter)
                    {
                        var rt = FilterBounds; rt.Inflate(-2, -2);
                        Util.DrawBox(canvas, rt, cIF, cIB, GoRoundType.Rect, thm.Corner);
                        Util.DrawText(canvas, FilterText, Grid.FontName, Grid.FontStyle, Grid.FontSize, rt, cText);
                    }

                    if (UseSort)
                    {
                        Util.DrawIcon(canvas, "fa-sort", 12, rtSort, cText.WithAlpha(60));

                        if (SortState == GoDataGridColumnSortState.Asc) Util.DrawIcon(canvas, "fa-sort-up", 12, rtSort, cText);
                        else if (SortState == GoDataGridColumnSortState.Desc) Util.DrawIcon(canvas, "fa-sort-down", 12, rtSort, cText);
                    }
                }
                OnDraw(canvas);
            }
        }
        #endregion

        #region Mouse
        internal void MouseDown(float x, float y, GoMouseButton button)
        {
            var (rtText, rtSort) = bounds();
            var isCol = Grid?.Columns.Contains(this) ?? false;

            if (CollisionTool.Check(Bounds, x, y))
            {
                bDown = true;
                OnMouseDown(x, y, button);
            }

            if (isCol)
            {
                if (UseFilter && CollisionTool.Check(FilterBounds, x, y) && Grid != null)
                {
                    Grid.InputObject = this;
                    GoInputEventer.Current.FireInputString(Grid, FilterBounds, (s) => { FilterText = s; Grid?.RefreshRows(); }, FilterText);
                }

                if (CollisionTool.Check(rtSort, x, y))
                {
                    if (SortState == GoDataGridColumnSortState.None) SortState = GoDataGridColumnSortState.Asc;
                    else if (SortState == GoDataGridColumnSortState.Asc) SortState = GoDataGridColumnSortState.Desc;
                    else if (SortState == GoDataGridColumnSortState.Desc) SortState = GoDataGridColumnSortState.None;

                    Grid?.RefreshRows();
                }
            }
        }

        internal void MouseUp(float x, float y, GoMouseButton button)
        {
            if (bDown)
            {
                bDown = false;
                OnMouseUp(x, y, button);
            }
        }

        internal void MouseMove(float x, float y)
        {
            if (bDown)
            {
                OnMouseMove(x, y);
            }
        }
        #endregion
        #endregion

        #region Virtual
        protected virtual void OnDraw(SKCanvas canvas) { }
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseMove(float x, float y) { }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtSort) bounds()
        {
            var isCol = Grid?.Columns.Contains(this) ?? false;
            var rts = Util.Columns(Bounds, ["100%", $"{(UseSort && isCol ? 24 : 0)}px"]);
            if (UseSort) rts[0].Offset(6, 0);
            return (rts[0], rts[1]);
        }
        #endregion
        #endregion
    }
    #endregion
    #region class : GoDataGridRow
    public class GoDataGridRow
    {
        #region Properties
        public int RowIndex { get; internal set; }
        public float RowHeight { get; set; }
        public List<GoDataGridCell> Cells { get; private set; } = [];
        public object? Source { get; internal set; }
        public object? Tag { get; set; }
        public bool Selected { get; set; }
        public GoDataGrid? Grid { get; internal set; }

        internal SKRect Bounds { get; set; }
        #endregion
    }
    #endregion
    #region class : GoDataGridSummaryCell
    public abstract class GoDataGridSummaryCell
    {
        #region Properties
        public string? Name { get; set; }

        public int ColSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
        public int ColumnIndex => Row.Cells.IndexOf(this);
        public int RowIndex => Grid.SummaryRows.IndexOf(Row);

        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public object? Tag { get; set; }

        public string CellBackColor { get; set; }
        public string CellTextColor { get; set; }

        public GoDataGrid Grid { get; private set; }
        public GoDataGridSummaryRow Row { get; private set; }
        public GoDataGridColumn Column { get; private set; }

        internal SKRect Bounds => new SKRect(Column.Bounds.Left, Row.Bounds.Top, Column.Bounds.Right, Row.Bounds.Bottom);
        #endregion

        #region Constructor
        public GoDataGridSummaryCell(GoDataGrid Grid, GoDataGridSummaryRow Row, GoDataGridColumn Column)
        {
            this.Grid = Grid;
            this.Row = Row;
            this.Column = Column;

            CellBackColor = Grid.SummaryRowColor;
            CellTextColor = Grid.TextColor;
        }
        #endregion

        public virtual void Draw(SKCanvas canvas) { }
        public virtual void MouseDown(float x, float y, GoMouseButton button) { }
        public virtual void MouseUp(float x, float y, GoMouseButton button) { }
        public virtual void MouseMove(float x, float y) { }

        public virtual void Calculate() { }
    }
    #endregion
    #region class : GoDataGridSummaryRow
    public abstract class GoDataGridSummaryRow
    {
        #region Properties
        public int RowIndex { get; internal set; }
        public int RowHeight { get; set; }
        public List<GoDataGridSummaryCell> Cells { get; private set; } = [];
        public GoDataGrid? Grid { get; internal set; }

        internal SKRect Bounds { get; set; }
        #endregion

        public virtual void Draw(SKCanvas canvas) { }
        public virtual void MouseDown(float x, float y, GoMouseButton button) { }
        public virtual void MouseUp(float x, float y, GoMouseButton button) { }
        public virtual void MouseMove(float x, float y) { }
    }
    #endregion


    public class GoDataGridSumSummaryRow : GoDataGridSummaryRow { }

    #region Label
    public class GoDataGridLabelCell : GoDataGridCell
    {
        #region Constructor
        public GoDataGridLabelCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = ValueTool.ToString(Value, null);
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
    }

    public class GoDataGridLabelColumn : GoDataGridColumn
    {
        #region Constructor
        public GoDataGridLabelColumn()
        {
            CellType = typeof(GoDataGridLabelCell);
        }
        #endregion
    }
    #endregion
    #region Button
    public class GoDataGridButtonCell : GoDataGridCell
    {
        #region Constructor
        public GoDataGridButtonCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion
    }

    public class GoDataGridButtonColumn : GoDataGridColumn
    {
        #region Constructor
        public GoDataGridButtonColumn()
        {
            CellType = typeof(GoDataGridButtonCell);
        }
        #endregion
    }
    #endregion
}
