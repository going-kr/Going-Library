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
        [GoFontNameProperty(PCategory.Control, 1)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public float FontSize { get; set; } = 18;
        /// <summary>
        /// 값 레이블의 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public float ValueFontSize { get; set; } = 14;

        /// <summary>
        /// 텍스트 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 채워진 영역의 색상 테마 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public string FillColor { get; set; } = "Good";
        /// <summary>
        /// 채움 그라데이션의 끝 색상 테마 이름을 가져오거나 설정합니다. null이면 단색(<see cref="FillColor"/>).
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public string? FillColor2 { get; set; } = null;
        /// <summary>
        /// 빈 영역의 색상 테마 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string EmptyColor { get; set; } = "Base1";
        /// <summary>
        /// 테두리 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public string BorderColor { get; set; } = "Base1";
        /// <summary>테두리 두께</summary>
        [GoProperty(PCategory.Control, 10)] public float BorderWidth { get; set; } = 1.5F;

        /// <summary>
        /// 프로그레스바의 진행 방향을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public ProgressDirection Direction { get; set; } = ProgressDirection.LeftToRight;

        private double nValue = 0;
        /// <summary>
        /// 현재 값을 가져오거나 설정합니다. 값이 변경되면 <see cref="ValueChanged"/> 이벤트가 발생합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)]
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
        [GoProperty(PCategory.Control, 14)] public double Minimum { get; set; } = 0;
        /// <summary>
        /// 최대값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 15)] public double Maximum { get; set; } = 100;

        /// <summary>
        /// 값 표시 형식 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 16)] public string Format { get; set; } = "0";
        /// <summary>
        /// 바의 크기(두께)를 가져오거나 설정합니다. null이면 컨트롤 크기에 맞춥니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public int? BarSize { get; set; }
        /// <summary>
        /// 값 레이블 표시 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 17)] public bool ShowValueLabel { get; set; } = false;

        /// <summary>
        /// 바 모서리 반경. <c>null</c>이면 완전 둥근(pill) 모양(두께/2), 값이 있으면 해당 반경을 사용합니다.
        /// (반경은 두께/2를 넘지 않도록 제한됩니다.)
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public float? Corner { get; set; } = null;

        /// <summary>
        /// 빈(트랙) 배경을 그릴지 여부. <c>false</c>면 <see cref="EmptyColor"/> 배경을 그리지 않고 채움만 표시합니다.
        /// 기본값은 <c>true</c>.
        /// </summary>
        [GoProperty(PCategory.Control, 18)] public bool DrawEmpty { get; set; } = true;

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
            var cBorder = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBar = rts["Gauge"];
            var rtFill = rts["Fill"];
            var rtValueText = rts["ValueText"];

            using var p = new SKPaint { IsAntialias = true };

            // 반지름: Corner 가 null 이면 완전 둥근(pill, 두께/2), 값이 있으면 그 반경(두께/2 이내로 제한).
            float rPill = Math.Min(rtBar.Width, rtBar.Height) / 2F;
            float rBar = Corner.HasValue ? Math.Clamp(Corner.Value, 0F, rPill) : rPill;

            // 배경(트랙) — DrawEmpty 가 false 면 생략
            if (DrawEmpty)
            {
                p.Color = cEmpty;
                canvas.DrawRoundRect(rtBar, rBar, rBar, p);
            }

            if (rtFill.Width > 0 && rtFill.Height > 0)
            {
                // 트랙 pill 모양으로 클립 → 채움은 사각형으로. 좌측 끝은 트랙 둥근 모서리를 따르고, 값 끝만 직선.
                using (new SKAutoCanvasRestore(canvas))
                {
                    using var clip = new SKRoundRect(rtBar, rBar);
                    canvas.ClipRoundRect(clip, SKClipOperation.Intersect, true);

                    if (!string.IsNullOrEmpty(FillColor2))
                    {
                        // FillColor → FillColor2 linear 그라데이션 (진행 방향 따라, Value 기준)
                        float mx = rtFill.MidX, my = rtFill.MidY;
                        SKPoint g0, g1;
                        switch (Direction)
                        {
                            case ProgressDirection.RightToLeft: g0 = new SKPoint(rtFill.Right, my); g1 = new SKPoint(rtFill.Left, my); break;
                            case ProgressDirection.BottomToTop: g0 = new SKPoint(mx, rtFill.Bottom); g1 = new SKPoint(mx, rtFill.Top); break;
                            case ProgressDirection.TopToBottom: g0 = new SKPoint(mx, rtFill.Top); g1 = new SKPoint(mx, rtFill.Bottom); break;
                            default: g0 = new SKPoint(rtFill.Left, my); g1 = new SKPoint(rtFill.Right, my); break; // LeftToRight
                        }
                        using var sh = SKShader.CreateLinearGradient(g0, g1, new[] { cFill, thm.ToColor(FillColor2) }, new[] { 0F, 1F }, SKShaderTileMode.Clamp);
                        p.Shader = sh;
                        canvas.DrawRoundRect(rtFill, rBar, rBar, p);
                        p.Shader = null;
                    }
                    else
                    {
                        p.Color = cFill;
                        canvas.DrawRoundRect(rtFill, rBar, rBar, p);
                    }
                }
            }

            // 테두리
            if (cBorder != SKColors.Transparent && BorderWidth > 0)
            {
                p.Color = cBorder;
                p.IsStroke = true;
                p.StrokeWidth = BorderWidth;
                canvas.DrawRoundRect(rtBar, rBar, rBar, p);
                p.IsStroke = false;
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
            // BarSize가 없고 '보더가 실제로 그려질 때만' 테두리가 경계 밖으로 잘리지 않게 BorderWidth만큼 안으로 줄인다.
            // (BorderWidth 0 또는 Transparent면 보더를 안 그리므로 줄이지도 않음)
            bool hasBorder = BorderWidth > 0 && !string.Equals(BorderColor, "Transparent", StringComparison.OrdinalIgnoreCase);
            if (!BarSize.HasValue && hasBorder)
            {
                rtBar.Inflate(-BorderWidth, -BorderWidth);
                barWidth = rtBar.Width;
                barHeight = rtBar.Height;
            }
            rts["Gauge"] = rtBar;

            // Fill은 트랙에 꽉 차게(flush)
            float usableWidth = barWidth;
            float usableHeight = barHeight;
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
                    rtFill = new SKRect(x, y, x + fillSize, y + barHeight);
                    // 텍스트 영역이 rtFill보다 크지 않도록 조정
                    rtValueText = new SKRect(
                        Math.Max(rtFill.Right - 40, rtFill.Left),
                        y + (barHeight / 2) - (ValueFontSize / 2),
                        rtFill.Right,
                        y + (barHeight / 2) + (ValueFontSize / 2)
                    );
                    break;

                case ProgressDirection.RightToLeft:
                    rtFill = new SKRect(x + barWidth - fillSize, y, x + barWidth, y + barHeight);
                    rtValueText = new SKRect(
                        rtFill.Left,
                        y + (barHeight / 2) - (ValueFontSize / 2),
                        Math.Min(rtFill.Left + 40, rtFill.Right),
                        y + (barHeight / 2) + (ValueFontSize / 2)
                    );
                    break;

                case ProgressDirection.BottomToTop:
                    rtFill = new SKRect(x, y + barHeight - fillSize, x + barWidth, y + barHeight);
                    rtValueText = new SKRect(
                        x + (barWidth / 2) - 20,
                        Math.Max(rtFill.Top, y),
                        x + (barWidth / 2) + 20,
                        Math.Min(rtFill.Top + 30, rtFill.Bottom)
                    );
                    break;

                case ProgressDirection.TopToBottom:
                    rtFill = new SKRect(x, y, x + barWidth, y + fillSize);
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