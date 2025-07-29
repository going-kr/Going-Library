using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Icons;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoSwitch : GoControl
    {
        #region Const
        const float SO = 0.075F;
        const float EO = 1F - SO;
        #endregion

        #region Properties
        [GoProperty(PCategory.Control, 0)] public string OnText { get; set; } = "On";
        [GoProperty(PCategory.Control, 1)] public string OffText { get; set; } = "Off";
        [GoProperty(PCategory.Control, 2)] public string? OnIconString { get; set; }
        [GoProperty(PCategory.Control, 3)] public string? OffIconString { get; set; }
        [GoFontNameProperty(PCategory.Control, 4)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 5)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 6)] public float FontSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 7)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 8)] public GoDirectionHV IconDirection { get; set; }
        [GoProperty(PCategory.Control,10)] public float IconGap { get; set; } = 5;

        [GoProperty(PCategory.Control,11)] public string OnTextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control,12)] public string OffTextColor { get; set; } = "Base5";
        [GoProperty(PCategory.Control,13)] public string BoxColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control,14)] public string BorderColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control,15)] public string SwitchColor { get; set; } = "Base3";

        [GoProperty(PCategory.Control,16)] public string OnIconColor { get; set; } = "lime";
        [GoProperty(PCategory.Control,17)] public string OffIconColor { get; set; } = "red";

        private bool bOnOff = false;
        [GoProperty(PCategory.Control,18)]
        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value)
                {
                    ani.Stop();
                    ani.Start(Animation.Time200, value ? "ON" : "OFF", () =>
                    {
                        bOnOff = value;
                        OnOffChanged?.Invoke(this, EventArgs.Empty);
                    });
                }
            }
        }
        #endregion

        #region Member Variable
        private bool bHover = false;
        private bool bDown = false;

        private Animation ani = new Animation();
        private bool downOnOff = false;
        #endregion

        #region Event
        public event EventHandler? OnOffChanged;
        #endregion

        #region Constructor
        public GoSwitch()
        {
            Selectable = true;
            ani.Refresh = () => Invalidate?.Invoke();
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cOnText = thm.ToColor(OnTextColor);
            var cOffText = thm.ToColor(OffTextColor);
            var cBox = thm.ToColor(BoxColor);
            var cBorder = thm.ToColor(BorderColor);
            var cSwitch = thm.ToColor(SwitchColor);
            var cOn = thm.ToColor(OnIconColor);
            var cOff = thm.ToColor(OffIconColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtSwitch = Util.Int(rts["Switch"]); rtSwitch.Offset(0.5F, 0.5F);
            var rtSW = new SKRoundRect(rtSwitch, thm.Corner);
            var rtOnText = rts["OnText"];
            var rtOffText = rts["OffText"];

            using var p = new SKPaint { IsAntialias = true };

            Util.DrawBox(canvas, rtContent, cBox, cBorder, GoRoundType.All, thm.Corner);

            #region Switch
            #region Color
            SKColor c1, c2, c3, c4;
            GetSwitchColors(cSwitch.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), out c1, out c2, out c3, out c4);

            var ca1 = !OnOff ? c2 : c4;
            var ca2 = !OnOff ? c3 : c1;
            var ca3 = !OnOff ? c1 : c3;
            var ca4 = !OnOff ? c4 : c2;

            var va1 = !OnOff ? 0 : 0;
            var va2 = !OnOff ? 0.5F : SO;
            var va3 = !OnOff ? EO : 0.5F;
            var va4 = !OnOff ? 1 : 1;

            if (thm.Animation && ani.IsPlaying)
            {
                if (ani.Variable == "ON")
                {
                    ca1 = ani.Value(AnimationAccel.DCL, c2, c4);
                    ca2 = ani.Value(AnimationAccel.DCL, c3, c1);
                    ca3 = ani.Value(AnimationAccel.DCL, c1, c3);
                    ca4 = ani.Value(AnimationAccel.DCL, c4, c2);

                    va2 = ani.Value(AnimationAccel.DCL, 0.5F, SO);
                    va3 = ani.Value(AnimationAccel.DCL, EO, 0.5F);
                }
                else if (ani.Variable == "OFF")
                {
                    ca1 = ani.Value(AnimationAccel.DCL, c4, c2);
                    ca2 = ani.Value(AnimationAccel.DCL, c1, c3);
                    ca3 = ani.Value(AnimationAccel.DCL, c3, c1);
                    ca4 = ani.Value(AnimationAccel.DCL, c2, c4);

                    va2 = ani.Value(AnimationAccel.DCL, SO, 0.5F);
                    va3 = ani.Value(AnimationAccel.DCL, 0.5F, EO);
                }
            }
            #endregion

            #region Fill
            using (var lg = SKShader.CreateLinearGradient(new SKPoint(rtSwitch.Left, rtSwitch.Top), new SKPoint(rtSwitch.Right, rtSwitch.Top),
                                                        [ca1, ca2, ca3, ca4], [va1, va2, va3, va4], SKShaderTileMode.Clamp))
            {
                p.Shader = lg;
                p.IsStroke = false;
                canvas.DrawRoundRect(rtSW, p);
                p.Shader = null;
            }
            #endregion

            #region Border
            {
                var hb = 0.25F;
                var gp = rtSwitch.Width * SO - (thm.Corner / 2F);
                var rtvon = Util.FromRect(rtSwitch.Left + gp, rtSwitch.Top, rtSwitch.Width - gp, rtSwitch.Height);
                var rtvoff = Util.FromRect(rtSwitch.Left, rtSwitch.Top, rtSwitch.Width - gp, rtSwitch.Height);
                if (thm.Animation && ani.IsPlaying)
                {
                    using (var lg = SKShader.CreateLinearGradient(new SKPoint(rtSwitch.Left, rtSwitch.Top), new SKPoint(rtSwitch.Right, rtSwitch.Top),
                                                           [ca1.BrightnessTransmit(hb), ca2.BrightnessTransmit(hb), ca3.BrightnessTransmit(hb), ca4.BrightnessTransmit(hb)], [va1, va2, va3, va4], SKShaderTileMode.Clamp))
                    {
                        var rtv = ani.Variable == "ON" ? ani.Value(AnimationAccel.DCL, rtvoff, rtvon) : ani.Value(AnimationAccel.DCL, rtvon, rtvoff);
                        p.Shader = lg;
                        p.IsStroke = true;
                        p.StrokeWidth = 1;
                        canvas.DrawRoundRect(new SKRoundRect(rtv, thm.Corner), p);
                        p.Shader = null;
                    }
                }
                else
                {
                    using (var lg = SKShader.CreateLinearGradient(new SKPoint(rtSwitch.Left, rtSwitch.Top), new SKPoint(rtSwitch.Right, rtSwitch.Top),
                                                           [ca1.BrightnessTransmit(hb), ca2.BrightnessTransmit(hb), ca3.BrightnessTransmit(hb), ca4.BrightnessTransmit(hb)], [va1, va2, va3, va4], SKShaderTileMode.Clamp))
                    {
                        var rtv = OnOff ? rtvon : rtvoff;
                        p.Shader = lg;
                        p.IsStroke = true;
                        p.StrokeWidth = 1;
                        canvas.DrawRoundRect(new SKRoundRect(rtv, thm.Corner), p);
                        p.Shader = null;
                    }
                }

                p.IsStroke = true;
                p.StrokeWidth = 0.5F;
                p.Color = Util.FromArgb(30, SKColors.Black);
                canvas.DrawLine(rtSwitch.MidX, rtSwitch.Top + 1, rtSwitch.MidX, rtSwitch.Bottom - 1, p);
            }
            #endregion
            #endregion

            #region Text
            var cTL = cOnText;
            var cTD = cOffText;

            if (thm.Animation && ani.IsPlaying)
            {
                var cton = ani.Variable == "ON" ? ani.Value(AnimationAccel.DCL, cTD, cTL) : ani.Value(AnimationAccel.DCL, cTL, cTD);
                var ctoff = ani.Variable == "ON" ? ani.Value(AnimationAccel.DCL, cTL, cTD) : ani.Value(AnimationAccel.DCL, cTD, cTL);
                var cion = ani.Variable == "ON" ? ani.Value(AnimationAccel.DCL, cTD, cOn) : ani.Value(AnimationAccel.DCL, cOn, cTD);
                var cioff = ani.Variable == "ON" ? ani.Value(AnimationAccel.DCL, cOff, cTD) : ani.Value(AnimationAccel.DCL, cTD, cOff);

                Util.DrawTextIcon(canvas, OnText, FontName, FontStyle, FontSize, OnIconString, IconSize, IconDirection, IconGap, rtOnText, cton, cion);
                Util.DrawTextIcon(canvas, OffText, FontName, FontStyle, FontSize, OffIconString, IconSize, IconDirection, IconGap, rtOffText, ctoff, cioff);
            }
            else
            {
                var cton = OnOff ? cTL : cTD;
                var ctoff = OnOff ? cTD : cTL;
                var cion = OnOff ? cOn : cTD;
                var cioff = OnOff ? cTD : cOff;

                Util.DrawTextIcon(canvas, OnText, FontName, FontStyle, FontSize, OnIconString, IconSize, IconDirection, IconGap, rtOnText, cton, cion);
                Util.DrawTextIcon(canvas, OffText, FontName, FontStyle, FontSize, OffIconString, IconSize, IconDirection, IconGap, rtOffText, ctoff, cioff);

            }
            #endregion
            base.OnDraw(canvas);
        }


        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtSwitch = rts["Switch"];
            var rtOn = rts["On"];
            var rtOff = rts["Off"];

            if (CollisionTool.Check(rtOn, x, y)) OnOff = true;
            else if (CollisionTool.Check(rtOff, x, y)) OnOff = false;

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtSwitch = rts["Switch"];
            var rtOn = rts["On"];
            var rtOff = rts["Off"];

            bHover = CollisionTool.Check(rtSwitch, x, y);

            base.OnMouseMove(x, y);
        }

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var thm = GoTheme.Current;
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var SwitchPadding = 5;
            var rtSwitch = Util.FromRect(rtContent); rtSwitch.Inflate(-SwitchPadding, -SwitchPadding);
            var cols = Util.Columns(rtSwitch, ["50%", "50%"]);
            var rtOff = cols[0];
            var rtOn = cols[1];

            rts["Switch"] = rtSwitch;
            rts["On"] = rtOn;
            rts["Off"] = rtOff;

            var rtOnText = rtOn;
            var rtOffText = rtOff;
            var n = rtSwitch.Width * SO - (thm.Corner / 2F);

            if (GoTheme.Current.Animation && ani.IsPlaying)
            {
                if (ani.Variable == "ON")
                {
                    rtOnText.Offset(ani.Value(AnimationAccel.DCL, -n, 0), 0);
                    rtOffText.Offset(ani.Value(AnimationAccel.DCL, 0, n), 0);
                }
                else if (ani.Variable == "OFF")
                {
                    rtOnText.Offset(ani.Value(AnimationAccel.DCL, 0, -n), 0);
                    rtOffText.Offset(ani.Value(AnimationAccel.DCL, n, 0), 0);
                }
            }
            else
            {
                if (!OnOff)
                {
                    rtOnText.Offset(-n, 0);
                    rtOffText.Offset(0, 0);
                }
                else
                {
                    rtOnText.Offset(0, 0);
                    rtOffText.Offset(n, 0);
                }
            }

            rts["OnText"] = rtOnText;
            rts["OffText"] = rtOffText;

            return rts;
        }
        #endregion
        #endregion

        #region GetSwitchColors
        void GetSwitchColors(SKColor Color, out SKColor c1, out SKColor c2, out SKColor c3, out SKColor c4)
        {
            c1 = Color.BrightnessTransmit(0.3F);
            c2 = Color.BrightnessTransmit(0F);
            c3 = Color.BrightnessTransmit(-0.2F);
            c4 = Color.BrightnessTransmit(-0.4F);
        }
        #endregion
       
    }
}
