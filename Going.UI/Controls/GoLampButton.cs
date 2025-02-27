using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoLampButton : GoControl
    {
        #region Properties
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string ButtonColor { get; set; } = "Base3";
        public string OnColor { get; set; } = "Good";
        public string OffColor { get; set; } = "Base2";

        public GoRoundType Round { get; set; } = GoRoundType.All;

        private bool bOnOff = false;
        public bool OnOff
        {
            get => bOnOff; set
            {
                if (bOnOff != value)
                {
                    bOnOff = value;
                    OnOffChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int LampSize { get; set; } = 24;
        public int Gap { get; set; } = 10;
        #endregion

        #region Event
        public event EventHandler? ButtonClicked;
        public event EventHandler? OnOffChanged;
        #endregion

        #region Constructor
        public GoLampButton()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        private bool bHover = false;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cBtn = thm.ToColor(ButtonColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cOff = thm.ToColor(OffColor);
            var cOn = thm.ToColor(OnColor);
            var cLmp = OnOff ? cOn : cOff;
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtBox = rts["Box"];
            var rtText = rts["Text"];

            Util.DrawBox(canvas, rtContent, cBtn.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cBtn.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), Round, thm.Corner);

            if (bDown) { rtBox.Offset(0, 1); rtText.Offset(0, 1); }

#if true
            /*
            var rtb = rtBox;
            var rtl = rtBox; rtl.Inflate(-1.5F, -1.5F);
            using var p = new SKPaint { IsAntialias = true };
            using var sh = SKShader.CreateLinearGradient(new SKPoint(rtb.Left, rtb.Top), new SKPoint(rtb.Left, rtb.Bottom), [cOff.BrightnessTransmit(-0.5F), cBtn.BrightnessTransmit(0.3F)], SKShaderTileMode.Clamp);
            p.IsStroke = false;
            p.Shader = sh;
            canvas.DrawOval(rtb, p);
            Util.DrawLamp(canvas, rtl, cOn, cOff, OnOff, false);
            p.Shader = null;
            */

            var rtb = rtBox;
            var rtl = rtBox; rtl.Inflate(-1.5F, -1.5F);
            using var p = new SKPaint { IsAntialias = true };
            p.IsStroke = false;
            p.Color = cBtn.BrightnessTransmit(-0.5F);
            canvas.DrawOval(rtb, p);
            Util.DrawLamp(canvas, rtl, cOn, cOff, OnOff, false);
#else
            var rtb = rtBox; rtb.Inflate(1.5F, 1.5F);
            var cLT = cLmp.BrightnessTransmit(0.1F);
            var cRB = cLmp.BrightnessTransmit(-0.1F);
            using var p = new SKPaint { IsAntialias = true };
            using var sh = SKShader.CreateLinearGradient(new SKPoint(rtb.Left, rtb.Top), new SKPoint(rtb.Right, rtb.Bottom), [cLT, cLT, cRB, cRB], [0, 0.5F, 0.5F, 1], SKShaderTileMode.Clamp);
            p.IsStroke = false;
            p.Color = cOff.BrightnessTransmit(-0.5F);
            p.Shader = sh;
            canvas.DrawOval(rtb, p);

            rtb.Inflate(-1.5F, -1.5F);
            var cLT2 = cLmp.BrightnessTransmit(-0.3F);
            var cRB2 = cLmp.BrightnessTransmit(0.3F);
            using var sh2 = SKShader.CreateLinearGradient(new SKPoint(rtb.Left, rtb.Top), new SKPoint(rtb.Right, rtb.Bottom), [cLT2, cRB2], SKShaderTileMode.Clamp);
            p.IsStroke = true;
            p.StrokeWidth = 3;
            p.Shader = sh2;
            canvas.DrawOval(rtb, p);


#endif
            Util.DrawText(canvas, Text, FontName, FontSize, rtText, cText, GoContentAlignment.MiddleCenter);


            base.OnDraw(canvas);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (CollisionTool.Check(rtBox, x, y)) bDown = true;

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (bDown)
            {
                bDown = false;
                if (CollisionTool.Check(rtBox, x, y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            bHover = CollisionTool.Check(rtBox, x, y);

            base.OnMouseMove(x, y);
        }

        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var (rtBox, rtText) = Util.TextIconBounds(Text, FontName, FontSize, new SKSize(LampSize, LampSize), GoDirectionHV.Horizon, Gap, rtContent, GoContentAlignment.MiddleCenter);

            rts["Box"] = rtBox;
            rts["Text"] = rtText;

            return rts;
        }
        #endregion
    }
}
