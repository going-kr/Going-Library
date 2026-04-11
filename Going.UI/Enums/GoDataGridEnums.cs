using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 데이터 그리드 열의 정렬 상태를 정의하는 열거형입니다.
    /// </summary>
    public enum GoDataGridColumnSortState
    {
        /// <summary>정렬 없음</summary>
        None,
        /// <summary>오름차순 정렬</summary>
        Asc,
        /// <summary>내림차순 정렬</summary>
        Desc
    }

    /// <summary>
    /// 데이터 그리드의 행 선택 모드를 정의하는 열거형입니다.
    /// </summary>
    public enum GoDataGridSelectionMode
    {
        /// <summary>선택 불가</summary>
        None,
        /// <summary>체크박스 셀렉터를 통한 선택</summary>
        Selector,
        /// <summary>단일 행 선택</summary>
        Single,
        /// <summary>다중 행 선택</summary>
        Multi,
        /// <summary>PC 스타일 다중 선택 (Ctrl/Shift 키 조합)</summary>
        MultiPC
    }
}
