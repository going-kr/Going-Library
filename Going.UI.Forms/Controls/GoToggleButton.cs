using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoToggleButton : GoWrapperControl<Going.UI.Controls.GoToggleButton>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => Control.IconDirection; set { if (Control.IconDirection != value) { Control.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }
        public override string Text { get => base.Text; set { base.Text = value; if (Control.Text != value) { Control.Text = value; Invalidate(); } } }
        public string CheckedText { get => Control.CheckedText; set { Control.CheckedText = value; if (Control.CheckedText != value) { Control.CheckedText = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string ButtonColor { get => Control.ButtonColor; set { if (Control.ButtonColor != value) { Control.ButtonColor = value; Invalidate(); } } }
        public string CheckedButtonColor { get => Control.CheckedButtonColor; set { if (Control.CheckedButtonColor != value) { Control.CheckedButtonColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public bool Checked { get => Control.Checked; set { if (Control.Checked != value) { Control.Checked = value; Invalidate(); } } }
        public bool AllowToggle { get => Control.AllowToggle; set { if (Control.AllowToggle != value) { Control.AllowToggle = value; Invalidate(); } } }

        #endregion

        #region Event
        public event EventHandler? ButtonClicked { add => Control.ButtonClicked += value; remove => Control.ButtonClicked -= value; }
        public event EventHandler? CheckedChanged { add => Control.CheckedChanged += value; remove => Control.CheckedChanged -= value; }
        #endregion

        #region Constructor
        public GoToggleButton()
        {
            SetStyle(ControlStyles.Selectable, true);
            Control.Text = base.Text;
        }
        #endregion
    }
}
