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
    public class GoListBox : GoControl
    {
        #region Properties
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; } = GoDirectionHV.Horizon;
        public float IconGap { get; set; } = 5;
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string BoxColor { get; set; } = "Base1";
        public string BorderColor { get; set; } = "Base3";
        public string SelectedColor { get; set; } = "Select";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool BackgroundDraw { get; set; } = true;

        public float ItemHeight { get; set; } = 30;
        public GoContentAlignment ItemAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        public List<GoListItem> Items { get; set; } = [];
        [JsonIgnore] public List<GoListItem> SelectedItems { get; } = new List<GoListItem>();
        public GoItemSelectionMode SelectionMode { get; set; } = GoItemSelectionMode.SIngle;

        [JsonIgnore] public double ScrollPosition { get => scroll.ScrollPosition; set => scroll.ScrollPosition = value; }
        #endregion

        #region Member Variable
        private Scroll scroll = new Scroll() { Direction = ScrollDirection.Vertical };

        bool bShift, bControl, bAlt;
        GoListItem? first = null;
        #endregion

        #region Event 
        public event EventHandler? SelectedChanged;
        public event EventHandler<ListItemClickEventArgs>? ItemClicked;
        public event EventHandler<ListItemClickEventArgs>? ItemLongClicked;
        public event EventHandler<ListItemClickEventArgs>? ItemDoubleClicked;
        #endregion

        #region Constructor
        public GoListBox()
        {
            Selectable = true;

            scroll.GetScrollTotal = () => ItemHeight * Items.Count;
            scroll.GetScrollTick = () => ItemHeight; // 휠 틱(한번에 이동하는 양)
            scroll.GetScrollView = () => Height;     // 스크롤 뷰(보이는 화면을 얼마나 보여주는지)
            scroll.Refresh = () => Invalidate?.Invoke();
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;
            
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var cBox = thm.ToColor(BoxColor);
            var cSel = thm.ToColor(SelectedColor);
            
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
                using var pth = PathTool.Box(rtContent, Round, thm.Corner);
                canvas.ClipPath(pth, SKClipOperation.Intersect, true);
                canvas.Translate(rtContent.Left, spos + rtContent.Top);     // 스크롤 위치만큼 이동(spos가 필요한 이유) - 여기서부터 위치변경 후 그리기 시작
                itemLoop((i, rt, item) =>
                {
                    if(SelectedItems.Contains(item))
                        Util.DrawBox(canvas, rt, cSel, GoRoundType.Rect, thm.Corner);   // 선택여부(초록색)

                    Util.DrawTextIcon(canvas, item.Text, FontName, FontSize, item.IconString, IconSize, IconDirection, IconGap, rt, cText, ItemAlignment);
                });
            }

            scroll.Draw(canvas, rtScroll);

            #region Border
            if (BackgroundDraw)
            {
                var rnds = Util.Rounds(GoDirectionHV.Horizon, Round, 2);
                Util.DrawBox(canvas, rtContent, SKColors.Transparent, cBorder, Round, thm.Corner);
            }
            #endregion


            base.OnDraw(canvas);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            if (CollisionTool.Check(rts["Content"], x, y))
            {
                scroll.MouseDown(x, y, rts["Scroll"]);
                if (scroll.TouchMode && CollisionTool.Check(rts["Box"], x, y)) scroll.TouchDown(x, y);
            }
            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            scroll.MouseMove(x, y, rts["Scroll"]);
            if (scroll.TouchMode) scroll.TouchMove(x, y);
            base.OnMouseMove(x, y);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            scroll.MouseUp(x, y);
            if (scroll.TouchMode) scroll.TouchUp(x, y);
            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseWheel(float x, float y, float delta)
        {
            var rts = Areas();
            if (CollisionTool.Check(rts["Content"], x, y))
            {
                scroll.MouseWheel(x, y,delta);
            }
            base.OnMouseWheel(x, y, delta);
        }

        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtBox.Top - spos;

            itemLoop((i, rt, item) =>
            {
                if (CollisionTool.Check(rt, x, ry))
                {
                    select(item, i);
                    ItemClicked?.Invoke(this, new ListItemClickEventArgs(item));
                }
            });

            base.OnMouseClick(x, y, button);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtBox.Top - spos;

            itemLoop((i, rt, item) =>
            {
                if (CollisionTool.Check(rt, x, ry))
                    ItemLongClicked?.Invoke(this, new ListItemClickEventArgs(item));
            });

            base.OnMouseClick(x, y, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Box"];
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtBox.Top - spos;

            itemLoop((i, rt, item) =>
            {
                if (CollisionTool.Check(rt, x, ry))
                    ItemDoubleClicked?.Invoke(this, new ListItemClickEventArgs(item));
            });

            base.OnMouseClick(x, y, button);
        }

        protected override void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            bShift = Shift;
            bControl = Control;
            bAlt = Alt;
            base.OnKeyDown(Shift, Control, Alt, key);
        }

        protected override void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            bShift = Shift;
            bControl = Control;
            bAlt = Alt;
            base.OnKeyUp(Shift, Control, Alt, key);
        }

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

        #region Method
        #region itemLoop
        void itemLoop(Action<int, SKRect, GoListItem> loop)
        {
            var rts = Areas();
            var rtBox = rts["Box"];

            var spos = -Convert.ToSingle(scroll.ScrollPositionWithOffset);

            var si = Math.Max(0, Convert.ToInt32(Math.Floor(spos / ItemHeight)));   // 시작 인덱스(1000만개가 있다고 해도, 30개만 보여주면 되니까, 속도도 빠름)
            var ei = Math.Min(Items.Count - 1, Convert.ToInt32(Math.Ceiling((spos + rtBox.Height) / ItemHeight))); // 끝 인덱스

            for (int i = si; i <= ei; i++)
                loop(i, Util.FromRect(0, i * ItemHeight, rtBox.Width, ItemHeight), Items[i]);
        }
        #endregion

        #region select
        private void select(GoListItem item, int i)
        {
            #region Single
            if (SelectionMode == GoItemSelectionMode.SIngle)
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
                if (SelectedItems.Contains(item))
                {
                    SelectedItems.Remove(item);
                    SelectedChanged?.Invoke(this, EventArgs.Empty);
                }
                else
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
                            int idx2 = i;
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
            }
            #endregion
        }
        #endregion
        #endregion
    }
}
