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
    /// <summary>
    /// 컨테이너 내 자식 컨트롤들의 이벤트를 일괄 처리하는 유틸리티 클래스입니다.
    /// </summary>
    public class GUI
    {
        /// <summary>
        /// 컨테이너의 모든 자식 컨트롤을 초기화합니다.
        /// </summary>
        /// <param name="design">디자인 설정</param>
        /// <param name="container">대상 컨테이너</param>
        public static void Init(GoDesign? design, IGoContainer container )
        {
            foreach (var c in container.Childrens)
            {
                c.FireInit(design);
                if (c.Parent != container && c is GoControl c2) c2.Parent = container;
            }
        }

        /// <summary>
        /// 컨테이너의 모든 자식 컨트롤에 Show 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        public static void Show(IGoContainer container)
        {
            foreach (var c in container.Childrens)
                c.FireShow();
        }

        /// <summary>
        /// 컨테이너의 모든 자식 컨트롤에 Hide 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        public static void Hide(IGoContainer container)
        {
            foreach (var c in container.Childrens)
                c.FireHide();
        }

        /// <summary>
        /// 컨테이너의 모든 자식 컨트롤을 캔버스에 그립니다.
        /// </summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="thm">적용할 테마</param>
        /// <param name="container">대상 컨테이너</param>
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

        /// <summary>
        /// 컨테이너의 모든 자식 컨트롤에 Update 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        public static void Update(IGoContainer container)
        {
            foreach (var c in container.Childrens)
                c.FireUpdate();
        }

        /// <summary>
        /// 컨테이너의 활성화된 자식 컨트롤에 마우스 다운 이벤트를 전달합니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="btn">마우스 버튼</param>
        public static void MouseDown(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds))
                    c.FireMouseDown(x - c.Left, y - c.Top, btn);
        }   

        /// <summary>
        /// 컨테이너의 활성화된 자식 컨트롤에 마우스 업 이벤트를 전달합니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="btn">마우스 버튼</param>
        public static void MouseUp(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds) || (c is GoControl c2 && c2._MouseDown_))
                    c.FireMouseUp(x - c.Left, y - c.Top, btn);
        }

        /// <summary>
        /// 컨테이너의 활성화된 자식 컨트롤에 마우스 더블클릭 이벤트를 전달합니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="btn">마우스 버튼</param>
        public static void MouseDoubleClick(IGoContainer container, float x, float y, GoMouseButton btn)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds))
                    c.FireMouseDoubleClick(x - c.Left, y - c.Top, btn);
        }

        /// <summary>
        /// 컨테이너의 활성화된 자식 컨트롤에 마우스 이동 이벤트를 전달합니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        public static void MouseMove(IGoContainer container, float x, float y)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds) || (c is GoControl c2 && c2._MouseDown_))
                    c.FireMouseMove(x - c.Left, y - c.Top);
        }

        /// <summary>
        /// 컨테이너의 활성화된 자식 컨트롤에 마우스 휠 이벤트를 전달합니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="delta">휠 회전량</param>
        public static void MouseWheel(IGoContainer container, float x, float y, float delta)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                if (CollisionTool.Check(Util.FromRect(container.ViewPosition.X, container.ViewPosition.Y, container.PanelBounds.Width, container.PanelBounds.Height), c.Bounds))
                    c.FireMouseWheel(x - c.Left, y - c.Top, delta);
        }

        /// <summary>
        /// 컨테이너의 활성화된 자식 컨트롤에 키 다운 이벤트를 전달합니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">눌린 키</param>
        public static void KeyDown(IGoContainer container, bool Shift, bool Control, bool Alt, GoKeys key)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                c.FireKeyDown(Shift, Control, Alt, key);    
        }

        /// <summary>
        /// 컨테이너의 활성화된 자식 컨트롤에 키 업 이벤트를 전달합니다.
        /// </summary>
        /// <param name="container">대상 컨테이너</param>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">놓인 키</param>
        public static void KeyUp(IGoContainer container, bool Shift, bool Control, bool Alt, GoKeys key)
        {
            foreach (var c in container.Childrens.Where(x => x.Enabled))
                c.FireKeyUp(Shift, Control, Alt, key);
        }

    }
}
