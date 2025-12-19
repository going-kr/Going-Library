using Going.UI.Controls;
using Going.UI.Managers;
using Going.UI.Themes;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.OpenTK.Ime
{
    internal unsafe class ImeInputManager
    {
        #region Const
        private const int GWL_WNDPROC = -4;
        private const uint WM_IME_COMPOSITION = 0x010F;
        private const uint WM_IME_ENDCOMPOSITION = 0x010E;
        private const uint WM_IME_STARTCOMPOSITION = 0x010D;
        private const uint WM_IME_SETCONTEXT = 0x0028;
        private const uint WM_IME_CHAR = 0x0286;
        private const uint WM_IME_NOTIFY = 0x0282;
        private const int CFS_EXCLUDE = 0x0080;
        #endregion

        #region interop
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        #endregion

        #region Member Variable
        private static readonly Lazy<ImeInputManager> _instance = new Lazy<ImeInputManager>(() => new ImeInputManager());
        public static ImeInputManager Current => _instance.Value;
        public bool IsInput => InputControl != null;
        public IGoControl? InputControl => GoInputEventer.Current.InputControl;

        private TextBox txt;
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private IntPtr _oldWndProc;
        private WndProcDelegate _newWndProc;
        private IntPtr _hwnd;
        private bool _isMouseDown;
        private bool _isDragging;
        private Stopwatch _stopwatch;
        private float _lastTime;
        #endregion

        #region Constructor
        public ImeInputManager()
        {
            txt = new TextBox();
        }
        #endregion

        #region Init
        public void Init(Window* WindowPtr)
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    _stopwatch = Stopwatch.StartNew();
                    _lastTime = 0;

                    _hwnd = GLFW.GetWin32Window(WindowPtr);
                    _newWndProc = new WndProcDelegate(WndProc);
                    _oldWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
                }
                catch (Exception ex)
                {
                }
            }
        }
        #endregion
        #region WndProc
        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_IME_SETCONTEXT:
                    return CallWindowProc(_oldWndProc, hWnd, msg, wParam, IntPtr.Zero);

                case WM_IME_STARTCOMPOSITION:
                    ImeHelper.HideImeCompositionWindow(hWnd);
                    break;

                case WM_IME_COMPOSITION:
                    if (txt != null && txt.IsFocused)
                    {
                        if (((long)lParam & 0x0008) != 0)
                            txt.OnIMEComposition(ImeHelper.GetCompositionString(hWnd), ImeHelper.GetCompositionCursorPos(hWnd));

                        if (((long)lParam & 0x0800) != 0)
                            txt.OnIMECommit(ImeHelper.GetResultString(hWnd));
                    }

                    if (((long)lParam & 0x0008) != 0) return IntPtr.Zero;
                    break;

                case WM_IME_ENDCOMPOSITION:
                    if (txt != null && txt.IsFocused)
                        txt.OnIMECommit("");
                    break;
            }

            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }
        #endregion

        #region OnDraw
        public void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            if(txt.IsFocused)
            {
                txt.Draw(canvas, thm);
            }
        }
        #endregion
        #region OnUpdate
        public void OnUpdate()
        {
            if (txt.IsFocused)
            {
                txt.Update();
            }
        }
        #endregion
        #region OnTextInput
        public void OnTextInput(TextInputEventArgs e)
        {
            if (txt.IsFocused)
            {
                string text = char.ToString((char)e.Unicode);
                txt.OnTextInput(text);
            }
        }
        #endregion
        #region OnKeyDown
        public void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (txt.IsFocused) txt.OnKeyPress(e, null);
        }
        #endregion
        #region OnMouseDown
        public void OnMouseDown(MouseState mouseState, KeyboardState keyboardState, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                _isMouseDown = true;
                _isDragging = false;

                var mousePos = mouseState.Position;
                bool isShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

                if (txt.IsFocused)
                    txt.OnMouseDown(mousePos.X, mousePos.Y, isShift);
            }
        }
        #endregion
        #region OnMouseUp
        public void OnMouseUp(MouseState mouseState, KeyboardState keyboardState, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                _isMouseDown = false;
                _isDragging = false;
            }
        }
        #endregion
        #region OnMouseMove
        public void OnMouseMove(MouseState mouseState, KeyboardState keyboardState)
        {
            if (_isMouseDown)
            {
                _isDragging = true;

                var mousePos = mouseState.Position;

                if (txt.IsFocused)
                    txt.OnMouseDrag(mousePos.X, mousePos.Y);
            }
        }
        #endregion

        #region Input
        public void InputString(IGoControl control, SKRect bounds, Action<string> callback, string? value = null)
        {
            txt.InputString(control, bounds, callback, value);
        }

        public void InputNumber(IGoControl control, SKRect bounds, Action<string> callback, Type valueType, object value, object? min, object? max)
        {
            txt.InputNumber(control, bounds, callback, valueType, value, min, max);
        }
        #endregion
     
    }
}
