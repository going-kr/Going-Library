using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    /// <summary>
    /// 이미지 캔버스에서 텍스트와 아이콘을 표시하는 라벨 컨트롤입니다.
    /// </summary>
    public class IcLabel : GoControl
    {
        #region Properties
        /// <summary>
        /// 아이콘 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        /// <summary>
        /// 아이콘 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        /// <summary>
        /// 아이콘 배치 방향을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public GoDirectionHV IconDirection { get; set; } = GoDirectionHV.Horizon;
        /// <summary>
        /// 아이콘과 텍스트 사이의 간격을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public float IconGap { get; set; } = 5;
        /// <summary>
        /// 표시할 텍스트를 가져오거나 설정합니다.
        /// </summary>
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "label";
        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public float FontSize { get; set; } = 12;
        /// <summary>
        /// 텍스트 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 콘텐츠 정렬 방식을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        #endregion

        #region OnDraw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var cText = thm.ToColor(TextColor);

            if (Design != null && Parent != null && (Parent is IcPage || Parent is IcContainer))
                Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, ContentAlignment);

            base.OnDraw(canvas, thm);
        }
        #endregion
    }
}
