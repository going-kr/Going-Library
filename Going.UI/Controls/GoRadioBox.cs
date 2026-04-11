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
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    /// <summary>
    /// 라디오 박스 컨트롤. 같은 부모 컨테이너 내에서 하나만 선택할 수 있는 원형 선택 컨트롤입니다.
    /// </summary>
    public class GoRadioBox : GoControl
    {
        #region Properties
        /// <summary>라디오 박스 옆에 표시할 텍스트</summary>
        [GoMultiLineProperty(PCategory.Control, 0)] public string Text { get; set; } = "radiobox";
        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 1)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 2)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 3)] public float FontSize { get; set; } = 12;

        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        /// <summary>라디오 박스 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 5)] public string BoxColor { get; set; } = "Base1";
        /// <summary>선택 표시 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 6)] public string CheckColor { get; set; } = "Fore";

        private bool bCheck = false;
        /// <summary>선택 상태. 선택되면 같은 부모의 다른 GoRadioBox는 자동으로 선택 해제됩니다.</summary>
        [GoProperty(PCategory.Control, 7)]
        public bool Checked
        {
            get => bCheck; set
            {
                if (bCheck != value)
                {
                    bCheck = value;

                    if (Parent != null  && bCheck)
                    {
                        var ls = Parent.Childrens.Where(x => x is GoRadioBox && x != this).Select(x => x as GoRadioBox);
                        foreach (var c in ls)
                            if (c != null) c.Checked = false;
                    }

                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>라디오 박스 크기 (픽셀)</summary>
        [GoProperty(PCategory.Control, 8)] public int BoxSize { get; set; } = 24;
        /// <summary>라디오 박스와 텍스트 사이 간격</summary>
        [GoProperty(PCategory.Control, 9)] public int Gap { get; set; } = 10;
        /// <summary>콘텐츠 정렬 방식</summary>
        [GoProperty(PCategory.Control, 10)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        /// <summary>자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 11)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;
        #endregion

        #region Event
        /// <summary>선택 상태가 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? CheckedChanged;
        #endregion

        #region Constructor
        public GoRadioBox()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bHover = false;
        private bool bDown = false;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cBox = thm.ToColor(BoxColor);
            var cChk = thm.ToColor(CheckColor);
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtText = rts["Text"];

            Util.DrawBox(canvas, rtBox, cBox.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cBox.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), GoRoundType.Ellipse, thm.Corner);

            var sz = BoxSize * 0.45F;
            if (Checked) Util.DrawBox(canvas, MathTool.MakeRectangle(rtBox, new SKSize(sz, sz)), cText, GoRoundType.Ellipse, thm.Corner);

            var fsz = Util.FontSize(AutoFontSize, rtBox.Height) ?? FontSize;

            Util.DrawText(canvas, Text, FontName, FontStyle, fsz, rtText, cText, GoContentAlignment.MiddleCenter);

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
                if (CollisionTool.Check(rtBox, x, y))
                {
                    Checked = true;
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            bHover = CollisionTool.Check(rtContent, x, y);


            base.OnMouseMove(x, y);
        }

        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var (rtBox, rtText) = Util.TextIconBounds(Text, FontName, FontStyle, FontSize, new SKSize(BoxSize, BoxSize), GoDirectionHV.Horizon, Gap, rtContent, ContentAlignment);

            rts["Box"] = rtBox;
            rts["Text"] = rtText;

            return rts;
        }
        #endregion
    }
}
