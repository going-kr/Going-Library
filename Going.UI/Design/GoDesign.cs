using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Dialogs;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.ImageCanvas;
using Going.UI.Json;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Design
{
    /// <summary>
    /// 디자인 최상위 클래스. 페이지, 윈도우, 사이드바, 타이틀바, 푸터 등 전체 UI 트리를 관리하고 마우스/키보드 이벤트를 처리합니다.
    /// </summary>
    public class GoDesign : IDisposable
    {
        #region Properties
        /// <summary>
        /// 현재 활성화된 디자인 인스턴스를 가져오거나 설정합니다.
        /// </summary>
        public static GoDesign? ActiveDesign { get; set; }

        /// <summary>
        /// 디자인의 이름을 가져오거나 설정합니다.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 디자인의 기본 너비를 가져오거나 설정합니다.
        /// </summary>
        public int DesignWidth { get; set; }
        /// <summary>
        /// 디자인의 기본 높이를 가져오거나 설정합니다.
        /// </summary>
        public int DesignHeight { get; set; }
        /// <summary>
        /// 프로젝트 폴더 경로를 가져오거나 설정합니다.
        /// </summary>
        public string? ProjectFolder { get; set; }

        /// <summary>
        /// 디자인에 포함된 페이지 딕셔너리를 가져옵니다. 키는 페이지 이름입니다.
        /// </summary>
        [JsonInclude] public Dictionary<string, GoPage> Pages { get; private set; } = [];
        /// <summary>
        /// 디자인에 포함된 윈도우 딕셔너리를 가져옵니다. 키는 윈도우 이름입니다.
        /// </summary>
        [JsonInclude] public Dictionary<string, GoWindow> Windows { get; private set; } = [];
        [JsonInclude] private Dictionary<string, List<SKImage>> Images { get; } = new(StringComparer.OrdinalIgnoreCase);
        [JsonInclude] private Dictionary<string, List<byte[]>> Fonts { get; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 타이틀바 컨트롤을 가져옵니다.
        /// </summary>
        [JsonInclude] public GoTitleBar TitleBar { get; private set; } = new() { Visible = false };
        /// <summary>
        /// 왼쪽 사이드바 컨트롤을 가져옵니다.
        /// </summary>
        [JsonInclude] public GoSideBar LeftSideBar { get; private set; } = new() { Visible = false };
        /// <summary>
        /// 오른쪽 사이드바 컨트롤을 가져옵니다.
        /// </summary>
        [JsonInclude] public GoSideBar RightSideBar { get; private set; } = new() { Visible = false };
        /// <summary>
        /// 푸터 컨트롤을 가져옵니다.
        /// </summary>
        [JsonInclude] public GoFooter Footer { get; private set; } = new() { Visible = false };

        [JsonIgnore] internal string Id { get; } = Guid.NewGuid().ToString();
        /// <summary>
        /// 현재 디자인 영역의 실제 너비를 가져옵니다.
        /// </summary>
        [JsonIgnore] public int Width { get; private set; }
        /// <summary>
        /// 현재 디자인 영역의 실제 높이를 가져옵니다.
        /// </summary>
        [JsonIgnore] public int Height { get; private set; }
        /// <summary>
        /// 현재 선택된 컨트롤을 가져옵니다.
        /// </summary>
        [JsonIgnore] public IGoControl? SelectedControl { get; private set; }
        /// <summary>
        /// 현재 표시 중인 페이지를 가져옵니다.
        /// </summary>
        [JsonIgnore] public GoPage? CurrentPage { get; private set; }
        /// <summary>
        /// 현재 마우스 위치를 가져옵니다.
        /// </summary>
        [JsonIgnore] public SKPoint MousePosition { get; private set; }
        /// <summary>
        /// 현재 드래그 중인지 여부를 가져옵니다.
        /// </summary>
        [JsonIgnore] public bool IsDrag => dragItem != null;
        /// <summary>UIEditor 에서 디자인 모드로 사용 중인지 여부를 가져오거나 설정합니다.</summary>
        [JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)] public bool DesignMode { get; set; } = false;

        /// <summary>
        /// 타이틀바 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public bool UseTitleBar { get => TitleBar.Visible; set => TitleBar.Visible = value; }
        /// <summary>
        /// 왼쪽 사이드바 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public bool UseLeftSideBar { get => LeftSideBar.Visible; set => LeftSideBar.Visible = value; }
        /// <summary>
        /// 오른쪽 사이드바 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public bool UseRightSideBar { get => RightSideBar.Visible; set => RightSideBar.Visible = value; }
        /// <summary>
        /// 푸터 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public bool UseFooter { get => Footer.Visible; set => Footer.Visible = value; }
        /// <summary>
        /// 타이틀바, 사이드바, 푸터 등 바 영역의 배경색을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public string BarColor { get; set; } = "Base2";
        /// <summary>
        /// 사이드바를 페이지 위에 오버레이하여 표시할지 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public bool OverlaySideBar { get; set; } = false;
        /// <summary>
        /// 왼쪽 사이드바의 확장 상태를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public bool ExpandLeftSideBar { get => LeftSideBar.Expand; set { if (UseLeftSideBar) LeftSideBar.Expand = value; } }
        /// <summary>
        /// 오른쪽 사이드바의 확장 상태를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public bool ExpandRightSideBar { get => RightSideBar.Expand; set { if (UseRightSideBar) RightSideBar.Expand = value; } }

        /// <summary>
        /// 사용자 정의 테마를 가져오거나 설정합니다. null이면 기본 다크 테마를 사용합니다.
        /// </summary>
        public GoTheme? CustomTheme { get; set; }
        /// <summary>
        /// 현재 적용 중인 테마를 가져옵니다. CustomTheme이 없으면 기본 다크 테마를 반환합니다.
        /// </summary>
        [JsonIgnore] public GoTheme Theme => CustomTheme ?? GoTheme.DarkTheme;

        /// <summary>
        /// 화면 갱신이 필요할 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action? RequestInvalidate;
        #endregion

        #region Member Variable
        private Stack<GoWindow> stkWindow = new Stack<GoWindow>();
        private GoDropDownWindow? dropdownWindow;
        private object? dragItem = null;
        private bool bPageDown = false;
        private bool bFirst = false;
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="GoDesign"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoDesign() => ActiveDesign = this;

        /// <summary>
        /// <see cref="GoDesign"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        [JsonConstructor]
        public GoDesign(Dictionary<string, GoPage> pages, 
                        Dictionary<string, List<SKImage>> images, Dictionary<string, List<byte[]>> fonts,
                        GoTitleBar titleBar, GoSideBar leftSideBar, GoSideBar rightSideBar, GoFooter footer) : this()
        {
            Pages = pages;
            Images = images ?? new(StringComparer.OrdinalIgnoreCase);
            Fonts = fonts ?? new(StringComparer.OrdinalIgnoreCase);

            TitleBar = titleBar;
            LeftSideBar = leftSideBar;
            RightSideBar = rightSideBar;
            Footer = footer;
        }
        #endregion

        #region Method
        #region Select
        /// <summary>
        /// 지정한 컨트롤을 선택합니다. 이미 선택된 컨트롤이 있으면 무시됩니다.
        /// </summary>
        /// <param name="control">선택할 컨트롤</param>
        public void Select(IGoControl? control)
        {
            if (SelectedControl == null) SelectedControl = control;
        }
        #endregion

        #region Page
        /// <summary>
        /// 페이지를 디자인에 추가합니다.
        /// </summary>
        /// <param name="page">추가할 페이지</param>
        public void AddPage(GoPage page) { if (page.Name != null) Pages.TryAdd(page.Name, page); }

        /// <summary>
        /// 지정한 이름의 페이지로 전환합니다.
        /// </summary>
        /// <param name="pageName">전환할 페이지 이름</param>
        public void SetPage(string pageName)
        {
            if (Pages.TryGetValue(pageName, out var page))
            {
                CurrentPage?.FireHide();
                CurrentPage = page;
                CurrentPage.FireShow();
                CurrentPage.FireMouseMove(-1, -1);
            }
        }
        /// <summary>
        /// 지정한 페이지 인스턴스로 전환합니다.
        /// </summary>
        /// <param name="page">전환할 페이지</param>
        public void SetPage(GoPage page) { if (page.Name != null) SetPage(page.Name); }
        #endregion

        #region DropDownWindow
        /// <summary>
        /// 드롭다운 윈도우를 표시합니다.
        /// </summary>
        /// <param name="wnd">표시할 드롭다운 윈도우</param>
        public void ShowDropDownWindow(GoDropDownWindow wnd)
        {
            if (dropdownWindow == null)
            {
                wnd.Design = this;
                dropdownWindow = wnd;
                dropdownWindow.Visible = true;
                dropdownWindow.FireShow();

                CurrentPage?.FireMouseMove(-1, -1);
            }
        }
        /// <summary>
        /// 드롭다운 윈도우를 숨깁니다.
        /// </summary>
        /// <param name="wnd">숨길 드롭다운 윈도우</param>
        public void HideDropDownWindow(GoDropDownWindow wnd)
        {
            if (dropdownWindow == wnd)
            {
                dropdownWindow.Visible = false;
                dropdownWindow.FireHide();
                dropdownWindow = null;
            }
        }
        #endregion

        #region Window
        /// <summary>
        /// 지정한 이름의 윈도우를 표시합니다. 등록된 윈도우 또는 시스템 윈도우에서 검색합니다.
        /// </summary>
        /// <param name="name">표시할 윈도우 이름</param>
        public void ShowWindow(string name)
        {
            if (Windows.TryGetValue(name, out var wnd))
                ShowWindow(wnd);
            else if (GoDialogs.SystemWindows.TryGetValue(name, out var wnd2))
                ShowWindow(wnd2);
        }

        /// <summary>
        /// 지정한 윈도우 인스턴스를 표시합니다.
        /// </summary>
        /// <param name="wnd">표시할 윈도우</param>
        public void ShowWindow(GoWindow wnd)
        {
            wnd.FireInit(this);
            wnd.Visible = true;
            stkWindow.Push(wnd);
            wnd.Bounds = MathTool.MakeRectangle(Util.FromRect(0, 0, Width, Height), new SKSize(wnd.Width, wnd.Height));
            CurrentPage?.FireMouseMove(-1, -1);

            wnd.FireShow();
        }
        /// <summary>
        /// 지정한 윈도우를 숨깁니다.
        /// </summary>
        /// <param name="wnd">숨길 윈도우</param>
        public void HideWindow(GoWindow wnd)
        {
            if (stkWindow.Count > 0 && wnd == stkWindow.Peek())
            {
                var w = stkWindow.Pop();
                w.Visible = false;
                w.FireHide();
            }
        }
        #endregion

        #region SetSize
        /// <summary>
        /// 디자인 영역의 크기를 설정합니다.
        /// </summary>
        /// <param name="width">너비 (픽셀)</param>
        /// <param name="height">높이 (픽셀)</param>
        public void SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
        #endregion

        #region Json
        /// <summary>
        /// 디자인을 JSON 문자열로 직렬화합니다.
        /// </summary>
        /// <returns>직렬화된 JSON 문자열</returns>
        public string JsonSerialize() => JsonSerializer.Serialize(this, GoJsonConverter.Options);
        /// <summary>
        /// JSON 문자열에서 디자인을 역직렬화합니다.
        /// </summary>
        /// <param name="json">역직렬화할 JSON 문자열</param>
        /// <returns>역직렬화된 GoDesign 인스턴스. 실패 시 null을 반환합니다.</returns>
        public static GoDesign? JsonDeserialize(string? json)
        {
            if (json != null)
            {
                try
                {
                    var ds = JsonSerializer.Deserialize<GoDesign>(json, GoJsonConverter.Options);

                    var ch = ds?.TitleBar?.Childrens;

                    return ds;
                }
                catch (Exception ex) { return null; }
            }
            else return null;
        }
        #endregion

        #region Fire
        #region Init
        /// <summary>
        /// 디자인의 모든 구성 요소(페이지, 윈도우, 바)를 초기화합니다.
        /// </summary>
        public void Init()
        {
            foreach (var page in Pages.Values) page.FireInit(this);
            foreach (var wnd in Windows.Values) wnd.FireInit(this);
            foreach (var wnd in GoDialogs.SystemWindows.Values) wnd.FireInit(this);

            TitleBar.FireInit(this);
            LeftSideBar.FireInit(this);
            RightSideBar.FireInit(this);
            Footer.FireInit(this);

            Util.SetExternalFonts(Fonts);
        }

        /// <summary>
        /// 화면 갱신을 요청합니다.
        /// </summary>
        public void Invalidate() => RequestInvalidate?.Invoke();
        #endregion

        #region Draw / Update
        /// <summary>
        /// 디자인 전체를 캔버스에 그립니다. 페이지, 윈도우, 드롭다운 윈도우, 드래그 아이콘 순으로 렌더링합니다.
        /// </summary>
        /// <param name="canvas">그리기 대상 캔버스</param>
        public void Draw(SKCanvas canvas)
        {
            // Drawing : Page > Window > DropDownWindow 순으로 처리
            // 화면은 다 띄워야해서 else 처리 안함
            // MouseEvent : DropDownWindow > Window > Page 순으로 처리

            var thm = Theme;

            #region First Draw
            if (bFirst)
            {
                LeftSideBar.FireShow();
                RightSideBar.FireShow();
                TitleBar.FireShow();
                Footer.FireShow();
                bFirst = false;
            }
            #endregion

            #region Page
            DrawPage(canvas, thm, CurrentPage);
            #endregion

            #region Window
            if (stkWindow.Count > 0)
            {
                var ls = stkWindow.Reverse().ToList();
                var p = new SKPaint { IsAntialias = true };
                p.IsStroke = false;
                p.Color = Util.FromArgb(120, SKColors.Black);
                canvas.DrawRect(Util.FromRect(0, 0, Width, Height), p);

                foreach (var w in ls)
                {
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.Translate(w.Bounds.Left, w.Bounds.Top);
                        w.FireDraw(canvas, thm);
                    }
                }
            }
            #endregion

            #region DropDownWindow
            if (dropdownWindow != null)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Translate(dropdownWindow.Bounds.Left, dropdownWindow.Bounds.Top);
                    dropdownWindow.FireDraw(canvas, thm);
                }
            }
            #endregion

            #region DragDrop
            if (dragItem != null)
            {
                var rt = Util.FromRect(MousePosition.X - 12, MousePosition.Y - 12, 24, 24);
                rt.Offset(5, 10);
                Util.DrawIcon(canvas, "fa-hand-pointer", 24, rt, thm.Fore);
            }
            #endregion
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DrawPage(SKCanvas canvas, GoTheme thm, GoPage? page = null)
        {
            #region bounds
            var (rtL, rtT, rtR, rtB, rtF, rtFR) = bounds();
            LeftSideBar.Bounds = rtL;
            TitleBar.Bounds = rtT;
            RightSideBar.Bounds = rtR;
            Footer.Bounds = rtB;
            if (page != null) page.Bounds = rtFR;
            #endregion

            #region Background
            if (UseTitleBar || UseLeftSideBar || UseRightSideBar || UseFooter)
            {
                canvas.Clear(thm.ToColor(BarColor));

                var vrt = Util.Int(Util.FromRect(rtF.Left, rtF.Top, rtF.Width - 1, rtF.Height - 1)); vrt.Offset(0.5F, 0.5F);
                var rtP = roundPage(vrt, thm.Corner);
                Util.DrawBox(canvas, rtP, thm.Back, thm.Back);
            }
            #endregion

            #region page
            if (page != null)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(rtF);
                    canvas.Translate(page.Bounds.Left, page.Bounds.Top);
                    page.FireDraw(canvas, thm);
                }
            }
            #endregion

            #region TitleBar
            if (UseTitleBar)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(TitleBar.Bounds);
                    canvas.Translate(TitleBar.Bounds.Left, TitleBar.Bounds.Top);
                    TitleBar.FireDraw(canvas, thm);
                }
            }
            #endregion
            #region LeftSideBar
            if (UseLeftSideBar)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(LeftSideBar.Bounds);
                    canvas.Translate(LeftSideBar.Bounds.Left, LeftSideBar.Bounds.Top);
                    LeftSideBar.FireDraw(canvas, thm);
                }
            }
            #endregion
            #region RightSideBar
            if (UseRightSideBar)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(RightSideBar.Bounds);
                    canvas.Translate(RightSideBar.Bounds.Left, RightSideBar.Bounds.Top);
                    RightSideBar.FireDraw(canvas, thm);
                }
            }
            #endregion
            #region Footer
            if (UseFooter)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRect(Footer.Bounds);
                    canvas.Translate(Footer.Bounds.Left, Footer.Bounds.Top);
                    Footer.FireDraw(canvas, thm);
                }
            }
            #endregion
        }

        /// <summary>
        /// 디자인의 모든 구성 요소(바, 페이지, 윈도우, 드롭다운)를 업데이트합니다.
        /// </summary>
        public void Update()
        {
            #region Page
            if (UseTitleBar) TitleBar.FireUpdate();
            if (UseLeftSideBar) LeftSideBar.FireUpdate();
            if (UseRightSideBar) RightSideBar.FireUpdate();
            if (UseFooter) Footer.FireUpdate();
            if (CurrentPage != null) CurrentPage.FireUpdate();
            #endregion

            #region Window
            if (stkWindow.Count > 0)
            {
                var ls = stkWindow.Reverse().ToList();
                foreach (var w in ls) w.FireUpdate();
            }
            #endregion

            #region DropDownWindow
            if (dropdownWindow != null)
                dropdownWindow.FireUpdate();
            #endregion
        }
        #endregion

        #region Mouse
        /// <summary>
        /// 마우스 버튼 누름 이벤트를 처리합니다. 드롭다운 윈도우, 윈도우, 페이지 순으로 전달합니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">눌린 마우스 버튼</param>
        public void MouseDown(float x, float y, GoMouseButton button)
        {
            // MouseEvent : DropDownWindow > Window > Page 순으로 처리
            ActiveDesign = this;

            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseDown(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
                else
                    dropdownWindow.Hide();
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseDown(x - w.Left, y - w.Top, button);
            }
            #endregion
            #region Page
            else
            {
                var (rtL, rtT, rtR, rtB, rtF, rtFR) = bounds();

                if (UseTitleBar) TitleBar.FireMouseDown(x - TitleBar.Left, y - TitleBar.Top, button);
                if (UseLeftSideBar) LeftSideBar.FireMouseDown(x - LeftSideBar.Left, y - LeftSideBar.Top, button);
                if (UseRightSideBar) RightSideBar.FireMouseDown(x - RightSideBar.Left, y - RightSideBar.Top, button);
                if (UseFooter) Footer.FireMouseDown(x - Footer.Left, y - Footer.Top, button);

                if (CollisionTool.Check(rtF, x, y))
                {
                    bPageDown = true;
                    CurrentPage?.FireMouseDown(x - CurrentPage.Left, y - CurrentPage.Top, button);
                }
            }
            #endregion
        }

        /// <summary>
        /// 마우스 버튼 뗌 이벤트를 처리합니다. 드래그 앤 드롭도 여기서 완료됩니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">뗀 마우스 버튼</param>
        public void MouseUp(float x, float y, GoMouseButton button)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y) || dropdownWindow._MouseDown_)
                    dropdownWindow.FireMouseUp(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y) || w._MouseDown_)
                    w.FireMouseUp(x - w.Left, y - w.Top, button);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) TitleBar.FireMouseUp(x - TitleBar.Left, y - TitleBar.Top, button);
                if (UseLeftSideBar) LeftSideBar.FireMouseUp(x - LeftSideBar.Left, y - LeftSideBar.Top, button);
                if (UseRightSideBar) RightSideBar.FireMouseUp(x - RightSideBar.Left, y - RightSideBar.Top, button);
                if (UseFooter) Footer.FireMouseUp(x - Footer.Left, y - Footer.Top, button);

                CurrentPage?.FireMouseUp(x - CurrentPage.Left, y - CurrentPage.Top, button);
            }
            #endregion

            #region Drag
            if (dragItem != null && CurrentPage != null)
            {
                var ls = ControlStack(CurrentPage, x - CurrentPage.Left - CurrentPage.PanelBounds.Left, y - CurrentPage.Top - CurrentPage.PanelBounds.Top);
                if (ls.LastOrDefault() is GoControl c) c.InvokeDragDrop(x, y, dragItem);
                dragItem = null;
            }
            #endregion

            SelectedControl = null;

            bPageDown = false;
        }

        /// <summary>
        /// 마우스 더블 클릭 이벤트를 처리합니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">더블 클릭한 마우스 버튼</param>
        public void MouseDoubleClick(float x, float y, GoMouseButton button)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseDoubleClick(x - dropdownWindow.Left, y - dropdownWindow.Top, button);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseDoubleClick(x - w.Left, y - w.Top, button);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) TitleBar.FireMouseUp(x - TitleBar.Left, y - TitleBar.Top, button);
                if (UseLeftSideBar) LeftSideBar.FireMouseUp(x - LeftSideBar.Left, y - LeftSideBar.Top, button);
                if (UseRightSideBar) RightSideBar.FireMouseUp(x - RightSideBar.Left, y - RightSideBar.Top, button);
                if (UseFooter) Footer.FireMouseUp(x - Footer.Left, y - Footer.Top, button);

                CurrentPage?.FireMouseDoubleClick(x - CurrentPage.Left, y - CurrentPage.Top, button);
            }
            #endregion
        }

        /// <summary>
        /// 마우스 이동 이벤트를 처리합니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        public void MouseMove(float x, float y)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y) || dropdownWindow._MouseDown_)
                    dropdownWindow.FireMouseMove(x - dropdownWindow.Left, y - dropdownWindow.Top);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y) || w._MouseDown_)
                    w.FireMouseMove(x - w.Left, y - w.Top);
            }
            #endregion
            #region Page
            else
            {
                var (rtL, rtT, rtR, rtB, rtF, rtFR) = bounds();

                if (UseTitleBar) TitleBar.FireMouseMove(x - TitleBar.Left, y - TitleBar.Top);
                if (UseLeftSideBar) LeftSideBar.FireMouseMove(x - LeftSideBar.Left, y - LeftSideBar.Top);
                if (UseRightSideBar) RightSideBar.FireMouseMove(x - RightSideBar.Left, y - RightSideBar.Top);
                if (UseFooter) Footer.FireMouseMove(x - Footer.Left, y - Footer.Top);

                if (bPageDown || CollisionTool.Check(rtF, x, y))
                    CurrentPage?.FireMouseMove(x - CurrentPage.Left, y - CurrentPage.Top);
                else
                    CurrentPage?.FireMouseMove(-1, -1);
            }
            #endregion

            MousePosition = new SKPoint(x, y);
        }

        /// <summary>
        /// 마우스 휠 이벤트를 처리합니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="delta">휠 스크롤량</param>
        public void MouseWheel(float x, float y, float delta)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                if (CollisionTool.Check(dropdownWindow.Bounds, x, y))
                    dropdownWindow.FireMouseWheel(x - dropdownWindow.Left, y - dropdownWindow.Top, delta);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                var w = stkWindow.Peek();
                if (CollisionTool.Check(w.Bounds, x, y))
                    w.FireMouseWheel(x - w.Left, y - w.Top, delta);
            }
            #endregion
            #region Page
            else
            {
                var (rtL, rtT, rtR, rtB, rtF, rtFR) = bounds();

                if (UseTitleBar) TitleBar.FireMouseWheel(x - TitleBar.Left, y - TitleBar.Top, delta);
                if (UseLeftSideBar) LeftSideBar.FireMouseWheel(x - LeftSideBar.Left, y - LeftSideBar.Top, delta);
                if (UseRightSideBar) RightSideBar.FireMouseWheel(x - RightSideBar.Left, y - RightSideBar.Top, delta);
                if (UseFooter) Footer.FireMouseWheel(x - Footer.Left, y - Footer.Top, delta);

                if (CollisionTool.Check(rtF, x, y))
                    CurrentPage?.FireMouseWheel(x - CurrentPage.Left, y - CurrentPage.Top, delta);
            }
            #endregion
        }
        #endregion

        #region Key
        /// <summary>
        /// 키보드 키 누름 이벤트를 처리합니다.
        /// </summary>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">눌린 키</param>
        public void KeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                GUI.KeyDown(dropdownWindow, Shift, Control, Alt, key);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                GUI.KeyDown(stkWindow.Peek(), Shift, Control, Alt, key);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) GUI.KeyDown(TitleBar, Shift, Control, Alt, key);
                if (UseLeftSideBar) GUI.KeyDown(LeftSideBar, Shift, Control, Alt, key);
                if (UseRightSideBar) GUI.KeyDown(RightSideBar, Shift, Control, Alt, key);
                if (UseFooter) GUI.KeyDown(Footer, Shift, Control, Alt, key);

                if (CurrentPage != null) GUI.KeyDown(CurrentPage, Shift, Control, Alt, key);
            }
            #endregion
        }

        /// <summary>
        /// 키보드 키 뗌 이벤트를 처리합니다.
        /// </summary>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">뗀 키</param>
        public void KeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            #region DropDownWindow
            if (dropdownWindow != null)
            {
                GUI.KeyUp(dropdownWindow, Shift, Control, Alt, key);
            }
            #endregion
            #region Window
            else if (stkWindow.Count > 0)
            {
                GUI.KeyUp(stkWindow.Peek(), Shift, Control, Alt, key);
            }
            #endregion
            #region Page
            else
            {
                if (UseTitleBar) GUI.KeyUp(TitleBar, Shift, Control, Alt, key);
                if (UseLeftSideBar) GUI.KeyUp(LeftSideBar, Shift, Control, Alt, key);
                if (UseRightSideBar) GUI.KeyUp(RightSideBar, Shift, Control, Alt, key);
                if (UseFooter) GUI.KeyUp(Footer, Shift, Control, Alt, key);

                if (CurrentPage != null) GUI.KeyUp(CurrentPage, Shift, Control, Alt, key);
            }
            #endregion
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var page in Pages.Values) page.Dispose();
            foreach (var wnd in Windows.Values) wnd.Dispose();
            foreach (var wnd in GoDialogs.SystemWindows.Values) wnd.Dispose();

            TitleBar.Dispose();
            LeftSideBar.Dispose();
            RightSideBar.Dispose();
            Footer.Dispose();

            Util.UnloadExternalFonts();

            foreach (var imgs in Images.Values)
                foreach (var img in imgs)
                    img.Dispose();
        }
        #endregion
        #endregion

        #region Image
        /// <summary>
        /// 바이트 배열로부터 이미지를 추가합니다.
        /// </summary>
        /// <param name="name">이미지 이름</param>
        /// <param name="data">이미지 바이트 데이터</param>
        public void AddImage(string name, byte[] data)
            => Images[name] = ImageExtractor.ProcessImageFromMemory(data);

        /// <summary>
        /// SKImage 목록으로 이미지를 추가합니다. 동일한 이름이 이미 존재하면 추가하지 않습니다.
        /// </summary>
        /// <param name="name">이미지 이름</param>
        /// <param name="imgs">SKImage 목록</param>
        public void AddImage(string name, List<SKImage> imgs)
        {
            var nm = name.ToLower();
            if (!Images.ContainsKey(nm))
            {
                if (imgs != null && imgs.Count > 0)
                    Images.Add(nm, imgs);
            }
        }

        /// <summary>
        /// 지정한 디렉터리의 모든 이미지 파일을 로드하여 추가합니다.
        /// </summary>
        /// <param name="directory">이미지 파일이 있는 디렉터리 경로</param>
        public void AddImageFolder(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (var path in Directory.GetFiles(directory))
                {
                    var data = File.ReadAllBytes(path);
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (Images.ContainsKey(name))
                    {
                        var ls = Images.Keys.Where(x => x.StartsWith(name));
                        var ls2 = ls.Where(x => x.Split('_').Length == 2);
                        var max = ls2.Count() > 0 ? ls2.Max(x => int.TryParse(x.Split('_')[1], out var n) ? n : 0) : 0;
                        AddImage($"{name}_{max + 1}", data);
                    }
                    else AddImage(name, data);
                }
            }
        }

        /// <summary>
        /// 지정한 이름의 이미지를 제거합니다.
        /// </summary>
        /// <param name="name">제거할 이미지 이름</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveImage(string name) => Images.Remove(name);

        /// <summary>
        /// 등록된 모든 이미지 목록을 가져옵니다.
        /// </summary>
        /// <returns>이미지 이름과 SKImage 목록의 튜플 리스트</returns>
        public List<(string name, List<SKImage> images)> GetImages() => [.. Images.Select(x => (x.Key, x.Value))];
        /// <summary>
        /// 지정한 이름의 이미지 목록을 가져옵니다.
        /// </summary>
        /// <param name="name">이미지 이름</param>
        /// <returns>해당 이름의 SKImage 목록. 없으면 빈 목록을 반환합니다.</returns>
        public List<SKImage> GetImage(string? name)
        {
            var k = Images.Keys.FirstOrDefault(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            return k != null && Images.TryGetValue(k, out var ls) ? ls : [];
        }
        #endregion

        #region Font
        /// <summary>
        /// 폰트 데이터를 추가합니다. 동일한 이름의 폰트가 있으면 목록에 추가합니다.
        /// </summary>
        /// <param name="name">폰트 이름</param>
        /// <param name="data">폰트 바이트 데이터</param>
        public void AddFont(string name, byte[] data)
        {
            if (Fonts.TryGetValue(name, out var imgs)) imgs.Add(data);
            else Fonts.Add(name, [data]);
        }

        /// <summary>
        /// 지정한 디렉터리의 모든 폰트 파일(ttf, otf)을 로드하여 추가합니다.
        /// </summary>
        /// <param name="directory">폰트 파일이 있는 디렉터리 경로</param>
        public void AddFontFolder(string directory)
        {
            if (Directory.Exists(directory))
            {
                var paths = new List<string>();
                paths.AddRange(Directory.GetFiles(directory, "*.ttf"));
                paths.AddRange(Directory.GetFiles(directory, "*.otf"));

                foreach (var path in paths)
                {
                    var data = File.ReadAllBytes(path);
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (Fonts.ContainsKey(name))
                    {
                        var ls = Fonts.Keys.Where(x => x.StartsWith(name));
                        var ls2 = ls.Where(x => x.Split('_').Length == 2);
                        var max = ls2.Count() > 0 ? ls2.Max(x => int.TryParse(x.Split('_')[1], out var n) ? n : 0) : 0;
                        AddFont($"{name}_{max + 1}", data);
                    }
                    else AddFont(name, data);
                }
            }
        }

        /// <summary>
        /// 지정한 이름의 폰트를 제거합니다.
        /// </summary>
        /// <param name="name">제거할 폰트 이름</param>
        public void RemoveFont(string name) => Fonts.Remove(name);

        /// <summary>
        /// 등록된 모든 폰트 목록을 가져옵니다.
        /// </summary>
        /// <returns>폰트 이름과 바이트 배열 목록의 튜플 리스트</returns>
        public List<(string name, List<byte[]> fonts)> GetFonts() => [.. Fonts.Select(x => (x.Key, x.Value))];
        /// <summary>
        /// 지정한 이름의 폰트 데이터를 가져옵니다.
        /// </summary>
        /// <param name="name">폰트 이름</param>
        /// <returns>해당 폰트의 바이트 배열 목록. 없으면 빈 목록을 반환합니다.</returns>
        public List<byte[]> GetFont(string? name) => name != null && Fonts.TryGetValue(name, out var ls) ? ls : [];
        #endregion

        #region Drag
        /// <summary>
        /// 드래그 작업을 시작합니다.
        /// </summary>
        /// <param name="item">드래그할 항목</param>
        public void Drag(object? item) => dragItem = item;
        /// <summary>
        /// 현재 드래그 중인 항목을 가져옵니다.
        /// </summary>
        /// <returns>드래그 중인 항목. 드래그 중이 아니면 null을 반환합니다.</returns>
        public object? GetDragItem() => dragItem;
        #endregion

        #region ControlStack
        /// <summary>
        /// 지정한 좌표에 해당하는 컨트롤 스택을 계층적으로 수집합니다.
        /// </summary>
        /// <param name="container">탐색 시작 컨테이너</param>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>좌표에 해당하는 컨트롤 목록 (상위에서 하위 순)</returns>
        public List<IGoControl> ControlStack(IGoContainer container, float x, float y)
        {
            List<IGoControl> ret = [];

            if (container != null)
            {
                if (container is IGoControl vc) //&& CollisionTool.Check(Util.FromRect(0, 0, vc.Width, vc.Height), x, y))
                {
                    ret.Add(vc);
                    foreach (var c in container.Childrens)
                        if (CollisionTool.Check(c.Bounds, x, y))
                        {
                            if (c is IGoContainer con)
                            {
                                ret.AddRange(ControlStack(con, x - c.Left - con.PanelBounds.Left + con.ViewPosition.X,
                                                               y - c.Top - con.PanelBounds.Top + con.ViewPosition.Y));
                            }
                            else
                                ret.Add(c);
                        }
                }
            }

            return ret;
        }

        /// <summary>
        /// 지정한 영역과 교차하는 컨트롤 스택을 계층적으로 수집합니다.
        /// </summary>
        /// <param name="container">탐색 시작 컨테이너</param>
        /// <param name="rt">교차 검사할 영역</param>
        /// <returns>영역과 교차하는 컨트롤 목록 (상위에서 하위 순)</returns>
        public List<IGoControl> ControlStack(IGoContainer container, SKRect rt)
        {
            List<IGoControl> ret = [];

            if (container != null)
            {
                if (container is IGoControl vc && CollisionTool.Check(Util.FromRect(0, 0, vc.Width, vc.Height), rt))
                {
                    ret.Add(vc);
                    foreach (var c in container.Childrens)
                        if (CollisionTool.Check(c.Bounds, rt))
                        {
                            if (c is IGoContainer con)
                            {
                                var vrt = rt;
                                vrt.Offset(-c.Left - con.PanelBounds.Left + con.ViewPosition.X, -c.Top - con.PanelBounds.Top + con.ViewPosition.Y);
                                ret.AddRange(ControlStack(con, vrt));
                            }
                            else
                                ret.Add(c);
                        }
                }
            }

            return ret;
        }
        #endregion

        #region bounds
        (SKRect rtL, SKRect rtT, SKRect rtR, SKRect rtB, SKRect rtF, SKRect rtFR) bounds()
        {
            var hT = UseTitleBar ? TitleBar.BarSize : 0;
            var hB = UseFooter ? Footer.BarSize : 0;
            var wL = UseLeftSideBar ? LeftSideBar.RealBarSize : 0;
            var wR = UseRightSideBar ? RightSideBar.RealBarSize : 0;

            var rtT = Util.FromRect(0, 0, Width, hT);
            var rtB = Util.FromRect(0, Height - hB, Width, hB);
            var rtL = Util.FromRect(0, rtT.Bottom, wL, Height - hB - hT);
            var rtR = Util.FromRect(Width - wR, rtT.Bottom, wR, Height - hB - hT);
            var rtF = new SKRect(rtL.Right, rtT.Bottom, rtR.Left, rtB.Top);
            var rtFR = OverlaySideBar ? new SKRect(0, rtT.Bottom, Width, rtB.Top) : rtF;
            return (rtL, rtT, rtR, rtB, rtF, rtFR);
        }
        #endregion

        #region roundPage
        SKRoundRect roundPage(SKRect rtF, int corner)
        {
            var ret = new SKRoundRect(rtF, corner);

            #region var
            var ut = UseTitleBar;
            var ul = UseLeftSideBar;
            var ur = UseRightSideBar;
            var ub = UseFooter;
            #endregion

            ret.SetNinePatch(rtF, ul ? corner : 0, ut ? corner : 0, ur ? corner : 0, ub ? corner : 0);

            return ret;
        }
        #endregion

        #region LayoutBounds
        /// <summary>UIEditor 에서 사용할 레이아웃 경계 영역을 반환합니다.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public (SKRect rtL, SKRect rtT, SKRect rtR, SKRect rtB, SKRect rtF, SKRect rtFR) LayoutBounds() => bounds();
        #endregion
        #endregion
    }

}
