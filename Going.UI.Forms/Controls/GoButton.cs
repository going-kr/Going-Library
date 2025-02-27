using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoButton : GoWrapperControl<Going.UI.Controls.GoButton>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => Control.IconDirection; set { if (Control.IconDirection != value) { Control.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }
        public override string Text { get => Control.Text; set { if (Control.Text != value) { Control.Text = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string ButtonColor { get => Control.ButtonColor; set { if (Control.ButtonColor != value) { Control.ButtonColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public bool BackgroundDraw { get => Control.BackgroundDraw; set { if (Control.BackgroundDraw != value) { Control.BackgroundDraw = value; Invalidate(); } } }
        public bool BorderOnly { get => Control.BorderOnly; set { if (Control.BorderOnly != value) { Control.BorderOnly = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ButtonClicked { add => Control.ButtonClicked += value; remove => Control.ButtonClicked -= value; }
        #endregion

        #region Constructor
        public GoButton()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
