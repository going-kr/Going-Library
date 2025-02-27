using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public class GoButtonItem
    {
        public string Name { get; set; }
        public string? Text { get; set; }
        public string? IconString { get; set; }
        public string Size { get; set; } = "100%";

        [JsonIgnore] internal bool Hover { get; set; } = false;
        [JsonIgnore] internal bool Down { get; set; } = false;
    }

    public class GoButtonsItem : GoButtonItem
    {
        public bool Selected { get; set; }
    }


    public class ButtonClickEventArgs(GoButtonItem btn) : EventArgs
    {
        public GoButtonItem Button { get; } = btn;
    }

    public class ButtonsClickEventArgs(GoButtonsItem btn) : EventArgs
    {
        public GoButtonsItem Button { get; } = btn;
    }

    public class ButtonsSelectedEventArgs(GoButtonsItem selBtn, GoButtonsItem? deselBtn) : EventArgs
    {
        public GoButtonsItem SelectedButton { get; } = selBtn;
        public GoButtonsItem? DeselectedButton { get; } = deselBtn;
    }
}
