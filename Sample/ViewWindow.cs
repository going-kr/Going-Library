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
using Going.UI.Collections;

namespace Sample
{
    public class ViewWindow : GameWindow, IGoContainer
    {
        #region Properties
        public int Width => Size.X;
        public int Height => Size.Y;

        public bool Debug { get; set; } = false;

        public GoControlCollection Childrens { get; } = [];
        #endregion

        #region Member Variable
        private GRGlInterface? gli;
        private GRContext? ctx;
        private GRBackendRenderTarget? target;
        private SKSurface? surface;
        private DateTime dcTime = DateTime.Now;
        private bool IsFirstRender = true;
        private IntPtr Handle;
        #endregion

        #region Constructor
        public ViewWindow(int width, int height)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                WindowBorder = WindowBorder.Resizable,
                ClientSize = new Vector2i(width, height),
                StartVisible = false,
                API = (Environment.OSVersion.Platform == PlatformID.Unix ? ContextAPI.OpenGLES : ContextAPI.OpenGL),
                APIVersion = (Environment.OSVersion.Platform == PlatformID.Unix ? new Version(3, 1) : new Version(3, 2))
            })
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix) this.WindowState = WindowState.Fullscreen;

            Childrens.Initialize(this);
            Childrens.Add(new GoLabel { Left = 10, Top = 10, Width = 120, Height = 150, Text = "동해물과 백두산이 마르고 닳도록\n하느님이 보우하사 우리 나라 만세\n무궁화 삼천리 화려강산\n대한 사람 대한으로 길이 보전하세", BorderOnly = true, ContentAlignment = GoContentAlignment.TopLeft, TextPadding = new GoPadding(10) });
            Childrens.Add(new GoButton { Left = 200, Top = 10, Width = 90, Height = 40, IconString = "fa-check", Text = "확인" });
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
            base.OnRenderFrame(e);

            if (ctx != null && target != null && surface != null)
            {
                var canvas = surface.Canvas;

                #region Draw
                canvas.Clear(new SKColor(32, 32, 32));

                var topMargin = 0;
                var borderWidth = 0;
                using (new SKAutoCanvasRestore(canvas))
                {
                    #region Translate
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT && WindowBorder != WindowBorder.Hidden)
                    {
                        GetWindowRect(Handle, out var windowRect);
                        GetClientRect(Handle, out var clientRect);

                        var borderX = ((windowRect.right - windowRect.left) - clientRect.right) / 2;
                        var titleH = (windowRect.bottom - windowRect.top) - clientRect.bottom - borderX;
                        topMargin = titleH + borderX;
                        borderWidth = borderX * 2;
                        canvas.Translate(0, titleH + borderX);
                    }
                    #endregion
                    #region Draw Content
                    UI.Draw(canvas, Childrens);
                    #endregion
                    #region Debug
                    if (Debug)
                    {
                        using (var p = new SKPaint { IsAntialias = true })
                        {
                            var rt = Util.FromRect(0, Height - topMargin - 20, Width - borderWidth, 20);
                            p.Color = SKColors.Black;
                            p.IsStroke = false;
                            canvas.DrawRect(rt, p);
                            Util.DrawText(canvas, $"UpdateTime : {UpdateTime * 1000:0 ms} ", "나눔고딕", 12, rt, SKColors.White, GoContentAlignment.MiddleRight);
                        }
                    }
                    #endregion
                }
                #endregion

                ctx.Flush();
                SwapBuffers();

                if (IsFirstRender)
                {
                    IsFirstRender = false;
                    IsVisible = true;
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
                UI.Update(Childrens);
            }
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            float x = MousePosition.X;
            float y = MousePosition.Y;
            GoMouseButton mb = ToGoMouseButton(e.Button);

            UI.MouseDown(Childrens, x, y, mb);

            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            float x = MousePosition.X;
            float y = MousePosition.Y;
            GoMouseButton mb = ToGoMouseButton(e.Button);

            UI.MouseUp(Childrens, x, y, mb);
            if ((DateTime.Now - dcTime).TotalMilliseconds < 300) UI.MouseDoubleClick(Childrens, x, y, mb);

            dcTime = DateTime.Now;
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            float x = MousePosition.X;
            float y = MousePosition.Y;

            UI.MouseMove(Childrens, x, y);

            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            float x = MousePosition.X;
            float y = MousePosition.Y;

            UI.MouseWheel(Childrens, x, y, e.OffsetY);

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
            return new GRBackendRenderTarget(Width, Height, 0, stencil, new GRGlFramebufferInfo(0, SKColorType.Rgba8888.ToGlSizedFormat()));
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
        #endregion

        #region Interop
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_BACKGROUND_COLOR = 26;

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

        void DarkMode(IntPtr hwnd, bool dark)
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
