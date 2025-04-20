using Going.UI.Controls;
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
        [GoProperty(PCategory.Misc, 0)] public string Name { get; set; }
        [GoProperty(PCategory.Misc, 1)] public string? Text { get; set; }
        [GoProperty(PCategory.Misc, 2)] public string? IconString { get; set; }
        [GoSizeProperty(PCategory.Misc, 3)] public string Size { get; set; } = "100%";

        [JsonIgnore] internal bool Hover { get; set; } = false;
        [JsonIgnore] internal bool Down { get; set; } = false;

        public override string ToString() => Name ?? string.Empty;
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
