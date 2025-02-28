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
using Windows.ApplicationModel;
using Going.UI.Tools;

namespace Going.UI.Forms.ImageCanvas
{
    public class IcProgress : SKControl
    {
        #region Properties
        public string FontName { get => sFontName; set { if (sFontName != value) { sFontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => eFontStyle; set { if (eFontStyle != value) { eFontStyle = value; Invalidate(); } } }
        public float FontSize { get => nFontSize; set { if (nFontSize != value) { nFontSize = value; Invalidate(); } } }
        public string TextColor { get => sTextColor; set { if (sTextColor != value) { sTextColor = value; Invalidate(); } } }

        public string? BarName { get => sBarName; set { if (sBarName != value) { sBarName = value; Invalidate(); } } }

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

        private string? sBarName;
        #endregion

        #region Override
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var rtBox = Util.FromRect(0, 0, Width, Height);

            var ic = GetCanvas();
            if (ic != null && Parent is TabPage tab && ic.PageImages.TryGetValue(tab.Name, out var img) && img.On != null && img.Off != null)
            {
                var sx = (double)img.On.Width / Parent.Width;
                var sy = (double)img.On.Height / Parent.Height;
                canvas.DrawBitmap(img.Off, Util.FromRect(Convert.ToInt32(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);
            }
            else if (Parent is IcContainer con && con.On != null && con.Off != null)
            {
                var sx = (double)con.On.Width / con.Width;
                var sy = (double)con.On.Height / con.Height;
                canvas.DrawBitmap(con.Off, Util.FromRect(Convert.ToInt32(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);
            }

            bounds(ic, (_, rtBar, rtFill, bmBar) =>
            {
                canvas.DrawBitmap(bmBar.Off, rtBar);
                if (Value > Minimum) canvas.DrawBitmap(bmBar.On, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill);
                if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtBar, cText);
            });

            if (!Enabled)
            {
                using var p = new SKPaint { IsAntialias = true };

                p.IsStroke = false;
                p.Color = thm.ToColor(ic.BackgroundColor).WithAlpha(180);
                canvas.DrawRect(rtBox, p);
            }

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

            base.OnPaintSurface(e);
        }
        #endregion

        #region Method
        void bounds(IcCanvas? ic, Action<SKRect, SKRect, SKRect, IcOnOffImage> act)
        {
            if (ic != null && sBarName != null && ic.Images.TryGetValue(sBarName, out var bmBar) && bmBar.On != null && bmBar.Off != null)
            {
                var rtBox = Util.FromRect(0, 0, Width, Height);
                var rtBar = MathTool.MakeRectangle(rtBox, new SKSize(bmBar.Off.Width, bmBar.Off.Height));

                var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, rtBar.Width));
                var rtFill = Util.FromRect(rtBar.Left, rtBar.Top, w, rtBar.Height);

                act(rtBox, rtBar, rtFill, bmBar);
            }
        }

        IcCanvas? GetCanvas()
        {
            var p = Parent;
            while (p is not IcCanvas && p != null) p = p.Parent;
            return p as IcCanvas;
        }
        #endregion
    }
}
