using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.ImageCanvas;
using Going.UI.Json;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Going.UI.Design
{
    public class GoDesign
    {
        #region Properties
        public static GoDesign? ActiveDesign { get; set; }

        [JsonInclude]
        public Dictionary<string, GoPage> Pages { get; private set; } = [];

        [JsonInclude]
        private Dictionary<string, List<SKBitmap>> Images { get; } = [];

        [JsonInclude]
        private IcImageFolder? IcFolder { get; set; }

        [JsonIgnore] public int Width { get; private set; }
        [JsonIgnore] public int Height { get; private set; }
        [JsonIgnore] public IGoControl? SelectedControl { get; private set; }
        [JsonIgnore] public GoPage? CurrentPage { get; private set; }

        [JsonIgnore] public SKPoint MousePosition { get; private set; }

        [JsonIgnore] public bool IsDrag => dragItem != null;
        #endregion

        #region Member Variable
        private Stack<GoWindow> stkWindow = new Stack<GoWindow>();
        private GoDropDownWindow? dropdownWindow;
        private object? dragItem = null;
        #endregion

        #region Constructor
        public GoDesign() => ActiveDesign = this;

        [JsonConstructor]
        public GoDesign(Dictionary<string, GoPage> pages, Dictionary<string, List<SKBitmap>> images) : this()
        {
            Pages = pages;
            Images = images;
        }
        #endregion

        #region Method
        #region Select
        public void Select(IGoControl? control)
        {
            if (SelectedControl == null) SelectedControl = control;
        }
        #endregion

        #region Page
        public void AddPage(GoPage page) { if (page.Name != null) Pages.TryAdd(page.Name, page); }

        public void SetPage(string pageName)
        {
            if (Pages.TryGetValue(pageName, out var page))
            {
                CurrentPage = page; 
                CurrentPage.FireMouseMove(-1, -1);
            }
        }
        public void SetPage(GoPage page) { if (page.Name != null) SetPage(page.Name); }
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

        /// <summary>
        /// 윈도우에서 발생하는 일반적인 기능들을 이쪽으로 bypass
        /// CurrentPage를 봐서 현 페이지에 전달하는 기능을 함
        /// </summary>
        #region Fire
        #region Init
        public void Init()
        {
            foreach (var page in Pages.Values)
            {
                page.Design = this;
                GUI.Init(this, page);
            }
        }
        #endregion

        #region Draw / Update
        public void Draw(SKCanvas canvas)
        {
            // Drawing : Page > Window > DropDownWindow 순으로 처리
            // 화면은 다 띄워야해서 else 처리 안함
            // MouseEvent : DropDownWindow > Window > Page 순으로 처리
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

            #region DragDrop
            if (dragItem != null)
            {
                var rt = Util.FromRect(MousePosition.X - 12, MousePosition.Y - 12, 24, 24);
                rt.Offset(5, 10);
                Util.DrawIcon(canvas, "fa-hand-pointer", 24, rt, GoTheme.Current.Fore);
            }
            #endregion
        }

        public void Update()
        {
            #region Page
            if (CurrentPage != null)
            {
                CurrentPage.FireUpdate();
            }
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
        #endregion

        #region Mouse
        public void MouseDown(float x, float y, GoMouseButton button)
        {
            // MouseEvent : DropDownWindow > Window > Page 순으로 처리
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

            if (dragItem != null && CurrentPage != null)
            {
                var ls = ControlStack(CurrentPage, x - CurrentPage.Left - CurrentPage.PanelBounds.Left, y - CurrentPage.Top - CurrentPage.PanelBounds.Top);
                if (ls.LastOrDefault() is GoControl c) c.InvokeDragDrop(x, y, dragItem);
                dragItem = null;
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

        #region Image
        public void AddImage(string name, string path)
        {
            if (!Images.ContainsKey(name) && File.Exists(path))
            {
                var data = File.ReadAllBytes(path);
                var ls = ImageExtractor.ProcessImageFromMemory(data);
                if (ls != null && ls.Count > 0) Images.Add(name, ls);
            }
        }

        public void AddImageFolder(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (var path in Directory.GetFiles(directory))
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (Images.ContainsKey(name))
                    {
                        var ls = Images.Keys.Where(x => x.StartsWith(name));
                        var ls2 = ls.Where(x => x.Split('_').Length == 2);
                        var max = ls2.Count() > 0 ? ls2.Max(x => int.TryParse(x.Split('_')[1], out var n) ? n : 0) : 0;
                        AddImage($"{name}_{max + 1}", path);
                    }
                    else AddImage(name, path);
                }
            }
        }

        public List<SKBitmap> GetImage(string name) => Images.TryGetValue(name, out var ls) ? ls : [];
        #endregion

        #region ImageCnavas
        public void LoadIC(string path)
        {
            if (path != null && Directory.Exists(path))
            {
                IcFolder = new IcImageFolder(path);
            }
        }

        public IcImageFolder? GetIC() => IcFolder;
        #endregion

        #region Drag
        public void Drag(object? item) => dragItem = item;
        public object? GetDragItem() => dragItem;
        #endregion

        #region ControlStack
        public List<IGoControl> ControlStack(IGoContainer container, float x, float y)
        {
            List<IGoControl> ret = [];

            if(container != null)
            {
                if (container is IGoControl vc) ret.Add(vc);
                foreach (var c in container.Childrens)
                    if (CollisionTool.Check(c.Bounds, x, y))
                    {
                        if (c is IGoContainer con)
                            ret.AddRange(ControlStack(con, x - c.Left - con.PanelBounds.Left, y - c.Top - con.PanelBounds.Top));
                        else
                            ret.Add(c);
                    }
            }

            return ret;
        }
        #endregion
        #endregion
    }

}
