using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System.Globalization;

namespace Going.UI.Controls
{
    /// <summary>
    /// 제네릭 슬라이더 컨트롤 - 성능 최적화, 바인딩 지원, 시각적 개선 및 사용자 경험 향상 기능 포함
    /// </summary>
    public class GoSlider : GoControl
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
        private double sValue;
        public double Value {
            get => sValue;
            set {
                var clampedValue = MathClamp(value, Minimum, Maximum);
                if (!(Math.Abs(sValue - value) > 0.00001f)) return;
                sValue = clampedValue;
                ValueChanged?.Invoke(this, EventArgs.Empty);
                UpdateValueString();
                needsLayoutUpdate = true;
            }
        }
        private string sValueString;
        public string ValueString
        {
            get => sValueString;
            private set
            {
                if (string.Equals(sValueString, value, StringComparison.Ordinal)) return;
                sValueString = value;
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
                Value = Math.Max(value, Value);
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
                Value = Math.Min(value, Value);
                needsLayoutUpdate = true;
            }
        }
        #endregion

        #region 슬라이더 값 설정(내부)
        private bool isDragging;
        private SKRect trackRect;
        private SKRect handleRect;
        private const float TrackHeight = 4f;
        private int MinHeightForLabelBelow { get; set; } = 80;  // 라벨을 아래에 표시하기 위한 최소 높이
        #endregion

        #region 슬라이더 상태값 설정
        private List<GoIconButton> Buttons { get; set; } = [];
        #endregion

        #region 캐싱된 값들
        private double normalizedValue; // 0.0 ~ 1.0 사이의 정규화된 값
        private bool needsLayoutUpdate = true;
        #endregion

        #region 성능 최적화를 위한 페인트 객체 재사용
        private readonly SKPaint trackPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint progressPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint handlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint tickPaint = new SKPaint {IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint borderPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        private readonly SKPaint textPaint = new SKPaint { IsAntialias = true };
        private SKTextAlign textAlign = SKTextAlign.Center;
        private readonly SKFont textFont = new SKFont();
        #endregion

        #endregion

        #region Event

        public event EventHandler? SliderDragStarted;
        public event EventHandler? SliderDragCompleted;
        public event EventHandler? ValueChanged;

        #endregion

        #region Constructor
        public GoSlider()
        {
            Selectable = true;
            InitializeDefaults();
        }
        #endregion

        #region Member Variable
        private bool sDown;
        private bool sHover;
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

            if (CollisionTool.Check(handleRect, x, y))
            {
                sDown = true;
                isDragging = true;
                SliderDragStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (CollisionTool.Check(trackRect, x, y))
            {
                // 트랙 클릭 시 즉시 해당 위치로 이동
                UpdateValueFromPosition(x, y);
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
            mx = x;
            my = y;
            sHover = CollisionTool.Check(handleRect, x, y);

            // var rts = Areas();
            // var rtBox = rts["Content"];

            if (isDragging)
            {
                UpdateValueFromPosition(x, y);
            }

            base.OnMouseMove(x, y);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            mx = x;
            my = y;

            // var rts = Areas();
            // var rtBox = rts["Content"];

            if (sDown)
            {
                sDown = false;
                if (isDragging) SliderDragCompleted?.Invoke(this, EventArgs.Empty);
                // if (CollisionTool.Check(handleRect, x, y)) SliderDragStarted?.Invoke(this, EventArgs.Empty);
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

            if (needsLayoutUpdate)
            {
                UpdateLayout(contentBox);
                needsLayoutUpdate = false;
            }

            if (ShowTicks)
            {
                DrawTicks(canvas, thm, colors.slider);
            }

            //DrawSliderBackground(canvas, thm, contentBox, colors.background);
            UpdateLayout(contentBox);   // 슬라이더 트랙과 핸들 값 업데이트
            DrawSliderTrack(canvas, colors.slider);
            DrawSliderProgress(canvas, thm, contentBox, colors.slider);
            DrawSliderHandle(canvas, handleRect, colors.slider);

            if (ShowValueLabel)
            {
                DrawValueLabel(canvas, colors.text);
            }

            // Util.DrawText(canvas, ValueString, FontName, FontSize, handleRect, SKColors.Aqua);
        }
        #endregion
        #region DrawTicks
        private void DrawTicks(SKCanvas canvas, GoTheme thm, SKColor color)
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
        private void DrawSliderProgress(SKCanvas canvas, GoTheme thm, SKRect contentBox, SKColor color)
        {
            // 슬라이더 진행 트랙 그리기
            progressPaint.Color = color;

            var progressRect = trackRect;

            if (Direction == GoDirectionHV.Horizon)
            {
                progressRect.Right = handleRect.MidX;
            }
            else
            {
                // 수직 방향에서는 아래에서 위로 채워짐
                // progressRect = new SKRect(trackRect.Left, trackRect.Top, trackRect.Right, handleRect.MidY);
                progressRect.Bottom = handleRect.MidY;
            }

            if (EnableShadow)
            {
                using var shadow = SKImageFilter.CreateDropShadow(2, 2, 3, 3, new SKColor(0, 0, 0, 80));
                progressPaint.ImageFilter = shadow;
            }

            canvas.DrawRoundRect(progressRect, TrackHeight / 2, TrackHeight / 2, progressPaint);
            progressPaint.ImageFilter = null;
        }
        #endregion
        #region DrawSliderHandle
        private void DrawSliderHandle(SKCanvas canvas, SKRect rect, SKColor color)
        {
            handlePaint.Color = sDown ? color.WithAlpha(230) : color;

            // 그림자 효과
            if (EnableShadow)
            {
                using var shadow = SKImageFilter.CreateDropShadow(2, 2, 3, 3, new SKColor(0, 0, 0, 80));
                handlePaint.ImageFilter = shadow;
            }

            // 핸들 그리기
            var radius = HandleRadius;
            if (isDragging) radius *= 1.1f;
            else if (sHover) radius *= 1.05f;

            canvas.DrawCircle(handleRect.MidX, handleRect.MidY, HandleRadius, handlePaint);
            handlePaint.ImageFilter = null;

            // 핸들 내부 하이라이트 효과
            handlePaint.Color = SKColors.White.WithAlpha(80);
            canvas.DrawCircle(handleRect.MidX - radius * 0.2f, handleRect.MidY - radius * 0.2f, radius * 0.6f, handlePaint);
        }
        #endregion
        #region DrawValueLabel
        private void DrawValueLabel(SKCanvas canvas, SKColor color)
        {
            textPaint.Color = color;
            textAlign = SKTextAlign.Center;
            textFont.Size = FontSize;
            textFont.Typeface = SKTypeface.FromFamilyName(FontName);

            // 텍스트 측정하여 공간 계산
            var textBounds = new SKRect();
            textFont.MeasureText(ValueString);
            var isHeightEnough = Bounds.Height >= MinHeightForLabelBelow;

            SKPoint labelPosition;
            if (Direction == GoDirectionHV.Horizon)
            {
                if (isHeightEnough)
                {
                    // 충분한 공간이 있을 때 - 핸들 아래에 표시
                    labelPosition = new SKPoint(handleRect.MidX, handleRect.MidY + HandleRadius + FontSize + 5);
                }
                else
                {
                    // 공간이 좁을 때 - 핸들과 겹치게 표시 (중앙에)
                    labelPosition = new SKPoint(handleRect.MidX, handleRect.MidY - textBounds.Height / 4);

                    // 핸들과 겹치므로 대비를 위해 흰색으로 표시
                    textPaint.Color = SKColors.White;
                }
            }
            else
            {
                if (isHeightEnough)
                {
                    // 수직 슬라이더의 경우 - 핸들 오른쪽에 표시
                    labelPosition = new SKPoint(handleRect.MidX + HandleRadius + 10, handleRect.MidY);
                    textAlign = SKTextAlign.Left;
                    textPaint.Color = color;
                }
                else
                {
                    // 공간이 좁을 때 - 핸들 위에 표시
                    labelPosition = new SKPoint(handleRect.MidX + HandleRadius + 10, handleRect.MidY);
                    textAlign = SKTextAlign.Center;

                    // 핸들과 겹치므로 대비를 위해 흰색으로 표시
                    textPaint.Color = SKColors.White;
                }
            }

            canvas.DrawText(ValueString, labelPosition, textAlign, textFont, textPaint);
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
            sValue = 0D;
            sValueString = "0";

            UpdateValueString();
        }
        #endregion
        #region UpdateValueString
        private void UpdateValueString()
        {
            ValueString = string.IsNullOrWhiteSpace(ValueFormat) ? Value.ToString(CultureInfo.InvariantCulture) : Value.ToString(ValueFormat);
        }
        #endregion
        #region OnValueStringChanged
        private void OnValueStringChanged()
        {
            UpdateValueString();
        }
        #endregion

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
            // 값 정규화 업데이트
            normalizedValue = (Value - Minimum) / (Maximum - Minimum);
            normalizedValue = MathClamp(normalizedValue, 0, 1);

            // 트랙 위치 업데이트
            var availableSpace = Direction == GoDirectionHV.Horizon ? contentBox.Height : contentBox.Width;
            var isHeightEnough = Bounds.Height >= MinHeightForLabelBelow;

            // 값 레이블을 위한 여분 공간 계산
            var extraSpace = isHeightEnough && ShowValueLabel ? FontSize + 10 : 0;

            if (Direction == GoDirectionHV.Horizon)
            {
                // 수평 슬라이더
                var trackY = contentBox.MidY;

                // 화면이 작을 경우 라벨이 위로 가므로 슬라이더를 약간 아래로 조정
                if (!isHeightEnough && ShowValueLabel) trackY += extraSpace / 2;

                trackRect = new SKRect(
                    contentBox.Left + HandleRadius,
                    trackY - (float)BarSize / 2,
                    contentBox.Right - HandleRadius,
                    trackY + (float)BarSize / 2
                );
            }
            else
            {
                // 수직 슬라이더
                var trackX = contentBox.MidX;
                trackRect = new SKRect(
                    trackX - (float)BarSize / 2,    // 오류 : 여기가 문제 > 수정 = 기호를 +에서 -로 변경
                    contentBox.Top + HandleRadius + (isHeightEnough ? extraSpace / 2 : 0),
                    trackX + (float)BarSize / 2,    // 오류 : 여기도 동일한 값
                    contentBox.Bottom - HandleRadius - (isHeightEnough ? 0 : extraSpace / 2)
                );
            }

            // 핸들 위치 계산
            float handleX, handleY;
            if (Direction == GoDirectionHV.Horizon)
            {
                handleX = trackRect.Left + (float)normalizedValue * trackRect.Width;
                handleY = trackRect.MidY;
            }
            else
            {
                handleX = trackRect.MidX;
                handleY = trackRect.Bottom - (float)normalizedValue * trackRect.Height;
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
        private void UpdateValueFromPosition(float x, float y)
        {

            var normalizedPos = Direction == GoDirectionHV.Horizon ? MathClamp((x - trackRect.Left) / trackRect.Width, 0, 1) : MathClamp(1 - (y - trackRect.Top) / trackRect.Height, 0, 1);

            var newValue = Minimum + normalizedPos * (Maximum - Minimum);

            if (Tick is > 0)
            {
                // 틱 값에 맞게 조정
                newValue = Math.Round(newValue / Tick.Value) * Tick.Value;
            }

            Value = MathClamp(newValue, Minimum, Maximum);
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
        #region IncrementalValue
        public void IncrementalValue()
        {
            var step = (Maximum - Minimum) / 100.0;
            if (Tick is > 0)
            {
                step = Tick.Value;
            }
            else
            {
                // 최소 스텝은 0.1
                step = Math.Max(0.1, step);
            }

            Value = MathClamp(Value + step, Minimum, Maximum);
        }
        #endregion
        #region DecrementalValue
        public void DecrementalValue()
        {
            var step = (Maximum - Minimum) / 100.0;
            if (Tick is > 0)
            {
                step = Tick.Value;
            }
            else
            {
                // 최소 스텝은 0.1
                step = Math.Max(0.1, step);
            }

            Value = MathClamp(Value - step, Minimum, Maximum);
        }
        #endregion
        #region SetValueToMinimum
        public void SetValueToMinimum()
        {
            Value = Minimum;
        }
        #endregion
        #region SetValueMaximum
        public void SetValueToMaximum()
        {
            Value = Maximum;
        }
        #endregion
        #endregion
        #endregion
    }
}

