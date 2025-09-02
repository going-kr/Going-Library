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
using Going.UI.Forms.Controls;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

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

                    var thm = GoThemeW.Current;
                    BackColor = Util.FromArgb(thm.ToColor(this.BackgroundColor));

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

        #region Constructor
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
            BackColor = Util.FromArgb(GoThemeW.Current.Back);
            Font = new Font("나눔고딕", 9);
            ResizeRedraw = true;
            #endregion

            var thm = GoThemeW.Current;
            BackColor = Util.FromArgb(thm.ToColor(this.BackgroundColor));
        }
        #endregion

        #region Override
        #region OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            var thm = GoThemeW.Current;

            using (var bitmap = new SKBitmap(this.Width, this.Height))
            using (var canvas = new SKCanvas(bitmap))
            using (var surface = SKSurface.Create(bitmap.Info))
            {
                var cBack = GoThemeW.Current.ToColor(BackgroundColor);
                canvas.Clear(cBack);

                using var p2 = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                var sp = canvas.SaveLayer(p2);

                OnContentDraw(new ContentDrawEventArgs(canvas));

                canvas.RestoreToCount(sp);

                using (var bmp = bitmap.ToBitmap())
                    e.Graphics.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
            }

            base.OnPaint(e);
        }
        #endregion
        #region OnShown
        protected override void OnShown(EventArgs e)
        {
            if (!DesignMode) DarkMode(this.Handle, GoThemeW.Current.Dark);
            base.OnShown(e);
        }
        #endregion
        #region OnLoad
        protected override void OnLoad(EventArgs e)
        {
            if (!DesignMode)
            {
                DarkMode(this.Handle, GoThemeW.Current.Dark);
                Icon = IconTool.GetIcon(TitleIconString, GoThemeW.Current.Fore);
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

        public virtual void OnContentDraw(ContentDrawEventArgs e) { }
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

        #region ToGoMouseButton
        protected GoMouseButton ToGoMouseButton(MouseButtons btn)
        {
            GoMouseButton ret = GoMouseButton.Left;
            if (btn == MouseButtons.Left) ret = GoMouseButton.Left;
            else if (btn == MouseButtons.Right) ret = GoMouseButton.Right;
            else if (btn == MouseButtons.Middle) ret = GoMouseButton.Middle;
            return ret;
        }
        #endregion
        #endregion
    }
}
