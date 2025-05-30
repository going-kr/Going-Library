﻿using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoRadioBox : GoWrapperControl<Going.UI.Controls.GoRadioBox>
    {
        #region Properties
        public override string Text { get => base.Text; set { base.Text = value; if (Control.Text != value) { Control.Text = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BoxColor { get => Control.BoxColor; set { if (Control.BoxColor != value) { Control.BoxColor = value; Invalidate(); } } }

        public bool Checked { get => Control.Checked; set { if (Control.Checked != value) { Control.Checked = value; Invalidate(); } } }
        public int BoxSize { get => Control.BoxSize; set { if (Control.BoxSize != value) { Control.BoxSize = value; Invalidate(); } } }
        public int Gap { get => Control.Gap; set { if (Control.Gap != value) { Control.Gap = value; Invalidate(); } } }
        public GoContentAlignment ContentAlignment { get => Control.ContentAlignment; set { if (Control.ContentAlignment != value) { Control.ContentAlignment = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? CheckedChanged;
        #endregion

        #region Constructor
        public GoRadioBox()
        {
            SetStyle(ControlStyles.Selectable, true);
            Control.Text = base.Text;
            Control.CheckedChanged += (o, s) =>
            {
                if (Parent != null && Control.Checked)
                {
                    var ls = Parent.Controls.Cast<Control>().Where(x => x is GoRadioBox && x != this).Select(x => x as GoRadioBox);
                    foreach (var c in ls)
                        if (c != null) c.Checked = false;
                }

                CheckedChanged?.Invoke(this, EventArgs.Empty);  
            };
        }
        #endregion
    }
}
