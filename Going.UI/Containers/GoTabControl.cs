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
    /// <summary>
    /// 여러 탭 페이지를 전환하며 표시하는 탭 컨트롤입니다. 탭 위치를 상/하/좌/우로 설정할 수 있습니다.
    /// </summary>
    public class GoTabControl : GoContainer
    {
        #region Properties
        /// <summary>
        /// 탭 아이콘 배치 방향을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public GoDirectionHV IconDirection { get; set; }
        /// <summary>
        /// 아이콘 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        /// <summary>
        /// 아이콘과 텍스트 사이의 간격을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public float IconGap { get; set; } = 5;

        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 3)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public float FontSize { get; set; } = 12;

        /// <summary>
        /// 텍스트 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 탭 배경 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public string TabColor { get; set; } = "Base2";
        /// <summary>
        /// 탭 테두리 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string TabBorderColor { get; set; } = "Base3";
        /// <summary>
        /// 탭 네비게이션의 위치를 가져오거나 설정합니다. (Up, Down, Left, Right)
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public GoDirection TabPosition { get; set; } = GoDirection.Up;

        /// <summary>
        /// 탭 네비게이션 영역의 크기(너비 또는 높이)를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public float NavSize { get; set; } = 40;

        /// <summary>
        /// 현재 선택된 탭 페이지를 가져오거나 설정합니다. 변경 시 <see cref="TabChanging"/> 및 <see cref="TabChanged"/> 이벤트가 발생합니다.
        /// </summary>
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
                        if (selTab != null) foreach (var c in selTab.Childrens) c.FireHide();
                        selTab = value;
                        if (selTab != null) foreach (var c in selTab.Childrens) c.FireShow();
                        TabChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 선택된 탭 페이지의 자식 컨트롤 컬렉션을 가져옵니다. 선택된 탭이 없으면 빈 컬렉션을 반환합니다.
        /// </summary>
        [JsonIgnore] public override IEnumerable<IGoControl> Childrens => SelectedTab?.Childrens ?? [];

        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다.
        /// </summary>
        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];

        //[GoProperty(PCategory.Misc, 11),JsonInclude] public List<GoTabPage> TabPages { get; } = [];
        /// <summary>
        /// 탭 페이지 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public List<GoTabPage> TabPages { get; set; } = [];
        #endregion

        #region Event
        /// <summary>
        /// 탭이 변경된 후 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler? TabChanged;
        /// <summary>
        /// 탭이 변경되기 전에 발생하는 이벤트입니다. Cancel을 true로 설정하면 변경을 취소할 수 있습니다.
        /// </summary>
        public event EventHandler<CancelEventArgs>? TabChanging;
        #endregion

        #region Member Variable
        GoTabPage? selTab;
        Dictionary<GoTabPage, SKPath> paths = [];
        #endregion

        #region Constructor
        //[JsonConstructor]
        //public GoTabControl(List<GoTabPage> tabPages) : this() => this.TabPages = tabPages;
        /// <summary>
        /// <see cref="GoTabControl"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoTabControl() { }
        #endregion

        #region Override
        #region Init
        /// <inheritdoc/>
        protected override void OnInit(GoDesign? design)
        {
            base.OnInit(design);

            foreach (var tab in TabPages)
                foreach (var c in tab.Childrens)
                {
                    c.FireInit(design);
                    if (c is GoControl c2) c2.Parent = this;
                }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
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

                    if (!paths.ContainsKey(tab)) paths[tab] = new SKPath();
                    var pth = paths[tab];
                    PathTool.Tab(pth, rt, TabPosition, thm.Corner);

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

                                    var n = Convert.ToInt32(rtPage.Left) + 0.5F;
                                    p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                    canvas.DrawLine(n, rt.Bottom - c2, n, rt.Bottom + c2, p);
                                }
                                break;
                            case GoDirection.Down:
                                {
                                    p.IsStroke = false; p.Color = cTab;
                                    canvas.DrawRect(Util.FromRect(rtPage.Left, rt.Top - c2, c2, c2), p);

                                    var n = Convert.ToInt32(rtPage.Left) + 0.5F;
                                    p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                    canvas.DrawLine(n, rt.Top + c2, n, rt.Top - c2, p);
                                }
                                break;
                            case GoDirection.Left:
                                {
                                    p.IsStroke = false; p.Color = cTab;
                                    canvas.DrawRect(Util.FromRect(rt.Right, rtPage.Top, c2, c2), p);

                                    var n = Convert.ToInt32(rtPage.Top) + 0.5F;
                                    p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                    canvas.DrawLine(rt.Right - c2, n, rt.Right + c2, n, p);
                                }
                                break;
                            case GoDirection.Right:
                                {
                                    p.IsStroke = false; p.Color = cTab;
                                    canvas.DrawRect(Util.FromRect(rt.Left - c2, rtPage.Top, c2, c2), p);

                                    var n = Convert.ToInt32(rtPage.Top) + 0.5F;
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

            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];

            if (SelectedTab != null)
            {
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

            //base.OnLayout();
        }

        /// <inheritdoc/>
        protected override void OnDispose()
        {
            //base.OnDispose();

            foreach (var p in TabPages)
                foreach (var c in p.Childrens)
                    c.Dispose();

            foreach (var v in paths.Values) v.Dispose();

        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);

            var rts = Areas();
            var rtNav = rts["Nav"];
            tabLoop(rtNav, (i, tab, rt) =>
            {
                tab.Hover = CollisionTool.Check(rt, x, y);
            });
        }

        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            if (!(Design?.DesignMode ?? false)) base.OnMouseClick(x, y, button);

            var rts = Areas();
            var rtNav = rts["Nav"];
            tabLoop(rtNav, (i, tab, rt) =>
            {
                if (CollisionTool.Check(rt, x, y)) SelectedTab = tab;
            });
        }
        #endregion

        #region Areas
        /// <inheritdoc/>
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
                        dic["Panel"] = Util.FromRect(rts[1], new GoPadding(0));
                    }
                    break;
                case GoDirection.Down:
                    {
                        var rts = Util.Rows(rtContent, ["100%", $"{NavSize}px"]);
                        dic["Nav"] = rts[1];
                        dic["Page"] = rts[0];
                        dic["Panel"] = Util.FromRect(rts[0], new GoPadding(0));
                    }
                    break;
                case GoDirection.Left:
                    {
                        var rts = Util.Columns(rtContent, [$"{NavSize}px", "100%"]);
                        dic["Nav"] = rts[0];
                        dic["Page"] = rts[1];
                        dic["Panel"] = Util.FromRect(rts[1], new GoPadding(0));
                    }
                    break;
                case GoDirection.Right:
                    {
                        var rts = Util.Columns(rtContent, ["100%", $"{NavSize}px"]);
                        dic["Nav"] = rts[1];
                        dic["Page"] = rts[0];
                        dic["Panel"] = Util.FromRect(rts[0], new GoPadding(0));
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
        /// <summary>
        /// 이름으로 탭을 선택합니다.
        /// </summary>
        /// <param name="name">선택할 탭의 이름</param>
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
    /// <summary>
    /// GoTabControl에서 사용되는 탭 페이지 클래스입니다. 이름, 아이콘, 텍스트 및 자식 컨트롤을 포함합니다.
    /// </summary>
    public class GoTabPage
    {
        /// <summary>
        /// 탭 페이지의 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 0)] public string Name { get; set; }
        /// <summary>
        /// 탭에 표시할 아이콘 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 1)] public string? IconString { get; set; }
        /// <summary>
        /// 탭에 표시할 텍스트를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 2)] public string? Text { get; set; }
        internal bool Hover { get; set; }

        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [JsonInclude] public List<IGoControl> Childrens { get; } = [];

        /// <summary>
        /// 자식 컨트롤 목록을 사용하여 <see cref="GoTabPage"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">자식 컨트롤 목록</param>
        [JsonConstructor]
        public GoTabPage(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoTabPage"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoTabPage() { }

        /// <inheritdoc/>
        public override string ToString() => Name;
    }
    #endregion
}
