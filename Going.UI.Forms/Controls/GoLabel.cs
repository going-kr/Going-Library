using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UILabel = Going.UI.Controls.GoLabel;

namespace Going.UI.Forms.Controls
{
    public class GoLabel : GoControl
    {
        #region Properties
        public string? IconString { get => lbl.IconString; set { if (lbl.IconString != value) { lbl.IconString = value; Invalidate(); } } }
        public float IconSize { get => lbl.IconSize; set { if (lbl.IconSize != value) { lbl.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => lbl.IconDirection; set { if (lbl.IconDirection != value) { lbl.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => lbl.IconGap; set { if (lbl.IconGap != value) { lbl.IconGap = value; Invalidate(); } } }

        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public override string Text { get => lbl.Text; set { if (lbl.Text != value) { lbl.Text = value; Invalidate(); } } }
        public string FontName { get => lbl.FontName; set { if (lbl.FontName != value) { lbl.FontName = value; Invalidate(); } } }
        public float FontSize { get => lbl.FontSize; set { if (lbl.FontSize != value) { lbl.FontSize = value; Invalidate(); } } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GoPadding TextPadding
        {
            get => lbl.TextPadding;
            set
            {
                if (lbl.TextPadding.Left != value.Left || lbl.TextPadding.Top != value.Top || 
                    lbl.TextPadding.Right != value.Right || lbl.TextPadding.Bottom != value.Bottom)
                {
                    lbl.TextPadding = value; 
                    Invalidate();
                }
            }
        }

        public GoContentAlignment ContentAlignment { get => lbl.ContentAlignment; set { if (lbl.ContentAlignment != value) { lbl.ContentAlignment = value; Invalidate(); } } }

        public string TextColor { get => lbl.TextColor; set { if (lbl.TextColor != value) { lbl.TextColor = value; Invalidate(); } } }
        public string LabelColor { get => lbl.LabelColor; set { if (lbl.LabelColor != value) { lbl.LabelColor = value; Invalidate(); } } }
        public GoRoundType Round { get => lbl.Round; set { if (lbl.Round != value) { lbl.Round = value; Invalidate(); } } }

        public bool BackgroundDraw { get => lbl.BackgroundDraw; set { if (lbl.BackgroundDraw != value) { lbl.BackgroundDraw = value; Invalidate(); } } }
        public bool BorderOnly { get => lbl.BorderOnly; set { if (lbl.BorderOnly != value) { lbl.BorderOnly = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        UILabel lbl = new UILabel();
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            lbl.Bounds = Util.FromRect(0, 0, Width, Height);
            lbl.Draw(e.Canvas);
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            lbl.MouseDown(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            lbl.MouseUp(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            lbl.MouseMove(e.X, e.Y);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            lbl.MouseDoubleClick(e.X, e.Y, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            lbl.MouseWheel(e.X, e.Y, e.Delta / 120F);
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            lbl.MouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            lbl.MouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            lbl.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            lbl.Visible = Visible;
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }
}
