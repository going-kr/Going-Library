using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace Going.UI.Forms.Controls
{
    public class GoCircleGraph : GoWrapperControl<Going.UI.Controls.GoCircleGraph>
    {
        #region Properties
        public string GridColor { get => Control.GridColor; set { if (Control.GridColor != value) { Control.GridColor = value; Invalidate(); } } }
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string RemarkColor { get => Control.RemarkColor; set { if (Control.RemarkColor != value) { Control.RemarkColor = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public List<GoGraphSeries> Series { get => Control.Series; set { if (Control.Series != value) { Control.Series = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        Timer tmr;
        SKPoint mp;
        #endregion

        #region Constructor
        public GoCircleGraph()
        {
            SetStyle(ControlStyles.Selectable, true);

            tmr = new System.Windows.Forms.Timer { Interval = 10, Enabled = true };
            tmr.Tick += (o, s) =>
            {
                if (!IsDisposed)
                {
                    try
                    {
                        var rtGraph = Control.Areas()["Graph"];
                        var wh = Math.Min(rtGraph.Width, rtGraph.Height) - 40;
                        var rtF = MathTool.MakeRectangle(rtGraph, new SKSize(wh, wh));
                        var rtS = MathTool.MakeRectangle(rtGraph, new SKSize(wh * 0.5F, wh * 0.5F));
                        var bSel = CollisionTool.CheckCircle(rtF, mp) && !CollisionTool.CheckCircle(rtS, mp);
                        if (bSel) Invalidate();
                    } catch { }
                }
            };
        }
        #endregion

        #region Override
        protected override void OnMouseMove(MouseEventArgs e)
        {
            mp.X = e.X; mp.Y = e.Y;
            base.OnMouseMove(e);
        }
        #endregion

        #region Method
        public void SetDataSource<T>(string CategoryAxisName, IEnumerable<T> values)
        {
            Control.SetDataSource(CategoryAxisName, values);
            Invalidate();
        }
        #endregion
    }
}
