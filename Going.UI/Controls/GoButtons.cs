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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Controls
{
    public class GoButtons : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Misc, 0)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Misc, 1)] public GoDirectionHV IconDirection { get; set; }
        [GoProperty(PCategory.Misc, 2)] public float IconGap { get; set; } = 5;
        [GoProperty(PCategory.Misc, 3)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Misc, 4)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Misc, 5)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Misc, 6)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Misc, 7)] public string ButtonColor { get; set; } = "Base3";
        [GoProperty(PCategory.Misc, 8)] public string SelectedButtonColor { get; set; } = "Select";
        [GoProperty(PCategory.Misc, 9)] public GoRoundType Round { get; set; } = GoRoundType.All;

        [GoProperty(PCategory.Misc, 10)] public List<GoButtonsItem> Buttons { get; set; } = [];

        [GoProperty(PCategory.Misc, 11)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        [GoProperty(PCategory.Misc, 12)] public GoButtonsMode Mode { get; set; } = GoButtonsMode.Button;
        #endregion

        #region Event
        public event EventHandler<ButtonsClickEventArgs>? ButtonClicked;
        public event EventHandler<ButtonsSelectedEventArgs>? SelectedChanged;
        #endregion

        #region Constructor
        public GoButtons()
        {
            Selectable = true;
        }
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;
            #region color
            var cText = thm.ToColor(TextColor);
            var cButton = thm.ToColor(ButtonColor);
            var cSelButton = thm.ToColor(SelectedButtonColor);
            #endregion
            #region bounds
            var rts = Areas();
            var rtContent = rts["Content"];
            #endregion
            #region round
            var rnds = Util.Rounds(Direction, Round, Math.Max(1, Buttons.Count));
            #endregion
            #endregion

            #region Button
            if (Buttons.Count > 0)
            {
                #region Buttons
                buttonLoop((i, btn, rt) =>
                {
                    var rnd = rnds[i];
                    var cBtn = (btn.Selected ? cSelButton : cButton).BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                    var cTxt = cText.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                    Util.DrawBox(canvas, rt, cBtn.BrightnessTransmit(btn.Hover ? thm.HoverFillBrightness : 0), rnd, thm.Corner);

                    if (i > 0 && !btn.Hover && i - 1 >= 0 && !Buttons[i - 1].Hover)
                    {
                        using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 1, Color = cBtn.BrightnessTransmit(thm.BorderBrightness) };
                        using var pe = SKPathEffect.CreateDash([2, 2], 2);
                        float x = Convert.ToInt32(rt.Left); x += 0.5F;
                        p.PathEffect = pe;
                        canvas.DrawLine(x, rt.Top + 5, x, rt.Bottom - 5, p);
                    }

                    if (btn.Down) rt.Offset(0, 1);
                    Util.DrawTextIcon(canvas, btn.Text, FontName, FontStyle, FontSize, btn.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rt, cTxt, GoContentAlignment.MiddleCenter);
                });
                #endregion
                #region Border
                buttonLoop((i, btn, rt) =>
                {
                    if (btn.Hover)
                    {
                        var rnd = rnds[i];
                        var cBtn = cButton.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                        var cTxt = cText.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                        Util.DrawBox(canvas, rt, SKColors.Transparent, cBtn.BrightnessTransmit(btn.Hover ? thm.HoverBorderBrightness : 0), rnd, thm.Corner);
                    }
                });
                #endregion
            }
            else
            {
                Util.DrawBox(canvas, rtContent, cButton, Round, thm.Corner);
            }
            #endregion

            base.OnDraw(canvas);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            if (Buttons.Count > 0)
            {
                buttonLoop((i, btn, rt) =>
                {
                    if (CollisionTool.Check(rt, x, y)) btn.Down = true;
                });
            }

            base.OnMouseDown(x, y, button);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            if (Buttons.Count > 0)
            {
                buttonLoop((i, btn, rt) =>
                {
                    if (btn.Down)
                    {
                        btn.Down = false;

                        if (CollisionTool.Check(rt, x, y))
                        {
                            if (Mode == GoButtonsMode.Button)
                                ButtonClicked?.Invoke(this, new ButtonsClickEventArgs(btn));
                            else if (Mode == GoButtonsMode.Toggle)
                            {
                                btn.Selected = !btn.Selected;
                                SelectedChanged?.Invoke(this, new ButtonsSelectedEventArgs(btn, null));
                            }
                            else if (Mode == GoButtonsMode.Radio)
                            {
                                var old = Buttons.FirstOrDefault(x => x.Selected);
                                if (old != null) old.Selected = false;

                                btn.Selected = !btn.Selected;
                                SelectedChanged?.Invoke(this, new ButtonsSelectedEventArgs(btn, old));
                            }
                        }
                    }
                });
            }

            base.OnMouseUp(x, y, button);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
            if (Buttons.Count > 0)
            {
                buttonLoop((i, btn, rt) =>
                {
                    btn.Hover = CollisionTool.Check(rt, x, y);
                });
            }

            base.OnMouseMove(x, y);
        }
        #endregion
        #endregion

        #region Method
        void buttonLoop(Action<int, GoButtonsItem, SKRect> act)
        {
            if (Buttons.Count > 0)
            {
                var rtButton = Areas()["Content"];
                var rtsBtn = Util.Columns(rtButton, Buttons.Select(x => x.Size).ToArray());

                for (int i = 0; i < Buttons.Count; i++)
                {
                    var btn = Buttons[i];
                    var rt = rtsBtn[i];

                    act(i, btn, rt);
                }
            }
        }
        #endregion
    }
}
