using Going.UI.Forms.Containers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel;
using static OpenTK.Graphics.OpenGL.GL;
using Timer = System.Windows.Forms.Timer;

namespace Going.UI.Forms.Containers
{
    public class GoSplitterTableLayoutPanel : GoTableLayoutPanel
    {
        #region Properties
        #region SplitterSize
        public int SplitterSize { get; set; } = 3;
        #endregion
        #region SplitterColor
        private string cSplitterColor = "Red";
        public string SplitterColor
        {
            get { return cSplitterColor; }
            set
            {
                if (cSplitterColor != value)
                {
                    cSplitterColor = value;
                    Invalidate();
                }
            }
        }
        #endregion
        #region Minimum Width / Height
        public int?[]? MinimumColWidths { get; set; }
        public int?[]? MinimumRowHeights { get; set; }
        #endregion
        #endregion

        #region Member Variable
        List<GoSplitter> Splitter = [];

        GoSplitter? downSplitter = null;
        Point downPos = Point.Empty;
        int[]? downcws = null;
        int[]? downrhs = null;
        Point? op1, op2;
        #endregion

        #region Constructor
        public GoSplitterTableLayoutPanel()
        {
            #region Update Style
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            #endregion
        }
        #endregion

        #region Override
        #region OnLayout
        protected override void OnLayout(LayoutEventArgs levent)
        {
            MakeSplitterBounds();
            Invalidate();
            base.OnLayout(levent);
        }
        #endregion
        #region OnResize
        protected override void OnResize(EventArgs eventargs)
        {
            MakeSplitterBounds();
            Invalidate();
            base.OnResize(eventargs);
        }
        #endregion
        #region OnSizeChanged
        protected override void OnSizeChanged(EventArgs e)
        {
            MakeSplitterBounds();
            Invalidate();
            base.OnSizeChanged(e);
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        #endregion
        #region OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            MakeSplitterBounds();
        }
        #endregion

        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            Cursor = Cursors.Default;
            base.OnMouseLeave(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            #region Cursor
            var cur = Cursors.Default;

            if (downSplitter == null)
            {
                foreach (var sp in Splitter)
                {
                    if (sp.Bounds.Contains(e.Location))
                    {
                        if (sp.SplitterType == GoSplitterType.VERTICAL) cur = Cursors.SizeNS;
                        if (sp.SplitterType == GoSplitterType.HORIZON) cur = Cursors.SizeWE;
                    }
                }
            }
            else
            {
                if (downSplitter?.SplitterType == GoSplitterType.VERTICAL) cur = Cursors.SizeNS;
                else if (downSplitter?.SplitterType == GoSplitterType.HORIZON) cur = Cursors.SizeWE;
            }
            this.Cursor = cur;
            #endregion

            if (downSplitter != null)
            {
                var SplitterColor = Util.FromArgb(GoThemeW.Current.ToColor(this.SplitterColor));

                if (downSplitter.SplitterType == GoSplitterType.HORIZON)
                {
                    var ls = ProcessSplitter2(e.Location);
                    var rc = RectangleToScreen(new Rectangle(0, 0, Width, Height));
                    var rts = Util.Columns(Util.FromRect(rc), [.. ls]);
                    var x = Convert.ToInt32(rts[downSplitter.ColIndex].Right);
                    var p1 = new Point(x, rc.Top);
                    var p2 = new Point(x, rc.Bottom);
                    if (op1.HasValue && op2.HasValue) ControlPaint.DrawReversibleLine(op1.Value, op2.Value, SplitterColor);
                    ControlPaint.DrawReversibleLine(p1, p2, Color.Red);

                    op1 = p1;
                    op2 = p2;
                }
                else if(downSplitter?.SplitterType == GoSplitterType.VERTICAL)
                {
                    var ls = ProcessSplitter2(e.Location);
                    var rc = RectangleToScreen(new Rectangle(0, 0, Width, Height));
                    var rts = Util.Rows(Util.FromRect(rc), [.. ls]);
                    var y = Convert.ToInt32(rts[downSplitter.ColIndex].Bottom);
                    var p1 = new Point(rc.Left , y);
                    var p2 = new Point(rc.Right, y);
                    if (op1.HasValue && op2.HasValue) ControlPaint.DrawReversibleLine(op1.Value, op2.Value, SplitterColor);
                    ControlPaint.DrawReversibleLine(p1, p2, Color.Red);

                    op1 = p1;
                    op2 = p2;
                }
            }
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            foreach (var sp in Splitter)
            {
                if (sp.Bounds.Contains(e.Location))
                {
                    downSplitter = sp;
                    downPos = e.Location;
                    downrhs = GetRowHeights();
                    downcws = GetColumnWidths();
                }
            }

            MakeSplitterBounds();
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            ProcessSplitter(e.Location);
            downSplitter = null;
            op1 = op2 = null;
            this.Cursor = Cursors.Default;
            MakeSplitterBounds();
            Refresh();
            base.OnMouseUp(e);
        }
        #endregion
        #endregion

        #region Method
        #region MakeSplitterBounds
        private void MakeSplitterBounds()
        {
            SuspendLayout();
            var lst = new List<GoSplitter>();
            var cols = GetColumnWidths();
            var rows = GetRowHeights();

            if (cols.Length == ColumnStyles.Count && rows.Length == RowStyles.Count)
            {
                int x = 0, y = 0;
                for (int i = 0; i < cols.Length; i++)
                {
                    var w = cols[i];

                    y = 0;
                    for (int j = 0; j < rows.Length; j++)
                    {
                        var h = rows[j];

                        var rtTop = new Rectangle(x, y - (SplitterSize / 2), w, SplitterSize);
                        var rtLeft = new Rectangle(x - (SplitterSize / 2), y, SplitterSize, h);
                        if (j > 0) lst.Add(new GoSplitter(rtTop, GoSplitterType.VERTICAL, i, j - 1));
                        if (i > 0) lst.Add(new GoSplitter(rtLeft, GoSplitterType.HORIZON, i - 1, j));
                        y += h;
                    }
                    x += w;
                }
            }


            Splitter.Clear();
            Splitter.AddRange(lst);

            ResumeLayout();
        }
        #endregion
        #region ProcessSplitter
        private void ProcessSplitter(Point e)
        {
            #region Process
            if (downSplitter != null)
            {
                if (downSplitter.SplitterType == GoSplitterType.VERTICAL && downrhs != null)
                {
                    #region Init
                    var ri = downSplitter.RowIndex;
                    var rowhs = (int[])downrhs.Clone();
                    var rowst = RowStyles[ri];
                    var rowst2 = ri + 1 < RowStyles.Count ? RowStyles[ri + 1] : null;
                    var rowh = rowhs[ri];
                    var rowh2 = ri + 1 < rowhs.Length ? rowhs[ri + 1] : 0;
                    #endregion
                    #region Calc Resize
                    var gapy = (int)MathTool.Constrain((e.Y - downPos.Y), -rowh + (SplitterSize / 2), rowh2 - (SplitterSize / 2));
                    if (MinimumRowHeights != null)
                    {
                        int min1 = MinimumRowHeights.ElementAtOrDefault(ri) ?? 0;
                        int min2 = MinimumRowHeights.ElementAtOrDefault(ri + 1) ?? 0;

                        gapy = Math.Max(gapy, min1 - rowh);
                        gapy = Math.Min(gapy, rowh2 - min2);
                    }
                    rowhs[ri] += gapy;
                    rowhs[ri + 1] -= gapy;
                    #endregion
                    #region ResultData
                    var sum = RowStyles.Cast<RowStyle>().Where(x => x.SizeType == SizeType.Percent).Select(x => rowhs[RowStyles.IndexOf(x)]).Sum();
                    var ls = new List<RowStyle>();
                    for (int i = 0; i < rowhs.Length && i < RowStyles.Count; i++)
                    {
                        var cs = RowStyles[i];
                        if (cs.SizeType == SizeType.Absolute) ls.Add(new RowStyle(SizeType.Absolute, rowhs[i]));
                        else if (cs.SizeType == SizeType.Percent) ls.Add(new RowStyle(SizeType.Percent, (float)rowhs[i] / (float)sum * 100F));
                    }
                    #endregion
                    #region Layout
                    SuspendLayout();
                    RowStyles.Clear();
                    for (int i = 0; i < ls.Count; i++) RowStyles.Add(ls[i]);
                    ResumeLayout();
                    //PerformLayout();
                    #endregion
                }
                else if (downSplitter.SplitterType == GoSplitterType.HORIZON && downcws != null)
                {
                    #region Init
                    var ci = downSplitter.ColIndex;
                    var colws = (int[])downcws.Clone();
                    var colst = ColumnStyles[ci];
                    var colst2 = ci + 1 < ColumnStyles.Count ? ColumnStyles[ci + 1] : null;
                    var colw = colws[ci];
                    var colw2 = ci + 1 < colws.Length ? colws[ci + 1] : 0;
                    #endregion
                    #region Calc Resize
                    var gapx = (int)MathTool.Constrain((e.X - downPos.X), -colw + (SplitterSize / 2), colw2 - (SplitterSize / 2));
                    if (MinimumColWidths != null)
                    {
                        int min1 = MinimumColWidths.ElementAtOrDefault(ci) ?? 0;
                        int min2 = MinimumColWidths.ElementAtOrDefault(ci + 1) ?? 0;

                        gapx = Math.Max(gapx, min1 - colw);
                        gapx = Math.Min(gapx, colw2 - min2);
                    }
                    colws[ci] += gapx;
                    colws[ci + 1] -= gapx;
                    #endregion
                    #region ResultData
                    var sum = ColumnStyles.Cast<ColumnStyle>().Where(x => x.SizeType == SizeType.Percent).Select(x => colws[ColumnStyles.IndexOf(x)]).Sum();
                    var ls = new List<ColumnStyle>();
                    for (int i = 0; i < colws.Length && i < ColumnStyles.Count; i++)
                    {
                        var cs = ColumnStyles[i];
                        if (cs.SizeType == SizeType.Absolute) ls.Add(new ColumnStyle(SizeType.Absolute, colws[i]));
                        else if (cs.SizeType == SizeType.Percent) ls.Add(new ColumnStyle(SizeType.Percent, (float)colws[i] / (float)sum * 100F));
                    }
                    #endregion
                    #region Layout
                    SuspendLayout();
                    ColumnStyles.Clear();
                    for (int i = 0; i < ls.Count; i++) ColumnStyles.Add(ls[i]);
                    var d = ls.Sum(x => x.Width);
                    ResumeLayout();
                    //PerformLayout();
                    #endregion
                }
            }
            #endregion
        }

        private List<string> ProcessSplitter2(Point e)
        {
            var ls = new List<string>();
            #region Process
            if (downSplitter != null)
            {
                if (downSplitter.SplitterType == GoSplitterType.VERTICAL && downrhs != null)
                {
                    #region Init
                    var ri = downSplitter.RowIndex;
                    var rowhs = (int[])downrhs.Clone();
                    var rowst = RowStyles[ri];
                    var rowst2 = ri + 1 < RowStyles.Count ? RowStyles[ri + 1] : null;
                    var rowh = rowhs[ri];
                    var rowh2 = ri + 1 < rowhs.Length ? rowhs[ri + 1] : 0;
                    #endregion
                    #region Calc Resize
                    var gapy = (int)MathTool.Constrain((e.Y - downPos.Y), -rowh + (SplitterSize / 2), rowh2 - (SplitterSize / 2));
                    if (MinimumRowHeights != null)
                    {
                        int min1 = MinimumRowHeights.ElementAtOrDefault(ri) ?? 0;
                        int min2 = MinimumRowHeights.ElementAtOrDefault(ri + 1) ?? 0;

                        gapy = Math.Max(gapy, min1 - rowh);
                        gapy = Math.Min(gapy, rowh2 - min2);
                    }
                    rowhs[ri] += gapy;
                    rowhs[ri + 1] -= gapy;
                    #endregion
                    #region ResultData
                    var sum = RowStyles.Cast<RowStyle>().Where(x => x.SizeType == SizeType.Percent).Select(x => rowhs[RowStyles.IndexOf(x)]).Sum();
                    for (int i = 0; i < rowhs.Length && i < RowStyles.Count; i++)
                    {
                        var cs = RowStyles[i];
                        if (cs.SizeType == SizeType.Absolute) ls.Add($"{rowhs[i]}px");
                        else if (cs.SizeType == SizeType.Percent) ls.Add($"{((float)rowhs[i] / (float)sum * 100F)}%");
                    }
                    #endregion
                }
                else if (downSplitter.SplitterType == GoSplitterType.HORIZON && downcws != null)
                {
                    #region Init
                    var ci = downSplitter.ColIndex;
                    var colws = (int[])downcws.Clone();
                    var colst = ColumnStyles[ci];
                    var colst2 = ci + 1 < ColumnStyles.Count ? ColumnStyles[ci + 1] : null;
                    var colw = colws[ci];
                    var colw2 = ci + 1 < colws.Length ? colws[ci + 1] : 0;
                    #endregion
                    #region Calc Resize
                    var gapx = (int)MathTool.Constrain((e.X - downPos.X), -colw + (SplitterSize / 2), colw2 - (SplitterSize / 2));
                    if (MinimumColWidths != null)
                    {
                        int min1 = MinimumColWidths.ElementAtOrDefault(ci) ?? 0;
                        int min2 = MinimumColWidths.ElementAtOrDefault(ci + 1) ?? 0;

                        gapx = Math.Max(gapx, min1 - colw);
                        gapx = Math.Min(gapx, colw2 - min2);
                    }
                    colws[ci] += gapx;
                    colws[ci + 1] -= gapx;
                    #endregion
                    #region ResultData
                    var sum = ColumnStyles.Cast<ColumnStyle>().Where(x => x.SizeType == SizeType.Percent).Select(x => colws[ColumnStyles.IndexOf(x)]).Sum();
                    for (int i = 0; i < colws.Length && i < ColumnStyles.Count; i++)
                    {
                        var cs = ColumnStyles[i];
                        if (cs.SizeType == SizeType.Absolute) ls.Add($"{colws[i]}px");
                        else if (cs.SizeType == SizeType.Percent) ls.Add($"{((float)colws[i] / (float)sum * 100F)}%");
                    }
                    #endregion
                }
            }
            #endregion
            return ls;
        }
        #endregion
        #endregion
    }

    #region enum : GoSplitterType
    public enum GoSplitterType { VERTICAL, HORIZON }
    #endregion
    #region class : GoSplitter
    public class GoSplitter(Rectangle Bounds, GoSplitterType SplitterType, int ColIndex, int RowIndex)
    {
        public Rectangle Bounds { get; private set; } = Bounds;
        public GoSplitterType SplitterType { get; private set; } = SplitterType;
        public int RowIndex { get; private set; } = RowIndex;
        public int ColIndex { get; private set; } = ColIndex;
    }
    #endregion
}
