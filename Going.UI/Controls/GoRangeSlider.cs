using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System.Globalization;

namespace Going.UI.Controls
{
    // todo : 화면에서 이상하게 출력되는 이유를 알았다. : Event가 발생을 했을 때, 해당 이벤트 동작이 일어나면서 다시 그려지는데, 그 때, 이벤트가 발생한 위치를 기준으로 다시 그리기 때문에, 이상하게 출력되는 것이다.
    /// <summary>
    /// 범위 선택 슬라이더 컨트롤 - 두 개의 핸들을 사용하여 최소값과 최대값을 선택할 수 있는 컴포넌트
    /// </summary>
    public class GoRangeSlider : GoControl, IDisposable
    {
        #region Properties

        #region 아이콘 설정
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;
        public GoDirectionHV IconDirection { get; set; }
        #endregion

        #region 슬라이더 라벨 설정
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;
        #endregion

        #region 슬라이더 배경 설정
        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; }
        #endregion

        #region 슬라이더 설정
        public string TextColor { get; set; } = "Fore";
        public string BgColor { get; set; } = "Base3";
        public string SliderColor { get; set; } = "danger";
        public string EmptyColor { get; set; } = "Base2";
        public string BorderColor { get; set; } = "danger";
        public GoRoundType Round { get; set; } = GoRoundType.All;
        public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        #endregion

        #region 슬라이더 표시 설정(외형)
        public bool ShowValueLabel { get; set; } = true;
        public string ValueFormat { get; set; } = "F1";
        public int BarSize { get; set; } = 4;
        public float HandleRadius { get; set; } = 15f;
        public bool EnableShadow { get; set; } = true;
        #endregion

        #region 틱(단계) 설정
        public double? Tick { get; set; } = null;
        public bool ShowTicks { get; set; } = false;
        public int TickCount { get; set; } = 5;
        public float TickSize { get; set; } = 10f;
        #endregion

        #region 슬라이더 값 설정(외부)
        private double sLowerValue;
        public double LowerValue
        {
            get => sLowerValue;
            set
            {
                var clampedValue = MathClamp(value, Minimum, UpperValue);
                if (Math.Abs(sLowerValue - clampedValue) < 0.00001f) return;

                sLowerValue = clampedValue;
                LowerValueChanged?.Invoke(this, EventArgs.Empty);
                RangeChanged?.Invoke(this, EventArgs.Empty);
                UpdateValueString();
                needsLayoutUpdate = true;
            }
        }
        private double sUpperValue;
        public double UpperValue
        {
            get => sUpperValue;
            set
            {
                var clampedValue = MathClamp(value, LowerValue, Maximum);
                if (Math.Abs(sUpperValue - value) < 0.00001f) return;
                sUpperValue = clampedValue;
                UpperValueChanged?.Invoke(this, EventArgs.Empty);
                RangeChanged?.Invoke(this, EventArgs.Empty);
                UpdateValueString();
                needsLayoutUpdate = true;
            }
        }
        private string? sLowerValueString;
        public string? LowerValueString
        {
            get => sLowerValueString;
            private set
            {
                if (string.Equals(sLowerValueString, value, StringComparison.Ordinal)) return;
                sLowerValueString = value;
            }
        }
        private string? sUpperValueString;
        public string? UpperValueString
        {
            get => sUpperValueString;
            private set
            {
                if (string.Equals(sUpperValueString, value, StringComparison.Ordinal)) return;
                sUpperValueString = value;
            }
        }
        private double sMinimum;
        public double Minimum
        {
            get => sMinimum;
            set
            {
                if (Math.Abs(sMinimum - value) < 0.00001f) return;
                sMinimum = value;
                // 최소값이 변경되면 현재 값도 제한 범위 내로 조정
                LowerValue = Math.Max(value, LowerValue);
                needsLayoutUpdate = true;
            }
        }
        private double sMaximum = 100D;
        public double Maximum
        {
            get => sMaximum;
            set
            {
                if (Math.Abs(sMaximum - value) < 0.00001f) return;
                sMaximum = value;
                UpperValue = Math.Min(value, UpperValue);
                needsLayoutUpdate = true;
            }
        }
        #endregion

        #region 슬라이더 값 설정(내부)
        private bool isDraggingLower;
        private bool isDraggingUpper;
        private SKRect trackRect;
        private SKRect lowerHandleRect;
        private SKRect upperHandleRect;
        private const float TrackHeight = 4f;
        private int MinHeightForLabelBelow { get; set; } = 80;  // 라벨을 아래에 표시하기 위한 최소 높이
        public float MinHandleSeparation { get; set; } = 0.05f; // 핸들 간 최소 간격 (정규화된 값 0-1)
        #endregion

        #region 슬라이더 상태값 설정
        private List<GoIconButton> Buttons { get; set; } = [];
        #endregion

        #region 캐싱된 값들
        private double normalizedLowerValue; // 0.0 ~ 1.0 사이의 정규화된 값
        private double normalizedUpperValue;
        private bool needsLayoutUpdate = true;
        #endregion

        #region 성능 최적화를 위한 페인트 객체 재사용
        private readonly SKPaint trackPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint progressPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint handlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint tickPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint borderPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        private readonly SKPaint textPaint = new SKPaint { IsAntialias = true };
        private SKTextAlign textAlign = SKTextAlign.Center;
        private readonly SKFont textFont = new SKFont();
        #endregion

        #endregion

        #region Event

        public event EventHandler? LowerValueChanged;
        public event EventHandler? UpperValueChanged;
        public event EventHandler? RangeChanged;
        public event EventHandler? SliderDragStarted;
        public event EventHandler? SliderDragCompleted;

        #endregion

        #region Constructor
        public GoRangeSlider()
        {
            Selectable = true;
            InitializeDefaults();
        }
        #endregion

        #region Member Variable

        private bool sLowerDown;
        private bool sUpperDown;
        private bool sLowerHover;
        private bool sUpperHover;
        private float mx, my;   // 마우스 위치 추적

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
            mx = x;
            my = y;
            var rts = Areas();
            var rtBox = rts["Content"];

            // 핸들 충돌 검사 (우선순위 : 낮은 값 핸들, 높은 값 핸들, 트랙)
            if (CollisionTool.Check(lowerHandleRect, x, y))
            {
                sLowerDown = true;
                isDraggingLower = true;
                SliderDragStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (CollisionTool.Check(upperHandleRect, x, y))
            {
                sUpperDown = true;
                isDraggingUpper = true;
                SliderDragStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (CollisionTool.Check(trackRect, x, y))
            {
                // 트랙 클릭 - 가장 가까운 핸들을 찾아 이동
                UpdateClosesHandleFromPosition(x, y);
                SliderDragStarted?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseDown(x, y, button);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
            mx = x;
            my = y;

            // 호버 상태 업데이트
            sLowerHover = CollisionTool.Check(lowerHandleRect, x, y);
            sUpperHover = CollisionTool.Check(upperHandleRect, x, y);

            // 드래그 처리
            if (isDraggingLower)
            {
                UpdateLowerValueFromPosition(x, y);
            }
            else if (isDraggingUpper)
            {
                UpdateUpperValueFromPosition(x, y);
            }

            base.OnMouseMove(x, y);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            mx = x;
            my = y;

            var wasDragging = isDraggingLower || isDraggingUpper;

            // 드래그 상태 및 다운 상태 초기화
            sLowerDown = false;
            sUpperDown = false;
            isDraggingLower = false;
            isDraggingUpper = false;

            // 드래그 완료 이벤트 발생
            if (wasDragging)
            {
                SliderDragCompleted?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseUp(x, y, button);
        }
        #endregion

        #endregion

        #region Methods

        #region Draw

        #region DrawSlider
        private void DrawSlider(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var colors = GetThemeColors(thm);
            var areas = Areas();
            var contentBox = areas["Content"];

            if (needsLayoutUpdate)
            {
                UpdateLayout(contentBox);
                needsLayoutUpdate = false;
            }

            if (ShowTicks)
            {
                DrawTicks(canvas, colors.slider);
            }

            DrawSliderBackground(canvas, thm, contentBox, colors.background);
            DrawSliderTrack(canvas, colors.empty);
            DrawSliderProgress(canvas, colors.slider);

            // 두 핸들 그리기
            DrawsSliderHandle(canvas, lowerHandleRect, colors.slider, sLowerDown, sLowerHover);
            DrawsSliderHandle(canvas, upperHandleRect, colors.slider, sUpperDown, sUpperHover);

            if (ShowValueLabel)
            {
                DrawValueLabels(canvas, colors.text);
            }
        }
        #endregion
        #region DrawTicks
        private void DrawTicks(SKCanvas canvas, SKColor color)
        {
            tickPaint.Color = color.WithAlpha(150);
            tickPaint.StrokeWidth = 1;

            var count = TickCount;
            if (Tick.HasValue)
            {
                // 틱 값이 설정되었으면 범위에 맞게 틱 개수 계산
                count = (int)Math.Ceiling((Maximum - Minimum) / Tick.Value);
            }

            for (var i = 0; i <= count; i++)
            {
                double tickValue;
                if (Tick.HasValue)
                {
                    tickValue = Minimum + i * Tick.Value;
                    if (tickValue > Maximum) break;
                }
                else
                {
                    tickValue = Minimum + (Maximum - Minimum) * i / count;
                }

                var normalizedPos = (float)((tickValue - Minimum) / (Maximum - Minimum));
                float tickX, tickY;

                if (Direction == GoDirectionHV.Horizon)
                {
                    tickX = trackRect.Left + normalizedPos * trackRect.Width;
                    tickY = trackRect.MidY;

                    canvas.DrawLine(tickX, tickY - TickSize / 2, tickX, tickY + TickSize / 2, tickPaint);
                }
                else
                {
                    tickX = trackRect.MidX;
                    tickY = trackRect.Bottom - normalizedPos * trackRect.Height;

                    canvas.DrawLine(tickX - TickSize / 2, tickY, tickX + TickSize / 2, tickY, tickPaint);
                }
            }
        }
        #endregion
        #region DrawSliderBackground
        private void DrawSliderBackground(SKCanvas canvas, GoTheme thm, SKRect rect, SKColor color)
        {
            if (BackgroundDraw)
            {
                // 배경 그리기
                Util.DrawBox(canvas, rect, BorderOnly ? SKColors.Transparent : color, Round, thm.Corner);

                // 테두리 그리기
                if (BorderOnly) return;
                borderPaint.Color = thm.ToColor(BorderColor);
                borderPaint.StrokeWidth = 1;
            }
        }
        #endregion
        #region DrawSliderTrack
        private void DrawSliderTrack(SKCanvas canvas, SKColor color)
        {
            trackPaint.Color = color;
            canvas.DrawRoundRect(trackRect, TrackHeight / 2, TrackHeight / 2, trackPaint);
        }
        #endregion
        #region DrawSliderProgress
        private void DrawSliderProgress(SKCanvas canvas, SKColor color)
        {
            // 슬라이더 진행 트랙 그리기 (두 핸들 사이)
            progressPaint.Color = color;

            SKRect progressRect;
            if (Direction == GoDirectionHV.Horizon)
            {
                // 수평 방향 : 낮은 값 핸들에서 높은 값 핸들까지
                progressRect = new SKRect(lowerHandleRect.MidX, trackRect.Top, upperHandleRect.MidX, trackRect.Bottom);
            }
            else
            {
                // 수직 방향 : 낮은 값 핸들에서 높은 값 핸들까지
                progressRect = new SKRect(trackRect.Left, upperHandleRect.MidY, trackRect.Right, lowerHandleRect.MidY);
            }

            // 프로그레스 바 유효성 검사 - 너비나 높이가 항상 양수가 되도록
            var isValidProgress = false;

            if (Direction == GoDirectionHV.Horizon)
            {
                isValidProgress = progressRect.Width > 0;
            }
            else
            {
                isValidProgress = progressRect.Height > 0;
            }

            if (isValidProgress)
            {
                if (EnableShadow)
                {
                    using var shadow = SKImageFilter.CreateDropShadow(2, 2, 3, 3, new SKColor(0, 0, 0, 80));
                    progressPaint.ImageFilter = shadow;
                }

                canvas.DrawRoundRect(progressRect, TrackHeight / 2, TrackHeight / 2, progressPaint);
                progressPaint.ImageFilter = null;
            }
        }
        #endregion
        #region DrawSliderHandle
        private void DrawsSliderHandle(SKCanvas canvas, SKRect rect, SKColor color, bool isDown, bool isHover)
        {
            handlePaint.Color = isDown ? color.WithAlpha(230) : color;

            // 그림자 효과
            if (EnableShadow)
            {
                using var shadow = SKImageFilter.CreateDropShadow(2, 2, 3, 3, new SKColor(0, 0, 0, 80));
                handlePaint.ImageFilter = shadow;
            }

            // 핸들 그리기
            var radius = HandleRadius;
            if (isDown) radius *= 1.1f;
            else if (isHover) radius *= 1.05f;

            canvas.DrawCircle(rect.MidX, rect.MidY, radius, handlePaint);
            handlePaint.ImageFilter = null;

            // 핸들 내부 하이라이트 효과
            handlePaint.Color = SKColors.White.WithAlpha(80);
            canvas.DrawCircle(rect.MidX - radius * 0.2f, rect.MidY - radius * 0.2f, radius * 0.6f, handlePaint);
        }
        #endregion
        #region DrawValueLabels
        private void DrawValueLabels(SKCanvas canvas, SKColor color)
        {
            textPaint.Color = color;
            textAlign = SKTextAlign.Center;
            textFont.Size = FontSize;
            textFont.Typeface = SKTypeface.FromFamilyName(FontName);

            // 텍스트 측정하여 공간 계산
            var isHeightEnough = Bounds.Height >= MinHeightForLabelBelow;
            
            // 핸들이 너무 가까이 있는지 확인
            bool areHandleClose;

            if (Direction == GoDirectionHV.Horizon)
            {
                // 수평 모드에서는 X 거리 확인
                var handleDistance = Math.Abs(upperHandleRect.MidX - lowerHandleRect.MidX);

                // 두 핸들 사이 거리가 텍스트를 표시하기에 충분한지 확인
                var textWidth = Math.Max(textFont.MeasureText(LowerValueString), textFont.MeasureText(UpperValueString));

                areHandleClose = handleDistance < textWidth * 2 + 10; // 여유 공간 추가
            }
            else
            {
                // 수직 모드에서는 Y 거리 확인
                var handleDistance = Math.Abs(upperHandleRect.MidY - lowerHandleRect.MidY);
                areHandleClose = handleDistance < FontSize * 2 + 10;
            }

            if (Direction == GoDirectionHV.Horizon)
            {
                if (isHeightEnough)
                {
                    // 충분한 공간이 있을 때 - 핸들 아래에 표시
                    if (areHandleClose)
                    {
                        // 핸들이 가까울 때는 사이에 하나의 레이블만 표시
                        var centerX = (lowerHandleRect.MidX + upperHandleRect.MidX) / 2;
                        var rangeText = $"{LowerValueString} - {UpperValueString}";

                        canvas.DrawText(rangeText, centerX, lowerHandleRect.MidY + HandleRadius + FontSize + 5, textAlign, textFont, textPaint);
                    }
                    else
                    {
                        // 각 핸들 아래에 개별 레이블 표시
                        canvas.DrawText(LowerValueString, lowerHandleRect.MidX, lowerHandleRect.MidY + HandleRadius + FontSize + 5, textAlign, textFont, textPaint);
                        canvas.DrawText(UpperValueString, upperHandleRect.MidX, upperHandleRect.MidY + HandleRadius + FontSize + 5, textAlign, textFont, textPaint);
                    }
                }
                else
                {
                    // 공간이 부족할 때 - 핸들과 겹치게 표시
                    textPaint.Color = SKColors.White;

                    // 핸들이 가까울 때는 각 핸들의 값을 표시하는 대신 범위를 표시
                    if (areHandleClose)
                    {
                        canvas.DrawText(LowerValueString?[..Math.Min(3, LowerValueString.Length)], lowerHandleRect.MidX, lowerHandleRect.MidY + FontSize/3, textAlign, textFont, textPaint);
                        canvas.DrawText(UpperValueString?[..Math.Min(3, UpperValueString.Length)], upperHandleRect.MidX, upperHandleRect.MidY + FontSize/3, textAlign, textFont, textPaint);
                    }
                    else
                    {
                        canvas.DrawText(LowerValueString, lowerHandleRect.MidX, lowerHandleRect.MidY + FontSize/3, textAlign, textFont, textPaint);
                        canvas.DrawText(UpperValueString, upperHandleRect.MidX, upperHandleRect.MidY + FontSize/3, textAlign, textFont, textPaint);
                    }
                }
            }
            else
            {
                if (isHeightEnough)
                {
                    // 수직 슬라이더 - 핸들 오른쪽에 표시
                    textAlign = SKTextAlign.Left;
                    textPaint.Color = color;

                    if (areHandleClose)
                    {
                        // 핸들과 가까울 때는 가운데에 범위 표시
                        var centerY = (lowerHandleRect.MidY + upperHandleRect.MidY) / 2;
                        var rangeText = $"{LowerValueString} - {UpperValueString}";

                        canvas.DrawText(rangeText, lowerHandleRect.MidX + HandleRadius + 10, centerY, textAlign, textFont, textPaint);
                    }
                    else
                    {
                        canvas.DrawText(LowerValueString, lowerHandleRect.MidX + HandleRadius + 10, lowerHandleRect.MidY, textAlign, textFont, textPaint);
                        canvas.DrawText(UpperValueString, upperHandleRect.MidX + HandleRadius + 10, upperHandleRect.MidY, textAlign, textFont, textPaint);
                    }
                }
                else
                {
                    // 핸들과 겹치게 표시
                    textAlign = SKTextAlign.Center;
                    textPaint.Color = SKColors.White;

                    if (areHandleClose)
                    {
                        // 공간이 부족할 때는 짧게 표시
                        canvas.DrawText(LowerValueString?[..Math.Min(3, LowerValueString.Length)], lowerHandleRect.MidX, lowerHandleRect.MidY + FontSize/3, textAlign, textFont, textPaint);
                        canvas.DrawText(UpperValueString?[..Math.Min(3, UpperValueString.Length)], upperHandleRect.MidX, upperHandleRect.MidY + FontSize/3, textAlign, textFont, textPaint);
                    }
                    else
                    {
                        canvas.DrawText(LowerValueString, lowerHandleRect.MidX, lowerHandleRect.MidY + FontSize/3, textAlign, textFont, textPaint);
                        canvas.DrawText(UpperValueString, upperHandleRect.MidX, upperHandleRect.MidY + FontSize/3, textAlign, textFont, textPaint);
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Functions

        #region InitializeDefaults
        private void InitializeDefaults()
        {
            // 초기값 설정
            sMinimum = 0D;
            sMaximum = 100D;
            sLowerValue = 25D;
            sUpperValue = 75D;

            UpdateValueString();
        }
        #endregion
        #region GetThemeColors
        private (SKColor text, SKColor background, SKColor slider, SKColor empty) GetThemeColors(GoTheme theme)
        {
            var brightness = sLowerDown || sUpperDown ? theme.DownBrightness : 0;
            return (
                theme.ToColor(TextColor).BrightnessTransmit(brightness),
                theme.ToColor(BgColor),
                theme.ToColor(SliderColor).BrightnessTransmit(brightness),
                theme.ToColor(EmptyColor)
            );
        }
        #endregion

        #region UpdateLayout
        private void UpdateLayout(SKRect contentBox)
        {
            // 값 정규화 업데이트
            normalizedLowerValue = (LowerValue - Minimum) / (Maximum - Minimum);
            normalizedUpperValue = (UpperValue - Minimum) / (Maximum - Minimum);

            normalizedLowerValue = MathClamp(normalizedLowerValue, 0, 1);
            normalizedUpperValue = MathClamp(normalizedUpperValue, 0, 1);

            // 트랙 위치 업데이트
            var isHeightEnough = Bounds.Height >= MinHeightForLabelBelow;

            // 값 레이블을 위한 여분 공간 계산
            var extraSpace = isHeightEnough && ShowValueLabel ? FontSize + 10 : 0;

            if (Direction == GoDirectionHV.Horizon)
            {
                // 수평 슬라이더
                var trackY = contentBox.MidY;

                // 화면이 작을 경우 라벨이 위로 가므로 슬라이더를 약간 아래로 조정
                if (!isHeightEnough && ShowValueLabel) trackY += FontSize / 2;

                trackRect = new SKRect(
                    contentBox.Left + HandleRadius,
                    trackY - (float)BarSize / 2,
                    contentBox.Right - HandleRadius,
                    trackY + (float)BarSize / 2
                );

                // 핸들 위치 계산
                var lowerHandleX = trackRect.Left + (float)normalizedLowerValue * trackRect.Width;
                var upperHandleX = trackRect.Left + (float)normalizedUpperValue * trackRect.Width;

                lowerHandleRect = new SKRect(lowerHandleX - HandleRadius, trackY - HandleRadius, lowerHandleX + HandleRadius, trackY + HandleRadius);
                upperHandleRect = new SKRect(upperHandleX - HandleRadius, trackY - HandleRadius, upperHandleX + HandleRadius, trackY + HandleRadius);
            }
            else
            {
                // 수직 슬라이더
                var trackX = contentBox.MidX;

                trackRect = new SKRect(
                    trackX - (float)BarSize / 2,
                    contentBox.Top + HandleRadius + (isHeightEnough ? extraSpace / 2 : 0),
                    trackX + (float)BarSize / 2,
                    contentBox.Bottom - HandleRadius - (isHeightEnough ? 0 : extraSpace / 2)
                );

                // 핸들 위치 계산 (수직에서는 위쪽이 최대값)
                var lowerHandleY = trackRect.Bottom - (float)normalizedLowerValue * trackRect.Height;
                var upperHandleY = trackRect.Bottom - (float)normalizedUpperValue * trackRect.Height;

                lowerHandleRect = new SKRect(trackX - HandleRadius, lowerHandleY - HandleRadius, trackX + HandleRadius, lowerHandleY + HandleRadius);
                upperHandleRect = new SKRect(trackX - HandleRadius, upperHandleY - HandleRadius, trackX + HandleRadius, upperHandleY + HandleRadius);
            }
        }
        #endregion
        #region UpdateValueString
        private void UpdateValueString()
        {
            LowerValueString = string.IsNullOrWhiteSpace(ValueFormat) ? LowerValue.ToString(CultureInfo.InvariantCulture) : LowerValue.ToString(ValueFormat);
            UpperValueString = string.IsNullOrWhiteSpace(ValueFormat) ? UpperValue.ToString(CultureInfo.InvariantCulture) : UpperValue.ToString(ValueFormat);
        }
        #endregion
        #region UpdateLowerValueFromPosition
        private void UpdateLowerValueFromPosition(float x, float y)
        {
            double normalizedPos;

            if (Direction == GoDirectionHV.Horizon)
            {
                normalizedPos = MathClamp((x - trackRect.Left) / trackRect.Width, 0, 1);
            }
            else
            {
                normalizedPos = MathClamp(1 - (y - trackRect.Top) / trackRect.Height, 0, 1);
            }

            // 상한값에 너무 근접하지 않도록 제한
            normalizedPos = Math.Min(normalizedPos, normalizedUpperValue - MinHandleSeparation);

            var newValue = Minimum + normalizedPos * (Maximum - Minimum);

            if (Tick is > 0)
            {
                // 틱 값에 맞게 조정
                newValue = Math.Round(newValue / Tick.Value) * Tick.Value;
            }

            LowerValue = MathClamp(newValue, Minimum, UpperValue);
        }
        #endregion
        #region UpdateUpperValueFromPosition
        private void UpdateUpperValueFromPosition(float x, float y)
        {
            double normalizedPos;

            if (Direction == GoDirectionHV.Horizon)
            {
                normalizedPos = MathClamp((x - trackRect.Left) / trackRect.Width, 0, 1);
            }
            else
            {
                normalizedPos = MathClamp(1 - (y - trackRect.Top) / trackRect.Height, 0, 1);
            }

            // 하한값에 너무 근접하지 않도록 제한
            normalizedPos = Math.Max(normalizedPos, normalizedLowerValue + MinHandleSeparation);

            var newValue = Minimum + normalizedPos * (Maximum - Minimum);

            if (Tick is > 0)
            {
                // 틱 값에 맞게 조정
                newValue = Math.Round(newValue / Tick.Value) * Tick.Value;
            }

            UpperValue = MathClamp(newValue, LowerValue, Maximum);
        }
        #endregion
        #region UpdateClosesHandleFromPosition
        private void UpdateClosesHandleFromPosition(float x, float y)
        {
            double normalizedPos;

            if (Direction == GoDirectionHV.Horizon)
            {
                normalizedPos = MathClamp((x - trackRect.Left) / trackRect.Width, 0, 1);
            }
            else
            {
                normalizedPos = MathClamp(1 - (y - trackRect.Top) / trackRect.Height, 0, 1);
            }

            // 어느 핸들이 더 가까운지 결정
            if (Math.Abs(normalizedPos - normalizedLowerValue) < Math.Abs(normalizedPos - normalizedUpperValue))
            {
                // 낮은 값 핸들이 더 가까움
                sLowerDown = true;
                isDraggingLower = true;
                UpdateLowerValueFromPosition(x, y);
            }
            else
            {
                // 높은 값 핸들이 더 가까움
                sUpperDown = true;
                isDraggingUpper = true;
                UpdateUpperValueFromPosition(x, y);
            }
        }
        #endregion

        #region MathClamp
        private double MathClamp(double valueData, double min, double max)
        {
            return Math.Max(min, Math.Min(max, valueData));
        }
        #endregion

        #endregion

        #region User Interaction Methods

        #region SetRange
        /// <summary>
        /// 슬라이더의 범위를 한번에 설정합니다.
        /// </summary>
        public void SetRange(double lower, double upper)
        {
            // 값 범위 검증
            lower = MathClamp(lower, Minimum, Maximum);
            upper = MathClamp(upper, Minimum, Maximum);

            var changed = false;

            if (Math.Abs(sLowerValue - lower) > 0.00001f)
            {
                sLowerValue = lower;
                changed = true;
            }

            if (Math.Abs(sUpperValue - upper) > 0.00001)
            {
                sUpperValue = upper;
                changed = true;
            }


            if (changed)
            {
                UpdateValueString();
                RangeChanged?.Invoke(this, EventArgs.Empty);
                needsLayoutUpdate = true;
            }
        }
        #endregion

        #endregion

        #endregion

        #region Dispose
        public void Dispose()
        {
            trackPaint.Dispose();
            progressPaint.Dispose();
            handlePaint.Dispose();
            tickPaint.Dispose();
            borderPaint.Dispose();
            textPaint.Dispose();
            textFont.Dispose();
        }
        #endregion
    }
}
