using OpenTK.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GLES = OpenTK.Graphics.ES30.GL;
using GL = OpenTK.Graphics.OpenGL.GL;
using GetPNameES = OpenTK.Graphics.ES30.GetPName;
using GetPName = OpenTK.Graphics.OpenGL.GetPName;
using RenderbufferTargetES = OpenTK.Graphics.ES30.RenderbufferTarget;
using RenderbufferTarget = OpenTK.Graphics.OpenGL.RenderbufferTarget;
using RenderbufferParameterNameES = OpenTK.Graphics.ES30.RenderbufferParameterName;
using RenderbufferParameterName = OpenTK.Graphics.OpenGL.RenderbufferParameterName;
using Going.UI.Enums;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Going.UI.Controls;
using System.Reflection.Metadata;
using System.Drawing;
using Going.UI.Utils;
using Going.UI.Datas;
using Going.UI.Containers;
using Going.UI.Themes;
using OpenTK.Windowing.Common.Input;
using Image = OpenTK.Windowing.Common.Input.Image;
using Going.UI.OpenTK.Input;
using Going.UI.Managers;
using System.Security.Cryptography.X509Certificates;
using Going.UI.Design;

namespace Going.UI.OpenTK.Windows
{
    public class GoViewWindow : GameWindow
    {
        #region Properties
        public int Width => ClientRectangle.Size.X;
        public int Height => ClientRectangle.Size.Y;

        public string? TItleIconString { get; set; }

        public bool Debug { get; set; } = false;

        public GoDesign Design { get; private set; } = new GoDesign();
        #endregion

        #region Member Variable
        private GRGlInterface? gli;
        private GRContext? ctx;
        private GRBackendRenderTarget? target;
        private SKSurface? surface;
        private bool IsFirstRender = true;
        private nint Handle;
        private DateTime dcTime = DateTime.Now;
        //private float DPI_H, DPI_V;
        #endregion

        #region Constructor
        public GoViewWindow(int width, int height, WindowBorder Style)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                WindowBorder = Style,
                ClientSize = new Vector2i(width, height),
                StartVisible = false,
                API = Environment.OSVersion.Platform == PlatformID.Unix ? ContextAPI.OpenGLES : ContextAPI.OpenGL,
                APIVersion = Environment.OSVersion.Platform == PlatformID.Unix ? new Version(3, 1) : new Version(3, 2)
            })
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix) WindowState = WindowState.Fullscreen;

            GoInputEventer.Current.InputString += OpenTKInputManager.Current.InputString;
            GoInputEventer.Current.InputNumber += OpenTKInputManager.Current.InputNumber;
        }
        #endregion

        #region Override
        #region OnLoad
        protected override void OnLoad()
        {
            var bnd = new GLFWBindingsContext();
            GL.LoadBindings(bnd);

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                gli = GRGlInterface.CreateGles(new GRGlGetProcedureAddressDelegate((name) => bnd.GetProcAddress(name)));
            else
                gli = GRGlInterface.CreateOpenGl(new GRGlGetProcedureAddressDelegate((name) => bnd.GetProcAddress(name)));

            ctx = GRContext.CreateGl(gli);
            target = CreateRenderTarget();
            surface = SKSurface.Create(ctx, target, SKColorType.Rgba8888);

            base.OnLoad();

            unsafe
            {
                Handle = GLFW.GetWin32Window(WindowPtr);
                DarkMode(Handle, true);
            }
        }
        #endregion
        #region OnUnload
        protected override void OnUnload()
        {
            base.OnUnload();
            gli?.Dispose(); gli = null;
            target?.Dispose(); target = null;
            ctx?.Dispose(); ctx = null;
            surface?.Dispose(); surface = null;
        }
        #endregion
        #region OnRenderFrame
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //if (TryGetCurrentMonitorDpi(out var dpiH, out var dpiV)) { DPI_H = dpiH / 96F; DPI_V = dpiV / 96F; }

            Design.SetSize(Width, Height);
            base.OnRenderFrame(e);

            if (ctx != null && target != null && surface != null)
            {
                var canvas = surface.Canvas;
                using (new SKAutoCanvasRestore(canvas))
                {
                    //canvas.Scale(DPI_H, DPI_V);

                    if (IsFirstRender) Design.Init();

                    #region Draw
                    canvas.Clear(GoTheme.Current.Back);

                    var topMargin = 0;
                    var borderWidth = 0;
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        #region Translate
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT && WindowBorder != WindowBorder.Hidden)
                        {
                            GetWindowRect(Handle, out var windowRect);
                            GetClientRect(Handle, out var clientRect);

                            var borderX = (windowRect.right - windowRect.left - clientRect.right) / 2;
                            var titleH = windowRect.bottom - windowRect.top - clientRect.bottom - borderX;
                            topMargin = titleH + borderX;
                            borderWidth = borderX * 2;
                            canvas.Translate(0, titleH + borderX);
                        }
                        #endregion

                        #region Draw
                        OpenTKInputManager.Current.ScreenSize = new SKSize(Width, Height);

                        using (new SKAutoCanvasRestore(canvas))
                        {
                            canvas.Translate(0, OpenTKInputManager.Current.TranslateY);
                            Design.Draw(canvas);
                        }

                        OpenTKInputManager.Current.Draw(canvas);
                        #endregion

                        #region Debug
                        if (Debug)
                        {
                            using (var p = new SKPaint { IsAntialias = true })
                            {
                                var rt = Util.FromRect(0, Height - 20, Width, 20);
                                p.Color = SKColors.Black;
                                p.IsStroke = false;
                                canvas.DrawRect(rt, p);
                                Util.DrawText(canvas, $"UpdateTime : {UpdateTime * 1000:0 ms} ", "나눔고딕", GoFontStyle.Normal, 12, rt, SKColors.White, GoContentAlignment.MiddleRight);
                            }
                        }
                        #endregion
                    }
                    #endregion

                    ctx.Flush();
                    SwapBuffers();

                    if (IsFirstRender)
                    {
                        var ico = CreateTitleIcon();
                        if (ico != null) Icon = ico;
                        IsFirstRender = false;
                        IsVisible = true;
                    }
                }
            }
        }
        #endregion
        #region OnUpdateFrame
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (ctx != null && target != null)
            {
                Design.Update();
            }
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            //float x = MousePosition.X / DPI_H;
            //float y = MousePosition.Y / DPI_V;
            float x = MousePosition.X;
            float y = MousePosition.Y;
            GoMouseButton mb = ToGoMouseButton(e.Button);

            if (OpenTKInputManager.Current.IsInput) OpenTKInputManager.Current.MouseDown(x, y, mb);
            else Design.MouseDown(x, y, mb);

            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            float x = MousePosition.X;
            float y = MousePosition.Y;
            GoMouseButton mb = ToGoMouseButton(e.Button);

            if (OpenTKInputManager.Current.IsInput) OpenTKInputManager.Current.MouseUp(x, y, mb);
            else Design.MouseUp(x, y, mb);

            if ((DateTime.Now - dcTime).TotalMilliseconds < 300) Design.MouseDoubleClick(x, y, mb);
            dcTime = DateTime.Now;

            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            float x = MousePosition.X;
            float y = MousePosition.Y;

            if (OpenTKInputManager.Current.IsInput) OpenTKInputManager.Current.MouseMove(x, y);
            else Design.MouseMove(x, y);

            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            float x = MousePosition.X;
            float y = MousePosition.Y;

            Design.MouseWheel(x, y, e.OffsetY);

            base.OnMouseWheel(e);
        }
        #endregion
        #region OnResize
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            target?.Dispose();
            surface?.Dispose();

            target = CreateRenderTarget();
            surface = SKSurface.Create(ctx, target, SKColorType.Rgba8888);

            OnRenderFrame(new FrameEventArgs());
            base.OnResize(e);
        }
        #endregion
        #region OnKeyDown
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            Design.KeyDown(e.Shift, e.Control, e.Alt, (GoKeys)e.Key);
            base.OnKeyDown(e);
        }
        #endregion
        #region OnKeyUp
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Design.KeyUp(e.Shift, e.Control, e.Alt, (GoKeys)e.Key);
            base.OnKeyDown(e);
        }
        #endregion
        #endregion

        #region Method
        #region CreateRenderTarget
        GRBackendRenderTarget CreateRenderTarget()
        {
            int stencil = 8;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                GLES.GetInteger(GetPNameES.FramebufferBinding, out int framebuffer);
                GLES.GetInteger(GetPNameES.StencilBits, out stencil);
                GLES.GetInteger(GetPNameES.Samples, out int samples);
                stencil = 8;
                GLES.GetRenderbufferParameter(RenderbufferTargetES.Renderbuffer, RenderbufferParameterNameES.RenderbufferWidth, out int bufferWidth);
                GLES.GetRenderbufferParameter(RenderbufferTargetES.Renderbuffer, RenderbufferParameterNameES.RenderbufferHeight, out int bufferHeight);
            }
            else
            {
                GL.GetInteger(GetPName.FramebufferBinding, out int framebuffer);
                GL.GetInteger(GetPName.StencilBits, out stencil);
                GL.GetInteger(GetPName.Samples, out int samples);
                stencil = 8;
                GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out int bufferWidth);
                GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out int bufferHeight);

            }
            
            return new GRBackendRenderTarget(Size.X, Size.Y, 0, stencil, new GRGlFramebufferInfo(0, SKColorType.Rgba8888.ToGlSizedFormat()));
        }
        #endregion
        #region ToGoMouseButton
        GoMouseButton ToGoMouseButton(MouseButton btn)
        {
            GoMouseButton ret = GoMouseButton.Left;
            if (btn == MouseButton.Left) ret = GoMouseButton.Left;
            else if (btn == MouseButton.Right) ret = GoMouseButton.Right;
            else if (btn == MouseButton.Middle) ret = GoMouseButton.Middle;
            return ret;
        }
        #endregion

        #region CreateTitleIcon
        WindowIcon? CreateTitleIcon()
        {
            WindowIcon? ret = null;
            if (GoIconManager.Contains(TItleIconString ?? ""))
            {
                var iconSize = 16;
                var color = GoTheme.Current.Fore;
                using var surface = SKSurface.Create(new SKImageInfo(iconSize, iconSize, SKColorType.Rgba8888));
                var canvas = surface.Canvas;

                canvas.Clear(SKColors.Transparent);
                Util.DrawIcon(canvas, TItleIconString, iconSize, Util.FromRect(0, 0, iconSize, iconSize), color);

                using var img = surface.Snapshot();
                using var bmp = SKBitmap.FromImage(img);
                byte[] pixels = new byte[bmp.Width * bmp.Height * 4];
                Marshal.Copy(bmp.GetPixels(), pixels, 0, bmp.Width * bmp.Height * 4);
                return new WindowIcon(new Image(iconSize, iconSize, pixels));
            }
            return ret;
        }
        #endregion
        #endregion

        #region Interop
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_BACKGROUND_COLOR = 26;

        [DllImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
        private static extern int DwmSetWindowAttribute(nint hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(nint hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left, top, right, bottom;
        }

        void DarkMode(nint hwnd, bool dark)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                int darkMode = dark ? 1 : 0;
                int result = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            }
        }
        #endregion
    }
}
