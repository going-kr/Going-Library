using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Forms;
using Going.UI.Forms.Controls;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Controls
{
    public class GoContentGrid : GoControl
    {
        #region Properties
        public Size ContentSize { get; set; } = new Size(250, 250);
        public List<GoContentGridItem> SelectedItems { get; } = new List<GoContentGridItem>();
        public List<GoContentGridItem> Items { get; } = new List<GoContentGridItem>();
        public bool MultiSelect { get; set; } = true;
        public int Gap { get; set; } = 3;
        public bool Selectable { get; set; } = true;
        #endregion

        #region Member Variable
        private Scroll scroll = new Scroll { Direction = ScrollDirection.Vertical };
        private List<GoContentGridItem> vls = [];
        private Size szprev;
        private double pcw;
        private float ScrollMax;
        private float mx, my;
        private Point? downPoint = null;
        private Point? movePoint = null;
        #endregion

        #region Event
        public event EventHandler? SelectedChanged;
        public event EventHandler<ItemClickedEventArgs>? ItemDoubleClicked;
        #endregion

        #region Constructor
        public GoContentGrid()
        {
            SetStyle(ControlStyles.Selectable, true);
            UpdateStyles();

            TabStop = true;

            Size = new Size(300, 200);

            scroll.Refresh += () => Invoke(Invalidate);
            scroll.GetScrollTotal = () => ScrollMax;
            scroll.GetScrollTick = () => ContentSize.Height;
            scroll.GetScrollView = () => Height;
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            var thm = GoThemeW.Current;
            var canvas = e.Canvas;

            var SelectedColor = thm.Select;
            var (rtContent, rtBox, rtScroll) = Areas();

            canvas.Clear(Util.FromArgb(BackColor.R, BackColor.G, BackColor.B));

            #region Items
            var sp = canvas.Save();

            canvas.ClipRect(Util.FromRect(rtBox.Left - 1, rtBox.Top - 1, rtBox.Width + 2, rtBox.Height + 2));
            Loop((ls, i, rt, itm) =>
            {
                if (itm.Visible)
                {
                    if (itm.SelectedDraw && itm.Selected)
                    {
                        var rtv = Util.FromRect(rt.Left, rt.Top, rt.Width, rt.Height); rtv.Inflate(Gap+1, Gap+1);

                        using (var p = new SKPaint { IsAntialias = true })
                        {
                            p.IsStroke = false;
                            p.StrokeWidth = 1.5F;
                            p.Color = SelectedColor;
                            canvas.DrawRect(rtv, p);
                        }
                    }

                    itm.Draw(canvas, rt);
                }
            });
            canvas.RestoreToCount(sp);
            #endregion

            #region Scroll
            {
                if (scroll.ScrollVisible) scroll.Draw(canvas, thm, rtScroll);
            }
            #endregion

            #region Drag
            if (Selectable && downPoint.HasValue && movePoint.HasValue && MultiSelect)
            {
                var rt = MathTool.MakeRectangle(new SKPoint(downPoint.Value.X, downPoint.Value.Y), new SKPoint(movePoint.Value.X, movePoint.Value.Y));

                using (var p = new SKPaint { IsAntialias = true })
                {
                    p.IsStroke = true;
                    p.StrokeWidth = 1.5F;
                    p.Color = SKColors.Cyan;
                    canvas.DrawRect(rt, p);
                }
            }
            #endregion
            base.OnContentDraw(e);
        }
        #endregion
        #region OnMouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            mx = e.X; my = e.Y;
            var (rtContent, rtBox, rtScroll) = Areas();
            Loop((ls, i, rt, itm) =>
            {
                if (itm.Enabled && itm.Visible && itm.Collision(rt, e.X, e.Y))
                {
                    itm.MouseDown(rt, e.X, e.Y);
                }
            });
            scroll.MouseDown(e.X, e.Y, rtScroll);


            if (Selectable && CollisionTool.Check(rtBox, e.X, e.Y))
            {
                downPoint = e.Location;
                movePoint = null;
            }
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            mx = e.X; my = e.Y;
            var (rtContent, rtBox, rtScroll) = Areas();
            Loop((ls, i, rt, itm) => { if (itm.Enabled && itm.Visible) itm.MouseMove(rt, e.X, e.Y); });
            scroll.MouseMove(e.X, e.Y, rtScroll);

            if (Selectable && downPoint.HasValue) movePoint = e.Location;
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mx = e.X; my = e.Y;
            var (rtContent, rtBox, rtScroll) = Areas();
            Loop((ls, i, rt, itm) => { if (itm.Enabled && itm.Visible) { itm.MouseUp(rt, e.X, e.Y); } });
            scroll.MouseUp(e.X, e.Y);

            #region 선택
            if (Selectable && downPoint.HasValue)
            {
                var rtdrag = MathTool.MakeRectangle(new SKPoint(e.X, e.Y), new SKPoint(downPoint.Value.X, downPoint.Value.Y));

                bool bChange = false;

                var sels = new List<GoContentGridItem>();
                Loop((ls, i, rt, itm) =>
                {
                    bool sel = MultiSelect ? itm.Collision(rt, rtdrag) : itm.Collision(rt, e.X, e.Y);
                    if (itm.Enabled && sel)
                    {
                        bChange = true;
                        sels.Add(itm);
                    }
                });

                if (MultiSelect)
                {
                    if ((ModifierKeys & Keys.Shift) != Keys.Shift) SelectedItems.Clear();
                    SelectedItems.AddRange(sels);
                }
                else
                {
                    if (sels.Count > 0)
                    {
                        SelectedItems.Clear();
                        SelectedItems.AddRange(sels);
                    }
                }


                if (bChange) { SelectedChanged?.Invoke(this, EventArgs.Empty); }
            }
            #endregion

            downPoint = movePoint = null;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            var (rtContent, rtBox, rtScroll) = Areas();

            Loop((ls, i, rt, itm) =>
            {
                if (itm.Enabled && itm.Visible)
                {
                    itm.MouseDoubleClick(rt, e.X, e.Y);

                    if (itm.Collision(rt, e.X, e.Y))
                        ItemDoubleClicked?.Invoke(this, new ItemClickedEventArgs(itm));
                }
            });

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var (rtContent, rtBox, rtScroll) = Areas();
            if (CollisionTool.Check(rtContent, e.X, e.Y))
            {
                if (scroll.ScrollVisible) ((HandledMouseEventArgs)e).Handled = true;
                scroll.MouseWheel(e.X, e.Y, e.Delta);
            }
            Invalidate();
            base.OnMouseWheel(e);
        }
        #endregion
        #endregion

        #region Method
        #region Areas
        public (SKRect rtContent, SKRect rtBox, SKRect rtScroll) Areas()
        {
            var scwh = Convert.ToInt32(Scroll.SC_WH);
            var gp = 10;
            var rtContent = Util.FromRect(0, 0, Width, Height);
            var rtBox = Util.FromRect(rtContent.Left, rtContent.Top, rtContent.Width - gp - scwh, rtContent.Height);
            var rtScroll = Util.FromRect(rtBox.Right + gp, rtBox.Top, scwh, rtBox.Height);
            return (rtContent, rtBox, rtScroll);
        }
        #endregion

        #region Arrange
        public void Arrange()
        {
            var (rtContent, rtBox, rtScroll) = Areas();

            var ls = this.Items.Where(x => x.Visible).ToList();
            var cw = Math.Round((double)(rtBox.Width) / (double)ContentSize.Width);

            if (vls.Count != ls.Count || szprev.Width != this.Size.Width || szprev.Height != this.Size.Height || cw != pcw)
            {
                int ir = 0, ic = 0;
                vls.Clear();
                for (int i = 0; i < ls.Count; i++)
                {
                    var itm = ls[i];

                    itm.Column = ic;
                    itm.Row = ir;

                    vls.Add(itm);

                    var vitm = i + 1 < ls.Count ? ls[i + 1] : null;
                    do
                    {
                        ic++;
                        if (vitm != null && ic + vitm.ColSpan > cw) { ir++; ic = 0; }

                    } while (vitm != null && vls.Where(x => x.Check(ic, ir, vitm.ColSpan, vitm.RowSpan)).Count() > 0);
                }
                pcw = cw;
                ScrollMax = vls.Count > 0 ? vls.Max(x => x.Bounds.Bottom) : 0;

                szprev = this.Size;
            }
        }
        #endregion
        #region Loop
        private void Loop(Action<List<GoContentGridItem>, int, SKRect, GoContentGridItem> Func)
        {
            var (rtContent, rtBox, rtScroll) = Areas();

            var spos = Convert.ToInt32(scroll.ScrollPositionWithOffset);

            Arrange();

            var ls = vls.Where(itm => CollisionTool.Check(rtBox, Util.FromRect(rtBox.Left + itm.Bounds.Left, rtBox.Top + itm.Bounds.Top + spos, itm.Bounds.Width, itm.Bounds.Height))).ToList();
            foreach (var itm in ls)
            {
                var i = vls.IndexOf(itm);
                var rt = Util.FromRect(rtBox.Left + itm.Bounds.Left, rtBox.Top + itm.Bounds.Top + spos, itm.Bounds.Width, itm.Bounds.Height);
                rt.Inflate(-Gap, -Gap);
                if (CollisionTool.Check(Util.FromRect(rt.Left + 1, rt.Top + 1, rt.Width - 2, rt.Height - 2), rtBox))
                {
                    Func(vls, i, rt, itm);
                }
            }

        }
        #endregion
        #endregion
    }

    #region class : GoContentGridItem
    public abstract class GoContentGridItem
    {
        #region Properties
        public int Column { get; set; }
        public int Row { get; set; }
        public int RowSpan { get => rowspan; set { rowspan = Math.Max(value, 1); } }
        public int ColSpan { get => colspan; set { colspan = Math.Max(value, 1); } }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public object? Tag { get; set; } = null;

        public GoContentGrid Control { get; private set; }

        public bool Selected => Control.SelectedItems.Contains(this);
        #endregion

        #region Member Variable
        int colspan = 1, rowspan = 1;
        #endregion

        #region Constructor
        public GoContentGridItem(GoContentGrid Control) { this.Control = Control; }
        #endregion

        #region Abstract / Virtual
        public abstract bool SelectedDraw { get; }
        public abstract SKRect GetBounds(SKRect Bounds);

        public virtual void Draw(SKCanvas canvas, SKRect Bounds) { }
        public virtual void MouseDown(SKRect Bounds, int x, int y) { }
        public virtual void MouseUp(SKRect Bounds, int x, int y) { }
        public virtual void MouseMove(SKRect Bounds, int x, int y) { }
        public virtual void MouseDoubleClick(SKRect Bounds, int x, int y) { }
        public virtual bool Collision(SKRect Bounds, int x, int y) { return CollisionTool.Check(GetBounds(Bounds), x, y); }
        public virtual bool Collision(SKRect Bounds, SKRect Bounds2) { return CollisionTool.Check(GetBounds(Bounds), Bounds2); }
        #endregion

        #region Internal
        internal SKRectI GridBounds => SKRectI.Create(Column, Row, ColSpan, RowSpan);

        internal SKRect Bounds
        {
            get
            {
                var (rtContent, rtBox, rtScroll) = Control.Areas();
                var ContentSize = new SizeF(Control.ContentSize.Width, Control.ContentSize.Height);
                var cw = (int)Math.Round((double)(rtBox.Width) / (double)ContentSize.Width);
                var sw = rtBox.Width / (float)cw;
                ContentSize.Width = sw;
                return Util.FromRect(Convert.ToInt32(Column * ContentSize.Width), Convert.ToInt32(Row * ContentSize.Height),
                                     Convert.ToInt32(ContentSize.Width * ColSpan), Convert.ToInt32(ContentSize.Height * RowSpan));
            }
        }

        internal bool Check(int col, int row, int colspan, int rowspan)
        {
            var Left1 = Column;
            var Right1 = Column + ColSpan;
            var Top1 = Row;
            var Bottom1 = Row + RowSpan;

            var Left2 = col;
            var Right2 = col + colspan;
            var Top2 = row;
            var Bottom2 = row + rowspan;

            return Right2 > Left1 && Right1 > Left2 && Bottom2 > Top1 && Bottom1 > Top2;
        }
        #endregion
    }
    #endregion
    #region class : ImageContent
    public class ImageContent(GoDesign design, GoContentGrid sc) : GoContentGridItem(sc)
    {
        public List<SKImage> Images { get; set; } = [];
        public string? Name { get; set; }
        public override bool SelectedDraw => true;
        public override SKRect GetBounds(SKRect bounds) => bounds;

        private GoDesign design = design;

        public override void Draw(SKCanvas canvas, SKRect Bounds)
        {
            var nmh = 24;
            var thm = GoThemeW.Current;
            var rtImage = Util.FromRect(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height - nmh);
            var rtName = Util.FromRect(Bounds.Left, rtImage.Bottom, Bounds.Width, nmh);

            if (design != null)
            {
                using (var p = new SKPaint { IsAntialias = true })
                {
                    if (Images.Count > 0)
                    {
                        var Image = Images.First();
                        var ratioH = Convert.ToSingle(Image.Height) / Convert.ToSingle(Image.Width);
                        var ratioV = Convert.ToSingle(Image.Width) / Convert.ToSingle(Image.Height);

                        if (ratioV * rtImage.Height < rtImage.Width)
                        {
                            var w = ratioV * rtImage.Height;
                            var h = rtImage.Height;
                            var vrt =  MathTool.MakeRectangle(rtImage, new SKSize(w, h));
                            canvas.DrawImage(Image, vrt, Util.Sampling);
                        }
                        else if (ratioH * rtImage.Width < rtImage.Height)
                        {
                            var w = rtImage.Width;
                            var h = ratioH * rtImage.Width;
                            var vrt = MathTool.MakeRectangle(rtImage, new SKSize(w, h));
                            canvas.DrawImage(Image, vrt, Util.Sampling);
                        }
                        else
                        {
                            canvas.DrawImage(Image, rtImage, Util.Sampling);
                        }
                    }

                    Util.DrawText(canvas, Name, sc.Font.Name, GoFontStyle.Normal, 12, rtName,  thm.Fore);
                }
            }

            base.Draw(canvas, Bounds);
        }
    }
    #endregion
    #region class : FontContent
    public class FontContent(GoDesign design, GoContentGrid sc) : GoContentGridItem(sc)
    {
        public string? Name { get; set; }
        public override bool SelectedDraw => true;
        public override SKRect GetBounds(SKRect bounds) => bounds;

        private GoDesign design = design;

        public override void Draw(SKCanvas canvas,  SKRect Bounds)
        {
            var nmh = 24;
            var thm = GoThemeW.Current;
            var rtImage = Util.FromRect(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height - nmh);
            var rtName = Util.FromRect(Bounds.Left, rtImage.Bottom, Bounds.Width, nmh);

            if (design != null)
            {
                using (var p = new SKPaint { IsAntialias = true })
                {
                    Util.DrawText(canvas, Name, Name ?? "", GoFontStyle.Normal, 24, rtImage, thm.Fore);
                    Util.DrawText(canvas, Name, sc.Font.Name, GoFontStyle.Normal, 12, rtName, thm.Base5);
                }
            }

            base.Draw(canvas, Bounds);
        }
    }
    #endregion
    #region class : ItemClickedEventArgs
    public class ItemClickedEventArgs(GoContentGridItem item) : EventArgs
    {
        public GoContentGridItem Item { get; set; } = item;
    }
    #endregion
}
