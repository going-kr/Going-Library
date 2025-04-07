using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoLampButton : GoWrapperControl<Going.UI.Controls.GoLampButton>
    {
        #region Properties
        public override string Text { get => base.Text; set { base.Text = value; if (Control.Text != value) { Control.Text = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string OnColor { get => Control.OnColor; set { if (Control.OnColor != value) { Control.OnColor = value; Invalidate(); } } }
        public string OffColor { get => Control.OffColor; set { if (Control.OffColor != value) { Control.OffColor = value; Invalidate(); } } }
        public string ButtonColor { get => Control.ButtonColor; set { if (Control.ButtonColor != value) { Control.ButtonColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public bool OnOff { get => Control.OnOff; set { if (Control.OnOff != value) { Control.OnOff = value; Invalidate(); } } }
        public int LampSize { get => Control.LampSize; set { if (Control.LampSize != value) { Control.LampSize = value; Invalidate(); } } }
        public int Gap { get => Control.Gap; set { if (Control.Gap != value) { Control.Gap = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ButtonClicked { add => Control.ButtonClicked += value; remove => Control.ButtonClicked -= value; }
        public event EventHandler? OnOffChanged { add => Control.OnOffChanged += value; remove => Control.OnOffChanged -= value; }
        #endregion

        #region Constructor
        public GoLampButton()
        {
            SetStyle(ControlStyles.Selectable, true);
            Control.Text = base.Text;
        }
        #endregion
    }
}
