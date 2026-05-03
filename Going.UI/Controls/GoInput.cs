using Going.UI.Datas;
using Going.UI.Design;
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Controls
{
    /// <summary>
    /// 입력 컨트롤의 추상 기본 클래스. 제목, 값 영역, 버튼 영역으로 구성됩니다.
    /// </summary>
    public abstract class GoInput : GoControl
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
        [GoProperty(PCategory.Control, 10)] public string ValueColor { get; set; } = "Base1";
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
        /// <summary>Title 영역 텍스트(+아이콘) 패딩</summary>
        [GoProperty(PCategory.Control, 23)] public GoPadding TitleTextPadding { get; set; } = new GoPadding(0, 0, 0, 0);

        /// <summary>입력 값의 유효성 여부</summary>
        [JsonIgnore] public virtual bool Valid => true;
        /// <summary>키보드 입력을 사용하는지 여부</summary>
        [JsonIgnore] public virtual bool IsKeyboardInput => false;
        /// <summary>입력 모드에서 텍스트를 숨길지 여부 (내부 사용)</summary>
        [JsonIgnore, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)] public bool _InputModeInvisibleText_ { get; set; } = false;
        [JsonIgnore] private bool UseTitle => (int)(TitleSize ?? 0) > 0;
        [JsonIgnore] private bool UseButton => (int)(ButtonSize ?? 0) > 0 && Buttons.Count > 0;
        #endregion

        #region Member Variable
        private bool bValueDown = false;
        #endregion

        #region Constructor
        /// <summary><see cref="GoInput"/> 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoInput()
        {
            Selectable = true;
        }
        #endregion

        #region Event
        /// <summary>버튼이 클릭되었을 때 발생하는 이벤트</summary>
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked;
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
                if (TitleBoxDraw)
                    Util.DrawBox(canvas, rtTitle, cFill, rndTitle, thm.Corner);
                var rtTitleText = Util.FromRect(rtTitle, TitleTextPadding);
                var tfsz = Util.FontSize(AutoTitleFontSize, rtTitleText.Height) ?? TitleFontSize;
                Util.DrawTextIcon(canvas, Title, FontName, FontStyle, tfsz, IconString, isz, GoDirectionHV.Horizon, IconGap, rtTitleText, cText, TitleContentAlignment);
            }
            #endregion

            #region Value
            Util.DrawBox(canvas, rtValue, cValue, rndValue, thm.Corner);
            if (!_InputModeInvisibleText_) OnDrawValue(canvas, thm, rtValue);
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
                    Util.DrawTextIcon(canvas, btn.Text, FontName, FontStyle, fsz, btn.IconString, isz, GoDirectionHV.Horizon, IconGap, rt, cTxt, GoContentAlignment.MiddleCenter);
                });
            }
            #endregion

            #region Border
            // titleVisible=false 시 Title 영역 제외하고 Border. Value(+Button) 영역만 외곽 round.
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

            if (GoInputEventer.Current.InputControl == this) Util.DrawBox(canvas, rtValue, SKColors.Transparent, cInput, rndValue, thm.Corner);
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
                if (CollisionTool.Check(rtValue, x, y)) OnValueClick(x, y, button);
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
        /// <summary>값 영역을 그립니다. 파생 클래스에서 구현합니다.</summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="thm">테마</param>
        /// <param name="valueBounds">값 영역 경계</param>
        protected abstract void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect valueBounds);
        /// <summary>값 영역이 클릭되었을 때 호출됩니다. 파생 클래스에서 구현합니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">클릭한 마우스 버튼</param>
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

    /// <summary>
    /// 문자열 입력 컨트롤. 키보드를 통해 텍스트를 입력받습니다.
    /// </summary>
    public class GoInputString : GoInput
    {
        #region Properties
        private string sVal = "";
        /// <summary>입력 값</summary>
        [GoProperty(PCategory.Control, 18)]
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

        /// <summary>키보드 입력 사용 여부</summary>
        [JsonIgnore] public override bool IsKeyboardInput => true;
        #endregion

        #region Event
        /// <summary>값이 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ValueChanged;

        /// <summary>값 편집 시 호출되는 필터 이벤트. false를 반환하면 값 변경이 거부됩니다.</summary>
        public event Func<string, bool>? Editing;
        #endregion

        #region OnDrawValue
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            var cText = thm.ToColor(TextColor);

            var fsz = Util.FontSize(AutoFontSize, rtValue.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtValue.Height) ?? IconSize;


            Util.DrawText(canvas, Value, FontName, FontStyle, fsz, rtValue, cText);

        }
        #endregion

        #region OnValueClick
        /// <inheritdoc/>
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            GoInputEventer.Current.FireInputString(this, Areas()["Value"], (s) => {
                if (Editing?.Invoke(s) ?? true) Value = s;
            }, Value);
        }
        #endregion
    }

    /// <summary>
    /// 숫자 입력 컨트롤. 최솟값, 최댓값, 단위 표시를 지원합니다.
    /// </summary>
    /// <typeparam name="T">숫자 자료형 (sbyte, short, int, long, byte, ushort, uint, ulong, float, double, decimal)</typeparam>
    [GoNumericAlias]
    public class GoInputNumber<T> : GoInput where T : struct
    {
        #region Properties
        private T sVal = default(T);
        /// <summary>입력 값</summary>
        [GoProperty(PCategory.Control, 18)]
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

        /// <summary>최솟값. null이면 제한 없음.</summary>
        [GoProperty(PCategory.Control, 19)] public T? Minimum { get; set; } = null;
        /// <summary>최댓값. null이면 제한 없음.</summary>
        [GoProperty(PCategory.Control, 20)] public T? Maximum { get; set; } = null;

        /// <summary>값 표시 형식 문자열</summary>
        [GoProperty(PCategory.Control, 21)] public string? FormatString { get; set; } = null;

        /// <summary>입력 값의 유효성 여부</summary>
        [JsonIgnore] public override bool Valid => bValid;
        /// <summary>키보드 입력 사용 여부</summary>
        [JsonIgnore] public override bool IsKeyboardInput => true;

        /// <summary>단위 텍스트</summary>
        [GoProperty(PCategory.Control, 22)] public string? Unit { get; set; }
        /// <summary>단위 표시 영역 크기 (픽셀). null이면 단위를 표시하지 않습니다.</summary>
        [GoProperty(PCategory.Control, 23)] public float? UnitSize { get; set; } = null;
        #endregion

        #region Event
        /// <summary>값이 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ValueChanged;

        /// <summary>값 편집 시 호출되는 필터 이벤트. false를 반환하면 값 변경이 거부됩니다.</summary>
        public event Func<T, bool>? Editing;
        #endregion

        #region Member Variable
        bool bValid = true;
        T valOrigin;
        #endregion

        #region Constructor
        /// <summary><see cref="GoInputNumber{T}"/> 클래스의 새 인스턴스를 초기화합니다.</summary>
        /// <exception cref="Exception">지원하지 않는 자료형 T인 경우 발생합니다.</exception>
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
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtUnit) = bounds(rtValue);

            var fsz = Util.FontSize(AutoFontSize, rtValue.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtValue.Height) ?? IconSize;

            var text = ValueTool.ToString(Value, FormatString) ?? "";
            Util.DrawText(canvas, text, FontName, FontStyle, fsz, rtText, cText);

            if(UnitSize.HasValue)
            {
                Util.DrawText(canvas, Unit, FontName, FontStyle, fsz, rtUnit, cText);

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
        /// <inheritdoc/>
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

                T val;
                if (v != null)
                {
                    if (bValid) val = (T)v;
                    else val = ValueTool.Constrain((T)v, Minimum, Maximum);
                }
                else val = valOrigin;

                if (Editing?.Invoke(val) ?? true) Value = val;

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

    /// <summary>
    /// 불리언(ON/OFF) 입력 컨트롤. On/Off 영역을 클릭하여 값을 전환합니다.
    /// </summary>
    public class GoInputBoolean : GoInput
    {
        #region Properties
        private bool sVal = false;
        /// <summary>입력 값</summary>
        [GoProperty(PCategory.Control, 18)]
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

        /// <summary>On 상태 텍스트</summary>
        [GoProperty(PCategory.Control, 19)] public string? OnText { get; set; } = "ON";
        /// <summary>Off 상태 텍스트</summary>
        [GoProperty(PCategory.Control, 20)] public string? OffText { get; set; } = "OFF";
        /// <summary>On 상태 아이콘 문자열</summary>
        [GoProperty(PCategory.Control, 21)] public string? OnIconString { get; set; }
        /// <summary>Off 상태 아이콘 문자열</summary>
        [GoProperty(PCategory.Control, 22)] public string? OffIconString { get; set; }
        #endregion

        #region Event
        /// <summary>값이 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ValueChanged;

        /// <summary>값 편집 시 호출되는 필터 이벤트. false를 반환하면 값 변경이 거부됩니다.</summary>
        public event Func<bool, bool>? Editing;
        #endregion

        #region Member Variable
        private Animation ani = new Animation();
        #endregion

        #region Constructor
        /// <summary><see cref="GoInputBoolean"/> 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoInputBoolean()
        {
            ani.Refresh = () =>
            Invalidate();
        }
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

            if (Animation.UseAnimation && ani.IsPlaying)
            {
                var cOn = ani.Variable == "ON" ? ani.Value(AnimationAccel.Linear, cTextD, cTextL) : ani.Value(AnimationAccel.Linear, cTextL, cTextD);
                var cOff = ani.Variable == "ON" ? ani.Value(AnimationAccel.Linear, cTextL, cTextD) : ani.Value(AnimationAccel.Linear, cTextD, cTextL);

                Util.DrawTextIcon(canvas, OnText, FontName, FontStyle, fsz, OnIconString, isz, GoDirectionHV.Horizon, IconGap, rtOn, cOn);
                Util.DrawTextIcon(canvas, OffText, FontName, FontStyle, fsz, OffIconString, isz, GoDirectionHV.Horizon, IconGap, rtOff, cOff);

            }
            else
            {
                Util.DrawTextIcon(canvas, OnText, FontName, FontStyle, fsz, OnIconString, isz, GoDirectionHV.Horizon, IconGap, rtOn, Value ? cTextL : cTextD);
                Util.DrawTextIcon(canvas, OffText, FontName, FontStyle, fsz, OffIconString, isz, GoDirectionHV.Horizon, IconGap, rtOff, Value ? cTextD : cTextL);
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
        /// <inheritdoc/>
        protected override void OnValueClick(float x, float y, GoMouseButton button)
        {
            var rtValue = Areas()["Value"];
            var (rtOn, rtOff) = bounds(rtValue);

            bool val = Value;
            if (CollisionTool.Check(rtOn, x, y)) val = true;
            else if (CollisionTool.Check(rtOff, x, y)) val = false;

            if (Editing?.Invoke(val) ?? true) Value = val;
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

    /// <summary>
    /// 콤보박스(드롭다운) 입력 컨트롤. 목록에서 항목을 선택합니다.
    /// </summary>
    // 콤보BOX에 Input이 들어갈 수 있음
    public class GoInputCombo : GoInput
    {
        #region Properties
        /// <summary>콤보박스 항목 목록</summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Control, 18)] public List<GoListItem> Items { get; set; } = [];

        private int nSelIndex = -1;
        /// <summary>선택된 항목의 인덱스. -1이면 선택된 항목 없음.</summary>
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

        /// <summary>선택된 항목. 선택된 항목이 없으면 null.</summary>
        [JsonIgnore] public GoListItem? SelectedItem => SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;

        /// <summary>드롭다운 항목 높이 (픽셀)</summary>
        [GoProperty(PCategory.Control, 19)] public int ItemHeight { get; set; } = 30;
        /// <summary>드롭다운에 표시할 최대 항목 수</summary>
        [GoProperty(PCategory.Control, 20)] public int MaximumViewCount { get; set; } = 8;
        #endregion

        #region Event
        /// <summary>선택된 인덱스가 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? SelectedIndexChanged;

        /// <summary>드롭다운이 열리기 전에 발생하는 이벤트. Cancel을 true로 설정하면 드롭다운이 열리지 않습니다.</summary>
        public event EventHandler<GoCancelableEventArgs>? DropDownOpening;
        #endregion

        #region Member Variable
        GoComboBoxDropDownWindow dwnd = new GoComboBoxDropDownWindow();
        #endregion

        #region OnDrawValue
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtArrow) = bounds(rtValue);

            var fsz = Util.FontSize(AutoFontSize, rtText.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtText.Height) ?? IconSize;

            #region item
            if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
            {
                var item = Items[SelectedIndex];
                Util.DrawTextIcon(canvas, item.Text, FontName, FontStyle, fsz, item.IconString, isz, GoDirectionHV.Horizon, IconGap, rtText, cText);
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
        /// <inheritdoc/>
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

    /// <summary>
    /// 좌우 화살표로 항목을 선택하는 셀렉터 입력 컨트롤.
    /// </summary>
    public class GoInputSelector : GoInput
    {
        #region Properties
        /// <summary>선택 가능한 항목 목록</summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Control, 18)] public List<GoListItem> Items { get; set; } = [];

        private int nSelIndex = -1;
        /// <summary>선택된 항목의 인덱스. -1이면 선택된 항목 없음.</summary>
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

        /// <summary>선택된 항목. 선택된 항목이 없으면 null.</summary>
        [JsonIgnore] public GoListItem? SelectedItem => SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;
        #endregion

        #region Event
        /// <summary>선택된 인덱스가 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? SelectedIndexChanged;
        #endregion

        #region Member Variable
        bool bDownL = false, bHoverL = false;
        bool bDownR = false, bHoverR = false;

        Animation ani = new Animation();
        #endregion

        #region Constructor
        /// <summary><see cref="GoInputSelector"/> 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoInputSelector()
        {
            ani.Refresh = () => Invalidate();
        }
        #endregion

        #region Override
        #region OnDrawValue
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtArrowL, rtArrowR) = bounds(rtValue);

            var fsz = Util.FontSize(AutoFontSize, rtText.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtText.Height) ?? IconSize;

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
                                Util.DrawTextIcon(canvas, itmP.Text, FontName, FontStyle, fsz, itmP.IconString, isz, GoDirectionHV.Horizon, IconGap, rtP, cP);
                            }

                            var rtN = Util.FromRect(rtText.Left + ani.Value(AnimationAccel.DCL, rtText.Width, 0), rtText.Top, rtText.Width, rtText.Height);
                            var itmN = Items[selidx];
                            var cN = cText.WithAlpha(Convert.ToByte(ani.Value(AnimationAccel.DCL, 0, 255)));
                            Util.DrawTextIcon(canvas, itmN.Text, FontName, FontStyle, fsz, itmN.IconString, isz, GoDirectionHV.Horizon, IconGap, rtN, cN);
                        }
                        else if(dir == "Prev")
                        {
                            if (selidx + 1 < Items.Count)
                            {
                                var itmP = Items[selidx + 1];
                                var rtP = Util.FromRect(rtText.Left + ani.Value(AnimationAccel.DCL, 0, rtText.Width), rtText.Top, rtText.Width, rtText.Height);
                                var cP = cText.WithAlpha(Convert.ToByte(ani.Value(AnimationAccel.DCL, 255, 0)));
                                Util.DrawTextIcon(canvas, itmP.Text, FontName, FontStyle, fsz, itmP.IconString, isz, GoDirectionHV.Horizon, IconGap, rtP, cP);
                            }

                            var itmN = Items[selidx];
                            var rtN = Util.FromRect(rtText.Left + ani.Value(AnimationAccel.DCL, -rtText.Width, 0), rtText.Top, rtText.Width, rtText.Height);
                            var cN = cText.WithAlpha(Convert.ToByte(ani.Value(AnimationAccel.DCL, 0, 255)));
                            Util.DrawTextIcon(canvas, itmN.Text, FontName, FontStyle, fsz, itmN.IconString, isz, GoDirectionHV.Horizon, IconGap, rtN, cN);
                        }

                    }
                }
                else
                {
                    if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
                    {
                        var item = Items[SelectedIndex];
                        Util.DrawTextIcon(canvas, item.Text, FontName, FontStyle, fsz, item.IconString, isz, GoDirectionHV.Horizon, IconGap, rtText, cText);
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
        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            base.OnMouseDown(x, y, button);
            var (rtText, rtArrowL, rtArrowR) = bounds(Areas()["Value"]);


            if (CollisionTool.Check(rtArrowL, x, y)) bDownL = true;
            if (CollisionTool.Check(rtArrowR, x, y)) bDownR = true;
        }
        #endregion
        #region OnMouseUp
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);
            var (rtText, rtArrowL, rtArrowR) = bounds(Areas()["Value"]);
            bHoverL = CollisionTool.Check(rtArrowL, x, y);
            bHoverR = CollisionTool.Check(rtArrowR, x, y);
        }
        #endregion
        #region OnValueClick
        /// <inheritdoc/>
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

    /// <summary>
    /// 색상 입력 컨트롤. 드롭다운을 통해 색상을 선택합니다.
    /// </summary>
    public class GoInputColor : GoInput
    {
        #region Properties
        /// <summary>선택된 색상 값</summary>
        [GoProperty(PCategory.Control, 18)]
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
        /// <summary>값이 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ValueChanged;

        /// <summary>드롭다운이 열리기 전에 발생하는 이벤트. Cancel을 true로 설정하면 드롭다운이 열리지 않습니다.</summary>
        public event EventHandler<GoCancelableEventArgs>? DropDownOpening;
        #endregion

        #region Member Variable
        SKColor cValue = SKColors.White;
        GoColorDropDownWindow dwnd = new GoColorDropDownWindow();
        #endregion

        #region OnDrawValue
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtArrow) = bounds(rtValue);

            var fsz = Util.FontSize(AutoFontSize, rtText.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtText.Height) ?? IconSize;

            #region item
            var vsz = fsz;
            var text = $"#{Value.Red:X2}{Value.Green:X2}{Value.Blue:X2}";
            var (rtIco, rtVal) = Util.TextIconBounds(text, FontName, FontStyle, fsz, new SKSize(vsz, vsz), GoDirectionHV.Horizon, IconGap, rtText, GoContentAlignment.MiddleCenter);

            Util.DrawText(canvas, text, FontName, FontStyle, fsz, rtVal, cText);
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
        /// <inheritdoc/>
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

    /// <summary>
    /// 날짜/시간 입력 컨트롤. 드롭다운을 통해 날짜 또는 시간을 선택합니다.
    /// </summary>
    public class GoInputDateTime : GoInput
    {
        #region Properties
        /// <summary>선택된 날짜/시간 값</summary>
        [GoProperty(PCategory.Control, 18)]
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

        /// <summary>날짜/시간 표시 스타일 (날짜만/시간만/날짜+시간)</summary>
        [GoProperty(PCategory.Control, 19)] public GoDateTimeKind DateTimeStyle { get; set; } = GoDateTimeKind.DateTime;
        /// <summary>날짜 표시 형식 문자열</summary>
        [GoProperty(PCategory.Control, 20)] public string DateFormat { get; set; } = "yyyy-MM-dd";
        /// <summary>시간 표시 형식 문자열</summary>
        [GoProperty(PCategory.Control, 21)] public string TimeFormat { get; set; } = "HH:mm:ss";
        #endregion

        #region Event
        /// <summary>값이 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ValueChanged;

        /// <summary>드롭다운이 열리기 전에 발생하는 이벤트. Cancel을 true로 설정하면 드롭다운이 열리지 않습니다.</summary>
        public event EventHandler<GoCancelableEventArgs>? DropDownOpening;
        #endregion

        #region Member Variable
        DateTime cValue = DateTime.Now;
        GoDateTimeDropDownWindow dwnd = new GoDateTimeDropDownWindow();
        #endregion

        #region OnDrawValue
        /// <inheritdoc/>
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtValue)
        {
            using var p = new SKPaint { IsAntialias = false };

            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var (rtText, rtArrow) = bounds(rtValue);

            var fsz = Util.FontSize(AutoFontSize, rtText.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtText.Height) ?? IconSize;

            #region item
            var format = $"{DateFormat} {TimeFormat}";
            switch (DateTimeStyle)
            {
                case GoDateTimeKind.DateTime: format = $"{DateFormat} {TimeFormat}"; break;
                case GoDateTimeKind.Time: format = TimeFormat; break;
                case GoDateTimeKind.Date: format = DateFormat; break;
            }
            var text = Value.ToString(format);

            Util.DrawText(canvas, text, FontName, FontStyle, fsz, rtText, cText);
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
        /// <inheritdoc/>
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
