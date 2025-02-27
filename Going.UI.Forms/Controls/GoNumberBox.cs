using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoNumberBox : GoWrapperControl<Going.UI.Controls.GoNumberBox>
    {
        #region Properties
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string ButtonColor { get => Control.ButtonColor; set { if (Control.ButtonColor != value) { Control.ButtonColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public double Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        public double Minimum { get => Control.Minimum; set { if (Control.Minimum != value) { Control.Minimum = value; Invalidate(); } } }
        public double Maximum { get => Control.Maximum; set { if (Control.Maximum != value) { Control.Maximum = value; Invalidate(); } } }
        public double Tick { get => Control.Tick; set { if (Control.Tick != value) { Control.Tick = value; Invalidate(); } } }
        public string? Format { get => Control.Format; set { if (Control.Format != value) { Control.Format = value; Invalidate(); } } }
        public float ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoNumberBox()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
