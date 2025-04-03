using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoProgress : GoWrapperControl<Going.UI.Controls.GoProgress>
    {
        #region Properties
        public string FontName { get => Control.FontName; set { Control.FontName = value; Invalidate(); } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { Control.FontStyle = value; Invalidate(); } }
        public float FontSize { get => Control.FontSize; set { Control.FontSize = value; Invalidate(); } }
        public float ValueFontSize { get => Control.ValueFontSize; set { Control.ValueFontSize = value; Invalidate(); } }

        public string TextColor { get => Control.TextColor; set { Control.TextColor = value; Invalidate(); } }
        public string FillColor { get => Control.FillColor; set { Control.FillColor = value; Invalidate(); } }
        public string EmptyColor { get => Control.EmptyColor; set { Control.EmptyColor = value; Invalidate(); } }
        public string BorderColor { get => Control.BorderColor; set { Control.BorderColor = value; Invalidate(); } }

        public ProgressDirection Direction { get => Control.Direction; set { Control.Direction = value; Invalidate(); } }

        public double Value { get => Control.Value; set { Control.Value = value; Invalidate(); } }

        public double Minimum { get => Control.Minimum; set { Control.Minimum = value; Invalidate(); } }
        public double Maximum { get => Control.Maximum; set { Control.Maximum = value; Invalidate(); } }

        public string Format { get => Control.Format; set { Control.Format = value; Invalidate(); } }
        public int Gap { get => Control.Gap; set { Control.Gap = value; Invalidate(); } }
        public int CornerRadius { get => Control.CornerRadius; set { Control.CornerRadius = value; Invalidate(); } }
        #endregion

        #region Event
        public event EventHandler? ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion
    }
}
