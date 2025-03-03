using Going.UI.Collections;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoToolBox : GoWrapperControl<Going.UI.Controls.GoToolBox>
    {
        #region Properties
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BoxColor { get => Control.BoxColor; set { if (Control.BoxColor != value) { Control.BoxColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string SelectColor { get => Control.SelectColor; set { if (Control.SelectColor != value) { Control.SelectColor = value; Invalidate(); } } }
        public string CategoryColor { get => Control.CategoryColor; set { if (Control.CategoryColor != value) { Control.CategoryColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public bool BackgroundDraw { get => Control.BackgroundDraw; set { if (Control.BackgroundDraw != value) { Control.BackgroundDraw = value; Invalidate(); } } }

        public float ItemHeight { get => Control.ItemHeight; set { if (Control.ItemHeight != value) { Control.ItemHeight = value; Invalidate(); } } }

        [Browsable(false)]
        public ObservableList<GoToolCategory> Categories { get => Control.Categories; set { if (Control.Categories != value) { Control.Categories = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler<ToolItemEventArgs>? DragStart { add => Control.DragStart += value; remove => Control.DragStart -= value; }
        public event EventHandler<ToolItemEventArgs>? ItemClicked { add => Control.ItemClicked += value; remove => Control.ItemClicked -= value; }
        public event EventHandler<ToolItemEventArgs>? ItemLongClicked { add => Control.ItemLongClicked += value; remove => Control.ItemLongClicked -= value; }
        public event EventHandler<ToolItemEventArgs>? ItemDoubleClicked { add => Control.ItemDoubleClicked += value; remove => Control.ItemDoubleClicked -= value; }
        #endregion

        #region Constructor
        public GoToolBox()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
