using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoSwitch : GoWrapperControl<Going.UI.Controls.GoSwitch>
    {
        #region Properties
        public string OnText { get => Control.OnText; set { if (Control.OnText != value) { Control.OnText = value; Invalidate(); } } }
        public string OffText { get => Control.OffText; set { if (Control.OffText != value) { Control.OffText = value; Invalidate(); } } }
        public string? OnIconString { get => Control.OnIconString; set { if (Control.OnIconString != value) { Control.OnIconString = value; Invalidate(); } } }
        public string? OffIconString { get => Control.OffIconString; set { if (Control.OffIconString != value) { Control.OffIconString = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }
        public float IconSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => Control.IconDirection; set { if (Control.IconDirection != value) { Control.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string OnTextColor { get => Control.OnTextColor; set { if (Control.OnTextColor != value) { Control.OnTextColor = value; Invalidate(); } } }
        public string OffTextColor { get => Control.OffTextColor; set { if (Control.OffTextColor != value) { Control.OffTextColor = value; Invalidate(); } } }
        public string BoxColor { get => Control.BoxColor; set { if (Control.BoxColor != value) { Control.BoxColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string SwitchColor { get => Control.SwitchColor; set { if (Control.SwitchColor != value) { Control.SwitchColor = value; Invalidate(); } } }

        public string OnIconColor { get => Control.OnIconColor; set { if (Control.OnIconColor != value) { Control.OnIconColor = value; Invalidate(); } } }
        public string OffIconColor { get => Control.OffIconColor; set { if (Control.OffIconColor != value) { Control.OffIconColor = value; Invalidate(); } } }

        public bool OnOff { get => Control.OnOff; set { if (Control.OnOff != value) { Control.OnOff = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? OnOffChanged { add => Control.OnOffChanged += value; remove => Control.OnOffChanged -= value; }
        #endregion

        #region Constructor
        public GoSwitch()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
