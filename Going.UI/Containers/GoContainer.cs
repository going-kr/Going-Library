﻿using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
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
    public class GoContainer : GoControl, IGoContainer
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

                if (c is GoNavBar nav)
                {
                    var rt = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height);
                    switch (nav.Direction)
                    {
                        case GoDirection.Left: c.Bounds = MathTool.MakeRectangle(rt, new SKSize(nav.NavBarSize, rt.Height), GoContentAlignment.TopLeft); break;
                        case GoDirection.Up: c.Bounds = MathTool.MakeRectangle(rt, new SKSize(rt.Width, nav.NavBarSize), GoContentAlignment.TopLeft); break;
                        case GoDirection.Right: c.Bounds = MathTool.MakeRectangle(rt, new SKSize(nav.NavBarSize, rt.Height), GoContentAlignment.TopRight); break;
                        case GoDirection.Down: c.Bounds = MathTool.MakeRectangle(rt, new SKSize(rt.Width, nav.NavBarSize), GoContentAlignment.BottomLeft); break;
                    }
                }
            }
        }
        #endregion
    }
}
