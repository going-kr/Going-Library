using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 버튼 그룹의 동작 모드를 정의하는 열거형입니다.
    /// </summary>
    public enum GoButtonsMode
    {
        /// <summary>일반 버튼 모드</summary>
        Button,
        /// <summary>토글 버튼 모드 (클릭 시 선택/해제 전환)</summary>
        Toggle,
        /// <summary>라디오 버튼 모드 (하나만 선택 가능)</summary>
        Radio
    }
}
