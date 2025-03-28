using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Dialogs
{
    internal class GoDiaglos
    {
        internal static GoMessageBox MessageBox { get; private set; } = new GoMessageBox();
        internal static GoSelectorBox SelectorBox { get; private set; } = new GoSelectorBox();
        internal static GoInputBox InputBox { get; private set; } = new GoInputBox();
    }
}
