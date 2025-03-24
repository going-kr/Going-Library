using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
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
    public class GoCircleGraph : GoControl
    {
        #region Properties
        public string GridColor { get; set; } = "Base3";
        public string TextColor { get; set; } = "Fore";
        public string RemarkColor { get; set; } = "Base2";

        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public bool ValueDraw { get; set; } = false;
        public List<GoGraphSeries> Series { get; set; } = [];
        #endregion

        #region Member Variable
        List<GoGraphValue> datas = new List<GoGraphValue>();
        float mx, my;
        int sel = -1;
        bool bDownL, bDownR;
        #endregion

        #region Constructor
        public GoCircleGraph()
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
            var cTextBorder = thm.Base1;
            var nTextBorder = 5F;
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
            if (sel >= 0 && sel < datas.Count)
            {
                var wh = Math.Min(rtGraph.Width, rtGraph.Height) - 20;
                var rtF = MathTool.MakeRectangle(rtGraph, new SKSize(wh, wh));
                var rtS = MathTool.MakeRectangle(rtGraph, new SKSize(wh * 0.5F, wh * 0.5F));
                var itm = datas[sel];

                using (new SKAutoCanvasRestore(canvas))
                {
                    using var fout = SKImageFilter.CreateDropShadow(2, 2, 2, 2, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));
                    var sum = itm.Values.Sum(x => x.Value);
                    var a = 0.0F;
                    var dicser = Series.ToDictionary(x => x.Name);
                    
                    foreach (var k in itm.Values.Keys)
                    {
                        #region var
                        var ser = dicser[k];
                        var v = itm.Values[k];
                        var vang = sum == 0 ? 0 : Convert.ToSingle(v / sum * 360.0);
                        var c = cSeries[ser];

                        var mp = new SKPoint(mx, my);
                        var cp = new SKPoint(rtF.MidX, rtF.MidY);
                        var ma = MathTool.GetAngle(cp, mp);
                        var bSel = CollisionTool.CheckCircle(rtF, mp) && MathTool.CompareAngle(ma, a, a + vang);

                        var rt = Util.FromRect(rtF);
                        var rtv = Util.FromRect(rtS);
                        var gp = Math.Abs(DateTime.Now.Millisecond - 500) / 50.0F;
                        var hp = MathTool.GetPointWithAngle(new SKPoint(0, 0), a + vang / 2f, bSel ? gp : 0);
                        var tpt = MathTool.GetPointWithAngle(cp, a + vang / 2f, wh * 0.75F / 2F);
                        tpt.Offset(hp);
                        rt.Offset(hp);
                        rtv.Offset(hp);
                        #endregion

                        #region path
                        using var path = new SKPath();
                        path.AddArc(rt, a, vang);
                        path.ArcTo(rtv, a + vang, -vang, false);
                        path.Close();
                        #endregion

                        #region fill
                        p.Color = c;
                        p.IsStroke = false;
                        if (bSel) p.ImageFilter = fout;
                        canvas.DrawPath(path, p);
                        p.ImageFilter = null;
                        #endregion

                        #region border
                        p.Color = c;// bSel ? c.BrightnessTransmit(thm.BorderBrightness) : c;
                        p.IsStroke = true;
                        p.StrokeWidth = 1;
                        canvas.DrawPath(path, p);
                        #endregion

                        #region Text
                        if(ValueDraw)
                        {
                            using (new SKAutoCanvasRestore(canvas))
                            {
                                canvas.ClipPath(path);
                                canvas.Translate(Util.Int(tpt));
                                canvas.RotateDegrees((a + vang / 2F) + 90);

                                var s = $"{v}";
                                Util.DrawText(canvas, s, FontName, FontStyle, FontSize, Util.FromRect(-100, -50, 200, 100), cText, cTextBorder, nTextBorder);
                            }
                        }
                        #endregion

                        #region Text2
                        if (bSel)
                        {
                            var s = $"{ser.Alias}\r\n{v}\r\n{v / sum:0.0%}";
                            Util.DrawText(canvas, s, FontName, FontStyle, FontSize, rtF, cText);
                        }
                        #endregion

                        a += vang;
                    }
                 
                }
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
             
            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
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

        protected override void OnMouseMove(float x, float y)
        {
            mx = x; my = y;
            base.OnMouseMove(x, y);
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
                dic["Remark"] = MathTool.MakeRectangle(rtRemark, new SKSize(rw, (box * rc) + (gap * (rc + 1))));
                dic["Category"] = rtsCat[1];
                dic["CategoryLeft"] = rtsCat[0];
                dic["CategoryRight"] = rtsCat[2];
            }

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region SetDataSource
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
