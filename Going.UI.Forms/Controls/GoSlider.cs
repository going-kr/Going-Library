﻿
using Going.UI.Enums;
using SkiaSharp;

namespace Going.UI.Forms.Controls
{
    public class GoSlider : GoWrapperControl<Going.UI.Controls.GoSlider>
    {
        #region Properties

        #region 아이콘 설정
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => Control.IconDirection; set { if (Control.IconDirection != value) { Control.IconDirection = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 라벨 설정
        public override string Text { get => base.Text; set { base.Text = value; if (Control.Text != value) { Control.Text = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 배경 설정
        public bool BackgroundDraw { get => Control.BackgroundDraw; set { if (Control.BackgroundDraw != value) { Control.BackgroundDraw = value; Invalidate(); } } }
        public bool BorderOnly { get => Control.BorderOnly; set { if (Control.BorderOnly != value) { Control.BorderOnly = value; Invalidate(); } } }
        #endregion

        #region 슬라이더 설정
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BoxColor { get => Control.BoxColor; set { if (Control.BoxColor != value) { Control.BoxColor = value; Invalidate(); } } }
        public string SliderColor { get => Control.SliderColor; set { if (Control.SliderColor != value) { Control.SliderColor = value; Invalidate(); } } }
        public string ProgressColor { get => Control.ProgressColor; set { if (Control.ProgressColor != value) { Control.ProgressColor = value; Invalidate(); } } }
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
        public double Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        public string? ValueString { get => Control.ValueString; set { if (Control.ValueString != value) { Control.ValueString = value; Invalidate(); } } }
        public double Minimum { get => Control.Minimum; set { if (Control.Minimum != value) { Control.Minimum = value; Invalidate(); } } }
        public double Maximum { get => Control.Maximum; set { if (Control.Maximum != value) { Control.Maximum = value; Invalidate(); } } }
        #endregion

        #endregion

        #region Event

        public event EventHandler? SliderDragStarted { add => Control.SliderDragStarted += value; remove => Control.SliderDragStarted -= value; }
        public event EventHandler? SliderDragCompleted { add => Control.SliderDragCompleted += value; remove => Control.SliderDragCompleted -= value; }
        public event EventHandler? ValueChanged { add => Control.SliderDragStarted += value; remove => Control.SliderDragStarted -= value; }

        #endregion

        #region Constructor
        public GoSlider()
        {
            Control.Text = base.Text;
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion

        #region User Interaction Methods

        #region IncrementalValue
        public void IncrementalValue() => Control.IncrementalValue();
        public void DecrementalValue() => Control.DecrementalValue();
        public void SetValueToMinimum() => Control.SetValueToMinimum();
        public void SetValueToMaximum() => Control.SetValueToMaximum();
        #endregion

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
