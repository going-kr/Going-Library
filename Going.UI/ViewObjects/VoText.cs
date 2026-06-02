using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 텍스트를 그리는 시각 컨트롤입니다 (Vo 패밀리, leaf). 색은 테마 색 키, 폰트는 디자인 폰트 이름.
    /// </summary>
    public class VoText : GoControl
    {
        /// <summary>표시할 텍스트.</summary>
        [GoMultiLineProperty(PCategory.Control, 0)] public string Text { get; set; } = "Text";
        /// <summary>글꼴 이름.</summary>
        [GoFontNameProperty(PCategory.Control, 1)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 크기.</summary>
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 14F;
        /// <summary>글꼴 스타일.</summary>
        [GoProperty(PCategory.Control, 3)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>텍스트 색 (테마 색 키).</summary>
        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        /// <summary>정렬.</summary>
        [GoProperty(PCategory.Control, 5)] public GoContentAlignment Alignment { get; set; } = GoContentAlignment.MiddleCenter;

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rt = Areas()["Content"];
            Util.DrawText(canvas, Text, FontName, FontStyle, FontSize, rt, thm.ToColor(TextColor), Alignment);
            base.OnDraw(canvas, thm);
        }
    }
}
