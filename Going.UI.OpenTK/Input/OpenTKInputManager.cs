using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Managers;
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

        public SKSize ScreenSize { get; set; }

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
                                var kh = keyboard.GetHeight(ScreenSize);
                                var rtk = Util.FromRect(0, ScreenSize.Height - kh, ScreenSize.Width, kh);

                                if (CollisionTool.Check(rtk, rt))
                                {
                                    var ty = -kh;

                                    if (ani.Variable == "set")
                                        tranY = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, 0, ty) : ty;
                                    else
                                        tranY = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, ty, 0) : 0;
                                }
                            }
                            break;

                        case InputType.Number:
                            {
                                var rt = Util.FromRect(InputControl.ScreenX + InputBounds.Left, InputControl.ScreenY + InputBounds.Top, InputBounds.Width, InputBounds.Height);
                                var kh = keypad.GetHeight(ScreenSize);
                                var rtk = Util.FromRect(0, ScreenSize.Height - kh, ScreenSize.Width, kh);

                                if (CollisionTool.Check(rtk, rt))
                                {
                                    var ty = -kh;

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
        private TKKeyboard keyboard = new();
        private TKKeypad keypad = new();

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
                    ani.Start(Animation.Time200, "clear", GoInputEventer.Current.ClearInputControl);
                }
            };

            keypad.Completed += (s) =>
            {
                if (InputControl != null && InputCallback != null)
                {
                    InputCallback(keypad.Text ?? "");
                    ani.Start(Animation.Time200, "clear", GoInputEventer.Current.ClearInputControl);
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
                ani.Start(Animation.Time200, "set");

                InputBounds = bounds;
                InputCallback = callback;
                InputType = InputType.String;
                keyboard.Set(value);
            }
        }

        public void InputNumber(IGoControl control, SKRect bounds, Action<string> callback, Type valueType, object value, object? min, object? max)
        {
            if (InputControl == null)
            {
                GoInputEventer.Current.SetInputControl(control);
                ani.Start(Animation.Time200, "set");

                InputBounds = bounds;
                InputCallback = callback;
                InputType = InputType.Number;
                keypad.Set(valueType, value, min, max);
            }
        }
        #endregion

        #region Draw / Mouse
        public void Draw(SKCanvas canvas)
        {
            if (InputControl != null)
            {
                switch (InputType)
                {
                    case InputType.String:
                        {
                            var kh = keyboard.GetHeight(ScreenSize);
                            var rtS = Util.FromRect(0, ScreenSize.Height - kh, ScreenSize.Width, kh);
                            var rtH = Util.FromRect(0, ScreenSize.Height, ScreenSize.Width, kh);

                            if (ani.Variable == "set")
                                keyboard.Bounds = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, rtH, rtS) : rtS;
                            else
                                keyboard.Bounds = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, rtS, rtH) : rtH;
                         
                            keyboard.Draw(canvas);
                        }
                        break;

                    case InputType.Number:
                        {
                            var kh = keypad.GetHeight(ScreenSize);
                            var rtS = Util.FromRect(0, ScreenSize.Height - kh, ScreenSize.Width, kh);
                            var rtH = Util.FromRect(0, ScreenSize.Height, ScreenSize.Width, kh);

                            if (ani.Variable == "set")
                                keypad.Bounds = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, rtH, rtS) : rtS;
                            else
                                keypad.Bounds = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, rtS, rtH) : rtH;

                            keypad.Draw(canvas);
                        }
                        break;
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
                            if (InputControl != null)
                                ani.Start(Animation.Time200, "clear", GoInputEventer.Current.ClearInputControl);
                        break;

                    case InputType.Number:
                        if (!keypad.MouseDown(x, y, button))
                            if (InputControl != null)
                                ani.Start(Animation.Time200, "clear", GoInputEventer.Current.ClearInputControl);
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
                    case InputType.Number: keypad.MouseUp(x, y, button); break;
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
                    case InputType.Number: keypad.MouseMove(x, y); break;
                }
            }
        }
        #endregion
    }
}
