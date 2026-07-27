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
    /// <summary>
    /// 자식 컨트롤을 포함할 수 있는 컨테이너의 추상 기본 클래스입니다. GoControl을 상속하며 IGoContainer를 구현합니다.
    /// </summary>
    public abstract class GoContainer : GoControl, IGoContainer
    {
        #region Properties
        /// <summary>
        /// 자식 컨트롤 컬렉션을 가져옵니다.
        /// </summary>
        public virtual IEnumerable<IGoControl> Childrens { get; } = [];

        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다.
        /// </summary>
        [JsonIgnore] public virtual SKRect PanelBounds => Util.FromRect(0, 0, Width, Height);
        /// <summary>
        /// 현재 뷰의 스크롤 오프셋 위치를 가져옵니다.
        /// </summary>
        [JsonIgnore] public virtual SKPoint ViewPosition => new SKPoint(0, 0);
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnInit(GoDesign? design)
        {
            base.OnInit(design);

            GUI.Init(design, this);
        }

        /// <inheritdoc/>
        protected override void OnShow()
        {
            base.OnShow();

            GUI.Show(this);
        }

        /// <inheritdoc/>
        protected override void OnHide()
        {
            base.OnHide();

            GUI.Hide(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            GUI.Update(this);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            base.OnMouseDown(x, y, button);

            GUI.MouseDown(this, x - PanelBounds.Left, y - PanelBounds.Top, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            base.OnMouseUp(x, y, button);

            GUI.MouseUp(this, x - PanelBounds.Left, y - PanelBounds.Top, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            base.OnMouseDoubleClick(x, y, button);

            GUI.MouseDoubleClick(this, x - PanelBounds.Left, y - PanelBounds.Top, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);

            GUI.MouseMove(this, x - PanelBounds.Left, y - PanelBounds.Top);
        }

        /// <inheritdoc/>
        protected override void OnMouseWheel(float x, float y, float delta)
        {
            base.OnMouseWheel(x, y, delta);

            GUI.MouseWheel(this, x - PanelBounds.Left, y - PanelBounds.Top, delta);
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            base.OnKeyDown(Shift, Control, Alt, key);

            GUI.KeyDown(this, Shift, Control, Alt, key);
        }

        /// <inheritdoc/>
        protected override void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            base.OnKeyUp(Shift, Control, Alt, key);

            GUI.KeyUp(this, Shift, Control, Alt, key);
        }

        /// <inheritdoc/>
        protected override void OnDispose()
        {
            base.OnDispose();

            foreach (var c in Childrens) c.Dispose();
        }
        #endregion

        #region Virtual
        /// <summary>
        /// 자식 컨트롤들의 위치와 크기를 배치합니다. Dock 스타일에 따라 각 컨트롤의 경계를 설정합니다.
        /// </summary>
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
                c.Bottom = bnds.Bottom - c.Margin.Bottom;
            }
        }
        #endregion

        #region ApplyDocking
        /// <summary>
        /// 자식 컨트롤에 도킹 스타일을 적용하고 남은 영역을 반환합니다.
        /// </summary>
        /// <param name="c">도킹을 적용할 자식 컨트롤</param>
        /// <param name="rt">현재 사용 가능한 영역</param>
        /// <returns>도킹 적용 후 남은 영역</returns>
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
