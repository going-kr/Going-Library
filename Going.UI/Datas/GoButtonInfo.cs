using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public class GoButtonInfo
    {
        public string Name { get; set; }
        public string? Text { get; set; }
        public string? IconString { get; set; }
        public string Size { get; set; } = "100%";

        internal bool Hover { get; set; } = false;
        internal bool Down { get; set; } = false;
    }

    
    public class ButtonClickEventArgs(GoButtonInfo btn) : EventArgs
    {
        public GoButtonInfo Button { get; } = btn;
    }
     
}
