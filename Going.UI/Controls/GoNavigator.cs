using Going.UI.Collections;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
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
    public class GoNavigator : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 1)] public GoDirectionHV IconDirection { get; set; }
        [GoProperty(PCategory.Control, 2)] public float IconGap { get; set; } = 5;
        [GoFontNameProperty(PCategory.Control, 3)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 4)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 5)] public float FontSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 6)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 7)] public float Indent { get; set; } = 20;
        [GoProperty(PCategory.Control, 8)] public float MenuGap { get; set; } = 30F;

        [GoProperty(PCategory.Control, 9)] public ObservableList<GoMenuItem> Menus { get; set; } = [];

        [GoProperty(PCategory.Control, 10)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        #endregion

        #region Member Variable
        SKRect rtBoxP = new SKRect();
        int si = 0;
        float mx = -1, my = -1;
        bool bPrevDown = false, bNextDown = false;
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtPrev = rts["Prev"];
            var rtNext = rts["Next"];
            var cText = thm.ToColor(TextColor);

            using var p = new SKPaint { IsAntialias = true };
            using var pe = SKPathEffect.CreateDash([2, 2], 2);

            #region Horizeon
            if (Direction == GoDirectionHV.Horizon)
            {
                var ls = itemsH(rtBox);
                var cs = ls.FirstOrDefault();
                var ce = ls.LastOrDefault();

                #region Prev / Next
                if (Menus.Count > 0 && Menus.Last().Bounds.Right > rtBox.Width)
                {
                    if (bPrevDown) rtPrev.Offset(0, 1);
                    if (bNextDown) rtNext.Offset(0, 1);

                    Util.DrawIcon(canvas, "fa-angle-left", IconSize, rtPrev, cText.BrightnessTransmit(bPrevDown ? thm.DownBrightness : 0).WithAlpha(Convert.ToByte(CollisionTool.Check(rtPrev, mx, my) ? 255 : 60)));
                    Util.DrawIcon(canvas, "fa-angle-right", IconSize, rtNext, cText.BrightnessTransmit(bNextDown ? thm.DownBrightness : 0).WithAlpha(Convert.ToByte(CollisionTool.Check(rtNext, mx, my) ? 255 : 60)));
                }
                #endregion

                #region Items
                if (ls.Count > 0)
                {
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(rtBox);
                        canvas.Translate(rtBox.Left, 0);
                        canvas.Translate(si >= 0 ? -Menus[si].Bounds.Left : 0, 0);

                        foreach (var v in ls)
                        {
                            var tmx = mx;
                            tmx -= rtBox.Left;
                            tmx -= si >= 0 ? -Menus[si].Bounds.Left : 0;

                            var rt = v.Bounds; rt.Offset(-MenuGap / 2, 0);
                            var c = cText.WithAlpha(Convert.ToByte(!string.IsNullOrWhiteSpace(v.PageName) && Design?.CurrentPage?.Name == v.PageName ? 255 : (CollisionTool.Check(rt, tmx, my) ? 150 : 60)));
                            Util.DrawTextIcon(canvas, v.Text, FontName, FontStyle, FontSize, v.IconString, IconSize, IconDirection, IconGap, v.Bounds, c, GoContentAlignment.MiddleLeft);

                            if (v != cs)
                            {
                                var vx = v.Bounds.Left - MenuGap / 2;
                                p.IsAntialias = false;
                                p.IsStroke = true;
                                p.StrokeWidth = 1;
                                p.PathEffect = pe;
                                p.Color = cText.WithAlpha(60);
                                canvas.DrawLine(vx, rtBox.MidY - FontSize / 2, vx, rtBox.MidY + FontSize / 2, p);
                                p.PathEffect = null;
                                p.IsAntialias = true;
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion
            #region Vertical
            else if(Direction == GoDirectionHV.Vertical)
            {
                var ls = itemsV(rtBox);
                var cs = ls.FirstOrDefault();
                var ce = ls.LastOrDefault();

                #region Prev / Next
                if (Menus.Count > 0 && Menus.Last().Bounds.Bottom > rtBox.Height)
                {
                    if (bPrevDown) rtPrev.Offset(0, 1);
                    if (bNextDown) rtNext.Offset(0, 1);

                    Util.DrawIcon(canvas, "fa-angle-up", IconSize, rtPrev, cText.BrightnessTransmit(bPrevDown ? thm.DownBrightness : 0).WithAlpha(Convert.ToByte(CollisionTool.Check(rtPrev, mx, my) ? 255 : 60)));
                    Util.DrawIcon(canvas, "fa-angle-down", IconSize, rtNext, cText.BrightnessTransmit(bNextDown ? thm.DownBrightness : 0).WithAlpha(Convert.ToByte(CollisionTool.Check(rtNext, mx, my) ? 255 : 60)));
                }
                #endregion

                #region Items
                if (ls.Count > 0)
                {
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(rtBox);
                        canvas.Translate(0, rtBox.Top);
                        canvas.Translate(0, si >= 0 ? -Menus[si].Bounds.Top : 0);

                        foreach (var v in ls)
                        {
                            var tmy = my;
                            tmy -= rtBox.Top;
                            tmy -= si >= 0 ? -Menus[si].Bounds.Top : 0;

                            var rt = v.Bounds; rt.Offset(0, -MenuGap / 2);
                            var c = cText.WithAlpha(Convert.ToByte(!string.IsNullOrWhiteSpace(v.PageName) && Design?.CurrentPage?.Name == v.PageName ? 255 : (CollisionTool.Check(rt, mx, tmy) ? 150 : 60)));
                            Util.DrawTextIcon(canvas, v.Text, FontName, FontStyle, FontSize, v.IconString, IconSize, IconDirection, IconGap, v.Bounds, c, GoContentAlignment.TopCenter);
                        }
                    }
                }
                #endregion
            }
            #endregion

            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtPrev = rts["Prev"];
            var rtNext = rts["Next"];

            #region Horizon
            if (Direction == GoDirectionHV.Horizon)
            {
                var ls = itemsH(rtBox);
                var cs = ls.FirstOrDefault();
                var ce = ls.LastOrDefault();

                #region Prev / Next
                if (Menus.Count > 0 && Menus.Last().Bounds.Right > rtBox.Width)
                {
                    if (CollisionTool.Check(rtPrev, x, y)) bPrevDown = true;
                    if (CollisionTool.Check(rtNext, x, y)) bNextDown = true;
                }
                #endregion

                #region Items
                var tmx = x;
                tmx -= rtBox.Left;
                tmx -= si >= 0 ? -Menus[si].Bounds.Left : 0;
                foreach (var v in ls)
                {
                    var rt = v.Bounds; rt.Offset(-MenuGap / 2, 0);
                    if (CollisionTool.Check(rt, tmx, y)) Design?.SetPage(v.PageName ?? "");
                }
                #endregion
            }
            #endregion
            #region Vertical
            else if (Direction == GoDirectionHV.Vertical)
            {
                var ls = itemsV(rtBox);
                var cs = ls.FirstOrDefault();
                var ce = ls.LastOrDefault();

                #region Prev / Next
                if (Menus.Count > 0 && Menus.Last().Bounds.Bottom > rtBox.Height)
                {
                    if (CollisionTool.Check(rtPrev, x, y)) bPrevDown = true;
                    if (CollisionTool.Check(rtNext, x, y)) bNextDown = true;
                }
                #endregion

                #region Items
                var tmy = y;
                tmy -= rtBox.Top;
                tmy -= si >= 0 ? -Menus[si].Bounds.Top : 0;
                foreach (var v in ls)
                {
                    var rt = v.Bounds; rt.Offset(0, -MenuGap / 2);
                    if (CollisionTool.Check(rt, x, tmy)) Design?.SetPage(v.PageName ?? "");
                }
                #endregion
            }
            #endregion

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtPrev = rts["Prev"];
            var rtNext = rts["Next"];

            #region Horizon
            if (Direction == GoDirectionHV.Horizon)
            {
                var ls = itemsH(rtBox);
                var cs = ls.FirstOrDefault();
                var ce = ls.LastOrDefault();

                #region Prev / Next
                if (bPrevDown)
                {
                    bPrevDown = false;
                    if (CollisionTool.Check(rtPrev, x, y)) si = Math.Max(0, si - 1);
                }

                if(bNextDown)
                {
                    bNextDown = false;
                    if (CollisionTool.Check(rtNext, x, y))
                    {
                        if (si >= 0 && Menus.Count > 0 && ls.Count > 0)
                        {
                            if (ls.Last() != Menus.Last()) si = Math.Min(Menus.Count - 1, si + 1);
                        }
                    }
                }
                #endregion
            }
            #endregion
            #region Vertical
            else if (Direction == GoDirectionHV.Vertical)
            {
                var ls = itemsV(rtBox);
                var cs = ls.FirstOrDefault();
                var ce = ls.LastOrDefault();

                #region Prev / Next
                if (bPrevDown)
                {
                    bPrevDown = false;
                    if (CollisionTool.Check(rtPrev, x, y)) si = Math.Max(0, si - 1);
                }

                if (bNextDown)
                {
                    bNextDown = false;
                    if (CollisionTool.Check(rtNext, x, y))
                    {
                        if (si >= 0 && Menus.Count > 0 && ls.Count > 0)
                        {
                            if (ls.Last() != Menus.Last()) si = Math.Min(Menus.Count - 1, si + 1);
                        }
                    }
                }
                #endregion
            }
            #endregion

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y) { mx = x; my = y; base.OnMouseMove(x, y); }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];
            
            if (Direction == GoDirectionHV.Horizon)
            {
                var rts = Util.Columns(rtContent, [$"{Indent}px", "100%", "40px", "40px"]);
                dic["Box"] = rts[1];
                dic["Prev"] = rts[2];
                dic["Next"] = rts[3];
            }
            else
            {
                var rts = Util.Rows(rtContent, [$"{Indent}px", "100%", "40px", "40px"]);
                dic["Box"] = rts[1];
                dic["Prev"] = rts[2];
                dic["Next"] = rts[3];
            }

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region itemsH
        List<GoMenuItem> itemsH(SKRect rtBox)
        {
            List<GoMenuItem> ret = [];

            #region calcbox
            if (Menus.Changed || !rtBoxP.Equals(rtBox))
            {
                rtBoxP = rtBox;

                var x = 0F;
                foreach (var v in Menus)
                {
                    var sz = Util.MeasureTextIcon(v.Text, FontName, FontStyle, FontSize, v.IconString, IconSize, IconDirection, IconGap);
                    var w = sz.Width + MenuGap;
                    v.Bounds = Util.FromRect(x, rtBox.Top, w, rtBox.Height);
                    x += w;
                }

                Menus.Changed = false;
            }
            #endregion

            if (si >= 0 && si < Menus.Count)
            {
                if (Menus.Count > 0 && Menus.Last().Bounds.Right < rtBox.Width) si = 0;

                var sx = Menus[si].Bounds.Left;
                var ex = sx + rtBox.Width;
                var ls = Menus.Where(x => x.Bounds.Left >= sx && x.Bounds.Right < ex).ToList();
                if (ls.Count == 0) ls.Add(Menus[si]);
                ret = ls;
            }

            return ret;
        }
        #endregion
        #region itemsV
        List<GoMenuItem> itemsV(SKRect rtBox)
        {
            List<GoMenuItem> ret = [];

            #region calcbox
            if (Menus.Changed || !rtBoxP.Equals(rtBox))
            {
                rtBoxP = rtBox;

                var y = 0F;
                foreach (var v in Menus)
                {
                    var sz = Util.MeasureTextIcon(v.Text, FontName, FontStyle, FontSize, v.IconString, IconSize, IconDirection, IconGap);
                    var h = sz.Height + MenuGap;
                    v.Bounds = Util.FromRect(rtBox.Left, y, rtBox.Width, h);
                    y += h;
                }

                Menus.Changed = false;
            }
            #endregion

            if (si >= 0 && si < Menus.Count)
            {
                if (Menus.Count > 0 && Menus.Last().Bounds.Bottom < rtBox.Width) si = 0;

                var sy = Menus[si].Bounds.Top;
                var ey = sy + rtBox.Height;
                var ls = Menus.Where(x => x.Bounds.Top >= sy && x.Bounds.Bottom < ey).ToList();
                if (ls.Count == 0) ls.Add(Menus[si]);
                ret = ls;
            }

            return ret;
        }
        #endregion
        #endregion
    }
}
