using Going.UI.Enums;
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
    /// 램프(표시등) 컨트롤. ON/OFF 상태를 시각적으로 표시합니다.
    /// </summary>
    public class GoLamp : GoControl
    {
        #region Properties
        /// <summary>
        /// 램프 옆에 표시할 텍스트를 가져오거나 설정합니다.
        /// </summary>
        [GoMultiLineProperty(PCategory.Control, 0)] public string Text { get; set; } = "lamp";
        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 1)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public float FontSize { get; set; } = 12;

        /// <summary>
        /// 텍스트 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// ON 상태일 때 램프 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string OnColor { get; set; } = "Good";
        /// <summary>
        /// OFF 상태일 때 램프 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public string OffColor { get; set; } = "Base2";

        private bool bOnOff = false;
        /// <summary>
        /// 램프의 ON/OFF 상태를 가져오거나 설정합니다. 값이 변경되면 <see cref="OnOffChanged"/> 이벤트가 발생합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)]
        public bool OnOff
        {
            get => bOnOff; set
            {
                if (bOnOff != value)
                {
                    bOnOff = value;
                    OnOffChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 램프 아이콘의 크기(픽셀)를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public int LampSize { get; set; } = 24;
        /// <summary>
        /// 램프 아이콘과 텍스트 사이의 간격을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public int Gap { get; set; } = 10;
        /// <summary>
        /// 콘텐츠 정렬 방식을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        /// <summary>
        /// 자동 글꼴 크기 조절 모드를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;
        #endregion

        #region Event
        /// <summary>
        /// <see cref="OnOff"/> 속성 값이 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? OnOffChanged;
        #endregion

        #region Constructor
        public GoLamp()
        {
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cOff = thm.ToColor(OffColor);
            var cOn = thm.ToColor(OnColor);
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtText = rts["Text"];

            Util.DrawLamp(canvas, thm, rtBox, cOn, cOff, OnOff);

            var fsz = Util.FontSize(AutoFontSize, rtBox.Height) ?? FontSize;
            
            Util.DrawText(canvas, Text, FontName, FontStyle, fsz, rtText, cText, GoContentAlignment.MiddleCenter);

            base.OnDraw(canvas, thm);
        }
         
        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var (rtBox, rtText) = Util.TextIconBounds(Text, FontName, FontStyle, FontSize, new SKSize(LampSize, LampSize), GoDirectionHV.Horizon, Gap, rtContent, ContentAlignment);

            rts["Box"] = rtBox;
            rts["Text"] = rtText;

            return rts;
        }
        #endregion
    }
}
