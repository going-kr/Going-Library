using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Tools
{
    public class DwmTool
    {
        #region Const
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_MICA_EFFECT = 1029;
        private const int WM_CREATE = 0x0001;
        internal const int WS_THICKFRAME = 0x40000;
        internal const int WS_BORDER = 0x800000;
        internal const int WS_CAPTION = 0xC00000;
        internal const int BORDER_WIDTH = 7;
        #endregion

        #region Arguments
        #region struct : RECT
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion
        #region struct : PAINTSTRUCT
        [StructLayout(LayoutKind.Sequential)]
        internal struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
        }
        #endregion
        #region struct : Margins
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        };
        #endregion
        #region struct : WTA_OPTIONS
        public struct WTA_OPTIONS
        {
            public uint Flags;
            public uint Mask;
        }
        #endregion
        #region enum : SetWindowPosFlag
        [Flags]
        private enum SetWindowPosFlag : uint
        {
            SWP_ASYNCWINDOWPOS = 0x4000,
            SWP_DEFERERASE = 0x2000,
            SWP_DRAWFRAME = 0x0020,
            SWP_FRAMECHANGED = 0x0020,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOACTIVATE = 0x0010,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOMOVE = 0x0002,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOREDRAW = 0x0008,
            SWP_NOREPOSITION = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_NOSIZE = 0x0001,
            SWP_NOZORDER = 0x0004,
            SWP_SHOWWINDOW = 0x0040,
        }
        #endregion
        #region enum : WindowThemeAttributeType
        public enum WindowThemeAttributeType : uint
        {
            /// <summary>Non-client area window attributes will be set.</summary>
            WTA_NONCLIENT = 1,
        }
        #endregion
        #endregion

        #region Interop
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr windowHandle, IntPtr windowHandleInsertAfter, int x, int y, int width, int height, SetWindowPosFlag flag);

        [DllImport("user32")]
        private static extern int GetWindowRect(int hwnd, ref RECT lpRect);

        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        private static extern Int32 SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);

        [DllImport("user32.dll")]
        private static extern void DisableProcessWindowsGhosting();

        [DllImport("DwmApi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        [DllImport("uxtheme.dll")]
        private static extern int SetWindowThemeAttribute(IntPtr hWnd, WindowThemeAttributeType wtype, ref WTA_OPTIONS attributes, uint size);

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("user32.dll")]
        internal static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        internal static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("User32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr hWnd);

        #endregion

        #region Method
        #region SetTheme
        private static bool IsWindows10OrGreater(int build = -1)
        {
            var version = Environment.OSVersion.Version;
            return version.Major >= 10 && version.Build >= build;
        }

        public static void SetTheme(Form form, bool isDarkMode)
        {
            if (IsWindows10OrGreater(17763))
            {
                int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = isDarkMode ? 1 : 0;
                DwmSetWindowAttribute(form.Handle, attribute, ref useImmersiveDarkMode, sizeof(int));
                DwmSetWindowAttribute(form.Handle, DWMWA_MICA_EFFECT, ref useImmersiveDarkMode, sizeof(int));
            }
        }

        public static void SetTheme(IntPtr handle, bool isDarkMode)
        {
            if (IsWindows10OrGreater(17763))
            {
                int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = isDarkMode ? 1 : 0;
                DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int));
                DwmSetWindowAttribute(handle, DWMWA_MICA_EFFECT, ref useImmersiveDarkMode, sizeof(int));
            }
        }
        public static void SetDarkMode(Form Wnd, bool isDarkMode)
        {
            int value = isDarkMode ? 1 : 0;
            SetTheme(Wnd, isDarkMode);
            SetWindowTheme(Wnd.Handle, isDarkMode ? "DarkMode_Explorer" : "Explorer", null);
            DwmSetWindowAttribute(Wnd.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, Marshal.SizeOf(typeof(bool)));

        }

        public static void SetDarkMode(IntPtr handle, bool isDarkMode)
        {
            int value = isDarkMode ? 1 : 0;
            SetTheme(handle, isDarkMode);
            SetWindowTheme(handle, isDarkMode ? "DarkMode_Explorer" : "Explorer", null);
            DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, Marshal.SizeOf(typeof(bool)));

        }
        #endregion

        #region Drawable
        /// <summary>
        /// IntializeComponent 전에
        /// </summary>
        public static void Drawable1() => DisableProcessWindowsGhosting();

        /// <summary>
        /// OnHandleCreated 안에
        /// </summary>
        /// <param name="Handle"></param>
        public static void Drawable2(IntPtr Handle) => SetWindowTheme(Handle, "", "");
        #endregion

        #region ExtendFrame
        public static void ExtendFrame(IntPtr Handle, int Left, int Top, int Right, int Bottom)
        {
            var v = new MARGINS() { cxLeftWidth = Left, cyTopHeight = Top, cxRightWidth = Right, cyBottomHeight = Bottom };
            var r = DwmExtendFrameIntoClientArea(Handle, ref v);
        }
        #endregion

        #region RemoveTitleIcon - SetPos GetRect
        public static void RemoveTitleIcon(IntPtr Handle, int Msg)
        {
            if (Msg == WM_CREATE)
            {
                var rc = new RECT();
                GetWindowRect(Handle.ToInt32(), ref rc);
                SetWindowPos(Handle, IntPtr.Zero, rc.Right - rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top, SetWindowPosFlag.SWP_FRAMECHANGED);
            }
        }
        #endregion

        #region EnableAero
        public static void EnableAero(bool b)
        {
            DwmIsCompositionEnabled(out b);
        }
        #endregion
        #endregion
    }
}
