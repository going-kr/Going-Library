using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Design
{
    public class GoDesign
    {
        #region Properties
        public Dictionary<string, GoPage> Pages { get; private set; } = [];

        [JsonIgnore] public int Width { get; private set; }
        [JsonIgnore] public int Height { get; private set; }
        [JsonIgnore] public IGoControl? SelectedControl { get; private set; }
        [JsonIgnore] public GoPage? CurrentPage { get; private set; }

        [JsonIgnore] public SKPoint MousePosition { get; private set; }
        #endregion

        #region Member Variable
        private Stack<GoWindow> stkWindow = new Stack<GoWindow>();
        private GoDropDownWindow? dropdownWindow;
        #endregion

        #region Method
        #region Select
        public void Select(IGoControl? control) => SelectedControl = control;
        #endregion

        #region Page
        public void AddPage(GoPage page) => Pages.TryAdd(page.Name, page);

        public void SetPage(string pageName) { if (Pages.TryGetValue(pageName, out var page)) { CurrentPage = page; CurrentPage.FireMouseMove(MousePosition.X, MousePosition.Y); } }
        public void SetPage(GoPage page) => SetPage(page.Name);
        #endregion

        #region DropDownWindow
        public void ShowDropDownWindow(GoDropDownWindow wnd)
        {
            if (dropdownWindow == null)
            {
                dropdownWindow = wnd;
                dropdownWindow.Visible = true;
            }
        }
        public void HideDropDownWindow(GoDropDownWindow wnd)
        {
            if (dropdownWindow == wnd)
            {
                dropdownWindow.Visible = false;
                dropdownWindow = null;
            }
        }
        #endregion

        #region Window
        public void ShowWindow(GoWindow wnd)
        {
            wnd.Visible = true;
            stkWindow.Push(wnd);
        }
        public void HideWindow(GoWindow wnd)
        {
            if (stkWindow.Count > 0 && wnd == stkWindow.Peek())
            {
                var w = stkWindow.Pop();
                w.Visible = false;
            }
        }
        #endregion

        #region SetSize
        public void SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
        #endregion

        #region Fire
        public void Init()
        {
            foreach (var page in Pages.Values)
            {
                page.Design = this;
                GUI.Init(this, page);
            }
        }

        public void Draw(SKCanvas canvas)
        {
            #region Page
            if (CurrentPage != null)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    CurrentPage.Bounds = Util.FromRect(0, 0, Width, Height);
                    canvas.Translate(CurrentPage.Bounds.Left, CurrentPage.Bounds.Top);
                    CurrentPage.FireDraw(canvas);
                }
            }
            #endregion

            #region Window
            if (stkWindow.Count > 0)
            {
                var ls = stkWindow.Reverse().ToList();

                foreach (var w in ls)
                {
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.Translate(w.Bounds.Left, w.Bounds.Top);
                        w.FireDraw(canvas);
                    }
                }
            }
            #endregion

            #region DropDownWindow
            if (dropdownWindow != null)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Translate(dropdownWindow.Bounds.Left, dropdownWindow.Bounds.Top);
                    dropdownWindow.FireDraw(canvas);
                }
            }
            #endregion
        }

        public void Update()
        {
            #region Page
            if (CurrentPage != null)
                GUI.Update(CurrentPage);
            #endregion

            #region Window
            if (stkWindow.Count > 0)
            {
                var ls = stkWindow.Reverse().ToList();
                foreach (var w in ls) w.FireUpdate();
            }
            #endregion

            #region DropDownWindow
            if (dropdownWindow != null)
                dropdownWindow.FireUpdate();
            #endregion
        }

        #region Mouse
        public void MouseDown(float x, float y, GoMouseButton button)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    GUI.MouseDown(dropdownWindow, x, y, button);
                else
                    HideDropDownWindow(dropdownWindow);
            }
            else if (stkWindow.Count > 0)
            {
                if (CollisionTool.Check(stkWindow.Peek().Bounds, x, y))
                    GUI.MouseDown(stkWindow.Peek(), x, y, button);
            }
            else
            {
                if (CurrentPage != null)
                    GUI.MouseDown(CurrentPage, x, y, button);
            }
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    GUI.MouseUp(dropdownWindow, x, y, button);
            }
            else if (stkWindow.Count > 0)
            {
                if (CollisionTool.Check(stkWindow.Peek().Bounds, x, y))
                    GUI.MouseUp(stkWindow.Peek(), x, y, button);
            }
            else
            {
                if (CurrentPage != null)
                    GUI.MouseUp(CurrentPage, x, y, button);
            }

            SelectedControl = null;
        }

        public void MouseDoubleClick(float x, float y, GoMouseButton button)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    GUI.MouseDoubleClick(dropdownWindow, x, y, button);
            }
            else if (stkWindow.Count > 0)
            {
                if (CollisionTool.Check(stkWindow.Peek().Bounds, x, y))
                    GUI.MouseDoubleClick(stkWindow.Peek(), x, y, button);
            }
            else
            {
                if (CurrentPage != null)
                    GUI.MouseDoubleClick(CurrentPage, x, y, button);
            }
        }

        public void MouseMove(float x, float y)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    GUI.MouseMove(dropdownWindow, x, y);
            }
            else if (stkWindow.Count > 0)
            {
                if (CollisionTool.Check(stkWindow.Peek().Bounds, x, y))
                    GUI.MouseMove(stkWindow.Peek(), x, y);
            }
            else
            {
                if (CurrentPage != null)
                    GUI.MouseMove(CurrentPage, x, y);
            }

            MousePosition = new SKPoint(x, y);
        }

        public void MouseWheel(float x, float y, float delta)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    GUI.MouseWheel(dropdownWindow, x, y, delta);
            }
            else if (stkWindow.Count > 0)
            {
                if (CollisionTool.Check(stkWindow.Peek().Bounds, x, y))
                    GUI.MouseWheel(stkWindow.Peek(), x, y, delta);
            }
            else
            {
                if (CurrentPage != null)
                    GUI.MouseWheel(CurrentPage, x, y, delta);
            }
        }
        #endregion

        #region Key
        public void KeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            if (dropdownWindow != null)
            {
                GUI.KeyDown(dropdownWindow, Shift, Control, Alt, key);
            }
            else if (stkWindow.Count > 0)
            {
                GUI.KeyDown(stkWindow.Peek(), Shift, Control, Alt, key);
            }
            else
            {
                if (CurrentPage != null)
                    GUI.KeyDown(CurrentPage, Shift, Control, Alt, key);
            }
      
        }

        public void KeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            if (dropdownWindow != null)
            {
                GUI.KeyUp(dropdownWindow, Shift, Control, Alt, key);
            }
            else if (stkWindow.Count > 0)
            {
                GUI.KeyUp(stkWindow.Peek(), Shift, Control, Alt, key);
            }
            else
            {
                if (CurrentPage != null)
                    GUI.KeyUp(CurrentPage, Shift, Control, Alt, key);
            }
        }
        #endregion
        #endregion
        #endregion
    }

}
