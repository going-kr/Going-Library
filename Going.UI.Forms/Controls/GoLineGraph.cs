using Going.UI.Datas;
using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoLineGraph : GoWrapperControl<Going.UI.Controls.GoLineGraph>
    {
        #region Properties
        public string GridColor { get => Control.GridColor; set { if (Control.GridColor != value) { Control.GridColor = value; Invalidate(); } } }
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string RemarkColor { get => Control.RemarkColor; set { if (Control.RemarkColor != value) { Control.RemarkColor = value; Invalidate(); } } }
        public string GraphColor { get => Control.GraphColor; set { if (Control.GraphColor != value) { Control.GraphColor = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public int GraduationCount { get => Control.GraduationCount; set { if (Control.GraduationCount != value) { Control.GraduationCount = value; Invalidate(); } } }
        public string? FormatString { get => Control.FormatString; set { if (Control.FormatString != value) { Control.FormatString = value; Invalidate(); } } }

        public List<GoLineGraphSeries> Series { get => Control.Series; set { if (Control.Series != value) { Control.Series = value; Invalidate(); } } }

        public int PointWidth { get => Control.PointWidth; set { if (Control.PointWidth != value) { Control.PointWidth = value; Invalidate(); } } }
        #endregion

        #region Constructor
        public GoLineGraph()
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
