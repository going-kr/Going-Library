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
        /**
         * - 속성
         * IconString : 아이콘 문자열
         * IconSize : 아이콘 크기
         * IconDirection : 아이콘 방향
         * IconGap : 아이콘 간격
         * Text : 텍스트
         * FontName : 폰트 이름
         * FontSize : 폰트 크기
         * Direction : 방향
         * TextColor : 텍스트 색상
         * BgColor : 배경 색상
         * SliderColor : 슬라이더 색상
         * Round : 라운드
         * SliderValue : 슬라이더 값
         * BackgroundDraw : 배경 그리기 여부
         * BorderOnly : 테두리만 그리기 여부
         *
         * - 특징
         * Width를 여기서 설정하지 않는다.
         * Width는 컨테이너에서 설정한다.
         * 여기서는 이미 width와 height가 정해져 있다고 생각하고, 그에 맞게 그린다.
         * 프로그레스 바 위에도 값이나 Icon을 적용할 수 있다.(색 등)
         * 하지만 역시나 여기서도 Value를 직접적으로 컨트롤하지 않는다.
         * 그리는 그림 속성만 정의되어 있다.
         * 그 말은 이 컨트롤을 사용할 때 인스턴스를 생성해서 그리는 역할에만 집중하겠다는 의미다.
         */
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; }
        public float IconGap { get; set; } = 5;
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        public string TextColor { get; set; } = "Fore";
        public string BgColor { get; set; } = "Base3";
        public string SliderColor { get; set; } = "danger";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; }

        // 슬라이더 현재 값, 최소값, 최대값
        public float Value { get; set; } = 0f;
        public double Minimum { get; set; } = 0D;
        public double Maximum { get; set; } = 100D;

        // 드래그 상태를 추적하기 위함 변수
        private bool isDragging;
        // 슬라이더 내부 영역(트랙)과 핸들 크기
        private SKRect trackRect;
        private SKRect handleRect;
        // 핸들의 반경 (원형 핸들로 가정)
        private const float HandleRadius = 10f;
        // private bool disable;
        #endregion
        #region Event
        public event EventHandler? SliderDragStarted;
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
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor).BrightnessTransmit(sDown ? thm.DownBrightness : 0);
            var cBg = thm.ToColor(BgColor).BrightnessTransmit(sDown ? thm.DownBrightness : 0);
            var cSlider = thm.ToColor(SliderColor).BrightnessTransmit(sDown ? thm.DownBrightness : 0);
            var rts = Areas();
            var rtBox = rts["Content"];

            if (BackgroundDraw) Util.DrawBox(canvas, rtBox, BorderOnly ? SKColors.Transparent : cBg, Round, thm.Corner);
            if (sDown) rtBox.Offset(0, 1);
            // Util.DrawTextIcon(canvas, Text, FontName, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText);

            // using var trackPaint = new SKPaint();
            // trackPaint.IsAntialias = true;
            // trackPaint.Color = cSlider;
            // trackPaint.Style = SKPaintStyle.Fill;
            //
            // canvas.DrawRect(trackRect, trackPaint);

            var rtc = Areas();
            var rtcBox = rtc["Content"];
            Util.DrawCircle(canvas, rtcBox, BorderOnly ? SKColors.Transparent : cSlider);
            // Util.DrawCircle(canvas, handleRect.MidX, handleRect.MidY, HandleRadius, cSlider);

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
        private void UpdateLayout()
        {
            // 트랙 높이와 위치
            const float trackHeight = 4;
            var trackY = Bounds.MidY - trackHeight / 2;
            trackRect = new SKRect(Bounds.Left + HandleRadius, trackY, Bounds.Right - HandleRadius, trackY + trackHeight);

            // 핸들의 위치는 Value에 따라 결정됨 (선형 보간)
            var t = (Value - Minimum) / (Maximum - Minimum);
            var handleCenterX = trackRect.Left + t * trackRect.Width;
            var handleCenterY = Bounds.MidY;
            handleRect = new SKRect((float)(handleCenterX - HandleRadius), handleCenterY - HandleRadius, (float)(handleCenterX + HandleRadius), handleCenterY + HandleRadius);
        }

        private void UpdateValueFromPosition(float x)
        {
            var t = (x - trackRect.Left) / trackRect.Width;
            Value = (float)(Minimum + t * (Maximum - Minimum));
            UpdateLayout();
        }
        #endregion
    }
}

