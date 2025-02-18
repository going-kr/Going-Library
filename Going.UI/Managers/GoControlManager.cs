using Going.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Managers
{
    public class GoControlManager
    {
        private static IGoControl? sel;

        public static IGoControl? SelectedControl => sel;
        public static void Select(IGoControl control) { sel = control; }
        public static void Deselect() => sel = null;
        
    }
}
