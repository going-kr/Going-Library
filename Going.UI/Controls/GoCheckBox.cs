using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    /// <summary>
    /// 체크박스 컨트롤. 체크 표시와 텍스트를 함께 표시합니다.
    /// </summary>
    public class GoCheckBox : GoControl
    {
        #region Properties
        /// <summary>체크박스 옆에 표시할 텍스트</summary>
        [GoMultiLineProperty(PCategory.Control, 0)] public string Text { get; set; } = "checkbox";
        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 1)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 2)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 3)] public float FontSize { get; set; } = 12;

        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        /// <summary>체크박스 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 5)] public string BoxColor { get; set; } = "Base1";
        /// <summary>체크 표시 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 6)] public string CheckColor { get; set; } = "Fore";

        /// <summary>체크 상태. 값이 변경되면 <see cref="CheckedChanged"/> 이벤트가 발생합니다.</summary>
        [GoProperty(PCategory.Control, 7)]
        public bool Checked
        {
            get => bCheck; set
            {
                if (bCheck != value)
                {
                    bCheck = value; 
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>체크박스 크기 (픽셀)</summary>
        [GoProperty(PCategory.Control, 8)] public int BoxSize { get; set; } = 24;
        /// <summary>체크박스와 텍스트 사이 간격</summary>
        [GoProperty(PCategory.Control, 9)] public int Gap { get; set; } = 10;
        /// <summary>콘텐츠 정렬 방식</summary>
        [GoProperty(PCategory.Control, 10)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        /// <summary>자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 11)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;
        #endregion

        #region Event
        /// <summary>체크 상태가 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? CheckedChanged;
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="GoCheckBox"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoCheckBox()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bCheck = false;
        private bool bHover = false;
        private bool bDown = false;
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cBox = thm.ToColor(BoxColor);
            var cChk = thm.ToColor(CheckColor);
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtText= rts["Text"];

            Util.DrawBox(canvas, rtBox, cBox.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cBox.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), GoRoundType.All, thm.Corner);

            if (Checked) Util.DrawIcon(canvas, "fa-check", BoxSize * 0.65F, rtBox, cChk);

            var fsz = Util.FontSize(AutoFontSize, rtText.Height) ?? FontSize;

            Util.DrawText(canvas, Text, FontName, FontStyle, fsz, rtText, cText, GoContentAlignment.MiddleCenter);

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (CollisionTool.Check(rtBox, x, y)) bDown = true;

            base.OnMouseDown(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (bDown)
            {
                bDown = false;
                if (CollisionTool.Check(rtBox, x, y))
                {
                    Checked = !Checked;
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            base.OnMouseUp(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            bHover = CollisionTool.Check(rtContent, x, y);


            base.OnMouseMove(x, y);
        }

        /// <inheritdoc/>
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
