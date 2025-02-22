using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoIconButton : GoWrapperControl<Going.UI.Controls.GoIconButton>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float Rotate { get => Control.Rotate; set { if (Control.Rotate != value) { Control.Rotate = value; Invalidate(); } } }
        public string ButtonColor { get => Control.ButtonColor; set { if (Control.ButtonColor != value) { Control.ButtonColor = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ButtonClicked { add => Control.ButtonClicked += value; remove => Control.ButtonClicked -= value; }
        #endregion

        #region Constructor
        public GoIconButton()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
