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
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string TabColor { get; set; } = "Base2";
        public string TabBorderColor { get; set; } = "Base3";
        public GoRoundType Round { get; set; } = GoRoundType.All;
        public GoDirection TabPosition { get; set; } = GoDirection.Up;

        public float NavSize { get; set; } = 40;

        [JsonIgnore]
        public GoTabPage? SelectedTab
        {
            get => selTab; 
            set
            {
                if(selTab != value)
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

        [JsonIgnore]
        public override IEnumerable<IGoControl> Childrens => SelectedTab?.Childrens ?? [];

        [JsonInclude]
        public List<GoTabPage> TabPages { get; } = [];
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoTabControl(List<GoTabPage> tabPages) : this() => this.TabPages = tabPages;
        public GoTabControl() { }
        #endregion

        #region Event
        public event EventHandler? TabCnanged;
        public event EventHandler<CancelEventArgs>? TabChanging;
        #endregion
        
        #region Member Variable
        GoTabPage? selTab;
        #endregion

        #region Override
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
            using var p = new SKPaint { IsAntialias = true };
            if (FirstRender && SelectedTab == null) SelectedTab = TabPages.FirstOrDefault();

            Util.DrawBox(canvas, rtPage, cTab, cBorder, GoRoundType.All, thm.Corner);

            tabLoop(rtNav, (i, tab, rt) =>
            {
                var bSel = tab == SelectedTab;
                if (bSel)
                {
                    p.IsStroke = false;
                    p.Color = cTab;
                    switch (TabPosition)
                    {
                        case GoDirection.Up: canvas.DrawRect(Util.FromRect(rt.Left - thm.Corner, rt.Bottom - 1, rt.Width + thm.Corner * 2, 3), p); break;
                        case GoDirection.Down: canvas.DrawRect(Util.FromRect(rt.Left - thm.Corner, rt.Top - 1, rt.Width + thm.Corner * 2, 3), p); break;
                        case GoDirection.Left: canvas.DrawRect(Util.FromRect(rt.Right - 1, rt.Top - thm.Corner, 3, rt.Height + thm.Corner * 2), p); break;
                        case GoDirection.Right: canvas.DrawRect(Util.FromRect(rt.Left - 1, rt.Top - thm.Corner, 3, rt.Height + thm.Corner * 2), p); break;
                    }
                  
                    using var pth = PathTool.Tab(rt, TabPosition, thm.Corner);

                    p.IsStroke = false;
                    p.Color = cTab;
                    canvas.DrawPath(pth, p);

                    p.IsStroke = true;
                    p.Color = cBorder;
                    p.StrokeWidth = 1F;
                    canvas.DrawPath(pth, p);

                }

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(rt);
                    Util.DrawTextIcon(canvas, tab.Text, FontName, FontSize, tab.IconString, IconSize, IconDirection, IconGap, rt, Util.FromArgb(Convert.ToByte(bSel ? 255 : 60), cText));
                }
            });

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipRect(rtPage);
                canvas.Translate(rtPage.Left, rtPage.Top);
                base.OnDraw(canvas);
            }
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtPage = rts["Page"];
            base.OnMouseDown(x - rtPage.Left, y - rtPage.Top, button);
        }
        
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtPage = rts["Page"];
            base.OnMouseUp(x - rtPage.Left, y - rtPage.Top, button);
        }
        
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtPage = rts["Page"];
            base.OnMouseMove(x - rtPage.Left, y - rtPage.Top);
        }

        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtPage = rts["Page"];
            base.OnMouseClick(x - rtPage.Left, y - rtPage.Top, button);

            var rtNav = rts["Nav"];
            tabLoop(rtNav, (i, tab, rt) =>
            {
                if (CollisionTool.Check(rt, x, y)) SelectedTab = tab;
            });
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtPage = rts["Page"];
            base.OnMouseDoubleClick(x - rtPage.Left, y - rtPage.Top, button);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtPage = rts["Page"];
            base.OnMouseLongClick(x - rtPage.Left, y - rtPage.Top, button);
        }

        protected override void OnMouseWheel(float x, float y, float delta)
        {
            var rts = Areas();
            var rtPage = rts["Page"];
            base.OnMouseWheel(x - rtPage.Left, y - rtPage.Top, delta);
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
                    }
                    break;
                case GoDirection.Down:
                    {
                        var rts = Util.Rows(rtContent, ["100%", $"{NavSize}px"]);
                        dic["Nav"] = rts[1];
                        dic["Page"] = rts[0];
                    }
                    break;
                case GoDirection.Left:
                    {
                        var rts = Util.Columns(rtContent, [$"{NavSize}px", "100%"]);
                        dic["Nav"] = rts[0];
                        dic["Page"] = rts[1];
                    }
                    break;
                case GoDirection.Right:
                    {
                        var rts = Util.Columns(rtContent, ["100%", $"{NavSize}px"]);
                        dic["Nav"] = rts[1];
                        dic["Page"] = rts[0];
                    }
                    break;
            }

            return dic;
        }
        #endregion
        #endregion

        #region Method
        void tabLoop(SKRect rtNav, Action<int, GoTabPage, SKRect> loop)
        {
            var n = 15F; //Indent : 10px
            for (int i = 0; i < TabPages.Count; i++)
            {
                var tab = TabPages[i];
                var sz = Util.MeasureTextIcon(tab.Text, FontName, FontSize, tab.IconString, IconSize, IconDirection, IconGap);
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
    }

    #region class : GoTabPage
    public class GoTabPage 
    {
        public string? IconString { get; set; }
        public string? Text { get; set; }

        [JsonInclude]
        public List<IGoControl> Childrens { get; } = [];

        [JsonConstructor]
        public GoTabPage(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoTabPage() { }
    }
    #endregion
}
