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
    /// <summary>
    /// 게이지 컨트롤. 원호 형태의 게이지로 값을 시각적으로 표시합니다.
    /// </summary>
    public class GoGauge : GoControl
    {
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
        /// 값 텍스트의 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 18;

        /// <summary>
        /// 제목 텍스트를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public string Title { get; set; } = "Title";
        /// <summary>
        /// 제목 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public float TitleFontSize { get; set; } = 12;

        /// <summary>
        /// 텍스트 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 채워진 영역의 색상 테마 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public string FillColor { get; set; } = "Good";
        /// <summary>
        /// 빈 영역의 색상 테마 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public string EmptyColor { get; set; } = "Base1";
        /// <summary>
        /// 테두리 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string BorderColor { get; set; } = "Base1";

        private double nValue = 0;
        /// <summary>
        /// 현재 값을 가져오거나 설정합니다. 값이 변경되면 <see cref="ValueChanged"/> 이벤트가 발생합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)]
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
        [GoProperty(PCategory.Control, 10)] public double Minimum { get; set; } = 0;
        /// <summary>
        /// 최대값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public double Maximum { get; set; } = 100;

        /// <summary>
        /// 값 표시 형식 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public string Format { get; set; } = "0";
        /// <summary>
        /// 게이지 시작 각도를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public int StartAngle { get; set; } = 135;
        /// <summary>
        /// 게이지 스윕 각도(호의 전체 각도)를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 14)] public int SweepAngle { get; set; } = 270;
        /// <summary>
        /// 게이지 바의 두께(픽셀)를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 15)] public int BarSize { get; set; } = 24;
        /// <summary>
        /// 값 텍스트와 제목 사이의 간격을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 16)] public int Gap { get; set; } = 0;
        #endregion

        #region Member Variable
        SKPath pathEmpty = new SKPath();
        SKPath pathFill = new SKPath();
        #endregion

        #region Event
        /// <summary>
        /// <see cref="Value"/> 속성 값이 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public GoGauge()
        {
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
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
            PathTool.Gauge(pathEmpty, rtBox, StartAngle, SweepAngle, BarSize);
            {
                var pth = pathEmpty;

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
                var pth = pathFill;
                using var imgf = SKImageFilter.CreateDropShadow(2, 2, 2, 2, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));
                PathTool.Gauge(pth, rtBox, StartAngle, Ang, BarSize);

                p.IsStroke = false;
                p.Color = cFill;
                p.ImageFilter = imgf;
                canvas.DrawPath(pth, p);
                p.ImageFilter = null;
            }
            #endregion

            var txt = string.IsNullOrWhiteSpace(Format) ? Value.ToString() : Value.ToString(Format);
            Util.DrawText(canvas, txt, FontName, FontStyle, FontSize, rtText, cText);

            Util.DrawText(canvas, Title, FontName, FontStyle, TitleFontSize, rtTitle, cText);

            base.OnDraw(canvas, thm);
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

            var vh = FontSize + 4;
            var th = TitleFontSize + 4;
            rts["Text"] = Util.FromRect(rt.Left, rt.MidY - (vh / 2), rt.Width, vh);
            rts["Title"] = Util.FromRect(rt.Left, rt.MidY - (vh / 2) + vh + Gap, rt.Width, th);
            return rts;
        }

        protected override void OnDispose()
        {
            pathEmpty.Dispose();
            pathFill.Dispose();

            base.OnDispose();
        }
        #endregion
    }
}
