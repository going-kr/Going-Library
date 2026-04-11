using Going.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    /// <summary>
    /// 버튼의 채우기 스타일을 정의하는 열거형입니다.
    /// </summary>
    public enum GoButtonFillStyle
    {
        /// <summary>평면 스타일</summary>
        Flat,
        /// <summary>엠보싱(입체) 스타일</summary>
        Emboss,
        /// <summary>그라데이션 스타일</summary>
        Gradient
    }

    /// <summary>
    /// 버튼 항목의 기본 정보를 나타내는 클래스입니다.
    /// </summary>
    public class GoButtonItem
    {
        /// <summary>버튼의 이름(식별자)</summary>
        [GoProperty(PCategory.Control, 0)] public string Name { get; set; }
        /// <summary>버튼에 표시할 텍스트</summary>
        [GoProperty(PCategory.Control, 1)] public string? Text { get; set; }
        /// <summary>버튼에 표시할 아이콘 문자열</summary>
        [GoProperty(PCategory.Control, 2)] public string? IconString { get; set; }
        /// <summary>버튼의 크기 (퍼센트 또는 픽셀)</summary>
        [GoSizeProperty(PCategory.Control, 3)] public string Size { get; set; } = "100%";

        [JsonIgnore] internal bool Hover { get; set; } = false;
        [JsonIgnore] internal bool Down { get; set; } = false;

        /// <summary>버튼의 이름을 반환합니다.</summary>
        public override string ToString() => Name ?? string.Empty;
    }

    /// <summary>
    /// 버튼 그룹에서 사용되는 선택 가능한 버튼 항목 클래스입니다.
    /// </summary>
    public class GoButtonsItem : GoButtonItem
    {
        /// <summary>버튼의 선택 상태</summary>
        public bool Selected { get; set; }
    }


    /// <summary>
    /// 단일 버튼 클릭 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="btn">클릭된 버튼 항목</param>
    public class ButtonClickEventArgs(GoButtonItem btn) : EventArgs
    {
        /// <summary>클릭된 버튼 항목</summary>
        public GoButtonItem Button { get; } = btn;
    }

    /// <summary>
    /// 버튼 그룹의 버튼 클릭 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="btn">클릭된 버튼 항목</param>
    public class ButtonsClickEventArgs(GoButtonsItem btn) : EventArgs
    {
        /// <summary>클릭된 버튼 항목</summary>
        public GoButtonsItem Button { get; } = btn;
    }

    /// <summary>
    /// 버튼 그룹에서 선택 변경 시 발생하는 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="selBtn">새로 선택된 버튼</param>
    /// <param name="deselBtn">선택 해제된 버튼 (없으면 null)</param>
    public class ButtonsSelectedEventArgs(GoButtonsItem selBtn, GoButtonsItem? deselBtn) : EventArgs
    {
        /// <summary>새로 선택된 버튼</summary>
        public GoButtonsItem SelectedButton { get; } = selBtn;
        /// <summary>선택 해제된 버튼 (없으면 null)</summary>
        public GoButtonsItem? DeselectedButton { get; } = deselBtn;
    }
}
