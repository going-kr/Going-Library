using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 날짜/시간 표시 유형을 정의하는 열거형입니다.
    /// </summary>
    public enum GoDateTimeKind
    {
        /// <summary>날짜와 시간 모두 표시</summary>
        DateTime,
        /// <summary>날짜만 표시</summary>
        Date,
        /// <summary>시간만 표시</summary>
        Time
    }
}
