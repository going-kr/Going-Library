using Going.UI.Containers;
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

namespace Going.UI.Design
{
    public class GoTitleBar : GoContainer
    {
        #region Properties
        public float BarSize { get; set; } = 50F;

        public string Title { get; set; } = "Title";
        public string? TitleImage { get; set; } = null;

        public string LeftExpandIconString { get; set; } = "fa-bars";
        public string LeftCollapseIconString { get; set; } = "fa-chevron-left";
        public string RightExpandIconString { get; set; } = "fa-ellipsis-vertical";
        public string RightCollapseIconString { get; set; } = "fa-chevron-right";
        public float IconSize { get; set; } = 16;

        public string TextColor { get; set; } = "Fore";
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 16;

        public override SKRect PanelBounds => Areas()["Panel"];

        public override List<IGoControl> Childrens { get; } = [];
        #endregion

        #region Member Variable
        float mx = -1, my = -1;
        #endregion 

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var rts = Areas();
            var rtExpandL = rts["ExpandL"];
            var rtTitle = rts["Title"];
            var rtPanel = rts["Panel"];
            var rtExpandR = rts["ExpandR"];
            var img = Design?.GetImage(TitleImage)?.FirstOrDefault();
            #endregion

            #region Title
            var (rtIco, rtText) = Util.TextIconBounds(Title, FontName, FontStyle, FontSize, new SKSize(img?.Width ?? 0, img?.Height ?? 0), GoDirectionHV.Horizon, 10, rtTitle, GoContentAlignment.MiddleCenter);
            if (img != null) canvas.DrawBitmap(img, rtIco);
            Util.DrawText(canvas, Title, FontName, FontStyle, FontSize, rtText, cText);
            #endregion

            if (rtExpandL.Width > 1)
            {
                var hover = CollisionTool.Check(rtExpandL, mx, my);
                Util.DrawIcon(canvas, (Design?.LeftSideBar.Expand ?? false) ? LeftCollapseIconString : LeftExpandIconString, IconSize, rtExpandL, cText.WithAlpha(Convert.ToByte(hover ? 255 : 60)));
            }

            if (rtExpandR.Width > 1)
            {
                var hover = CollisionTool.Check(rtExpandR, mx, my);
                Util.DrawIcon(canvas, (Design?.RightSideBar.Expand ?? false) ? RightCollapseIconString : RightExpandIconString, IconSize, rtExpandR, cText.WithAlpha(Convert.ToByte(hover ? 255 : 60)));
            }

            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtExpandL = rts["ExpandL"];
            var rtTitle = rts["Title"];
            var rtPanel = rts["Panel"];
            var rtExpandR = rts["ExpandR"];

            if (Design != null)
            {
                if (CollisionTool.Check(rtExpandL, x, y)) Design.LeftSideBar.Expand = !Design.LeftSideBar.Expand;
                if (CollisionTool.Check(rtExpandR, x, y)) Design.RightSideBar.Expand = !Design.RightSideBar.Expand;
            }

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseMove(float x, float y) { mx = x; my = y; base.OnMouseMove(x, y); }
        #endregion

        #region OnLayout
        protected override void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];
            foreach (var c in Childrens)
            {
                if (c.Fill)
                {
                    c.Bounds = Util.FromRect(Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height), c.Margin);
                }
            }

            //base.OnLayout();
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            #region tw
            var sz = Util.MeasureText(Title, FontName, FontStyle, FontSize);
            var tw = string.IsNullOrEmpty(Title) ? 0 : sz.Width + 20;
            var img = Design?.GetImage(TitleImage)?.FirstOrDefault();
            if (img != null) tw = sz.Width + 10 + (img?.Width ?? 0) + 20;
           
            var lbw = (Design?.UseLeftSideBar ?? false) && !(Design?.LeftSideBar.Fixed ?? false) ? rtContent.Height : 0;
            var rbw = (Design?.UseRightSideBar ?? false) && !(Design?.RightSideBar.Fixed ?? false) ? rtContent.Height : 0;
            #endregion

            var rts = Util.Columns(rtContent, [$"{lbw}px", $"{tw}px", $"100%", $"{rbw}px"]);

            dic["ExpandL"] = rts[0];
            dic["Title"] = rts[1];
            dic["Panel"] = rts[2];
            dic["ExpandR"] = rts[3];

            return dic;
        }
        #endregion
        #endregion
    }


    public class GoSideBar : GoContainer
    {
        #region Properties
        public float BarSize { get; set; } = 150F;
        public bool Fixed { get; set; } = false;

        internal float RealBarSize => Fixed ? BarSize : (ani.IsPlaying ? (ani.Variable == "Expand" ? ani.Value(AnimationAccel.DCL, 0F, BarSize)
                                                                                                   : ani.Value(AnimationAccel.DCL, BarSize, 0F))
                                                                       : (Expand ? BarSize : 0));
        public bool Opening => !Fixed && ani.IsPlaying && ani.Variable == "Expand";
        public bool Closing => !Fixed && ani.IsPlaying && ani.Variable == "Collapse";

        public override List<IGoControl> Childrens { get; } = [];

        [JsonIgnore]
        public bool Expand
        {
            get => Fixed ? true : bExpand;
            set
            {
                if (Fixed) bExpand = true;
                else
                {
                    if (bExpand != value)
                    {
                        ani.Stop();
                        ani.Start(Animation.Time200, value ? "Expand" : "Collapse", () =>
                        {
                            bExpand = value;
                        });
                    }
                }
            }
        }
        #endregion

        #region Member Variable
        private bool bExpand = false;
        private Animation ani = new();
        #endregion
    }

    public class GoFooter : GoContainer
    {
        public float BarSize { get; set; } = 40F;

        public override List<IGoControl> Childrens { get; } = [];
    }
}
