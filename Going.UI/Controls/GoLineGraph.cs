using Going.UI.Datas;
using Going.UI.Dialogs;
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
using static System.Net.Mime.MediaTypeNames;
using System.Xml;
using System.Xml.Linq;

namespace Going.UI.Controls
{
    public class GoLineGraph : GoControl
    {
        #region Properties
        public string GridColor { get; set; } = "Base3";
        public string TextColor { get; set; } = "Fore";
        public string RemarkColor { get; set; } = "Base2";

        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public int GraduationCount { get; set; } = 10;
        public string? FormatString { get; set; } = null;

        public List<GoLineGraphSeries> Series { get; set; } = [];
        public string XAxisName { get; set; } = "";

        public bool PointDraw { get; set; } = true;
        public bool ValueDraw { get; set; } = false;
        public int PointWidth { get; set; } = 50;
        #endregion

        #region Member Variable
        private int DataWH => PointWidth;
        List<GoGraphValue> datas = new List<GoGraphValue>();
        Scroll scroll = new Scroll() { Direction = ScrollDirection.Horizon };
        float mx, my;
        #endregion

        #region Constructor
        public GoLineGraph()
        {
            Selectable = true;

            scroll.GetScrollTotal = () => datas.Count > 0 && Series.Where(x => x.Visible).Count() > 0 ? datas.Count * DataWH : 0;
            scroll.GetScrollTick = () => DataWH;
            scroll.GetScrollView = () =>
            {
                var rt = Areas()["Graph"];
                return  rt.Width;
            };
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
            var rtNameGrid = rts["NameGrid"];
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtScroll = rts["rtScroll"];
            using var p = new SKPaint { IsAntialias = true };

            var rc = Series.Where(x => x.Visible).Count();
            #endregion
            #region spos
            var spos = 0;
            var dwh = 0F;
            if (scroll.ScrollVisible)
            {
                dwh = DataWH;
                spos = Convert.ToInt32(scroll.ScrollPositionWithOffset);
            }
            else
            {
                if (datas.Count > 0) dwh = rtNameGrid.Width / datas.Count;
                else dwh = PointWidth;
            }
            #endregion

            #region Value Axis
            {
                #region Axis
                var x = rtValueTitle.Left;
                if (rc > 0)
                {
                    foreach (var v in Series.Where(x => x.Visible))
                    {
                        var nw = new string[] { v.Alias, ValueTool.ToString(v.Minimum, FormatString) ?? "", ValueTool.ToString(v.Maximum, FormatString) ?? "" }.Select(y => Util.MeasureText(y, FontName, FontStyle, FontSize).Width).OrderByDescending(z => z).FirstOrDefault();
                        var rtTitle = Util.FromRect(x, rtValueTitle.Top, nw, rtValueTitle.Height);
                        var c = cSeries[v];
                        Util.DrawText(canvas, v.Alias, FontName, FontStyle, FontSize, rtTitle, c, GoContentAlignment.MiddleRight);

                        for (int i = 0; i <= GraduationCount; i++)
                        {
                            var y = Convert.ToSingle(MathTool.Map(i, 0.0, GraduationCount, rtValueGrid.Bottom, rtValueGrid.Top));
                            var sVal = ValueTool.ToString(MathTool.Map(i, 0, GraduationCount, v.Minimum, v.Maximum), FormatString);
                            var rtValue = Util.FromRect(x, y - ((FontSize + 2) / 2), nw, FontSize + 2);
                            Util.DrawText(canvas, sVal, FontName, FontStyle, FontSize, rtValue, c, GoContentAlignment.MiddleRight);
                        }

                        x += nw + 10;
                    }
                }
                #endregion

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

            #region Name Axis
            if (rc > 0)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(rtNameGrid);
                    canvas.Translate(rtNameGrid.Left, rtNameGrid.Top);
                    canvas.Translate(spos, 0);

                    var vrt = Util.FromRect(-spos, 0, rtNameGrid.Width, rtNameGrid.Height);

                    var si = Math.Max(0, Convert.ToInt32(Math.Floor(vrt.Left / dwh)));
                    var ei = Math.Min(datas.Count - 1, Convert.ToInt32(Math.Ceiling(vrt.Right / dwh)));

                    for (int i = si; i <= ei; i++)
                    {
                        var itm = datas[i];
                        var rt = Util.FromRect((dwh * i), 0, dwh, rtNameGrid.Height);
                        Util.DrawText(canvas, itm.Name, FontName, FontStyle, FontSize, rt, cText, GoContentAlignment.TopCenter);
                    }
                }
            }
            #endregion

            #region Graph
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(rtGraph);
                    canvas.Translate(rtGraph.Left, rtGraph.Top);
                    canvas.Translate(spos, 0);

                    var vrt = Util.FromRect(-spos, 0, rtGraph.Width, rtGraph.Height);

                    var si = Math.Max(0, Convert.ToInt32(Math.Floor(vrt.Left / dwh) - 1));
                    var ei = Math.Min(datas.Count - 1, Convert.ToInt32(Math.Ceiling(vrt.Right / dwh)) + 1);

                    using var fOut = SKImageFilter.CreateDropShadow(2, 2, 1, 1, Util.FromArgb(90, SKColors.Black));

                    foreach (var ser in Series.Where(x => x.Visible))
                    {
                        var c = cSeries[ser];
                        #region make lines
                        var ls = new List<LGV>();
                        for (int i = si; i <= ei; i++)
                        {
                            var itm = datas[i];
                            var rt = Util.FromRect((dwh * i), 0, dwh, rtGraph.Height);

                            var x = rt.MidX;
                            var y = Convert.ToInt32(MathTool.Map(itm.Values[ser.Name], ser.Minimum, ser.Maximum, rt.Bottom, rt.Top));
                            ls.Add(new LGV() { Position = new SKPoint(x, y), Value = itm.Values[ser.Name] });
                        }

                        p.StrokeWidth = 2F;
                        p.Color =c;
                        p.IsStroke = false;
                        if (ls.Count >= 2)
                        {
                            var pts = ls.Select(x => x.Position).ToArray();
                            canvas.DrawPoints(SKPointMode.Polygon, pts, p);
                        }
                        #endregion

                        #region point draw
                        if (PointDraw)
                        {
                            p.ImageFilter = fOut;
                            foreach (var v in ls)
                            {
                                if (PointDraw)
                                {
                                    p.IsStroke = false;
                                    p.Color = c;
                                    canvas.DrawCircle(v.Position, 5, p);
                                }
                            }
                            p.ImageFilter = null;
                        }
                        #endregion

                        #region value draw
                        if (ValueDraw)
                        {
                            foreach (var v in ls)
                            {
                                var txt = ValueTool.ToString(v.Value, FormatString) ?? "";
                                var sz = Util.MeasureText(txt, FontName, FontStyle, FontSize);

                                var w = sz.Width + 20;
                                var trt = Util.FromRect(v.Position.X - (w / 2), v.Position.Y - sz.Height - 10, w, sz.Height);
                                Util.DrawText(canvas, txt, FontName, FontStyle, FontSize, trt, c, thm.Back, 6);
                            }
                        }
                        #endregion
                    }
                }
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
                    var rt = Util.FromRect(rtRemark.Left + 10, y, rtRemark.Width - 20, box);
                    var c = cSeries[v];
                    var (rtIco, rtText) = Util.TextIconBounds(v.Alias, FontName, FontStyle, FontSize, new SKSize(box, box), GoDirectionHV.Horizon, 10, rt, GoContentAlignment.MiddleLeft);
                    Util.DrawBox(canvas, rtIco, c, GoRoundType.Rect, thm.Corner);
                    Util.DrawText(canvas, v.Alias, FontName, FontStyle, FontSize, rtText, cText);
                    y += box + gap;
                }
            }
            #endregion

            scroll.Draw(canvas, rtScroll);

            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtValueGrid = rts["ValueGrid"];
            var rtNameGrid = rts["NameGrid"];
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtScroll = rts["rtScroll"];
            #endregion

            scroll.MouseDown(x, y, rtScroll);
            if (scroll.TouchMode && CollisionTool.Check(rtGraph, x, y)) scroll.TouchDown(x, y);

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            mx = x;
            my = y;
            #region var
            var rts = Areas();
            var rtValueGrid = rts["ValueGrid"];
            var rtNameGrid = rts["NameGrid"];
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtScroll = rts["rtScroll"];
            #endregion

            scroll.MouseMove(x, y, rtScroll);
            if (scroll.TouchMode) scroll.TouchMove(x, y);
            base.OnMouseMove(x, y);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtValueGrid = rts["ValueGrid"];
            var rtNameGrid = rts["NameGrid"];
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtScroll = rts["rtScroll"];
            #endregion

            scroll.MouseUp(x, y);
            if (scroll.TouchMode) scroll.TouchUp(x, y);

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtValueGrid = rts["ValueGrid"];
            var rtNameGrid = rts["NameGrid"];
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtScroll = rts["rtScroll"];
            #endregion

            if (CollisionTool.Check(rtRemark, x, y))
            {
                var ls = Series.Select(x => new GoListItem { Text = x.Alias, Tag = x }).ToList();
                var sels = ls.Where(x => ((GoLineGraphSeries)x.Tag!).Visible).ToList();
                GoDiaglos.SelectorBox.ShowCheck("Graph Series", 1, ls, sels, (vs) =>
                {
                    if (vs != null)
                    {
                        var vls = vs.Select(x => x.Tag as GoLineGraphSeries);
                        foreach (var ser in Series) ser.Visible = vls.Contains(ser);
                    }
                });
            }

            base.OnMouseDoubleClick(x, y, button);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            {
                var box = FontSize + 2;
                var gap = 10;

                var lsw = Series.Where(x => x.Visible).Select(x => new string[] { x.Alias, ValueTool.ToString(x.Minimum, FormatString) ?? "", ValueTool.ToString(x.Maximum, FormatString) ?? "" }.Select(y => Util.MeasureText(y, FontName, FontStyle, FontSize).Width).OrderByDescending(z => z).FirstOrDefault());
                var rsw = Series.Select(x => Util.MeasureText(x.Alias, FontName, FontStyle, FontSize).Width + gap + box);
                var vw = lsw.Any() ? lsw.Sum() + ((lsw.Count() - 1) * 10) : 0;
                var rw = rsw.Any() ? rsw.Max() + 20 : 20;
                var rc = Math.Max(1, Series.Where(x => x.Visible).Count());
                var rts = Util.Grid(rtContent, [$"{vw}px", "10px", "100%", "10px", $"{rw}px"], ["20px", "10px", "100%", "10px", "40px", $"{Scroll.SC_WH}px"]);

                var rtValueTitle = Util.Merge(rts, 0, 0, 1, 1);
                var rtValueGrid = Util.Merge(rts, 0, 2, 1, 1);
                var rtNameGrid = Util.Merge(rts, 2, 4, 1, 1);
                var rtGraph = Util.Merge(rts, 2, 2, 1, 1);
                var rtRemark = Util.Merge(rts, 4, 2, 1, 1);
                var rtScroll = Util.Merge(rts, 2, 5, 1, 1);

                dic["ValueTitle"] = rtValueTitle;
                dic["ValueGrid"] = rtValueGrid;
                dic["NameGrid"] = rtNameGrid;
                dic["Remark"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, (box * rc) + (gap * (rc + 1))));
                dic["Graph"] = rtGraph;
                dic["rtScroll"] = rtScroll;
            }

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region SetDataSource
        public void SetDataSource<T>(IEnumerable<T> values)
        {
            if (Series.Count > 0)
            {
                var pls = typeof(T).GetProperties();

                var xprop = pls.FirstOrDefault(x => x.Name == XAxisName);
                if (xprop != null && xprop.PropertyType == typeof(string))
                {
                    var props = typeof(T).GetProperties().Where(x => x.PropertyType == typeof(double) || x.PropertyType == typeof(float) || x.PropertyType == typeof(decimal) ||
                                                                     x.PropertyType == typeof(byte) || x.PropertyType == typeof(sbyte) ||
                                                                     x.PropertyType == typeof(short) || x.PropertyType == typeof(ushort) ||
                                                                     x.PropertyType == typeof(int) || x.PropertyType == typeof(uint) ||
                                                                     x.PropertyType == typeof(long) || x.PropertyType == typeof(ulong));
                    var nmls = props.Select(x => x.Name).ToList();
                    int nCnt = Series.Where(x => nmls.Contains(x.Name)).Count();
                    if (nCnt == Series.Count)
                    {
                        var dic = props.ToDictionary(x => x.Name);

                        datas.Clear();
                        foreach (var v in values)
                        {
                            var nm = xprop.GetValue(v) as string;
                            if (!string.IsNullOrWhiteSpace(nm))
                            {
                                var gv = new GoGraphValue() { Name = nm, };

                                if (gv.Name != null)
                                {
                                    foreach (var prop in props)
                                    {
                                        var val = Convert.ToDouble(prop.GetValue(v));
                                        gv.Values.Add(prop.Name, val);
                                    }

                                    datas.Add(gv);
                                }
                            }
                        }


                    }
                    else throw new Exception("잘못된 데이터 입니다.");
                }
                else throw new Exception("해당 타입에 지정한 X축 이름이 존재하지 않거나 string 형이 아닙니다.");
            }
            else throw new Exception("GraphSeries는 최소 1개 이상이어야 합니다.");
        }
        #endregion
        #endregion
    }

    #region class : LGV
    class LGV
    {
        public SKPoint Position { get; set; }
        public double Value { get; set; }
    }
    #endregion
}
