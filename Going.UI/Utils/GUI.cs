using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
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
    /// <summary>
    /// 부모에서 자식을 그려주기 위한 GUI 클래스입니다.
    /// 이를 따로 뺀 이유는 컨테이너가 복잡해지지 않게끔 하기 위함입니다.
    /// FirstRender는 컨트롤이 처음 그려졌는지를 나타냅니다.
    /// 그래서 처음 각 객체를 Init을 더 이상을 호출하지 않게끔 하기 위함입니다.
    /// 게다가 Collection을 Json처리에 불편해서, Design이나 Parents가 자동으로 세팅되었으면 좋겠다 싶어서 밑에 using new SKAutoCanvasRestore(canvas)를 사용했습니다.
    /// </summary>
    public class GUI
    {
        public static void Init(GoDesign? design, IGoContainer container )
        {
            foreach (var c in container.Childrens)
                c.FireInit(design);
        }

        public static void Draw(SKCanvas canvas, IGoContainer container)
        {
            foreach (var c in container.Childrens)
            {
                if (CollisionTool.Check(Util.FromRect(container.ViewX, container.ViewY, container.ViewWidth, container.ViewHeight), c.Bounds))
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
        }

        public static void Update(IGoContainer container)
        {
            foreach (var c in container.Childrens)
                c.FireUpdate();
        }

        public static void MouseDown(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewX, container.ViewY, container.ViewWidth, container.ViewHeight), c.Bounds))
                    c.FireMouseDown(x - c.Left, y - c.Top, btn);
        }   

        public static void MouseUp(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewX, container.ViewY, container.ViewWidth, container.ViewHeight), c.Bounds) || (c is GoControl c2 && c2._MouseDown_))
                    c.FireMouseUp(x - c.Left, y - c.Top, btn);
        }

        public static void MouseDoubleClick(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewX, container.ViewY, container.ViewWidth, container.ViewHeight), c.Bounds))
                    c.FireMouseDoubleClick(x - c.Left, y - c.Top, btn);
        }

        public static void MouseMove(IGoContainer container, float x, float y)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewX, container.ViewY, container.ViewWidth, container.ViewHeight), c.Bounds) || (c is GoControl c2 && c2._MouseDown_))
                    c.FireMouseMove(x - c.Left, y - c.Top);
        }

        public static void MouseWheel(IGoContainer container, float x, float y, float delta)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewX, container.ViewY, container.ViewWidth, container.ViewHeight), c.Bounds))
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
