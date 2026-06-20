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
    public class TKInputManager
    {
        #region Properties
        private static readonly Lazy<TKInputManager> _instance = new Lazy<TKInputManager>(() => new TKInputManager());
        public static TKInputManager Current => _instance.Value;

        public bool IsInput => InputControl != null;
        public IGoControl? InputControl => GoInputEventer.Current.InputControl;
        public InputType InputType { get; private set; }

        public SKSize ScreenSize { get; set; }
        public bool IsPlaying => ani.IsPlaying;

        public bool KeyboardShiftHold { get => keyboard.UseShiftHolding; set => keyboard.UseShiftHolding = value; }
        #region TranslateY
        public float TranslateY
        {
            get
            {
                float tranY = 0;
                var ctrl = InputControl;   // 백그라운드 clear 애니메이션이 중간에 null로 바꾸는 race 방지: 한 번만 캡처
                if (ctrl != null)
                {
                    switch (InputType)
                    {
                        case InputType.String:
                            {
                                var rt = Util.FromRect(ctrl.ScreenX + InputBounds.Left, ctrl.ScreenY + InputBounds.Top, InputBounds.Width, InputBounds.Height);
                                var kh = keyboard.GetHeight(ScreenSize);
                                var rtk = Util.FromRect(0, ScreenSize.Height - kh, ScreenSize.Width, kh);

                                if (CollisionTool.Check(rtk, rt))
                                {
                                    // 키보드 전체 높이가 아니라 "가려지는 만큼"만 올린다(입력부 하단을 키보드 상단 바로 위로).
                                    var ty = Math.Min(0f, rtk.Top - rt.Bottom - 4);

                                    if (ani.Variable == "set")
                                        tranY = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, 0, ty) : ty;
                                    else
                                        tranY = ani.IsPlaying ? ani.Value(AnimationAccel.DCL, ty, 0) : 0;
                                }
                            }
                            break;

                        case InputType.Number:
                            {
                                var rt = Util.FromRect(ctrl.ScreenX + InputBounds.Left, ctrl.ScreenY + InputBounds.Top, InputBounds.Width, InputBounds.Height);
                                var kh = keypad.GetHeight(ScreenSize);
                                var rtk = Util.FromRect(0, ScreenSize.Height - kh, ScreenSize.Width, kh);

                                if (CollisionTool.Check(rtk, rt))
                                {
                                    // 키보드 전체 높이가 아니라 "가려지는 만큼"만 올린다(입력부 하단을 키보드 상단 바로 위로).
                                    var ty = Math.Min(0f, rtk.Top - rt.Bottom - 4);

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
        private Action? actInv;
        #endregion

        #region Constructor
        public TKInputManager()
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

            ani.Refresh = () => actInv?.Invoke();

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
        public void Draw(SKCanvas canvas, GoTheme thm)
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

                            keyboard.Draw(canvas, thm);
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

                            keypad.Draw(canvas, thm);
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

        public void SetInvalidate(Action invalidate)
        {
            actInv = invalidate;
        }
        #endregion
    }
}
