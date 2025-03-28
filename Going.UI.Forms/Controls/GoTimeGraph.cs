using Going.UI.Datas;
using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoTimeGraph : GoWrapperControl<Going.UI.Controls.GoTimeGraph>
    {
        #region Properties
        public string GridColor { get => Control.GridColor; set { if (Control.GridColor != value) { Control.GridColor = value; Invalidate(); } } }
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string RemarkColor { get => Control.RemarkColor; set { if (Control.RemarkColor != value) { Control.RemarkColor = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public TimeSpan XScale { get => Control.XScale; set { if (Control.XScale != value) { Control.XScale = value; Invalidate(); } } }
        public TimeSpan XAxisGraduationTime { get => Control.XAxisGraduationTime; set { if (Control.XAxisGraduationTime != value) { Control.XAxisGraduationTime = value; Invalidate(); } } }
        public int YAxisGraduationCount { get => Control.YAxisGraduationCount; set { if (Control.YAxisGraduationCount != value) { Control.YAxisGraduationCount = value; Invalidate(); } } }
        public string? TimeFormatString { get => Control.TimeFormatString; set { if (Control.TimeFormatString != value) { Control.TimeFormatString = value; Invalidate(); } } }
        public string? ValueFormatString { get => Control.ValueFormatString; set { if (Control.ValueFormatString != value) { Control.ValueFormatString = value; Invalidate(); } } }

        public List<GoLineGraphSeries> Series { get => Control.Series; set { if (Control.Series != value) { Control.Series = value; Invalidate(); } } }
        #endregion

        #region Constructor
        public GoTimeGraph()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion

        #region Method
        public void SetDataSource<T>(string XAxisName, IEnumerable<T> values, DateTime? start, DateTime? end)
        {
            Control.SetDataSource(XAxisName, values, start, end);
            Invalidate();
        }
        #endregion
    }
}
