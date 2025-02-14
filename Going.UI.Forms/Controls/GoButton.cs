using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIButton = Going.UI.Controls.GoButton;

namespace Going.UI.Forms.Controls
{
    public class GoButton : GoControl
    {
        #region Properties
        public string? IconString { get => btn.IconString; set { if (btn.IconString != value) { btn.IconString = value; Invalidate(); } } }
        public float IconSize { get => btn.IconSize; set { if (btn.IconSize != value) { btn.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => btn.IconDirection; set { if (btn.IconDirection != value) { btn.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => btn.IconGap; set { if (btn.IconGap != value) { btn.IconGap = value; Invalidate(); } } }
        public override string Text { get => btn.Text; set { if (btn.Text != value) { btn.Text = value; Invalidate(); } } }
        public string FontName { get => btn.FontName; set { if (btn.FontName != value) { btn.FontName = value; Invalidate(); } } }
        public float FontSize { get => btn.FontSize; set { if (btn.FontSize != value) { btn.FontSize = value; Invalidate(); } } }

        public string TextColor { get => btn.TextColor; set { if (btn.TextColor != value) { btn.TextColor = value; Invalidate(); } } }
        public string ButtonColor { get => btn.ButtonColor; set { if (btn.ButtonColor != value) { btn.ButtonColor = value; Invalidate(); } } }
        public GoRoundType Round { get => btn.Round; set { if (btn.Round != value) { btn.Round = value; Invalidate(); } } }

        public bool BackgroundDraw { get => btn.BackgroundDraw; set { if (btn.BackgroundDraw != value) { btn.BackgroundDraw = value; Invalidate(); } } }
        public bool BorderOnly { get => btn.BorderOnly; set { if (btn.BackgroundDraw != value) { btn.BorderOnly = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler ButtonClicked { add => btn.ButtonClicked += value; remove => btn.ButtonClicked -= value; }
        #endregion

        #region Member Variable
        UIButton btn = new();
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            btn.Bounds = Util.FromRect(0, 0, Width, Height);
            btn.Draw(e.Canvas);
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            btn.MouseDown(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            btn.MouseUp(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            btn.MouseMove(e.X, e.Y);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            btn.MouseDoubleClick(e.X, e.Y, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            btn.MouseWheel(e.X, e.Y, e.Delta / 120F);
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            btn.MouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            btn.MouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion
        #endregion
    }
}
