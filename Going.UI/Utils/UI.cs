using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Enums;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Utils
{
    public class UI
    {
        public static void Draw(SKCanvas canvas, IEnumerable<IGoControl> controls)
        {
            foreach (var c in controls)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(c.Bounds);
                    canvas.Translate(c.Left, c.Top);
                    c.Draw(canvas);
                }
            }
        }

        public static void Update(IEnumerable<IGoControl> controls)
        {
            foreach (var c in controls) c.Update();
        }

        public static void MouseDown(IEnumerable<IGoControl> controls, float x, float y, GoMouseButton btn)
        {
            foreach (var c in controls)
                c.MouseDown(x - c.Left, y - c.Top, btn);
        }

        public static void MouseUp(IEnumerable<IGoControl> controls, float x, float y, GoMouseButton btn)
        {
            foreach (var c in controls)
                c.MouseUp(x - c.Left, y - c.Top, btn);
        }

        public static void MouseDoubleClick(IEnumerable<IGoControl> controls, float x, float y, GoMouseButton btn)
        {
            foreach (var c in controls)
                c.MouseDoubleClick(x - c.Left, y - c.Top, btn);
        }

        public static void MouseMove(IEnumerable<IGoControl> controls, float x, float y)
        {
            foreach (var c in controls)
                c.MouseMove(x - c.Left, y - c.Top);
        }

        public static void MouseWheel(IEnumerable<IGoControl> controls, float x, float y, float delta)
        {
            foreach (var c in controls)
                c.MouseWheel(x - c.Left, y - c.Top, delta);
        }
    }
}
