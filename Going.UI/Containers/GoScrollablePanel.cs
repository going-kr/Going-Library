using Going.UI.Controls;
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
    public class GoScrollablePanel : GoContainer
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public float? PanelWidth { get; set; }
        [GoProperty(PCategory.Control, 1)] public float? PanelHeight { get; set; }

        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        [JsonIgnore] public override SKPoint ViewPosition => new SKPoint(-Convert.ToSingle(hscroll.ScrollPositionWithOffset), -Convert.ToSingle(vscroll.ScrollPositionWithOffset));
        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];
        #endregion

        #region Member Variable
        Scroll vscroll = new Scroll() { Direction = ScrollDirection.Vertical };
        Scroll hscroll = new Scroll() { Direction = ScrollDirection.Horizon };
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoScrollablePanel(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoScrollablePanel()
        {
            hscroll.GetScrollTotal = () => PanelWidth.HasValue ? PanelWidth.Value : (Childrens.Count() > 0 ? Childrens.Max(x => x.Right) : 0);
            hscroll.GetScrollTick = () => 10;
            hscroll.GetScrollView = () =>
            {
                var t = PanelWidth.HasValue ? PanelWidth.Value : (Childrens.Count() > 0 ? Childrens.Max(x => x.Right) : 0);
                return Width - 1 - (t > Width ? Scroll.SC_WH : 0);
            };
            hscroll.Refresh = () => Invalidate();

            vscroll.GetScrollTotal = () => PanelHeight.HasValue ? PanelHeight.Value : (Childrens.Count() > 0 ? Childrens.Max(x => x.Bottom) : 0);
            vscroll.GetScrollTick = () => 10;
            vscroll.GetScrollView = () =>
            {
                var t = PanelHeight.HasValue ? PanelHeight.Value : (Childrens.Count() > 0 ? Childrens.Max(x => x.Bottom) : 0);
                return Height - 1 - (t > Height ? Scroll.SC_WH : 0);
            };
            vscroll.Refresh = () => Invalidate();
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];
            var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);
            var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);

            vscroll.Draw(canvas, thm, rtScrollV);
            hscroll.Draw(canvas, thm, rtScrollH);

            if (Design != null && Design.DesignMode)
            {
                var rt = rtPanel; rt.Inflate(-0.5F, -0.5F);
                using var pe = SKPathEffect.CreateDash([1, 2], 2);
                using var p = new SKPaint { };
                p.IsStroke = true; p.StrokeWidth = 1; p.Color = thm.Base3;
                p.PathEffect = pe;
                canvas.DrawRect(rt, p);
            }

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.Translate(hspos, vspos);
                canvas.ClipRect(Util.FromRect(rtPanel.Left + ViewPosition.X, rtPanel.Top + ViewPosition.Y, rtPanel.Width, rtPanel.Height));


                base.OnDraw(canvas, thm);
            }
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            if (!(Design?.DesignMode ?? false))
            {
                base.OnMouseDown(x + ViewPosition.X, y + ViewPosition.Y, button);

                if (Design?.SelectedControl == null)
                {
                    #region Scroll
                    vscroll.MouseDown(x, y, rtScrollV);
                    if (Scroll.TouchMode && CollisionTool.Check(rtPanel, x, y)) vscroll.TouchDown(x, y);

                    hscroll.MouseDown(x, y, rtScrollH);
                    if (Scroll.TouchMode && CollisionTool.Check(rtPanel, x, y)) hscroll.TouchDown(x, y);
                    #endregion
                }
            }
            else
            {
                vscroll.MouseDown(x, y, rtScrollV);
                hscroll.MouseDown(x, y, rtScrollH);
            }
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            if (!(Design?.DesignMode ?? false))
            {
                base.OnMouseMove(x + ViewPosition.X, y + ViewPosition.Y);

                #region Scroll
                vscroll.MouseMove(x, y, rtScrollV);
                if (Scroll.TouchMode) vscroll.TouchMove(x, y);

                hscroll.MouseMove(x, y, rtScrollH);
                if (Scroll.TouchMode) hscroll.TouchMove(x, y);
                #endregion
            }
            else
            {
                vscroll.MouseMove(x, y, rtScrollV);
                hscroll.MouseMove(x, y, rtScrollH);
            }
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            if (!(Design?.DesignMode ?? false))
            {
                base.OnMouseUp(x + ViewPosition.X, y + ViewPosition.Y, button);

                #region Scroll
                vscroll.MouseUp(x, y);
                if (Scroll.TouchMode) vscroll.TouchUp(x, y);

                hscroll.MouseUp(x, y);
                if (Scroll.TouchMode) hscroll.TouchUp(x, y);
                #endregion
            }
            else
            {
                vscroll.MouseUp(x, y);
                hscroll.MouseUp(x, y);
            }
        }

        protected override void OnMouseWheel(float x, float y, float delta)
        {
            if (CollisionTool.Check(Areas()["Content"], x, y))
            {
                base.OnMouseWheel(x + ViewPosition.X, y + ViewPosition.Y, delta);

                vscroll.MouseWheel(x, y, delta);
            }
        }

        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            base.OnMouseClick(x + ViewPosition.X, y + ViewPosition.Y, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            base.OnMouseDoubleClick(x + ViewPosition.X, y + ViewPosition.Y, button);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            base.OnMouseLongClick(x + ViewPosition.X, y + ViewPosition.Y, button);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var scv = vscroll.ScrollVisible ? Scroll.SC_WH : 0;
            var sch = hscroll.ScrollVisible ? Scroll.SC_WH : 0;

            var dic = base.Areas();
            var rtContent = dic["Content"];

            var rtVO = MathTool.MakeRectangle(rtContent, new SKSize(PanelWidth ?? rtContent.Width, rtContent.Height));

            var rtPanel = Util.FromRect(rtVO.Left, rtContent.Top, rtVO.Width - scv, rtContent.Height - sch);
            var rtScrollV = Util.FromRect(rtPanel.Right, rtPanel.Top, scv, rtPanel.Height);
            var rtScrollH = Util.FromRect(rtPanel.Left, rtPanel.Bottom, rtPanel.Width, sch);

            dic["Panel"] = rtPanel;
            dic["ScrollV"] = rtScrollV;
            dic["ScrollH"] = rtScrollH;

            return dic;
        }
        #endregion
        #endregion
    }
}
