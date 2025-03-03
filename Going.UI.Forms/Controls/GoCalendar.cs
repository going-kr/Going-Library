using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoCalendar : GoWrapperControl<Going.UI.Controls.GoCalendar>
    {
        #region Properties
        public override string Text { get => Control.Text; set { if (Control.Text != value) { Control.Text = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }
 
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BoxColor { get => Control.BoxColor; set { if (Control.BoxColor != value) { Control.BoxColor = value; Invalidate(); } } }
        public string SelectColor { get => Control.SelectColor; set { if (Control.SelectColor != value) { Control.SelectColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public int CurrentYear => Control.CurrentYear;
        public int CurrentMonth => Control.CurrentMonth;
        public bool BackgroundDraw { get => Control.BackgroundDraw; set { if (Control.BackgroundDraw != value) { Control.BackgroundDraw = value; Invalidate(); } } }

        public bool MultiSelect { get => Control.MultiSelect; set { if (Control.MultiSelect != value) { Control.MultiSelect = value; Invalidate(); } } }
        public bool NoneSelect { get => Control.NoneSelect; set { if (Control.NoneSelect != value) { Control.NoneSelect = value; Invalidate(); } } }
        public List<DateTime> SelectedDays => Control.SelectedDays;
        #endregion
    }
}
