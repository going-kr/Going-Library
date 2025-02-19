using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Enums;
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
        public static void Draw(SKCanvas canvas, IGoContainer container)
        {
            foreach (var c in container.Childrens)
            {
                if (c.FirstRender && c is GoControl c2)
                    c2.Parent = container;

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(c.Bounds);
                    canvas.Translate(c.Left, c.Top);
                    c.FireDraw(canvas);
                }

                if (c.FirstRender && c is GoControl c3) c3.FirstRender = false;
            }
        }

        public static void Update(IGoContainer container)
        {
            foreach (var c in container.Childrens) c.FireUpdate();
        }

        public static void MouseDown(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                c.FireMouseDown(x - c.Left, y - c.Top, btn);
        }

        public static void MouseUp(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                c.FireMouseUp(x - c.Left, y - c.Top, btn);
        }

        public static void MouseDoubleClick(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                c.FireMouseDoubleClick(x - c.Left, y - c.Top, btn);
        }

        public static void MouseMove(IGoContainer container, float x, float y)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                c.FireMouseMove(x - c.Left, y - c.Top);
        }

        public static void MouseWheel(IGoContainer container, float x, float y, float delta)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
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
