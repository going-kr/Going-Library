using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System.Globalization;

namespace Going.UI.Controls
{
    public class GoRangeSlider : GoControl, IDisposable
    {
        #region Properties

        #region 아이콘 설정
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 2)] public float IconGap { get; set; } = 5;
        [GoProperty(PCategory.Control, 3)] public GoDirectionHV IconDirection { get; set; }
        #endregion

        #region 슬라이더 라벨 설정
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "slider";
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 6)] public float FontSize { get; set; } = 12;
        #endregion

        #region 슬라이더 배경 설정
        [GoProperty(PCategory.Control, 7)] public bool BackgroundDraw { get; set; }
        [GoProperty(PCategory.Control, 8)] public bool BorderOnly { get; set; }
        #endregion

        #region 슬라이더 설정
        [GoProperty(PCategory.Control, 9)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 10)] public string BoxColor { get; set; } = "Back";
        [GoProperty(PCategory.Control, 11)] public string SliderColor { get; set; } = "Base5";
        [GoProperty(PCategory.Control, 12)] public string ProgressColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control, 13)] public string BorderColor { get; set; } = "danger";
        [GoProperty(PCategory.Control, 14)] public GoRoundType Round { get; set; } = GoRoundType.All;
        [GoProperty(PCategory.Control, 15)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        #endregion

        #region 슬라이더 표시 설정(외형)
        [GoProperty(PCategory.Control, 16)] public bool ShowValueLabel { get; set; } = true;
        [GoProperty(PCategory.Control, 17)] public string ValueFormat { get; set; } = "0";
        [GoProperty(PCategory.Control, 18)] public int BarSize { get; set; } = 4;
        [GoProperty(PCategory.Control, 19)] public float HandleRadius { get; set; } = 15f;
        [GoProperty(PCategory.Control, 20)] public bool EnableShadow { get; set; } = true;
        [GoProperty(PCategory.Control, 21)] public float HandleHoverScale { get; set; } = 1.05f;
        #endregion

        #region 틱(단계) 설정
        [GoProperty(PCategory.Control, 22)] public double? Tick { get; set; } = null;
        [GoProperty(PCategory.Control, 23)] public bool ShowTicks { get; set; } = false;
        [GoProperty(PCategory.Control, 24)] public int TickCount { get; set; } = 5;
        [GoProperty(PCategory.Control, 25)] public float TickSize { get; set; } = 10f;
        #endregion

        #region 슬라이더 값 설정(외부)
        private double sLowerValue;
        [GoProperty(PCategory.Control, 26)]
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
        [GoProperty(PCategory.Control, 27)]
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
        [GoProperty(PCategory.Control, 28)]
        public string? LowerValueString
        {
            get => sLowerValueString;
            set
            {
                if (string.Equals(sLowerValueString, value, StringComparison.Ordinal)) return;
                sLowerValueString = value;
            }
        }
        private string? sUpperValueString;
        [GoProperty(PCategory.Control, 29)]
        public string? UpperValueString
        {
            get => sUpperValueString;
            set
            {
                if (string.Equals(sUpperValueString, value, StringComparison.Ordinal)) return;
                sUpperValueString = value;
            }
        }
        private double sMinimum;
        [GoProperty(PCategory.Control, 30)]
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
        [GoProperty(PCategory.Control, 31)]
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
        [GoProperty(PCategory.Control, 32)] public float MinHandleSeparation { get; set; } = 0.05f; // 핸들 간 최소 간격 (정규화된 값 0-1)
        #endregion

        #region 슬라이더 상태값 설정
        private List<GoIconButton> Buttons { get; set; } = [];
        #endregion

        #region 캐싱된 값들
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
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            DrawSlider(canvas, thm);
            base.OnDraw(canvas, thm);
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
        private void DrawSlider(SKCanvas canvas, GoTheme thm)
        {
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
            UpdateLayout(contentBox);
            DrawSliderTrack(canvas, colors.empty);
            DrawSliderProgress(canvas, colors.slider);

            // 두 핸들 그리기
            DrawsSliderHandle(canvas, lowerHandleRect, colors.slider, sLowerDown, sLowerHover);

            if (ShowValueLabel)
            {
                DrawLowerValueLabel(canvas, colors.text);
            }

            DrawsSliderHandle(canvas, upperHandleRect, colors.slider, sUpperDown, sUpperHover);

            if (ShowValueLabel)
            {
                DrawUpperValueLabel(canvas, colors.text);
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
            if (!BackgroundDraw) return;

            Util.DrawBox(canvas, rect, BorderOnly ? SKColors.Transparent : color, Round, thm.Corner);

            if (BorderOnly) return;
            borderPaint.Color = thm.ToColor(BorderColor);
            borderPaint.StrokeWidth = 1;
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
            progressPaint.Color = color;

            SKRect progressRect;
            if (Direction == GoDirectionHV.Horizon)
            {
                progressRect = new SKRect(lowerHandleRect.MidX, trackRect.Top, upperHandleRect.MidX, trackRect.Bottom);
            }
            else
            {
                progressRect = new SKRect(trackRect.Left, upperHandleRect.MidY, trackRect.Right, lowerHandleRect.MidY);
            }

            bool isValidProgress;

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

            if (EnableShadow)
            {
                using var shadow = SKImageFilter.CreateDropShadow(2, 2, 3, 3, new SKColor(0, 0, 0, 80));
                handlePaint.ImageFilter = shadow;
            }

            canvas.DrawCircle(rect.MidX, rect.MidY, MathHandleSizeClamp(isDown, isHover), handlePaint);
            handlePaint.ImageFilter = null;
        }
        #endregion
        #region DrawLowerValueLabel
        private void DrawLowerValueLabel(SKCanvas canvas, SKColor color)
        {
            textPaint.Color = color;
            textAlign = SKTextAlign.Center;
            textFont.Size = FontSize;
            textFont.Typeface = SKTypeface.FromFamilyName(FontName);

            canvas.Save();

            using (var clipPath = new SKPath())
            {
                clipPath.AddOval(lowerHandleRect);
                clipPath.Close();
                canvas.ClipPath(clipPath);

                var textOffset = FontSize * 0.3f;
                canvas.DrawText(LowerValueString, lowerHandleRect.MidX, lowerHandleRect.MidY + textOffset, textAlign, textFont, textPaint);
            }
            canvas.Restore();
        }
        #endregion
        #region DrawUpperValueLabel
        private void DrawUpperValueLabel(SKCanvas canvas, SKColor color)
        {
            textPaint.Color = color;
            textAlign = SKTextAlign.Center;
            textFont.Size = FontSize;
            textFont.Typeface = SKTypeface.FromFamilyName(FontName);

            canvas.Save();

            using (var clipPath = new SKPath())
            {
                clipPath.AddOval(upperHandleRect);
                clipPath.Close();
                canvas.ClipPath(clipPath);

                var textOffset = FontSize * 0.3f;
                canvas.DrawText(UpperValueString, upperHandleRect.MidX, upperHandleRect.MidY + textOffset, textAlign, textFont, textPaint);
            }
            canvas.Restore();
        }
        #endregion

        #endregion

        #region Functions

        #region InitializeDefaults
        private void InitializeDefaults()
        {
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
                theme.ToColor(BoxColor),
                theme.ToColor(SliderColor).BrightnessTransmit(brightness),
                theme.ToColor(ProgressColor)
            );
        }
        #endregion

        #region UpdateLayout
        private void UpdateLayout(SKRect contentBox)
        {
            var normalizedLowerValue = (LowerValue - Minimum) / (Maximum - Minimum);
            var normalizedUpperValue = (UpperValue - Minimum) / (Maximum - Minimum);

            normalizedLowerValue = MathClamp(normalizedLowerValue, 0, 1);
            normalizedUpperValue = MathClamp(normalizedUpperValue, 0, 1);

            var isHeightEnough = Bounds.Height >= MinHeightForLabelBelow;

            var extraSpace = isHeightEnough && ShowValueLabel ? FontSize + 10 : 0;

            var maxHandleRadius = HandleRadius * HandleHoverScale;

            if (Direction == GoDirectionHV.Horizon)
            {
                var trackY = contentBox.MidY;

                trackRect = new SKRect(
                    contentBox.Left + maxHandleRadius,
                    trackY - (float)BarSize / 2,
                    contentBox.Right - maxHandleRadius,
                    trackY + (float)BarSize / 2
                );

                var lowerHandleX = trackRect.Left + (float)normalizedLowerValue * trackRect.Width;
                var lowerHandleY = trackY;
                var upperHandleX = trackRect.Left + (float)normalizedUpperValue * trackRect.Width;
                var upperHandleY = trackY;

                lowerHandleRect = new SKRect(
                    lowerHandleX - maxHandleRadius,
                    lowerHandleY - maxHandleRadius,
                    lowerHandleX + maxHandleRadius,
                    lowerHandleY + maxHandleRadius
                    );
                upperHandleRect = new SKRect(
                    upperHandleX - maxHandleRadius,
                    upperHandleY - maxHandleRadius,
                    upperHandleX + maxHandleRadius,
                    upperHandleY + maxHandleRadius
                    );
            }
            else
            {
                var trackX = contentBox.MidX;

                trackRect = new SKRect(
                    trackX - (float)BarSize / 2,
                    contentBox.Top + maxHandleRadius + (isHeightEnough ? extraSpace / 2 : 0),
                    trackX + (float)BarSize / 2,
                    contentBox.Bottom - maxHandleRadius - (isHeightEnough ? 0 : extraSpace / 2)
                );

                var lowerHandleX = trackX;
                var lowerHandleY = trackRect.Bottom - (float)normalizedLowerValue * trackRect.Height;
                var upperHandleX = trackX;
                var upperHandleY = trackRect.Bottom - (float)normalizedUpperValue * trackRect.Height;

                lowerHandleRect = new SKRect(
                    lowerHandleX - maxHandleRadius,
                    lowerHandleY - maxHandleRadius,
                    lowerHandleX + maxHandleRadius,
                    lowerHandleY + maxHandleRadius
                    );
                upperHandleRect = new SKRect(
                    upperHandleX - maxHandleRadius,
                    upperHandleY - maxHandleRadius,
                    upperHandleX + maxHandleRadius,
                    upperHandleY + maxHandleRadius
                    );
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

            var normalizedUpperValue = (UpperValue - Minimum) / (Maximum - Minimum);
            normalizedPos = Math.Min(normalizedPos, normalizedUpperValue - MinHandleSeparation);

            var newValue = Minimum + normalizedPos * (Maximum - Minimum);

            if (Tick is > 0)
            {
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

            var normalizedLowerValue = (LowerValue - Minimum) / (Maximum - Minimum);
            normalizedPos = Math.Max(normalizedPos, normalizedLowerValue + MinHandleSeparation);

            var newValue = Minimum + normalizedPos * (Maximum - Minimum);

            if (Tick is > 0)
            {
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

            var normalizedLowerValue = (LowerValue - Minimum) / (Maximum - Minimum);
            var normalizedUpperValue = (UpperValue - Minimum) / (Maximum - Minimum);
            if (Math.Abs(normalizedPos - normalizedLowerValue) < Math.Abs(normalizedPos - normalizedUpperValue))
            {
                sLowerDown = true;
                isDraggingLower = true;
                UpdateLowerValueFromPosition(x, y);
            }
            else
            {
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
        #region MathHandleSizeClamp
        private float MathHandleSizeClamp(bool isDown, bool isHover)
        {
            float radius;

            if (HandleRadius * 2 < Height) radius = HandleRadius;
            else radius = Height / 2 - 2;

            if (isDown || isHover) radius *= HandleHoverScale;

            return radius;
        }
        #endregion

        #endregion

        #region User Interaction Methods

        #region SetRange
        public void SetRange(double lower, double upper)
        {
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
