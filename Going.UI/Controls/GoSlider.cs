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
    /// 슬라이더 컨트롤. 핸들을 드래그하여 값을 조절할 수 있으며, 수평/수직 방향을 지원합니다.
    /// </summary>
    public class GoSlider : GoControl, IDisposable
    {
        #region Properties

        #region 아이콘 설정
        /// <summary>아이콘 문자열 (FontAwesome 등)</summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        /// <summary>아이콘 크기</summary>
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        /// <summary>아이콘과 텍스트 사이 간격</summary>
        [GoProperty(PCategory.Control, 2)] public float IconGap { get; set; } = 5;
        /// <summary>아이콘 배치 방향</summary>
        [GoProperty(PCategory.Control, 3)] public GoDirectionHV IconDirection { get; set; }
        #endregion

        #region 슬라이더 라벨 설정
        /// <summary>슬라이더 라벨 텍스트</summary>
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "slider";
        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 6)] public float FontSize { get; set; } = 12;
        #endregion

        #region 슬라이더 배경 설정
        /// <summary>배경 그리기 여부</summary>
        [GoProperty(PCategory.Control, 7)] public bool BackgroundDraw { get; set; }
        /// <summary>테두리만 그리기 여부</summary>
        [GoProperty(PCategory.Control, 8)] public bool BorderOnly { get; set; }
        #endregion

        #region 슬라이더 설정
        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 9)] public string TextColor { get; set; } = "Fore";
        /// <summary>배경 박스 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 10)] public string BoxColor { get; set; } = "Back";
        /// <summary>슬라이더 핸들 및 진행 바 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 11)] public string SliderColor { get; set; } = "Base5";
        /// <summary>트랙 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 12)] public string ProgressColor { get; set; } = "Base1";
        /// <summary>테두리 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 13)] public string BorderColor { get; set; } = "danger";
        /// <summary>모서리 둥글기 유형</summary>
        [GoProperty(PCategory.Control, 14)] public GoRoundType Round { get; set; } = GoRoundType.All;
        /// <summary>슬라이더 방향 (가로/세로)</summary>
        [GoProperty(PCategory.Control, 15)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        #endregion

        #region 슬라이더 표시 설정(외형)
        /// <summary>핸들에 값 라벨 표시 여부</summary>
        [GoProperty(PCategory.Control, 16)] public bool ShowValueLabel { get; set; } = true;
        /// <summary>값 표시 형식 문자열</summary>
        [GoProperty(PCategory.Control, 17)] public string ValueFormat { get; set; } = "0";
        /// <summary>트랙 바 두께 (픽셀)</summary>
        [GoProperty(PCategory.Control, 18)] public int BarSize { get; set; } = 4;
        /// <summary>핸들 반지름 (픽셀)</summary>
        [GoProperty(PCategory.Control, 19)] public float HandleRadius { get; set; } = 15f;
        /// <summary>그림자 효과 사용 여부</summary>
        [GoProperty(PCategory.Control, 20)] public bool EnableShadow { get; set; } = true;
        /// <summary>호버 시 핸들 확대 비율</summary>
        [GoProperty(PCategory.Control, 21)] public float HandleHoverScale { get; set; } = 1.05f;
        #endregion

        #region 틱(단계) 설정
        /// <summary>증감 단위. null이면 연속 값을 사용합니다.</summary>
        [GoProperty(PCategory.Control, 22)] public double? Tick { get; set; } = null;
        /// <summary>틱 표시 여부</summary>
        [GoProperty(PCategory.Control, 23)] public bool ShowTicks { get; set; } = false;
        /// <summary>틱 개수</summary>
        [GoProperty(PCategory.Control, 24)] public int TickCount { get; set; } = 5;
        /// <summary>틱 표시 크기 (픽셀)</summary>
        [GoProperty(PCategory.Control, 25)] public float TickSize { get; set; } = 10f;
        #endregion

        #region 슬라이더 값 설정(외부)
        private double sValue;
        /// <summary>현재 값. 값이 변경되면 <see cref="ValueChanged"/> 이벤트가 발생합니다.</summary>
        [GoProperty(PCategory.Control, 26)]
        public double Value
        {
            get => sValue;
            set
            {
                var clampedValue = MathClamp(value, Minimum, Maximum);
                if (Math.Abs(sValue - value) < 0.00001f) return;

                sValue = clampedValue;
                ValueChanged?.Invoke(this, EventArgs.Empty);
                UpdateValueString();
                needsLayoutUpdate = true;
            }
        }
        private string? sValueString;
        /// <summary>값의 형식화된 문자열 표현</summary>
        [GoProperty(PCategory.Control, 27)]
        public string? ValueString
        {
            get => sValueString;
            set
            {
                if (string.Equals(sValueString, value, StringComparison.Ordinal)) return;
                sValueString = value;
            }
        }
        private double sMinimum;
        /// <summary>최솟값</summary>
        [GoProperty(PCategory.Control, 28)]
        public double Minimum
        {
            get => sMinimum;
            set
            {
                if (Math.Abs(sMinimum - value) < 0.00001f) return;
                sMinimum = value;

                Value = Math.Max(value, Value);
                needsLayoutUpdate = true;
            }
        }
        private double sMaximum = 100D;
        /// <summary>최댓값</summary>
        [GoProperty(PCategory.Control, 29)]
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
        private float SHandleMaxHeight => Height;
        #endregion

        #region 슬라이더 값 설정(내부)
        private bool isDragging;
        private SKRect trackRect;
        private SKRect handleRect;
        private const float TrackHeight = 4f;
        private int MinHeightForLabelBelow { get; set; } = 80;
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

        /// <summary>슬라이더 드래그가 시작되었을 때 발생하는 이벤트</summary>
        public event EventHandler? SliderDragStarted;
        /// <summary>슬라이더 드래그가 완료되었을 때 발생하는 이벤트</summary>
        public event EventHandler? SliderDragCompleted;
        /// <summary>값이 변경되었을 때 발생하는 이벤트</summary>
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
        private float mx, my;

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

            if (CollisionTool.Check(handleRect, x, y))
            {
                sDown = true;
                isDragging = true;
                SliderDragStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (CollisionTool.Check(trackRect, x, y))
            {
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




            if (sDown)
            {
                sDown = false;
                if (isDragging) SliderDragCompleted?.Invoke(this, EventArgs.Empty);

            }
            isDragging = false;

            base.OnMouseUp(x, y, button);
        }
        #endregion

        #endregion

        #region Method
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
                DrawTicks(canvas, thm, colors.slider);
            }

            DrawSliderBackground(canvas, thm, contentBox, colors.background);
            UpdateLayout(contentBox);
            DrawSliderTrack(canvas, colors.empty);
            DrawSliderProgress(canvas, thm, contentBox, colors.slider);
            DrawSliderHandle(canvas, handleRect, colors.slider);

            if (ShowValueLabel)
            {
                DrawValueLabel(canvas, colors.text);
            }
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

                Util.DrawBox(canvas, rect, BorderOnly ? SKColors.Transparent : color, Round, thm.Corner);

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

            progressPaint.Color = color;

            var progressRect = trackRect;

            if (Direction == GoDirectionHV.Horizon)
            {
                progressRect.Right = handleRect.MidX;
            }
            else
            {
                progressRect.Top = handleRect.MidY;
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

            if (EnableShadow)
            {
                using var shadow = SKImageFilter.CreateDropShadow(2, 2, 3, 3, new SKColor(0, 0, 0, 80));
                handlePaint.ImageFilter = shadow;
            }

            float radius;
            if (HandleRadius * 2 > SHandleMaxHeight)
            {
                radius = SHandleMaxHeight / 2 - 2;
            }
            else
            {
                radius = HandleRadius;
            }

            if (isDragging || sHover) radius *= HandleHoverScale;

            canvas.DrawCircle(handleRect.MidX, handleRect.MidY, radius, handlePaint);
            handlePaint.ImageFilter = null;




        }
        #endregion
        #region DrawValueLabel
        private void DrawValueLabel(SKCanvas canvas, SKColor color)
        {
            textPaint.Color = color;
            textAlign = SKTextAlign.Center;
            textFont.Size = FontSize;
            textFont.Typeface = SKTypeface.FromFamilyName(FontName);

            var textHeight = textFont.Size;

            var labelPosition = new SKPoint(handleRect.MidX, handleRect.MidY);
            canvas.Save();

            using var clipPath = new SKPath();
            clipPath.AddOval(handleRect);
            clipPath.Close();
            canvas.ClipPath(clipPath);

            var textOffset = textHeight * 0.3f;
            canvas.DrawText(ValueString, labelPosition.X, labelPosition.Y + textOffset, textAlign, textFont, textPaint);
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
            sValue = 0D;
            sValueString = "0";

            UpdateValueString();
        }
        #endregion
        #region GetThemeColors
        private (SKColor text, SKColor background, SKColor slider, SKColor empty) GetThemeColors(GoTheme theme)
        {
            var brightness = sDown ? theme.DownBrightness : 0;
            return (
                theme.ToColor(TextColor).BrightnessTransmit(brightness),
                theme.ToColor(BoxColor),
                theme.ToColor(SliderColor).BrightnessTransmit(brightness),
                theme.ToColor(ProgressColor)
            );
        }
        #endregion
        #region OnValueStringChanged
        private void OnValueStringChanged()
        {
            UpdateValueString();
        }
        #endregion

        #region UpdateValueString
        private void UpdateValueString()
        {
            ValueString = string.IsNullOrWhiteSpace(ValueFormat) ? Value.ToString(CultureInfo.InvariantCulture) : Value.ToString(ValueFormat);
        }
        #endregion
        #region UpdateLayout
        private void UpdateLayout(SKRect contentBox)
        {
            var normalizedValue = (Value - Minimum) / (Maximum - Minimum);
            normalizedValue = MathClamp(normalizedValue, 0, 1);


            var isHeightEnough = Bounds.Height >= MinHeightForLabelBelow;

            var extraSpace = isHeightEnough && ShowValueLabel ? FontSize + 10 : 0;
            var maxHandleRadius = HandleRadius * HandleHoverScale;

            if (Direction == GoDirectionHV.Horizon)
            {
                var trackY = contentBox.MidY;
                if (!isHeightEnough && ShowValueLabel) trackY += extraSpace / 2;

                trackRect = new SKRect(
                    contentBox.Left + maxHandleRadius,
                    trackY - (float)BarSize / 2,
                    contentBox.Right - maxHandleRadius,
                    trackY + (float)BarSize / 2
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
            }

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
                handleX - maxHandleRadius,
                handleY - maxHandleRadius,
                handleX + maxHandleRadius,
                handleY + maxHandleRadius
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
        /// <summary>값을 한 단계 증가시킵니다.</summary>
        public void IncrementalValue()
        {
            var step = (Maximum - Minimum) / 100.0;
            if (Tick is > 0)
            {
                step = Tick.Value;
            }
            else
            {

                step = Math.Max(0.1, step);
            }

            Value = MathClamp(Value + step, Minimum, Maximum);
        }
        #endregion
        #region DecrementalValue
        /// <summary>값을 한 단계 감소시킵니다.</summary>
        public void DecrementalValue()
        {
            var step = (Maximum - Minimum) / 100.0;
            if (Tick is > 0)
            {
                step = Tick.Value;
            }
            else
            {

                step = Math.Max(0.1, step);
            }

            Value = MathClamp(Value - step, Minimum, Maximum);
        }
        #endregion
        #region SetValueToMinimum
        /// <summary>값을 최솟값으로 설정합니다.</summary>
        public void SetValueToMinimum()
        {
            Value = Minimum;
        }
        #endregion
        #region SetValueMaximum
        /// <summary>값을 최댓값으로 설정합니다.</summary>
        public void SetValueToMaximum()
        {
            Value = Maximum;
        }
        #endregion
        #endregion
        #endregion

        #region Dispose
        /// <summary>슬라이더에서 사용하는 리소스를 해제합니다.</summary>
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

