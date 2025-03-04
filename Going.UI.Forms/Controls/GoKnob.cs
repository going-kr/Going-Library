using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoKnob : GoWrapperControl<Going.UI.Controls.GoKnob>
    {
        #region Properties
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string KnobColor { get => Control.KnobColor; set { if (Control.KnobColor != value) { Control.KnobColor = value; Invalidate(); } } }
        public string ButtonColor { get => Control.CursorColor; set { if (Control.CursorColor != value) { Control.CursorColor = value; Invalidate(); } } }

        public double Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        public double Minimum { get => Control.Minimum; set { if (Control.Minimum != value) { Control.Minimum = value; Invalidate(); } } }
        public double Maximum { get => Control.Maximum; set { if (Control.Maximum != value) { Control.Maximum = value; Invalidate(); } } }
        public double? Tick { get => Control.Tick; set { if (Control.Tick != value) { Control.Tick = value; Invalidate(); } } }

        public string Format { get => Control.Format; set { if (Control.Format != value) { Control.Format = value; Invalidate(); } } }
        public int SweepAngle { get => Control.SweepAngle; set { if (Control.SweepAngle != value) { Control.SweepAngle = value; Invalidate(); } } }
        public bool DrawText { get => Control.DrawText; set { if (Control.DrawText != value) { Control.DrawText = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoKnob()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
