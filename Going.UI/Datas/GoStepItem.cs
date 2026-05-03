using Going.UI.Controls;

namespace Going.UI.Datas
{
    /// <summary>
    /// GoStepBar에서 사용되는 스텝 항목 클래스입니다 (Text + Icon).
    /// </summary>
    public class GoStepItem
    {
        /// <summary>아이콘 문자열 (FontAwesome 등). 없으면 null.</summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }

        /// <summary>표시 텍스트</summary>
        [GoProperty(PCategory.Control, 1)] public string? Text { get; set; }
    }
}
