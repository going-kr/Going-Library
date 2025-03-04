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

namespace Going.UI.Controls
{
    public class GoTreeView : GoControl
    {
        #region Properties
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string BoxColor { get; set; } = "Base1";
        public string BorderColor { get; set; } = "Base3";
        public string SelectColor { get; set; } = "Select";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool BackgroundDraw { get; set; } = true;
        public bool DragMode { get; set; } = false;

        public float ItemHeight { get; set; } = 30;
        public ObservableList<GoTreeNode> Nodes { get; set; } = [];

        public GoItemSelectionMode SelectionMode { get; set; } = GoItemSelectionMode.SIngle;
        [JsonIgnore] public List<GoTreeNode> SelectedNodes { get; } = [];

        [JsonIgnore] public double ScrollPosition { get => scroll.ScrollPosition; set => scroll.ScrollPosition = value; }
        [JsonIgnore] internal double ScrollPositionWithOffset => scroll.ScrollPositionWithOffset;

        [JsonIgnore] internal float ItemMouseX { get; set; }
        [JsonIgnore] internal float ItemMouseY { get; set; }
        #endregion

        #region Member Variable
        private Scroll scroll = new Scroll() { Direction = ScrollDirection.Vertical };

        private bool bShift, bControl;
        private GoTreeNode ? first = null;
        #endregion

        #region Event 
        public event EventHandler? SelectedChanged;

        public event EventHandler<TreeNodeEventArgs>? DragStart;
        public event EventHandler<TreeNodeEventArgs>? ItemClicked;
        public event EventHandler<TreeNodeEventArgs>? ItemLongClicked;
        public event EventHandler<TreeNodeEventArgs>? ItemDoubleClicked;
        #endregion

        #region Constructor
        public GoTreeView()
        {
            Selectable = true;

            scroll.GetScrollTotal = () => Nodes.LastOrDefault()?.Bounds.Bottom ?? 0;
            scroll.GetScrollTick = () => ItemHeight;
            scroll.GetScrollView = () => Height;
            scroll.Refresh = () => Invalidate?.Invoke();
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;

            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var cBox = thm.ToColor(BoxColor);
            var cSel = thm.ToColor(SelectColor);

            var rts = Areas();
            var rtContent = rts["Content"];
            var rtBox = rts["Box"];
            var rtScroll = rts["Scroll"];

            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
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
                canvas.Translate(rtContent.Left, spos + rtContent.Top);
                
                itemLoop((i, node) =>
                {
                    node.Draw(canvas);
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
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();

            GoTreeNode? sel = null;
            itemLoop((i, category) => { category.MouseDown(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, (v) => sel ??= v); });

            if (sel != null)
            {
                if (DragMode)
                {
                    DragStart?.Invoke(this, new TreeNodeEventArgs(sel));
                    Design?.Drag(sel);
                }
            }
            else
            {
                #region Scroll
                if (CollisionTool.Check(rts["Content"], x, y))
                {
                    scroll.MouseDown(x, y, rts["Scroll"]);
                    if (scroll.TouchMode && CollisionTool.Check(rts["Box"], x, y)) scroll.TouchDown(x, y);
                }
                #endregion
            }
            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();

            ItemMouseX = x;
            ItemMouseY = y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset);

            itemLoop((i, category) => { category.MouseMove(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), null); });

            #region Scroll
            scroll.MouseMove(x, y, rts["Scroll"]);
            if (scroll.TouchMode) scroll.TouchMove(x, y);
            #endregion
            base.OnMouseMove(x, y);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();

            itemLoop((i, category) => { category.MouseUp(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, null); });

            #region Scroll
            scroll.MouseUp(x, y);
            if (scroll.TouchMode) scroll.TouchUp(x, y);
            #endregion
            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseWheel(float x, float y, float delta)
        {
            var rts = Areas();
            if (CollisionTool.Check(rts["Content"], x, y)) scroll.MouseWheel(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), delta);
            base.OnMouseWheel(x, y, delta);
        }

        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            GoTreeNode? sel = null;
            itemLoop((i, category) => { category.MouseClick(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, (v) => sel ??= v); });
            if (sel != null)
            {
                ItemClicked?.Invoke(this, new TreeNodeEventArgs(sel));
                select(sel);
            }

            base.OnMouseClick(x, y, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            GoTreeNode? sel = null;
            itemLoop((i, category) => { category.MouseDoubleClick(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, (v) => sel ??= v); });
            if (sel != null) ItemDoubleClicked?.Invoke(this, new TreeNodeEventArgs(sel));
            base.OnMouseDoubleClick(x, y, button);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            GoTreeNode? sel = null;
            itemLoop((i, category) => { category.MouseLongClick(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, (v) => sel ??= v); });
            if (sel != null) ItemLongClicked?.Invoke(this, new TreeNodeEventArgs(sel));

            base.OnMouseLongClick(x, y, button);
        }
        #endregion

        #region Key
        protected override void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            bShift = Shift;
            bControl = Control;
            base.OnKeyDown(Shift, Control, Alt, key);
        }

        protected override void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            bShift = Shift;
            bControl = Control;
            base.OnKeyUp(Shift, Control, Alt, key);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];
            var scwh = Scroll.SC_WH;
            var rtBox = Util.FromRect(rtContent.Left, rtContent.Top, rtContent.Width - scwh, rtContent.Height);
            var rtScroll = Util.FromRect(rtBox.Right, rtBox.Top, scwh, rtBox.Height);

            dic["Box"] = rtBox;
            dic["Scroll"] = rtScroll;

            return dic;
        }
        #endregion
        #endregion

        #region Method
        #region itemLoop
        void itemLoop(Action<int, GoTreeNode> loop)
        {
            var rts = Areas();
            var rtBox = rts["Box"];

            #region calcbox
            if (Nodes.Changed || Nodes.Any(x => x.Changed))
            {
                var y = 0F;
                foreach (var item in Nodes)
                {
                    item.TreeView = this;

                    var ih = item.GetHeight();
                    item.Bounds = Util.FromRect(0, y, rtBox.Width, ih);
                    y += ih;

                    item.Changed = false;
                }

                Nodes.Changed = false;
            }
            #endregion

            rtBox.Offset(0, -Convert.ToSingle(scroll.ScrollPositionWithOffset));
            var (si, ei) = Util.FindRect(Nodes.Select(x => x.Bounds).ToList(), rtBox);
            if (si >= 0 && si < Nodes.Count && ei >= 0 && ei < Nodes.Count)
                for (var i = si; i <= ei; i++) loop(i, Nodes[i]);
        }
        #endregion

        #region select
        private void select(GoTreeNode item)
        {
            #region Single
            if (SelectionMode == GoItemSelectionMode.SIngle)
            {
                SelectedNodes.Clear();
                SelectedNodes.Add(item);
                SelectedChanged?.Invoke(this, EventArgs.Empty);
                first = item;
            }
            #endregion
            #region Multi
            else if (SelectionMode == GoItemSelectionMode.Multi)
            {
                if (SelectedNodes.Contains(item))
                {
                    SelectedNodes.Remove(item);
                    if (SelectedChanged != null) SelectedChanged.Invoke(this, new EventArgs());
                }
                else
                {
                    SelectedNodes.Add(item);
                    if (SelectedChanged != null) SelectedChanged.Invoke(this, new EventArgs());
                }
            }
            #endregion
            #region MultiPC
            else if (SelectionMode == GoItemSelectionMode.MultiPC)
            {
                if (bControl)
                {
                    #region Control
                    if (SelectedNodes.Contains(item))
                    {
                        SelectedNodes.Remove(item);
                        SelectedChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        SelectedNodes.Add(item);
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
                        SelectedNodes.Add(item);
                        SelectedChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        var Items = mk(Nodes);

                        int idx1 = Items.IndexOf(first);
                        int idx2 = Items.IndexOf(item);
                        int min = Math.Min(idx1, idx2);
                        int max = Math.Max(idx1, idx2);

                        bool b = false;
                        for (int ii = min; ii <= max; ii++)
                        {
                            if (!SelectedNodes.Contains(Items[ii]))
                            {
                                SelectedNodes.Add(Items[ii]);
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
                    SelectedNodes.Clear();
                    SelectedNodes.Add(item);
                    SelectedChanged?.Invoke(this, EventArgs.Empty);
                    first = item;
                    #endregion
                }
            }
            #endregion
        }

        List<GoTreeNode> mk(IEnumerable<GoTreeNode> nodes)
        {
            List<GoTreeNode> ret = [];
            foreach(var nd in nodes)
            {
                ret.Add(nd);
                if (nd.Expand) ret.AddRange(mk(nd.Nodes));
            }
            return ret;
        }
        #endregion
        #endregion
    }
}
