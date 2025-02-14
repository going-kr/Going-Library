using Going.UI.Controls;
using Going.UI.Datas;
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
    public class GoContainer : GoControl, IGoContainer
    {
        #region Properties
        public List<IGoControl> Childrens { get; } = [];
        #endregion


        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            OnContainerDraw(canvas);

            OnLayout();
            GUI.Draw(canvas, this);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            base.OnMouseDown(x, y, button);
            GUI.MouseDown(this, x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            base.OnMouseUp(x, y, button);
            GUI.MouseUp(this, x, y, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            base.OnMouseDoubleClick(x, y, button);
            GUI.MouseDoubleClick(this, x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);
            GUI.MouseMove(this, x, y);
        }

        protected override void OnMouseWheel(float x, float y, float delta)
        {
            base.OnMouseWheel(x, y, delta);
            GUI.MouseWheel(this, x, y, delta);
        }
        #endregion

        #region Virtual
        protected virtual void OnContainerDraw(SKCanvas canvas) { }
        protected virtual void OnLayout() { }
        #endregion
    }
}
