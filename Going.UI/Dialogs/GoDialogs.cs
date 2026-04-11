using Going.UI.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Dialogs
{
    /// <summary>
    /// 다이얼로그 관리자. 메시지 박스, 선택 박스, 입력 박스 등 시스템 다이얼로그 인스턴스를 제공합니다.
    /// </summary>
    public static class GoDialogs
    {
        /// <summary>
        /// 시스템 윈도우 딕셔너리를 가져옵니다. 키는 윈도우 이름입니다.
        /// </summary>
        public static Dictionary<string, GoWindow> SystemWindows { get; } = [];

        static GoDialogs()
        {
            SystemWindows.Add("MessageBox", MessageBox);
            SystemWindows.Add("SelectorBox", SelectorBox);
            SystemWindows.Add("InputBox", InputBox);
        }

        /// <summary>
        /// 메시지 박스 다이얼로그 인스턴스를 가져옵니다.
        /// </summary>
        public static GoMessageBox MessageBox { get; private set; } = new GoMessageBox();
        /// <summary>
        /// 선택 박스 다이얼로그 인스턴스를 가져옵니다.
        /// </summary>
        public static GoSelectorBox SelectorBox { get; private set; } = new GoSelectorBox();
        /// <summary>
        /// 입력 박스 다이얼로그 인스턴스를 가져옵니다.
        /// </summary>
        public static GoInputBox InputBox { get; private set; } = new GoInputBox();
    }
}
