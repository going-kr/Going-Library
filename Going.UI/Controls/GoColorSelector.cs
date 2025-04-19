using Going.UI.Enums;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Controls
{
    public class GoColorSelector : GoControl
    {
        #region Const
        const int HueW = 30;
        const int DesH = 30;
        #endregion

        #region Properties
        [GoProperty(PCategory.Misc, 0)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Misc, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Misc, 2)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Misc, 3)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Misc, 4)] public string InputColor { get; set; } = "Base1";
        [GoProperty(PCategory.Misc, 5)] public string BorderColor { get; set; } = "Base3";

        [GoProperty(PCategory.Misc, 6)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        [JsonIgnore]
        [GoProperty(PCategory.Misc, 7)]
        public SKColor Value
        {
            get => Util.FromHsvDouble(nH, nS, nV);
            set
            {
                Util.ToHsvDouble(value, out nH, out nS, out  nV);

                var c = value;
                var c2 = Util.FromHsvDouble(nH, nS, nV);
                if(c.Red != c2.Red || c.Green != c2.Green || c.Blue != c2.Blue)
                {
                }
                HueSet();
            }
        }
        #endregion

        #region Member Variable
        byte[] baC = new byte[256 * 256 * 4];
        byte[] baH = new byte[256 * HueW * 4];

        double nH = 360, nS = 0, nV = 100;
        byte or, og, ob;
        bool bDownColor = false, bDownHue = false;
        bool bDownR = false, bDownG = false, bDownB = false;
        bool bValid = true;
        string? sInput = null;
        #endregion

        #region Constructor
        public GoColorSelector()
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
            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var cInput = thm.ToColor(InputColor);
            var cInputBorder = !bValid ? thm.Error : thm.Hignlight;

            var rts = Areas();
            var rtColor = rts["Color"];
            var rtHue = rts["Hue"];
            var rtSelect = rts["Select"];
            var rtTitleR = rts["TitleR"];
            var rtValueR = rts["ValueR"];
            var rtTitleG = rts["TitleG"];
            var rtValueG = rts["ValueG"];
            var rtTitleB = rts["TitleB"];
            var rtValueB = rts["ValueB"];
            #endregion

            if (rtColor.Width > 1)
            {
                #region Memory
                var wh = Convert.ToInt32(Math.Sqrt(baC.Length / 4));
                if (wh != rtColor.Height)
                {
                    wh = Convert.ToInt32(rtColor.Width);
                    baC = new byte[wh * wh * 4];
                    baH = new byte[wh * HueW * 4];

                    #region Color
                    var r = Parallel.For(0, wh * wh, (iv) =>
                    {
                        int y = iv / wh;
                        int x = iv - (y * wh);
                        int numBytes = (y * (wh * 4)) + (x * 4);

                        var s = MathTool.Map(x, 0.0, wh - 1, 0.0, 100.0);
                        var v = MathTool.Map(y, 0.0, wh - 1, 100.0, 0.0);
                        var c = Util.FromHsvDouble(nH, s, v);

                        baC[numBytes] = c.Red;
                        baC[numBytes + 1] = c.Green;
                        baC[numBytes + 2] = c.Blue;
                        baC[numBytes + 3] = c.Alpha;
                    });
                    #endregion

                    #region Hue
                    var r2 = Parallel.For(0, wh * HueW, (iv) =>
                    {
                        int y = iv / HueW;
                        int x = iv - (y * HueW);
                        int numBytes = (y * (HueW * 4)) + (x * 4);

                        var h = Convert.ToSingle(MathTool.Map(y, 0.0, wh, 360.0, 0.0));
                        var c = SKColor.FromHsv(h, 100, 100);

                        baH[numBytes] = c.Red;
                        baH[numBytes + 1] = c.Green;
                        baH[numBytes + 2] = c.Blue;
                        baH[numBytes + 3] = c.Alpha;
                    });
                    #endregion
                }
                #endregion

                #region Box & Text
                Util.DrawBox(canvas, rtSelect, Value, GoRoundType.All, thm.Corner);
                Util.DrawBox(canvas, rtValueR, cInput, cBorder, GoRoundType.All, thm.Corner);
                Util.DrawBox(canvas, rtValueG, cInput, cBorder, GoRoundType.All, thm.Corner);
                Util.DrawBox(canvas, rtValueB, cInput, cBorder, GoRoundType.All, thm.Corner);
                Util.DrawText(canvas, "R", FontName, FontStyle, FontSize, rtTitleR, cText);
                Util.DrawText(canvas, "G", FontName, FontStyle, FontSize, rtTitleG, cText);
                Util.DrawText(canvas, "B", FontName, FontStyle, FontSize, rtTitleB, cText);
                Util.DrawText(canvas, $"{Value.Red}", FontName, FontStyle, FontSize, rtValueR, cText);
                Util.DrawText(canvas, $"{Value.Green}", FontName, FontStyle, FontSize, rtValueG, cText);
                Util.DrawText(canvas, $"{Value.Blue}", FontName, FontStyle, FontSize, rtValueB, cText);
                #endregion

                #region Color
                #region Fill
                using (var bm = new SKBitmap(wh, wh, SKColorType.Rgba8888, SKAlphaType.Premul))
                {
                    var handle = GCHandle.Alloc(baC, GCHandleType.Pinned);
                    var info = new SKImageInfo(wh, wh, SKColorType.Rgba8888, SKAlphaType.Premul);
                    var map = new SKPixmap(info, handle.AddrOfPinnedObject());
                    bm.InstallPixels(map);
                    handle.Free();
                    map.Dispose();

                    canvas.DrawBitmap(bm, rtColor);
                }
                #endregion
                #region Cursor
                using (var p = new SKPaint() { IsAntialias = true })
                {
                    var x = Convert.ToInt32(MathTool.Map(nS, 0, 100, 0, wh - 1)) + rtColor.Left + 0.5F;
                    var y = Convert.ToInt32(MathTool.Map(nV, 0, 100, wh - 1, 0)) + rtColor.Top + 0.5F;

                    p.IsStroke = true;
                    p.StrokeWidth = 1;

                    p.Color = SKColors.Black;
                    canvas.DrawLine(x - 4 + 1, y + 1, x + 4 + 1, y + 1, p);
                    canvas.DrawLine(x + 1, y - 4 + 1, x + 1, y + 4 + 1, p);

                    p.Color = SKColors.White;
                    canvas.DrawLine(x - 4, y, x + 4, y, p);
                    canvas.DrawLine(x, y - 4, x, y + 4, p);
                }
                #endregion
                #endregion

                #region Hue
                #region Fill
                using (var bm = new SKBitmap(HueW, wh, SKColorType.Rgba8888, SKAlphaType.Premul))
                {
                    var handle = GCHandle.Alloc(baH, GCHandleType.Pinned);
                    var info = new SKImageInfo(HueW, wh, SKColorType.Rgba8888, SKAlphaType.Premul);
                    var map = new SKPixmap(info, handle.AddrOfPinnedObject());
                    bm.InstallPixels(map);
                    handle.Free();
                    map.Dispose();

                    canvas.DrawBitmap(bm, rtHue);
                }
                #endregion
                #region Cursor
                var hy = rtHue.Top + Convert.ToInt32(MathTool.Map(nH, 360, 0, 0, wh));
                using (var p = new SKPaint())
                {
                    p.IsStroke = true;

                    p.Color = SKColors.White;
                    canvas.DrawLine(rtHue.Left, hy, rtHue.Right, hy, p);

                    p.Color = SKColors.Black;
                    canvas.DrawLine(rtHue.Left, hy - 1, rtHue.Right, hy - 1, p);
                    canvas.DrawLine(rtHue.Left, hy + 1, rtHue.Right, hy + 1, p);
                }
                #endregion
                #endregion

                #region Input
                if (GoInputEventer.Current.InputControl == this)
                {
                    if (sInput == "R") Util.DrawBox(canvas, rtValueR, SKColors.Transparent, cInputBorder, GoRoundType.All, thm.Corner);
                    if (sInput == "G") Util.DrawBox(canvas, rtValueG, SKColors.Transparent, cInputBorder, GoRoundType.All, thm.Corner);
                    if (sInput == "B") Util.DrawBox(canvas, rtValueB, SKColors.Transparent, cInputBorder, GoRoundType.All, thm.Corner);
                }
                #endregion
            }

            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtColor = rts["Color"];
            var rtHue = rts["Hue"];
            var rtVR = rts["ValueR"];
            var rtVG = rts["ValueG"];
            var rtVB = rts["ValueB"];

            var wh = Convert.ToInt32(Math.Sqrt(baC.Length / 4));

            #region Color
            if (CollisionTool.Check(rtColor, x, y))
            {
                bDownColor = true;
                nS = Convert.ToSingle(MathTool.Map(MathTool.Constrain(x - rtColor.Left, 0, wh - 1), 0, wh - 1, 0, 100));
                nV = Convert.ToSingle(MathTool.Map(MathTool.Constrain(y - rtColor.Top, 0, wh - 1), 0, wh - 1, 100, 0));
            }
            #endregion

            #region Hue
            if (CollisionTool.Check(rtHue, x, y))
            {
                bDownHue = true;
                nH = Convert.ToSingle(MathTool.Map(MathTool.Constrain(y - rtHue.Top, 0, wh - 1), 0, wh - 1, 360, 0));
                HueSet();
            }
            #endregion

            #region Input
            if (CollisionTool.Check(rtVR, x, y)) bDownR = true;
            if (CollisionTool.Check(rtVG, x, y)) bDownG = true;
            if (CollisionTool.Check(rtVB, x, y)) bDownB = true;
            #endregion

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtColor = rts["Color"];
            var rtHue = rts["Hue"];
            var wh = Convert.ToInt32(Math.Sqrt(baC.Length / 4));

            #region Color
            if (bDownColor)
            {
                nS = Convert.ToSingle(MathTool.Map(MathTool.Constrain(x - rtColor.Left, 0, wh - 1), 0, wh - 1, 0, 100));
                nV = Convert.ToSingle(MathTool.Map(MathTool.Constrain(y - rtColor.Top, 0, wh - 1), 0, wh - 1, 100, 0));
            }
            #endregion

            #region Hue
            if (bDownHue)
            {
                nH = Convert.ToSingle(MathTool.Map(MathTool.Constrain(y - rtHue.Top, 0, wh - 1), 0, wh - 1, 360, 0));
                HueSet();
            }
            #endregion


            base.OnMouseMove(x, y);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtColor = rts["Color"];
            var rtHue = rts["Hue"];
            var rtVR = rts["ValueR"];
            var rtVG = rts["ValueG"];
            var rtVB = rts["ValueB"];

            var wh = Convert.ToInt32(Math.Sqrt(baC.Length / 4));

            #region Color
            if (bDownColor)
            {
                bDownColor = false;
                nS = Convert.ToSingle(MathTool.Map(MathTool.Constrain(x - rtColor.Left, 0, wh-1), 0, wh - 1, 0, 100));
                nV = Convert.ToSingle(MathTool.Map(MathTool.Constrain(y - rtColor.Top, 0, wh-1), 0, wh - 1, 100, 0));
            }
            #endregion

            #region Hue
            if (bDownHue)
            {
                bDownHue = false;
                nH = Convert.ToSingle(MathTool.Map(MathTool.Constrain(y - rtHue.Top, 0, wh - 1), 0, wh - 1, 360, 0));
                HueSet();
            }
            #endregion

            #region Input
            if (bDownR)
            {
                bDownR = false;
                if (CollisionTool.Check(rtVR, x, y))
                {
                    sInput = "R";
                    or = Value.Red;
                    og = Value.Green;
                    ob = Value.Blue;
                    GoInputEventer.Current.FireInputNumber(this, rtVR, (s) =>
                    {
                        var v = ValueTool.FromString<byte>(s);
                        if (v.HasValue) { bValid = true; Value = Util.FromArgb(v.Value, og , ob); }
                        else bValid = false;
                    }, Value.Red, 0, 255);
                }
            }
            if (bDownG)
            {
                bDownG = false;
                if (CollisionTool.Check(rtVG, x, y))
                {
                    sInput = "G";
                    or = Value.Red;
                    og = Value.Green;
                    ob = Value.Blue;
                    GoInputEventer.Current.FireInputNumber(this, rtVG, (s) =>
                    {
                        var v = ValueTool.FromString<byte>(s);
                        if (v.HasValue) { bValid = true; Value = Util.FromArgb(or, v.Value, ob); }
                        else bValid = false;
                    }, Value.Green, 0, 255);
                }
            }
            if (bDownB)
            {
                bDownB = false;
                if (CollisionTool.Check(rtVB, x, y))
                {
                    sInput = "B";
                    or = Value.Red;
                    og = Value.Green;
                    ob = Value.Blue;
                    GoInputEventer.Current.FireInputNumber(this, rtVB, (s) =>
                    {
                        var v = ValueTool.FromString<byte>(s);
                        if (v.HasValue) { bValid = true; Value = Util.FromArgb(or, og, v.Value); }
                        else bValid = false;
                    }, Value.Blue, 0, 255);
                }
            }
            #endregion
            base.OnMouseUp(x, y, button);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            var whC = Math.Min(rtContent.Width - 10 - HueW, rtContent.Height - 10 - DesH);

            var w = whC + 10 + HueW;
            var h = whC + 10 + DesH;

            var vrt = MathTool.MakeRectangle(rtContent, new SKSize(w, h), ContentAlignment);

            var rtColor = Util.FromRect(vrt.Left, vrt.Top, whC, whC);
            var rtHue = Util.FromRect(rtColor.Right + 10, 0, HueW, whC);
            var rtDesc = Util.FromRect(rtColor.Left, rtColor.Bottom + 10, rtHue.Right - rtColor.Left, DesH);
            var rtsDesc = Util.Columns(rtDesc, ["40px", "10px", "30px", "33.33%", "30px", "33.33%", "30px", "33.34%"]);
            
            dic["Color"] = rtColor;
            dic["Hue"] = rtHue;
            dic["Select"] = rtsDesc[0];
            dic["TitleR"] = rtsDesc[2];
            dic["ValueR"] = rtsDesc[3];
            dic["TitleG"] = rtsDesc[4];
            dic["ValueG"] = rtsDesc[5];
            dic["TitleB"] = rtsDesc[6];
            dic["ValueB"] = rtsDesc[7];

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region HueSet
        void HueSet()
        {
            var wh = Convert.ToInt32(Areas()["Color"].Width);
            if (wh > 1)
            {
                var r = Parallel.For(0, wh * wh, (iv) =>
                {
                    int y = iv / wh;
                    int x = iv - (y * wh);
                    int numBytes = (y * (wh * 4)) + (x * 4);

                    var s = MathTool.Map(x, 0.0, wh - 1, 0.0, 100.0);
                    var v = MathTool.Map(y, 0.0, wh - 1, 100.0, 0.0);
                    var c = Util.FromHsvDouble(nH, s, v);

                    baC[numBytes] = c.Red;
                    baC[numBytes + 1] = c.Green;
                    baC[numBytes + 2] = c.Blue;
                    baC[numBytes + 3] = c.Alpha;
                });
            }
        }
        #endregion
        #endregion
    }
}
