using Going.UI.Collections;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoDataGrid : GoControl
    {
        #region Const
        internal const float InputBright = -0.2F;
        internal const float CheckBoxBright = -0.2F;
        internal const float BorderBright = 0.35F;
        #endregion

        #region Properties
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string RowColor { get; set; } = "Base2";
        public string SummaryRowColor { get; set; } = "Base1";
        public string ColumnColor { get; set; } = "Base1";
        public string SelectedRowColor { get; set; } = "Select";

        public float RowHeight { get; set; } = 30F;
        public float ColumnHeight { get; set; } = 30F;

        public ScrollMode ScrollMode { get; set; } = ScrollMode.Vertical;
        public GoDataGridSelectionMode SelectionMode { get; set; } = GoDataGridSelectionMode.Single;

        public ObservableList<GoDataGridColumn> ColumnGroups { get; private set; } = [];
        public ObservableList<GoDataGridColumn> Columns { get; private set; } = [];
        
        [JsonIgnore] public List<GoDataGridRow> Rows { get; private set; } = [];
        [JsonIgnore] public List<GoDataGridSummaryRow> SummaryRows { get; private set; } = [];
        [JsonIgnore] internal Type? DataType { get; private set; }
        [JsonIgnore] internal object? InputObject { get; set; }
        [JsonIgnore] internal List<GoDataGridRow> ViewRows => mrows;
        #endregion

        #region Member Variable
        private SKRect pBound;
        private Scroll hscroll = new Scroll() { Direction = ScrollDirection.Horizon };
        private Scroll vscroll = new Scroll() { Direction = ScrollDirection.Vertical };

        private List<GoDataGridRow> mrows = [];
        private object? objs = null;

        private double hsV, vsV;
        
        #endregion

        #region Constructor
        public GoDataGrid()
        {
            Selectable = true;

            #region hscroll
            hscroll.GetScrollTotal = () =>
            {
                var l = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
                var r = Columns.LastOrDefault(x => !x.Fixed)?.Bounds.Right ?? 0;
                return r - l;
            };
            hscroll.GetScrollTick = () => 10;
            hscroll.GetScrollView = () => hsV;
            hscroll.Refresh = () => Invalidate?.Invoke();
            #endregion
            #region vscroll
            vscroll.GetScrollTotal = () =>
            {
                var t = mrows.FirstOrDefault()?.Bounds.Top ?? 0;
                var b = mrows.LastOrDefault()?.Bounds.Bottom ?? 0;
                return b - t;
            };
            vscroll.GetScrollTick = () => RowHeight;
            vscroll.GetScrollView = () => vsV;
            vscroll.Refresh = () => Invalidate?.Invoke();
            #endregion
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;
            #region color
            var cText = thm.ToColor(TextColor);
            var cRow = thm.ToColor(RowColor);
            var cSum = thm.ToColor(SummaryRowColor);
            var cCol = thm.ToColor(ColumnColor);
            var cSel = thm.ToColor(SelectedRowColor);
            #endregion
            #region bounds
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];
            var rtSummary = rts["Summary"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];
            var rtScrollContent = rts["ScrollContent"];
            #endregion
            #region scroll
            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;
            #endregion

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);
            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
            var cr = rtColumn.Right;
            var vsx = cl - hspos;
            var vex = vsx + (cr - cl);
            var br = thm.Dark ? 1F : -1F;
            #endregion

            #region Column
            {
                using (var path = PathTool.Box(rtColumn, GoRoundType.T, thm.Corner))
                {
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipPath(path, SKClipOperation.Intersect, true);

                        Util.DrawBox(canvas, rtColumn, cCol, GoRoundType.T, thm.Corner);
                        using (new SKAutoCanvasRestore(canvas))
                        {
                            canvas.ClipRect(new SKRect(cl, rtColumn.Top, cr, rtColumn.Bottom));
                            canvas.Translate(Convert.ToInt64(hspos), 0);
                            foreach (var c in Columns.Where(x => !x.Fixed && x.Bounds.Right > vsx && x.Bounds.Left < vex)) c.Draw(canvas);
                            foreach (var c in ColumnGroups.Where(x => !x.Fixed && x.Bounds.Right > vsx && x.Bounds.Left < vex)) c.Draw(canvas);
                        }

                        foreach (var c in Columns.Where(x => x.Fixed)) c.Draw(canvas);
                        foreach (var c in ColumnGroups.Where(x => x.Fixed)) c.Draw(canvas);

                        if (SelectionMode == GoDataGridSelectionMode.Selector)
                        {
                            bool val = mrows.Any(x => x.Selected);
                            var rtChk = MathTool.MakeRectangle(Util.FromRect(rtColumn.Left, rtColumn.Top, 30, rtColumn.Height), new SKSize(20, 20));

                            var cF = cCol.BrightnessTransmit(CheckBoxBright * br);
                            var cB = cCol.BrightnessTransmit(BorderBright * br);
                            Util.DrawBox(canvas, rtChk, cF, cB, GoRoundType.Rect, thm.Corner);
                            if (val) Util.DrawIcon(canvas, "fa-check", 12, rtChk, cText);
                        }

                    }
                }
            }
            #endregion
            #region Summary
            if (SummaryRows.Count > 0)
            {
                Util.DrawBox(canvas, rtSummary, cSum, GoRoundType.B, thm.Corner);
            }
            #endregion

            #region Border
            Util.DrawBox(canvas, rtColumn, SKColors.Transparent, cRow.BrightnessTransmit(BorderBright * br), GoRoundType.T, thm.Corner);
            Util.DrawBox(canvas, rtSummary, SKColors.Transparent, cRow.BrightnessTransmit(BorderBright * br), GoRoundType.B, thm.Corner);
            #endregion

            #region Rows
            {
                Util.DrawBox(canvas, rtRow, SKColors.Transparent, cRow.BrightnessTransmit(BorderBright * br), SummaryRows.Count > 0 ? GoRoundType.Rect : GoRoundType.B, thm.Corner);

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(Util.FromRect(rtRow.Left, rtRow.Top, rtRow.Width + 1, rtRow.Height + 1));
                    canvas.Translate(0, rtRow.Top + Convert.ToInt32(vspos));

                    var rt = Util.FromRect(0, 0, rtRow.Width, rtRow.Height);
                    rt.Offset(0, -Convert.ToSingle(vscroll.ScrollPositionWithOffset));
                    var (si, ei) = Util.FindRect(mrows.Select(x => x.Bounds).ToList(), rt);

                    if (si >= 0 && si < mrows.Count && ei > 0 && ei < mrows.Count)
                    {
                        using (new SKAutoCanvasRestore(canvas))
                        {
                            canvas.ClipRect(new SKRect(cl, rt.Top, cr + 1, rt.Bottom));
                            canvas.Translate(rtRow.Left + Convert.ToInt64(hspos), 0);

                            for (int i = si; i <= ei; i++)
                            {
                                var r = mrows[i];
                                foreach (var c in r.Cells.Where(x => !x.Column.Fixed && x.Bounds.Right > vsx && x.Bounds.Left < vex))
                                    if (c.Visible) c.Draw(canvas);
                            }
                        }

                        var bsel = SelectionMode == GoDataGridSelectionMode.Selector;
                        for (int i = si; i <= ei; i++)
                        {
                            var r = mrows[i];
                            foreach (var c in r.Cells.Where(x => x.Column.Fixed))
                                if (c.Visible) c.Draw(canvas);

                            if (bsel)
                            {
                                var c = r.Selected ? cSel : cRow;
                                var rtSel = Util.FromRect(r.Bounds.Left, r.Bounds.Top, 30, r.Bounds.Height);
                                var rtChk = MathTool.MakeRectangle(rtSel, new SKSize(20, 20));

                                var cF = c.BrightnessTransmit(CheckBoxBright * br);
                                var cB = c.BrightnessTransmit(BorderBright * br);

                                Util.DrawBox(canvas, rtSel, c, cB, GoRoundType.Rect, thm.Corner);
                                Util.DrawBox(canvas, rtChk, cF, cB, GoRoundType.Rect, thm.Corner);
                                if (r.Selected) Util.DrawIcon(canvas, "fa-check", 12, rtChk, cText);
                            }
                        }
                    }
                }

            }
            #endregion


            hscroll.Draw(canvas, rtScrollH);
            vscroll.Draw(canvas, rtScrollV);

            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;
            #endregion

            if (CollisionTool.Check(rts["Content"], x, y))
            {
                #region ScrollH
                if (ush)
                {
                    hscroll.MouseDown(x, y, rts["ScrollH"]);
                    if (hscroll.TouchMode && CollisionTool.Check(rts["Row"], x, y)) hscroll.TouchDown(x, y);
                }
                #endregion
                #region ScrollV
                if (usv)
                {
                    vscroll.MouseDown(x, y, rts["ScrollV"]);
                    if (vscroll.TouchMode && CollisionTool.Check(rts["Row"], x, y)) vscroll.TouchDown(x, y);
                }
                #endregion
            }

            #region Column
            {
                var rtColumn = rts["Column"];
                var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
                var rx = x - rtColumn.Left - hspos;
                var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;

                if (x < cl)
                {
                    foreach (var c in Columns.Where(x => x.Fixed)) c.MouseDown(x, y, button);
                    foreach (var c in ColumnGroups.Where(x => x.Fixed)) c.MouseDown(x, y, button);
                }
                else
                {
                    foreach (var c in Columns.Where(x => !x.Fixed)) c.MouseDown(rx, y, button);
                    foreach (var c in ColumnGroups.Where(x => !x.Fixed)) c.MouseDown(rx, y, button);
                }

                if (SelectionMode == GoDataGridSelectionMode.Selector)
                {
                    var rtChk = MathTool.MakeRectangle(Util.FromRect(rtColumn.Left, rtColumn.Top, 30, rtColumn.Height), new SKSize(20, 20));
                    if (CollisionTool.Check(rtChk, x, y))
                    {
                        bool val = mrows.Any(x => x.Selected);
                        foreach (var v in mrows) v.Selected = !val;
                    }
                }
            }
            #endregion
            #region Rows
            if (mrows.Count > 0)
            {
                var rtRow = rts["Row"];
                var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);
                var ry = y - rtRow.Top - vspos;

                var rt = Util.FromRect(0, 0, rtRow.Width, rtRow.Height);
                rt.Offset(0, -Convert.ToSingle(vscroll.ScrollPositionWithOffset));
                var (si, ei) = Util.FindRect(mrows.Select(x => x.Bounds).ToList(), rt);

                if (si >= 0 && si < mrows.Count && ei > 0 && ei < mrows.Count)
                {
                    var bsel = SelectionMode == GoDataGridSelectionMode.Selector;
                    for (int i = si; i <= ei; i++)
                    {
                        var r = mrows[i];

                        if (bsel)
                        {
                            var rtSel = Util.FromRect(r.Bounds.Left, r.Bounds.Top, 30, r.Bounds.Height);
                            var rtChk = MathTool.MakeRectangle(rtSel, new SKSize(20, 20));

                            if (CollisionTool.Check(rtChk, x, ry)) r.Selected = !r.Selected;
                        }
                    }
                }
            }
            #endregion

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;
            #endregion

            #region ScrollH
            if (ush)
            {
                hscroll.MouseUp(x, y);
                if (hscroll.TouchMode) hscroll.TouchUp(x, y);
            }
            #endregion
            #region ScrollV
            if (usv)
            {
                vscroll.MouseUp(x, y);
                if (vscroll.TouchMode) vscroll.TouchUp(x, y);
            }
            #endregion

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            #region var
            var rts = Areas();
            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;
            #endregion

            #region ScrollH
            if (ush)
            {
                hscroll.MouseMove(x, y, rts["ScrollH"]);
                if (hscroll.TouchMode) hscroll.TouchMove(x, y);
            }
            #endregion
            #region ScrollV
            if (usv)
            {
                vscroll.MouseMove(x, y, rts["ScrollV"]);
                if (vscroll.TouchMode) vscroll.TouchMove(x, y);
            }
            #endregion

            base.OnMouseMove(x, y);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            #region columns
            var dicCols = new Dictionary<GoDataGridColumn, List<GoDataGridColumn>>();
            foreach (var col in Columns)
            {
                var ls = ColumnTree(col);
                dicCols.Add(col, ls);

                for (int i = 0; i < ls.Count; i++) ls[i].Depth = i;
            }
            var colC = dicCols.Count > 0 ? dicCols.Values.Max(x => x.Count) : 0;
            var colH = (colC + (Columns.Where(x => x.UseFilter).Count() > 0 ? 1 : 0)) * ColumnHeight;
            #endregion
            #region summary
            var sumH = SummaryRows.Count * RowHeight;
            #endregion
            #region scroll
            var scwh = Convert.ToInt32(Scroll.SC_WH);

            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;
            #endregion

            var rtScrollContent = Util.FromRect(rtContent.Left, rtContent.Top, rtContent.Width - (usv ? scwh : 0), rtContent.Height - (ush ? scwh : 0));
            var rts = Util.Rows(rtScrollContent, [$"{colH}px", $"100%", $"{sumH}px"]);
            var rtColumn = rts[0];
            var rtRow = rts[1];
            var rtSummary = rts[2];
            var rtScrollV = Util.FromRect(rtRow.Right, rtRow.Top, usv ? scwh : 0, rtRow.Height);
            var tl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? rtSummary.Left;
            var rtScrollH = new SKRect(tl, rtSummary.Bottom, rtSummary.Right, rtSummary.Bottom + (ush ? scwh : 0));
            
            dic["Column"] = rtColumn;
            dic["Row"] = rtRow;
            dic["Summary"] = rtSummary;
            dic["ScrollV"] = rtScrollV;
            dic["ScrollH"] = rtScrollH;
            dic["ScrollContent"] = rtScrollContent;

            #region bounds
            if (Columns.Changed || ColumnGroups.Changed || !pBound.Equals(rtContent))
            {
                var uf = Columns.Any(x => x.UseFilter);
                var bsel = SelectionMode == GoDataGridSelectionMode.Selector;
                var rtcol = Util.FromRect(rtColumn.Left + (bsel ? 30 : 0), rtColumn.Bottom - ColumnHeight - (uf ? ColumnHeight : 0), rtColumn.Width - (bsel ? 30 : 0), ColumnHeight);
                var rtsCol = Util.Columns(rtcol, Columns.Select(x => x.Size ?? "").ToArray());
                
                for (int i = 0; i < Columns.Count; i++)
                {
                    var col = Columns[i];
                    var rt = rtsCol[i];
                    var rtFilter = Util.FromRect(rt.Left, rt.Bottom, rt.Width, uf ? ColumnHeight : 0);

                    
                    col.Grid = this;
                    col.Bounds = rt;
                    col.FilterBounds = rtFilter;
                }

                foreach (var col in ColumnGroups)
                {
                    var ls = ColumnChilds(col);

                    var hasParent = ColumnGroups.Any(x => x.Name == col.GroupName);
                    var rows = !hasParent ? colC - col.Depth : 1;

                    var l = ls.Min(x => x.Bounds.Left);
                    var r = ls.Max(x => x.Bounds.Right);
                    var b = rtColumn.Bottom - (uf ? ColumnHeight : 0) - (col.Depth * RowHeight);
                    var t = b - (rows * ColumnHeight);

                    col.Grid = this; 
                    col.Bounds = new SKRect(l, t, r, b);
                    col.FilterBounds = new SKRect(l, b, r, b);
                }

                Columns.Changed = ColumnGroups.Changed = false;
                pBound = rtContent;
            }
            #endregion

            #region Scroll View / Total
            hsV = rtScrollH.Width;
            vsV = rtRow.Height;
            #endregion

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region GetColumnRowCount
        private int GetColumnRowCount()
        {
            int ret = 0;
            foreach (var col in Columns)
            {
                var ls = ColumnTree(col);
                ret = Math.Max(ls.Count, ret);
            }
            return ret;
        }
        #endregion
        #region ColumnTree
        List<GoDataGridColumn> ColumnTree(GoDataGridColumn col)
        {
            List<GoDataGridColumn> ret = [];
            ret.Add(col);

            if (col.GroupName != null)
            {
                var v = ColumnGroups.Where(x => x.Name == col.GroupName).FirstOrDefault();
                if (v != null) ret.AddRange(ColumnTree(v));
            }

            return ret;
        }
        #endregion
        #region ColumnChilds
        List<GoDataGridColumn> ColumnChilds(GoDataGridColumn col)
        {
            List<GoDataGridColumn> ret = [];

            if (col != null)
            {
                if (Columns.Contains(col)) { ret.Add(col); }
                else
                {
                    if (Columns.Count(x => x.GroupName == col.Name) > 0) ret.AddRange(Columns.Where(x => x.GroupName == col.Name));
                    else if (ColumnGroups.Count(x => x.GroupName == col.Name) > 0)
                    {
                        var vls = ColumnGroups.Where(x => x.GroupName == col.Name);
                        foreach (var v in vls) ret.AddRange(ColumnChilds(v));
                    }
                }
            }

            return ret;
        }
        #endregion

        #region SetDataSource<T>
        public void SetDataSource<T>(IEnumerable<T> values)
        {
            #region bounds
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];
            var rtSummary = rts["Summary"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];
            #endregion

            objs = values;
            DataType = typeof(T);
            var props = typeof(T).GetProperties();
            int nCnt = Columns.Where(x => props.Select(v => v.Name).Contains(x.Name) || x is GoDataGridButtonColumn).Count();
            if (nCnt == Columns.Count)
            {
                var dic = props.ToDictionary(x => x.Name);
                Rows.Clear();
                if (values != null)
                {
                    float y = 0;
                    int ri = 0;
                    foreach (var src in values)
                    {
                        var row = new GoDataGridRow { Grid = this, Source = src, RowIndex = ri, RowHeight = RowHeight, Bounds = Util.FromRect(rtRow.Left, y, rtRow.Width, RowHeight) };

                        for (int i = 0; i < Columns.Count; i++)
                        {
                            var col = Columns[i];

                            if (col is GoDataGridButtonColumn colBtn)
                            {
                                var cell = new GoDataGridButtonCell(this, row, (GoDataGridButtonColumn)col);
                                row.Cells.Add(cell);
                            }
                            else
                            {
                                if (col.Name != null && col.CellType != null)
                                {
                                    var prop = dic[col.Name];
                                    var cell = Activator.CreateInstance(col.CellType, this, row, col) as GoDataGridCell;
                                    if (cell != null) row.Cells.Add(cell);
                                }
                            }
                        }

                        Rows.Add(row);

                        y += row.RowHeight;
                        ri++;
                    }
                }

                RefreshRows();
            }
            else throw new Exception("VALID COUNT");
        }
        #endregion

        #region RefreshRows
        public void RefreshRows()
        {
            #region bounds
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];
            var rtSummary = rts["Summary"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];
            #endregion

            mrows.Clear();
            mrows.AddRange(Rows);

            #region Sort
            {
                var cols = Columns.Where(x => x.UseSort).ToList();
                var ls = cols.Where(x => x.UseSort).OrderBy(x => x.SortOrder);
                if (ls.Count() > 0)
                {
                    IOrderedEnumerable<GoDataGridRow>? result = null;
                    bool bFirst = true;
                    foreach (var col in ls)
                    {
                        int i = Columns.IndexOf(col);
                        switch (col.SortState)
                        {
                            case GoDataGridColumnSortState.Asc:
                                if (bFirst) { result = mrows.OrderBy(x => x.Cells[i].Value); bFirst = false; }
                                else if (result != null)
                                {
                                    result = result.ThenBy(x => x.Cells[i].Value);
                                }
                                break;
                            case GoDataGridColumnSortState.Desc:
                                if (bFirst) { result = mrows.OrderByDescending(x => x.Cells[i].Value); bFirst = false; }
                                else if (result != null)
                                {
                                    result = result.ThenByDescending(x => x.Cells[i].Value);
                                }
                                break;
                        }
                    }

                    if (result != null)
                    {
                        var l = result.ToList();
                        mrows.Clear();
                        mrows.AddRange(l);
                    }
                }
            }
            #endregion
            #region Filter
            {
                var cols = Columns.ToList();
                for (int i = 0; i < cols.Count; i++)
                {
                    if (cols[i].UseFilter && !string.IsNullOrWhiteSpace(cols[i].FilterText))
                    {
                        mrows = mrows.Where(m => ((m.Cells[i].Value?.ToString()?.ToLower() ?? "").IndexOf((cols[i].FilterText ?? "").ToLower()) != -1)).ToList();
                    }
                }
            }
            #endregion
            #region Rows
            {
                var y = 0F;
                int ri = 0;
                foreach (var v in mrows)
                {
                    v.RowIndex = ri;
                    v.Bounds = Util.FromRect(rtRow.Left, y, rtRow.Width, RowHeight);

                    y += v.RowHeight;
                    ri++;
                }
            }
            #endregion
            #region Summary
            {
                var y = 0F;
                var ri = 0;
                foreach (var v in SummaryRows)
                {
                    v.RowIndex = ri;
                    v.Bounds = Util.FromRect(rtRow.Left, y, rtRow.Width, RowHeight);

                    foreach (var c in v.Cells.Where(x => x is GoDataGridSummaryCell))
                        if (c is GoDataGridSummaryCell cell)
                            cell.Calculate();

                    y += RowHeight;
                    ri++;
                }
            }
            #endregion
        }
        #endregion
        #endregion
    }

}
