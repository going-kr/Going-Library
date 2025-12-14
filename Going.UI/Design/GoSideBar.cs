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
        [GoProperty(PCategory.Control, 0)] public float BarSize { get; set; } = 50F;

        [GoProperty(PCategory.Control, 1)] public string Title { get; set; } = "Title";
        [GoImageProperty(PCategory.Control, 2)] public string? TitleImage { get; set; } = null;

        [GoProperty(PCategory.Control, 3)] public string LeftExpandIconString { get; set; } = "fa-bars";
        [GoProperty(PCategory.Control, 4)] public string LeftCollapseIconString { get; set; } = "fa-chevron-left";
        [GoProperty(PCategory.Control, 5)] public string RightExpandIconString { get; set; } = "fa-ellipsis-vertical";
        [GoProperty(PCategory.Control, 6)] public string RightCollapseIconString { get; set; } = "fa-chevron-right";
        [GoProperty(PCategory.Control, 7)] public float IconSize { get; set; } = 16;

        [GoProperty(PCategory.Control, 8)] public string TextColor { get; set; } = "Fore";
        [GoFontNameProperty(PCategory.Control, 9)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 10)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 11)] public float FontSize { get; set; } = 16;

        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];

        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        #endregion

        #region Member Variable
        float mx = -1, my = -1;
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoTitleBar(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoTitleBar() { }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            #region var
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
            if (img != null) canvas.DrawImage(img, rtIco, Util.Sampling);
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

            base.OnDraw(canvas, thm);
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
        [GoProperty(PCategory.Control, 0)] public float BarSize { get; set; } = 150F;
        [GoProperty(PCategory.Control, 1)] public bool Fixed { get; set; } = false;

        internal float RealBarSize => Fixed ? BarSize : (ani.IsPlaying ? (ani.Variable == "Expand" ? ani.Value(AnimationAccel.DCL, 0F, BarSize)
                                                                                                   : ani.Value(AnimationAccel.DCL, BarSize, 0F))
                                                                       : (Expand ? BarSize : 0));
        public bool Opening => !Fixed && ani.IsPlaying && ani.Variable == "Expand";
        public bool Closing => !Fixed && ani.IsPlaying && ani.Variable == "Collapse";

        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        [GoProperty(PCategory.Control, 2)]
        [JsonIgnore]
        public bool Expand
        {
            get => Fixed ? true : bExpand;
            set
            {
                if (Fixed) bExpand = true;
                else
                {
                    if (Design?.DesignMode ?? false)
                    {
                        bExpand = value;
                    }
                    else
                    {
                        var thm = Design?.Theme ?? GoTheme.DarkTheme;
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
        }
        #endregion

        #region Member Variable
        private bool bExpand = false;
        private Animation ani = new();
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoSideBar(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoSideBar()
        {
            ani.Refresh = () => Invalidate();
        }
        #endregion
    }

    public class GoFooter : GoContainer
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public float BarSize { get; set; } = 40F;

        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoFooter(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoFooter() { }
        #endregion
    }
}
