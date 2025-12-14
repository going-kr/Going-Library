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
using System.IO;
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

        protected override void OnShow()
        {
            base.OnShow();

            GUI.Show(this);
        }

        protected override void OnHide()
        {
            base.OnHide();

            GUI.Hide(this);
        }

        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            OnLayout();

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.Translate(PanelBounds.Left, PanelBounds.Top);
                GUI.Draw(canvas, thm, this);
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

            var nofills = Childrens.Where(c => c.Dock != GoDockStyle.Fill).ToList();
            var fills = Childrens.Where(c => c.Dock == GoDockStyle.Fill).ToList();
            var bnds = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height); //Util.FromRect(rtPanel);
            foreach (var control in nofills) bnds = ApplyDocking(control, bnds);
            foreach (var c in fills)
            {
                c.Left = bnds.Left + c.Margin.Left;
                c.Top = bnds.Top + c.Margin.Top;
                c.Right = bnds.Right - c.Margin.Right;
                c.Bottom = bnds.Bottom- c.Margin.Bottom;
            }
        }
        #endregion

        #region ApplyDocking
        protected SKRect ApplyDocking(IGoControl c, SKRect rt)
        {
            var m = c.Margin;

            float ow = c.Width + m.Left + m.Right;
            float oh = c.Height + m.Top + m.Bottom;

            float dw = c.Width;
            float dh = c.Height;

            switch (c.Dock)
            {
                case GoDockStyle.Left:
                    c.Left = rt.Left + m.Left;
                    c.Top = rt.Top + m.Top;
                    c.Right = c.Left + dw;
                    c.Bottom = rt.Bottom - m.Bottom;
                    return new SKRect(rt.Left + ow, rt.Top, rt.Right, rt.Bottom);

                case GoDockStyle.Right:
                    c.Right = rt.Right - m.Right;
                    c.Bottom = rt.Bottom - m.Bottom;
                    c.Left = c.Right - dw;
                    c.Top = rt.Top + m.Top;
                    return new SKRect(rt.Left, rt.Top, rt.Right - ow, rt.Bottom);

                case GoDockStyle.Top:
                    c.Left = rt.Left + m.Left;
                    c.Top = rt.Top + m.Top;
                    c.Right = rt.Right - m.Right;
                    c.Bottom = c.Top + dh;
                    return new SKRect(rt.Left, rt.Top + oh, rt.Right, rt.Bottom);

                case GoDockStyle.Bottom:
                    c.Right = rt.Right - m.Right;
                    c.Bottom = rt.Bottom - m.Bottom;
                    c.Left = rt.Left + m.Left;
                    c.Top = c.Bottom - dh;
                    return new SKRect(rt.Left, rt.Top, rt.Right, rt.Bottom - oh);

                default:
                    return rt;
            }
        }
        #endregion
    }

}
