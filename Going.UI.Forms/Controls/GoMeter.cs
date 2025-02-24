using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoMeter : GoWrapperControl<Going.UI.Controls.GoMeter>
    {
        #region Properties
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }
        public float TitleFontSize { get => Control.TitleFontSize; set { if (Control.TitleFontSize != value) { Control.TitleFontSize = value; Invalidate(); } } }
        public float RemarkFontSize { get => Control.RemarkFontSize; set { if (Control.RemarkFontSize != value) { Control.RemarkFontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string NeedleColor { get => Control.NeedleColor; set { if (Control.NeedleColor != value) { Control.NeedleColor = value; Invalidate(); } } }
        public string NeedlePointColor { get => Control.NeedlePointColor; set { if (Control.NeedlePointColor != value) { Control.NeedlePointColor = value; Invalidate(); } } }
        public string RemarkColor { get => Control.RemarkColor; set { if (Control.RemarkColor != value) { Control.RemarkColor = value; Invalidate(); } } }

        public double Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        public double Minimum { get => Control.Minimum; set { if (Control.Minimum != value) { Control.Minimum = value; Invalidate(); } } }
        public double Maximum { get => Control.Maximum; set { if (Control.Maximum != value) { Control.Maximum = value; Invalidate(); } } }
        public string Format { get => Control.Format; set { if (Control.Format != value) { Control.Format = value; Invalidate(); } } }
        public int GraduationLarge { get => Control.GraduationLarge; set { if (Control.GraduationLarge != value) { Control.GraduationLarge = value; Invalidate(); } } }
        public int GraduationSmall { get => Control.GraduationSmall; set { if (Control.GraduationSmall != value) { Control.GraduationSmall = value; Invalidate(); } } }
        public int Gap { get => Control.Gap; set { if (Control.Gap != value) { Control.Gap = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ValueChanged { add => Control.ValueChanged += value; remove => Control.ValueChanged -= value; }
        #endregion

        #region Constructor
        public GoMeter()
        {
        }
        #endregion
    }
}
