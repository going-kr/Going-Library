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
        float vwmax, vhmax;
        int n;
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoScrollablePanel(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoScrollablePanel()
        {
            hscroll.GetScrollTotal = () => vwmax;
            hscroll.GetScrollTick = () => 10;
            hscroll.GetScrollView = () =>
            {
                var pw = PanelWidth ?? Width;
                return pw - 1;
            };
            hscroll.Refresh = () => Invalidate();

            vscroll.GetScrollTotal = () => vhmax;
            vscroll.GetScrollTick = () => 10;
            vscroll.GetScrollView = () =>
            {
                var pw = PanelWidth ?? Width;
                var ratio = Width / pw;
                var rh = ratio < 1 ? Height / ratio : Height;
                return rh - 1;
            };
            vscroll.Refresh = () => Invalidate();
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtPanel = rts["Panel"];
            var rtView = rts["View"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];
            
            var ratio = rtContent.Width / rtPanel.Width;
            using (new SKAutoCanvasRestore(canvas))
            {
                if (ratio < 1)
                {
                    canvas.Scale(ratio);
                }

                var vspos = Convert.ToSingle(vscroll.ScrollPositionWithOffset);
                var hspos = Convert.ToSingle(hscroll.ScrollPositionWithOffset);

                vscroll.Draw(canvas, thm, rtScrollV);
                hscroll.Draw(canvas, thm, rtScrollH);

                if (Design != null && Design.DesignMode)
                {
                    var rt = rtView; rt.Inflate(-0.5F, -0.5F);
                    using var pe = SKPathEffect.CreateDash([1, 2], 2);
                    using var p = new SKPaint { };
                    p.IsStroke = true; p.StrokeWidth = 1; p.Color = thm.Base3;
                    p.PathEffect = pe;
                    canvas.DrawRect(rt, p);
                }

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Translate(hspos, vspos);
                    canvas.ClipRect(Util.FromRect(rtView.Left + ViewPosition.X, rtView.Top + ViewPosition.Y, rtView.Width, rtView.Height));


                    base.OnDraw(canvas, thm);
                }
            }
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtPanel = rts["Panel"];
            var rtView = rts["View"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            var ratio = rtContent.Width / rtPanel.Width;
            if (ratio < 1)
            {
                x /= ratio;
                y /= ratio;
            }

            if (!(Design?.DesignMode ?? false))
            {
                base.OnMouseDown(x + ViewPosition.X, y + ViewPosition.Y, button);

                if (Design?.SelectedControl == null)
                {
                    #region Scroll
                    vscroll.MouseDown(x, y, rtScrollV);
                    if (Scroll.TouchMode && CollisionTool.Check(rtContent, x, y)) vscroll.TouchDown(x, y);

                    hscroll.MouseDown(x, y, rtScrollH);
                    if (Scroll.TouchMode && CollisionTool.Check(rtContent, x, y)) hscroll.TouchDown(x, y);
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
            var rtContent = rts["Content"];
            var rtPanel = rts["Panel"];
            var rtView = rts["View"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            var ratio = rtContent.Width / rtPanel.Width;
            
            if (ratio < 1)
            {
                x /= ratio;
                y /= ratio;
            }

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
            var rtContent = rts["Content"];
            var rtPanel = rts["Panel"];
            var rtView = rts["View"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            var ratio = rtContent.Width / rtPanel.Width;
            if (ratio < 1)
            {
                x /= ratio;
                y /= ratio;
            }

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
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtPanel = rts["Panel"];
            var rtView = rts["View"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            var ratio = rtContent.Width / rtPanel.Width;
            if (ratio < 1)
            {
                x /= ratio;
                y /= ratio;
            }

            if (CollisionTool.Check(Areas()["Content"], x, y))
            {
                base.OnMouseWheel(x + ViewPosition.X, y + ViewPosition.Y, delta);
                vscroll.MouseWheel(x, y, delta);
            }

        }

        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtPanel = rts["Panel"];
            var rtView = rts["View"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            var ratio = rtContent.Width / rtPanel.Width;
            if (ratio < 1)
            {
                x /= ratio;
                y /= ratio;
            }
            base.OnMouseClick(x + ViewPosition.X, y + ViewPosition.Y, button);
 
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtPanel = rts["Panel"];
            var rtView = rts["View"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            var ratio = rtContent.Width / rtPanel.Width;
            if (ratio < 1)
            {
                x /= ratio;
                y /= ratio;
            }
            base.OnMouseDoubleClick(x + ViewPosition.X, y + ViewPosition.Y, button);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtPanel = rts["Panel"];
            var rtView = rts["View"];
            var rtScrollV = rts["ScrollV"];
            var rtScrollH = rts["ScrollH"];

            var ratio = rtContent.Width / rtPanel.Width;
            if (ratio < 1)
            {
                x /= ratio;
                y /= ratio;
            }
            base.OnMouseLongClick(x + ViewPosition.X, y + ViewPosition.Y, button);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var scv = vscroll.ScrollVisible ? Scroll.SC_WH : 0;
            var sch = hscroll.ScrollVisible ? Scroll.SC_WH : 0;

            vwmax = Childrens.Count() > 0 ? Childrens.Max(x => x.Right) : 0;
            vhmax = Childrens.Count() > 0 ? Childrens.Max(x => x.Bottom)+10 : 0;

            var dic = base.Areas();
            var rtContent = dic["Content"];

            var pw = PanelWidth ?? rtContent.Width;

            if (rtContent.Width >= pw)
            {
                var rtPanel = MathTool.MakeRectangle(rtContent, new SKSize(pw, rtContent.Height));
                var rtView = Util.FromRect(rtPanel.Left, rtContent.Top, rtPanel.Width - scv, rtPanel.Height - sch);
                var rtScrollV = Util.FromRect(rtView.Right, rtView.Top, scv, rtView.Height);
                var rtScrollH = Util.FromRect(rtView.Left, rtView.Bottom, rtView.Width, sch);

                dic["Panel"] = rtPanel;
                dic["View"] = rtPanel;
                dic["ScrollV"] = rtScrollV;
                dic["ScrollH"] = rtScrollH;
            }
            else
            {
                var ratio = rtContent.Width / pw;

                var rtPanel = new SKRect(0, 0, pw, rtContent.Height / ratio);
                var rtView = Util.FromRect(rtPanel.Left, rtPanel.Top, rtPanel.Width - scv, rtPanel.Height - sch);
                var rtScrollV = Util.FromRect(rtView.Right, rtView.Top, scv, rtView.Height);
                var rtScrollH = Util.FromRect(rtView.Left, rtView.Bottom, rtView.Width, sch);

                dic["Panel"] = rtPanel;
                dic["View"] = rtView;
                dic["ScrollV"] = rtScrollV;
                dic["ScrollH"] = rtScrollH;
            }

            return dic;
        }
        #endregion
        #endregion
    }
}
