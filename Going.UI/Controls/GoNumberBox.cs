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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Controls
{
    public class GoNumberBox : GoControl
    {
        #region Const 
        const int CHKTM = 500;
        #endregion

        #region Properties
        [GoFontNameProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 3)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 5)] public string BorderColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 6)] public string ButtonColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 7)] public string ValueColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control, 8)] public GoRoundType Round { get; set; } = GoRoundType.All;

        private double nVal = 0;
        [GoProperty(PCategory.Control,10)]
        public double Value
        {
            get => nVal;
            set
            {
                if (nVal != value)
                {
                    nVal = value;
                    ValueChanged?.Invoke(this, System.EventArgs.Empty);
                }
            }
        }

        [GoProperty(PCategory.Control,11)] public double Minimum { get; set; } = 0D;
        [GoProperty(PCategory.Control,12)] public double Maximum { get; set; } = 100D;
        [GoProperty(PCategory.Control,13)] public double Tick { get; set; } = 1D;
        [GoProperty(PCategory.Control,14)] public string? Format { get; set; }
        [GoProperty(PCategory.Control,15)] public float ButtonSize { get; set; } = 40;
        #endregion

        #region Member Variable
        bool bDownP = false, bDownM = false;
        bool bHoverP = false, bHoverM = false;

        DateTime dtPlus = DateTime.Now, dtMinus = DateTime.Now;
        int maxinterval = 250, mininterval = 10;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var cButton = thm.ToColor(ButtonColor);
            var cValue = thm.ToColor(ValueColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtValue = rts["Value"];
            var rtPlus = rts["Plus"];
            var rtMinus = rts["Minus"];
            
            var rndv = GoRoundType.T;
            var rndm = GoRoundType.LB;
            var rndp = GoRoundType.RB;

            if (Direction == GoDirectionHV.Horizon)
            {
                var rnds = Util.Rounds(GoDirectionHV.Horizon, Round, 3);
                rndv = rnds[1];
                rndm = rnds[0];
                rndp = rnds[2];
            }
            else if(Direction == GoDirectionHV.Vertical) 
            {
                var rnds1 = Util.Rounds(GoDirectionHV.Vertical, Round, 2);
                var rnds2 = Util.Rounds(GoDirectionHV.Horizon, rnds1[1], 2);
                rndv = rnds1[0];
                rndm = rnds2[0];
                rndp = rnds2[1];
            }
            #endregion
            #region Back
            Util.DrawBox(canvas, rtValue, cValue, rndv, thm.Corner);
            #endregion
            #region Button
            #region Plus
            {
                var bHover = bHoverP;
                var bDown = bDownP;
                var rt = rtPlus;
                var rnd = rndp;

                var cBtn = cButton.BrightnessTransmit(bDown ? thm.DownBrightness : 0);
                var cTxt = cText.BrightnessTransmit(bDown ? thm.DownBrightness : 0);
                Util.DrawBox(canvas, rt, cBtn.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), rnd, thm.Corner);
                if (bDown) rt.Offset(0, 1);
                Util.DrawIcon(canvas, "fa-plus", rt.Height / 2, rt, cTxt);
            }
            #endregion
            #region Minus
            {
                var bHover = bHoverM;
                var bDown = bDownM;
                var rt = rtMinus;
                var rnd = rndm;

                var cBtn = cButton.BrightnessTransmit(bDown ? thm.DownBrightness : 0);
                var cTxt = cText.BrightnessTransmit(bDown ? thm.DownBrightness : 0);
                Util.DrawBox(canvas, rt, cBtn.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), rnd, thm.Corner);
                if (bDown) rt.Offset(0, 1);
                Util.DrawIcon(canvas, "fa-minus", rt.Height / 2, rt, cTxt);
            }
            #endregion
            #endregion
            #region Value
            var txt = ValueTool.ToString<double>(Value, Format);
            Util.DrawText(canvas, txt, FontName, FontStyle, FontSize, rtValue, cText);
            #endregion
            #region Border
            Util.DrawBox(canvas, rtContent, SKColors.Transparent, cBorder, Round, thm.Corner);
            #endregion
            #region Border2
            #region Minus
            if(bHoverM)
            {
                var bHover = bHoverM;
                var bDown = bDownM;
                var rt = rtMinus;
                var rnd = rndm;

                var cBtn = cButton.BrightnessTransmit(bDown ? thm.DownBrightness : 0);
                var cTxt = cText.BrightnessTransmit(bDown ? thm.DownBrightness : 0);
                Util.DrawBox(canvas, rt, SKColors.Transparent, cBtn.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), rnd, thm.Corner);
            }
            #endregion
            #region Plus
            if(bHoverP)
            {
                var bHover = bHoverP;
                var bDown = bDownP;
                var rt = rtPlus;
                var rnd = rndp;

                var cBtn = cButton.BrightnessTransmit(bDown ? thm.DownBrightness : 0);
                var cTxt = cText.BrightnessTransmit(bDown ? thm.DownBrightness : 0);
                Util.DrawBox(canvas, rt, SKColors.Transparent, cBtn.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), rnd, thm.Corner);
            }
            #endregion
           
            #endregion

            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtValue = rts["Value"];
            var rtPlus = rts["Plus"];
            var rtMinus = rts["Minus"];

            if (CollisionTool.Check(rtPlus, x, y))
            {
                bDownP = true;

                dtPlus = DateTime.Now;
                Task.Run(async () =>
                {
                    while (bDownP)
                    {
                        var tmp = (DateTime.Now - dtPlus).TotalMilliseconds;

                        if (bDownP && tmp >= CHKTM)
                        {
                            var delay = (int)MathTool.Constrain(MathTool.Map(tmp, 2000, CHKTM, mininterval, maxinterval), mininterval, maxinterval);
                            Value = MathTool.Constrain(Value + Tick, Minimum, Maximum);
                            await Task.Delay(delay);
                        }
                        else await Task.Delay(100);
                        Invalidate();
                    }
                });
            }
            if (CollisionTool.Check(rtMinus, x, y))
            {
                bDownM = true;

                dtMinus = DateTime.Now;
                Task.Run(async () =>
                {
                    while (bDownM)
                    {
                        var tmm = (DateTime.Now - dtMinus).TotalMilliseconds;

                        if (bDownM && tmm >= CHKTM)
                        {
                            var delay = (int)MathTool.Constrain(MathTool.Map(tmm, 2000, CHKTM, mininterval, maxinterval), mininterval, maxinterval);
                            Value = MathTool.Constrain(Value - Tick, Minimum, Maximum);
                            await Task.Delay(delay);
                        }
                        else await Task.Delay(100);
                        Invalidate();
                    }
                });
            }

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtValue = rts["Value"];
            var rtPlus = rts["Plus"];
            var rtMinus = rts["Minus"];

            if (bDownP)
            {
                if (CollisionTool.Check(rtPlus, x, y))
                {
                    if ((DateTime.Now - dtPlus).TotalMilliseconds < CHKTM)
                        Value = MathTool.Constrain(Value + Tick, Minimum, Maximum);
                }
                bDownP = false;
            }
            if (bDownM)
            {
                if (CollisionTool.Check(rtMinus, x, y))
                {
                    if ((DateTime.Now - dtMinus).TotalMilliseconds < CHKTM)
                        Value = MathTool.Constrain(Value - Tick, Minimum, Maximum);
                }
                bDownM = false;
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtValue = rts["Value"];
            var rtPlus = rts["Plus"];
            var rtMinus = rts["Minus"];

            bHoverP = CollisionTool.Check(rtPlus, x, y);
            bHoverM = CollisionTool.Check(rtMinus, x, y);

            base.OnMouseMove(x, y);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];
            if (Direction == GoDirectionHV.Vertical)
            {
                var rts = Util.Grid(rtContent, ["50%", "50%"], ["100%", $"{ButtonSize}px"]);
                dic["Value"] = Util.Merge(rts, 0, 0, 2, 1);
                dic["Plus"] = rts[1, 1];
                dic["Minus"] = rts[1, 0];
            }
            else
            {
                var rts = Util.Columns(rtContent, [$"{ButtonSize}px", "100%", $"{ButtonSize}px"]);
                dic["Value"] = rts[1];
                dic["Plus"] = rts[2];
                dic["Minus"] = rts[0];
            }
            return dic;
        }
        #endregion
        #endregion

    }
}
