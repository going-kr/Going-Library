using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Managers;
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

namespace Going.UI.Controls
{
    public abstract class GoValue : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 2)] public float IconGap { get; set; } = 5;

        [GoFontNameProperty(PCategory.Control, 3)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 4)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 5)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 6)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        [GoProperty(PCategory.Control, 7)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 8)] public string BorderColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 9)] public string FillColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 10)] public string ValueColor { get; set; } = "Base2";
        [GoProperty(PCategory.Control, 11)] public GoRoundType Round { get; set; } = GoRoundType.All;

        [GoProperty(PCategory.Control, 12)] public float? TitleSize { get; set; }
        [GoProperty(PCategory.Control, 13)] public string? Title { get; set; }

        [GoProperty(PCategory.Control, 14)] public float? ButtonSize { get; set; }
        [GoProperty(PCategory.Control, 15)] public List<GoButtonItem> Buttons { get; set; } = [];

        [JsonIgnore] private bool UseTitle => TitleSize.HasValue && TitleSize.Value > 0;
        [JsonIgnore] private bool UseButton => ButtonSize.HasValue && ButtonSize.Value > 0 && Buttons.Count > 0;
        #endregion

        #region Member Variable
        private bool bValueDown = false;
        #endregion

        #region Constructor
        public GoValue()
        {
            Selectable = true;
        }
        #endregion

        #region Event
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked;
        public event EventHandler? ValueClicked;
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            #region var
            #region color
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var cFill = thm.ToColor(FillColor);
            var cValue = thm.ToColor(ValueColor);
            #endregion
            #region bounds
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtValue = rts["Value"];
            var rtTitle = rts["Title"];
            var rtButton = rts["Button"];
            #endregion
            #region round
            var useT = UseTitle;
            var useB = UseButton;
            var rnds = Util.Rounds(Direction, Round, (TitleSize.HasValue ? 1 : 0) + 1 + (ButtonSize.HasValue ? 1 : 0));
            var rndTitle = TitleSize.HasValue ? rnds[0] : GoRoundType.Rect;
            var rndValue = TitleSize.HasValue ? rnds[1] : rnds[0];
            var rndButton = ButtonSize.HasValue ? rnds[rnds.Length - 1] : GoRoundType.Rect;
            var rndButtons = Util.Rounds(GoDirectionHV.Horizon, rndButton, (ButtonSize.HasValue ? Buttons.Count : 0));

            #endregion
            #endregion

            #region Title
            if (useT)
            {
                Util.DrawBox(canvas, rtTitle, cFill, rndTitle, thm.Corner);
                Util.DrawTextIcon(canvas, Title, FontName, FontStyle, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtTitle, cText);
            }
            #endregion

            #region Value
            Util.DrawBox(canvas, rtValue, cValue, rndValue, thm.Corner);

            OnDrawValue(canvas, thm, rtValue);
            #endregion

            #region Button
            if (useB)
            {
                buttonLoop((i, btn, rt) =>
                {
                    var rnd = rndButtons[i];
                    var cBtn = cFill.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                    var cTxt = cText.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                    Util.DrawBox(canvas, rt, cBtn.BrightnessTransmit(btn.Hover ? thm.HoverFillBrightness : 0), rnd, thm.Corner);

                    if (i > 0)
                    {
                        using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 1, Color = cFill.BrightnessTransmit(thm.BorderBrightness) };
                        using var pe = SKPathEffect.CreateDash([2, 2], 2);
                        float x = (int)rt.Left; x += 0.5F;
                        p.PathEffect = pe;
                        canvas.DrawLine(x, rt.Top + 5, x, rt.Bottom - 5, p);
                    }

                    if (btn.Down) rt.Offset(0, 1);
                    Util.DrawTextIcon(canvas, btn.Text, FontName, FontStyle, FontSize, btn.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rt, cTxt, GoContentAlignment.MiddleCenter);
                });
            }
            #endregion

            #region Border
            Util.DrawBox(canvas, rtBox, SKColors.Transparent, cBorder, Round, thm.Corner);
            #endregion

            #region Border2
            if (useB)
            {
                buttonLoop((i, btn, rt) =>
                {
                    if (btn.Hover)
                    {
                        var rnd = rndButtons[i];
                        var cBtn = cFill.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                        var cTxt = cText.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                        Util.DrawBox(canvas, rt, SKColors.Transparent, cBtn.BrightnessTransmit(btn.Hover ? thm.HoverBorderBrightness : 0), rnd, thm.Corner);
                    }
                });
            }
            #endregion

            base.OnDraw(canvas, thm);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            #region Button
            var useB = ButtonSize.HasValue && ButtonSize.Value > 0 && Buttons.Count > 0;
            if (useB)
            {
                buttonLoop((i, btn, rt) =>
                {
                    if (CollisionTool.Check(rt, x, y)) btn.Down = true;
                });
            }
            #endregion

            #region Value
            var rtValue = Areas()["Value"];
            if (CollisionTool.Check(rtValue, x, y)) bValueDown = true;
            #endregion
            base.OnMouseDown(x, y, button);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            #region Button
            var useB = ButtonSize.HasValue && ButtonSize.Value > 0 && Buttons.Count > 0;
            if (useB)
            {
                buttonLoop((i, btn, rt) =>
                {
                    if (btn.Down)
                    {
                        btn.Down = false;
                        if (CollisionTool.Check(rt, x, y)) ButtonClicked?.Invoke(this, new ButtonClickEventArgs(btn));
                    }
                });
            }
            #endregion

            #region Value
            if (bValueDown)
            {
                var rtValue = Areas()["Value"];
                ValueClicked?.Invoke(this, EventArgs.Empty);
                bValueDown = false;
            }
            #endregion
            base.OnMouseUp(x, y, button);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
            #region Button
            var useB = ButtonSize.HasValue && ButtonSize.Value > 0 && Buttons.Count > 0;
            if (useB)
            {
                buttonLoop((i, btn, rt) =>
                {
                    btn.Hover = CollisionTool.Check(rt, x, y);
                });
            }
            #endregion
            base.OnMouseMove(x, y);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();

            var rts = Direction == GoDirectionHV.Vertical ? Util.Rows(dic["Content"], [$"{TitleSize ?? 0}px", $"100%", $"{ButtonSize ?? 0}px"])
                                                          : Util.Columns(dic["Content"], [$"{TitleSize ?? 0}px", $"100%", $"{ButtonSize ?? 0}px"]);
            dic["Title"] = rts[0];
            dic["Value"] = rts[1];
            dic["Button"] = rts[2];

            return dic;
        }
        #endregion
        #endregion

        #region Abstract
        protected abstract void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect valueBounds);
        #endregion

        #region Method
        void buttonLoop(Action<int, GoButtonItem, SKRect> act)
        {
            var rtButton = Areas()["Button"];
            var rnds = Util.Rounds(Direction, Round, (TitleSize.HasValue ? 1 : 0) + 1 + (ButtonSize.HasValue ? 1 : 0));
            var rtsBtn = Util.Columns(rtButton, Buttons.Select(x => x.Size).ToArray());

            for (int i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];
                var rt = rtsBtn[i];

                act(i, btn, rt);
            }
        }
        #endregion
    }

    public class GoValueString : GoValue
    {
        #region Properties
        [GoProperty(PCategory.Control, 16)] public string? Value { get; set; }
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            var cText = thm.ToColor(TextColor);

            Util.DrawText(canvas, Value, FontName, FontStyle, FontSize, rtValue, cText);
        }
        #endregion
    }

    public class GoValueNumber<T> : GoValue where T : struct
    {
        #region Properties
        [GoProperty(PCategory.Control, 16)] public T Value { get; set; }
        [GoProperty(PCategory.Control, 17)] public string? FormatString { get; set; } = null;

        [GoProperty(PCategory.Control, 18)] public string? Unit { get; set; }
        [GoProperty(PCategory.Control, 19)] public float? UnitSize { get; set; } = null;
        #endregion

        #region Constructor
        public GoValueNumber()
        {
            if (typeof(T) == typeof(sbyte)) { }
            else if (typeof(T) == typeof(short)) { }
            else if (typeof(T) == typeof(int)) { }
            else if (typeof(T) == typeof(long)) { }
            else if (typeof(T) == typeof(byte)) { }
            else if (typeof(T) == typeof(ushort)) { }
            else if (typeof(T) == typeof(uint)) { }
            else if (typeof(T) == typeof(ulong)) { }
            else if (typeof(T) == typeof(float)) { }
            else if (typeof(T) == typeof(double)) { }
            else if (typeof(T) == typeof(decimal)) { }
            else throw new Exception("숫자 자료형이 아닙니다");
        }
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtUnit) = bounds(rtValue);

            var text = ValueTool.ToString(Value, FormatString) ?? "";
            Util.DrawText(canvas, text, FontName, FontStyle, FontSize, rtText, cText);

            if (UnitSize.HasValue)
            {
                Util.DrawText(canvas, Unit, FontName, FontStyle, FontSize, rtUnit, cText);

                using var pe = SKPathEffect.CreateDash([2, 2], 2);
                p.StrokeWidth = 1;
                p.IsStroke = false;
                p.Color = Util.FromArgb(128, cBorder);
                p.PathEffect = pe;
                canvas.DrawLine(rtUnit.Left, rtValue.Top + 10, rtUnit.Left, rtValue.Bottom - 10, p);
            }
        }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtUnit) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["100%", $"{UnitSize ?? 0}px"]);

            return (rts[0], rts[1]);
        }
        #endregion
    }
     
    public class GoValueBoolean : GoValue
    {
        #region Properties
        [GoProperty(PCategory.Control, 16)] public bool Value { get; set; }

        [GoProperty(PCategory.Control, 17)] public string? OnText { get; set; } = "ON";
        [GoProperty(PCategory.Control, 18)] public string? OffText { get; set; } = "OFF";
        [GoProperty(PCategory.Control, 19)] public string? OnIconString { get; set; }
        [GoProperty(PCategory.Control, 20)] public string? OffIconString { get; set; }
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cTextL = thm.ToColor(TextColor);
            var cTextD = Util.FromArgb(80, cTextL);
            var (rtOn, rtOff) = bounds(rtValue);


            Util.DrawTextIcon(canvas, OnText, FontName, FontStyle, FontSize, OnIconString, IconSize, GoDirectionHV.Horizon, IconGap, rtOn, Value ? cTextL : cTextD);
            Util.DrawTextIcon(canvas, OffText, FontName, FontStyle, FontSize, OffIconString, IconSize, GoDirectionHV.Horizon, IconGap, rtOff, Value ? cTextD : cTextL);

            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cBorder);
            p.PathEffect = pe;

            canvas.DrawLine(rtValue.MidX, rtValue.Top + 10, rtValue.MidX, rtValue.Bottom - 10, p);
        }
        #endregion
         
        #region bounds
        (SKRect rtOn, SKRect rtOff) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["50%", "50%"]);
            return (rts[1], rts[0]);
        }
        #endregion
    }
}
