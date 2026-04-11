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

namespace Going.UI.Controls
{
    /// <summary>
    /// 클릭 가능한 버튼 컨트롤. 아이콘과 텍스트를 표시할 수 있습니다.
    /// </summary>
    public class GoButton : GoControl
    {
        #region Properties
        /// <summary>아이콘 문자열 (FontAwesome 등)</summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        /// <summary>아이콘 크기</summary>
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        /// <summary>아이콘 배치 방향</summary>
        [GoProperty(PCategory.Control, 2)] public GoDirectionHV IconDirection { get; set; }
        /// <summary>아이콘과 텍스트 사이 간격</summary>
        [GoProperty(PCategory.Control, 3)] public float IconGap { get; set; } = 5;
        /// <summary>버튼에 표시할 텍스트</summary>
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "button";
        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 7)] public float FontSize { get; set; } = 12;

        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 8)] public string TextColor { get; set; } = "Fore";
        /// <summary>버튼 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 9)] public string ButtonColor { get; set; } = "Base3";
        /// <summary>테두리 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 10)] public string BorderColor { get; set; } = "Base3";
        /// <summary>모서리 둥글기 유형</summary>
        [GoProperty(PCategory.Control, 11)] public GoRoundType Round { get; set; } = GoRoundType.All;
        /// <summary>테두리 두께</summary>
        [GoProperty(PCategory.Control, 12)] public float BorderWidth { get; set; } = 1F;
        /// <summary>배경 그리기 여부</summary>
        [GoProperty(PCategory.Control, 13)] public bool BackgroundDraw { get; set; } = true;
        /// <summary>테두리만 그리기 여부</summary>
        [GoProperty(PCategory.Control, 14)] public bool BorderOnly { get; set; } = false;
        /// <summary>버튼 채우기 스타일</summary>
        [GoProperty(PCategory.Control, 15)] public GoButtonFillStyle FillStyle { get; set; } = GoButtonFillStyle.Flat;
        /// <summary>콘텐츠 정렬 방식</summary>
        [GoProperty(PCategory.Control, 16)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        /// <summary>자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 17)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;
        /// <summary>자동 아이콘 크기 설정</summary>
        [GoProperty(PCategory.Control, 18)] public GoAutoFontSize AutoIconSize { get; set; } = GoAutoFontSize.NotUsed;
        #endregion

        #region Event
        /// <summary>버튼이 클릭되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ButtonClicked;
        #endregion

        #region Constructor
        public GoButton()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        private bool bHover = false;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cBtn = thm.ToColor(ButtonColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cBor = thm.ToColor(BorderColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var rts = Areas();
            var rtBox = rts["Content"];

            if (BackgroundDraw)
            {
                if (BorderOnly)
                    Util.DrawBox(canvas, rtBox, SKColors.Transparent, cBor.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), Round, thm.Corner, true, BorderWidth);
                else
                    Util.DrawButton(canvas, thm, rtBox, cBtn.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cBor.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), Round, thm.Corner, true, BorderWidth, FillStyle, bDown);
            }

            if (bDown) rtBox.Offset(0, 1);

            var fsz = Util.FontSize(AutoFontSize, rtBox.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtBox.Height) ?? IconSize;

            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, fsz, IconString, isz, IconDirection, IconGap, rtBox, cText, ContentAlignment);

            base.OnDraw(canvas, thm);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (CollisionTool.Check(rtBox, x, y)) bDown = true;

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (bDown)
            {
                bDown = false;
                if (CollisionTool.Check(rtBox, x, y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            bHover = CollisionTool.Check(rtBox, x, y);
        
            base.OnMouseMove(x, y);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            bDown = false;

            base.OnMouseLongClick(x, y, button);
        }

        #endregion
    }
}
