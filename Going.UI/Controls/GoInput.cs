using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Controls
{
    public abstract class GoInput : GoControl
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
        [GoProperty(PCategory.Control, 10)] public string ValueColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control, 11)] public GoRoundType Round { get; set; } = GoRoundType.All;

        [GoProperty(PCategory.Control, 12)] public float? TitleSize { get; set; }
        [GoProperty(PCategory.Control, 13)] public string? Title { get; set; }

        [GoProperty(PCategory.Control, 14)] public float? ButtonSize { get; set; }
        [GoProperty(PCategory.Control, 15)] public List<GoButtonItem> Buttons { get; set; } = [];
        [JsonIgnore] public virtual bool Valid => true;

        [JsonIgnore] private bool UseTitle => TitleSize.HasValue && TitleSize.Value > 0;
        [JsonIgnore] private bool UseButton => ButtonSize.HasValue && ButtonSize.Value > 0 && Buttons.Count > 0;
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
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            #region var
            #region color
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var cFill = thm.ToColor(FillColor);
            var cValue = thm.ToColor(ValueColor);
            var cInput = !Valid ? thm.Error : thm.Highlight;
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
                        float x = Convert.ToInt32(rt.Left); x += 0.5F;
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

            if (GoInputEventer.Current.InputControl == this) Util.DrawBox(canvas, rtValue, SKColors.Transparent, cInput, rndValue, thm.Corner);
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
        protected abstract void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect valueBounds);
        protected abstract void OnValueClick(float x, float y, GoMouseButton button);
        #endregion

        #region Method
        void buttonLoop(Action<int, GoButtonItem, SKRect> act)
        {
            var rtButton = Areas()["Button"];
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
        [GoProperty(PCategory.Control, 16)]
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
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            var cText = thm.ToColor(TextColor);

            Util.DrawText(canvas, Value, FontName, FontStyle, FontSize, rtValue, cText);

        }
        #endregion

        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            GoInputEventer.Current.FireInputString(this, Areas()["Value"], (s) => Value = s, Value);
        }
        #endregion
    }

    public class GoInputNumber<T> : GoInput where T : struct
    {
        #region Properties
        private T sVal = default(T);
        [GoProperty(PCategory.Control, 16)]
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

        [GoProperty(PCategory.Control, 17)] public T? Minimum { get; set; } = null;
        [GoProperty(PCategory.Control, 18)] public T? Maximum { get; set; } = null;

        [GoProperty(PCategory.Control, 19)] public string? FormatString { get; set; } = null;

        [JsonIgnore] public override bool Valid => bValid;

        [GoProperty(PCategory.Control, 20)] public string? Unit { get; set; }
        [GoProperty(PCategory.Control, 21)] public float? UnitSize { get; set; } = null;
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
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtUnit) = bounds(rtValue);

            var text = ValueTool.ToString(Value, FormatString) ?? "";
            Util.DrawText(canvas, text, FontName, FontStyle, FontSize, rtText, cText);

            if(UnitSize.HasValue)
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

        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            var rtValue = Areas()["Value"];
            var (rtText, rtUnit) = bounds(rtValue);

            bValid = true;
            valOrigin = Value;
            
            GoInputEventer.Current.FireInputNumber(this, rtText, (s) =>
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

        #region bounds
        (SKRect rtText, SKRect rtUnit) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["100%", $"{UnitSize??0}px"]);
         
            return (rts[0], rts[1]);
        }
        #endregion
    }

    public class GoInputBoolean : GoInput
    {
        #region Properties
        private bool sVal = false;
        [GoProperty(PCategory.Control, 16)]
        public bool Value
        {
            get => sVal;
            set
            {
                if (sVal != value)
                {
                    ani.Stop();
                    ani.Start(Animation.Time200, value ? "ON" : "OFF", () =>
                    {
                        sVal = value;
                        ValueChanged?.Invoke(this, System.EventArgs.Empty);
                    });
                }
            }
        }

        [GoProperty(PCategory.Control, 17)] public string? OnText { get; set; } = "ON";
        [GoProperty(PCategory.Control, 18)] public string? OffText { get; set; } = "OFF";
        [GoProperty(PCategory.Control, 19)] public string? OnIconString { get; set; }
        [GoProperty(PCategory.Control, 20)] public string? OffIconString { get; set; }
        #endregion
        
        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Member Variable
        private Animation ani = new Animation();
        #endregion

        #region Constructor
        public GoInputBoolean()
        {
            ani.Refresh = () => 
            Invalidate();
        }
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cTextL = thm.ToColor(TextColor);
            var cTextD = Util.FromArgb(80, cTextL);
            var (rtOn, rtOff) = bounds(rtValue);

            if (Animation.UseAnimation && ani.IsPlaying)
            {
                var cOn = ani.Variable == "ON" ? ani.Value(AnimationAccel.Linear, cTextD, cTextL) : ani.Value(AnimationAccel.Linear, cTextL, cTextD);
                var cOff = ani.Variable == "ON" ? ani.Value(AnimationAccel.Linear, cTextL, cTextD) : ani.Value(AnimationAccel.Linear, cTextD, cTextL);

                Util.DrawTextIcon(canvas, OnText, FontName, FontStyle, FontSize, OnIconString, IconSize, GoDirectionHV.Horizon, IconGap, rtOn, cOn);
                Util.DrawTextIcon(canvas, OffText, FontName, FontStyle, FontSize, OffIconString, IconSize, GoDirectionHV.Horizon, IconGap, rtOff, cOff);

            }
            else
            {
                Util.DrawTextIcon(canvas, OnText, FontName, FontStyle, FontSize, OnIconString, IconSize, GoDirectionHV.Horizon, IconGap, rtOn, Value ? cTextL : cTextD);
                Util.DrawTextIcon(canvas, OffText, FontName, FontStyle, FontSize, OffIconString, IconSize, GoDirectionHV.Horizon, IconGap, rtOff, Value ? cTextD : cTextL);
            }

            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cBorder);
            p.PathEffect = pe;

            canvas.DrawLine(rtValue.MidX, rtValue.Top + 10, rtValue.MidX, rtValue.Bottom - 10, p);
        }
        #endregion

        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            var rtValue = Areas()["Value"];
            var (rtOn, rtOff) = bounds(rtValue);

            if (CollisionTool.Check(rtOn, x, y)) Value = true;
            else if (CollisionTool.Check(rtOff, x, y)) Value = false;
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

    // 콤보BOX에 Input이 들어갈 수 있음
    public class GoInputCombo : GoInput
    {
        #region Properties
        [GoProperty(PCategory.Control, 16)] public List<GoListItem> Items { get; set; } = [];

        private int nSelIndex = -1;
        [JsonIgnore]
        public int SelectedIndex
        {
            get => nSelIndex;
            set
            {
                if (nSelIndex != value)
                {
                    nSelIndex = value;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [JsonIgnore] public GoListItem? SelectedItem => SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;

        [GoProperty(PCategory.Control, 17)] public int ItemHeight { get; set; } = 30;
        [GoProperty(PCategory.Control, 18)] public int MaximumViewCount { get; set; } = 8;
        #endregion

        #region Event
        public event EventHandler? SelectedIndexChanged;

        public event EventHandler<GoCancelableEventArgs>? DropDownOpening;
        #endregion

        #region Member Variable
        GoComboBoxDropDownWindow dwnd = new GoComboBoxDropDownWindow();
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtArrow) = bounds(rtValue);

            #region item
            if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
            {
                var item = Items[SelectedIndex];
                Util.DrawTextIcon(canvas, item.Text, FontName, FontStyle, FontSize, item.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtText, cText);
            }
            #endregion

            #region sep
            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cBorder);
            p.PathEffect = pe;
            canvas.DrawLine(rtArrow.Left, rtValue.Top + 10, rtArrow.Left, rtValue.Bottom - 10, p);
            #endregion

            #region arrow
            Util.DrawIcon(canvas, dwnd.Visible ? "fa-angle-up" : "fa-angle-down", 14, rtArrow, cText);
            #endregion
        }
        #endregion

        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            var rtValue = Areas()["Value"];

            if (CollisionTool.Check(rtValue, x, y) && Items.Count > 0)
            {
                var args = new GoCancelableEventArgs(); // OpenTK는 윈도우 1개, WinForm은 윈도우 여러개(컨트롤 개수만큼) 그래서 이벤트를 취소할 수 있게 해야함
                DropDownOpening?.Invoke(this, args);

                if (!args.Cancel)
                {   // false이면 내장된 이벤트를 실행
                    var rt = rtValue; rt.Offset(ScreenX, ScreenY);
                    var sel = SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;
                    dwnd.Show(rt, FontName, FontStyle, FontSize, ItemHeight, MaximumViewCount, Items, sel, (item) =>
                    {
                        if (item != null)
                            SelectedIndex = Items.IndexOf(item);
                    });
                }
            }
        }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtArrow) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["100%", "40px"]);
            return (rts[0], rts[1]);
        }
        #endregion
    }

    public class GoInputSelector : GoInput
    {
        #region Properties
        [GoProperty(PCategory.Control, 16)] public List<GoListItem> Items { get; set; } = [];

        private int nSelIndex = -1;
        [JsonIgnore]
        public int SelectedIndex
        {
            get => nSelIndex;
            set
            {
                if (nSelIndex != value)
                {
                    nSelIndex = value;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [JsonIgnore] public GoListItem? SelectedItem => SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;
        #endregion

        #region Event
        public event EventHandler? SelectedIndexChanged;
        #endregion

        #region Member Variable
        bool bDownL = false, bHoverL = false;
        bool bDownR = false, bHoverR = false;

        Animation ani = new Animation();
        #endregion

        #region Constructor
        public GoInputSelector()
        {
            ani.Refresh = () => Invalidate();
        }
        #endregion

        #region Override
        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtArrowL, rtArrowR) = bounds(rtValue);

            #region item
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipRect(rtText);
                if (Animation.UseAnimation && ani.IsPlaying)
                {
                    var sp = ani.Variable?.Split(':');
                    if (sp != null && sp.Length == 2 && int.TryParse(sp[1], out var selidx))
                    {
                        var dir = sp[0];

                        if(dir == "Next")
                        {
                            if (selidx - 1 >= 0)
                            {
                                var itmP = Items[selidx - 1];
                                var rtP = Util.FromRect(rtText.Left + ani.Value(AnimationAccel.DCL, 0, -rtText.Width), rtText.Top, rtText.Width, rtText.Height);
                                var cP = cText.WithAlpha(Convert.ToByte(ani.Value(AnimationAccel.DCL, 255, 0)));
                                Util.DrawTextIcon(canvas, itmP.Text, FontName, FontStyle, FontSize, itmP.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtP, cP);
                            }

                            var rtN = Util.FromRect(rtText.Left + ani.Value(AnimationAccel.DCL, rtText.Width, 0), rtText.Top, rtText.Width, rtText.Height);
                            var itmN = Items[selidx];
                            var cN = cText.WithAlpha(Convert.ToByte(ani.Value(AnimationAccel.DCL, 0, 255)));
                            Util.DrawTextIcon(canvas, itmN.Text, FontName, FontStyle, FontSize, itmN.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtN, cN);
                        }
                        else if(dir == "Prev")
                        {
                            if (selidx + 1 < Items.Count)
                            {
                                var itmP = Items[selidx + 1];
                                var rtP = Util.FromRect(rtText.Left + ani.Value(AnimationAccel.DCL, 0, rtText.Width), rtText.Top, rtText.Width, rtText.Height);
                                var cP = cText.WithAlpha(Convert.ToByte(ani.Value(AnimationAccel.DCL, 255, 0)));
                                Util.DrawTextIcon(canvas, itmP.Text, FontName, FontStyle, FontSize, itmP.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtP, cP);
                            }

                            var itmN = Items[selidx];
                            var rtN = Util.FromRect(rtText.Left + ani.Value(AnimationAccel.DCL, -rtText.Width, 0), rtText.Top, rtText.Width, rtText.Height);
                            var cN = cText.WithAlpha(Convert.ToByte(ani.Value(AnimationAccel.DCL, 0, 255)));
                            Util.DrawTextIcon(canvas, itmN.Text, FontName, FontStyle, FontSize, itmN.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtN, cN);
                        }

                    }
                }
                else
                {
                    if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
                    {
                        var item = Items[SelectedIndex];
                        Util.DrawTextIcon(canvas, item.Text, FontName, FontStyle, FontSize, item.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtText, cText);
                    }
                }
            }
            #endregion

            #region sep
            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cBorder);
            p.PathEffect = pe;
            canvas.DrawLine(rtArrowL.Right, rtValue.Top + 10, rtArrowL.Right, rtValue.Bottom - 10, p);
            canvas.DrawLine(rtArrowR.Left, rtValue.Top + 10, rtArrowR.Left, rtValue.Bottom - 10, p);
            #endregion

            #region arrow
            if (bDownL) rtArrowL.Offset(0, 1);
            if (bDownR) rtArrowR.Offset(0, 1);
            Util.DrawIcon(canvas, "fa-angle-left", 14, rtArrowL, cText.BrightnessTransmit(bDownL ? thm.DownBrightness : 0));
            Util.DrawIcon(canvas, "fa-angle-right", 14, rtArrowR, cText.BrightnessTransmit(bDownR ? thm.DownBrightness : 0));
            #endregion
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            base.OnMouseDown(x, y, button);
            var (rtText, rtArrowL, rtArrowR) = bounds(Areas()["Value"]);


            if (CollisionTool.Check(rtArrowL, x, y)) bDownL = true;
            if (CollisionTool.Check(rtArrowR, x, y)) bDownR = true;
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            base.OnMouseUp(x, y, button);
            var (rtText, rtArrowL, rtArrowR) = bounds(Areas()["Value"]);

            if (bDownL)
            {
                if (Items.Count > 0)
                {
                    if (CollisionTool.Check(rtArrowL, x, y))
                    {
                        var n = MathTool.Constrain(SelectedIndex - 1, 0, Items.Count - 1);
                        if (n != SelectedIndex)
                        {
                            ani.Stop();
                            ani.Start(Animation.Time150, $"Prev:{n}", () => SelectedIndex = n);
                        }
                    }
                }
                else SelectedIndex = -1;
                bDownL = false;
            }

            if (bDownR)
            {
                if (Items.Count > 0)
                {
                    if (CollisionTool.Check(rtArrowR, x, y))
                    {
                        var n = MathTool.Constrain(SelectedIndex + 1, 0, Items.Count - 1);
                        if (n != SelectedIndex)
                        {
                            ani.Stop();
                            ani.Start(Animation.Time150, $"Next:{n}", () => SelectedIndex = n);
                        }
                    }
                }
                else SelectedIndex = -1;
                bDownR = false;
            }
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);
            var (rtText, rtArrowL, rtArrowR) = bounds(Areas()["Value"]);
            bHoverL = CollisionTool.Check(rtArrowL, x, y);
            bHoverR = CollisionTool.Check(rtArrowR, x, y);
        }
        #endregion
        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
        }
        #endregion
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtArrowL, SKRect rtArrowR) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["40px", "100%", "40px"]);
            return (rts[1], rts[0], rts[2]);
        }
        #endregion
    }

    public class GoInputColor : GoInput
    {
        #region Properties
        [GoProperty(PCategory.Control, 16)]
        public SKColor Value
        {
            get => cValue; 
            set
            {
                if(cValue != value)
                {
                    cValue = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Event
        public event EventHandler? ValueChanged;

        public event EventHandler<GoCancelableEventArgs>? DropDownOpening;
        #endregion

        #region Member Variable
        SKColor cValue = SKColors.White;
        GoColorDropDownWindow dwnd = new GoColorDropDownWindow();
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtArrow) = bounds(rtValue);

            #region item
            var isz = FontSize;
            var text = $"#{Value.Red:X2}{Value.Green:X2}{Value.Blue:X2}";
            var (rtIco, rtVal) = Util.TextIconBounds(text, FontName, FontStyle, FontSize, new SKSize(isz, isz), GoDirectionHV.Horizon, IconGap, rtText, GoContentAlignment.MiddleCenter);

            Util.DrawText(canvas, text, FontName, FontStyle, FontSize, rtVal, cText);
            Util.DrawBox(canvas, rtIco, Value, GoRoundType.Rect, thm.Corner);
            #endregion

            #region sep
            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cBorder);
            p.PathEffect = pe;
            canvas.DrawLine(rtArrow.Left, rtValue.Top + 10, rtArrow.Left, rtValue.Bottom - 10, p);
            #endregion

            #region arrow
            if (dwnd.Visible) Util.DrawIcon(canvas, "fa-palette", 14, rtArrow, SKColors.Transparent, cText);
            else Util.DrawIcon(canvas, "fa-palette", 14, rtArrow, cText);
            #endregion
        }
        #endregion

        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            var rtValue = Areas()["Value"];

            if (CollisionTool.Check(rtValue, x, y))
            {
                var args = new GoCancelableEventArgs();
                DropDownOpening?.Invoke(this, args);

                if (!args.Cancel)
                {
                    var rt = rtValue; rt.Offset(ScreenX, ScreenY);

                    dwnd.Show(rt, FontName, FontStyle, FontSize, Value, (color) =>
                    {
                        if (color.HasValue)
                            Value = color.Value;
                    });
                }
            }
        }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtArrow) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["100%", "40px"]);
            return (rts[0], rts[1]);
        }
        #endregion
    }

    public class GoInputDateTime : GoInput
    {
        #region Properties
        [GoProperty(PCategory.Control, 16)]
        public DateTime Value
        {
            get => cValue;
            set
            {
                if (cValue != value)
                {
                    cValue = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [GoProperty(PCategory.Control, 17)] public GoDateTimeKind DateTimeStyle { get; set; } = GoDateTimeKind.DateTime;
        [GoProperty(PCategory.Control, 18)] public string DateFormat { get; set; } = "yyyy-MM-dd";
        [GoProperty(PCategory.Control, 19)] public string TimeFormat { get; set; } = "HH:mm:ss";
        #endregion

        #region Event
        public event EventHandler? ValueChanged;

        public event EventHandler<GoCancelableEventArgs>? DropDownOpening;
        #endregion

        #region Member Variable
        DateTime cValue = DateTime.Now;
        GoDateTimeDropDownWindow dwnd = new GoDateTimeDropDownWindow();
        #endregion

        #region OnDrawValue
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtArrow) = bounds(rtValue);

            #region item
            var format = $"{DateFormat} {TimeFormat}";
            switch (DateTimeStyle)
            {
                case GoDateTimeKind.DateTime: format = $"{DateFormat} {TimeFormat}"; break;
                case GoDateTimeKind.Time: format = TimeFormat; break;
                case GoDateTimeKind.Date: format = DateFormat; break;
            }
            var text = Value.ToString(format);

            Util.DrawText(canvas, text, FontName, FontStyle, FontSize, rtText, cText);
            #endregion

            #region sep
            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cBorder);
            p.PathEffect = pe;
            canvas.DrawLine(rtArrow.Left, rtValue.Top + 10, rtArrow.Left, rtValue.Bottom - 10, p);
            #endregion

            #region arrow
            var ico = "fa-calendar";
            switch (DateTimeStyle)
            {
                case GoDateTimeKind.DateTime: ico = "fa-calendar"; break;
                case GoDateTimeKind.Time: ico = "fa-clock"; break;
                case GoDateTimeKind.Date: ico = "fa-calendar"; break;
            }

            if (dwnd.Visible) Util.DrawIcon(canvas, ico, 14, rtArrow, SKColors.Transparent, cText);
            else Util.DrawIcon(canvas, ico, 14, rtArrow, cText);
            #endregion
        }
        #endregion

        #region OnValueClick
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            var rtValue = Areas()["Value"];

            if (CollisionTool.Check(rtValue, x, y))
            {
                var args = new GoCancelableEventArgs();
                DropDownOpening?.Invoke(this, args);

                if (!args.Cancel)
                {
                    var rt = rtValue; rt.Offset(ScreenX, ScreenY);

                    dwnd.Show(rt, FontName, FontStyle, FontSize, Value, DateTimeStyle, (datetime) =>
                    {
                        if (datetime.HasValue)
                            Value = datetime.Value;
                    });
                }
            }
        }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtArrow) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["100%", "40px"]);
            return (rts[0], rts[1]);
        }
        #endregion
    }
}
