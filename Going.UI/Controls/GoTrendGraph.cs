using Going.UI.Datas;
using Going.UI.Dialogs;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoTrendGraph : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public string GridColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 1)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 2)] public string RemarkColor { get; set; } = "Base2";
        [GoProperty(PCategory.Control, 3)] public string GraphColor { get; set; } = "Back";

        [GoProperty(PCategory.Control, 4)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 5)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 6)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 7)] public TimeSpan MaximumXScale { get; set; } = new TimeSpan(1, 0, 0, 0);
        [GoProperty(PCategory.Control, 8)] public TimeSpan XScale { get; set; } = new TimeSpan(1, 0, 0);
        [GoProperty(PCategory.Control, 9)] public TimeSpan XAxisGraduationTime { get; set; } = new TimeSpan(0, 10, 0);
        [GoProperty(PCategory.Control, 10)] public int YAxisGraduationCount { get; set; } = 10;
        [GoProperty(PCategory.Control, 11)] public string? TimeFormatString { get; set; } = null;
        [GoProperty(PCategory.Control, 12)] public string? ValueFormatString { get; set; } = null;

        [GoProperty(PCategory.Control, 13)] public int Interval { get; set; } = 1000;
        [GoProperty(PCategory.Control, 14)] public bool IsStart { get; private set; } = false;

        [GoProperty(PCategory.Control, 15)] public List<GoLineGraphSeries> Series { get; set; } = [];

        #region Pause
        private bool bPause = false;
        [JsonIgnore]
        public bool Pause
        {
            get => bPause;
            set
            {
                if (bPause != value)
                {
                    bPause = value;
                    if (bPause) pdatas.Clear();
                    else
                    {
                        if (pdatas.Count > 0)
                        {
                            var last = pdatas.Last();

                            datas.AddRange(pdatas);
                            var ar = datas.ToArray();
                            datas = ar.Where(x => last.Time - MaximumXScale - TimeSpan.FromMilliseconds(Interval * 2) <= x.Time).ToList();
                            pdatas.Clear();
                        }
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Member Variable
        DateTime startTime = DateTime.Now;
        DateTime nowTime = DateTime.Now;
        List<GoTimeGraphValue> datas = new List<GoTimeGraphValue>();
        List<GoTimeGraphValue> pdatas = new List<GoTimeGraphValue>();

        Scroll scroll = new Scroll() { Direction = ScrollDirection.Horizon };
        float mx, my;
        bool bView;

        object? value = null;
        Dictionary<string, PropertyInfo> dicProps = new Dictionary<string, PropertyInfo>();
        object oLock = new object();

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Constructor
        public GoTrendGraph()
        {
            Selectable = true;

            scroll.GetScrollTotal = () => Series.Count > 0 ? (nowTime - startTime).TotalMilliseconds : 0L;
            scroll.GetScrollTick = () => XAxisGraduationTime.TotalMilliseconds;
            scroll.GetScrollView = () => XScale.TotalMilliseconds;
            scroll.GetScrollScaleFactor = () =>
            {
                var rtGraph = Areas()["Graph"];
                return XScale.TotalMilliseconds / (double)rtGraph.Width;
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
            var cGraph = thm.ToColor(GraphColor);
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
            var rtScroll = rts["Scroll"];
            var rtViewBox = rts["ViewBox"];
            var rtPauseBox = rts["PauseBox"];
            var pdt = new DateTime(DateTime.Now.Year, 1, 1);

            using var p = new SKPaint { IsAntialias = true };

            var rc = Series.Where(x => x.Visible).Count();
            var sposw = Convert.ToSingle(TimeSpan.FromMilliseconds(scroll.ScrollPositionWithOffsetR).TotalMilliseconds / XScale.TotalMilliseconds * rtGraph.Width);

            var now = nowTime;
            #endregion

            p.Color = cGraph; p.IsStroke = false; canvas.DrawRect(rtGraph, p);

            #region Value Axis
            {
                #region Axis
                var x = rtValueTitle.Left;
                if (rc > 0)
                {
                    foreach (var v in Series.Where(x => x.Visible))
                    {
                        var nw = new string[] { v.Alias, ValueTool.ToString(v.Minimum, ValueFormatString) ?? "", ValueTool.ToString(v.Maximum, ValueFormatString) ?? "" }.Select(y => Util.MeasureText(y, FontName, FontStyle, FontSize).Width).OrderByDescending(z => z).FirstOrDefault();
                        var rtTitle = Util.FromRect(x, rtValueTitle.Top, nw, rtValueTitle.Height);
                        var c = cSeries[v];
                        Util.DrawText(canvas, v.Alias, FontName, FontStyle, FontSize, rtTitle, c, GoContentAlignment.MiddleRight);

                        for (int i = 0; i <= YAxisGraduationCount; i++)
                        {
                            var y = Convert.ToSingle(MathTool.Map(i, 0.0, YAxisGraduationCount, rtValueGrid.Bottom, rtValueGrid.Top));
                            var sVal = ValueTool.ToString(MathTool.Map(i, 0, YAxisGraduationCount, v.Minimum, v.Maximum), ValueFormatString);
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
                for (int i = 0; i <= YAxisGraduationCount; i++)
                {
                    var y = Convert.ToSingle(MathTool.Map(i, 0.0, YAxisGraduationCount, rtGraph.Bottom, rtGraph.Top));
                    p.IsStroke = false;
                    p.IsAntialias = false;
                    p.StrokeWidth = 1F;
                    p.Color = cGrid;
                    canvas.DrawLine(rtGraph.Left, y, rtGraph.Right, y, p);
                    p.IsAntialias = true;
                }
                p.PathEffect = null;
                #endregion
            }
            #endregion

            #region Name Axis
            if (rc > 0 && datas.Count > 0)
            {
                #region var
                var rmts = (now - startTime).TotalMilliseconds;
                var rw = MathTool.Map(rmts, 0, XScale.TotalMilliseconds, 0, rtGraph.Width);
                var rsz = Util.MeasureText(now.ToString(TimeFormatString ?? "yyyy.MM.dd\r\nHH:mm:ss"), FontName, FontStyle, FontSize);
                var wts = TimeSpan.FromMilliseconds(-scroll.ScrollPositionWithOffsetR);
                var ved = now + wts;
                var vst = ved - XScale;
                #endregion

                #region Axis
                {
                    var vrt = Util.FromRect(-sposw, 0, rtNameGrid.Width, rtNameGrid.Height);
                    using var pe = SKPathEffect.CreateDash([3, 3,], 2);
                    p.PathEffect = pe;

                    var tmst = new DateTime(startTime.Ticks / XAxisGraduationTime.Ticks * XAxisGraduationTime.Ticks);
                    for (DateTime i = new(vst.Year, vst.Month, vst.Day, vst.Hour, vst.Minute, vst.Second); i <= ved; i += XAxisGraduationTime)
                    {
                        if (i >= tmst)
                        {
                            var x = Convert.ToSingle(MathTool.Map((i - startTime).TotalMilliseconds, 0, rmts, rtGraph.Width - rw, rtGraph.Width));

                            using (new SKAutoCanvasRestore(canvas))
                            {
                                canvas.ClipRect(Util.FromRect(rtNameGrid.Left - (rsz.Width / 2f), rtNameGrid.Top, rtNameGrid.Width + rsz.Width, rtNameGrid.Height));
                                canvas.Translate(rtNameGrid.Left, rtNameGrid.Top);
                                canvas.Translate(sposw, 0);

                                var sval = i.ToString(TimeFormatString ?? "yyyy.MM.dd\r\nHH:mm:ss");
                                var sz = Util.MeasureText(sval, FontName, FontStyle, FontSize);
                                var rt = Util.FromRect(x - sz.Width / 2F, 0, sz.Width, rtNameGrid.Height);

                                Util.DrawText(canvas, sval, FontName, FontStyle, FontSize, rt, cText);
                            }

                            using (new SKAutoCanvasRestore(canvas))
                            {
                                canvas.ClipRect(rtGraph);
                                canvas.Translate(rtGraph.Left, rtGraph.Top);
                                canvas.Translate(sposw, 0);

                                p.IsAntialias = false;
                                p.IsStroke = true;
                                p.StrokeWidth = 1F;
                                p.Color = cGrid;
                                canvas.DrawLine(x, 0, x, rtGraph.Height, p);
                                p.IsAntialias = true;
                            }

                        }
                    }
                    p.PathEffect = null;
                }
                #endregion
            }
            #endregion

            #region ViewBox / Pause
            Util.DrawTextIcon(canvas, "View", FontName, FontStyle, FontSize, "fa-magnifying-glass-chart", 20, GoDirectionHV.Horizon, 10, rtViewBox, bView ? cText : cGrid);
            Util.DrawBox(canvas, rtViewBox, SKColors.Transparent, bView ? cText : cGrid, GoRoundType.All, thm.Corner);

            Util.DrawTextIcon(canvas, "Pause", FontName, FontStyle, FontSize, "fa-pause", 20, GoDirectionHV.Horizon, 10, rtPauseBox, Pause ? cText : cGrid);
            Util.DrawBox(canvas, rtPauseBox, SKColors.Transparent, Pause ? cText : cGrid, GoRoundType.All, thm.Corner);
            #endregion

            #region Graph
            if (datas.Count > 0)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    #region var
                    var rmts = (now - startTime).TotalMilliseconds;
                    var rw = MathTool.Map(rmts, 0, XScale.TotalMilliseconds, 0, rtGraph.Width);
                    var rsz = Util.MeasureText(now.ToString(TimeFormatString ?? "yyyy.MM.dd\r\nHH:mm:ss"), FontName, FontStyle, FontSize);
                    var wts = TimeSpan.FromMilliseconds(-scroll.ScrollPositionWithOffsetR);
                    var ved = now + wts;
                    var vst = ved - XScale;
                    #endregion

                    canvas.ClipRect(rtGraph);
                    canvas.Translate(rtGraph.Left, rtGraph.Top);
                    canvas.Translate(sposw, 0);

                    var vrt = Util.FromRect(-sposw, 0, rtGraph.Width, rtGraph.Height);

                    var (si, ei) = FindRange(datas, vst, ved);

                    var va = datas.ToArray();
                    if (si >= 0 && si < va.Length && ei >= 0 && ei < va.Length)
                    {
                        foreach (var ser in Series.Where(x => x.Visible))
                        {
                            var c = cSeries[ser];
                            #region make lines
                            var ls = new List<LGV>();
                            for (int i = si; i <= ei; i++)
                            {
                                var itm = va[i];
                                var x = Convert.ToSingle(MathTool.Map((itm.Time - startTime).TotalMilliseconds, 0, rmts, rtGraph.Width - rw, rtGraph.Width));
                                var y = Convert.ToSingle(MathTool.Map(itm.Values[ser.Name], ser.Minimum, ser.Maximum, rtGraph.Height, 0));
                                ls.Add(new LGV() { Position = new SKPoint(x, y), Value = itm.Values[ser.Name] });
                            }

                            p.StrokeWidth = 1F;
                            p.Color = c;
                            p.IsStroke = false;
                            if (ls.Count >= 2)
                            {
                                var pts = ls.Select(x => x.Position).ToArray();
                                canvas.DrawPoints(SKPointMode.Polygon, pts, p);
                            }
                            #endregion
                        }
                    }

                    #region Hover
                    if (bView)
                    {
                        using var pe = SKPathEffect.CreateDash([2, 2], 2);

                        p.IsStroke = false;
                        p.Color = Util.FromArgb(200, cGraph);
                        canvas.DrawRect(Util.FromRect(-sposw, 0, rtGraph.Width, rtGraph.Height), p);

                        if (CollisionTool.Check(rtGraph, mx, my))
                        {
                            var tmx = Convert.ToSingle(mx - rtGraph.Left - sposw);
                            var tmSel = vst + TimeSpan.FromMilliseconds(MathTool.Map(tmx + sposw, 0, rtGraph.Width, 0, XScale.TotalMilliseconds));

                            var (pi, ni) = FindItem([.. va], tmSel);

                            if (pi >= 0 && pi < va.Length && ni >= 0 && ni < va.Length)
                            {
                                #region line
                                p.IsStroke = true;
                                p.IsAntialias = false;
                                p.StrokeWidth = 1;
                                p.Color = cGrid;
                                p.PathEffect = pe;
                                canvas.DrawLine(tmx, 0, tmx, rtGraph.Height, p);
                                p.PathEffect = null;
                                p.IsAntialias = true;
                                #endregion

                                #region time
                                var s = ValueTool.ToString(tmSel, TimeFormatString ?? "yyyy.MM.dd HH:mm:ss");
                                var tw = Util.MeasureText(s, FontName, FontStyle, FontSize).Width;
                                var mvw = Series.Where(x => x.Visible).Select(x => Util.MeasureText($"{x.Alias} : {ValueTool.ToString(x.Maximum, ValueFormatString ?? "0.0")}", FontName, FontStyle, FontSize).Width + 20).Max();
                                var gp = 15;
                                var bdir = tmx + gp + Math.Max(tw, mvw) < rtGraph.Width - sposw;
                                var tx = bdir ? tmx + gp : tmx - gp;
                                var ty = gp;

                                Util.DrawText(canvas, s, FontName, FontStyle, FontSize, Util.FromRect(tx - (bdir ? 0 : tw), ty, tw, 30), cText, GoContentAlignment.MiddleLeft);
                                ty += 30;
                                #endregion

                                foreach (var ser in Series.Where(x => x.Visible))
                                {
                                    var v = MathTool.Map(tmSel.Ticks, datas[pi].Time.Ticks, datas[ni].Time.Ticks, datas[pi].Values[ser.Name], datas[ni].Values[ser.Name]);

                                    #region point
                                    var c = cSeries[ser];
                                    var y = Convert.ToInt32(MathTool.Map(v, ser.Minimum, ser.Maximum, rtGraph.Height, 0));
                                    p.IsStroke = false;
                                    p.Color = c;
                                    canvas.DrawCircle(tmx, y, 5, p);
                                    #endregion

                                    #region box
                                    var sv = $"{ser.Alias} : {ValueTool.ToString(v, ValueFormatString ?? "0.0")}";
                                    var vw = Util.MeasureText(sv, FontName, FontStyle, FontSize).Width + 20;
                                    var trt = Util.FromRect(tx - (bdir ? 0 : vw), ty, vw, 30);

                                    Util.DrawBox(canvas, trt, cGraph, c, GoRoundType.All, thm.Corner);
                                    Util.DrawText(canvas, sv, FontName, FontStyle, FontSize, trt, cText);
                                    ty += 40;
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion
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

            scroll.DrawR(canvas, rtScroll);

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
            var rtPauseBox = rts["PauseBox"];
            #endregion

            scroll.MouseDownR(x, y, rtScroll);
            if (scroll.TouchMode && CollisionTool.Check(rtGraph, x, y)) scroll.TouchDownR(x, y);

            if (CollisionTool.Check(rtViewBox, x, y)) bView = !bView;
            if (CollisionTool.Check(rtPauseBox, x, y)) Pause = !Pause;

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

            scroll.MouseMoveR(x, y, rtScroll);
            if (scroll.TouchMode) scroll.TouchMoveR(x, y);
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

            scroll.MouseUpR(x, y);
            if (scroll.TouchMode) scroll.TouchUpR(x, y);

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

                var lsw = Series.Where(x => x.Visible).Select(x => new string[] { x.Alias, ValueTool.ToString(x.Minimum, ValueFormatString) ?? "", ValueTool.ToString(x.Maximum, ValueFormatString) ?? "" }.Select(y => Util.MeasureText(y, FontName, FontStyle, FontSize).Width).OrderByDescending(z => z).FirstOrDefault());
                var rsw = Series.Select(x => Util.MeasureText(x.Alias, FontName, FontStyle, FontSize).Width + gap + box);
                var vw = lsw.Any() ? lsw.Sum() + ((lsw.Count() - 1) * 10) : 0;
                var trw = Util.MeasureTextIcon("View", FontName, FontStyle, FontSize, "fa-magnifying-glass-chart", 20, GoDirectionHV.Horizon, 10).Width;
                var rw = Math.Max(trw + 20, rsw.Any() ? rsw.Max() + 20 : 20);
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
                dic["ViewBox"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, 40), GoContentAlignment.TopCenter);
                var rtPause = dic["ViewBox"]; rtPause.Offset(0, 50);
                dic["PauseBox"] = rtPause;
                dic["Graph"] = rtGraph;
                dic["Scroll"] = rtScroll;
            }

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region Start
        public void Start<T>(T value)
        {
            if (!IsStart)
            {
                if (value != null)
                {
                    if (Series.Count > 0)
                    {
                        var pls = typeof(T).GetProperties();

                        var props = typeof(T).GetProperties().Where(x => x.PropertyType == typeof(double) || x.PropertyType == typeof(float) || x.PropertyType == typeof(decimal) ||
                                                                     x.PropertyType == typeof(byte) || x.PropertyType == typeof(sbyte) ||
                                                                     x.PropertyType == typeof(short) || x.PropertyType == typeof(ushort) ||
                                                                     x.PropertyType == typeof(int) || x.PropertyType == typeof(uint) ||
                                                                     x.PropertyType == typeof(long) || x.PropertyType == typeof(ulong));

                        var nmls = props.Select(x => x.Name).ToList();
                        int nCnt = Series.Where(x => nmls.Contains(x.Name)).Count();
                        if (nCnt == Series.Count)
                        {
                            dicProps = props.ToDictionary(x => x.Name);
                            pdatas.Clear();
                            datas.Clear();
                            this.value = value;

                            cancel = new CancellationTokenSource();
                            task = Task.Run(async () =>
                            {
                                var token = cancel.Token;

                                startTime = DateTime.Now;
                                IsStart = true;
                                while (!token.IsCancellationRequested && IsStart)
                                {
                                    AddData();
                                    Invalidate?.Invoke();
                                    await Task.Delay(Interval);
                                }
                                IsStart = false;

                            }, cancel.Token);
                        }
                        else throw new Exception("잘못된 데이터 입니다.");
                    }
                    else throw new Exception("해당 타입에 지정한 X축 이름이 존재하지 않거나 DateTime 형이 아닙니다.");
                }
                else throw new Exception("Data가 Null 일 수 없습니다.");
            }
            else throw new Exception("이미 실행 중 입니다");
        }
        #endregion
        #region Stop
        public void Stop()
        {
            try { cancel?.Cancel(false); }
            finally
            {
                cancel?.Dispose();
                cancel = null;
            }

            if (task != null)
            {
                Task.WhenAny(task);
                task = null;
            }
        }
        #endregion
        #region SetData
        public void SetData<T>(T Data)
        {
            if (IsStart && this.value != null && this.value.GetType() == typeof(T))
                this.value = Data;
        }
        #endregion
        #region AddData
        void AddData()
        {
            if (value != null)
            {
                lock (oLock)
                {
                    var tgv = new GoTimeGraphValue() { Time = DateTime.Now };

                    foreach (var vk in dicProps.Keys) tgv.Values.Add(vk, Convert.ToDouble(dicProps[vk].GetValue(value)));

                    if (Pause)
                    {
                        pdatas.Add(tgv);
                    }
                    else
                    {
                        datas.Add(tgv);
                        var ar = datas.ToArray();
                        datas = ar.Where(x => tgv.Time - MaximumXScale - TimeSpan.FromMilliseconds(Interval * 2) <= x.Time).ToList();
                        startTime = datas[0].Time;
                        nowTime = tgv.Time;
                    }
                }
            }
        }
        #endregion

        #region FindRange
        (int startIdx, int endIdx) FindRange(List<GoTimeGraphValue> ls, DateTime startTime, DateTime endTime)
        {
            int left = 0, right = ls.Count - 1;
            int startIdx = -1, endIdx = -1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (ls[mid].Time >= startTime)
                {
                    startIdx = mid;
                    right = mid - 1;
                }
                else left = mid + 1;
            }

            left = 0;
            right = ls.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (ls[mid].Time <= endTime)
                {
                    endIdx = mid;
                    left = mid + 1;
                }
                else right = mid - 1;
            }

            return (startIdx, endIdx);
        }
        #endregion
        #region FindItem
        (int si, int ei) FindItem(List<GoTimeGraphValue> ls, DateTime target)
        {
            int left = 0, right = ls.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;

                if (ls[mid].Time == target)
                    return (mid, mid);
                else if (ls[mid].Time < target)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            int prevIndex = Math.Max(0, right);
            int nextIndex = Math.Min(ls.Count - 1, left);

            return (prevIndex, nextIndex);
        }
        #endregion
        #endregion
    }
}
