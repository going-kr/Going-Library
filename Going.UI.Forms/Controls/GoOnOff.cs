using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoOnOff : GoWrapperControl<Going.UI.Controls.GoOnOff>
    {
        #region Properties
        public string OnText { get => Control.OnText; set { if (Control.OnText != value) { Control.OnText = value; Invalidate(); } } }
        public string OffText { get => Control.OffText; set { if (Control.OffText != value) { Control.OffText = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BoxColor { get => Control.BoxColor; set { if (Control.BoxColor != value) { Control.BoxColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string CursorColor { get => Control.CursorColor; set { if (Control.CursorColor != value) { Control.CursorColor = value; Invalidate(); } } }
        public string OnColor { get => Control.OnColor; set { if (Control.OnColor != value) { Control.OnColor = value; Invalidate(); } } }
        public string OffColor { get => Control.OffColor; set { if (Control.OffColor != value) { Control.OffColor = value; Invalidate(); } } }

        public bool OnOff { get => Control.OnOff; set { if (Control.OnOff != value) { Control.OnOff = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? OnOffChanged { add => Control.OnOffChanged += value; remove => Control.OnOffChanged -= value; }
        #endregion

        #region Constructor
        public GoOnOff()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
