using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 텍스트를 그리는 시각 노드입니다. 색은 테마 색 키, 폰트는 디자인의 폰트 이름을 사용합니다.
    /// </summary>
    public class VoText : VoObj
    {
        /// <summary>표시할 텍스트.</summary>
        [VoMultiLineProperty("Text", 0)] public string Text { get; set; } = "Text";
        /// <summary>글꼴 이름.</summary>
        [VoFontNameProperty("Text", 1)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 크기.</summary>
        [VoProperty("Text", 2)] public float FontSize { get; set; } = 14F;
        /// <summary>글꼴 스타일.</summary>
        [VoProperty("Text", 3)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>텍스트 색 (테마 색 키).</summary>
        [VoProperty("Text", 4)] public string TextColor { get; set; } = "Fore";
        /// <summary>정렬.</summary>
        [VoProperty("Text", 5)] public GoContentAlignment Alignment { get; set; } = GoContentAlignment.MiddleCenter;

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, SKRect bounds, GoTheme thm)
        {
            Util.DrawText(canvas, Text, FontName, FontStyle, FontSize, bounds, thm.ToColor(TextColor), Alignment);
        }
    }
}
