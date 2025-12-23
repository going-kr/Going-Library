using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Utils
{
    public class GUI
    {
        public static void Init(GoDesign? design, IGoContainer container )
        {
            foreach (var c in container.Childrens)
            {
                c.FireInit(design);
                if (c.Parent != container && c is GoControl c2) c2.Parent = container;
            }
        }

        public static void Show(IGoContainer container)
        {
            foreach (var c in container.Childrens)
                c.FireShow();
        }

        public static void Hide(IGoContainer container)
        {
            foreach (var c in container.Childrens)
                c.FireHide();
        }

        public static void Draw(SKCanvas canvas, GoTheme thm, IGoContainer container)
        {
            foreach (var c in container.Childrens)
            {
                if ((c.FirstRender || c.Parent != container) && c is GoControl c2)
                    c2.Parent = container;

                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds))
                {
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(c.Bounds);
                        canvas.Translate(c.Left, c.Top);
                        c.FireDraw(canvas, thm);
                    }
                }

                if (c.FirstRender && c is GoControl c3) c3.FirstRender = false;
            }
        }

        public static void Update(IGoContainer container)
        {
            foreach (var c in container.Childrens)
                c.FireUpdate();
        }

        public static void MouseDown(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds))
                    c.FireMouseDown(x - c.Left, y - c.Top, btn);
        }   

        public static void MouseUp(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds) || (c is GoControl c2 && c2._MouseDown_))
                    c.FireMouseUp(x - c.Left, y - c.Top, btn);
        }

        public static void MouseDoubleClick(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds))
                    c.FireMouseDoubleClick(x - c.Left, y - c.Top, btn);
        }

        public static void MouseMove(IGoContainer container, float x, float y)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds) || (c is GoControl c2 && c2._MouseDown_))
                    c.FireMouseMove(x - c.Left, y - c.Top);
        }

        public static void MouseWheel(IGoContainer container, float x, float y, float delta)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds))
                    c.FireMouseWheel(x - c.Left, y - c.Top, delta);
        }

        public static void KeyDown(IGoContainer container, bool Shift, bool Control, bool Alt, GoKeys key)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                c.FireKeyDown(Shift, Control, Alt, key);    
        }

        public static void KeyUp(IGoContainer container, bool Shift, bool Control, bool Alt, GoKeys key)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                c.FireKeyUp(Shift, Control, Alt, key);
        }

    }
}
