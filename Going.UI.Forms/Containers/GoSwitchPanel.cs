using Going.UI.Forms.Tools;
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

                    var c = GoThemeW.Current.ToColor(BackgroundColor);
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

            var thm = GoThemeW.Current;
            var vc = Util.FromArgb(ColorTool.EnableColor(this, thm.ToColor(BackgroundColor)));
            e.Graphics.Clear(vc);

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
            var thm = GoThemeW.Current;
            var vc = Util.FromArgb(ColorTool.EnableColor(this, thm.ToColor(BackgroundColor)));

            foreach (var v in TabPages.Cast<TabPage>())
            {
                if (v.BackColor != vc) v.BackColor = vc;
                if (v.BorderStyle != BorderStyle.None) v.BorderStyle = BorderStyle.None;
            }
        }
        #endregion 
        #endregion
    }
}
