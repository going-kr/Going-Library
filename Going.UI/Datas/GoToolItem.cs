using Going.UI.Collections;
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
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    #region class : ToolItem
    public class GoToolItem
    {
        public string? Text { get; set; }
        public string? IconString { get; set; }
        public object? Tag { get; set; }

        internal SKRect Bounds { get; set; }

        public override string ToString() => Text ?? string.Empty;
    }
    #endregion
    #region class : ToolCategory
    public class GoToolCategory
    {
        #region Properties
        public string? Text { get; set; }
        public string? IconString { get; set; }
        public object? Tag { get; set; }
        public ObservableList<GoToolItem> Items { get; set; } = [];
        public bool Expand { get; set; } = true;

        internal bool Changed { get => Items.Changed; set => Items.Changed = value; }
        internal GoToolBox? ToolBox { get; set; }
        internal SKRect Bounds
        {
            get => sBounds; 
            set
            {
                if (sBounds != value)
                {
                    sBounds = value;

                    var ih = ToolBox?.ItemHeight ?? 30;
                    var y = Bounds.Top + ih;
                    foreach (var item in Items)
                    {
                        item.Bounds = Util.FromRect(Bounds.Left, y, Bounds.Width, ih);
                        y += ih;
                    }
                }
            }
        }
        #endregion

        #region Member Variable
        private SKRect sBounds;
        private float mx, my;
        #endregion

        #region Method
        #region Fire
        internal void Draw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var tb = ToolBox;
            if (tb != null)
            {
                var cText = thm.ToColor(tb.TextColor);
                var cBorder = thm.ToColor(tb.BorderColor);
                var cBox = thm.ToColor(tb.BoxColor);
                var cSel = thm.ToColor(tb.SelectColor);
                var cCat = thm.ToColor(tb.CategoryColor);

                var rtCat = Util.FromRect(Bounds.Left, Bounds.Top, Bounds.Width, tb.ItemHeight);
                var rtIco = Util.FromRect(Bounds.Left, rtCat.Top, tb.ItemHeight, tb.ItemHeight);
                var rtText = Util.FromRect(rtCat, new GoPadding(tb.ItemHeight, 0, 0, 0));

                Util.DrawBox(canvas, rtCat, cCat, GoRoundType.Rect, thm.Corner);
                Util.DrawIcon(canvas, Expand ? "fa-minus" : "fa-plus", tb.ItemHeight / 3, rtIco, cText);
                Util.DrawTextIcon(canvas, Text, tb.FontName, tb.FontStyle, tb.FontSize, IconString, tb.IconSize, GoDirectionHV.Horizon, tb.IconGap, rtText, cText, GoContentAlignment.MiddleLeft);

                if (Expand)
                {
                    itemLoop((i, item, rt) =>
                    {
                        if (CollisionTool.Check(rt, mx, my))
                        {
                            var vrt = rt; vrt.Inflate(-1, -1);
                            Util.DrawBox(canvas, vrt, SKColors.Transparent, cSel, GoRoundType.All, thm.Corner);
                        }

                        Util.DrawTextIcon(canvas, item.Text, tb.FontName, tb.FontStyle, tb.FontSize, item.IconString, tb.IconSize, GoDirectionHV.Horizon, tb.IconGap, rt, cText);
                    });
                }
            }
        }

        internal GoToolItem? MouseDown(float x, float y, GoMouseButton mb)
        {
            GoToolItem? ret = null;
            var tb = ToolBox;
            if (tb != null)
            {
                #region Expand
                var rtCat = Util.FromRect(Bounds.Left, Bounds.Top, Bounds.Width, tb.ItemHeight);
                var rtIco = Util.FromRect(Bounds.Left, rtCat.Top, tb.ItemHeight, tb.ItemHeight);
                if (CollisionTool.Check(rtIco, x, y)) { Expand = !Expand; Changed = true; }
                #endregion

                itemLoop((i, item, rt) => { if (CollisionTool.Check(rt, x, y)) ret = item; });
            }
            return ret;
        }

        internal GoToolItem? MouseMove(float x, float y)
        {
            mx = x; my = y;
            GoToolItem? ret = null;
            return ret;
        }

        internal GoToolItem? MouseUp(float x, float y, GoMouseButton button)
        {
            GoToolItem? ret = null;
            return ret;
        }

        internal GoToolItem? MouseClick(float x, float y, GoMouseButton button)
        {
            GoToolItem? ret = null;
            if (ToolBox != null)
                itemLoop((i, item, rt) => { if (CollisionTool.Check(rt, x, y)) ret = item; });
            return ret;
        }

        internal GoToolItem? MouseDoubleClick(float x, float y, GoMouseButton button)
        {
            GoToolItem? ret = null;
            if (ToolBox != null)
                itemLoop((i, item, rt) => { if (CollisionTool.Check(rt, x, y)) ret = item; });
            return ret;
        }

        internal GoToolItem? MouseLongClick(float x, float y, GoMouseButton button)
        {
            GoToolItem? ret = null;
            if (ToolBox != null)
                itemLoop((i, item, rt) => { if (CollisionTool.Check(rt, x, y)) ret = item; });
            return ret;
        }
        #endregion
        #region loop
        void itemLoop(Action<int, GoToolItem, SKRect> loop)
        {
            if (ToolBox != null)
            {
                var tb = ToolBox;
                var rts = ToolBox.Areas();
                var rtBox = rts["Box"];

                rtBox.Offset(0, -Convert.ToSingle(tb.ScrollPositionWithOffset));
                var (si, ei) = Util.FindRect(Items.Select(x => x.Bounds).ToList(), rtBox);
                if (si >= 0 && si < Items.Count && ei >= 0 && ei < Items.Count)
                    for (var i = si; i <= ei; i++)
                    {
                        var item = Items[i];
                        var sz = Util.MeasureTextIcon(item.Text, tb.FontName, tb.FontStyle, tb.FontSize, item.IconString, tb.IconSize, GoDirectionHV.Horizon, tb.IconGap);
                        var rt = Util.FromRect(item.Bounds.Left + 5, item.Bounds.Top + 1, sz.Width + 20, item.Bounds.Height - 2);

                        loop(i, item, rt);
                    }
            }
        }
        #endregion
        #region ToString
        public override string ToString() => Text ?? string.Empty;
        #endregion
        #endregion
    }
    #endregion

    public class ToolItemEventArgs(GoToolItem itm) : EventArgs
    {
        public GoToolItem Item { get; } = itm;
    }
}
