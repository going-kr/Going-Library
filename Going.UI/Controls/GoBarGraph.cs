using Going.UI.Datas;using Going.UI.Dialogs;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoBarGraph : GoControl
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

        public GoBarGraphMode Mode { get; set; } = GoBarGraphMode.List;
        #region  public GoDirectionHV Direction { get; set; } = GoDirectionHV.Vertical;
        private GoDirectionHV dir = GoDirectionHV.Vertical;
        public GoDirectionHV Direction
        {
            get => dir;
            set
            {
                dir = value;
                if (dir == GoDirectionHV.Vertical) scroll.Direction = ScrollDirection.Horizon;
                else if (dir == GoDirectionHV.Horizon) scroll.Direction = ScrollDirection.Vertical;
            }
        }
        #endregion

        public List<GoGraphSeries> Series { get; set; } = [];
        public int BarSize { get; set; } = 20;
        public int BarGap { get; set; } = 20;

        public double? Minimum { get; set; }
        public double? Maximum { get; set; }
        #endregion

        #region Member Variable
        int DataWH => Mode == GoBarGraphMode.List ? (BarGap + (Math.Max(1, Series.Where(x => x.Visible).Count()) * BarSize) + BarGap) : (BarGap + BarSize + BarGap);
        List<GoGraphValue> datas = new List<GoGraphValue>();
        Scroll scroll = new Scroll() { Direction = ScrollDirection.Horizon };
        float mx, my;
        bool bView;
        #endregion

        #region Constructor
        public GoBarGraph()
        {
            Selectable = true;

            scroll.GetScrollTotal = () => datas.Count > 0 && Series.Where(x => x.Visible).Count() > 0 ? datas.Count * DataWH : 0;
            scroll.GetScrollTick = () => DataWH;
            scroll.GetScrollView = () =>
            {
                var rt = Areas()["Graph"];
                return Direction == GoDirectionHV.Horizon ? rt.Height : rt.Width;
            };
            scroll.Refresh = () => Invalidate?.Invoke();
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
            var cTextBorder = thm.Back;
            var nTextBorder = 5F;
            var cRemark = thm.ToColor(RemarkColor);
            var cSeries = Series.ToDictionary(x => x, y => thm.ToColor(y.Color));
            var rts = Areas();
            var rtValueGrid = rts["ValueGrid"];
            var rtNameGrid = rts["NameGrid"];
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtScroll = rts["Scroll"];
            var rtViewBox = rts["ViewBox"];

            var rc = Series.Where(x => x.Visible).Count();
            using var p = new SKPaint { IsAntialias = true };
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
                if (datas.Count > 0)
                {
                    if (Direction == GoDirectionHV.Vertical) dwh = rtNameGrid.Width / datas.Count;
                    else if (Direction == GoDirectionHV.Horizon) dwh = rtNameGrid.Height / datas.Count;
                }
                else dwh = BarGap + BarSize + BarGap;
            }
            #endregion
            #region min / max
            double vmin = 0, vmax = 0;
            var dicser = Series.Where(x => x.Visible).ToDictionary(x => x.Name);
            if (dicser.Count > 0)
            {
                if (Mode == GoBarGraphMode.Stack)
                {
                    var cmin = datas.Min(x => x.Values.Where(x => dicser.TryGetValue(x.Key, out var vser) && vser.Visible).Sum(y => y.Value));
                    var cmax = datas.Max(x => x.Values.Where(x => dicser.TryGetValue(x.Key, out var vser) && vser.Visible).Sum(y => y.Value));
                    vmin = Minimum.HasValue ? Math.Min(cmin, Minimum.Value * Series.Where(x => x.Visible).Count()) : cmin;
                    vmax = Maximum.HasValue ? Math.Max(cmax, Maximum.Value * Series.Where(x => x.Visible).Count()) : cmax;
                }
                else if (Mode == GoBarGraphMode.List)
                {
                    var cmin = datas.Min(x => x.Values.Where(x => dicser.TryGetValue(x.Key, out var vser) && vser.Visible).Min(y => y.Value));
                    var cmax = datas.Max(x => x.Values.Where(x => dicser.TryGetValue(x.Key, out var vser) && vser.Visible).Max(y => y.Value));
                    vmin = Minimum.HasValue ? Math.Min(cmin, Minimum.Value) : cmin;
                    vmax = Maximum.HasValue ? Math.Max(cmax, Maximum.Value) : cmax;
                }
            }
            #endregion

            if (Direction == GoDirectionHV.Vertical)
            {
                #region Value Axis
                {
                    var x = rtValueGrid.Left;
                    var vw = Util.MeasureText(new string[] { $"{ValueTool.ToString(vmin, FormatString ?? "0")}", $"{ValueTool.ToString(vmax, FormatString ?? "0")}" }.OrderByDescending(x => x.Length).FirstOrDefault() ?? "", FontName, FontStyle, FontSize).Width;
                    var c = cText;

                    using var pe = SKPathEffect.CreateDash([3, 3,], 2);
                    p.PathEffect = pe;
                    for (int i = 0; i <= GraduationCount; i++)
                    {
                        var y = Convert.ToSingle(MathTool.Map(i, 0.0, GraduationCount, rtValueGrid.Bottom, rtValueGrid.Top));
                        var sVal = ValueTool.ToString(MathTool.Map(i, 0, GraduationCount, vmin, vmax), FormatString ?? "0");
                        var rtValue = Util.FromRect(x, y - ((FontSize + 2) / 2), vw, FontSize + 2);
                        if (rc > 0) Util.DrawText(canvas, sVal, FontName, FontStyle, FontSize, rtValue, c, GoContentAlignment.MiddleRight);

                        var ty = Convert.ToInt32(y) + 0.5F;
                        p.IsStroke = false;
                        p.StrokeWidth = 1F;
                        p.Color = cGrid;
                        canvas.DrawLine(rtGraph.Left, ty, rtGraph.Right, ty, p);
                    }
                    p.PathEffect = null;
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

                #region ViewBox
                Util.DrawTextIcon(canvas, "View", FontName, FontStyle, FontSize, "fa-magnifying-glass-chart", 20, GoDirectionHV.Horizon, 10, rtViewBox, bView ? cText : cGrid);
                Util.DrawBox(canvas, rtViewBox, SKColors.Transparent, bView ? cText : cGrid, GoRoundType.All, thm.Corner);
                #endregion

                #region Graph
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(Util.FromRect(rtGraph.Left, rtGraph.Top, rtGraph.Width + 1, rtGraph.Height + 1));
                    canvas.Translate(rtGraph.Left, rtGraph.Top);
                    canvas.Translate(spos, 0);

                    #region var
                    var vrt = Util.FromRect(-spos, 0, rtGraph.Width, rtGraph.Height);

                    var si = Math.Max(0, Convert.ToInt32(Math.Floor(vrt.Left / dwh)));
                    var ei = Math.Min(datas.Count - 1, Convert.ToInt32(Math.Ceiling(vrt.Right / dwh)));

                    var dic = Series.ToDictionary(x => x.Name);
                    int? hovidx = null;
                    SKRect[]? hovrts = null;
                    #endregion

                    #region draw
                    for (int i = si; i <= ei; i++)
                    {
                        var itm = datas[i];
                        var rt = Util.FromRect((dwh * i), 0, dwh, rtGraph.Height);

                        List<SKRect> vrts = [];
                        if (Mode == GoBarGraphMode.Stack)
                        {
                            var vsum = itm.Values.Where(x => Series.FirstOrDefault(y => y.Name == x.Key)?.Visible ?? false).Sum(x => x.Value);
                            var irt = new SKRect(rt.Left, Convert.ToSingle(MathTool.Map(vsum, vmin, vmax, rt.Bottom, rt.Top)), rt.Right, rt.Bottom);

                            #region Bar
                            var y = irt.Top;
                            foreach (var vk in itm.Values.Keys)
                            {
                                if (dic.TryGetValue(vk, out var ser) && ser.Visible)
                                {
                                    var brt = Util.FromRect(irt.Left + BarGap, y, irt.Width - (BarGap * 2), Convert.ToSingle(irt.Height * (itm.Values[vk] / vsum)));
                                    var c = cSeries[ser];

                                    Util.DrawBox(canvas, brt, c, GoRoundType.Rect, thm.Corner);
                                    y = brt.Bottom;

                                    vrts.Add(brt);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region Bar
                            var x = rt.Left + BarGap;
        
                            foreach (var vk in itm.Values.Keys)
                            {
                                if (dic.TryGetValue(vk, out var ser) && ser.Visible)
                                {
                                    var brt = new SKRect(x, Convert.ToSingle(MathTool.Map(itm.Values[vk], vmin, vmax, rt.Bottom, rt.Top)), x + BarSize, rt.Bottom);
                                    var c = cSeries[ser];
                                    Util.DrawBox(canvas, brt, c, GoRoundType.Rect, thm.Corner);
                                    x = brt.Right;

                                    vrts.Add(brt);
                                }
                            }
                            #endregion
                        }

                        if (CollisionTool.Check(rt, mx - rtGraph.Left - spos, my - rtGraph.Top)) { hovidx = i; hovrts = vrts.Count > 0 ? vrts.ToArray() : null; }
                    }
                    #endregion

                    #region Hover
                    if (bView)
                    {
                        using var pe = SKPathEffect.CreateDash([2, 2], 2);

                        using (new SKAutoCanvasRestore(canvas))
                        {
                            foreach (var vv in hovrts ?? []) canvas.ClipRect(vv, SKClipOperation.Difference);

                            p.IsStroke = false;
                            p.Color = Util.FromArgb(200, thm.Back);
                            canvas.DrawRect(Util.FromRect(-spos, 0, rtGraph.Width+1, rtGraph.Height+1), p);
                        }

                        if (hovidx.HasValue && CollisionTool.Check(rtGraph, mx, my) && hovrts != null)
                        {
                            var itm = datas[hovidx.Value];
                            var rt = Util.FromRect((dwh * hovidx.Value), 0, dwh, rtGraph.Height);
                            var x = Convert.ToInt32(rt.MidX) + 0.5F;

                            #region line
                            using (new SKAutoCanvasRestore(canvas))
                            {
                                foreach (var vv in hovrts) canvas.ClipRect(vv, SKClipOperation.Difference);

                                p.IsStroke = true;
                                p.StrokeWidth = 1;
                                p.Color = cGrid;
                                p.PathEffect = pe;
                                canvas.DrawLine(x, rt.Top, x, rt.Bottom, p);
                                p.PathEffect = null;
                            }
                            #endregion

                            #region time
                            var s = itm.Name;
                            var tw = Util.MeasureText(s, FontName, FontStyle, FontSize).Width;
                            var mvw = Series.Where(x => x.Visible).Select(x => Util.MeasureText($"{x.Alias} : {ValueTool.ToString(Maximum, FormatString ?? "0.0")}", FontName, FontStyle, FontSize).Width + 20).Max();
                            var gp = 15;
                            var gpw = (hovrts.Max(x => x.Right) - hovrts.Min(x => x.Left)) / 2F + 10F;
                            var bdir = x + gpw + Math.Max(tw, mvw) < rtGraph.Width - spos;
                            var tx = bdir ? x + gpw : x - gpw;
                            var ty = gp;

                            Util.DrawText(canvas, s, FontName, FontStyle, FontSize, Util.FromRect(tx - (bdir ? 0 : tw), ty, tw, 30), cText, GoContentAlignment.MiddleLeft);
                            ty += 30;
                            #endregion

                            foreach (var ser in Series.Where(x => x.Visible))
                            {
                                var v = itm.Values[ser.Name];
                                var c = cSeries[ser];

                                #region box
                                var sv = $"{ser.Alias} : {ValueTool.ToString(v, FormatString ?? "0.0")}";
                                var vw = Util.MeasureText(sv, FontName, FontStyle, FontSize).Width + 20;
                                var trt = Util.FromRect(tx - (bdir ? 0 : vw), ty, vw, 30);

                                Util.DrawBox(canvas, trt, thm.Back, c, GoRoundType.All, thm.Corner);
                                Util.DrawText(canvas, sv, FontName, FontStyle, FontSize, trt, cText);
                                ty += 40;
                                #endregion
                            }
                        }
                    }
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
                        var rt = Util.FromRect(rtRemark.Left + 10, y, rtRemark.Width - 20, box);
                        var c = cSeries[v];
                        var (rtIco, rtText) = Util.TextIconBounds(v.Alias, FontName, FontStyle, FontSize, new SKSize(box, box), GoDirectionHV.Horizon, 10, rt, GoContentAlignment.MiddleLeft);
                        Util.DrawBox(canvas, rtIco, c, GoRoundType.Rect, thm.Corner);
                        Util.DrawText(canvas, v.Alias, FontName, FontStyle, FontSize, rtText, cText);
                        y += box + gap;
                    }
                }
                #endregion

            }
            else if (Direction == GoDirectionHV.Horizon)
            {
                #region Value Axis
                {
                    var y = rtValueGrid.Top;
                    var c = cText;

                    using var pe = SKPathEffect.CreateDash([3, 3,], 2);
                    p.PathEffect = pe;

                    for (int i = 0; i <= GraduationCount; i++)
                    {
                        var x = Convert.ToSingle(MathTool.Map(i, 0.0, GraduationCount, rtValueGrid.Left, rtValueGrid.Right));
                        var sVal = ValueTool.ToString(MathTool.Map(i, 0, GraduationCount, vmin, vmax), FormatString ?? "0") ?? "";
                        var vw = Util.MeasureText(sVal, FontName, FontStyle, FontSize).Width;
                        var rtValue = Util.FromRect(x - vw / 2F, y, vw, FontSize + 2);
                        if (rc > 0) Util.DrawText(canvas, sVal, FontName, FontStyle, FontSize, rtValue, c, GoContentAlignment.MiddleRight);

                        var tx = Convert.ToInt32(x) + 0.5F;
                        p.IsStroke = false;
                        p.StrokeWidth = 1F;
                        p.Color = cGrid;
                        canvas.DrawLine(tx, rtGraph.Top, tx, rtGraph.Bottom, p);
                    }

                    p.PathEffect = null;

                }
                #endregion

                #region Name Axis
                if (rc > 0)
                {
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(rtNameGrid);
                        canvas.Translate(rtNameGrid.Left, rtNameGrid.Top);
                        canvas.Translate(0, spos);

                        var vrt = Util.FromRect(0, -spos, rtNameGrid.Width, rtNameGrid.Height);

                        var si = Math.Max(0, Convert.ToInt32(Math.Floor(vrt.Top / dwh)));
                        var ei = Math.Min(datas.Count - 1, Convert.ToInt32(Math.Ceiling(vrt.Bottom / dwh)));

                        for (int i = si; i <= ei; i++)
                        {
                            var itm = datas[i];
                            var rt = Util.FromRect(0, (dwh * i), rtNameGrid.Width, dwh);
                            Util.DrawText(canvas, itm.Name, FontName, FontStyle, FontSize, rt, cText, GoContentAlignment.MiddleRight);
                        }
                    }

                }
                #endregion

                #region ViewBox
                Util.DrawTextIcon(canvas, "View", FontName, FontStyle, FontSize, "fa-magnifying-glass-chart", 20, GoDirectionHV.Horizon, 10, rtViewBox, bView ? cText : cGrid);
                Util.DrawBox(canvas, rtViewBox, SKColors.Transparent, bView ? cText : cGrid, GoRoundType.All, thm.Corner);
                #endregion

                #region Graph
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(Util.FromRect(rtGraph.Left, rtGraph.Top, rtGraph.Width + 1, rtGraph.Height + 1));
                    canvas.Translate(rtGraph.Left, rtGraph.Top);
                    canvas.Translate(0, spos);

                    #region var
                    var vrt = Util.FromRect(0, -spos, rtNameGrid.Width, rtNameGrid.Height);

                    var si = Math.Max(0, Convert.ToInt32(Math.Floor(vrt.Top / dwh)));
                    var ei = Math.Min(datas.Count - 1, Convert.ToInt32(Math.Ceiling(vrt.Bottom / dwh)));

                    var dic = Series.ToDictionary(x => x.Name);

                    int? hovidx = null;
                    SKRect[]? hovrts = null;
                    #endregion

                    #region draw
                    for (int i = si; i <= ei; i++)
                    {
                        var itm = datas[i];
                        var rt = Util.FromRect(0, (dwh * i), rtGraph.Width, dwh);

                        List<SKRect> vrts = [];
                        if (Mode == GoBarGraphMode.Stack)
                        {
                            var vsum = itm.Values.Where(x => Series.FirstOrDefault(y => y.Name == x.Key)?.Visible ?? false).Sum(x => x.Value);
                            var irt = new SKRect(rt.Left, rt.Top, Convert.ToSingle(MathTool.Map(vsum, vmin, vmax, rt.Left, rt.Right)), rt.Bottom);

                            #region Bar
                            var x = irt.Right;
                            foreach (var vk in itm.Values.Keys)
                            {
                                if (dic.TryGetValue(vk, out var ser) && ser.Visible)
                                {
                                    var w = Convert.ToSingle(irt.Width * (itm.Values[vk] / vsum));
                                    var brt = Util.FromRect(x - w, irt.Top + BarGap, w, irt.Height - (BarGap * 2));
                                    var c = cSeries[ser];

                                    Util.DrawBox(canvas, brt, c, GoRoundType.Rect, thm.Corner);
                                    x = brt.Left;
                                    vrts.Add(brt);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region Bar
                            var y = rt.Top + BarGap;
                            foreach (var vk in itm.Values.Keys)
                            {
                                if (dic.TryGetValue(vk, out var ser) && ser.Visible)
                                {
                                    var brt = new SKRect(rt.Left, y, Convert.ToSingle(MathTool.Map(itm.Values[vk], vmin, vmax, rt.Left, rt.Right)), y + BarSize);
                                    var c = cSeries[ser];
                                    Util.DrawBox(canvas, brt, c, GoRoundType.Rect, thm.Corner);
                                    y = brt.Bottom;
                                    vrts.Add(brt);
                                }
                            }
                            #endregion
                        }

                        if (CollisionTool.Check(rt, mx - rtGraph.Left, my - rtGraph.Top - spos)) { hovidx = i; hovrts = vrts.Count > 0 ? vrts.ToArray() : null; }
                    }
                    #endregion

                    #region Hover
                    if (bView)
                    {
                        using var pe = SKPathEffect.CreateDash([2, 2], 2);

                        using (new SKAutoCanvasRestore(canvas))
                        {
                            foreach (var vv in hovrts ?? []) canvas.ClipRect(vv, SKClipOperation.Difference);

                            p.IsStroke = false;
                            p.Color = Util.FromArgb(200, thm.Back);
                            canvas.DrawRect(Util.FromRect(0, -spos, rtGraph.Width+1, rtGraph.Height+1), p);
                        }

                        if (hovidx.HasValue && CollisionTool.Check(rtGraph, mx, my) && hovrts != null)
                        {
                            var itm = datas[hovidx.Value];
                            var rt = Util.FromRect(0, (dwh * hovidx.Value), rtGraph.Width, dwh);
                            var y = Convert.ToInt32(rt.MidY) + 0.5F;

                            #region line
                            using (new SKAutoCanvasRestore(canvas))
                            {
                                foreach (var vv in hovrts) canvas.ClipRect(vv, SKClipOperation.Difference);

                                p.IsStroke = true;
                                p.StrokeWidth = 1;
                                p.Color = cGrid;
                                p.PathEffect = pe;
                                canvas.DrawLine(rt.Left, y, rt.Right, y, p);
                                p.PathEffect = null;
                            }
                            #endregion

                            #region time
                            var s = itm.Name;
                            var tw = Util.MeasureText(s, FontName, FontStyle, FontSize).Width;
                            var th = (rc * 40) + 30;
                            var gp = 15;
                            var gph = (hovrts.Max(x => x.Bottom) - hovrts.Min(x => x.Top)) / 2F + 10F;
                            var bdir = y + gph + th < rtGraph.Height - spos;
                            var ty = bdir ? y + gph : y - gph;
                            var tx = rtGraph.Width - gp;

                            Util.DrawText(canvas, s, FontName, FontStyle, FontSize, Util.FromRect(tx - tw, ty - (bdir ? 0 : th), tw, 30), cText, GoContentAlignment.MiddleRight);
                            ty += 30;
                            #endregion

                            foreach (var ser in Series.Where(x => x.Visible))
                            {
                                var v = itm.Values[ser.Name];
                                var c = cSeries[ser];

                                #region box
                                var sv = $"{ser.Alias} : {ValueTool.ToString(v, FormatString ?? "0.0")}";
                                var vw = Util.MeasureText(sv, FontName, FontStyle, FontSize).Width + 20;
                                var trt = Util.FromRect(tx - vw, ty - (bdir ? 0 : th), vw, 30);

                                Util.DrawBox(canvas, trt, thm.Back, c, GoRoundType.All, thm.Corner);
                                Util.DrawText(canvas, sv, FontName, FontStyle, FontSize, trt, cText);
                                ty += 40;
                                #endregion
                            }
                        }
                    }
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
                        var rt = Util.FromRect(rtRemark.Left + 10, y, rtRemark.Width - 20, box);
                        var c = cSeries[v];
                        var (rtIco, rtText) = Util.TextIconBounds(v.Alias, FontName, FontStyle, FontSize, new SKSize(box, box), GoDirectionHV.Horizon, 10, rt, GoContentAlignment.MiddleLeft);
                        Util.DrawBox(canvas, rtIco, c, GoRoundType.Rect, thm.Corner);
                        Util.DrawText(canvas, v.Alias, FontName, FontStyle, FontSize, rtText, cText);
                        y += box + gap;
                    }
                }
                #endregion
            }

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
            var rtScroll = rts["Scroll"];
            var rtViewBox = rts["ViewBox"];
            #endregion

            scroll.MouseDown(x, y, rtScroll);
            if (scroll.TouchMode && CollisionTool.Check(rtGraph, x, y)) scroll.TouchDown(x, y);

            if (CollisionTool.Check(rtViewBox, x, y)) bView = !bView;

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
            var rtScroll = rts["Scroll"];
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
            var rtScroll = rts["Scroll"];
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
            var rtScroll = rts["Scroll"];
            #endregion

            if (CollisionTool.Check(rtRemark, x, y))
            {
                var ls = Series.Select(x => new GoListItem { Text = x.Alias, Tag = x }).ToList();
                var sels = ls.Where(x => ((GoGraphSeries)x.Tag!).Visible).ToList();
                GoDiaglos.SelectorBox.ShowCheck("Graph Series", 1, ls, sels, (vs) =>
                {
                    if (vs != null)
                    {
                        var vls = vs.Select(x => x.Tag as GoGraphSeries);
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

            var box = FontSize + 2;
            var gap = 10;

            #region min / max
            double vmin = 0, vmax = 0;
            var dicser = Series.Where(x => x.Visible).ToDictionary(x => x.Name);
            if (dicser.Count > 0)
            {
                if (Mode == GoBarGraphMode.Stack)
                {
                    var cmin = datas.Min(x => x.Values.Where(x => dicser.TryGetValue(x.Key, out var vser) && vser.Visible).Sum(y => y.Value));
                    var cmax = datas.Max(x => x.Values.Where(x => dicser.TryGetValue(x.Key, out var vser) && vser.Visible).Sum(y => y.Value));
                    vmin = Minimum.HasValue ? Math.Min(cmin, Minimum.Value * Series.Where(x => x.Visible).Count()) : cmin;
                    vmax = Maximum.HasValue ? Math.Max(cmax, Maximum.Value * Series.Where(x => x.Visible).Count()) : cmax;
                }
                else if (Mode == GoBarGraphMode.List)
                {
                    var cmin = datas.Min(x => x.Values.Where(x => dicser.TryGetValue(x.Key, out var vser) && vser.Visible).Min(y => y.Value));
                    var cmax = datas.Max(x => x.Values.Where(x => dicser.TryGetValue(x.Key, out var vser) && vser.Visible).Max(y => y.Value));
                    vmin = Minimum.HasValue ? Math.Min(cmin, Minimum.Value) : cmin;
                    vmax = Maximum.HasValue ? Math.Max(cmax, Maximum.Value) : cmax;
                }
            }
            #endregion

            if (Direction == GoDirectionHV.Vertical)
            {
                var rsw = Series.Select(x => Util.MeasureText(x.Alias, FontName, FontStyle, FontSize).Width + gap + box);
                var vw = Util.MeasureText(new string[] { $"{ValueTool.ToString(vmin, FormatString ?? "0")}", $"{ValueTool.ToString(vmax, FormatString ?? "0")}" }.OrderByDescending(x => x.Length).FirstOrDefault() ?? "", FontName, FontStyle, FontSize).Width;

                var trw = Util.MeasureTextIcon("View", FontName, FontStyle, FontSize, "fa-magnifying-glass-chart", 20, GoDirectionHV.Horizon, 10).Width;
                var rw = Math.Max(trw + 20, rsw.Any() ? rsw.Max() + 20 : 20);
                var rc = Math.Max(1, Series.Where(x => x.Visible).Count());
                var rts = Util.Grid(rtContent, [$"{vw}px", "10px", "100%", "10px", $"{rw}px"], ["20px", "10px", "100%", "10px", "40px", $"{Scroll.SC_WH}px"]);

                var rtValueGrid = Util.Merge(rts, 0, 2, 1, 1);
                var rtNameGrid = Util.Merge(rts, 2, 4, 1, 1);
                var rtGraph = Util.Merge(rts, 2, 2, 1, 1);
                var rtRemark = Util.Merge(rts, 4, 2, 1, 1);
                var rtScroll = Util.Merge(rts, 2, 5, 1, 1);

                dic["ValueGrid"] = rtValueGrid;
                dic["NameGrid"] = rtNameGrid;
                dic["Remark"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, (box * rc) + (gap * (rc + 1))));
                dic["ViewBox"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, 40), GoContentAlignment.TopCenter);
                dic["Graph"] = rtGraph;
                dic["Scroll"] = rtScroll;
            }
            else
            {
                var rsw = Series.Select(x => Util.MeasureText(x.Alias, FontName, FontStyle, FontSize).Width + gap + box);
                var nw = Util.MeasureText(datas.OrderByDescending(x => x.Name.Length).FirstOrDefault()?.Name ?? "", FontName, FontStyle, FontSize).Width;

                var trw = Util.MeasureTextIcon("View", FontName, FontStyle, FontSize, "fa-magnifying-glass-chart", 20, GoDirectionHV.Horizon, 10).Width;
                var rw = Math.Max(trw + 20, rsw.Any() ? rsw.Max() + 20 : box + gap + 20);
                var rc = Math.Max(1, Series.Where(x => x.Visible).Count());
                var rts = Util.Grid(rtContent, [$"{nw}px", "10px", "100%", "10px", $"{(Scroll.SC_WH)}px", "10px", $"{rw}px"], ["20px", "10px", "100%", "10px", $"{FontSize}px"]);

                var rtNameGrid = Util.Merge(rts, 0, 2, 1, 1);
                var rtValueGrid = Util.Merge(rts, 2, 4, 1, 1);
                var rtGraph = Util.Merge(rts, 2, 2, 1, 1);
                var rtRemark = Util.Merge(rts, 6, 2, 1, 1);
                var rtScroll = Util.Merge(rts, 4, 2, 1, 1);

                dic["ValueGrid"] = rtValueGrid;
                dic["NameGrid"] = rtNameGrid;
                dic["Remark"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, (box * rc) + (gap * (rc + 1))));
                dic["ViewBox"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, 40), GoContentAlignment.TopCenter);
                dic["Graph"] = rtGraph;
                dic["Scroll"] = rtScroll;
            }

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region SetDataSource
        public void SetDataSource<T>(string XAxisName, IEnumerable<T> values)
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
    
}
