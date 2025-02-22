using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Managers;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Forms.Input;

namespace Going.UI.Forms.Controls
{
    public class GoValueString : GoWrapperControl<Going.UI.Controls.GoValueString>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }
        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        public string Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler ValueClicked { add => Control.ValueClicked += value; remove => Control.ValueClicked -= value; }
        #endregion

        #region Constructor
        public GoValueString()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }

    public class GoValueNumber<T> : GoWrapperControl<Going.UI.Controls.GoValueNumber<T>> where T : struct
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        public T Value { get => Control.Value; set { if (!Control.Value.Equals(value)) { Control.Value = value; Invalidate(); } } }
        public string? FormatString { get => Control.FormatString; set { if (Control.FormatString != value) { Control.FormatString = value; Invalidate(); } } }
        public string? Unit { get => Control.Unit; set { if (Control.Unit != value) { Control.Unit = value; Invalidate(); } } }
        public float? UnitSize { get => Control.UnitSize; set { if (Control.UnitSize != value) { Control.UnitSize = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler ValueClicked { add => Control.ValueClicked += value; remove => Control.ValueClicked -= value; }
        #endregion

        #region Constructor
        public GoValueNumber()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }

    public class GoValueInteger : GoValueNumber<int> { }
    public class GoValueFloat : GoValueNumber<double> { }

    public class GoValueBoolean : GoWrapperControl<Going.UI.Controls.GoValueBoolean>
    {
        #region Properties
        public string? IconString { get => Control.IconString; set { if (Control.IconString != value) { Control.IconString = value; Invalidate(); } } }
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }

        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public GoDirectionHV Direction { get => Control.Direction; set { if (Control.Direction != value) { Control.Direction = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string FillColor { get => Control.FillColor; set { if (Control.FillColor != value) { Control.FillColor = value; Invalidate(); } } }
        public string ValueColor { get => Control.ValueColor; set { if (Control.ValueColor != value) { Control.ValueColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public float? TitleSize { get => Control.TitleSize; set { if (Control.TitleSize != value) { Control.TitleSize = value; Invalidate(); } } }
        public string? Title { get => Control.Title; set { if (Control.Title != value) { Control.Title = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => Control.Buttons; set { if (Control.Buttons != value) { Control.Buttons = value; Invalidate(); } } }
        public float? ButtonSize { get => Control.ButtonSize; set { if (Control.ButtonSize != value) { Control.ButtonSize = value; Invalidate(); } } }

        public bool Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler ValueClicked { add => Control.ValueClicked += value; remove => Control.ValueClicked -= value; }
        #endregion

        #region Constructor
        public GoValueBoolean()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
