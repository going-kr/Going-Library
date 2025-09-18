using Going.UI.Datas;
using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UI.Forms.Controls
{
    public class GoButtons : GoWrapperControl<Going.UI.Controls.GoButtons>
    {
        #region Properties
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => Control.IconDirection; set { if (Control.IconDirection != value) { Control.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string ButtonColor { get => Control.ButtonColor; set { if (Control.ButtonColor != value) { Control.ButtonColor = value; Invalidate(); } } }
        public string SelectedButtonColor { get => Control.SelectedButtonColor; set { if (Control.SelectedButtonColor != value) { Control.SelectedButtonColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonsItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }
        public GoButtonsMode Mode { get => Control.Mode; set { if (Control.Mode != value) { Control.Mode = value; Invalidate(); } } }

        public string Bordercolor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public float BorderWidth { get => Control.BorderWidth; set { if (Control.BorderWidth != value) { Control.BorderWidth = value; Invalidate(); } } }
        public GoButtonFillStyle FillStyle { get => Control.FillStyle; set { if (Control.FillStyle != value) { Control.FillStyle = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler<ButtonsClickEventArgs>? ButtonClicked { add => Control.ButtonClicked += value; remove => Control.ButtonClicked -= value; }
        public event EventHandler<ButtonsSelectedEventArgs>? SelectedChanged { add => Control.SelectedChanged += value; remove => Control.SelectedChanged -= value; }
        #endregion

        #region Constructor
        public GoButtons()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
