using Going.UI.Enums;
using SkiaSharp;

namespace Going.UI.Forms.Controls
{
    public class GoRangeSlider : GoWrapperControl<Going.UI.Controls.GoRangeSlider>
    {
        #region Properties

        #region 아이콘 설정
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => Control.IconDirection; set { if (Control.IconDirection != value) { Control.IconDirection = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 라벨 설정
        public string Text { get => Control.Text; set { if (Control.Text != value) { Control.Text = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 배경 설정
        public bool BackgroundDraw { get => Control.BackgroundDraw; set { if (Control.BackgroundDraw != value) { Control.BackgroundDraw = value; Invalidate(); } } }
        public bool BorderOnly { get => Control.BorderOnly; set { if (Control.BorderOnly != value) { Control.BorderOnly = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 설정
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BgColor { get => Control.BackColor; set { if (Control.BackColor != value) { Control.BackColor = value; Invalidate(); } } }
        public string SliderColor { get => Control.SliderColor; set { if (Control.SliderColor != value) { Control.SliderColor = value; Invalidate(); } } }
        public string EmptyColor { get => Control.ProgressColor; set { if (Control.ProgressColor != value) { Control.ProgressColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }
        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 표시 설정(외형)
        public bool ShowValueLabel { get => Control.ShowValueLabel; set { if (Control.ShowValueLabel != value) { Control.ShowValueLabel = value; Invalidate(); } } }
        public string ValueFormat { get => Control.ValueFormat; set { if (Control.ValueFormat != value) { Control.ValueFormat = value; Invalidate(); } } }
        public int BarSize { get => Control.BarSize; set { if (Control.BarSize != value) { Control.BarSize = value; Invalidate(); } } }
        public float HandleRadius { get => Control.HandleRadius; set { if (Control.HandleRadius != value) { Control.HandleRadius = value; Invalidate(); } } }
        public bool EnableShadow { get => Control.EnableShadow; set { if (Control.EnableShadow != value) { Control.EnableShadow = value; Invalidate(); } } }
        public float HandleHoverScale { get => Control.HandleHoverScale; set { if (Control.HandleHoverScale != value) { Control.HandleHoverScale = value; Invalidate(); } } }
        #endregion

        #region 틱(단계) 설정
        public double? Tick { get => Control.Tick; set { if (Control.Tick != value) { Control.Tick = value; Invalidate(); } } }
        public bool ShowTicks { get => Control.ShowTicks; set { if (Control.ShowTicks != value) { Control.ShowTicks = value; Invalidate(); } } }
        public int TickCount { get => Control.TickCount; set { if (Control.TickCount != value) { Control.TickCount = value; Invalidate(); } } }
        public float TickSize { get => Control.TickSize; set { if (Control.TickSize != value) { Control.TickSize = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 값 설정(외부)
        public double LowerValue { get => Control.LowerValue; set { if (Control.LowerValue != value) { Control.LowerValue = value; Invalidate(); } } }
        public double UpperValue { get => Control.UpperValue; set { if (Control.UpperValue != value) { Control.UpperValue = value; Invalidate(); } }}
        public string? LowerValueString { get => Control.LowerValueString; set { if (Control.LowerValueString != value) { Control.LowerValueString = value; Invalidate(); } }}
        public string? UpperValueString { get => Control.UpperValueString; set { if (Control.UpperValueString != value) { Control.UpperValueString = value; Invalidate(); } }}
        public double Minimum { get => Control.Minimum; set { if (Control.Minimum != value) { Control.Minimum = value; Invalidate(); } } }
        public double Maximum { get => Control.Maximum; set { if (Control.Maximum != value) { Control.Maximum = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 값 설정(내부)
        public float MinHandleSeparation { get => Control.MinHandleSeparation; set { if (Control.MinHandleSeparation != value) { Control.MinHandleSeparation = value; Invalidate(); } } }
        #endregion

        #endregion

        #region Event
        public event EventHandler? LowerValueChanged { add => Control.LowerValueChanged += value; remove => Control.LowerValueChanged -= value; }
        public event EventHandler? UpperValueChanged { add => Control.UpperValueChanged += value; remove => Control.UpperValueChanged -= value; }
        public event EventHandler? RangeChanged { add => Control.RangeChanged += value; remove => Control.RangeChanged -= value; }
        public event EventHandler? SliderDragStarted { add => Control.SliderDragStarted += value; remove => Control.SliderDragStarted -= value; }
        public event EventHandler? SliderDragCompleted { add => Control.SliderDragCompleted += value; remove => Control.SliderDragCompleted -= value; }
        #endregion

        #region Constructor
        public GoRangeSlider()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
