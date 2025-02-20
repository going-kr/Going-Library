using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;

namespace Going.UI.Controls
{
    public abstract class GoSlider : GoControl
    {
        #region Properties
        // 아이콘 설정
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;
        public GoDirectionHV IconDirection { get; set; }
        // 타이틀 설정
        public float? TitleSize { get; set; }
        public string? Title { get; set; }
        // 입력 설정
        public float? InputSize { get; set; }
        public string? InputValue { get; set; }
        // 버튼 설정
        public float? ButtonSize { get; set; }
        public string? ButtonText { get; set; }
        // 슬라이더 라벨 설정
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;
        // 슬라이더 배경 설정
        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; }
        // 슬라이더 설정
        public string TextColor { get; set; } = "Fore";
        public string BgColor { get; set; } = "Base3";
        public string SliderColor { get; set; } = "danger";
        public GoRoundType Round { get; set; } = GoRoundType.All;
        public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        // 슬라이더 값 설정(외부)
        public float Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; } = 100D;
        // 슬라이더 값 설정(내부)
        private bool isDragging;
        private SKRect trackRect;
        private SKRect handleRect;
        private const float HandleRadius = 10f;
        private const float TrackHeight = 4f;
        // 슬라이더 상태값 설정
        private List<GoButtonInfo> Buttons { get; set; } = [];
        private bool UseTitle => TitleSize is > 0;
        private bool UseInput => InputSize is > 0;
        private bool UseButton => ButtonSize is > 0;
        #endregion

        #region Event
        public event EventHandler? SliderDragStarted;
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        protected GoSlider()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool sDown;
        private bool sHover;
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            DrawSlider(canvas);
            base.OnDraw(canvas);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (CollisionTool.Check(handleRect, x, y))
            {
                sDown = true;
                isDragging = true;
                SliderDragStarted?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseDown(x, y, button);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            sHover = CollisionTool.Check(handleRect, x, y);

            if (isDragging)
            {
                // 이전 값 저장
                var oldValue = Value;
                UpdateValueFromPosition(x);
                // 값이 실제로 변경되었을 때만 이벤트 발생
                if (Math.Abs(oldValue - Value) > 0.00001f)
                {
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            base.OnMouseMove(x, y);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (sDown)
            {
                sDown = false;
                if (CollisionTool.Check(handleRect, x, y)) SliderDragStarted?.Invoke(this, EventArgs.Empty);
            }
            isDragging = false;

            base.OnMouseUp(x, y, button);
        }
        #endregion
        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();

            var rts = Direction == GoDirectionHV.Vertical ? Util.Rows(dic["Content"], [$"{TitleSize ?? 0}px", "100%", $"{InputSize ?? 0}px", $"{ButtonSize ?? 0}px"])
                                                          : Util.Columns(dic["Content"], [$"{TitleSize ?? 0}px", "100%", $"{InputSize ?? 0}px", $"{ButtonSize ?? 0}px"]);
            dic["Title"] = rts[0];
            dic["Value"] = rts[1];
            dic["Input"] = rts[2];
            dic["Button"] = rts[3];

            return dic;
        }
        #endregion
        #endregion

        #region Abstract
        protected abstract void OnDrawValue(SKCanvas canvas, SKRect rect);
        #endregion

        #region Method
        #region Draw
        #region DrawSlider
        private void DrawSlider(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var colors = GetThemeColors(thm);
            var areas = Areas();
            var contentBox = areas["Content"];

            var useT = UseTitle;
            var useI = UseInput;
            var useB = UseButton;
            var rnds = Util.Rounds(Direction, Round, (TitleSize.HasValue ? 1 : 0) + 1 + + (InputSize.HasValue ? 1 : 0) + (ButtonSize.HasValue ? 1 : 0));
            var rndTitle = TitleSize.HasValue ? rnds[0] : GoRoundType.Rect;
            var rndValue = TitleSize.HasValue ? rnds[1] : rnds[0];
            var rndInput = InputSize.HasValue ? rnds[2] : GoRoundType.Rect;
            var rndButton = ButtonSize.HasValue ? rnds[^1] : GoRoundType.Rect;
            var rndButtons = Util.Rounds(GoDirectionHV.Horizon, rndButton, ButtonSize.HasValue ? Buttons.Count : 0);


            DrawSliderBackground(canvas, thm, contentBox, colors.background);
            UpdateLayout(contentBox);   // 슬라이더 트랙과 핸들 값 업데이트
            DrawSliderTrack(canvas, handleRect, colors.slider);
            DrawSliderProgress(canvas, thm, contentBox, colors.slider);
            DrawSliderHandle(canvas, handleRect, colors.slider);
        }
        #endregion
        #region DrawSliderBackground
        private void DrawSliderBackground(SKCanvas canvas, GoTheme thm, SKRect rect, SKColor color)
        {
            // 배경 그리기
            if (BackgroundDraw)
            {
                Util.DrawBox(canvas, rect, BorderOnly ? SKColors.Transparent : color, Round, thm.Corner);
            }
        }
        #endregion
        #region DrawSliderTrack
        private void DrawSliderTrack(SKCanvas canvas, SKRect rect, SKColor color)
        {
            using var trackPaint = new SKPaint();
            trackPaint.IsAntialias = true;
            trackPaint.Color = color;
            trackPaint.Style = SKPaintStyle.Fill;
            canvas.DrawRoundRect(trackRect, TrackHeight / 2, TrackHeight / 2, trackPaint);
        }
        #endregion
        #region DrawSliderProgress
        private void DrawSliderProgress(SKCanvas canvas, GoTheme thm, SKRect contentBox, SKColor color)
        {
            // 슬라이더 진행 트랙 그리기
            using var progressPaint = new SKPaint();
            progressPaint.IsAntialias = true;
            progressPaint.Color = color;
            progressPaint.Style = SKPaintStyle.Fill;
            var progressRect = trackRect;
            progressRect.Right = handleRect.MidX;
            canvas.DrawRoundRect(progressRect, TrackHeight / 2, TrackHeight / 2, progressPaint);
        }
        #endregion
        #region DrawSliderHandle
        private void DrawSliderHandle(SKCanvas canvask, SKRect rect, SKColor color)
        {
            // 핸들 그리기
            using var handlePaint = new SKPaint();
            handlePaint.IsAntialias = true;
            handlePaint.Color = color;
            handlePaint.Style = SKPaintStyle.Fill;
            canvask.DrawCircle(handleRect.MidX, handleRect.MidY, HandleRadius, handlePaint);
        }
        #endregion
        #endregion

        #region Functions
        #region GetThemeColors
        private (SKColor text, SKColor background, SKColor slider) GetThemeColors(GoTheme theme)
        {
            var brightness = sDown ? theme.DownBrightness : 0;
            return (
                theme.ToColor(TextColor).BrightnessTransmit(brightness),
                theme.ToColor(BgColor),
                theme.ToColor(SliderColor).BrightnessTransmit(brightness)
            );
        }
        #endregion
        #region UpdateLayout
        private void UpdateLayout(SKRect contentBox)
        {
            if (Direction == GoDirectionHV.Horizon)
            {
                // 수평 슬라이더
                var trackY = contentBox.MidY - TrackHeight / 2;
                trackRect = new SKRect(
                    contentBox.Left + HandleRadius,
                    trackY,
                    contentBox.Right - HandleRadius,
                    trackY + TrackHeight
                );
            }
            else
            {
                // 수직 슬라이더
                var trackX = contentBox.MidX - TrackHeight / 2;
                trackRect = new SKRect(
                    trackX,
                    contentBox.Top + HandleRadius,
                    trackX + TrackHeight,
                    contentBox.Bottom - HandleRadius
                );
            }

            // 핸들 위치 계산
            var t = (float)((Value - Minimum) / (Maximum - Minimum));
            t = Math.Max(0, Math.Min(1, t));

            float handleX, handleY;
            if (Direction == GoDirectionHV.Horizon)
            {
                handleX = trackRect.Left + t * trackRect.Width;
                handleY = trackRect.MidY;
            }
            else
            {
                handleX = trackRect.MidX;
                handleY = trackRect.Bottom - t * trackRect.Height;
            }

            handleRect = new SKRect(
                handleX - HandleRadius,
                handleY - HandleRadius,
                handleX + HandleRadius,
                handleY + HandleRadius
            );
        }
        #endregion
        #region UpdateValueFromPosition
        private void UpdateValueFromPosition(float position)
        {
            float t;
            if (Direction == GoDirectionHV.Horizon)
            {
                t = (position - trackRect.Left) / trackRect.Width;
            }
            else
            {
                t = 1 - (position - trackRect.Top) / trackRect.Height;
            }

            t = Math.Max(0, Math.Min(1, t));
            Value = (float)(Minimum + t * (Maximum - Minimum));
        }
        #endregion
        #endregion
        #endregion
    }

    public class GoSliderBasic : GoSlider
    {
        #region Properties
        private string sVal = "";
        public string Value
        {
            get => sVal;
            set
            {
                if (sVal != value)
                {
                    sVal = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, SKRect rtValue)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);

            Util.DrawText(canvas, Value, FontName, FontSize, rtValue, cText);
        }
        #endregion
    }
}

