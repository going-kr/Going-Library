using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public class GoTabControl : GoContainer
    {
        #region Properties
        public GoDirectionHV IconDirection { get; set; }
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;

        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string TabColor { get; set; } = "Base2";
        public string TabBorderColor { get; set; } = "Base3";
        public GoDirection TabPosition { get; set; } = GoDirection.Up;

        public float NavSize { get; set; } = 40;

        [JsonIgnore]
        public GoTabPage? SelectedTab
        {
            get => selTab;
            set
            {
                if ((value == null || (value != null && TabPages.Contains(value))) && selTab != value)
                {
                    var args = new CancelEventArgs { Cancel = false };
                    TabChanging?.Invoke(this, args);

                    if (!args.Cancel)
                    {
                        selTab = value;
                        TabCnanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        [JsonIgnore] public override IEnumerable<IGoControl> Childrens => SelectedTab?.Childrens ?? [];

        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];

        [JsonInclude] public List<GoTabPage> TabPages { get; } = [];
        #endregion

        #region Event
        public event EventHandler? TabCnanged;
        public event EventHandler<CancelEventArgs>? TabChanging;
        #endregion

        #region Member Variable
        GoTabPage? selTab;
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoTabControl(List<GoTabPage> tabPages) : this() => this.TabPages = tabPages;
        public GoTabControl() { }
        #endregion

        #region Override
        #region Init
        protected override void OnInit(GoDesign? design)
        {
            base.OnInit(design);

            foreach (var tab in TabPages)
                foreach (var c in tab.Childrens)
                    c.FireInit(design);
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cTab = thm.ToColor(TabColor);
            var cBorder = thm.ToColor(TabBorderColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtNav = rts["Nav"];
            var rtPage = rts["Page"];
            var rtPanel = rts["Panel"];
            using var p = new SKPaint { IsAntialias = true };
            if (FirstRender && SelectedTab == null) SelectedTab = TabPages.FirstOrDefault();

            Util.DrawBox(canvas, rtPage, cTab, cBorder, GoRoundType.All, thm.Corner);

            tabLoop(rtNav, (i, tab, rt) =>
            {
                var bSel = tab == SelectedTab;
                if (bSel)
                {
                    #region retouch 1
                    p.IsStroke = false;
                    p.Color = cTab;
                    switch (TabPosition)
                    {
                        case GoDirection.Up: canvas.DrawRect(Util.FromRect(rt.Left - thm.Corner, rt.Bottom - 1, rt.Width + thm.Corner * 2, 2), p); break;
                        case GoDirection.Down: canvas.DrawRect(Util.FromRect(rt.Left - thm.Corner, rt.Top - 1, rt.Width + thm.Corner * 2, 2), p); break;
                        case GoDirection.Left: canvas.DrawRect(Util.FromRect(rt.Right - 1, rt.Top - thm.Corner, 2, rt.Height + thm.Corner * 2), p); break;
                        case GoDirection.Right: canvas.DrawRect(Util.FromRect(rt.Left - 1, rt.Top - thm.Corner, 2, rt.Height + thm.Corner * 2), p); break;
                    }
                    #endregion

                    #region tab
                    using var pth = PathTool.Tab(rt, TabPosition, thm.Corner);

                    p.IsStroke = false;
                    p.Color = cTab;
                    canvas.DrawPath(pth, p);

                    p.IsStroke = true;
                    p.Color = cBorder;
                    p.StrokeWidth = 1F;
                    canvas.DrawPath(pth, p);
                    #endregion

                    #region retouch 2
                    if (tab == TabPages.FirstOrDefault())
                    {
                        var c2 = thm.Corner * 2;
                        switch (TabPosition)
                        {
                            case GoDirection.Up:
                                {
                                    p.IsStroke = false; p.Color = cTab;
                                    canvas.DrawRect(Util.FromRect(rtPage.Left, rt.Bottom, c2, c2), p);

                                    var n = (int)rtPage.Left + 0.5F;
                                    p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                    canvas.DrawLine(n, rt.Bottom - c2, n, rt.Bottom + c2, p);
                                }
                                break;
                            case GoDirection.Down:
                                {
                                    p.IsStroke = false; p.Color = cTab;
                                    canvas.DrawRect(Util.FromRect(rtPage.Left, rt.Top - c2, c2, c2), p);

                                    var n = (int)rtPage.Left + 0.5F;
                                    p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                    canvas.DrawLine(n, rt.Top + c2, n, rt.Top - c2, p);
                                }
                                break;
                            case GoDirection.Left:
                                {
                                    p.IsStroke = false; p.Color = cTab;
                                    canvas.DrawRect(Util.FromRect(rt.Right, rtPage.Top, c2, c2), p);

                                    var n = (int)rtPage.Top + 0.5F;
                                    p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                    canvas.DrawLine(rt.Right - c2, n, rt.Right + c2, n, p);
                                }
                                break;
                            case GoDirection.Right:
                                {
                                    p.IsStroke = false; p.Color = cTab;
                                    canvas.DrawRect(Util.FromRect(rt.Left - c2, rtPage.Top, c2, c2), p);

                                    var n = (int)rtPage.Top + 0.5F;
                                    p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                    canvas.DrawLine(rt.Left + c2, n, rt.Left - c2, n, p);
                                }
                                break;
                        }
                    }
                    #endregion

                }

                Util.DrawTextIcon(canvas, tab.Text, FontName, FontStyle, FontSize, tab.IconString, IconSize, IconDirection, IconGap, rt, Util.FromArgb(Convert.ToByte(bSel ? 255 : (tab.Hover ? 150 : 60)), cText));
            });

            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            base.OnMouseMove(x, y);

            var rtNav = rts["Nav"];
            tabLoop(rtNav, (i, tab, rt) =>
            {
                tab.Hover = CollisionTool.Check(rt, x, y);
            });
        }

        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            base.OnMouseClick(x, y, button);

            var rtNav = rts["Nav"];
            tabLoop(rtNav, (i, tab, rt) =>
            {
                if (CollisionTool.Check(rt, x, y)) SelectedTab = tab;
            });
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();

            var rtContent = dic["Content"];

            switch (TabPosition)
            {
                case GoDirection.Up:
                    {
                        var rts = Util.Rows(rtContent, [$"{NavSize}px", "100%"]);
                        dic["Nav"] = rts[0];
                        dic["Page"] = rts[1];
                        dic["Panel"] = Util.FromRect(rts[1], new GoPadding(10));
                    }
                    break;
                case GoDirection.Down:
                    {
                        var rts = Util.Rows(rtContent, ["100%", $"{NavSize}px"]);
                        dic["Nav"] = rts[1];
                        dic["Page"] = rts[0];
                        dic["Panel"] = Util.FromRect(rts[0], new GoPadding(10));
                    }
                    break;
                case GoDirection.Left:
                    {
                        var rts = Util.Columns(rtContent, [$"{NavSize}px", "100%"]);
                        dic["Nav"] = rts[0];
                        dic["Page"] = rts[1];
                        dic["Panel"] = Util.FromRect(rts[1], new GoPadding(10));
                    }
                    break;
                case GoDirection.Right:
                    {
                        var rts = Util.Columns(rtContent, ["100%", $"{NavSize}px"]);
                        dic["Nav"] = rts[1];
                        dic["Page"] = rts[0];
                        dic["Panel"] = Util.FromRect(rts[0], new GoPadding(10));
                    }
                    break;
            }

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region tabLoop
        void tabLoop(SKRect rtNav, Action<int, GoTabPage, SKRect> loop)
        {
            var n = 0F; //Indent : 10px
            for (int i = 0; i < TabPages.Count; i++)
            {
                var tab = TabPages[i];
                var sz = Util.MeasureTextIcon(tab.Text, FontName, FontStyle, FontSize, tab.IconString, IconSize, IconDirection, IconGap);
                SKRect rt = new SKRect();

                switch (TabPosition)
                {
                    case GoDirection.Up:
                    case GoDirection.Down:
                        {
                            rt = Util.FromRect(n, rtNav.Top, sz.Width + 20, rtNav.Height);
                            n += rt.Width;
                        }
                        break;
                    case GoDirection.Left:
                    case GoDirection.Right:
                        {
                            rt = Util.FromRect(rtNav.Left, n, rtNav.Width, sz.Height + 20);
                            n += rt.Height;
                        }
                        break;
                }

                rt = Util.Int(rt); rt.Offset(0.5F, 0.5F);
                loop(i, tab, rt);
            }
        }
        #endregion

        #region SetTab
        public void SetTab(string name)
        {
            if(name != null)
            {
                var page = TabPages.FirstOrDefault(x => x.Name == name);
                if (page != null) SelectedTab = page;
            }
        }
        #endregion
        #endregion
    }

    #region class : GoTabPage
    public class GoTabPage 
    {
        public string Name { get; set; }
        public string? IconString { get; set; }
        public string? Text { get; set; }
        internal bool Hover { get; set; }

        [JsonInclude]
        public List<IGoControl> Childrens { get; } = [];

        [JsonConstructor]
        public GoTabPage(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoTabPage() { }
    }
    #endregion
}
