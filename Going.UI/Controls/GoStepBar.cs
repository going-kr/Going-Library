using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Going.UI.Controls
{
    /// <summary>스텝 항목 (Text + Icon)</summary>
    public class GoStepItem
    {
        /// <summary>표시 텍스트</summary>
        public string? Text { get; set; }
        /// <summary>아이콘 문자열 (FontAwesome 등). 없으면 null.</summary>
        public string? IconString { get; set; }
    }

    /// <summary>GoStepBar 표시 모드</summary>
    public enum GoStepBarMode
    {
        /// <summary>지나온 단계 모두 채움 (1..Step)</summary>
        Story,
        /// <summary>현재 단계 한 개만 채움 (Step)</summary>
        Point,
    }

    /// <summary>
    /// 절차 진행 상태를 표시하는 인디케이터. dot + 라벨 + connector 직선으로 구성.
    /// Step=0이면 모든 dot 비활성, Step=N이면 Steps[N-1] 단계가 현재.
    /// </summary>
    public class GoStepBar : GoControl
    {
        #region Properties
        /// <summary>스텝 항목 목록</summary>
        [GoProperty(PCategory.Control, 0)] public List<GoStepItem> Steps { get; set; } = [];

        private int nStep = 0;
        /// <summary>현재 단계 (0=비활성, 1..Steps.Count=Steps[N-1] 활성)</summary>
        [GoProperty(PCategory.Control, 1)]
        public int Step
        {
            get => nStep;
            set
            {
                var v = Convert.ToInt32(MathTool.Constrain(value, 0, Steps.Count));
                if (nStep != v)
                {
                    nStep = v;
                    StepChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>표시 모드</summary>
        [GoProperty(PCategory.Control, 2)] public GoStepBarMode Mode { get; set; } = GoStepBarMode.Story;
        /// <summary>dot 클릭으로 Step 변경 가능 여부</summary>
        [GoProperty(PCategory.Control, 3)] public bool UseClick { get; set; } = false;

        /// <summary>비활성 dot 색 (테마 키)</summary>
        [GoProperty(PCategory.Control, 4)] public string DotColor { get; set; } = "Base2";
        /// <summary>활성 dot 색 (테마 키)</summary>
        [GoProperty(PCategory.Control, 5)] public string ActiveColor { get; set; } = "Select";
        /// <summary>connector 직선 색 (테마 키, 단일)</summary>
        [GoProperty(PCategory.Control, 6)] public string LineColor { get; set; } = "Base2";
        /// <summary>활성 라벨 색 (테마 키)</summary>
        [GoProperty(PCategory.Control, 7)] public string ActiveTextColor { get; set; } = "Fore";
        /// <summary>비활성 라벨 색 (테마 키)</summary>
        [GoProperty(PCategory.Control, 8)] public string InactiveTextColor { get; set; } = "Base3";

        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 9)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 10)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 11)] public float FontSize { get; set; } = 12F;

        /// <summary>dot 외경 지름</summary>
        [GoProperty(PCategory.Control, 12)] public float DotSize { get; set; } = 18F;
        /// <summary>아이콘 크기</summary>
        [GoProperty(PCategory.Control, 13)] public float IconSize { get; set; } = 12F;
        /// <summary>dot 아래 라벨 간격</summary>
        [GoProperty(PCategory.Control, 14)] public float LabelGap { get; set; } = 6F;
        #endregion

        #region Member Variable
        private int hitDownIndex = -1; // UseClick mouse 추적용 (1-based, 없으면 -1)
        #endregion

        #region Event
        /// <summary>Step 값이 변경되었을 때 발생</summary>
        public event EventHandler? StepChanged;
        #endregion

        #region Constructor
        /// <summary><see cref="GoStepBar"/>의 새 인스턴스를 초기화합니다.</summary>
        public GoStepBar()
        {
            Selectable = true;
        }
        #endregion

        #region Override (renderer는 Task 6에서 구현)
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);
            // 렌더링 구현은 Task 6
        }
        #endregion
    }
}
