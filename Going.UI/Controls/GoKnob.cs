using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Controls
{
    public class GoKnob : GoControl
    {
        #region Const
        const int StartAngle = -90;
        #endregion

        #region Properties
        [GoFontNameProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 3)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 4)] public string KnobColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 5)] public string CursorColor { get; set; } = "Fore";

        private double nValue = 0;
        [GoProperty(PCategory.Control, 6)]
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

        [GoProperty(PCategory.Control, 7)] public double Minimum { get; set; } = 0;
        [GoProperty(PCategory.Control, 8)] public double Maximum { get; set; } = 100;
        [GoProperty(PCategory.Control, 9)] public double? Tick { get; set; } = null;

        [GoProperty(PCategory.Control, 10)] public string Format { get; set; } = "0";
        [GoProperty(PCategory.Control, 11)] public int SweepAngle { get; set; } = 270;

        [GoProperty(PCategory.Control, 12)] public bool DrawText { get; set; } = true;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public GoKnob()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        private bool bHover = false;

        float mx = 0F, my = 0F;
        double DownValue;
        double calcAngle;
        double downAngle;
        SKPoint prev;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cKnob = thm.ToColor(KnobColor).BrightnessTransmit(bHover | bDown ? thm.HoverFillBrightness : 0);
            var cBorder = thm.ToColor(KnobColor).BrightnessTransmit(bHover | bDown ? thm.HoverBorderBrightness : 0);
            var cCur = thm.ToColor(CursorColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtBox = rts["Gauge"];
            var cp = new SKPoint(rtBox.MidX, rtBox.MidY);
            var whCursor = Convert.ToInt32(Math.Min(rtBox.Width, rtBox.Height)) / 2;
            var vang = Convert.ToSingle(MathTool.Map(MathTool.Constrain(Value, Minimum, Maximum), Minimum, Maximum, 0, SweepAngle)) + StartAngle;

            using var p = new SKPaint { IsAntialias = true };

            #region Table
            {
                var cL = cKnob.BrightnessTransmit(-0.05F);
                var cD = cKnob.BrightnessTransmit(0.05F);

                using var sh = SKShader.CreateSweepGradient(cp, [cL, cD, cL, cD, cL, cD, cL, cD, cL, cD, cL], SKShaderTileMode.Repeat, vang, vang + 360);
                using var imgf = SKImageFilter.CreateDropShadow(2, 2, 2, 2, thm.Dark ? SKColors.Black : SKColors.Gray);

                p.ImageFilter = imgf;
                p.Shader = sh;
                p.IsStroke = false;
                canvas.DrawCircle(cp, whCursor, p);
                p.ImageFilter = null;
                p.Shader = null;

                p.IsStroke = true;
                p.StrokeWidth = 1.5F;
                p.Color = cBorder;
                canvas.DrawCircle(cp, whCursor, p);
            }
            #endregion

            #region Cursor
            {
                var pt1 = MathTool.GetPointWithAngle(cp, vang, whCursor * 0.8F);
                var pt2 = MathTool.GetPointWithAngle(cp, vang, whCursor * 0.5F);
                using var imgf = SKImageFilter.CreateDropShadow(1, 1, 1, 1, thm.Dark ? SKColors.Black : SKColors.Gray);

                using (var pth = PathTool.KnobCursor(pt1, pt2, vang, whCursor / 8))
                {
                    p.IsStroke = false;
                    p.Color = cCur.BrightnessTransmit(1);
                    p.ImageFilter = imgf;
                    canvas.DrawPath(pth, p);
                    p.ImageFilter = null;
                }
            }
            #endregion
          
            #region Text
            if (DrawText)
            {
                var txt = string.IsNullOrWhiteSpace(Format) ? Value.ToString() : Value.ToString(Format);
                Util.DrawText(canvas, txt, FontName, FontStyle, FontSize, rtBox, cText);
            }
            #endregion

            base.OnDraw(canvas, thm);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            mx = x; my = y; var ptLoc = new SKPoint(x, y);


            var rts = Areas();
            var rtBox = rts["Content"];

            if (CollisionTool.CheckEllipse(rtBox, ptLoc))
            {
                bDown = true;
                calcAngle = 0;
                downAngle = MathTool.Map(Value, Minimum, Maximum, StartAngle, StartAngle + SweepAngle);
                prev = ptLoc;
                DownValue = Value;
            }

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            mx = x; my = y; var ptLoc = new SKPoint(x, y);

            var rts = Areas();
            var rtBox = rts["Content"];

            if (bDown)
            {
                var cp = MathTool.CenterPoint(rtBox);
                var pv = MathTool.GetAngle(cp, prev);
                var nv = MathTool.GetAngle(cp, ptLoc);

                var v = nv - pv;
                if (v < -300) v = 360 + v;
                else if (v > 300) v = v - 360;
                calcAngle += v;

                var cv = MathTool.Map(calcAngle, 0D, SweepAngle, Minimum, Maximum);
                var val = MathTool.Constrain(DownValue + cv, Minimum, Maximum);
                if (Tick.HasValue && Tick != 0) val = Math.Round(val / Tick.Value) * Tick.Value;
                if (Value != val) Value = val;

                bDown = false;
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            mx = x; my = y; var ptLoc = new SKPoint(x, y);

            var rts = Areas();
            var rtBox = rts["Content"];

            bHover = CollisionTool.CheckEllipse(rtBox, ptLoc);

            if (bDown)
            {
                var cp = MathTool.CenterPoint(rtBox);
                var pv = MathTool.GetAngle(cp, prev);
                var nv = MathTool.GetAngle(cp, ptLoc);

                var v = nv - pv;
                if (v < -300) v = 360 + v;
                else if (v > 300) v = v - 360;
                calcAngle += v;

                var va = downAngle + calcAngle;
                if (va > StartAngle + SweepAngle + 360) calcAngle -= 360;
                else if (va < StartAngle - 360) calcAngle += 360;

                var cv = MathTool.Map(calcAngle, 0D, SweepAngle, Minimum, Maximum);
                var val = MathTool.Constrain(DownValue + cv, Minimum, Maximum);
                if (Tick.HasValue && Tick != 0) val = Math.Round(val / Tick.Value) * Tick.Value;

                if (Value != val) Value = val;

                prev = ptLoc;
            }

            base.OnMouseMove(x, y);
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
            return rts;
        }
        #endregion
    }
}
