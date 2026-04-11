using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 항목 선택 모드를 정의하는 열거형입니다.
    /// </summary>
    public enum GoItemSelectionMode
    {
        /// <summary>선택 불가</summary>
        None,
        /// <summary>단일 항목 선택</summary>
        Single,
        /// <summary>다중 항목 선택</summary>
        Multi,
        /// <summary>PC 스타일 다중 선택 (Ctrl/Shift 키 조합)</summary>
        MultiPC
    }
}
