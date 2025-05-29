using Going.UI.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Dialogs
{
    public static class GoDialogs
    {
        public static Dictionary<string, GoWindow> SystemWindows { get; } = [];

        static GoDialogs()
        {
            SystemWindows.Add("MessageBox", MessageBox);
            SystemWindows.Add("SelectorBox", SelectorBox);
            SystemWindows.Add("InputBox", InputBox);
        }

        public static GoMessageBox MessageBox { get; private set; } = new GoMessageBox();
        public static GoSelectorBox SelectorBox { get; private set; } = new GoSelectorBox();
        public static GoInputBox InputBox { get; private set; } = new GoInputBox();
    }
}
