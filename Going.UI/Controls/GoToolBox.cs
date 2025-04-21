using Going.UI.Collections;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Going.UI.Controls
{
    public class GoToolBox : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 1)] public float IconGap { get; set; } = 5;
        [GoProperty(PCategory.Control, 2)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 3)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 4)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 5)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 6)] public string BoxColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control, 7)] public string BorderColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 8)] public string SelectColor { get; set; } = "Select";
        [GoProperty(PCategory.Control, 9)] public string CategoryColor { get; set; } = "Base2";
        [GoProperty(PCategory.Control, 10)] public GoRoundType Round { get; set; } = GoRoundType.All;

        [GoProperty(PCategory.Control, 11)] public bool BackgroundDraw { get; set; } = true;

        [GoProperty(PCategory.Control, 12)] public float ItemHeight { get; set; } = 30;
        [GoProperty(PCategory.Control, 13)] public ObservableList<GoToolCategory> Categories { get; set; } = [];

        [GoProperty(PCategory.Control, 14)] public bool DragMode { get; set; } = true;

        [JsonIgnore] public double ScrollPosition { get => scroll.ScrollPosition; set => scroll.ScrollPosition = value; }
        [JsonIgnore] internal double ScrollPositionWithOffset => scroll.ScrollPositionWithOffset;

        [JsonIgnore] internal float ItemMouseX { get; set; }
        [JsonIgnore] internal float ItemMouseY { get; set; }
        #endregion

        #region Member Variable
        private Scroll scroll = new Scroll() { Direction = ScrollDirection.Vertical };
        private SKRect rtBoxP = new SKRect();
        #endregion

        #region Event 
        public event EventHandler<ToolItemEventArgs>? DragStart;
        public event EventHandler<ToolItemEventArgs>? ItemClicked;
        public event EventHandler<ToolItemEventArgs>? ItemLongClicked;
        public event EventHandler<ToolItemEventArgs>? ItemDoubleClicked;
        #endregion

        #region Constructor
        public GoToolBox()
        {
            Selectable = true;

            scroll.GetScrollTotal = () => Categories.LastOrDefault()?.Bounds.Bottom ?? 0;
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
            var cCat = thm.ToColor(CategoryColor);

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
                canvas.Translate(rtContent.Left, Convert.ToInt64(spos) + rtContent.Top);
                itemLoop((i, category) =>
                {
                    category.Draw(canvas);
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

            GoToolItem? sel = null;
            itemLoop((i, category) => { category.MouseDown(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, (v) => sel ??= v); });

            if (sel != null)
            {
                if (DragMode)
                {
                    DragStart?.Invoke(this, new ToolItemEventArgs(sel));
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
            GoToolItem? sel = null;
            itemLoop((i, category) => { category.MouseClick(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, (v) => sel ??= v); });
            if (sel != null) ItemClicked?.Invoke(this, new ToolItemEventArgs(sel));

            base.OnMouseClick(x, y, button);
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            GoToolItem? sel = null;
            itemLoop((i, category) => { category.MouseDoubleClick(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, (v) => sel ??= v); });
            if (sel != null) ItemDoubleClicked?.Invoke(this, new ToolItemEventArgs(sel));
            base.OnMouseDoubleClick(x, y, button);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            GoToolItem? sel = null;
            itemLoop((i, category) => { category.MouseLongClick(x, y - rts["Box"].Top - Convert.ToSingle(scroll.ScrollPositionWithOffset), button, (v) => sel ??= v); });
            if (sel != null) ItemLongClicked?.Invoke(this, new ToolItemEventArgs(sel));

            base.OnMouseLongClick(x, y, button);
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
        void itemLoop(Action<int, GoToolCategory> loop)
        {
            var rts = Areas();
            var rtBox = rts["Box"];

            #region calcbox
            if (Categories.Changed || Categories.Any(x => x.Changed) || !rtBoxP.Equals(rtBox))
            {
                rtBoxP = rtBox;

                var y = 0F;
                foreach (var item in Categories)
                {
                    item.ToolBox = this;

                    var ih = item.Expand ? ItemHeight + (item.Items.Count * ItemHeight) : ItemHeight;
                    item.Bounds = Util.FromRect(0, y, rtBox.Width, ih);
                    y += ih;

                    item.Changed = false;
                }

                Categories.Changed = false;
            }
            #endregion

            rtBox.Offset(0, -Convert.ToSingle(scroll.ScrollPositionWithOffset));
            var (si, ei) = Util.FindRect(Categories.Select(x => x.Bounds).ToList(), rtBox);
            if (si >= 0 && si < Categories.Count && ei >= 0 && ei < Categories.Count)
                for (var i = si; i <= ei; i++) loop(i, Categories[i]);
        }
        #endregion
        #endregion
    }
}
