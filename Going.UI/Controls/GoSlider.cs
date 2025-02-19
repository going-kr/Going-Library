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
        public float Value { get; set; } = 0f;
        public double Minimum { get; set; } = 0D;
        public double Maximum { get; set; } = 100D;
        // 슬라이더 값 설정(내부)
        private bool isDragging;
        private SKRect trackRect;
        private SKRect handleRect;
        private const float HandleRadius = 10f;
        #endregion

        #region Event
        public event EventHandler? SliderDragStarted;
        #endregion

        #region Constructor
        public GoSlider(bool isDragging, SKRect trackRect, SKRect handleRect)
        {
            this.isDragging = isDragging;
            this.trackRect = trackRect;
            this.handleRect = handleRect;
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
            // var thm = GoTheme.Current;
            // var cText = thm.ToColor(TextColor).BrightnessTransmit(sDown ? thm.DownBrightness : 0);
            // var cBg = thm.ToColor(BgColor).BrightnessTransmit(sDown ? thm.DownBrightness : 0);
            // var cSlider = thm.ToColor(SliderColor).BrightnessTransmit(sDown ? thm.DownBrightness : 0);
            // var rts = Areas();
            // var rtBox = rts["Content"];
            //
            // if (BackgroundDraw) Util.DrawBox(canvas, rtBox, BorderOnly ? SKColors.Transparent : cBg, Round, thm.Corner);
            // if (sDown) rtBox.Offset(0, 1);
            // // Util.DrawTextIcon(canvas, Text, FontName, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText);
            //
            // // using var trackPaint = new SKPaint();
            // // trackPaint.IsAntialias = true;
            // // trackPaint.Color = cSlider;
            // // trackPaint.Style = SKPaintStyle.Fill;
            // //
            // // canvas.DrawRect(trackRect, trackPaint);
            //
            // var rtc = Areas();
            // var rtcBox = rtc["Content"];
            // Util.DrawCircle(canvas, rtcBox, BorderOnly ? SKColors.Transparent : cSlider);
            // // Util.DrawCircle(canvas, handleRect.MidX, handleRect.MidY, HandleRadius, cSlider);

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

            DrawBackground(canvas, contentBox, colors.background);
            DrawSliderContent(canvas, contentBox, colors);
        }
        private void DrawBackground(SKCanvas canvas, SKRect rect, SKColor color)
        {
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = color;
            paint.Style = SKPaintStyle.Fill;

            canvas.DrawRect(rect, paint);
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

        // private void UpdateLayout()
        // {
        //     // 트랙 높이와 위치
        //     const float trackHeight = 4;
        //     var trackY = Bounds.MidY - trackHeight / 2;
        //     trackRect = new SKRect(Bounds.Left + HandleRadius, trackY, Bounds.Right - HandleRadius, trackY + trackHeight);
        //
        //     // 핸들의 위치는 Value에 따라 결정됨 (선형 보간)
        //     var t = (Value - Minimum) / (Maximum - Minimum);
        //     var handleCenterX = trackRect.Left + t * trackRect.Width;
        //     var handleCenterY = Bounds.MidY;
        //     handleRect = new SKRect((float)(handleCenterX - HandleRadius), handleCenterY - HandleRadius, (float)(handleCenterX + HandleRadius), handleCenterY + HandleRadius);
        // }
        //
        // private void UpdateValueFromPosition(float x)
        // {
        //     var t = (x - trackRect.Left) / trackRect.Width;
        //     Value = (float)(Minimum + t * (Maximum - Minimum));
        //     UpdateLayout();
        // }
        #endregion
    }
}

