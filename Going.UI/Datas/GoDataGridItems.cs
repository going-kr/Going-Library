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
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Datas
{
    #region class : GoDataGridCell
    public abstract class GoDataGridCell
    {
        #region Const
        protected const float InputInflateW = -1;
        protected const float InputInflateH = -1;
        #endregion

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
                var cV = cF.BrightnessTransmit(RowIndex % 2 == 0 ? 0.05F : -0.05F);
                var cB = cV.BrightnessTransmit(thm.Dark ? GoDataGrid.BorderBright : -GoDataGrid.BorderBright);
                
                Util.DrawBox(canvas, Bounds, cV, GoRoundType.Rect, thm.Corner);

                using var pe = SKPathEffect.CreateDash([2, 2,], 3);
                using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 1, Color = cB, PathEffect = pe };
                var l = Convert.ToInt32(Bounds.Left) + 0.5F;
                var r = Convert.ToInt32(Bounds.Right) + 0.5F;
                var ci = ColumnIndex;
                if (ci > 0) canvas.DrawLine(l, Bounds.Top, l, Bounds.Bottom, p);
                if (ci < Grid.Columns.Count-1) canvas.DrawLine(r, Bounds.Top, r, Bounds.Bottom, p);
                p.PathEffect = null;
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

        internal void MouseClick(float x, float y, GoMouseButton button)
        {
            OnMouseClick(x, y, button);
        }

        internal void MouseMove(float x, float y)
        {
            OnMouseMove(x, y);
        }
        #endregion
        #endregion

        #region Virtual
        protected virtual void OnDraw(SKCanvas canvas) { }
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseMove(float x, float y) { }
        protected virtual void OnMouseClick(float x, float y, GoMouseButton button) { }
        #endregion
        #endregion
    }
    #endregion
    #region class : GoDataGridColumn
    public abstract class GoDataGridColumn
    {
        #region Const
        protected const float InputInflateW = -1;
        protected const float InputInflateH = -1;
        #endregion

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
                        var rt = FilterBounds; rt.Inflate(InputInflateW, InputInflateH);
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

        internal SKRect Bounds
        {
            get
            {
                var ci = ColumnIndex;
                var l = Grid.Columns[ci].Bounds.Left;
                var r = Grid.Columns[ci + ColSpan - 1].Bounds.Right;

                var ri = RowIndex;
                var t = Grid.SummaryRows[ri].Bounds.Top;
                var b = Grid.SummaryRows[ri + RowSpan - 1].Bounds.Bottom;

                return new SKRect(l, t, r, b);
            }
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
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

        #region Method
        #region Fire
        #region Draw
        internal void Draw(SKCanvas canvas)
        {
            if (Grid != null)
            {
                var thm = GoTheme.Current;
                var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
                var cBack = thm.ToColor(CellBackColor ?? Grid.SummaryRowColor);
                var cF = cBack;
                var cV = cF;
                var cB = cV.BrightnessTransmit(thm.Dark ? GoDataGrid.BorderBright : -GoDataGrid.BorderBright);

                Util.DrawBox(canvas, Bounds, cV, GoRoundType.Rect, thm.Corner);

                using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 1, Color = cB };
                var l = Convert.ToInt32(Bounds.Left) + 0.5F;
                var r = Convert.ToInt32(Bounds.Right) + 0.5F;
                canvas.DrawLine(l, Bounds.Top, l, Bounds.Bottom, p);
                canvas.DrawLine(r, Bounds.Top, r, Bounds.Bottom, p);
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

        public virtual void Calculate() { }
        #endregion
        #endregion
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
        public int TitleColumnIndex { get; set; } = 0;
        public int TitleColSpan { get; set; } = 1;
        public string Title { get; set; } = "";

        internal SKRect Bounds { get; set; }
        #endregion

        #region Method
        public void Calculate()
        {
            foreach (var c in Cells) c.Calculate();
        }
        #endregion
    }
    #endregion

    #region Summary 
    #region Label
    public class GoDataGridLabelSummaryCell : GoDataGridSummaryCell
    {
        #region Properties
        public string Text { get; internal set; }
        #endregion

        #region Constructor
        public GoDataGridLabelSummaryCell(GoDataGrid Grid, GoDataGridSummaryRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            Util.DrawText(canvas, Text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion
    }
    #endregion
    #region Sum
    public class GoDataGridSumSummaryRow : GoDataGridSummaryRow { }
    public class GoDataGridSumSummaryCell : GoDataGridSummaryCell 
    {
        #region Properties
        public decimal Value { get; private set; }
        public string? FormatString { get; internal set; }
        #endregion

        #region Constructor
        public GoDataGridSumSummaryCell(GoDataGrid Grid, GoDataGridSummaryRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);
            
            var thm = GoTheme.Current;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = ValueTool.ToString(Value, FormatString);
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion

        #region Calculate
        public override void Calculate()
        {
            var ci = ColumnIndex;
            Value = Grid.ViewRows.Count == 0 ? 0 : Grid.ViewRows.Sum(x => Convert.ToDecimal(x.Cells[ci].Value));
        }
        #endregion
    }
    #endregion
    #region Average
    public class GoDataGridAverageSummaryRow : GoDataGridSummaryRow { }
    public class GoDataGridAverageSummaryCell : GoDataGridSummaryCell 
    {
        #region Properties
        public decimal Value { get; private set; }
        public string? FormatString { get; internal set; }
        #endregion

        #region Constructor
        public GoDataGridAverageSummaryCell(GoDataGrid Grid, GoDataGridSummaryRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion
         
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);
         
            var thm = GoTheme.Current;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = ValueTool.ToString(Value, FormatString);
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion

        #region Calculate
        public override void Calculate()
        {
            var ci = ColumnIndex;
            Value = Grid.ViewRows.Count == 0 ? 0 : Grid.ViewRows.Average(x => Convert.ToDecimal(x.Cells[ci].Value));
        }
        #endregion
    }
    #endregion
    #endregion

    #region Rows
    #region Label
    public class GoDataGridLabelCell : GoDataGridCell
    {
        #region Constructor
        public GoDataGridLabelCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = "";
            if (Column is GoDataGridLabelColumn col)
            {
                if (col.TextConverter != null) text = col.TextConverter(Value);
                else text = ValueTool.ToString(Value, col.FormatString);
            }
            else text = ValueTool.ToString(Value, null);

            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion
    }

    public class GoDataGridLabelColumn : GoDataGridColumn
    {
        #region Properties
        public Func<object?, string?>? TextConverter { get; set; }
        public string? FormatString { get; set; }
        #endregion

        #region Constructor
        public GoDataGridLabelColumn()
        {
            CellType = typeof(GoDataGridLabelCell);
        }
        #endregion
    }
    #endregion
    #region Number
    public class GoDataGridNumberCell<T> : GoDataGridCell where T : struct
    {
        #region Constructor
        public GoDataGridNumberCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = ValueTool.ToString(Value, (Column is GoDataGridNumberColumn<T> col ? col.FormatString : null));
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion
    }

    public class GoDataGridNumberColumn<T> : GoDataGridColumn where T : struct
    {
        public string? FormatString { get; set; }
        #region Constructor
        public GoDataGridNumberColumn()
        {
            CellType = typeof(GoDataGridNumberCell<T>);
        }
        #endregion
    }
    #endregion
    #region Button
    public class GoDataGridButtonCell : GoDataGridCell
    {
        #region Properties
        public string ButtonColor { get; set; } = "Base3";
        public string SelectButtonColor { get; set; } = "Select";
        public string? Text { get; set; }
        public string? IconString { get; set; }
        public int IconSize { get; set; } = 12;
        public int IconGap { get; set; } = 5;
        #endregion

        #region Member Variable
        bool bDown, bHover;
        #endregion

        #region Constructor
        public GoDataGridButtonCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if(Column is GoDataGridButtonColumn col)
            {
                this.ButtonColor = col.ButtonColor;
                this.SelectButtonColor = col.SelectButtonColor;
                this.Text = col.Text;
                this.IconString = col.IconString;
                this.IconSize = col.IconSize;
                this.IconGap = col.IconGap;
            }
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cButton = thm.ToColor(Row.Selected ? SelectButtonColor : ButtonColor).BrightnessTransmit(RowIndex % 2 == 0 ? 0.05F : -0.05F).BrightnessTransmit(bDown ? thm.DownBrightness : 0);

            var rt = Bounds; rt.Bottom -= 1; rt.Right -= 1;// rt.Inflate(-1, -1);
            Util.DrawBox(canvas, rt, cButton.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cButton.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), GoRoundType.Rect, thm.Corner);
            if (bDown) rt.Offset(0, 1);
            Util.DrawTextIcon(canvas, Text, Grid.FontName, Grid.FontStyle, Grid.FontSize, IconString, Grid.FontSize, GoDirectionHV.Horizon, 5, rt, cText, GoContentAlignment.MiddleCenter);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(-1, -1);
            if (CollisionTool.Check(rt, x, y)) bDown = true;

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(-1, -1);
            if (bDown)
            {
                bDown = false;
                if (CollisionTool.Check(rt, x, y)) Grid.InvokeButtonClick(this);
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rt = Bounds; rt.Inflate(-1, -1);
            bHover = CollisionTool.Check(rt, x, y);
            base.OnMouseMove(x, y);
        }
        #endregion
    }

    public class GoDataGridButtonColumn : GoDataGridColumn
    {
        #region Properties
        public string ButtonColor { get; set; } = "Base3";
        public string SelectButtonColor { get; set; } = "Select-light";
        public string? Text { get; set; } 
        public string? IconString { get; set; }
        public int IconSize { get; set; } = 12;
        public int IconGap { get; set; } = 5;
        #endregion

        #region Constructor
        public GoDataGridButtonColumn()
        {
            CellType = typeof(GoDataGridButtonCell);
        }
        #endregion
    }
    #endregion
    #region Lamp
    public class GoDataGridLampCell : GoDataGridCell
    {
        #region Properties
        public string OnColor { get; set; } = "Good";
        #endregion 

        #region Constructor
        public GoDataGridLampCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridLampColumn col)
            {
                this.OnColor = col.OnColor;
            }
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var val = Value is bool b ? b : false;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cOn = thm.ToColor(OnColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(Grid.SelectedRowColor);
            var cOff = (Row.Selected ? cSel : cRow).BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);

            var sz = Math.Min(Convert.ToInt32(Grid.RowHeight * 0.75), 24);
            var rt = MathTool.MakeRectangle(Bounds, new SKSize(sz, sz));
            Util.DrawLamp(canvas, rt, cOn, cOff, val);

        }
        #endregion
    }

    public class GoDataGridLampColumn : GoDataGridColumn
    {
        #region Properties
        public string OnColor { get; set; } = "Good";
        #endregion 

        #region Constructor
        public GoDataGridLampColumn()
        {
            CellType = typeof(GoDataGridLampCell);
        }
        #endregion
    }
    #endregion
    #region CheckBox
    public class GoDataGridCheckBoxCell : GoDataGridCell
    {
        #region Properties
        #endregion 

        #region Constructor
        public GoDataGridCheckBoxCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridCheckBoxColumn col)
            {
            }
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var br = GoTheme.Current.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rtChk = MathTool.MakeRectangle(Bounds, new SKSize(20, 20));

            Util.DrawBox(canvas, rtChk, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);
            if (Value is bool v && v) Util.DrawIcon(canvas, "fa-check", 12, rtChk, cText);
        }
        #endregion

        #region Mouse
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rtChk = MathTool.MakeRectangle(Bounds, new SKSize(20, 20));
            if(CollisionTool.Check(rtChk, x, y) && Value is bool v)
            {
                var old = Value;
                Value = !v;
                Grid?.InvokeValueChange(this, old, Value);
            }
            base.OnMouseClick(x, y, button);
        }
        #endregion
    }

    public class GoDataGridCheckBoxColumn : GoDataGridColumn
    {
        #region Properties
        #endregion 

        #region Constructor
        public GoDataGridCheckBoxColumn()
        {
            CellType = typeof(GoDataGridCheckBoxCell);
        }
        #endregion
    }
    #endregion
    #region InputText
    public class GoDataGridInputTextCell : GoDataGridCell
    {
        #region Properties
        #endregion

        #region Member Variable
        bool bDown = false;
        #endregion

        #region Constructor
        public GoDataGridInputTextCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridCheckBoxColumn col)
            {
            }
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var br = GoTheme.Current.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = GoInputEventer.Current.InputControl == Grid && Grid.InputObject == this ? thm.Hignlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            var text = Value is string s ? s : null;
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, rt, cText);
        }
        #endregion

        #region Mouse
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (CollisionTool.Check(rt, x, y) && Value is string v) Grid?.InvokeEditText(this, v);

            base.OnMouseClick(x, y, button);
        }
        #endregion
    }

    public class GoDataGridInputTextColumn : GoDataGridColumn
    {
        #region Properties
        #endregion 

        #region Constructor
        public GoDataGridInputTextColumn()
        {
            CellType = typeof(GoDataGridInputTextCell);
        }
        #endregion
    }
    #endregion
    #region InputNumber
    public class GoDataGridInputNumberCell<T> : GoDataGridCell where T : struct
    {
        #region Properties
        public T? Minimum { get; set; }
        public T? Maximum { get; set; }
        public string? FormatString { get; set; }
        #endregion

        #region Member Variable
        bool bDown = false;
        #endregion

        #region Constructor
        public GoDataGridInputNumberCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridInputNumberColumn<T> col)
            {
                this.Minimum = col.Minimum;
                this.Maximum = col.Maximum;
                this.FormatString = col.FormatString;
            }
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var br = GoTheme.Current.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = GoInputEventer.Current.InputControl == Grid && Grid.InputObject == this ? thm.Hignlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            var text = Value is T val ? ValueTool.ToString<T>(val, FormatString) : null;
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, rt, cText);
        }
        #endregion

        #region Mouse
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (CollisionTool.Check(rt, x, y) && Value is T v && Column is GoDataGridInputNumberColumn<T> col) Grid?.InvokeEditNumber<T>(this, v, col.Minimum, col.Maximum);
            base.OnMouseClick(x, y, button);
        }
        #endregion
    }

    public class GoDataGridInputNumberColumn<T>: GoDataGridColumn where T : struct
    {
        #region Properties
        public T? Minimum { get; set; }
        public T? Maximum { get; set; }
        public string? FormatString { get; set; }
        #endregion 

        #region Constructor
        public GoDataGridInputNumberColumn()
        {
            CellType = typeof(GoDataGridInputNumberCell<T>);
        }
        #endregion
    }
    #endregion
    #region InputBool
    public class GoDataGridInputBoolCell : GoDataGridCell
    {
        #region Properties
        public string? OnText { get; set; } = "On";
        public string? OffText { get; set; } = "Off";
        #endregion 

        #region Constructor
        public GoDataGridInputBoolCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridInputBoolColumn col)
            {
                this.OnText = col.OnText;
                this.OffText = col.OffText;
            }
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var br = GoTheme.Current.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = GoInputEventer.Current.InputControl == Grid && Grid.InputObject == this ? thm.Hignlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            var rts = Util.Columns(rt, ["50%", "50%"]);
            var rtOff = rts[0];
            var rtOn = rts[1];
            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            if (Value is bool v)
            {
                var cTextL = cText;
                var cTextD = Util.FromArgb(80, cTextL);
                Util.DrawText(canvas, OnText, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtOn, v ? cTextL : cTextD);
                Util.DrawText(canvas, OffText, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtOff, v ? cTextD : cTextL);

                using var p = new SKPaint { IsAntialias = false };
                using var pe = SKPathEffect.CreateDash([2, 2], 2);
                p.StrokeWidth = 1;
                p.IsStroke = false;
                p.Color = Util.FromArgb(128, cB);
                p.PathEffect = pe;

                canvas.DrawLine(rt.MidX, rt.Top + 5, rt.MidX, rt.Bottom - 5, p);
            }
        }
        #endregion

        #region Mouse
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            var rts = Util.Columns(rt, ["50%", "50%"]);
            var rtOff = rts[0];
            var rtOn = rts[1];

            if (Value is bool v)
            {
                var old = Value;
                if (CollisionTool.Check(rtOn, x, y) && !v) { Value = true; Grid?.InvokeValueChange(this, old, Value); }
                if (CollisionTool.Check(rtOff, x, y) && v) { Value = false; Grid?.InvokeValueChange(this, old, Value); }
            }
            base.OnMouseClick(x, y, button);
        }
        #endregion
    }

    public class GoDataGridInputBoolColumn : GoDataGridColumn
    {
        #region Properties
        public string? OnText { get; set; } = "On";
        public string? OffText { get; set; } = "Off";
        #endregion 

        #region Constructor
        public GoDataGridInputBoolColumn()
        {
            CellType = typeof(GoDataGridInputBoolCell);
        }
        #endregion
    }
    #endregion
    //InputTime
    //Inputcolor
    //InputCombo
    #endregion

    #region classes : EventArgs
    public class GoDataGridCellButtonClickEventArgs(GoDataGridCell cell) : EventArgs
    {
        public GoDataGridCell Cell { get; private set; } = cell;
    }

    public class GoDataGridColumnMouseEventArgs(GoDataGridColumn column) : EventArgs
    {
        public GoDataGridColumn Column { get; private set; } = column;
    }

    public class GoDataGridCellMouseEventArgs(GoDataGridCell cell) : EventArgs
    {
        public GoDataGridCell Cell { get; private set; } = cell;
    }

    public class GoDataGridCellValueChangedEventArgs(GoDataGridCell cell, object? oldValue, object? newValue) : EventArgs
    {
        public GoDataGridCell Cell { get; private set; } = cell;
        public object? OldValue { get; private set; } = oldValue;
        public object? NewValue { get; private set; } = newValue;
    }

    public class GoDataGridRowDoubleClickEventArgs(GoDataGridRow row) : EventArgs
    {
        public GoDataGridRow Row { get; private set; } = row;
    }
    #endregion
}
