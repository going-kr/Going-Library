using Going.UI.Collections;
using Going.UI.Datas;
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
using System.Threading.Tasks.Sources;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Controls
{
    /// <summary>
    /// 리스트 박스 컨트롤. 항목 목록을 표시하고 선택 기능을 제공합니다.
    /// </summary>
    public class GoListBox : GoControl
    {
        #region Properties
        /// <summary>
        /// 아이콘 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public float IconSize { get; set; } = 12;
        /// <summary>
        /// 아이콘과 텍스트 사이의 간격을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public float IconGap { get; set; } = 5;
        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 2)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public GoFontStyle FontStyle { get; set; }= GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public float FontSize { get; set; } = 12;

        /// <summary>
        /// 텍스트 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 배경 상자 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public string BoxColor { get; set; } = "Base1";
        /// <summary>
        /// 테두리 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public string BorderColor { get; set; } = "Base3";
        /// <summary>
        /// 선택 항목 배경 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string SelectColor { get; set; } = "Select";
        /// <summary>
        /// 모서리 둥글기 타입을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public GoRoundType Round { get; set; } = GoRoundType.All;
        /// <summary>
        /// 배경을 그릴지 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public bool BackgroundDraw { get; set; } = true;
        /// <summary>
        /// 각 항목의 높이(픽셀)를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public float ItemHeight { get; set; } = 30;
        /// <summary>
        /// 항목 내 콘텐츠 정렬 방식을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public GoContentAlignment ItemAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        /// <summary>
        /// 항목 선택 모드를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public GoItemSelectionMode SelectionMode { get; set; } = GoItemSelectionMode.Single;

        /// <summary>
        /// 리스트에 표시할 항목 컬렉션을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 14)] public ObservableList<GoListItem> Items { get; set; } = [];
        /// <summary>
        /// 현재 선택된 항목 목록을 가져옵니다.
        /// </summary>
        [JsonIgnore] public List<GoListItem> SelectedItems { get; } = [];
        /// <summary>
        /// 스크롤 위치를 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore] public double ScrollPosition { get => scroll.ScrollPosition; set => scroll.ScrollPosition = value; }
        [JsonIgnore] internal double ScrollPositionWithOffset => scroll.ScrollPositionWithOffset;
        #endregion

        #region Member Variable
        private Scroll scroll = new Scroll() { Direction = ScrollDirection.Vertical };

        private bool bShift, bControl;
        private GoListItem? first = null;
        private SKRect rtBoxP = new SKRect();

        private SKPath path = new SKPath();
        #endregion

        #region Event
        /// <summary>
        /// 선택된 항목이 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? SelectedChanged;
        /// <summary>
        /// 항목이 클릭되었을 때 발생합니다.
        /// </summary>
        public event EventHandler<ListItemEventArgs>? ItemClicked;
        /// <summary>
        /// 항목이 길게 클릭되었을 때 발생합니다.
        /// </summary>
        public event EventHandler<ListItemEventArgs>? ItemLongClicked;
        /// <summary>
        /// 항목이 더블 클릭되었을 때 발생합니다.
        /// </summary>
        public event EventHandler<ListItemEventArgs>? ItemDoubleClicked;
        #endregion

        #region Constructor
        /// <summary>GoListBox 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoListBox()
        {
            Selectable = true;

            scroll.GetScrollTotal = () => ItemHeight * Items.Count;
            scroll.GetScrollTick = () => ItemHeight; // 휠 틱(한번에 이동하는 양)
            scroll.GetScrollView = () => Height;     // 스크롤 뷰(보이는 화면을 얼마나 보여주는지)
            scroll.Refresh = () => Invalidate();
        }
        #endregion

        #region Override
        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            #region var
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var cBox = thm.ToColor(BoxColor);
            var cSel = thm.ToColor(SelectColor);

            var rts = Areas();
            var rtContent = rts["Content"];
            var rtBox = rts["Box"];
            var rtScroll = rts["Scroll"];

            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);   // 스크롤 위치 + offset : 마우스 누르고 이동한 offset을 같이 가져옴
            #endregion

            #region Background
            if (BackgroundDraw)
            {
                var rnds = Util.Rounds(GoDirectionHV.Horizon, Round, 2);
                Util.DrawBox(canvas, rtContent, cBox, cBox, Round, thm.Corner);
            }
            #endregion

            using (new SKAutoCanvasRestore(canvas))
            {
                PathTool.Box(path, rtContent, Round, thm.Corner);
                canvas.ClipPath(path, SKClipOperation.Intersect, true);
                canvas.Translate(rtContent.Left, spos + rtContent.Top); // 스크롤 위치만큼 이동(spos가 필요한 이유) - 여기서부터 위치변경 후 그리기 시작
                itemLoop((i, item) =>
                {

                    if (SelectedItems.Contains(item))
                        Util.DrawBox(canvas, item.Bounds, cSel, GoRoundType.Rect, thm.Corner);  // 선택여부(초록색)

                    var ih = (ItemHeight - FontSize) / 2;
                    var rt = Util.FromRect(item.Bounds); rt.Inflate(-ih, 0);
                    Util.DrawTextIcon(canvas, item.Text, FontName, FontStyle, FontSize, item.IconString, IconSize, GoDirectionHV.Horizon, IconGap, rt, cText, ItemAlignment);
                });
            }

            scroll.Draw(canvas, thm, rtScroll);

            #region Border
            if (BackgroundDraw)
            {
                var rnds = Util.Rounds(GoDirectionHV.Horizon, Round, 2);
                Util.DrawBox(canvas, rtContent, SKColors.Transparent, cBorder, Round, thm.Corner);
            }
            #endregion


            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            if (CollisionTool.Check(rts["Content"], x, y))
            {
                scroll.MouseDown(x, y, rts["Scroll"]);
                if (Scroll.TouchMode && CollisionTool.Check(rts["Box"], x, y)) scroll.TouchDown(x, y);
            }
            base.OnMouseDown(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            scroll.MouseMove(x, y, rts["Scroll"]);
            if (Scroll.TouchMode) scroll.TouchMove(x, y);
            base.OnMouseMove(x, y);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            scroll.MouseUp(x, y);
            if (Scroll.TouchMode) scroll.TouchUp(x, y);
            base.OnMouseUp(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseWheel(float x, float y, float delta)
        {
            var rts = Areas();
            if (CollisionTool.Check(rts["Content"], x, y))
            {
                scroll.MouseWheel(x, y,delta);
            }
            base.OnMouseWheel(x, y, delta);
        }

        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtBox.Top - spos;

            itemLoop((i, item) =>
            {
                if (CollisionTool.Check(item.Bounds, x, ry))
                {
                    select(item);
                    ItemClicked?.Invoke(this, new ListItemEventArgs(item));
                }
            });

            base.OnMouseClick(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtBox.Top - spos;

            itemLoop((i, item) =>
            {
                if (CollisionTool.Check(item.Bounds, x, ry))
                    ItemLongClicked?.Invoke(this, new ListItemEventArgs(item));
            });

            base.OnMouseLongClick(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtBox.Top - spos;

            itemLoop((i, item) =>
            {
                if (CollisionTool.Check(item.Bounds, x, ry))
                    ItemDoubleClicked?.Invoke(this, new ListItemEventArgs(item));
            });

            base.OnMouseDoubleClick(x, y, button);
        }
        #endregion

        #region Key
        /// <inheritdoc/>
        protected override void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            bShift = Shift;
            bControl = Control;
            base.OnKeyDown(Shift, Control, Alt, key);
        }

        /// <inheritdoc/>
        protected override void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            bShift = Shift;
            bControl = Control;
            base.OnKeyUp(Shift, Control, Alt, key);
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            path.Dispose();
            base.OnDispose();
        }
        #endregion

        #region Areas
        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];
            var rtBox = Util.FromRect(rtContent.Left, rtContent.Top, rtContent.Width - Scroll.SC_WH, rtContent.Height);
            var rtScroll = Util.FromRect(rtBox.Right, rtBox.Top, Scroll.SC_WH, rtBox.Height);

            dic["Box"] = rtBox;
            dic["Scroll"] = rtScroll;

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region itemLoop
        void itemLoop(Action<int, GoListItem> loop)
        {
            var rts = Areas();
            var rtBox = rts["Box"];

            #region calcbox
            if (Items.Changed || !rtBoxP.Equals(rtBox))
            {
                rtBoxP = rtBox;
                var y = 0F;
                foreach (var item in Items) { item.Bounds = Util.FromRect(0, y, rtBox.Width, ItemHeight); y += ItemHeight; }
                Items.Changed = false;
            }
            #endregion

            rtBox.Offset(0, -Convert.ToSingle(scroll.ScrollPositionWithOffset));       // 시작 인덱스(1000만개가 있다고 해도, 30개만 보여주면 되니까, 속도도 빠름)
            var (si, ei) = Util.FindRect(Items.Select(x => x.Bounds).ToList(), rtBox); // 끝 인덱스

            if (si >= 0 && si < Items.Count && ei >= 0 && ei < Items.Count)
                for (int i = si; i <= ei; i++)
                    loop(i, Items[i]);
        }
        #endregion

        #region select
        private void select(GoListItem item)
        {
            #region Single
            if (SelectionMode == GoItemSelectionMode.Single)
            {
                SelectedItems.Clear();
                SelectedItems.Add(item);
                SelectedChanged?.Invoke(this, EventArgs.Empty);
                first = item;
            }
            #endregion
            #region Multi
            else if (SelectionMode == GoItemSelectionMode.Multi)
            {
                if (SelectedItems.Contains(item))
                {
                    SelectedItems.Remove(item);
                    if (SelectedChanged != null) SelectedChanged.Invoke(this, new EventArgs());
                }
                else
                {
                    SelectedItems.Add(item);
                    if (SelectedChanged != null) SelectedChanged.Invoke(this, new EventArgs());
                }
            }
            #endregion
            #region MultiPC
            // Ctrl, Shift 눌렀을 때 기능들(Selecting mode를 MultiPC로)
            else if (SelectionMode == GoItemSelectionMode.MultiPC)
            {
                if (bControl)
                {
                    #region Control
                    // Ctrl 누르다 Shift가 눌렸을 때 고려해서 first를 넣은 것
                    if (SelectedItems.Contains(item))
                    {
                        SelectedItems.Remove(item);
                        SelectedChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        SelectedItems.Add(item);
                        SelectedChanged?.Invoke(this, EventArgs.Empty);
                        first = item;
                    }
                    #endregion
                }
                else if (bShift)
                {
                    #region Shift
                    if (first == null)
                    {
                        SelectedItems.Add(item);
                        SelectedChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        int idx1 = Items.IndexOf(first);
                        int idx2 = Items.IndexOf(item);
                        int min = Math.Min(idx1, idx2);
                        int max = Math.Max(idx1, idx2);

                        bool b = false;
                        for (int ii = min; ii <= max; ii++)
                        {
                            if (!SelectedItems.Contains(Items[ii]))
                            {
                                SelectedItems.Add(Items[ii]);
                                b = true;
                            }
                        }
                        if (b) SelectedChanged?.Invoke(this, EventArgs.Empty);
                    }
                    #endregion
                }
                else
                {
                    #region Select
                    SelectedItems.Clear();
                    SelectedItems.Add(item);
                    SelectedChanged?.Invoke(this, EventArgs.Empty);
                    first = item;
                    #endregion
                }
            }
            #endregion
        }
        #endregion
        #endregion
    }
}
