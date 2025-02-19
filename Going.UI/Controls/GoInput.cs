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
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Controls
{
    public abstract class GoInput : GoControl
    {
        #region Properties
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;

        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        public string TextColor { get; set; } = "Fore";
        public string BorderColor { get; set; } = "Base3";
        public string FillColor { get; set; } = "Base3";
        public string ValueColor { get; set; } = "Base1";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public float? TitleSize { get; set; }
        public string? Title { get; set; }

        public float? ButtonSize { get; set; }
        public List<GoButtonInfo> Buttons { get; set; } = [];
        public virtual bool Valid => true;

        private bool UseTitle => TitleSize.HasValue && TitleSize.Value > 0;
        private bool UseButton => ButtonSize.HasValue && ButtonSize.Value > 0 && Buttons.Count > 0;
        #endregion

        #region Member Variable
        private bool bValueDown = false;
        #endregion

        #region Constructor
        public GoInput()
        {
            Selectable = true;
        }
        #endregion

        #region Event
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked;
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;
            #region color
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var cFill = thm.ToColor(FillColor);
            var cValue = thm.ToColor(ValueColor);
            var cInput = !Valid ? thm.Error : thm.Hignlight;
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
                Util.DrawTextIcon(canvas, Title, FontName, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtTitle, cText);
            }
            #endregion

            #region Value
            Util.DrawBox(canvas, rtValue, cValue, rndValue, thm.Corner);

            OnDrawValue(canvas, rtValue);
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
                    Util.DrawTextIcon(canvas, btn.Text, FontName, FontSize, btn.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rt, cTxt, GoContentAlignment.MiddleCenter);
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

            if (GoInputEventer.Current.InputControl == this) Util.DrawBox(canvas, rtValue, SKColors.Transparent,cInput, rndValue, thm.Corner);
            #endregion

            base.OnDraw(canvas);
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
                if (CollisionTool.Check(rtValue, x, y)) OnValueClick(x, y, button);
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
        protected abstract void OnDrawValue(SKCanvas canvas, SKRect valueBounds);
        protected abstract void OnValueClick(float x, float y, GoMouseButton button);
        #endregion

        #region Method
        void buttonLoop(Action<int, GoButtonInfo, SKRect> act)
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

    public class GoInputString : GoInput
    {
        #region Properties
        private string sVal = "";
        public string Value
        {
            get => sVal;
            set
            {
                if (sVal != value)
                {
                    sVal = value;
                    ValueChanged?.Invoke(this, System.EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, SKRect rtValue)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);

            Util.DrawText(canvas, Value, FontName, FontSize, rtValue, cText);

        }
        #endregion

        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            GoInputEventer.Current.GenString(this, Areas()["Value"], (s) => Value = s, Value);
        }
        #endregion
    }

    public class GoInputNumber<T> : GoInput where T : struct
    {
        #region Value
        private T sVal = default(T);
        public T Value
        {
            get => sVal;
            set
            {
                if (!sVal.Equals(value))
                {
                    sVal = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public T? Minimum { get; set; } = null;
        public T? Maximum { get; set; } = null;

        public string? FormatString { get; set; } = null;

        public override bool Valid => bValid;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Member Variable
        bool bValid = true;
        T valOrigin;
        #endregion

        #region Constructor
        public GoInputNumber()
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
        protected override void OnDrawValue(SKCanvas canvas, SKRect rtValue)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);

            var text = ValueTool.ToString(Value, FormatString) ?? "";
            Util.DrawText(canvas, text, FontName, FontSize, rtValue, cText);
        }
        #endregion

        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            bValid = true;
            valOrigin = Value;
            GoInputEventer.Current.GenNumber(this, Areas()["Value"], (s) =>
            {
                var v = ValueTool.FromString<T>(s);
                if (v != null) bValid = ValueTool.Valid((T)v, Minimum, Maximum);
                else bValid = false;

                if (v != null)
                {
                    if (bValid) Value = (T)v;
                    else Value = ValueTool.Constrain((T)v, Minimum, Maximum);
                }
                else
                {
                    Value = valOrigin;
                }

            }, Value, Minimum, Maximum);
        }
        #endregion
    }

    public class GoInputInt : GoInputNumber<int> { }
    public class GoInputDouble : GoInputNumber<double> { }
}
