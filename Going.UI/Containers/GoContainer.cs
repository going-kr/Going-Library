using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    // 컨테이너지만 컨트롤 중 하나라서 GoControl을 상속합니다.
    public abstract class GoContainer : GoControl, IGoContainer
    {
        #region Properties
        public virtual IEnumerable<IGoControl> Childrens { get; } = [];

        [JsonIgnore] public virtual SKRect PanelBounds => Util.FromRect(0, 0, Width, Height);
        [JsonIgnore] public virtual SKPoint ViewPosition => new SKPoint(0, 0);
        #endregion

        #region Override
        protected override void OnInit(GoDesign? design)
        {
            base.OnInit(design);

            GUI.Init(design, this);
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            OnLayout();
         
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.Translate(PanelBounds.Left, PanelBounds.Top);
                GUI.Draw(canvas, this);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            GUI.Update(this);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            base.OnMouseDown(x, y, button);

            GUI.MouseDown(this, x - PanelBounds.Left, y - PanelBounds.Top, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            base.OnMouseUp(x, y, button);

            GUI.MouseUp(this, x - PanelBounds.Left, y - PanelBounds.Top, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            base.OnMouseDoubleClick(x, y, button);

            GUI.MouseDoubleClick(this, x - PanelBounds.Left, y - PanelBounds.Top, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);

            GUI.MouseMove(this, x - PanelBounds.Left, y - PanelBounds.Top);
        }

        protected override void OnMouseWheel(float x, float y, float delta)
        {
            base.OnMouseWheel(x, y, delta);

            GUI.MouseWheel(this, x - PanelBounds.Left, y - PanelBounds.Top, delta);
        }

        protected override void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            base.OnKeyDown(Shift, Control, Alt, key);

            GUI.KeyDown(this, Shift, Control, Alt, key);
        }

        protected override void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            base.OnKeyUp(Shift, Control, Alt, key);

            GUI.KeyUp(this, Shift, Control, Alt, key);
        }
        #endregion

        #region Virtual
        protected virtual void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Content"];
            foreach (var c in Childrens)
            {
                if (c.Fill)
                {
                    c.Bounds = Util.FromRect(Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height), c.Margin);
                }
            }
        }
        #endregion
    }

    public class GoBox : GoContainer
    { 
        #region Properties
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoBox(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoBox() { }
        #endregion
    }
}
