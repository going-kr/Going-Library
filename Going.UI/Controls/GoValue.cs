using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Gudx;
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
    /// <summary>
    /// 값 표시 컨트롤의 추상 기본 클래스. 제목, 값 영역, 버튼 영역으로 구성됩니다.
    /// </summary>
    public abstract class GoValue : GoControl
    {
        #region Properties
        /// <summary>아이콘 문자열 (FontAwesome 등)</summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        /// <summary>아이콘 크기</summary>
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        /// <summary>아이콘과 텍스트 사이 간격</summary>
        [GoProperty(PCategory.Control, 2)] public float IconGap { get; set; } = 5;

        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 3)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 4)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 5)] public float FontSize { get; set; } = 12;

        /// <summary>레이아웃 배치 방향 (가로/세로)</summary>
        [GoProperty(PCategory.Control, 6)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 7)] public string TextColor { get; set; } = "Fore";
        /// <summary>테두리 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 8)] public string BorderColor { get; set; } = "Base3";
        /// <summary>테두리 두께</summary>
        [GoProperty(PCategory.Control, 19)] public float BorderWidth { get; set; } = 1F;
        /// <summary>제목/버튼 영역 채우기 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 9)] public string FillColor { get; set; } = "Base3";
        /// <summary>값 영역 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 10)] public string ValueColor { get; set; } = "Base2";
        /// <summary>모서리 둥글기 유형</summary>
        [GoProperty(PCategory.Control, 11)] public GoRoundType Round { get; set; } = GoRoundType.All;

        /// <summary>제목 영역 크기 (픽셀). null이면 제목 영역을 표시하지 않습니다.</summary>
        [GoProperty(PCategory.Control, 12)] public float? TitleSize { get; set; }
        /// <summary>제목 텍스트</summary>
        [GoProperty(PCategory.Control, 13)] public string? Title { get; set; }

        /// <summary>버튼 영역 크기 (픽셀). null이면 버튼 영역을 표시하지 않습니다.</summary>
        [GoProperty(PCategory.Control, 14)] public float? ButtonSize { get; set; }
        /// <summary>버튼 항목 목록</summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Control, 15)] public List<GoButtonItem> Buttons { get; set; } = [];

        /// <summary>자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 16)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;
        /// <summary>자동 아이콘 크기 설정</summary>
        [GoProperty(PCategory.Control, 17)] public GoAutoFontSize AutoIconSize { get; set; } = GoAutoFontSize.NotUsed;

        /// <summary>Title 영역의 배경 박스(FillColor) 그리기 여부. false면 텍스트만 표시되고 외곽 round 가 Value 영역으로 흡수됨.</summary>
        [GoProperty(PCategory.Control, 18)] public bool TitleBoxDraw { get; set; } = true;

        /// <summary>Title 영역 글꼴 크기. AutoTitleFontSize=NotUsed일 때 사용됩니다.</summary>
        [GoProperty(PCategory.Control, 20)] public float TitleFontSize { get; set; } = 12F;
        /// <summary>Title 텍스트/아이콘 정렬</summary>
        [GoProperty(PCategory.Control, 21)] public GoContentAlignment TitleContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        /// <summary>Title 영역 자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 22)] public GoAutoFontSize AutoTitleFontSize { get; set; } = GoAutoFontSize.NotUsed;

        [JsonIgnore] private bool UseTitle => (int)(TitleSize ?? 0) > 0;
        [JsonIgnore] private bool UseButton => (int)(ButtonSize ?? 0) > 0 && Buttons.Count > 0;
        #endregion

        #region Member Variable
        private bool bValueDown = false;
        #endregion

        #region Constructor
        /// <summary>GoValue 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoValue()
        {
            Selectable = true;
        }
        #endregion

        #region Event
        /// <summary>버튼이 클릭되었을 때 발생하는 이벤트</summary>
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked;
        /// <summary>값 영역이 클릭되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ValueClicked;
        #endregion

        #region Override
        #region OnDraw
        /// <inheritdoc/>
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
            // titleVisible: Title 박스가 외곽 round + Border 의 일부로 포함되는 케이스.
            // TitleBoxDraw=false 면 Title 영역은 박스/Border 모두 제외 — Value(+Button) 만 외곽.
            var titleVisible = useT && TitleBoxDraw;
            var rnds = Util.Rounds(Direction, Round, (titleVisible ? 1 : 0) + 1 + (useB ? 1 : 0));
            var rndTitle = titleVisible ? rnds[0] : GoRoundType.Rect;
            var rndValue = titleVisible ? rnds[1] : rnds[0];
            var rndButton = useB ? rnds[rnds.Length - 1] : GoRoundType.Rect;
            var rndButtons = Util.Rounds(GoDirectionHV.Horizon, rndButton, (useB ? Buttons.Count : 0));

            #endregion

            var fsz = Util.FontSize(AutoFontSize, rtBox.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtBox.Height) ?? IconSize;

            #endregion

            #region Title
            if (useT)
            {
                // TitleBoxDraw=false 시 Title 영역은 fill/Border 모두 제외, 텍스트만 표시.
                if (TitleBoxDraw)
                    Util.DrawBox(canvas, rtTitle, cFill, rndTitle, thm.Corner);
                var tfsz = Util.FontSize(AutoTitleFontSize, rtTitle.Height) ?? TitleFontSize;
                Util.DrawTextIcon(canvas, Title, FontName, FontStyle, tfsz, IconString, isz, GoDirectionHV.Horizon, IconGap, rtTitle, cText, TitleContentAlignment);
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
                    Util.DrawTextIcon(canvas, btn.Text, FontName, FontStyle, fsz, btn.IconString, isz, GoDirectionHV.Horizon, IconGap, rt, cTxt, GoContentAlignment.MiddleCenter);
                });
            }
            #endregion

            #region Border
            // titleVisible=false 시 Title 영역 제외하고 Border. Value(+Button) 영역만 외곽 round.
            // useT 면서 TitleBoxDraw=false 인 케이스 — Title 영역의 Border 도 빠짐.
            if (useT && !TitleBoxDraw)
            {
                var rtBorder = useB ? SKRect.Union(rtValue, rtButton) : rtValue;
                Util.DrawBox(canvas, rtBorder, SKColors.Transparent, cBorder, Round, thm.Corner, true, BorderWidth);
            }
            else
            {
                Util.DrawBox(canvas, rtBox, SKColors.Transparent, cBorder, Round, thm.Corner, true, BorderWidth);
            }
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <summary>값 영역을 그립니다. 파생 클래스에서 값의 표현 방식을 구현합니다.</summary>
        /// <param name="canvas">그리기 캔버스</param>
        /// <param name="thm">현재 테마</param>
        /// <param name="valueBounds">값 영역의 경계</param>
        protected abstract void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect valueBounds);
        #endregion

        #region Method
        void buttonLoop(Action<int, GoButtonItem, SKRect> act)
        {
            var rtButton = Areas()["Button"];
            var titleVisible = UseTitle && TitleBoxDraw;
            var rnds = Util.Rounds(Direction, Round, (titleVisible ? 1 : 0) + 1 + (UseButton ? 1 : 0));
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

    /// <summary>
    /// 문자열 값을 표시하는 컨트롤
    /// </summary>
    public class GoValueString : GoValue
    {
        #region Properties
        /// <summary>표시할 문자열 값</summary>
        [GoProperty(PCategory.Control, 18)] public string? Value { get; set; }
        #endregion

        #region OnDrawValue
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            var cText = thm.ToColor(TextColor);

            var fsz = Util.FontSize(AutoFontSize, rtValue.Height) ?? FontSize;

            Util.DrawText(canvas, Value, FontName, FontStyle, fsz, rtValue, cText);
        }
        #endregion
    }

    /// <summary>
    /// 숫자 값을 표시하는 컨트롤. 단위 표시를 지원합니다.
    /// </summary>
    /// <typeparam name="T">숫자 자료형 (sbyte, short, int, long, byte, ushort, uint, ulong, float, double, decimal)</typeparam>
    [GoNumericAlias]
    public class GoValueNumber<T> : GoValue where T : struct
    {
        #region Properties
        /// <summary>표시할 숫자 값</summary>
        [GoProperty(PCategory.Control, 18)] public T Value { get; set; }
        /// <summary>값 표시 형식 문자열</summary>
        [GoProperty(PCategory.Control, 19)] public string? FormatString { get; set; } = null;

        /// <summary>단위 텍스트</summary>
        [GoProperty(PCategory.Control, 20)] public string? Unit { get; set; }
        /// <summary>단위 글꼴 크기</summary>
        [GoProperty(PCategory.Control, 21)] public float UnitFontSize { get; set; } = 12;
        /// <summary>단위 표시 영역 크기 (픽셀). null이면 단위를 표시하지 않습니다.</summary>
        [GoProperty(PCategory.Control, 22)] public float? UnitSize { get; set; } = null;

        /// <summary>자동 단위 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 23)] public GoAutoFontSize AutoUnitFontSize { get; set; } = GoAutoFontSize.NotUsed;

        #endregion

        #region Constructor
        /// <summary>GoValueNumber 클래스의 새 인스턴스를 초기화합니다.</summary>
        /// <exception cref="Exception">지원하지 않는 숫자 자료형일 경우 발생합니다.</exception>
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
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtUnit) = bounds(rtValue);

            var fsz = Util.FontSize(AutoFontSize, rtValue.Height) ?? FontSize;
            var ufsz = Util.FontSize(AutoUnitFontSize, rtValue.Height) ?? UnitFontSize;

            var text = ValueTool.ToString(Value, FormatString) ?? "";
            Util.DrawText(canvas, text, FontName, FontStyle, fsz, rtText, cText);

            if (UnitSize.HasValue)
            {
                Util.DrawText(canvas, Unit, FontName, FontStyle, ufsz, rtUnit, cText);

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
     
    /// <summary>
    /// 불리언(ON/OFF) 값을 표시하는 컨트롤
    /// </summary>
    public class GoValueBoolean : GoValue
    {
        #region Properties
        /// <summary>표시할 불리언 값</summary>
        [GoProperty(PCategory.Control, 18)] public bool Value { get; set; }

        /// <summary>On 상태 텍스트</summary>
        [GoProperty(PCategory.Control, 19)] public string? OnText { get; set; } = "ON";
        /// <summary>Off 상태 텍스트</summary>
        [GoProperty(PCategory.Control, 20)] public string? OffText { get; set; } = "OFF";
        /// <summary>On 상태 아이콘 문자열</summary>
        [GoProperty(PCategory.Control, 21)] public string? OnIconString { get; set; }
        /// <summary>Off 상태 아이콘 문자열</summary>
        [GoProperty(PCategory.Control, 22)] public string? OffIconString { get; set; }
        #endregion

        #region OnDrawValue
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cTextL = thm.ToColor(TextColor);
            var cTextD = Util.FromArgb(80, cTextL);
            var (rtOn, rtOff) = bounds(rtValue);

            var fsz = Util.FontSize(AutoFontSize, rtValue.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtValue.Height) ?? IconSize;


            Util.DrawTextIcon(canvas, OnText, FontName, FontStyle, fsz, OnIconString, isz, GoDirectionHV.Horizon, IconGap, rtOn, Value ? cTextL : cTextD);
            Util.DrawTextIcon(canvas, OffText, FontName, FontStyle, fsz, OffIconString, isz, GoDirectionHV.Horizon, IconGap, rtOff, Value ? cTextD : cTextL);

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
