using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Controls
{
    /// <summary>
    /// 원형 그래프(도넛 차트) 컨트롤. 데이터를 원형 비율로 시각화합니다.
    /// </summary>
    public class GoCircleGraph : GoControl
    {
        #region Properties
        /// <summary>
        /// 그리드 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public string GridColor { get; set; } = "Base3";
        /// <summary>
        /// 텍스트 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 범례 배경 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string RemarkColor { get; set; } = "Base2";

        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;

        /// <summary>
        /// 그래프 시리즈 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Control, 6)] public List<GoGraphSeries> Series { get; set; } = [];
        #endregion

        #region Member Variable
        List<GoGraphValue> datas = new List<GoGraphValue>();
        float mx, my;
        int sel = -1;
        bool bDownL, bDownR;
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="GoCircleGraph"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoCircleGraph()
        {
            Selectable = true;
        }
        #endregion

        #region Override
        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            if (Design != null && Design.DesignMode) GenDesignSample();

            #region var
            var cGrid = thm.ToColor(GridColor);
            var cText = thm.ToColor(TextColor);
            var cRemark = thm.ToColor(RemarkColor);
            var cSeries = Series.ToDictionary(x => x, y => thm.ToColor(y.Color));
            var cSeries2 = Series.ToDictionary(x => x, y => string.IsNullOrEmpty(y.Color2) ? (SKColor?)null : thm.ToColor(y.Color2));
            var cTextBorder = thm.Back;
            var nTextBorder =3F;
            var rts = Areas();
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtCategory = rts["Category"];
            var rtBtnL = rts["CategoryLeft"];
            var rtBtnR = rts["CategoryRight"];
            using var p = new SKPaint { IsAntialias = true };

            var rc = Series.Where(x => x.Visible).Count();
            #endregion

            #region Graph

            // ── 새 디자인: 간격(gap) + 그림자 + radial 그라데이션 + hover pop + 중앙 라벨 ──
            if (sel >= 0 && sel < datas.Count)
            {
                var wh = Math.Min(rtGraph.Width, rtGraph.Height) - 40;
                var rtF = MathTool.MakeRectangle(rtGraph, new SKSize(wh, wh));
                var rtS = MathTool.MakeRectangle(rtGraph, new SKSize(wh * 0.5F, wh * 0.5F));
                var cen = new SKPoint(rtF.MidX, rtF.MidY);
                var rO = wh / 2F;
                var rI = wh * 0.25F;

                var itm = datas[sel];
                var sum = itm.Values.Sum(x => x.Value);
                var dicser = Series.ToDictionary(x => x.Name);

                const float gapDeg = 3F;
                const float popPx = 8F;

                using var fout = SKImageFilter.CreateDropShadow(0, 2, 5, 5, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));

                var mp = new SKPoint(mx, my);
                var ma = MathTool.GetAngle(cen, mp);
                var inRing = CollisionTool.CheckCircle(rtF, mp) && !CollisionTool.CheckCircle(rtS, mp);

                string? hovName = null;
                double hovVal = 0;

                var a = 0.0F;
                foreach (var k in itm.Values.Keys)
                {
                    var ser = dicser[k];
                    var v = itm.Values[k];
                    var vang = sum == 0 ? 0 : Convert.ToSingle(v / sum * 360.0);
                    if (vang <= 0.01F) { a += vang; continue; }
                    var c = cSeries[ser];
                    var c2 = cSeries2[ser];

                    var bSel = inRing && MathTool.CompareAngle(ma, a, a + vang);
                    if (bSel) { hovName = ser.Alias; hovVal = v; }

                    // 간격: sweep을 줄이고 가운데 정렬
                    var g = Math.Min(gapDeg, vang * 0.5F);
                    var sa = a + g / 2F;
                    var sw = vang - g;

                    // hover 시 바깥쪽으로 살짝 튀어나오게
                    var hp = bSel ? MathTool.GetPointWithAngle(new SKPoint(0, 0), a + vang / 2F, popPx) : new SKPoint(0, 0);
                    var rt = Util.FromRect(rtF); rt.Offset(hp);
                    var rtv = Util.FromRect(rtS); rtv.Offset(hp);

                    #region 도넛 조각
                    var o = new SKPoint(rt.MidX, rt.MidY);
                    var swRad = sw * (float)Math.PI / 180F;
                    var cr = Math.Min(7F, Math.Min((rO - rI) / 2F, rI * swRad / 2.5F));

                    using var path = new SKPath();
                    if (cr >= 1F) RoundedSegment(path, o, rI, rO, sa, sw, cr);
                    else
                    {
                        path.AddArc(rt, sa, sw);
                        path.ArcTo(rtv, sa + sw, -sw, false);
                        path.Close();
                    }

                    p.IsStroke = false;
                    p.ImageFilter = fout;
                    SKShader? sh = null;
                    var cc = bSel ? c.BrightnessTransmit(thm.HoverFillBrightness) : c;
                    if (c2.HasValue && c != c2.Value)
                    {
                        var cc2 = bSel ? c2.Value.BrightnessTransmit(thm.HoverFillBrightness) : c2.Value;
                        var ccen = new SKPoint(rt.MidX, rt.MidY);
                        sh = SKShader.CreateRadialGradient(ccen, rt.Width / 2F, new[] { cc2, cc },
                            new[] { rt.Width > 0 ? rtv.Width / rt.Width : 0F, 1F }, SKShaderTileMode.Clamp);
                        p.Shader = sh;
                        p.Color = SKColors.White;
                    }
                    else p.Color = cc;
                    canvas.DrawPath(path, p);
                    p.Shader = null;
                    p.ImageFilter = null;
                    sh?.Dispose();
                    #endregion

                    #region 조각 라벨
                    if (sw >= 12)
                    {
                        var lp = MathTool.GetPointWithAngle(cen, a + vang / 2F, (rO + rI) / 2F);
                        lp.Offset(hp);
                        Util.DrawText(canvas, ser.Alias, FontName, FontStyle, FontSize, Util.FromRect(lp.X - 50, lp.Y - 12, 100, 24), cText, c.BrightnessChange(-0.7F), nTextBorder);
                    }
                    #endregion

                    a += vang;
                }

                #region 중앙 라벨
                {
                    var cs = hovName != null
                        ? $"{hovName}\r\n{hovVal:0.##}\r\n{(sum == 0 ? 0 : hovVal / sum):0.0%}"
                        : $"{datas[sel].Name}\r\n{sum:0.##}";
                    var rin = MathTool.MakeRectangle(rtGraph, new SKSize(rI * 2, rI * 2));
                    Util.DrawText(canvas, cs, FontName, FontStyle, FontSize, rin, cText);
                }
                #endregion
            }
            #endregion

            #region Category
            {
                var cL = CollisionTool.Check(rtBtnL, mx, my) ? cText : Util.FromArgb(90, cText);
                var cR = CollisionTool.Check(rtBtnR, mx, my) ? cText : Util.FromArgb(90, cText);
                if (bDownL) rtBtnL.Offset(0, 1);
                if (bDownR) rtBtnR.Offset(0, 1);
                Util.DrawIcon(canvas, "fa-chevron-left", 14, rtBtnL, cL.BrightnessTransmit(bDownL ? thm.DownBrightness : 0));
                Util.DrawIcon(canvas, "fa-chevron-right", 14, rtBtnR, cR.BrightnessTransmit(bDownR ? thm.DownBrightness : 0));
                if (sel >= 0 && sel < datas.Count) Util.DrawText(canvas, datas[sel].Name, FontName, FontStyle, FontSize, rtCategory, cText);
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
             
            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtCategory = rts["Category"];
            var rtBtnL = rts["CategoryLeft"];
            var rtBtnR = rts["CategoryRight"];
            #endregion

            if (CollisionTool.Check(rtBtnL, x, y)) bDownL = true;
            if (CollisionTool.Check(rtBtnR, x, y)) bDownR = true;
            base.OnMouseDown(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            #region var
            var rts = Areas();
            var rtRemark = rts["Remark"];
            var rtGraph = rts["Graph"];
            var rtCategory = rts["Category"];
            var rtBtnL = rts["CategoryLeft"];
            var rtBtnR = rts["CategoryRight"];
            #endregion

            if (bDownL) { bDownL = false; if (CollisionTool.Check(rtBtnL, x, y)) sel = MathTool.Constrain(sel - 1, 0, datas.Count - 1); }
            if (bDownR) { bDownR = false; if (CollisionTool.Check(rtBtnR, x, y)) sel = MathTool.Constrain(sel + 1, 0, datas.Count - 1); }
            base.OnMouseUp(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            mx = x; my = y;
            base.OnMouseMove(x, y);
        }
        #endregion

        #region Areas
        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            {
                var box = FontSize + 2;
                var gap = 10;

                var csw = datas.Select(x => Util.MeasureText(x.Name, FontName, FontStyle, FontSize).Width );
                var rsw = Series.Select(x => Util.MeasureText(x.Alias, FontName, FontStyle, FontSize).Width + gap + box);
                var rw = rsw.Any() ? rsw.Max() + 20 : 20;
                var cw = csw.Any() ? csw.Max() + 20 : 20;
                var rc = Math.Max(1, Series.Where(x => x.Visible).Count());
                var rts = Util.Grid(rtContent, [$"100%", "10px", $"{rw}px"], ["20px", "10px", "100%", "10px", "40px", $"{Scroll.SC_WH}px"]);

                var rtGraph = Util.Merge(rts, 0, 2, 1, 1);
                var rtRemark = Util.Merge(rts, 2, 2, 1, 1);

                var rtsCat = Util.Columns(MathTool.MakeRectangle(Util.Merge(rts, 0, 4, 1, 1), new SKSize(cw + 80, 40)), ["40px", "100%", "40px"]);
                var rtCL = rtsCat[0];
                var rtCT = rtsCat[1];
                var rtCR = rtsCat[2];

                dic["Graph"] = rtGraph;
                dic["Remark"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, (box * rc) + (gap * (rc + 1))), GoContentAlignment.MiddleCenter);
                dic["Category"] = rtsCat[1];
                dic["CategoryLeft"] = rtsCat[0];
                dic["CategoryRight"] = rtsCat[2];
            }

            return dic;
        }
        #endregion
        #endregion

        #region Method
        /// <summary>중심 <paramref name="o"/> 기준 반지름 <paramref name="r"/>, 각도 <paramref name="deg"/>(도) 위치의 점.</summary>
        static SKPoint PolarPt(SKPoint o, float r, float deg)
        {
            var rad = deg * (float)Math.PI / 180F;
            return new SKPoint(o.X + r * (float)Math.Cos(rad), o.Y + r * (float)Math.Sin(rad));
        }

        /// <summary>모서리가 둥근 도넛 세그먼트 path를 만듭니다. 바깥/안쪽 호와 두 방사형 변을 모서리 반경 <paramref name="cr"/>로 둥글게 잇습니다.</summary>
        static void RoundedSegment(SKPath path, SKPoint o, float rI, float rO, float startDeg, float sweepDeg, float cr)
        {
            var rtO = new SKRect(o.X - rO, o.Y - rO, o.X + rO, o.Y + rO);
            var rtI = new SKRect(o.X - rI, o.Y - rI, o.X + rI, o.Y + rI);
            var endDeg = startDeg + sweepDeg;
            var daO = cr / rO * 180F / (float)Math.PI;
            var daI = cr / rI * 180F / (float)Math.PI;

            var po1 = PolarPt(o, rO, startDeg + daO);
            path.MoveTo(po1);
            path.ArcTo(rtO, startDeg + daO, sweepDeg - 2 * daO, false);                  // 바깥 호
            path.QuadTo(PolarPt(o, rO, endDeg), PolarPt(o, rO - cr, endDeg));            // 끝 바깥 모서리
            path.LineTo(PolarPt(o, rI + cr, endDeg));                                    // 끝 방사형 변
            path.QuadTo(PolarPt(o, rI, endDeg), PolarPt(o, rI, endDeg - daI));           // 끝 안쪽 모서리
            path.ArcTo(rtI, endDeg - daI, -(sweepDeg - 2 * daI), false);                 // 안쪽 호 (역방향)
            path.QuadTo(PolarPt(o, rI, startDeg), PolarPt(o, rI + cr, startDeg));        // 시작 안쪽 모서리
            path.LineTo(PolarPt(o, rO - cr, startDeg));                                  // 시작 방사형 변
            path.QuadTo(PolarPt(o, rO, startDeg), po1);                                  // 시작 바깥 모서리
            path.Close();
        }

        /// <summary>디자인 타임 전용. 설정된 <see cref="Series"/>에 맞춰 결정적 미리보기 데이터를 생성합니다. 시리즈가 없으면 표시하지 않습니다.</summary>
        void GenDesignSample()
        {
            datas.Clear();
            if (Series.Count == 0) return;

            const int cats = 5;
            for (int i = 0; i < cats; i++)
            {
                var gv = new GoGraphValue { Name = $"{i + 1:00}" };
                for (int s = 0; s < Series.Count; s++)
                {
                    var ser = Series[s];
                    if (string.IsNullOrEmpty(ser.Name) || gv.Values.ContainsKey(ser.Name)) continue;
                    var w = (Math.Sin((i * 0.9) + (s * 1.3)) + 1) / 2.0;   // 0..1 결정적
                    gv.Values[ser.Name] = 15 + 70 * w;   // 0~100 범위
                }
                datas.Add(gv);
            }
            if (sel < 0 || sel >= datas.Count) sel = 0;
        }

        #region SetDataSource
        /// <summary>
        /// 데이터 소스를 설정합니다.
        /// </summary>
        /// <typeparam name="T">데이터 항목의 타입</typeparam>
        /// <param name="CategoryAxisName">카테고리 축으로 사용할 string 타입 속성 이름</param>
        /// <param name="values">데이터 컬렉션</param>
        public void SetDataSource<T>(string CategoryAxisName, IEnumerable<T> values)
        {
            if (Series.Count > 0)
            {
                var pls = typeof(T).GetProperties();

                var xprop = pls.FirstOrDefault(x => x.Name == CategoryAxisName);
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
                        var vsel = -1;
                        foreach (var v in values)
                        {
                            var nm = xprop.GetValue(v) as string;
                            if (!string.IsNullOrWhiteSpace(nm))
                            {
                                var gv = new GoGraphValue() { Name = nm,  };

                                if (gv.Name != null)
                                {
                                    foreach (var prop in props)
                                    {
                                        var val = Convert.ToDouble(prop.GetValue(v));
                                        gv.Values.Add(prop.Name, val);
                                    }

                                    datas.Add(gv);
                                    vsel = 0;
                                }
                            }
                        }

                        sel = vsel;
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
