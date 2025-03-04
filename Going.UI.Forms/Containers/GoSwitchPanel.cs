using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Going.UI.Forms.Containers
{
    public class GoSwitchPanel : TabControl
    {
        #region Properties
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new TabPage SelectedTab { get => base.SelectedTab; set { base.SelectedTab = value; } }

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
        private string sBackgroundColor = "Back";
        private Color cParentColor = Color.White;
        #endregion

        #region Constructor
        public GoSwitchPanel()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            this.Appearance = TabAppearance.FlatButtons;
            this.ItemSize = new Size(0, 1);
            this.SizeMode = TabSizeMode.Fixed;

            this.Multiline = false;
        }
        #endregion

        #region Override
        #region WndProc
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328) return;
            base.WndProc(ref m);
        }
        #endregion
        #region OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(GetBackColor());

            RefreshPages();
        }
        #endregion
      
        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            Invalidate();
            RefreshPages();
        }
        #endregion
        #endregion

        #region Method
        #region RefreshPages
        void RefreshPages()
        {
            var vc = GetBackColor();

            foreach (var v in TabPages.Cast<TabPage>())
            {
                if (v.BackColor != vc) v.BackColor = vc;
                if (v.BorderStyle != BorderStyle.None) v.BorderStyle = BorderStyle.None;
            }
        }
        #endregion

        #region GetBackColor
        Color GetBackColor()
        {
            var thm = GoTheme.Current;
            var c = Util.FromArgb(GoTheme.Current.ToColor(BackgroundColor));
            var bc = Util.FromArgb(thm.Back);

            if (Parent is GoGroupBox grp)
                bc = Util.FromArgb(thm.ToColor(grp.BackgroundColor));
            else if (Parent is GoPanel pnl)
                bc = Enabled ? Util.FromArgb(thm.ToColor(pnl.PanelColor)) : MixColorAlpha(Util.FromArgb(thm.ToColor(pnl.BackgroundColor)), Util.FromArgb(thm.ToColor(pnl.PanelColor)), 255 - GoTheme.DisableAlpha);
            else if (Parent is GoScrollablePanel spnl)
                bc = Util.FromArgb(thm.ToColor(spnl.BackgroundColor));
            else if (Parent is TabPage page1 && page1.Parent is GoSwitchPanel sw)
                bc = Util.FromArgb(thm.ToColor(sw.BackgroundColor));
            else if (Parent is TabPage page2 && page2.Parent is GoTabControl tab)
                bc = Enabled ? Util.FromArgb(thm.ToColor(tab.TabColor)) : MixColorAlpha(Util.FromArgb(thm.ToColor(tab.BackgroundColor)), Util.FromArgb(thm.ToColor(tab.TabColor)), 255 - GoTheme.DisableAlpha);
            else if (Parent is GoTableLayoutPanel tpnl)
                bc = Util.FromArgb(thm.ToColor(tpnl.BackgroundColor));
            else if (Parent is GoContainer con)
                bc = Util.FromArgb(thm.ToColor(con.BackgroundColor));
            else
                bc = Parent?.BackColor ?? Util.FromArgb(thm.Back);

            var vc = Enabled ? c : bc;

            return vc;
        }
        #endregion

        #region MixColorAlpha
        Color MixColorAlpha(Color dest, Color src, int srcAlpha)
        {
            byte red = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.R, (int)src.R), 0.0, 255.0));
            byte green = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.G, (int)src.G), 0.0, 255.0));
            byte blue = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.B, (int)src.B), 0.0, 255.0));
            return Color.FromArgb(red, green, blue);
        }
        #endregion
        #endregion
    }
}
