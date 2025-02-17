using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Input;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.OpenTK.Input
{
    public class OpenTKInputManager
    {
        #region Properties
        private static readonly Lazy<OpenTKInputManager> _instance = new Lazy<OpenTKInputManager>(() => new OpenTKInputManager());
        public static OpenTKInputManager Current => _instance.Value;

        public bool IsInput => InputControl != null;
        public IGoControl? InputControl => GoInputEventer.Current.InputControl;
        public InputType InputType { get; private set; }

        public SKSize ScreenSize { get => keyboard.ScreenSize; set => keyboard.ScreenSize = value; }

        #region TranslateY
        public float TranslateY
        {
            get
            {
                float tranY = 0;
                if (InputControl != null)
                {
                    switch (InputType)
                    {
                        case InputType.String:
                            {
                                var rt = Util.FromRect(InputControl.ScreenX + InputBounds.Left, InputControl.ScreenY + InputBounds.Top, InputBounds.Width, InputBounds.Height);
                                if (CollisionTool.Check(keyboard.Bounds, rt))
                                {
                                    var ty = ((ScreenSize.Height - keyboard.Bounds.Height) / 2) - rt.MidY;

                                    if (ani.Variable == "set")
                                        tranY = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, 0, ty) : ty;
                                    else
                                        tranY = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, ty, 0) : 0;
                                }
                            }
                            break;
                    }
                }
                return Convert.ToInt32(tranY);
            }
        }
        #endregion
        #endregion

        #region Member Variable
        private TKKeyboard keyboard = new TKKeyboard();

        private SKRect InputBounds;
        private Action<string>? InputCallback;
        private Animation ani = new();
        #endregion

        #region Constructor
        public OpenTKInputManager()
        {
            keyboard.Completed += (s) =>
            {
                if (InputControl != null && InputCallback != null)
                {
                    InputCallback(keyboard.Text ?? "");
                    ani.Start(200, "clear", GoInputEventer.Current.ClearInputControl);
                }
            };
        }
        #endregion

        #region Input
        public void InputString(IGoControl control, SKRect bounds, Action<string> callback, string? value = null)
        {
            if (InputControl == null)
            {
                GoInputEventer.Current.SetInputControl(control);
                ani.Start(200, "set");

                InputBounds = bounds;
                InputCallback = callback;
                InputType = InputType.String;
                keyboard.Set(value);
            }
        }
        #endregion

        #region Draw / Mouse
        public void Draw(SKCanvas canvas)
        {
            if (InputControl != null && !(ani.IsPlaying && ani.Variable == "clear"))
            {
                switch (InputType)
                {
                    case InputType.String: keyboard.Draw(canvas); break;
                }
            }
        }

        public void MouseDown(float x, float y, GoMouseButton button)
        {
            if (InputControl != null && !(ani.IsPlaying && ani.Variable == "clear"))
            {
                switch (InputType)
                {
                    case InputType.String:
                        if (!keyboard.MouseDown(x, y, button))
                        {
                            if (InputControl != null)
                            {
                                ani.Start(200, "clear", GoInputEventer.Current.ClearInputControl);
                            }
                        }
                        break;
                }
            }
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            if (InputControl != null && !(ani.IsPlaying && ani.Variable == "clear"))
            {
                switch (InputType)
                {
                    case InputType.String: keyboard.MouseUp(x, y, button); break;
                }
            }
        }

        public void MouseMove(float x, float y)
        {
            if (InputControl != null && !(ani.IsPlaying && ani.Variable == "clear"))
            {
                switch (InputType)
                {
                    case InputType.String: keyboard.MouseMove(x, y); break;
                }
            }
        }
        #endregion

      

    }
}
