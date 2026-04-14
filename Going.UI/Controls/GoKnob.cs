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
    /// <summary>
    /// 노브(다이얼) 컨트롤. 회전식 다이얼을 통해 값을 조절할 수 있습니다.
    /// </summary>
    public class GoKnob : GoControl
    {
        #region Const
        const int StartAngle = -90;
        #endregion

        #region Properties
        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;

        /// <summary>
        /// 텍스트 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 노브 본체 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public string KnobColor { get; set; } = "Base3";
        /// <summary>
        /// 노브 커서(지시선) 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string CursorColor { get; set; } = "Fore";

        private double nValue = 0;
        /// <summary>
        /// 현재 값을 가져오거나 설정합니다. 값이 변경되면 <see cref="ValueChanged"/> 이벤트가 발생합니다.
        /// </summary>
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

        /// <summary>
        /// 최소값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public double Minimum { get; set; } = 0;
        /// <summary>
        /// 최대값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public double Maximum { get; set; } = 100;
        /// <summary>
        /// 값 변경 단위(틱)를 가져오거나 설정합니다. null이면 연속적으로 변경됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public double? Tick { get; set; } = null;

        /// <summary>
        /// 값 표시 형식 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public string Format { get; set; } = "0";
        /// <summary>
        /// 노브의 회전 범위 각도를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public int SweepAngle { get; set; } = 270;

        /// <summary>
        /// 값 텍스트 표시 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public bool DrawText { get; set; } = true;
        #endregion

        #region Event
        /// <summary>
        /// <see cref="Value"/> 속성 값이 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        /// <summary>GoKnob 클래스의 새 인스턴스를 초기화합니다.</summary>
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

        SKPath path = new SKPath();
        #endregion

        #region Override
        /// <inheritdoc/>
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

                {
                    PathTool.KnobCursor(path, pt1, pt2, vang, whCursor / 8);

                    p.IsStroke = false;
                    p.Color = cCur.BrightnessTransmit(1);
                    p.ImageFilter = imgf;
                    canvas.DrawPath(path, p);
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void OnDispose()
        {
            path.Dispose();
            base.OnDispose();
        }

        /// <inheritdoc/>
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
