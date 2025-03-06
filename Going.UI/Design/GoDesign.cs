using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
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

        public bool UseTitleBar { get => pnlT.Visible; set => pnlT.Visible = value; }
        public bool UseLeftSideBar { get => pnlL.Visible; set => pnlL.Visible = value; }
        public bool UseRightSideBar { get => pnlR.Visible; set => pnlR.Visible = value; }
        public bool UseFooter { get => pnlB.Visible; set => pnlB.Visible = value; }
        public string BarColor { get; set; } = "Base2";

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
        [JsonIgnore] public GoTitleBar TitleBar => pnlT;
        [JsonIgnore] public GoLeftSideBar LeftSideBar => pnlL;
        [JsonIgnore] public GoRightSideBar RightSideBar => pnlR;
        [JsonIgnore] public GoFooter Footer => pnlB;
        #endregion

        #region Member Variable
        [JsonInclude] private GoTitleBar pnlT = new() { Visible = false };
        [JsonInclude] private GoFooter pnlB = new() { Visible = false };
        [JsonInclude] private GoLeftSideBar pnlL = new() { Visible = false };
        [JsonInclude] private GoRightSideBar pnlR = new() { Visible = false };

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
            #region Page
            #region bounds
            var (rtL, rtT, rtR, rtB, rtF) = bounds();
            pnlL.Bounds = rtL;
            pnlT.Bounds = rtT;
            pnlR.Bounds = rtR;
            pnlB.Bounds = rtB;
            if (CurrentPage != null) CurrentPage.Bounds = rtF;
            #endregion

            #region Background
            if (UseTitleBar || UseLeftSideBar || UseRightSideBar || UseFooter)
            {
                var thm = GoTheme.Current;
                canvas.Clear(thm.ToColor(BarColor));

                var vrt = Util.Int(Util.FromRect(rtF.Left, rtF.Top, rtF.Width - 1, rtF.Height - 1)); vrt.Offset(0.5F, 0.5F);
                var rtP = roundPage(vrt);
                Util.DrawBox(canvas, rtP, thm.Back, thm.Back);
            }
            #endregion

            #region TitleBar
            if (UseTitleBar)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(pnlT.Bounds);
                    canvas.Translate(pnlT.Bounds.Left, pnlT.Bounds.Top);
                    pnlT.FireDraw(canvas);
                }
            }
            #endregion
            #region LeftSideBar
            if (UseLeftSideBar)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(pnlL.Bounds);
                    canvas.Translate(pnlL.Bounds.Left, pnlL.Bounds.Top);
                    pnlL.FireDraw(canvas);
                }
            }
            #endregion
            #region RightSideBar
            if (UseRightSideBar)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(pnlR.Bounds);
                    canvas.Translate(pnlR.Bounds.Left, pnlR.Bounds.Top);
                    pnlR.FireDraw(canvas);
                }
            }
            #endregion
            #region Footer
            if (UseFooter)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(pnlB.Bounds);
                    canvas.Translate(pnlB.Bounds.Left, pnlB.Bounds.Top);
                    pnlB.FireDraw(canvas);
                }
            }
            #endregion
            #region CurrentPage
            if (CurrentPage != null)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(CurrentPage.Bounds);
                    canvas.Translate(CurrentPage.Bounds.Left, CurrentPage.Bounds.Top);
                    CurrentPage.FireDraw(canvas);
                }
            }
            #endregion
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
            if (UseTitleBar) pnlT.FireUpdate();
            if (UseLeftSideBar) pnlL.FireUpdate();
            if (UseRightSideBar) pnlR.FireUpdate();
            if (UseFooter) pnlB.FireUpdate();
            if (CurrentPage != null) CurrentPage.FireUpdate();
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
            ActiveDesign = this;

            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseDown(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
                else
                    dropdownWindow.Hide();
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseDown(x - w.Left, y - w.Top, button);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) pnlT.FireMouseDown(x - pnlT.Left, y - pnlT.Top, button);
                if (UseLeftSideBar) pnlL.FireMouseDown(x - pnlL.Left, y - pnlL.Top, button);
                if (UseRightSideBar) pnlR.FireMouseDown(x - pnlR.Left, y - pnlR.Top, button);
                if (UseFooter) pnlB.FireMouseDown(x - pnlB.Left, y - pnlB.Top, button);

                CurrentPage?.FireMouseDown(x - CurrentPage.Left, y - CurrentPage.Top, button);
            }
            #endregion
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y) || dropdownWindow._MouseDown_)
                    dropdownWindow.FireMouseUp(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y) || w._MouseDown_)
                    w.FireMouseUp(x - w.Left, y - w.Top, button);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) pnlT.FireMouseUp(x - pnlT.Left, y - pnlT.Top, button);
                if (UseLeftSideBar) pnlL.FireMouseUp(x - pnlL.Left, y - pnlL.Top, button);
                if (UseRightSideBar) pnlR.FireMouseUp(x - pnlR.Left, y - pnlR.Top, button);
                if (UseFooter) pnlB.FireMouseUp(x - pnlB.Left, y - pnlB.Top, button);

                CurrentPage?.FireMouseUp(x - CurrentPage.Left, y - CurrentPage.Top, button);
            }
            #endregion

            #region Drag
            if (dragItem != null && CurrentPage != null)
            {
                var ls = ControlStack(CurrentPage, x - CurrentPage.Left - CurrentPage.PanelBounds.Left, y - CurrentPage.Top - CurrentPage.PanelBounds.Top);
                if (ls.LastOrDefault() is GoControl c) c.InvokeDragDrop(x, y, dragItem);
                dragItem = null;
            }
            #endregion

            SelectedControl = null;
        }

        public void MouseDoubleClick(float x, float y, GoMouseButton button)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseDoubleClick(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseDoubleClick(x - w.Left, y - w.Top, button);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) pnlT.FireMouseUp(x - pnlT.Left, y - pnlT.Top, button);
                if (UseLeftSideBar) pnlL.FireMouseUp(x - pnlL.Left, y - pnlL.Top, button);
                if (UseRightSideBar) pnlR.FireMouseUp(x - pnlR.Left, y - pnlR.Top, button);
                if (UseFooter) pnlB.FireMouseUp(x - pnlB.Left, y - pnlB.Top, button);

                CurrentPage?.FireMouseDoubleClick(x - CurrentPage.Left, y - CurrentPage.Top, button);
            }
            #endregion
        }

        public void MouseMove(float x, float y)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y) || dropdownWindow._MouseDown_)
                    dropdownWindow.FireMouseMove(x - dropdownWindow.Left, y - dropdownWindow.Top);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y) || w._MouseDown_)
                    w.FireMouseMove(x - w.Left, y - w.Top);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) pnlT.FireMouseMove(x - pnlT.Left, y - pnlT.Top);
                if (UseLeftSideBar) pnlL.FireMouseMove(x - pnlL.Left, y - pnlL.Top);
                if (UseRightSideBar) pnlR.FireMouseMove(x - pnlR.Left, y - pnlR.Top);
                if (UseFooter) pnlB.FireMouseMove(x - pnlB.Left, y - pnlB.Top);

                CurrentPage?.FireMouseMove(x - CurrentPage.Left, y - CurrentPage.Top);
            }
            #endregion

            MousePosition = new SKPoint(x, y);
        }

        public void MouseWheel(float x, float y, float delta)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseWheel(x - dropdownWindow.Left, y - dropdownWindow.Top, delta);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseWheel(x - w.Left, y - w.Top, delta);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) pnlT.FireMouseWheel(x - pnlT.Left, y - pnlT.Top, delta);
                if (UseLeftSideBar) pnlL.FireMouseWheel(x - pnlL.Left, y - pnlL.Top, delta);
                if (UseRightSideBar) pnlR.FireMouseWheel(x - pnlR.Left, y - pnlR.Top, delta);
                if (UseFooter) pnlB.FireMouseWheel(x - pnlB.Left, y - pnlB.Top, delta);

                CurrentPage?.FireMouseWheel(x - CurrentPage.Left, y - CurrentPage.Top, delta);
            }
            #endregion
        }
        #endregion

        #region Key
        public void KeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                GUI.KeyDown(dropdownWindow, Shift, Control, Alt, key);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                GUI.KeyDown(stkWindow.Peek(), Shift, Control, Alt, key);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) GUI.KeyDown(pnlT, Shift, Control, Alt, key);
                if (UseLeftSideBar) GUI.KeyDown(pnlL, Shift, Control, Alt, key);
                if (UseRightSideBar) GUI.KeyDown(pnlR, Shift, Control, Alt, key);
                if (UseFooter) GUI.KeyDown(pnlB, Shift, Control, Alt, key);
                
                if (CurrentPage != null) GUI.KeyDown(CurrentPage, Shift, Control, Alt, key);
            }
            #endregion
        }

        public void KeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                GUI.KeyUp(dropdownWindow, Shift, Control, Alt, key);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                GUI.KeyUp(stkWindow.Peek(), Shift, Control, Alt, key);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) GUI.KeyUp(pnlT, Shift, Control, Alt, key);
                if (UseLeftSideBar) GUI.KeyUp(pnlL, Shift, Control, Alt, key);
                if (UseRightSideBar) GUI.KeyUp(pnlR, Shift, Control, Alt, key);
                if (UseFooter) GUI.KeyUp(pnlB, Shift, Control, Alt, key);

                if (CurrentPage != null) GUI.KeyUp(CurrentPage, Shift, Control, Alt, key);
            }
            #endregion
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

            if (container != null)
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

        #region bounds
        (SKRect rtL, SKRect rtT, SKRect rtR, SKRect rtB, SKRect rtF) bounds()
        {
            var hT = UseTitleBar ? pnlT.BarSize : 0;
            var hB = UseFooter ? pnlB.BarSize : 0;
            var wL = UseLeftSideBar ? pnlL.BarSize : 0;
            var wR = UseRightSideBar ? pnlR.BarSize : 0;

            var rtT = Util.FromRect(0, 0, Width, hT);
            var rtB = Util.FromRect(0, Height - hB, Width, hB);
            var rtL = Util.FromRect(0, rtT.Bottom, wL, Height - hB - hT);
            var rtR = Util.FromRect(Width - wR, rtT.Bottom, wR, Height - hB - hT);
            var rtF = new SKRect(rtL.Right, rtT.Bottom, rtR.Left, rtB.Top);

            return (rtL, rtT, rtR, rtB, rtF);
        }
        #endregion

        #region roundPage
        SKRoundRect roundPage(SKRect rtF)
        {
            var corner = GoTheme.Current.Corner;
            var ret = new SKRoundRect(rtF, corner);

            #region var
            var ut = UseTitleBar;
            var ul = UseLeftSideBar;
            var ur = UseRightSideBar;
            var ub = UseFooter;
            #endregion

            ret.SetNinePatch(rtF, ul ? corner : 0, ut ? corner : 0, ur ? corner : 0, ub ? corner : 0);

            return ret;
        }
        #endregion
        #endregion
    }

}
