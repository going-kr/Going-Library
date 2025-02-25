using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoListBox : GoWrapperControl<Going.UI.Controls.GoListBox>
    {
        #region Properties
        public float IconSize { get => Control.IconSize; set { if (Control.IconSize != value) { Control.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => Control.IconDirection; set { if (Control.IconDirection != value) { Control.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => Control.IconGap; set { if (Control.IconGap != value) { Control.IconGap = value; Invalidate(); } } }
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string BoxColor { get => Control.BoxColor; set { if (Control.BoxColor != value) { Control.BoxColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }
        public string SelectedColor { get => Control.SelectedColor; set { if (Control.SelectedColor != value) { Control.SelectedColor = value; Invalidate(); } } }
        public GoRoundType Round { get => Control.Round; set { if (Control.Round != value) { Control.Round = value; Invalidate(); } } }

        public bool BackgroundDraw { get => Control.BackgroundDraw; set { if (Control.BackgroundDraw != value) { Control.BackgroundDraw = value; Invalidate(); } } }

        public float ItemHeight { get => Control.ItemHeight; set { if (Control.ItemHeight != value) { Control.ItemHeight = value; Invalidate(); } } }
        public GoContentAlignment ItemAlignment { get => Control.ItemAlignment; set { if (Control.ItemAlignment != value) { Control.ItemAlignment = value; Invalidate(); } } }

        [Browsable(false)]
        public List<GoListItem> Items { get => Control.Items; set { if (Control.Items != value) { Control.Items = value; Invalidate(); } } }

        [Browsable(false)]
        public List<GoListItem> SelectedItems { get => Control.SelectedItems; }
        public GoItemSelectionMode SelectionMode { get => Control.SelectionMode; set { if (Control.SelectionMode != value) { Control.SelectionMode = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? SelectedChanged { add => Control.SelectedChanged += value; remove => Control.SelectedChanged -= value; }
        public event EventHandler<ListItemClickEventArgs>? ItemClicked { add => Control.ItemClicked += value; remove => Control.ItemClicked -= value; }
        public event EventHandler<ListItemClickEventArgs>? ItemLongClicked { add => Control.ItemLongClicked += value; remove => Control.ItemLongClicked -= value; }
        public event EventHandler<ListItemClickEventArgs>? ItemDoubleClicked { add => Control.ItemDoubleClicked += value; remove => Control.ItemDoubleClicked -= value; }
        #endregion

        #region Constructor
        public GoListBox()
        {
            SetStyle(ControlStyles.Selectable, true);
        }
        #endregion
    }
}
