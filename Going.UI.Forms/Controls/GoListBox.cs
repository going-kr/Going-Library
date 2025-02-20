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
    public class GoListBox : GoControl
    {
        #region Properties
        public float IconSize { get => control.IconSize; set { if (control.IconSize != value) { control.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => control.IconDirection; set { if (control.IconDirection != value) { control.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => control.IconGap; set { if (control.IconGap != value) { control.IconGap = value; Invalidate(); } } }
        public string FontName { get => control.FontName; set { if (control.FontName != value) { control.FontName = value; Invalidate(); } } }
        public float FontSize { get => control.FontSize; set { if (control.FontSize != value) { control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => control.TextColor; set { if (control.TextColor != value) { control.TextColor = value; Invalidate(); } } }
        public string BoxColor { get => control.BoxColor; set { if (control.BoxColor != value) { control.BoxColor = value; Invalidate(); } } }
        public string BorderColor { get => control.BorderColor; set { if (control.BorderColor != value) { control.BorderColor = value; Invalidate(); } } }
        public string SelectedColor { get => control.SelectedColor; set { if (control.SelectedColor != value) { control.SelectedColor = value; Invalidate(); } } }
        public GoRoundType Round { get => control.Round; set { if (control.Round != value) { control.Round = value; Invalidate(); } } }

        public bool BackgroundDraw { get => control.BackgroundDraw; set { if (control.BackgroundDraw != value) { control.BackgroundDraw = value; Invalidate(); } } }

        public float ItemHeight { get => control.ItemHeight; set { if (control.ItemHeight != value) { control.ItemHeight = value; Invalidate(); } } }
        public GoContentAlignment ItemAlignment { get => control.ItemAlignment; set { if (control.ItemAlignment != value) { control.ItemAlignment = value; Invalidate(); } } }

        [Browsable(false)]
        public List<GoListItem> Items { get => control.Items; set { if (control.Items != value) { control.Items = value; Invalidate(); } } }

        [Browsable(false)]
        public List<GoListItem> SelectedItems { get => control.SelectedItems; }
        public GoItemSelectionMode SelectionMode { get => control.SelectionMode; set { if (control.SelectionMode != value) { control.SelectionMode = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? SelectedChanged { add => control.SelectedChanged += value; remove => control.SelectedChanged -= value; }
        public event EventHandler<ListItemClickEventArgs>? ItemClicked { add => control.ItemClicked += value; remove => control.ItemClicked -= value; }
        public event EventHandler<ListItemClickEventArgs>? ItemLongClicked { add => control.ItemLongClicked += value; remove => control.ItemLongClicked -= value; }
        public event EventHandler<ListItemClickEventArgs>? ItemDoubleClicked { add => control.ItemDoubleClicked += value; remove => control.ItemDoubleClicked -= value; }
        #endregion

        #region Member Variable
        Going.UI.Controls.GoListBox control = new Going.UI.Controls.GoListBox();
        #endregion

        #region Constructor
        public GoListBox()
        {
            SetStyle(ControlStyles.Selectable, true);

            control.SetInvalidate(Invalidate);
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            control.Bounds = Util.FromRect(0, 0, Width, Height);
            control.FireDraw(e.Canvas);
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Select();
            control.FireMouseDown(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            control.FireMouseUp(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            control.FireMouseMove(e.X, e.Y);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            control.FireMouseDoubleClick(e.X, e.Y, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            control.FireMouseWheel(e.X, e.Y, e.Delta / 120F);
            Invalidate();
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            control.FireMouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            control.FireMouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion
        #region OnKeyDown
        protected override void OnKeyDown(KeyEventArgs e)
        {
            control.FireKeyDown(e.Shift, e.Control, e.Alt, (GoKeys)e.KeyCode);
            base.OnKeyDown(e);
        }
        #endregion
        #region OnKeyUp
        protected override void OnKeyUp(KeyEventArgs e)
        {
            control.FireKeyDown(e.Shift, e.Control, e.Alt, (GoKeys)e.KeyCode);
            base.OnKeyDown(e);
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            control.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            control.Visible = Visible;
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }
}
