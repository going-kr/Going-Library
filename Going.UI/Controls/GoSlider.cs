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
        // 라벨 설정
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;
        // 배경 설정
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
        protected override void OnDraw(SKCanvas canvas)
        {
            DrawSlider(canvas);
            base.OnDraw(canvas);
        }
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (CollisionTool.Check(rtBox, x, y)) sDown = true;

            base.OnMouseDown(x, y, button);
        }
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            sHover = CollisionTool.Check(rtBox, x, y);

            base.OnMouseMove(x, y);
        }
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (sDown)
            {
                sDown = false;
                if (CollisionTool.Check(rtBox, x, y)) SliderDragStarted?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseUp(x, y, button);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            // todo : GoInput에 있는 Areas의 Row와 Colums는 grid처럼 구역을 나눈 것을 의미한다.(여기서는 따로 적용해야함)
            return dic;
        }
        #endregion

        #region Method
        #region Draw
        private void DrawSlider(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var colors = GetThemeColors(thm);
            var areas = Areas();
            var contentBox = areas["Content"];

            DrawSliderBackground(canvas, thm, contentBox, colors.background);
            UpdateLayout(contentBox);   // 슬라이더 트랙과 핸들 업데이트
            // DrawSliderContent(canvas, contentBox, colors);
            DrawSliderTrack(canvas, handleRect, colors.slider);
            DrawSliderProgress(canvas, thm, contentBox, colors.slider);
            DrawSliderHandle(canvas, handleRect, colors.slider);
        }
        private void DrawSliderBackground(SKCanvas canvas, GoTheme thm, SKRect rect, SKColor color)
        {
            // 배경 그리기
            if (BackgroundDraw)
            {
                Util.DrawBox(canvas, rect, BorderOnly ? SKColors.Transparent : color.BrightnessTransmit(sHover ? thm.HoverFillBrightness : 0), color.BrightnessTransmit(sHover ? thm.HoverBorderBrightness : 0), Round, thm.Corner);
            }
        }
        private void DrawSliderTrack(SKCanvas canvas, SKRect rect, SKColor color)
        {
            using var trackPaint = new SKPaint();
            trackPaint.IsAntialias = true;
            trackPaint.Color = color;
            trackPaint.Style = SKPaintStyle.Fill;
            canvas.DrawRoundRect(trackRect, TrackHeight / 2, TrackHeight / 2, trackPaint);
        }
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
        private void DrawSliderHandle(SKCanvas canvask, SKRect rect, SKColor color)
        {
            // 핸들 그리기
            using var handlePaint = new SKPaint();
            handlePaint.IsAntialias = true;
            handlePaint.Color = color;
            handlePaint.Style = SKPaintStyle.Fill;
            canvask.DrawCircle(handleRect.MidX, handleRect.MidY, HandleRadius, handlePaint);
        }
        private void DrawSliderContent(SKCanvas canvas, SKRect box, (SKColor text, SKColor bg, SKColor slider) colors)
        {
            if (sDown) box.Offset(0, 1);

            var contentBox = Areas()["Content"];
            Util.DrawCircle(canvas, contentBox,BorderOnly ? SKColors.Transparent : colors.slider);
        }
        #endregion

        #region Functions
        private (SKColor text, SKColor background, SKColor slider) GetThemeColors(GoTheme theme)
        {
            var brightness = sDown ? theme.DownBrightness : 0;
            return (
                theme.ToColor(TextColor).BrightnessTransmit(brightness),
                theme.ToColor(BgColor).BrightnessTransmit(brightness),
                theme.ToColor(SliderColor).BrightnessTransmit(brightness)
            );
        }
        #endregion

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
    }
}

