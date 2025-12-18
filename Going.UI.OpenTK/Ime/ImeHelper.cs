using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Going.UI.OpenTK.Ime
{
    public static class ImeHelper
    {
        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll")]
        private static extern int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);

        [DllImport("imm32.dll")]
        private static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

        [DllImport("imm32.dll")]
        private static extern bool ImmSetCandidateWindow(IntPtr hIMC, ref CANDIDATEFORM lpCandidate);

        [DllImport("imm32.dll")]
        private static extern bool ImmSetOpenStatus(IntPtr hIMC, bool fOpen);

        [DllImport("imm32.dll")]
        private static extern bool ImmGetOpenStatus(IntPtr hIMC);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        private const int GCS_COMPSTR = 0x0008;
        private const int GCS_COMPATTR = 0x0010;
        private const int GCS_CURSORPOS = 0x0080;
        private const int GCS_RESULTSTR = 0x0800;

        private const int CFS_POINT = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COMPOSITIONFORM
        {
            public int dwStyle;
            public POINT ptCurrentPos;
            public RECT rcArea;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CANDIDATEFORM
        {
            public int dwIndex;
            public int dwStyle;
            public POINT ptCurrentPos;
            public RECT rcArea;
        }

        public static string GetCompositionString(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return string.Empty;

            IntPtr hIMC = ImmGetContext(hwnd);
            if (hIMC == IntPtr.Zero) return string.Empty;

            try
            {
                int length = ImmGetCompositionStringW(hIMC, GCS_COMPSTR, null, 0);
                if (length <= 0) return string.Empty;

                byte[] buffer = new byte[length];
                ImmGetCompositionStringW(hIMC, GCS_COMPSTR, buffer, length);

                return Encoding.Unicode.GetString(buffer);
            }
            finally
            {
                ImmReleaseContext(hwnd, hIMC);
            }
        }

        public static int GetCompositionCursorPos(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return 0;

            IntPtr hIMC = ImmGetContext(hwnd);
            if (hIMC == IntPtr.Zero) return 0;

            try
            {
                return ImmGetCompositionStringW(hIMC, GCS_CURSORPOS, null, 0);
            }
            finally
            {
                ImmReleaseContext(hwnd, hIMC);
            }
        }

        public static string GetResultString(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return string.Empty;

            IntPtr hIMC = ImmGetContext(hwnd);
            if (hIMC == IntPtr.Zero) return string.Empty;

            try
            {
                int length = ImmGetCompositionStringW(hIMC, GCS_RESULTSTR, null, 0);
                if (length <= 0) return string.Empty;

                byte[] buffer = new byte[length];
                ImmGetCompositionStringW(hIMC, GCS_RESULTSTR, buffer, length);

                return Encoding.Unicode.GetString(buffer);
            }
            finally
            {
                ImmReleaseContext(hwnd, hIMC);
            }
        }

        public static void SetCompositionWindowPosition(IntPtr hwnd, int x, int y)
        {
            if (hwnd == IntPtr.Zero) return;

            IntPtr hIMC = ImmGetContext(hwnd);
            if (hIMC == IntPtr.Zero) return;

            try
            {
                COMPOSITIONFORM cf = new COMPOSITIONFORM
                {
                    dwStyle = CFS_POINT,
                    ptCurrentPos = new POINT { x = x, y = y }
                };

                ImmSetCompositionWindow(hIMC, ref cf);

                CANDIDATEFORM candidate = new CANDIDATEFORM
                {
                    dwIndex = 0,
                    dwStyle = CFS_POINT,
                    ptCurrentPos = new POINT { x = x, y = y + 20 }
                };

                ImmSetCandidateWindow(hIMC, ref candidate);
            }
            finally
            {
                ImmReleaseContext(hwnd, hIMC);
            }
        }

        private const int CFS_EXCLUDE = 0x0080;

        public static void HideImeCompositionWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;
            IntPtr hIMC = ImmGetContext(hwnd);
            if (hIMC == IntPtr.Zero) return;

            try
            {
                COMPOSITIONFORM cf = new COMPOSITIONFORM
                {
                    dwStyle = 0x0002, // CFS_POINT
                    ptCurrentPos = new POINT { x = -10000, y = -10000 }
                };
                ImmSetCompositionWindow(hIMC, ref cf);

                CANDIDATEFORM candidate = new CANDIDATEFORM
                {
                    dwIndex = 0,
                    dwStyle = CFS_EXCLUDE,
                    ptCurrentPos = new POINT { x = -10000, y = -10000 },
                    rcArea = new RECT { left = -10000, top = -10000, right = -9999, bottom = -9999 }
                };
                ImmSetCandidateWindow(hIMC, ref candidate);
            }
            finally
            {
                ImmReleaseContext(hwnd, hIMC);
            }
        }
    }
}
