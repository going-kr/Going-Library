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
    /// <summary>
    /// 프로그레스바의 진행 방향을 정의합니다.
    /// </summary>
    public enum ProgressDirection
    {
        /// <summary>
        /// 왼쪽에서 오른쪽으로 진행합니다.
        /// </summary>
        LeftToRight,
        /// <summary>
        /// 오른쪽에서 왼쪽으로 진행합니다.
        /// </summary>
        RightToLeft,
        /// <summary>
        /// 아래에서 위로 진행합니다.
        /// </summary>
        BottomToTop,
        /// <summary>
        /// 위에서 아래로 진행합니다.
        /// </summary>
        TopToBottom
    }

    /// <summary>
    /// 프로그레스바 컨트롤. 작업 진행률을 시각적으로 표시합니다.
    /// </summary>
    public class GoProgress : GoControl
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
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 18;
        /// <summary>
        /// 값 레이블의 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public float ValueFontSize { get; set; } = 14;

        /// <summary>
        /// 텍스트 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 채워진 영역의 색상 테마 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string FillColor { get; set; } = "Good";
        /// <summary>
        /// 빈 영역의 색상 테마 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public string EmptyColor { get; set; } = "Base1";
        /// <summary>
        /// 테두리 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public string BorderColor { get; set; } = "Transparent";

        /// <summary>
        /// 프로그레스바의 진행 방향을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public ProgressDirection Direction { get; set; } = ProgressDirection.LeftToRight;

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
        /// 바 내부 여백을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public int Gap { get; set; } = 5;
        /// <summary>
        /// 모서리 반경을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 14)] public int CornerRadius { get; set; } = 5;
        /// <summary>
        /// 바의 크기(두께)를 가져오거나 설정합니다. null이면 컨트롤 크기에 맞춥니다.
        /// </summary>
        [GoProperty(PCategory.Control, 15)] public int? BarSize { get; set; }
        /// <summary>
        /// 값 레이블 표시 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 16)] public bool ShowValueLabel { get; set; } = false;

        #endregion

        #region Event
        /// <summary>
        /// <see cref="Value"/> 속성 값이 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="GoProgress"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoProgress()
        {
        }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
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

            if (ShowValueLabel)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(rtFill);
                    var txt = string.IsNullOrWhiteSpace(Format) ? Value.ToString() : Value.ToString(Format);
                    Util.DrawText(canvas, txt, FontName, FontStyle, ValueFontSize, rtValueText, cText);
                }
            }

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rt = rts["Content"];

            float barWidth = Width;
            float barHeight = Height;
            if(BarSize.HasValue)
            {
                if (Direction == ProgressDirection.LeftToRight || Direction == ProgressDirection.RightToLeft)
                    barHeight = BarSize.Value;
                else
                    barWidth = BarSize.Value;
            }

            SKRect rtBar = MathTool.MakeRectangle(rt, new SKSize(barWidth, barHeight));
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

            float x = rtBar.Left;
            float y = rtBar.Top;

            switch (Direction)
            {
                case ProgressDirection.LeftToRight:
                    rtFill = new SKRect(x + Gap, y + Gap, x + Gap + fillSize, y + barHeight - Gap);
                    // 텍스트 영역이 rtFill보다 크지 않도록 조정
                    rtValueText = new SKRect(
                        Math.Max(rtFill.Right - 40, rtFill.Left),
                        y + (barHeight / 2) - (ValueFontSize / 2),
                        rtFill.Right,
                        y + (barHeight / 2) + (ValueFontSize / 2)
                    );
                    break;

                case ProgressDirection.RightToLeft:
                    rtFill = new SKRect(x + barWidth - Gap - fillSize, y + Gap, x + barWidth - Gap, y + barHeight - Gap);
                    rtValueText = new SKRect(
                        rtFill.Left,
                        y + (barHeight / 2) - (ValueFontSize / 2),
                        Math.Min(rtFill.Left + 40, rtFill.Right),
                        y + (barHeight / 2) + (ValueFontSize / 2)
                    );
                    break;

                case ProgressDirection.BottomToTop:
                    rtFill = new SKRect(x + Gap, y + barHeight - Gap - fillSize, x + barWidth - Gap, y + barHeight - Gap);
                    rtValueText = new SKRect(
                        x + (barWidth / 2) - 20,
                        Math.Max(rtFill.Top, y + Gap),
                        x + (barWidth / 2) + 20,
                        Math.Min(rtFill.Top + 30, rtFill.Bottom)
                    );
                    break;

                case ProgressDirection.TopToBottom:
                    rtFill = new SKRect(x + Gap, y + Gap, x + barWidth - Gap, y + Gap + fillSize);
                    rtValueText = new SKRect(
                        x + (barWidth / 2) - 20,
                        Math.Max(rtFill.Bottom - 30, rtFill.Top),
                        x + (barWidth / 2) + 20,
                        rtFill.Bottom
                    );
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