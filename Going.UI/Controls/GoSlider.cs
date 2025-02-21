using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;

namespace Going.UI.Controls
{
    public class GoSlider : GoControl
    {
        #region Properties
        // 아이콘 설정
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;
        public GoDirectionHV IconDirection { get; set; }
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
        public float Value { get => _value;
            set {
            if (!(Math.Abs(_value - value) > 0.00001f)) return;
            _value = value; ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public string ValueString => Value.ToString("F1");
        public double Minimum { get; set; }
        public double Maximum { get; set; } = 100D;
        // 슬라이더 값 설정(내부)
        private float _value;
        private bool isDragging;
        private SKRect trackRect;
        private SKRect handleRect;
        private const float HandleRadius = 15f;
        private const float TrackHeight = 4f;
        // 슬라이더 상태값 설정
        private List<GoButtonInfo> Buttons { get; set; } = [];
        #endregion

        #region Event
        public event EventHandler? SliderDragStarted;
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public GoSlider()
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
                UpdateValueFromPosition(x);
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

            DrawSliderBackground(canvas, thm, contentBox, colors.background);
            UpdateLayout(contentBox);   // 슬라이더 트랙과 핸들 값 업데이트
            DrawSliderTrack(canvas, handleRect, colors.slider);
            DrawSliderProgress(canvas, thm, contentBox, colors.slider);
            DrawSliderHandle(canvas, handleRect, colors.slider);
            Util.DrawText(canvas, ValueString, FontName, FontSize, handleRect, SKColors.Aqua);
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
        private void DrawSliderHandle(SKCanvas canvas, SKRect rect, SKColor color)
        {
            // 핸들 그리기
            using var handlePaint = new SKPaint();
            handlePaint.IsAntialias = true;
            handlePaint.Color = color;
            handlePaint.Style = SKPaintStyle.Fill;
            canvas.DrawCircle(handleRect.MidX, handleRect.MidY, HandleRadius, handlePaint);
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
}

