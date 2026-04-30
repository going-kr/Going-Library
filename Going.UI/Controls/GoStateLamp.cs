using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Going.UI.Controls
{
    /// <summary>
    /// 상태 값(State)에 따라 서로 다른 색상/텍스트/On-Off 톤을 표시하는 다중 상태 램프 컨트롤.
    /// IcState 가 이미지로 상태를 표현하는 반면, GoStateLamp 는 테마 색상 + 텍스트로 표현합니다.
    /// 각 상태 항목(<see cref="StateLamp"/>)이 자체 OnOff 플래그를 가져 같은 색상에서도 활성/비활성 톤을 표현합니다.
    /// </summary>
    public class GoStateLamp : GoControl
    {
        #region Properties
        /// <summary>
        /// 상태별 (색상/텍스트/OnOff) 정의 목록. <see cref="State"/> 와 일치하는 항목이 표시됩니다.
        /// </summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Control, 0)] public List<StateLamp> States { get; set; } = [];

        private int bState = 0;
        /// <summary>
        /// 현재 상태 값. <see cref="States"/> 에서 일치하는 항목을 찾아 색상/텍스트/OnOff 톤으로 표시합니다.
        /// 일치하는 항목이 없으면 "Base3" 색상 OFF 톤 + "Not Matched" 텍스트로 표시합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)]
        public int State
        {
            get => bState;
            set
            {
                if (bState != value)
                {
                    bState = value;
                    StateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>글꼴 이름을 가져오거나 설정합니다.</summary>
        [GoFontNameProperty(PCategory.Control, 2)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일을 가져오거나 설정합니다.</summary>
        [GoProperty(PCategory.Control, 3)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기를 가져오거나 설정합니다.</summary>
        [GoProperty(PCategory.Control, 4)] public float FontSize { get; set; } = 12;
        /// <summary>텍스트 색상의 테마 색상 이름을 가져오거나 설정합니다.</summary>
        [GoProperty(PCategory.Control, 5)] public string TextColor { get; set; } = "Fore";

        /// <summary>램프 아이콘 크기(픽셀)를 가져오거나 설정합니다.</summary>
        [GoProperty(PCategory.Control, 6)] public int LampSize { get; set; } = 24;
        /// <summary>램프 아이콘과 텍스트 사이의 간격을 가져오거나 설정합니다.</summary>
        [GoProperty(PCategory.Control, 7)] public int Gap { get; set; } = 10;
        /// <summary>콘텐츠 정렬 방식을 가져오거나 설정합니다.</summary>
        [GoProperty(PCategory.Control, 8)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        /// <summary>자동 글꼴 크기 조절 모드를 가져오거나 설정합니다.</summary>
        [GoProperty(PCategory.Control, 9)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;
        #endregion

        #region Event
        /// <summary><see cref="State"/> 속성 값이 변경되었을 때 발생합니다.</summary>
        public event EventHandler? StateChanged;
        #endregion

        #region Constructor
        /// <summary><see cref="GoStateLamp"/> 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoStateLamp()
        {
        }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtText = rts["Text"];

            // 현재 State 와 일치하는 항목을 찾음. 없으면 Base3 OFF 톤 + "Not Matched".
            var current = States.FirstOrDefault(x => x.State == State);
            var c = current != null ? thm.ToColor(current.Color) : thm.ToColor("Base3");
            var text = current?.Text ?? "Not Matched";
            var onOff = current?.OnOff ?? false;

            // 항목별 OnOff 플래그가 같은 색상의 활성/비활성 밝기 톤을 결정.
            Util.DrawLamp(canvas, thm, rtBox, c, c, onOff);

            var fsz = Util.FontSize(AutoFontSize, rtBox.Height) ?? FontSize;
            Util.DrawText(canvas, text, FontName, FontStyle, fsz, rtText, cText, GoContentAlignment.MiddleCenter);

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var current = States.FirstOrDefault(x => x.State == State);
            var text = current?.Text ?? "Not Matched";
            var (rtBox, rtText) = Util.TextIconBounds(text, FontName, FontStyle, FontSize, new SKSize(LampSize, LampSize), GoDirectionHV.Horizon, Gap, rtContent, ContentAlignment);
            rts["Box"] = rtBox;
            rts["Text"] = rtText;
            return rts;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="GoStateLamp"/> 의 한 상태 항목 — 매칭될 State 값 + 표시할 색상/텍스트/OnOff 톤을 정의합니다.
    /// (IcState 의 StateImage 와 유사한 패턴 — 이미지 대신 색상/텍스트/OnOff)
    /// </summary>
    public class StateLamp
    {
        /// <summary>이 항목이 표시될 상태 값.</summary>
        [GoProperty(PCategory.Control, 0)] public int State { get; set; }
        /// <summary>이 상태일 때 램프 색상 (테마 색상 키 또는 직접 지정).</summary>
        [GoProperty(PCategory.Control, 1)] public string Color { get; set; } = "Good";
        /// <summary>이 상태일 때 표시할 텍스트.</summary>
        [GoMultiLineProperty(PCategory.Control, 2)] public string? Text { get; set; }
        /// <summary>이 상태일 때 램프 활성 톤 여부. <c>true</c> 면 밝게, <c>false</c> 면 같은 색의 어두운 톤으로 그려집니다.</summary>
        [GoProperty(PCategory.Control, 3)] public bool OnOff { get; set; } = true;
    }
}
