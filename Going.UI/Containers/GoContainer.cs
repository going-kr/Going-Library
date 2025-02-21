using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    // 컨테이너지만 컨트롤 중 하나라서 GoControl을 상속합니다.
    public class GoContainer : GoControl, IGoContainer
    {
        #region Properties
        public virtual IEnumerable<IGoControl> Childrens { get; } = [];
        #endregion

        #region Override
        /// <summary>
        /// 컨테이너에서 재정의를 해주는 이유는 컨트롤과 컨테이너의 차이를 구분하기 위함입니다.
        /// GUI는 부모에서 자식을 그려주기 위한 클래스입니다.
        /// </summary>
        /// <param name="design"></param>
        protected override void OnInit(GoDesign? design)
        {
            base.OnInit(design);

            GUI.Init(design, this);
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            OnContainerDraw(canvas);

            OnLayout();
            GUI.Draw(canvas, this);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            GUI.MouseDown(this, x, y, button);
            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            GUI.MouseUp(this, x, y, button);
            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            GUI.MouseDoubleClick(this, x, y, button);
            base.OnMouseDoubleClick(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            GUI.MouseMove(this, x, y);
            base.OnMouseMove(x, y);
        }

        protected override void OnMouseWheel(float x, float y, float delta)
        {
            GUI.MouseWheel(this, x, y, delta);
            base.OnMouseWheel(x, y, delta);
        }

        protected override void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            GUI.KeyDown(this, Shift, Control, Alt, key);
            base.OnKeyDown(Shift, Control, Alt, key);
        }

        protected override void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            GUI.KeyUp(this, Shift, Control, Alt, key);
            base.OnKeyUp(Shift, Control, Alt, key);
        }
        #endregion

        #region Virtual
        protected virtual void OnContainerDraw(SKCanvas canvas) { }
        protected virtual void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Content"];
            foreach (var c in Childrens)
            {
                if (c.Fill)
                {
                    c.Bounds = Util.FromRect(rtPanel, c.Margin);
                }
            }
        }
        #endregion
    }
}
