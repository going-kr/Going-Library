using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoGauge : GoWrapperControl<Going.UI.Controls.GoGauge>
    {
        #region Properties
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }
        public float TitleFontSize { get => Control.TitleFontSize; set { if (Control.TitleFontSize != value) { Control.TitleFontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string EmptyColor { get => Control.EmptyColor; set { if (Control.EmptyColor != value) { Control.EmptyColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }

        public double Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        public double Minimum { get => Control.Minimum; set { if (Control.Minimum != value) { Control.Minimum = value; Invalidate(); } } }
        public double Maximum { get => Control.Maximum; set { if (Control.Maximum != value) { Control.Maximum = value; Invalidate(); } } }
        public string Format { get => Control.Format; set { if (Control.Format != value) { Control.Format = value; Invalidate(); } } }
        public int StartAngle { get => Control.StartAngle; set { if (Control.StartAngle != value) { Control.StartAngle = value; Invalidate(); } } }
        public int SweepAngle { get => Control.SweepAngle; set { if (Control.SweepAngle != value) { Control.SweepAngle = value; Invalidate(); } } }
        public int BarSize { get => Control.BarSize; set { if (Control.BarSize != value) { Control.BarSize = value; Invalidate(); } } }
        public int Gap { get => Control.Gap; set { if (Control.Gap != value) { Control.Gap = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoGauge()
        {
        }
        #endregion
    }
}
