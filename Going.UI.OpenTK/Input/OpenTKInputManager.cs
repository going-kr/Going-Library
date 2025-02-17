﻿using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Input;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
        public IGoControl? InputControl { get; private set; }
        public SKRect InputBounds { get; private set; }
        public InputType InputType { get; private set; }
        private Action<string>? InputCallback;

        public SKSize ScreenSize { get => keyboard.ScreenSize; set => keyboard.ScreenSize = value; }
        #endregion

        #region Member Variable
        TKKeyboard keyboard = new TKKeyboard();
        #endregion

        #region Constructor
        public OpenTKInputManager()
        {
            keyboard.Completed += (s) =>
            {
                if (InputControl != null && InputCallback != null)
                {
                    InputCallback(keyboard.Text ?? "");
                    InputControl = null;
                    InputCallback = null;
                }
            };
        }
        #endregion

        #region Input
        public void InputString(IGoControl control, SKRect bounds, Action<string> callback, string? value = null)
        {
            if (InputControl == null)
            {
                InputControl = control;
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
            if (InputControl != null)
            {
                switch (InputType)
                {
                    case InputType.String: keyboard.Draw(canvas); break;
                }
            }
        }
        
        public void MouseDown(float x, float y, GoMouseButton button)
        {
            if (InputControl != null)
            {
                switch (InputType)
                {
                    case InputType.String:
                        if (!keyboard.MouseDown(x, y, button))
                        {
                            if (InputControl != null && InputCallback != null)
                            {
                                InputControl = null;
                                InputCallback = null;
                            }
                        }
                        break;
                }
            }
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            if (InputControl != null)
            {
                switch (InputType)
                {
                    case InputType.String: keyboard.MouseUp(x, y, button); break;
                }
            }
        }

        public void MouseMove(float x, float y)
        {
            if (InputControl != null)
            {
                switch (InputType)
                {
                    case InputType.String: keyboard.MouseMove(x, y); break;
                }
            }
        }
        #endregion

        #region Input
        public (float tranY, SKRect? rtInputBounds) InputerBounds()
        {
            float tranY = 0;
            SKRect? rtInputBounds = null;
            if (InputControl != null)
            {
                switch (InputType)
                {
                    case InputType.String:
                        {
                            var rt = Util.FromRect(InputControl.ScreenX + InputBounds.Left, InputControl.ScreenY + InputBounds.Top, InputBounds.Width, InputBounds.Height);
                            rtInputBounds = rt;
                            if (CollisionTool.Check(keyboard.Bounds, rt))
                            {
                                tranY = ((ScreenSize.Height - keyboard.Bounds.Height)/ 2) - rt.MidY;
                                //tranY = -keyboard.Bounds.Height;
                            }
                        }
                        break;
                }
            }
            return (tranY, rtInputBounds);
        }
        #endregion

    }
}
