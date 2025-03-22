using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoLineGraph : GoControl
    {
        #region Properties
        public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        public string GridColor { get; set; } = "Base3";
        public string TextColor { get; set; } = "Fore";
        public string RemarkColor { get; set; } = "Base2";
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;
        public int GraduationCount { get; set; } = 10;
        public string? FormatString { get; set; } = null;
        public List<GoLineGraphSeries> Series { get; set; } = [];
        public bool Scrollable { get; set; } = true;
        #endregion

        #region Constructor
        public GoLineGraph()
        {
            Selectable = true;
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;
            var cGrid = thm.ToColor(GridColor);
            var cText = thm.ToColor(TextColor);
            var cRemark = thm.ToColor(RemarkColor);
            var cSeries = Series.ToDictionary(x => x, y => thm.ToColor(y.Color));
            var rts = Areas();
            var rtValueTitle = rts["ValueTitle"];
            var rtValueGrid = rts["ValueGrid"];
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtScroll = rts["rtScroll"];
            using var p = new SKPaint { IsAntialias = true };
            #endregion

            if (Direction == GoDirectionHV.Horizon)
            {
                #region Value
                {
                    var x = rtValueTitle.Left;
                    foreach (var v in Series.Where(x => x.Visible))
                    {
                        var sAlias = (new string[] { v.Alias, ValueTool.ToString(v.Minimum, FormatString) ?? "", ValueTool.ToString(v.Maximum, FormatString) ?? "" }.OrderByDescending(y => y.Length).FirstOrDefault() ?? "");
                        var sz = Util.MeasureText(sAlias, FontName, FontStyle, FontSize);

                        var rtTitle = Util.FromRect(x, rtValueTitle.Top, sz.Width, rtValueTitle.Height);
                        var c = cSeries[v];
                        Util.DrawText(canvas, v.Alias, FontName, FontStyle, FontSize, rtTitle, c, GoContentAlignment.MiddleRight);

                        for (int i = 0; i <= GraduationCount; i++)
                        {
                            var y = Convert.ToSingle(MathTool.Map(i, 0.0, GraduationCount, rtValueGrid.Bottom, rtValueGrid.Top));
                            var sVal = ValueTool.ToString(MathTool.Map(i, 0, GraduationCount, v.Minimum, v.Maximum), FormatString);
                            var rtValue = Util.FromRect(x, y - ((FontSize + 2) / 2), sz.Width, FontSize + 2);
                            Util.DrawText(canvas, sVal, FontName, FontStyle, FontSize, rtValue, c, GoContentAlignment.MiddleRight);
                        }

                        x += sz.Width + 10;
                    }
                }
                #endregion

                #region Graph
                {
                    #region Grid
                    using var pe = SKPathEffect.CreateDash([3, 3,], 2);
                    p.PathEffect = pe;
                    for (int i = 0; i <= GraduationCount; i++)
                    {
                        var y = Convert.ToInt32(MathTool.Map(i, 0.0, GraduationCount, rtGraph.Bottom, rtGraph.Top)) + 0.5F;
                        p.IsStroke = false;
                        p.StrokeWidth = 1F;
                        p.Color = cGrid;
                        canvas.DrawLine(rtGraph.Left, y, rtGraph.Right, y, p);
                    }
                    p.PathEffect = null;
                    #endregion
                }
                #endregion

                #region Remark
                {
                    Util.DrawBox(canvas, rtRemark, cRemark, cGrid, GoRoundType.All, thm.Corner);

                    var box = FontSize + 2;
                    var gap = 10;
                    var y = rtRemark.Top + 10;
                    foreach (var v in Series.Where(x => x.Visible))
                    {
                        var rt = Util.FromRect(rtRemark.Left, y, rtRemark.Width, box);
                        var c = cSeries[v];
                        var (rtIco, rtText) = Util.TextIconBounds(v.Alias, FontName, FontStyle, FontSize, new SKSize(box, box), GoDirectionHV.Horizon, 10, rt, GoContentAlignment.MiddleCenter);
                        Util.DrawBox(canvas, rtIco, c, GoRoundType.Rect, thm.Corner);
                        Util.DrawText(canvas, v.Alias, FontName, FontStyle, FontSize, rtText, cText);
                        y += box + gap;
                    }
                }
                #endregion
            }

            base.OnDraw(canvas);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            if (Direction == GoDirectionHV.Horizon)
            {
                var box = FontSize + 2;
                var gap = 10;

                var lsw = Series.Where(x => x.Visible).Select(x => Util.MeasureText((new string[] { x.Alias, ValueTool.ToString(x.Minimum, FormatString) ?? "", ValueTool.ToString(x.Maximum, FormatString) ?? "" }.OrderByDescending(y => y.Length).FirstOrDefault() ?? ""), FontName, FontStyle, FontSize).Width);
                var rsw = Series.Where(x => x.Visible).Select(x => Util.MeasureText(x.Alias, FontName, FontStyle, FontSize).Width + gap + box);
                var vw = lsw.Count() > 0 ? lsw.Sum() + ((lsw.Count() - 1) * 10) : 0;
                var rw = rsw.Count() > 0 ? rsw.Max() + 20 : 0;
                var rts = Util.Grid(rtContent, [$"{vw}px", "10px", "100%", "10px", $"{rw}px"], ["20px", "10px", "100%", "10px", "40px", $"{(Scrollable ? Scroll.SC_WH : 0)}px"]);

                var rtValueTitle = Util.Merge(rts, 0, 0, 1, 1);
                var rtValueGrid = Util.Merge(rts, 0, 2, 1, 1);
                var rtGraph = Util.Merge(rts, 2, 2, 1, 1);
                var rtRemark = Util.Merge(rts, 4, 2, 1, 1);
                var rtScroll = Util.Merge(rts, 2, 5, 1, 1);

                dic["ValueTitle"] = rtValueTitle;
                dic["ValueGrid"] = rtValueGrid;
                dic["Remark"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, (box * rsw.Count()) + (gap * (rsw.Count() + 1))));
                dic["Graph"] = rtGraph;
                dic["rtScroll"] = rtScroll;
            }
            else
            {

            }

            return dic;
        }
        #endregion
        #endregion
    }


}
