using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Controls
{
    public class GoGauge : GoControl
    {
        #region Properties
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 18;

        public string Title { get; set; } = "Title";
        public float TitleFontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string FillColor { get; set; } = "Good";
        public string EmptyColor { get; set; } = "Base1";
        public string BorderColor { get; set; } = "Base1";

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
        public int StartAngle { get; set; } = 135;
        public int SweepAngle { get; set; } = 270;
        public int BarSize { get; set; } = 24;
        public int Gap { get; set; } = 0;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public GoGauge()
        {
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cEmpty = thm.ToColor(EmptyColor);
            var cFill= thm.ToColor(FillColor);
            var cBorder = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtBox = rts["Gauge"];
            var rtTitle = rts["Title"];
            var rtText = rts["Text"];

            using var p = new SKPaint { IsAntialias = true };

            #region Empty
            using (var pth = PathTool.Gauge(rtBox, StartAngle, SweepAngle, BarSize))
            {
                p.Color = cEmpty;
                p.IsStroke = false;
                canvas.DrawPath(pth, p);

                p.Color = cBorder;
                p.IsStroke = true;
                p.StrokeWidth = 1;
                canvas.DrawPath(pth, p);
            }
            #endregion

            #region Fill
            var Ang = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, SweepAngle));
            if (Ang > 0)
            {
                using var imgf = SKImageFilter.CreateDropShadow(2, 2, 2, 2, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));
                using var pth = PathTool.Gauge(rtBox, StartAngle, Ang, BarSize);

                p.IsStroke = false;
                p.Color = cFill;
                p.ImageFilter = imgf;
                canvas.DrawPath(pth, p);
                p.ImageFilter = null;
            }
            #endregion

            var txt = string.IsNullOrWhiteSpace(Format) ? Value.ToString() : Value.ToString(Format);
            Util.DrawText(canvas, txt, FontName, FontSize, rtText, cText);

            Util.DrawText(canvas, Title, FontName, TitleFontSize, rtTitle, cText);

            base.OnDraw(canvas);
        }

        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rt = rts["Content"];
            var wh = Math.Min(Width, Height);
            var rtv = MathTool.MakeRectangle(rt, new SKSize(wh, wh), GoContentAlignment.MiddleCenter);
            rtv = Util.Int(rtv);
            rtv.Inflate(-1.5F, -1.5F);
            rts["Gauge"] = rtv;

            var vh = FontSize + 4;
            var th = TitleFontSize + 4;
            rts["Text"] = Util.FromRect(rt.Left, rt.MidY - (vh / 2), rt.Width, vh);
            rts["Title"] = Util.FromRect(rt.Left, rt.MidY - (vh / 2) + vh + Gap, rt.Width, th);
            return rts;
        }
        #endregion
    }
}
