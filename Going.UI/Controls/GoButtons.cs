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
    /// <summary>
    /// 여러 개의 버튼을 하나의 그룹으로 묶어 표시하는 다중 버튼 컨트롤. 일반 버튼, 토글, 라디오 모드를 지원합니다.
    /// </summary>
    public class GoButtons : GoControl
    {
        #region Properties
        /// <summary>아이콘 크기</summary>
        [GoProperty(PCategory.Control, 0)] public float IconSize { get; set; } = 12;
        /// <summary>아이콘 배치 방향</summary>
        [GoProperty(PCategory.Control, 1)] public GoDirectionHV IconDirection { get; set; }
        /// <summary>아이콘과 텍스트 사이 간격</summary>
        [GoProperty(PCategory.Control, 2)] public float IconGap { get; set; } = 5;
        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 3)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 4)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 5)] public float FontSize { get; set; } = 12;

        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 6)] public string TextColor { get; set; } = "Fore";
        /// <summary>버튼 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 7)] public string ButtonColor { get; set; } = "Base3";
        /// <summary>테두리 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 8)] public string BorderColor { get; set; } = "Base3";
        /// <summary>선택된 버튼 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 9)] public string SelectedButtonColor { get; set; } = "Select";
        /// <summary>선택된 버튼 테두리 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 8)] public string SelectedBorderColor { get; set; } = "Select";
        /// <summary>모서리 둥글기 유형</summary>
        [GoProperty(PCategory.Control, 10)] public GoRoundType Round { get; set; } = GoRoundType.All;
        /// <summary>테두리 두께</summary>
        [GoProperty(PCategory.Control, 11)] public float BorderWidth { get; set; } = 1F;
        /// <summary>버튼 채우기 스타일</summary>
        [GoProperty(PCategory.Control, 12)] public GoButtonFillStyle FillStyle { get; set; } = GoButtonFillStyle.Flat;

        /// <summary>버튼 항목 목록</summary>
        [GoProperty(PCategory.Control, 13)] public List<GoButtonsItem> Buttons { get; set; } = [];

        /// <summary>버튼 배치 방향 (가로/세로)</summary>
        [GoProperty(PCategory.Control, 14)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        /// <summary>버튼 동작 모드 (일반 버튼, 토글, 라디오)</summary>
        [GoProperty(PCategory.Control, 15)] public GoButtonsMode Mode { get; set; } = GoButtonsMode.Button;
        #endregion

        #region Event
        /// <summary>버튼이 클릭되었을 때 발생하는 이벤트</summary>
        public event EventHandler<ButtonsClickEventArgs>? ButtonClicked;
        /// <summary>선택 상태가 변경되었을 때 발생하는 이벤트</summary>
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
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            #region var
            #region color
            var cText = thm.ToColor(TextColor);
            var cButton = thm.ToColor(ButtonColor);
            var cSelButton = thm.ToColor(SelectedButtonColor);
            var cBorder = thm.ToColor(BorderColor);
            var cSelBorder = thm.ToColor(SelectedBorderColor);
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
                        var cBtn = (btn.Selected ? cSelButton : cButton).BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                        var cBor = (btn.Selected ? cSelBorder : cBorder).BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                        var cTxt = cText.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0);
                        rt.Inflate(1, 1);
                        Util.DrawButton(canvas, thm, rt, cBtn.BrightnessTransmit(btn.Hover ? thm.HoverFillBrightness : 0),
                                                         cBor.BrightnessTransmit(btn.Hover ? thm.HoverBorderBrightness : 0), rnd, thm.Corner, true, BorderWidth, FillStyle, btn.Down);

                        if (btn.Down) rt.Offset(0, 1);
                        Util.DrawTextIcon(canvas, btn.Text, FontName, FontStyle, FontSize, btn.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rt, cTxt, GoContentAlignment.MiddleCenter);
                    }
                });
                #endregion
            }
            else
            {
                Util.DrawBox(canvas, rtContent, cButton, Round, thm.Corner);
            }
            #endregion

            base.OnDraw(canvas, thm);
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
