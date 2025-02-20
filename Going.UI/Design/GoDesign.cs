using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Json;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Design
{
    public class GoDesign
    {
        #region Properties
        public static GoDesign? ActiveDesign { get; set; }

        [JsonInclude]
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

        #region Constructor
        [JsonConstructor]
        public GoDesign(Dictionary<string, GoPage> pages) : this() => Pages = pages;
        public GoDesign() => ActiveDesign = this;
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
                wnd.Design = this;
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
            wnd.Design = this;
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

        #region Json
        public string JsonSerialize() => JsonSerializer.Serialize(this, GoJsonConverter.Options);
        public static GoDesign? JsonDeserialize(string? json)
        {
            if (json != null)
            {
                try { return JsonSerializer.Deserialize<GoDesign>(json, GoJsonConverter.Options); }
                catch (Exception ex) { return null; }
            }
            else return null;
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
            ActiveDesign = this;
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseDown(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
                else
                    dropdownWindow.Hide();
            }
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseDown(x - w.Left, y - w.Top, button);
            }
            else
            {
                if (CurrentPage != null)
                    CurrentPage.FireMouseDown(x, y, button);
            }
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y) || dropdownWindow._MouseDown_)
                    dropdownWindow.FireMouseUp(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
            }
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y) || w._MouseDown_)
                    w.FireMouseUp(x - w.Left, y - w.Top, button);
            }
            else
            {
                if (CurrentPage != null)
                    CurrentPage.FireMouseUp(x, y, button);
            }

            SelectedControl = null;
        }

        public void MouseDoubleClick(float x, float y, GoMouseButton button)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseDoubleClick(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
            }
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseDoubleClick(x - w.Left, y - w.Top, button);
            }
            else
            {
                if (CurrentPage != null)
                    CurrentPage.FireMouseDoubleClick(x, y, button);
            }
        }

        public void MouseMove(float x, float y)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y) || dropdownWindow._MouseDown_)
                    dropdownWindow.FireMouseMove(x - dropdownWindow.Left, y - dropdownWindow.Top);
            }
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y) || w._MouseDown_)
                    w.FireMouseMove(x - w.Left, y - w.Top);
            }
            else
            {
                if (CurrentPage != null)
                    CurrentPage.FireMouseMove(x, y);
            }

            MousePosition = new SKPoint(x, y);
        }

        public void MouseWheel(float x, float y, float delta)
        {
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseWheel(x - dropdownWindow.Left, y - dropdownWindow.Top, delta);
            }
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseWheel(x - w.Left, y - w.Top, delta);
            }
            else
            {
                if (CurrentPage != null)
                    CurrentPage.FireMouseWheel(x, y, delta);
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
