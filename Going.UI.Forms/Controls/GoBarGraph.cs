using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoBarGraph : GoWrapperControl<Going.UI.Controls.GoBarGraph>
    {
        #region Properties
        public string GridColor { get => Control.GridColor; set { if (Control.GridColor != value) { Control.GridColor = value; Invalidate(); } } }
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string RemarkColor { get => Control.RemarkColor; set { if (Control.RemarkColor != value) { Control.RemarkColor = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public int GraduationCount { get => Control.GraduationCount; set { if (Control.GraduationCount != value) { Control.GraduationCount = value; Invalidate(); } } }
        public string? FormatString { get => Control.FormatString; set { if (Control.FormatString != value) { Control.FormatString = value; Invalidate(); } } }

        public GoBarGraphMode Mode { get => Control.Mode; set { if (Control.Mode != value) { Control.Mode = value; Invalidate(); } } }
        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public List<GoGraphSeries> Series { get => Control.Series; set { if (Control.Series != value) { Control.Series = value; Invalidate(); } } }
        public int BarSize { get => Control.BarSize; set { if (Control.BarSize != value) { Control.BarSize = value; Invalidate(); } } }
        public int BarGap { get => Control.BarGap; set { if (Control.BarGap != value) { Control.BarGap = value; Invalidate(); } } }

        public double? Minimum { get => Control.Minimum; set { if (Control.Minimum != value) { Control.Minimum = value; Invalidate(); } } }
        public double? Maximum { get => Control.Maximum; set { if (Control.Maximum != value) { Control.Maximum = value; Invalidate(); } } }
        #endregion

        #region Constructor
        public GoBarGraph()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion

        #region Method
        public void SetDataSource<T>(string XAxisName, IEnumerable<T> values)
        {
            Control.SetDataSource(XAxisName, values);
            Invalidate();
        }
        #endregion
    }
}
