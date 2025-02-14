using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Controls;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Themes;
using UIPanel = Going.UI.Containers.GoPanel;
using SkiaSharp.Views.Desktop;
using System.Windows.Forms.Design;
using Going.UI.Containers;
namespace Going.UI.Forms.Containers
{
    public class GoPanel : Going.UI.Forms.Containers.GoContainer
    {
        #region Properties
        public string? IconString { get => pnl.IconString; set { if (pnl.IconString != value) { pnl.IconString = value; Invalidate(); } } }
        public float IconSize { get => pnl.IconSize; set { if (pnl.IconSize != value) { pnl.IconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => pnl.IconDirection; set { if (pnl.IconDirection != value) { pnl.IconDirection = value; Invalidate(); } } }
        public float IconGap { get => pnl.IconGap; set { if (pnl.IconGap != value) { pnl.IconGap = value; Invalidate(); } } }

        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public override string Text { get => pnl.Text; set { if (pnl.Text != value) { pnl.Text = value; Invalidate(); } } }
        public string FontName { get => pnl.FontName; set { if (pnl.FontName != value) { pnl.FontName = value; Invalidate(); } } }
        public float FontSize { get => pnl.FontSize; set { if (pnl.FontSize != value) { pnl.FontSize = value; Invalidate(); } } }

        public string TextColor { get => pnl.TextColor; set { if (pnl.TextColor != value) { pnl.TextColor = value; Invalidate(); } } }
        public string PanelColor { get => pnl.PanelColor; set { if (pnl.PanelColor != value) { pnl.PanelColor = value; Invalidate(); } } }
        public GoRoundType Round { get => pnl.Round; set { if (pnl.Round != value) { pnl.Round = value; Invalidate(); } } }

        public bool BackgroundDraw { get => pnl.BackgroundDraw; set { if (pnl.BackgroundDraw != value) { pnl.BackgroundDraw = value; Invalidate(); } } }
        public bool BorderOnly { get => pnl.BorderOnly; set { if (pnl.BorderOnly != value) { pnl.BorderOnly = value; Invalidate(); } } }

        private string sBackgroundColor = "Back";
        public string BackgroundColor
        {
            get => sBackgroundColor;
            set
            {
                if (sBackgroundColor != value)
                {
                    sBackgroundColor = value;

                    var c = GoTheme.Current.ToColor(BackgroundColor);
                    this.BackColor = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);

                    Invalidate();
                }
            }
        }
        #endregion

        #region Member Variable
        UIPanel pnl = new UIPanel();
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            pnl.Bounds = Util.FromRect(0, 0, Width, Height);
            pnl.Draw(e.Canvas);
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            pnl.MouseDown(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            pnl.MouseUp(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            pnl.MouseMove(e.X, e.Y);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            pnl.MouseDoubleClick(e.X, e.Y, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            pnl.MouseWheel(e.X, e.Y, e.Delta / 120F);
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            pnl.MouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            pnl.MouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            pnl.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            pnl.Visible = Visible;
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }
     
}
