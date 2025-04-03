using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public enum ProgressDirection
    {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom
    }

    public class GoProgress : GoControl
    {
        #region Properties
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 18;
        public float ValueFontSize { get; set; } = 14;

        public string TextColor { get; set; } = "Fore";
        public string FillColor { get; set; } = "Good";
        public string EmptyColor { get; set; } = "Base1";
        public string BorderColor { get; set; } = "Transparent";

        public ProgressDirection Direction { get; set; } = ProgressDirection.LeftToRight;

        private double nValue = 0;
        public double Value
        {
            get => nValue;
            set
            {
                if (nValue != value)
                {
                    nValue = MathTool.Constrain(value, Minimum, Maximum);
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double Minimum { get; set; } = 0;
        public double Maximum { get; set; } = 100;

        public string Format { get; set; } = "0";
        public int Gap { get; set; } = 5;
        public int CornerRadius { get; set; } = 5;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public GoProgress()
        {
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cEmpty = thm.ToColor(EmptyColor);
            var cFill = thm.ToColor(FillColor);
            var rts = Areas();
            var rtBar = rts["Gauge"];
            var rtFill = rts["Fill"];
            var rtValueText = rts["ValueText"];

            using var p = new SKPaint { IsAntialias = true };

            // 배경
            p.Color = cEmpty;
            canvas.DrawRoundRect(rtBar, CornerRadius, CornerRadius, p);

            if (rtFill.Width > 0 && rtFill.Height > 0)
            {
                p.Color = cFill;
                canvas.DrawRoundRect(rtFill, CornerRadius, CornerRadius, p);
            }

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipRect(rtFill);
                var txt = string.IsNullOrWhiteSpace(Format) ? Value.ToString() : Value.ToString(Format);
                Util.DrawText(canvas, txt, FontName, FontStyle, ValueFontSize, rtValueText, cText);
            }
            base.OnDraw(canvas);
        }

        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rt = rts["Content"];

            float x = rt.Left;
            float y = rt.Top;
            float barWidth = Width;
            float barHeight = Height;

            SKRect rtBar = new SKRect(x, y, x + barWidth, y + barHeight);
            rts["Gauge"] = rtBar;

            float usableWidth = barWidth - Gap * 2;
            float usableHeight = barHeight - Gap * 2;
            float fillSize = 0;

            if (Maximum > Minimum)
            {
                double ratio = (Value - Minimum) / (Maximum - Minimum);
                float usable = (Direction == ProgressDirection.LeftToRight || Direction == ProgressDirection.RightToLeft)
                    ? usableWidth
                    : usableHeight;

                fillSize = (float)(Math.Clamp(ratio, 0, 1) * usable);
            }

            SKRect rtFill;
            SKRect rtValueText;

            switch (Direction)
            {
                case ProgressDirection.LeftToRight:
                    rtFill = new SKRect(x + Gap, y + Gap, x + Gap + fillSize, y + barHeight - Gap);
                    rtValueText = new SKRect( rtFill.Right - 40, y + (barHeight / 2) - (ValueFontSize / 2), rtFill.Right, y + (barHeight / 2) + (ValueFontSize / 2) );
                    break;

                case ProgressDirection.RightToLeft:
                    rtFill = new SKRect( x + barWidth - Gap - fillSize, y + Gap, x + barWidth - Gap, y + barHeight - Gap ); 
                    rtValueText = new SKRect( rtFill.Left + 5, y + (barHeight / 2) - (ValueFontSize / 2), rtFill.Left + 45, y + (barHeight / 2) + (ValueFontSize / 2) );
                    break;

                case ProgressDirection.BottomToTop:
                    rtFill = new SKRect( x + Gap, y + barHeight - Gap - fillSize, x + barWidth - Gap, y + barHeight - Gap );
                    rtValueText = new SKRect( x + (barWidth / 2) - 20, rtFill.Top, x + (barWidth / 2) + 20, rtFill.Top + 30 );
                    break;

                case ProgressDirection.TopToBottom:
                    rtFill = new SKRect( x + Gap, y + Gap, x + barWidth - Gap, y + Gap + fillSize);
                    rtValueText = new SKRect( x + (barWidth / 2) - 20, rtFill.Bottom - 30, x + (barWidth / 2) + 20, rtFill.Bottom);
                    break;

                default:
                    rtFill = new SKRect(0, 0, 0, 0);
                    rtValueText = new SKRect(0, 0, 0, 0);
                    break;
            }

            rts["Fill"] = rtFill;
            rts["ValueText"] = rtValueText;

            return rts;
        }
        #endregion
    }
}