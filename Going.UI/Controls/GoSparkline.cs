using Going.UI.Datas;
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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    /// <summary>
    /// 축/범례/스크롤 없이 라인(+옵션 영역 채움)과 기준선, 최소/최대/XScale 텍스트만 표시하는 미니 트렌드 그래프.
    /// 여러 시리즈를 공통 Min/Max 스케일로 그립니다.
    /// </summary>
    public class GoSparkline : GoControl
    {
        #region Properties
        /// <summary>그래프 배경 색상의 테마 색상 이름.</summary>
        [GoProperty(PCategory.Control, 0)] public string GraphColor { get; set; } = "Back";
        /// <summary>텍스트 색상의 테마 색상 이름.</summary>
        [GoProperty(PCategory.Control, 1)] public string TextColor { get; set; } = "Fore";

        /// <summary>글꼴 이름.</summary>
        [GoFontNameProperty(PCategory.Control, 2)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일.</summary>
        [GoProperty(PCategory.Control, 3)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기.</summary>
        [GoProperty(PCategory.Control, 4)] public float FontSize { get; set; } = 10;

        /// <summary>Y축 최소값 (모든 시리즈 공통).</summary>
        [GoProperty(PCategory.Control, 5)] public double Minimum { get; set; } = 0;
        /// <summary>Y축 최대값 (모든 시리즈 공통).</summary>
        [GoProperty(PCategory.Control, 6)] public double Maximum { get; set; } = 100;

        /// <summary>데이터를 보관할 최대 시간 범위.</summary>
        [GoProperty(PCategory.Control, 7)] public TimeSpan MaximumXScale { get; set; } = TimeSpan.FromMinutes(10);
        /// <summary>X축에 표시할 시간 범위.</summary>
        [GoProperty(PCategory.Control, 8)] public TimeSpan XScale { get; set; } = TimeSpan.FromMinutes(1);
        /// <summary>데이터 수집 간격(밀리초).</summary>
        [GoProperty(PCategory.Control, 9)] public int Interval { get; set; } = 1000;

        /// <summary>값 표시 형식 문자열.</summary>
        [GoMultiLineProperty(PCategory.Control, 10)] public string? ValueFormatString { get; set; } = null;

        /// <summary>라인 아래 영역을 시리즈 색상으로 채울지 여부.</summary>
        [GoProperty(PCategory.Control, 11)] public bool AreaDraw { get; set; } = false;
        /// <summary>라인 두께.</summary>
        [GoProperty(PCategory.Control, 12)] public float LineWidth { get; set; } = 1.5f;

        /// <summary>기준선 목록.</summary>
        [GoProperty(PCategory.Control, 13)] public List<GoBaseline> Baselines { get; set; } = [];
        /// <summary>그래프 시리즈 목록.</summary>
        [GoProperty(PCategory.Control, 14)] public List<GoLineGraphSeries> Series { get; set; } = [];

        /// <summary>트렌드 수집 실행 여부.</summary>
        [GoProperty(PCategory.Control, 15)] public bool IsStart { get; private set; } = false;

        #region Pause
        private bool bPause = false;
        /// <summary>데이터 수집 일시정지 여부.</summary>
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

        object? value = null;
        Dictionary<string, PropertyInfo> dicProps = new Dictionary<string, PropertyInfo>();
        object oLock = new object();

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Constructor
        /// <summary>GoSparkline 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoSparkline()
        {
            Selectable = false;
        }
        #endregion

        #region Override
        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            #region var
            var cGraph = thm.ToColor(GraphColor);
            var cText = thm.ToColor(TextColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtGraph = rts["Graph"];
            var now = nowTime;

            using var p = new SKPaint { IsAntialias = true };
            #endregion

            #region Background
            p.Color = cGraph; p.IsStroke = false;
            canvas.DrawRect(rtContent, p);
            #endregion

            #region Graph Lines
            if (datas.Count > 0 && Series.Count > 0)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(rtGraph);

                    var vst = now - XScale;
                    var ved = now;
                    var (si, ei) = FindRange(datas, vst, ved);
                    var va = datas.ToArray();

                    if (si >= 0 && si < va.Length && ei >= 0 && ei < va.Length)
                    {
                        foreach (var ser in Series)
                        {
                            var c = thm.ToColor(ser.Color);

                            #region build points
                            var pts = new List<SKPoint>();
                            for (int i = si; i <= ei; i++)
                            {
                                var itm = va[i];
                                if (!itm.Values.ContainsKey(ser.Name)) continue;

                                var x = Convert.ToSingle(MathTool.Map((itm.Time - vst).TotalMilliseconds,
                                                                       0, XScale.TotalMilliseconds,
                                                                       rtGraph.Left, rtGraph.Right));
                                var y = Convert.ToSingle(MathTool.Map(itm.Values[ser.Name],
                                                                       Minimum, Maximum,
                                                                       rtGraph.Bottom, rtGraph.Top));
                                pts.Add(new SKPoint(x, y));
                            }
                            #endregion

                            if (pts.Count >= 2)
                            {
                                #region Area fill
                                if (AreaDraw)
                                {
                                    using var path = new SKPath();
                                    path.MoveTo(pts[0].X, rtGraph.Bottom);
                                    foreach (var pt in pts) path.LineTo(pt);
                                    path.LineTo(pts[^1].X, rtGraph.Bottom);
                                    path.Close();

                                    p.IsStroke = false;
                                    p.Color = Util.FromArgb(70, c);
                                    canvas.DrawPath(path, p);
                                }
                                #endregion

                                #region Line
                                p.IsStroke = true;
                                p.StrokeWidth = LineWidth;
                                p.StrokeCap = SKStrokeCap.Round;
                                p.StrokeJoin = SKStrokeJoin.Round;
                                p.Color = c;
                                canvas.DrawPoints(SKPointMode.Polygon, pts.ToArray(), p);
                                #endregion
                            }
                            else if (pts.Count == 1)
                            {
                                p.IsStroke = false;
                                p.Color = c;
                                canvas.DrawCircle(pts[0], LineWidth + 0.5f, p);
                            }
                        }
                    }
                }
            }
            #endregion

            #region Baselines
            if (Baselines.Count > 0)
            {
                using var pe = SKPathEffect.CreateDash([3, 3], 2);

                foreach (var bl in Baselines)
                {
                    if (bl.Value < Minimum || bl.Value > Maximum) continue;

                    var c = thm.ToColor(bl.Color);
                    var y = Convert.ToSingle(MathTool.Map(bl.Value, Minimum, Maximum, rtGraph.Bottom, rtGraph.Top));

                    #region line
                    p.IsStroke = true;
                    p.StrokeWidth = 1F;
                    p.StrokeCap = SKStrokeCap.Butt;
                    p.Color = c;
                    p.PathEffect = pe;
                    p.IsAntialias = false;
                    canvas.DrawLine(rtGraph.Left, y, rtGraph.Right, y, p);
                    p.PathEffect = null;
                    p.IsAntialias = true;
                    #endregion

                    #region texts (좌측 이름, 우측 값)
                    var sName = bl.Name ?? "";
                    var sVal = ValueTool.ToString(bl.Value, ValueFormatString) ?? bl.Value.ToString();
                    var txtH = FontSize + 2;

                    if (!string.IsNullOrEmpty(sName))
                    {
                        var nw = Util.MeasureText(sName, FontName, FontStyle, FontSize).Width;
                        var rtN = Util.FromRect(rtGraph.Left + 4, y - txtH - 1, nw, txtH);
                        Util.DrawText(canvas, sName, FontName, FontStyle, FontSize, rtN, c, GoContentAlignment.MiddleLeft);
                    }

                    if (!string.IsNullOrEmpty(sVal))
                    {
                        var vw = Util.MeasureText(sVal, FontName, FontStyle, FontSize).Width;
                        var rtV = Util.FromRect(rtGraph.Right - vw - 4, y - txtH - 1, vw, txtH);
                        Util.DrawText(canvas, sVal, FontName, FontStyle, FontSize, rtV, c, GoContentAlignment.MiddleRight);
                    }
                    #endregion
                }
            }
            #endregion

            #region Corner texts (Min / Max / XScale)
            {
                var txtH = FontSize + 2;
                var sMax = ValueTool.ToString(Maximum, ValueFormatString) ?? Maximum.ToString();
                var sMin = ValueTool.ToString(Minimum, ValueFormatString) ?? Minimum.ToString();
                var sXSc = FormatXScale(XScale);

                var wMax = Util.MeasureText(sMax, FontName, FontStyle, FontSize).Width;
                var wMin = Util.MeasureText(sMin, FontName, FontStyle, FontSize).Width;
                var wXSc = Util.MeasureText(sXSc, FontName, FontStyle, FontSize).Width;

                // 좌상단: Max
                Util.DrawText(canvas, sMax, FontName, FontStyle, FontSize,
                              Util.FromRect(rtContent.Left + 4, rtContent.Top + 2, wMax, txtH),
                              cText, GoContentAlignment.MiddleLeft);

                // 좌하단: Min
                Util.DrawText(canvas, sMin, FontName, FontStyle, FontSize,
                              Util.FromRect(rtContent.Left + 4, rtContent.Bottom - txtH - 2, wMin, txtH),
                              cText, GoContentAlignment.MiddleLeft);

                // 우하단: XScale
                Util.DrawText(canvas, sXSc, FontName, FontStyle, FontSize,
                              Util.FromRect(rtContent.Right - wXSc - 4, rtContent.Bottom - txtH - 2, wXSc, txtH),
                              cText, GoContentAlignment.MiddleRight);
            }
            #endregion

            base.OnDraw(canvas, thm);
        }
        #endregion
        #endregion

        #region Areas
        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            // 코너 텍스트가 그래프를 가리지 않도록 상/하 패딩만 확보
            var pad = FontSize + 4;
            var rtGraph = new SKRect(
                rtContent.Left + 2,
                rtContent.Top + pad,
                rtContent.Right - 2,
                rtContent.Bottom - pad);

            dic["Graph"] = rtGraph;
            return dic;
        }
        #endregion

        #region Method
        #region Start
        /// <summary>트렌드 데이터 수집을 시작합니다.</summary>
        public void Start<T>(T value)
        {
            if (IsStart) throw new Exception("이미 실행 중 입니다");
            if (value == null) throw new Exception("Data가 Null 일 수 없습니다.");
            if (Series.Count == 0) throw new Exception("Series가 없습니다.");

            var props = typeof(T).GetProperties().Where(x =>
                x.PropertyType == typeof(double) || x.PropertyType == typeof(float) || x.PropertyType == typeof(decimal) ||
                x.PropertyType == typeof(byte) || x.PropertyType == typeof(sbyte) ||
                x.PropertyType == typeof(short) || x.PropertyType == typeof(ushort) ||
                x.PropertyType == typeof(int) || x.PropertyType == typeof(uint) ||
                x.PropertyType == typeof(long) || x.PropertyType == typeof(ulong)).ToList();

            var nmls = props.Select(x => x.Name).ToList();
            var nCnt = Series.Where(x => nmls.Contains(x.Name)).Count();
            if (nCnt != Series.Count) throw new Exception("잘못된 데이터 입니다.");

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
                try
                {
                    while (!token.IsCancellationRequested && IsStart)
                    {
                        AddData();
                        if (View) Invalidate();
                        await Task.Delay(Interval);
                    }
                }
                catch (OperationCanceledException) { }
                finally { IsStart = false; }
            }, cancel.Token);
        }
        #endregion
        #region Stop
        /// <summary>트렌드 데이터 수집을 중지합니다.</summary>
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
        /// <summary>데이터 소스 객체를 교체합니다.</summary>
        public void SetData<T>(T Data)
        {
            if (IsStart && this.value != null && this.value.GetType() == typeof(T))
                this.value = Data;
        }
        #endregion
        #region AddData
        void AddData()
        {
            if (value == null) return;
            lock (oLock)
            {
                var tgv = new GoTimeGraphValue() { Time = DateTime.Now };
                foreach (var vk in dicProps.Keys)
                    tgv.Values.Add(vk, Convert.ToDouble(dicProps[vk].GetValue(value)));

                if (Pause) pdatas.Add(tgv);
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
        #endregion
        #region FindRange
        (int startIdx, int endIdx) FindRange(List<GoTimeGraphValue> ls, DateTime startTime, DateTime endTime)
        {
            int left = 0, right = ls.Count - 1;
            int startIdx = -1, endIdx = -1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (ls[mid].Time >= startTime) { startIdx = mid; right = mid - 1; }
                else left = mid + 1;
            }

            left = 0; right = ls.Count - 1;
            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (ls[mid].Time <= endTime) { endIdx = mid; left = mid + 1; }
                else right = mid - 1;
            }

            return (startIdx, endIdx);
        }
        #endregion
        #region FormatXScale
        static string FormatXScale(TimeSpan ts)
        {
            if (ts.TotalHours >= 1 && ts.TotalHours == Math.Floor(ts.TotalHours)) return $"{(int)ts.TotalHours}h";
            if (ts.TotalMinutes >= 1 && ts.TotalMinutes == Math.Floor(ts.TotalMinutes)) return $"{(int)ts.TotalMinutes}m";
            if (ts.TotalSeconds >= 1) return $"{(int)ts.TotalSeconds}s";
            return $"{(int)ts.TotalMilliseconds}ms";
        }
        #endregion
        #endregion
    }

    /// <summary>GoSparkline의 기준선 정의.</summary>
    public class GoBaseline
    {
        /// <summary>기준선 왼쪽에 표시할 이름.</summary>
        [GoProperty(PCategory.Control, 0)] public string Name { get; set; } = "";
        /// <summary>기준선 값 (Y축 스케일 기준).</summary>
        [GoProperty(PCategory.Control, 1)] public double Value { get; set; } = 0;
        /// <summary>선 및 텍스트 색상의 테마 색상 이름.</summary>
        [GoProperty(PCategory.Control, 2)] public string Color { get; set; } = "Base3";

        /// <inheritdoc/>
        public override string ToString() => $"{Name}: {Value}";
    }
}
