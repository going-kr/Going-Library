using Going.UI.Enums;
using Going.UI.ImageCanvas;
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

namespace Going.UI.Forms.ImageCanvas
{
    public class IcSlider : SKControl
    {
        #region Properties
        public string FontName { get => sFontName; set { if (sFontName != value) { sFontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => eFontStyle; set { if (eFontStyle != value) { eFontStyle = value; Invalidate(); } } }
        public float FontSize { get => nFontSize; set { if (nFontSize != value) { nFontSize = value; Invalidate(); } } }
        public string TextColor { get => sTextColor; set { if (sTextColor != value) { sTextColor = value; Invalidate(); } } }

        public string? OffCursorImage { get => sOffCursorImage; set { if (sOffCursorImage != value) { sOffCursorImage = value; Invalidate(); } } }
        public string? OnCursorImage { get => sOnCursorImage; set { if (sOnCursorImage != value) { sOnCursorImage = value; Invalidate(); } } }
        public string? OffBarImage { get => sOffBarImage; set { if (sOffBarImage != value) { sOffBarImage = value; Invalidate(); } } }
        public string? OnBarImage { get => sOnBarImage; set { if (sOnBarImage != value) { sOnBarImage = value; Invalidate(); } } }

        public string FormatString { get => sFormatString; set { if (sFormatString != value) { sFormatString = value; Invalidate(); } } }
        public double Minimum { get => nMin; set { if (nMin != value) { nMin = value; Invalidate(); } } }
        public double Maximum { get => nMax; set { if (nMax != value) { nMax = value; Invalidate(); } } }
        public double Value
        {
            get => nValue;
            set
            {
                if (nValue != value)
                {
                    nValue = MathTool.Constrain(value, nMin, nMax);
                    if (Tick.HasValue) nValue = Math.Round(nValue / Tick.Value) * Tick.Value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public double? Tick { get; set; }

        public bool DrawText { get => bDrawText; set { if (bDrawText != value) { bDrawText = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public IcSlider()
        {
            Text = "";
        }
        #endregion

        #region Member Variable
        private bool bDrawText = false;

        private double nValue = 0, nMin = 0, nMax = 100;

        private GoFontStyle eFontStyle = GoFontStyle.Normal;
        private string sFontName = "나눔고딕";
        private float nFontSize = 12;
        private string sTextColor = "Black";
        private string sFormatString = "0";

        private string? sOffCursorImage, sOnCursorImage;
        private string? sOffBarImage, sOnBarImage;
        private string? sBarName;

        private bool bCurDown = false;
        #endregion

        #region Override
        #region Paint
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var thm = GoTheme.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var cText = thm.ToColor(TextColor);

            var (ip, off, on, cBack) = vars();
            if (ip != null && Parent != null && on != null && off != null)
            {
                var sx = on.Width / Parent.Width;
                var sy = on.Height / Parent.Height;
                canvas.DrawImage(off, Util.FromRect(Left * sx, Top * sy, Width * sx, Height * sy), rtBox, Util.Sampling);

                var offB = ip.GetImage(OffBarImage).FirstOrDefault();
                var onB = ip.GetImage(OnBarImage).FirstOrDefault();
                var offC = ip.GetImage(OffCursorImage).FirstOrDefault();
                var onC = ip.GetImage(OnCursorImage).FirstOrDefault();

                if (offB != null && onB != null && offC != null && onC != null)
                {
                    bounds(offB, offC, (_, rtBar, rtCur, rtFill) =>
                    {
                        canvas.DrawImage(offB, rtBar, Util.Sampling);
                        if (Value > Minimum) canvas.DrawImage(onB, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill, Util.Sampling);
                        canvas.DrawImage(bCurDown ? onC : offC, rtCur, Util.Sampling);
                        if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtCur, cText);
                    });
                }

            }

            #region Enabled
            if (!Enabled)
            {
                using var p = new SKPaint { IsAntialias = true };

                p.IsStroke = false;
                p.Color = cBack.WithAlpha(180);
                canvas.DrawRect(rtBox, p);
            }
            #endregion
            #region DesignMode
            if (DesignMode)
            {
                using var p = new SkiaSharp.SKPaint { IsAntialias = false };
                using var pe = SKPathEffect.CreateDash([2F, 2F], 2);
                p.IsStroke = true;
                p.Color = SKColors.Red;
                p.PathEffect = pe;
                canvas.DrawRect(Util.FromRect(0, 0, Width - 1, Height - 1), p);
                p.PathEffect = null;
            }
            #endregion

            base.OnPaintSurface(e);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            int x = e.X, y = e.Y;
            var (ip, off, on, cBack) = vars();
            if (ip != null && Parent != null && on != null && off != null)
            {
                var offB = ip.GetImage(OffBarImage).FirstOrDefault();
                var onB = ip.GetImage(OnBarImage).FirstOrDefault();
                var offC = ip.GetImage(OffCursorImage).FirstOrDefault();
                var onC = ip.GetImage(OnCursorImage).FirstOrDefault();

                if (offB != null && onB != null && offC != null && onC != null)
                {
                    bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                    {
                        if (CollisionTool.Check(rtCur, x, y)) bCurDown = true;
                    });
                }
            }


            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int x = e.X, y = e.Y;
            if (bCurDown)
            {
                var (ip, off, on, cBack) = vars();
                if (ip != null && Parent != null && on != null && off != null)
                {
                    var offB = ip.GetImage(OffBarImage).FirstOrDefault();
                    var onB = ip.GetImage(OnBarImage).FirstOrDefault();
                    var offC = ip.GetImage(OffCursorImage).FirstOrDefault();
                    var onC = ip.GetImage(OnCursorImage).FirstOrDefault();

                    if (offB != null && onB != null && offC != null && onC != null)
                    {
                        bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                        {
                            Value = MathTool.Map(MathTool.Constrain(x, rtBar.Left, rtBar.Right), rtBar.Left, rtBar.Right, Minimum, Maximum);
                        });
                    }
                }
            }
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            int x = e.X, y = e.Y;
            if (bCurDown)
            {
                var (ip, off, on, cBack) = vars();
                if (ip != null && Parent != null && on != null && off != null)
                {
                    var offB = ip.GetImage(OffBarImage).FirstOrDefault();
                    var onB = ip.GetImage(OnBarImage).FirstOrDefault();
                    var offC = ip.GetImage(OffCursorImage).FirstOrDefault();
                    var onC = ip.GetImage(OnCursorImage).FirstOrDefault();

                    if (offB != null && onB != null && offC != null && onC != null)
                    {
                        bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                        {
                            Value = MathTool.Map(MathTool.Constrain(x, rtBar.Left, rtBar.Right), rtBar.Left, rtBar.Right, Minimum, Maximum);
                        });
                    }
                }

                bCurDown = false;
            }
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #endregion

        #region Method
        #region bounds
        void bounds(SKImage? bmBar, SKImage bmCur, Action<SKRect, SKRect, SKRect, SKRect> act)
        {
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var rtBar = bmBar != null ? MathTool.MakeRectangle(rtBox, new SKSize(bmBar.Width, bmBar.Height)) : rtBox;

            var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, rtBar.Width));
            var rtFill = Util.FromRect(rtBar.Left, rtBar.Top, w, rtBar.Height);

            var x = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, rtBar.Left, rtBar.Right));
            var y = rtBar.MidY;
            var rtCur = MathTool.MakeRectangle(new SKPoint(x, y), bmCur.Width / 2F, bmCur.Height / 2F);
            act(rtBox, rtBar, rtCur, rtFill);
        }
        #endregion
        #region vars
        (IcFolder? ip, SKImage? on, SKImage? off, SKColor cBack) vars()
        {
            var thm = GoTheme.Current;
            IcFolder? ip = null;
            SKImage? on = null, off = null;
            SKColor cBack = thm.Back;

            if (Parent is IcContainer con)
            {
                ip = IcResources.Get(con.ImageFolder);
                cBack = thm.ToColor(con.BackgroundColor);
                if (ip != null && ip.GetImage(con.OffImage)?.FirstOrDefault() is SKImage ioff && ip.GetImage(con.OnImage)?.FirstOrDefault() is SKImage ion) { on = ion; off = ioff; }
            }
            else if (Parent is TabPage tab && tab.Parent is IcCanvas ic)
            {
                ip = IcResources.Get(ic.ImageFolder);
                cBack = thm.ToColor(ic.BackgroundColor);
                var nm = ic.TabPageImage.FirstOrDefault(x => x.TabPage == ic.SelectedTab);
                if (ip != null && nm != null && ip.GetImage(nm.OffImage)?.FirstOrDefault() is SKImage ioff && ip.GetImage(nm.OnImage)?.FirstOrDefault() is SKImage ion) { on = ion; off = ioff; }
            }

            return (ip, on, off, cBack);
        }
        #endregion
        #endregion
    }
}
