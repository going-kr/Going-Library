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

        public string Title { get; set; } = "Title";
        public float TitleFontSize { get; set; } = 12;

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
            var cBorder = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBar = rts["Gauge"];
            var rtFill = rts["Fill"];
            var rtValueText = rts["ValueText"];

            using var p = new SKPaint { IsAntialias = true };

            p.Color = cEmpty;
            canvas.DrawRoundRect(rtBar, CornerRadius, CornerRadius, p);

            var rtInnerFill = new SKRect(rtFill.Left + Gap, rtFill.Top + Gap, rtFill.Right - Gap, rtFill.Bottom - Gap);
            p.Color = cFill;
            canvas.DrawRoundRect(rtInnerFill, CornerRadius, CornerRadius, p);
                        
            var txt = string.IsNullOrWhiteSpace(Format) ? Value.ToString() : Value.ToString(Format);
            Util.DrawText(canvas, txt, FontName, FontStyle, ValueFontSize, rtValueText, cText);

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

            float fillSize = (float)MathTool.Map(Value, Minimum, Maximum, 0, (Direction == ProgressDirection.LeftToRight || Direction == ProgressDirection.RightToLeft) ? barWidth : barHeight);

            SKRect rtFill;
            SKRect rtValueText;
            switch (Direction)
            {
                case ProgressDirection.LeftToRight:
                    rtFill = new SKRect(x, y, x + fillSize, y + barHeight);
                    rtValueText = new SKRect(x + fillSize - 30, y + (barHeight / 2) - (ValueFontSize / 2), x + fillSize + 0, y + (barHeight / 2) + (ValueFontSize / 2));
                    break;
                case ProgressDirection.RightToLeft:
                    rtFill = new SKRect(x + barWidth - fillSize, y, x + barWidth, y + barHeight);
                    rtValueText = new SKRect(x + barWidth - fillSize + 10, y + (barHeight / 2) - (ValueFontSize / 2), x + barWidth - fillSize + 30, y + (barHeight / 2) + (ValueFontSize / 2));
                    break;
                case ProgressDirection.BottomToTop:
                    rtFill = new SKRect(x, y + barHeight - fillSize, x + barWidth, y + barHeight);
                    rtValueText = new SKRect(x + barWidth / 2 - 20, y + barHeight - fillSize + 30, x + barWidth / 2 + 20, y + barHeight - fillSize);
                    break;
                case ProgressDirection.TopToBottom:
                    rtFill = new SKRect(x, y, x + barWidth, y + fillSize);
                    rtValueText = new SKRect(x + barWidth / 2 - 20, y + fillSize, x + barWidth / 2 + 20, y + fillSize - 30);
                    break;
                default:
                    rtFill = new SKRect(x, y, x + fillSize, y + barHeight);
                    rtValueText = new SKRect(x + fillSize - 30, y + (barHeight / 2) - (ValueFontSize / 2), x + fillSize + 20, y + (barHeight / 2) + (ValueFontSize / 2));
                    break;
            }

            rts["Fill"] = rtFill;
            rts["ValueText"] = rtValueText;

            return rts;
        }
        #endregion
    }
}