using Going.UI.Forms.Tools;
using Going.UI.Enums;
using Going.UI.Icons;
using Going.UI.Themes;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Going.UI.Forms.Input;

namespace Going.UI.Forms.Dialogs
{
    public partial class GoForm : Form
    {
        #region Properties
        #region Title
        public string Title
        {
            get => base.Text;
            set
            {
                if (base.Text != value)
                {
                    base.Text = value;
                    Invalidate();
                }
            }
        }

        public override string Text
        {
            get => base.Text;
            set
            {
                if (base.Text != value)
                {
                    base.Text = value;
                    Invalidate();
                }
            }
        }
        #endregion
        #region TitleIcon
        public string? TitleIconString
        {
            get => strIconString;
            set { if (strIconString != value) { strIconString = value; Invalidate(); } }
        }

        public float TitleIconSize
        {
            get => nIconSize;
            set { if (nIconSize != value) { nIconSize = value; Invalidate(); } }
        }
        #endregion
        #region BackgroundColor
        private string cBackColor = "Back";
        public string BackgroundColor
        {
            get => cBackColor;
            set
            {
                if (cBackColor != value)
                {
                    cBackColor = value;
                    Invalidate();
                }
            }
        }
        #endregion
        #region TextColor
        private string cTextColor = "Fore";
        public string TextColor
        {
            get => cTextColor;
            set
            {
                if (cTextColor != value)
                {
                    cTextColor = value;
                    Invalidate();
                }
            }
        }
        #endregion
        #endregion

        #region Member Variable
        private bool useTrayicon = false;
        private NotifyIcon? notifyIcon;
        private string? strIconString;
        private float nIconSize = 12;
        #endregion

        #region Interop
        private static readonly int GWL_EXSTYLE = -20;
        private static readonly int WS_EX_COMPOSITED = 0x02000000;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_BACKGROUND_COLOR = 26;

        [DllImport("user32")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left, top, right, bottom;
        }
        #endregion

        #region GoForm
        public GoForm()
        {
            InitializeComponent();

            #region Styles
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.Opaque, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            UpdateStyles();
            #endregion
            #region Properties
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            BackColor = Util.FromArgb(GoTheme.Current.Back);
            Font = new Font("나눔고딕", 9);
            ResizeRedraw = true;
            #endregion

            var v = FormsInputManager.Current;

        }
        #endregion

        #region Override
        #region OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            var thm = GoTheme.Current;
            var BackColor = Util.FromArgb(thm.ToColor(this.BackgroundColor));
            e.Graphics.Clear(BackColor);

            OnContentDraw(e);

            base.OnPaint(e);
        }
        #endregion
        #region OnShown
        protected override void OnShown(EventArgs e)
        {
            if (!DesignMode) DarkMode(this.Handle, GoTheme.Current.Dark);

            base.OnShown(e);
        }
        #endregion
        #region OnLoad
        protected override void OnLoad(EventArgs e)
        {
            if (!DesignMode)
            {
                DarkMode(this.Handle, GoTheme.Current.Dark);
                Icon = IconTool.GetIcon(TitleIconString, GoTheme.Current.Fore);
            }
            base.OnLoad(e);
        }
        #endregion
        #region OnClosing
        protected override void OnClosing(CancelEventArgs e)
        {
            if (useTrayicon)
            {
                this.Visible = false;
                this.ShowIcon = false;
                if (notifyIcon != null) notifyIcon.Visible = true;
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
        #endregion

        public virtual void OnContentDraw(PaintEventArgs e) { }
        #endregion

        #region Method
        #region DarkMode
        void DarkMode(IntPtr hwnd, bool dark)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                int darkMode = dark ? 1 : 0;
                int result = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            }
        }
        #endregion
        #endregion
    }
}
