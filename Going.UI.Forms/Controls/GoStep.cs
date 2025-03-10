using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoStep : GoWrapperControl<Going.UI.Controls.GoStep>
    {
        #region Properties
        public string? PrevIconString { get => Control.PrevIconString; set { if (Control.PrevIconString != value) { Control.PrevIconString = value; Invalidate(); } } }
        public string? NextIconString { get => Control.NextIconString; set { if (Control.NextIconString != value) { Control.NextIconString = value; Invalidate(); } } }

        public string ButtonColor { get => Control.ButtonColor; set { if (Control.ButtonColor != value) { Control.ButtonColor = value; Invalidate(); } } }
        public string StepColor { get => Control.StepColor; set { if (Control.StepColor != value) { Control.StepColor = value; Invalidate(); } } }
        public string SelectColor { get => Control.SelectColor; set { if (Control.SelectColor != value) { Control.SelectColor = value; Invalidate(); } } }
        public bool IsCircle { get => Control.IsCircle; set { if (Control.IsCircle != value) { Control.IsCircle = value; Invalidate(); } } }
        public bool UseButton { get => Control.UseButton; set { if (Control.UseButton != value) { Control.UseButton = value; Invalidate(); } } }

        public int StepCount { get => Control.StepCount; set { if (Control.StepCount != value) { Control.StepCount = value; Invalidate(); } } }
        public int Step { get => Control.Step; set { if (Control.Step != value) { Control.Step = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? StepChagend { add => Control.StepChagend += value; remove => Control.StepChagend -= value; }
        #endregion

        #region Constructor
        public GoStep()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
