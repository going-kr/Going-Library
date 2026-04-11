using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 모서리 둥글기 유형을 정의하는 열거형입니다.
    /// </summary>
    public enum GoRoundType
    {
        /// <summary>직각 모서리 (둥글기 없음)</summary>
        Rect,
        /// <summary>모든 모서리 둥글게</summary>
        All,
        /// <summary>왼쪽 모서리만 둥글게</summary>
        L,
        /// <summary>오른쪽 모서리만 둥글게</summary>
        R,
        /// <summary>상단 모서리만 둥글게</summary>
        T,
        /// <summary>하단 모서리만 둥글게</summary>
        B,
        /// <summary>왼쪽 상단 모서리만 둥글게</summary>
        LT,
        /// <summary>오른쪽 상단 모서리만 둥글게</summary>
        RT,
        /// <summary>왼쪽 하단 모서리만 둥글게</summary>
        LB,
        /// <summary>오른쪽 하단 모서리만 둥글게</summary>
        RB,
        /// <summary>타원형</summary>
        Ellipse,
    }
}
