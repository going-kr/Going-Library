using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp.Views.Desktop;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Going.UI.Tools;
using Going.UI.ImageCanvas;

namespace Going.UI.Forms.ImageCanvas
{
    public class IcProgress : SKControl
    {
        #region Properties
        public string FontName { get => sFontName; set { if (sFontName != value) { sFontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => eFontStyle; set { if (eFontStyle != value) { eFontStyle = value; Invalidate(); } } }
        public float FontSize { get => nFontSize; set { if (nFontSize != value) { nFontSize = value; Invalidate(); } } }
        public string TextColor { get => sTextColor; set { if (sTextColor != value) { sTextColor = value; Invalidate(); } } }

        public string? OnBarImage { get => sOnBarImage; set { if (sOnBarImage != value) { sOnBarImage = value; Invalidate(); } } }
        public string? OffBarImage { get => sOffBarImage; set { if (sOffBarImage != value) { sOffBarImage = value; Invalidate(); } } }

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
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public bool DrawText { get => bDrawText; set { if (bDrawText != value) { bDrawText = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public IcProgress()
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

        private string? sOnBarImage, sOffBarImage;
        #endregion

        #region Override
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

                var offB = ip.GetImage(OffBarImage)?.FirstOrDefault();
                var onB = ip.GetImage(OnBarImage)?.FirstOrDefault();

                if (offB != null && onB != null)
                    bounds(offB, (_, rtBar, rtFill) =>
                    {
                        canvas.DrawImage(offB, rtBar, Util.Sampling);
                        if (Value > Minimum) canvas.DrawImage(onB, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill, Util.Sampling);
                        if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtBar, cText);
                    });
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

        #region Method
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
        #region bounds
        void bounds(SKImage? bmBar, Action<SKRect, SKRect, SKRect> act)
        {
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var rtBar = bmBar != null ? MathTool.MakeRectangle(rtBox, new SKSize(bmBar.Width, bmBar.Height)) : rtBox;

            var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, rtBar.Width));
            var rtFill = Util.FromRect(rtBar.Left, rtBar.Top, w, rtBar.Height);

            act(rtBox, rtBar, rtFill);
        }
        #endregion
        #endregion


    }
}
