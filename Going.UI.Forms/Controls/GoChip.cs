using Going.UI.Enums;
using System;

namespace Going.UI.Forms.Controls
{
    public class GoChip : GoWrapperControl<Going.UI.Controls.GoChip>
    {
        #region Properties
        public override string Text { get => base.Text; set { base.Text = value; if (Control.Text != value) { Control.Text = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }

        public string? ChipColor { get => Control.ChipColor; set { if (Control.ChipColor != value) { Control.ChipColor = value; Invalidate(); } } }
        public string? BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public float BorderWidth { get => Control.BorderWidth; set { if (Control.BorderWidth != value) { Control.BorderWidth = value; Invalidate(); } } }

        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public string? IconColor { get => Control.IconColor; set { if (Control.IconColor != value) { Control.IconColor = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }

        public string? DotColor { get => Control.DotColor; set { if (Control.DotColor != value) { Control.DotColor = value; Invalidate(); } } }
        public float DotSize { get => Control.DotSize; set { if (Control.DotSize != value) { Control.DotSize = value; Invalidate(); } } }
        public float Gap { get => Control.Gap; set { if (Control.Gap != value) { Control.Gap = value; Invalidate(); } } }

        public bool Closable { get => Control.Closable; set { if (Control.Closable != value) { Control.Closable = value; Invalidate(); } } }
        public string CloseIcon { get => Control.CloseIcon; set { if (Control.CloseIcon != value) { Control.CloseIcon = value; Invalidate(); } } }
        public string? CloseColor { get => Control.CloseColor; set { if (Control.CloseColor != value) { Control.CloseColor = value; Invalidate(); } } }
        public float CloseSize { get => Control.CloseSize; set { if (Control.CloseSize != value) { Control.CloseSize = value; Invalidate(); } } }

        public float? Corner { get => Control.Corner; set { if (Control.Corner != value) { Control.Corner = value; Invalidate(); } } }
        public GoContentAlignment ContentAlignment { get => Control.ContentAlignment; set { if (Control.ContentAlignment != value) { Control.ContentAlignment = value; Invalidate(); } } }
        public float SidePadding { get => Control.SidePadding; set { if (Control.SidePadding != value) { Control.SidePadding = value; Invalidate(); } } }

        public event EventHandler CloseClicked { add => Control.CloseClicked += value; remove => Control.CloseClicked -= value; }
        #endregion

        #region Constructor
        public GoChip() { Control.Text = base.Text; }
        #endregion
    }
}
