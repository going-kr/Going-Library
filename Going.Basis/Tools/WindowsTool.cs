using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Tools
{
    /// <summary>
    /// Windows 시스템 유틸리티 클래스.
    /// 시스템 시간 설정, 절전 모드 방지/허용 기능을 제공한다. Win32 API를 사용한다.
    /// </summary>
    public class WindowsTool
    {
        #region SetLocalTime
        /// <summary>시스템 로컬 시간을 설정한다. 관리자 권한이 필요하다.</summary>
        /// <param name="Time">설정할 시간</param>
        public static void SetLocalTime(DateTime Time)
        {
            var v = new SYSTEMTIME()
            {
                wYear = (short)Time.Year,
                wMonth = (short)Time.Month,
                wDay = (short)Time.Day,
                //wDayOfWeek = (short)Time.DayOfWeek,
                wHour = (short)Time.Hour,
                wMinute = (short)Time.Minute,
                wSecond = (short)Time.Second,
                wMilliseconds = (short)Time.Millisecond,
            };
            Win32Tool.SetLocalTime(ref v);
        }
        #endregion
        #region SetSystemTime
        /// <summary>시스템 UTC 시간을 설정한다. 관리자 권한이 필요하다.</summary>
        /// <param name="Time">설정할 UTC 시간</param>
        public static void SetSystemTime(DateTime Time)
        {
            var v = new SYSTEMTIME()
            {
                wYear = (short)Time.Year,
                wMonth = (short)Time.Month,
                wDay = (short)Time.Day,
                //wDayOfWeek = (short)Time.DayOfWeek,
                wHour = (short)Time.Hour,
                wMinute = (short)Time.Minute,
                wSecond = (short)Time.Second,
                wMilliseconds = (short)Time.Millisecond,
            };
            Win32Tool.SetSystemTime(ref v);
        }
        #endregion

        #region PreventSleep
        /// <summary>시스템 절전 모드 및 화면 꺼짐을 방지한다.</summary>
        public static void PreventSleep()
        {
            Win32Tool.SetThreadExecutionState(ExecutionStateKinds.ES_CONTINUOUS |
                                                ExecutionStateKinds.ES_SYSTEM_REQUIRED |
                                                ExecutionStateKinds.ES_AWAYMODE_REQUIRED |
                                                ExecutionStateKinds.ES_DISPLAY_REQUIRED);
        }
        #endregion
        #region AllowSleep
        /// <summary>절전 모드 방지를 해제하여 시스템이 정상적으로 절전될 수 있도록 한다.</summary>
        public static void AllowSleep()
        {
            Win32Tool.SetThreadExecutionState(ExecutionStateKinds.ES_CONTINUOUS);
        }
        #endregion
    }

    #region class : Win32Tool
    internal class Win32Tool
    {
        #region Const
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int WM_NCLBUTTONDBLCLK = 0xA3;
        public const int HT_CAPTION = 0x2;
        #endregion

        #region Interop
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessagePC(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        public static extern bool ReleaseCapturePC();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ExecutionStateKinds SetThreadExecutionState(ExecutionStateKinds esFlags);

        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetLocalTime(ref SYSTEMTIME st);
        #endregion

        #region Method
        public static int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix) return -1;
            else return SendMessagePC(hWnd, Msg, wParam, lParam);
        }

        public static bool ReleaseCapture()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix) { return false; }
            else return ReleaseCapturePC();
        }
        #endregion
    }

    [Flags]
    internal enum ExecutionStateKinds : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }
    #endregion
}
