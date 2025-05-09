using Going.UI.Collections;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
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
        [GoProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 3)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 4)] public string RowColor { get; set; } = "Base2";
        [GoProperty(PCategory.Control, 5)] public string SummaryRowColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control, 6)] public string ColumnColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control, 7)] public string SelectedRowColor { get; set; } = "Select";
        [GoProperty(PCategory.Control, 8)] public string BoxColor { get; set; } = "Base2";

        [GoProperty(PCategory.Control, 9)] public float RowHeight { get; set; } = 30F;
        [GoProperty(PCategory.Control, 10)] public float ColumnHeight { get; set; } = 30F;

        [GoProperty(PCategory.Control, 11)] public ScrollMode ScrollMode { get; set; } = ScrollMode.Vertical;
        [GoProperty(PCategory.Control, 12)] public GoDataGridSelectionMode SelectionMode { get; set; } = GoDataGridSelectionMode.Single;

        public ObservableList<GoDataGridColumn> ColumnGroups { get; private set; } = [];
        public ObservableList<GoDataGridColumn> Columns { get; private set; } = [];
        public List<GoDataGridSummaryRow> SummaryRows { get; private set; } = [];

        [JsonIgnore] public List<GoDataGridRow> Rows { get; private set; } = [];
        [JsonIgnore] internal Type? DataType { get; private set; }
        [JsonIgnore] public object? InputObject { get; internal set; }
        [JsonIgnore] public List<GoDataGridRow> ViewRows => mrows;
        #endregion

        #region Member Variable
        private SKRect pBound;
        private Scroll hscroll = new Scroll() { Direction = ScrollDirection.Horizon };
        private Scroll vscroll = new Scroll() { Direction = ScrollDirection.Vertical };

        private List<GoDataGridRow> mrows = [];
        private object? objs = null;

        private double hsV, vsV;
        private bool bShift, bControl;
        private GoDataGridRow? first;

        private GoDateTimeDropDownWindow dwndTime = new GoDateTimeDropDownWindow();
        private GoDataGridCell? dwndTimeCell;

        private GoColorDropDownWindow dwndColor = new GoColorDropDownWindow();
        private GoDataGridCell? dwndColorCell;

        private GoComboBoxDropDownWindow dwndCombo = new GoComboBoxDropDownWindow();
        private GoDataGridCell? dwndComboCell;
        #endregion

        #region Event
        public event EventHandler? SelectedChanged;
        public event EventHandler? SortChanged;

        public event EventHandler<GoDataGridColumnMouseEventArgs>? ColumnMouseClick;
        public event EventHandler<GoDataGridCellMouseEventArgs>? CellMouseClick;
        public event EventHandler<GoDataGridCellMouseEventArgs>? CellMouseDoubleClick;
        public event EventHandler<GoDataGridCellMouseEventArgs>? CellMouseLongClick;
        public event EventHandler<GoDataGridCellButtonClickEventArgs>? CellButtonClick;
        public event EventHandler<GoDataGridCellValueChangedEventArgs>? ValueChanged;

        public event EventHandler<GoDataGridDateTimeDropDownOpeningEventArgs>? DateTimeDropDownOpening;
        public event EventHandler<GoDataGridColorDropDownOpeningEventArgs>? ColorDropDownOpening;
        public event EventHandler<GoDataGridComboDropDownOpeningEventArgs>? ComboDropDownOpening;
        public event Func<GoDataGridCell, bool>? GetDateTimeDropDownVisible;
        public event Func<GoDataGridCell, bool>? GetColorDropDownVisible;
        public event Func<GoDataGridCell, bool>? GetComboDropDownVisible;
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
            using var p = new SKPaint { IsAntialias = true, };
            using var pe = SKPathEffect.CreateDash([2, 2,], 3);

            #region color
            var cText = thm.ToColor(TextColor);
            var cRow = thm.ToColor(RowColor);
            var cSum = thm.ToColor(SummaryRowColor);
            var cCol = thm.ToColor(ColumnColor);
            var cSel = thm.ToColor(SelectedRowColor);
            var cBox = thm.ToColor(BoxColor);
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
            #region etc
            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);
            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
            var cr = rtColumn.Right;
            var vsx = cl - hspos;
            var vex = vsx + (cr - cl);
            var br = thm.Dark ? 1F : -1F;
            #endregion
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

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(Util.FromRect(rtSummary.Left, rtSummary.Top, rtSummary.Width + 1, rtSummary.Height + 1));
                    canvas.Translate(0, rtSummary.Top);

                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(new SKRect(cl, 0, cr + 1, rtSummary.Height));
                        canvas.Translate(rtSummary.Left + Convert.ToInt64(hspos), 0);

                        foreach (var r in SummaryRows)
                            foreach (var c in r.Cells.Where(x => !x.Column.Fixed && x.Bounds.Right > vsx && x.Bounds.Left < vex))
                                if (c.Visible) c.Draw(canvas);
                    }

                    var bsel = SelectionMode == GoDataGridSelectionMode.Selector;
                    p.IsStroke = true; p.StrokeWidth = 1;
                    p.PathEffect = pe;
                    foreach (var r in SummaryRows)
                    {
                        foreach (var c in r.Cells.Where(x => x.Column.Fixed))
                            if (c.Visible) c.Draw(canvas);

                        #region Selector
                        if (bsel)
                        {
                            var rtSel = Util.FromRect(r.Bounds.Left, r.Bounds.Top, 30, r.Bounds.Height);

                            var cF = cSum;
                            var cV = cF.BrightnessTransmit(r.RowIndex % 2 == 0 ? 0.05F : -0.05F);
                            var cB = cV.BrightnessTransmit(BorderBright * br);

                            var rx = Convert.ToInt32(rtSel.Right) + 0.5F;
                            p.Color = cB;
                            canvas.DrawLine(rx, rtSel.Top, rx, rtSel.Bottom, p);
                        }
                        #endregion
                    }
                    p.PathEffect = null;
                }
            }
            #endregion
            #region Border
            Util.DrawBox(canvas, rtColumn, SKColors.Transparent, cCol, GoRoundType.T, thm.Corner);
            if (SummaryRows.Count > 0) Util.DrawBox(canvas, rtSummary, SKColors.Transparent, cSum, GoRoundType.B, thm.Corner);
            #endregion
            #region Rows
            {
                Util.DrawBox(canvas, rtRow, cBox, cRow, SummaryRows.Count > 0 ? GoRoundType.Rect : GoRoundType.B, thm.Corner);

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(Util.FromRect(rtRow.Left, rtRow.Top, rtRow.Width + 1, rtRow.Height + 1));
                    canvas.Translate(0, rtRow.Top + Convert.ToInt32(vspos));

                    var rt = Util.FromRect(0, 0, rtRow.Width, rtRow.Height);
                    rt.Offset(0, -Convert.ToSingle(vscroll.ScrollPositionWithOffset));
                    var (si, ei) = Util.FindRect(mrows.Select(x => x.Bounds).ToList(), rt);

                    if (si >= 0 && si < mrows.Count && ei >= 0 && ei < mrows.Count)
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
                        p.IsStroke = true; p.StrokeWidth = 1;
                        p.PathEffect = pe;
                        for (int i = si; i <= ei; i++)
                        {
                            var r = mrows[i];
                            foreach (var c in r.Cells.Where(x => x.Column.Fixed))
                                if (c.Visible) c.Draw(canvas);

                            #region Selector
                            if (bsel)
                            {
                                var rtSel = Util.FromRect(r.Bounds.Left, r.Bounds.Top, 30, r.Bounds.Height);
                                var rtChk = MathTool.MakeRectangle(rtSel, new SKSize(20, 20));

                                var cF = r.Selected ? cSel : cRow;
                                var cV = cF.BrightnessTransmit(r.RowIndex % 2 == 0 ? 0.05F : -0.05F);
                                var cB = cV.BrightnessTransmit(BorderBright * br);

                                Util.DrawBox(canvas, rtSel, cV, GoRoundType.Rect, thm.Corner);
                                Util.DrawBox(canvas, rtChk, cV.BrightnessTransmit(InputBright * br), cB, GoRoundType.Rect, thm.Corner);
                                if (r.Selected) Util.DrawIcon(canvas, "fa-check", 12, rtChk, cText);

                                var rx = Convert.ToInt32(rtSel.Right) + 0.5F;
                                p.Color = cB;
                                canvas.DrawLine(rx, rtSel.Top, rx, rtSel.Bottom, p);
                            }
                            #endregion
                        }
                        p.PathEffect = null;
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
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

            var rx = x - rtColumn.Left - hspos;
            var ry = y - rtRow.Top - vspos;

            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
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
            }
            #endregion
            #region Rows
            {
                rowsLoop(rtRow, (i, row) =>
                {
                    if (x < cl)
                    {
                        foreach (var c in row.Cells.Where(x => x.Column.Fixed)) c.MouseDown(x, ry, button);
                    }
                    else
                    {
                        foreach (var c in row.Cells.Where(x => !x.Column.Fixed)) c.MouseDown(rx, ry, button);
                    }
                });
            }
            #endregion

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

            var rx = x - rtColumn.Left - hspos;
            var ry = y - rtRow.Top - vspos;

            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
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

            #region Column
            {
                if (x < cl)
                {
                    foreach (var c in Columns.Where(x => x.Fixed)) c.MouseUp(x, y, button);
                    foreach (var c in ColumnGroups.Where(x => x.Fixed)) c.MouseUp(x, y, button);
                }
                else
                {
                    foreach (var c in Columns.Where(x => !x.Fixed)) c.MouseUp(rx, y, button);
                    foreach (var c in ColumnGroups.Where(x => !x.Fixed)) c.MouseUp(rx, y, button);
                }
            }
            #endregion
            #region Rows
            {
                rowsLoop(rtRow, (i, row) =>
                {
                    if (x < cl)
                    {
                        foreach (var c in row.Cells.Where(x => x.Column.Fixed)) c.MouseUp(x, ry, button);
                    }
                    else
                    {
                        foreach (var c in row.Cells.Where(x => !x.Column.Fixed)) c.MouseUp(rx, ry, button);
                    }
                });
            }
            #endregion

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            #region var
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

            var rx = x - rtColumn.Left - hspos;
            var ry = y - rtRow.Top - vspos;

            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
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

            #region Column
            {
                if (x < cl)
                {
                    foreach (var c in Columns.Where(x => x.Fixed)) c.MouseMove(x, y);
                    foreach (var c in ColumnGroups.Where(x => x.Fixed)) c.MouseMove(x, y);
                }
                else
                {
                    foreach (var c in Columns.Where(x => !x.Fixed)) c.MouseMove(rx, y);
                    foreach (var c in ColumnGroups.Where(x => !x.Fixed)) c.MouseMove(rx, y);
                }
            }
            #endregion
            #region Rows
            {
                rowsLoop(rtRow, (i, row) =>
                {
                    if (x < cl)
                    {
                        foreach (var c in row.Cells.Where(x => x.Column.Fixed)) c.MouseMove(x, ry);
                    }
                    else
                    {
                        foreach (var c in row.Cells.Where(x => !x.Column.Fixed)) c.MouseMove(rx, ry);
                    }
                });
            }
            #endregion

            base.OnMouseMove(x, y);
        }

        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

            var rx = x - rtColumn.Left - hspos;
            var ry = y - rtRow.Top - vspos;

            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
            #endregion

            #region Column
            {
                if (SelectionMode == GoDataGridSelectionMode.Selector)
                {
                    var rtChk = MathTool.MakeRectangle(Util.FromRect(rtColumn.Left, rtColumn.Top, 30, rtColumn.Height), new SKSize(20, 20));
                    if (CollisionTool.Check(rtChk, x, y))
                    {
                        bool val = mrows.Any(x => x.Selected);
                        foreach (var v in mrows) v.Selected = !val;
                    }
                }

                if (x < cl)
                {
                    foreach (var c in Columns.Where(x => x.Fixed)) if (CollisionTool.Check(c.Bounds, x, y)) ColumnMouseClick?.Invoke(this, new(c));
                    foreach (var c in ColumnGroups.Where(x => x.Fixed)) if (CollisionTool.Check(c.Bounds, x, y)) ColumnMouseClick?.Invoke(this, new(c));
                }
                else
                {
                    foreach (var c in Columns.Where(x => !x.Fixed)) if (CollisionTool.Check(c.Bounds, rx, y)) ColumnMouseClick?.Invoke(this, new(c));
                    foreach (var c in ColumnGroups.Where(x => !x.Fixed)) if (CollisionTool.Check(c.Bounds, rx, y)) ColumnMouseClick?.Invoke(this, new(c));
                }
            }
            #endregion
            #region Rows
            {
                rowsLoop(rtRow, (i, row) =>
                {
                    if (SelectionMode == GoDataGridSelectionMode.Selector)
                    {
                        var rtSel = Util.FromRect(row.Bounds.Left, row.Bounds.Top, 30, row.Bounds.Height);
                        var rtChk = MathTool.MakeRectangle(rtSel, new SKSize(20, 20));

                        if (CollisionTool.Check(rtChk, x, ry))
                        {
                            row.Selected = !row.Selected;
                            SelectedChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else if (CollisionTool.Check(row.Bounds, x, ry))
                    {
                        select(row);
                    }

                    if (x < cl)
                    {
                        foreach (var c in row.Cells.Where(x => x.Column.Fixed))
                        {
                            c.MouseClick(x, ry, button);
                            if (CollisionTool.Check(c.Bounds, x, ry)) CellMouseClick?.Invoke(this, new(c));
                        }
                    }
                    else
                    {
                        foreach (var c in row.Cells.Where(x => !x.Column.Fixed))
                        {
                            c.MouseClick(rx, ry, button);
                            if (CollisionTool.Check(c.Bounds, rx, ry)) CellMouseClick?.Invoke(this, new(c));
                        }
                    }
                });
            }
            #endregion
             
            base.OnMouseClick(x, y, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

            var rx = x - rtColumn.Left - hspos;
            var ry = y - rtRow.Top - vspos;

            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
            #endregion

            #region Rows
            {
                rowsLoop(rtRow, (i, row) =>
                {
                    if (x < cl)
                    {
                        foreach (var c in row.Cells.Where(x => x.Column.Fixed)) if (CollisionTool.Check(c.Bounds, x, ry)) CellMouseDoubleClick?.Invoke(this, new(c));
                    }
                    else
                    {
                        foreach (var c in row.Cells.Where(x => !x.Column.Fixed)) if (CollisionTool.Check(c.Bounds, rx, ry)) CellMouseDoubleClick?.Invoke(this, new(c));
                    }
                });
            }
            #endregion

            base.OnMouseDoubleClick(x, y, button);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

            var rx = x - rtColumn.Left - hspos;
            var ry = y - rtRow.Top - vspos;

            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
            #endregion

            #region Rows
            {
                rowsLoop(rtRow, (i, row) =>
                {
                    if (x < cl)
                    {
                        foreach (var c in row.Cells.Where(x => x.Column.Fixed)) if (CollisionTool.Check(c.Bounds, x, ry)) CellMouseLongClick?.Invoke(this, new(c));
                    }
                    else
                    {
                        foreach (var c in row.Cells.Where(x => !x.Column.Fixed)) if (CollisionTool.Check(c.Bounds, rx, ry)) CellMouseLongClick?.Invoke(this, new(c));
                    }
                });
            }
            #endregion

            base.OnMouseLongClick(x, y, button);
        }

        protected override void OnMouseWheel(float x, float y, float delta)
        {
            #region var
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var ush = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Horizon;
            var usv = ScrollMode == ScrollMode.Both || ScrollMode == ScrollMode.Vertical;

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

            var rx = x - rtColumn.Left - hspos;
            var ry = y - rtRow.Top - vspos;

            var cl = Columns.FirstOrDefault(x => !x.Fixed)?.Bounds.Left ?? 0;
            #endregion

            #region Wheel
            if (CollisionTool.Check(rts["Row"], x, y))
            {
                if (usv) vscroll.MouseWheel(x, y, delta);
                else if (ush) hscroll.MouseWheel(x, y, delta);
            }
            #endregion
            base.OnMouseWheel(x, y, delta);
        }
        #endregion

        #region Key
        protected override void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            bShift = Shift;
            bControl = Control;
            base.OnKeyDown(Shift, Control, Alt, key);
        }

        protected override void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            bShift = Shift;
            bControl = Control;
            base.OnKeyUp(Shift, Control, Alt, key);
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

                RefreshRows();
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
        #region Private
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

        #region select
        private void select(GoDataGridRow item)
        {
            #region Single
            if (SelectionMode == GoDataGridSelectionMode.Single)
            {
                Parallel.ForEach(Rows, (v) => v.Selected = false);
                item.Selected = true;
                first = item;
                SelectedChanged?.Invoke(this, EventArgs.Empty);
            }
            #endregion
            #region Multi
            else if (SelectionMode == GoDataGridSelectionMode.Multi)
            {
                item.Selected = !item.Selected;
                SelectedChanged?.Invoke(this, EventArgs.Empty);
            }
            #endregion
            #region MultiPC
            else if (SelectionMode == GoDataGridSelectionMode.MultiPC)
            {
                if (bControl)
                {
                    #region Control
                    item.Selected = !item.Selected;
                    if (item.Selected) first = item;

                    SelectedChanged?.Invoke(this, EventArgs.Empty);
                    #endregion
                }
                else if (bShift)
                {
                    #region Shift
                    if (first == null)
                    {
                        item.Selected = true;
                        SelectedChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        int idx1 = mrows.IndexOf(first);
                        int idx2 = mrows.IndexOf(item);
                        int min = Math.Min(idx1, idx2);
                        int max = Math.Max(idx1, idx2);
                        for (int ii = min; ii <= max; ii++) mrows[ii].Selected = true;

                        SelectedChanged?.Invoke(this, EventArgs.Empty);
                    }
                    #endregion
                }
                else
                {
                    Parallel.ForEach(Rows, (v) => v.Selected = false);
                    item.Selected = true;
                    first = item;
                    SelectedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            #endregion
        }
        #endregion

        #region rowsLoop
        void rowsLoop(SKRect rtRow, Action<int, GoDataGridRow> loop)
        {
            if (mrows.Count > 0)
            {
                var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

                var rt = Util.FromRect(0, 0, rtRow.Width, rtRow.Height);
                rt.Offset(0, -Convert.ToSingle(vscroll.ScrollPositionWithOffset));
                var (si, ei) = Util.FindRect(mrows.Select(x => x.Bounds).ToList(), rt);

                if (si >= 0 && si < mrows.Count && ei >= 0 && ei < mrows.Count)
                {
                    for (int i = si; i <= ei; i++)
                    {
                        var r = mrows[i];

                        loop(i, r);
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Internal
        [EditorBrowsable( EditorBrowsableState.Never), Browsable(false)]
        public void InvokeButtonClick(GoDataGridCell cell) => CellButtonClick?.Invoke(this, new GoDataGridCellButtonClickEventArgs(cell));
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public void InvokeValueChange(GoDataGridCell cell, object? oldValue, object? newValue) => ValueChanged?.Invoke(this, new GoDataGridCellValueChangedEventArgs(cell, oldValue, newValue));
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public void InvokeEditText(GoDataGridCell cell, string? value)
        {
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);
           
            InputObject = cell;
            var rt = cell.Bounds;
            rt.Offset(rtRow.Left + (cell.Column.Fixed ? 0 : hspos), rtRow.Top + vspos);
            GoInputEventer.Current.FireInputString(this, rt, (s) =>
            {
                var old = cell.Value;
                cell.Value = s;
                if ((old != null && !old.Equals(cell.Value)) || old == null) InvokeValueChange(cell, old, cell.Value);

            }, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public void InvokeEditNumber<T>(GoDataGridCell cell, T value, T? min, T? max) where T : struct
        {
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);

            InputObject = cell;
            var rt = cell.Bounds;
            rt.Offset(rtRow.Left + (cell.Column.Fixed ? 0 : hspos), rtRow.Top + vspos);
            GoInputEventer.Current.FireInputNumber(this, rt, (s) =>
            {
                var old = cell.Value;
                cell.Value = ValueTool.FromString<T>(s);
                if ((old != null && !old.Equals(cell.Value)) || old == null) InvokeValueChange(cell, old, cell.Value);

            }, value, min, max);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public void InvokeEditText(GoDataGridCell cell, string? value, SKRect rt, Action<string> act)
        {
            var pt = RowToScreen(cell.Column.Fixed, rt.Left, rt.Top);
            var vrt = Util.FromRect(pt.X, pt.Y, rt.Width, rt.Height);

            InputObject = cell;
            GoInputEventer.Current.FireInputString(this, vrt, act, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public void InvokeEditNumber<T>(GoDataGridCell cell, T value, T? min, T? max, SKRect rt, Action<string> act) where T : struct
        {
            var pt = RowToScreen(cell.Column.Fixed, rt.Left, rt.Top);
            var vrt = Util.FromRect(pt.X, pt.Y, rt.Width, rt.Height);

            InputObject = cell;
            GoInputEventer.Current.FireInputNumber(this, vrt, act, value, min, max);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public SKPoint RowToScreen(bool fix, float x, float y)
        {
            var rts = Areas();
            var rtColumn = rts["Column"];
            var rtRow = rts["Row"];

            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);
            x += rtRow.Left + (fix ? 0 : hspos) + ScreenX;
            y += rtRow.Top + vspos + ScreenY;

            return new SKPoint(x, y);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public bool DateTimeDropDownVisible(GoDataGridCell cell) => GetDateTimeDropDownVisible != null ? GetDateTimeDropDownVisible(cell) : dwndTime.Visible && dwndTimeCell == cell;
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public void DateTimeDropDownOpen(GoDataGridCell cell, SKRect rt, DateTime value, GoDateTimeKind style,  Action<DateTime?> action)
        {
            var pt = RowToScreen(cell.Column.Fixed, rt.Left, rt.Top);
            var vrt = Util.FromRect(pt.X, pt.Y, rt.Width, rt.Height);
            var args = new GoDataGridDateTimeDropDownOpeningEventArgs(cell, vrt, value, style, action);
            DateTimeDropDownOpening?.Invoke(this, args);

            if (!args.Cancel)
            {
                dwndTimeCell = cell;
                dwndTime.Show(vrt, FontName, FontStyle, FontSize, value, style, action);
            }
        }
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public bool ColorDropDownVisible(GoDataGridCell cell) => GetColorDropDownVisible != null ? GetColorDropDownVisible(cell) : dwndColor.Visible && dwndColorCell == cell;
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public void ColorDropDownOpen(GoDataGridCell cell, SKRect rt, SKColor value, Action<SKColor?> action)
        {
            var pt = RowToScreen(cell.Column.Fixed, rt.Left, rt.Top);
            var vrt = Util.FromRect(pt.X, pt.Y, rt.Width, rt.Height);
            var args = new GoDataGridColorDropDownOpeningEventArgs(cell, vrt, value, action);
            ColorDropDownOpening?.Invoke(this, args);

            if (!args.Cancel)
            {
                dwndColorCell = cell;
                dwndColor.Show(vrt, FontName, FontStyle, FontSize, value, action);
            }
        }
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public bool ComboDropDownVisible(GoDataGridCell cell) => GetComboDropDownVisible != null ? GetComboDropDownVisible(cell) : dwndCombo.Visible && dwndComboCell == cell;
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public void ComboDropDownOpen(GoDataGridCell cell, SKRect rt, float itemHeight, int maximumViewCount, List<GoDataGridInputComboItem> items, GoDataGridInputComboItem? selectedItem, Action<GoDataGridInputComboItem?> action)
        {
            var pt = RowToScreen(cell.Column.Fixed, rt.Left, rt.Top);
            var vrt = Util.FromRect(pt.X, pt.Y, rt.Width, rt.Height);
            var args = new GoDataGridComboDropDownOpeningEventArgs(cell, vrt, itemHeight, maximumViewCount, items, selectedItem, action);
            ComboDropDownOpening?.Invoke(this, args);

            if (!args.Cancel)
            {
                dwndComboCell = cell;
                dwndCombo.Show(vrt, FontName, FontStyle, FontSize, itemHeight, maximumViewCount, items.Cast<GoListItem>().ToList(), selectedItem, (v) => { if (v is GoDataGridInputComboItem c) action(c); });
            }
        }
        #endregion

        #region Public
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
                    #region Rows
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
                    #endregion
                    #region Summary
                    {
                        var y = 0F;
                        var ri = 0;
                        foreach (var row in SummaryRows)
                        {
                            row.RowIndex = ri;
                            row.Bounds = Util.FromRect(rtRow.Left, y, rtRow.Width, RowHeight);

                            for (int i = 0; i < Columns.Count; i++)
                            {
                                var col = Columns[i];
                                var tp = col.GetType();
                                if (tp.IsGenericType && (tp.GetGenericTypeDefinition() == typeof(GoDataGridNumberColumn<>) || tp.GetGenericTypeDefinition() == typeof(GoDataGridInputNumberColumn<>)))
                                {
                                    var p = tp.GetProperty("FormatString");
                                    var frmt = p?.GetValue(col) as string;

                                    if (row is GoDataGridSumSummaryRow)
                                    {
                                        var cell = new GoDataGridSumSummaryCell(this, row, col) { FormatString = frmt };
                                        row.Cells.Add(cell);
                                    }
                                    else if (row is GoDataGridAverageSummaryRow)
                                    {
                                        var cell = new GoDataGridAverageSummaryCell(this, row, col) { FormatString = frmt };
                                        row.Cells.Add(cell);
                                    }
                                    else
                                    {
                                        var cell = new GoDataGridLabelSummaryCell(this, row, col);
                                        row.Cells.Add(cell);
                                    }
                                }
                                else
                                {
                                    var cell = new GoDataGridLabelSummaryCell(this, row, col);
                                    row.Cells.Add(cell);
                                }
                            }

                            row.Cells[row.TitleColumnIndex].ColSpan = row.TitleColSpan;
                            for (var i = row.TitleColumnIndex; i < row.TitleColumnIndex + row.TitleColSpan; i++) row.Cells[i].Visible = i == row.TitleColumnIndex;
                            if (row.Cells[row.TitleColumnIndex] is GoDataGridLabelSummaryCell vc) vc.Text = row.Title;

                            y += RowHeight;
                            ri++;
                        }
                    }
                    #endregion
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
                        mrows = mrows.Where(m => (ToString(m.Cells[i]).Contains(cols[i].FilterText ?? "", StringComparison.CurrentCultureIgnoreCase))).ToList();
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
                    v.Calculate();
                    y += RowHeight;
                    ri++;
                }
            }
            #endregion
        }

        string ToString(GoDataGridCell cell)
        {
            var ret = ValueTool.ToString(cell.Value, null) ?? "";

            if (cell is GoDataGridLabelCell c && cell.Column is GoDataGridLabelColumn col)
            {
                if (col.TextConverter != null) ret = col.TextConverter(cell.Value) ?? "";
                else ret = ValueTool.ToString(cell.Value, col.FormatString) ?? "";
            }

            return ret;
        }
        #endregion

        #region InputCell
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void InputCell(GoDataGridCell cell, string s)
        {
            var tp = cell.GetType();
            var rt = Areas()["Row"];
            if (cell is GoDataGridInputTextCell || tp.IsGenericType && (tp.GetGenericTypeDefinition() == typeof(GoDataGridInputNumberCell<>)))
            {
                if (s == "u" && (cell.Bounds.Top + vscroll.ScrollPositionWithOffset) < 0) vscroll.ScrollPosition -= RowHeight;
                else if (s == "d" && (cell.Bounds.Bottom + vscroll.ScrollPositionWithOffset) > rt.Height) vscroll.ScrollPosition += RowHeight;
                else if (s == "l" && (cell.Bounds.Left + hscroll.ScrollPositionWithOffset) < 0) hscroll.ScrollPosition -= cell.Bounds.Width;
                else if (s == "r" && (cell.Bounds.Right + hscroll.ScrollPositionWithOffset) > rt.Width) hscroll.ScrollPosition += cell.Bounds.Width;

                cell.MouseClick(cell.Bounds.MidX, cell.Bounds.MidY, GoMouseButton.Left);
            }
        }
        #endregion
        #endregion
        #endregion
    }

}
