using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Controls
{
    public class GoMeter : GoControl
    {
        #region Const
        internal const float StartAngle = 135F;
        internal const float SweepAngle = 270F;
        internal const float GIN = 15F;
        #endregion

        #region Properties
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 18;

        public string Title { get; set; } = "Title";
        public float TitleFontSize { get; set; } = 12;
        public float RemarkFontSize { get; set; } = 10;

        public string TextColor { get; set; } = "Fore";
        public string NeedleColor { get; set; } = "Fore";
        public string NeedlePointColor { get; set; } = "Red";
        public string RemarkColor { get; set; } = "Base5";

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

        public int GraduationLarge { get; set; } = 10;
        public int GraduationSmall { get; set; } = 2;

        public string Format { get; set; } = "0";
        public int Gap { get; set; } = 0;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public GoMeter()
        {
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cNd = thm.ToColor(NeedleColor);
            var cNdp = thm.ToColor(NeedlePointColor);
            var cRmk = thm.ToColor(RemarkColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtBox = rts["Gauge"];
            var rtText = rts["Text"];
            var rtTitle = rts["Title"];

            var rwh = rtBox.Width / 2F;
            var cp = new SKPoint(rtContent.MidX, rtContent.MidY);

            using var p = new SKPaint { IsAntialias = true };

            #region Remark
            {
                var GraduationWidth = 2F;
                p.IsStroke = true;
                p.Color = cRmk;
                p.StrokeWidth = GraduationWidth;

                var rtCircleIn = rtBox; rtCircleIn.Inflate(-(RemarkFontSize + GIN), -(RemarkFontSize + GIN));
                canvas.DrawArc(rtCircleIn, StartAngle, SweepAngle, false, p);

                for (double i = Minimum; i <= Maximum; i += GraduationLarge)
                {
                    var gsang = Convert.ToSingle(MathTool.Map(MathTool.Constrain(i, Minimum, Maximum), Minimum, Maximum, 0D, SweepAngle)) + StartAngle;
                    var pT = MathTool.GetPointWithAngle(cp, gsang, rwh);
                    var pB = MathTool.GetPointWithAngle(cp, gsang, rwh - RemarkFontSize - GIN);
                    var pL = MathTool.GetPointWithAngle(cp, gsang, rwh - RemarkFontSize - (GIN-10));
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.DrawLine(pB, pL, p);

                        canvas.Translate(pT);
                        canvas.RotateDegrees(gsang + 90);
                        var rt = MathTool.MakeRectangle(new SKPoint(0, RemarkFontSize / 2F), 60);

                        Util.DrawText(canvas, i.ToString(), FontName, RemarkFontSize, rt, cRmk);
                    }
                }

                p.StrokeWidth = 1;
                for (double i = Minimum; i <= Maximum; i += GraduationSmall)
                {
                    var gsang = Convert.ToSingle(MathTool.Map(MathTool.Constrain(i, Minimum, Maximum), Minimum, Maximum, 0D, SweepAngle)) + StartAngle;
                    var pB = MathTool.GetPointWithAngle(cp, gsang, rwh - RemarkFontSize - GIN);
                    var pS = MathTool.GetPointWithAngle(cp, gsang, rwh - RemarkFontSize - (GIN - 5));
                    canvas.DrawLine(pB, pS, p);
                }
            }
            #endregion

            #region Needle
            using (var path = PathTool.Needle(rtContent, rtBox, Value, Minimum, Maximum, StartAngle, SweepAngle, RemarkFontSize))
            {
                var distN = rwh - RemarkFontSize - 5;
                using var imgf = SKImageFilter.CreateDropShadow(2, 2, 3, 3, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));
                using var lg = SKShader.CreateRadialGradient(cp, distN, [cNd, cNd, cNdp, cNdp], [0, 0.6F, 0.61F, 1], SKShaderTileMode.Clamp);

                p.ImageFilter = imgf;
                p.Shader = lg;
                p.IsStroke = false;
                canvas.DrawPath(path, p);
                p.ImageFilter = null;
                p.Shader = null;

                p.Color = thm.Base0;
                canvas.DrawCircle(cp, 3, p);
            }
            #endregion

            #region Text
            var txt = string.IsNullOrWhiteSpace(Format) ? Value.ToString() : Value.ToString(Format);
            Util.DrawText(canvas, txt, FontName, FontSize, rtText, cText);
            Util.DrawText(canvas, Title, FontName, TitleFontSize, rtTitle, cText);
            #endregion
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

            var cp = new SKPoint(rt.MidX, rt.MidY);
            var rwh = rtv.Width / 2F;
            
            var vh = FontSize + 4;
            var th = TitleFontSize + 4;
            var h = vh + Gap + th;
            //var my = rtv.Bottom - (h / 2) - (rwh / 10) - GIN;
            var my = MathTool.GetPointWithAngle(cp, 135, rwh- GIN).Y;
            rts["Text"] = Util.FromRect(rt.Left, my -  (h / 2), rt.Width, vh);
            rts["Title"] = Util.FromRect(rt.Left, my + (h / 2) - th, rt.Width, th);
            
            return rts;
        }
        #endregion
    }
}
