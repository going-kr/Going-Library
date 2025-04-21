using Going.UI.Collections;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Datas
{
    #region class : ToolItem
    public class GoToolItem
    {
        #region Properties
        [GoProperty(PCategory.Basic, 0)] public string? Text { get; set; }
        [GoProperty(PCategory.Basic, 1)] public string? IconString { get; set; }
        public object? Tag { get; set; }

        [JsonIgnore] internal SKRect Bounds { get; set; }
        [JsonIgnore] internal GoToolBox? ToolBox { get; set; }
        [JsonIgnore] public GoToolCategory? Category { get; internal set; }
        #endregion

        #region Method
        #region Fire
        #region Draw
        internal void Draw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var tb = ToolBox;
            if (tb != null)
            {
                var cText = thm.ToColor(tb.TextColor);
                var rt = bounds();
                var cSel = thm.ToColor(tb.SelectColor);

                if (!(tb.Design?.IsDrag ?? false) && CollisionTool.Check(rt, tb.ItemMouseX, tb.ItemMouseY))
                {
                    var vrt = rt; vrt.Inflate(-1, -1);
                    var c = cSel.BrightnessTransmit(thm.HoverBorderBrightness);
                    Util.DrawBox(canvas, vrt, SKColors.Transparent, c, GoRoundType.All, thm.Corner);
                }

                Util.DrawTextIcon(canvas, Text, tb.FontName, tb.FontStyle, tb.FontSize, IconString, tb.IconSize, GoDirectionHV.Horizon, tb.IconGap, rt, cText);
            }
        }
        #endregion
        #region Mouse
        internal void MouseDown(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null && CollisionTool.Check(bounds(), x, y)) itemSelected?.Invoke(this);
        }

        internal void MouseMove(float x, float y, Action<GoToolItem>? itemSelected) {   }
        internal void MouseUp(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected) { }

        internal void MouseClick(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null && CollisionTool.Check(bounds(), x, y)) itemSelected?.Invoke(this);
        }
        internal void MouseDoubleClick(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null && CollisionTool.Check(bounds(), x, y)) itemSelected?.Invoke(this);
        }

        internal void MouseLongClick(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null && CollisionTool.Check(bounds(), x, y)) itemSelected?.Invoke(this);
        }
        #endregion
        #endregion

        #region ToString
        public override string ToString() => Text ?? string.Empty;
        #endregion

        #region bounds
        SKRect bounds()
        {
            SKRect rt = Bounds;
            if (ToolBox != null)
            {
                var sz = Util.MeasureTextIcon(Text, ToolBox.FontName, ToolBox.FontStyle, ToolBox.FontSize, IconString, ToolBox.IconSize, GoDirectionHV.Horizon, ToolBox.IconGap);
                rt = Util.FromRect(Bounds.Left + 5, Bounds.Top + 1, sz.Width + 20, Bounds.Height - 2);
            }
            return rt;
        }
        #endregion
        #endregion
    }
    #endregion
    #region class : ToolCategory
    public class GoToolCategory
    {
        #region Properties
        [GoProperty(PCategory.Basic, 0)] public string? Text { get; set; }
        [GoProperty(PCategory.Basic, 1)] public string? IconString { get; set; }
        public object? Tag { get; set; }
        [GoProperty(PCategory.Basic, 2)] public ObservableList<GoToolItem> Items { get; set; } = [];
        [GoProperty(PCategory.Basic, 3)] public bool Expand { get; set; } = true;

        [JsonIgnore] internal bool Changed { get => Items.Changed; set => Items.Changed = value; }
       
        [JsonIgnore]
        internal GoToolBox? ToolBox
        {
            get => toolBox;
            set
            {
                toolBox = value;
                foreach (var v in Items)
                {
                    v.ToolBox = toolBox;
                    v.Category = this;
                }
            }
        }

        [JsonIgnore]
        internal SKRect Bounds
        {
            get => sBounds; 
            set
            {
                bool chk = false;
                if (sBounds != value)
                {
                    sBounds = value;
                    chk = true;
                }

                chk |= Items.Any(x => x.Bounds.Height == 0);
                if (chk)
                {
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
        private GoToolBox? toolBox = null;
        #endregion

        #region Method
        #region Fire
        #region Draw
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
                var rtIco = Util.FromRect(rtCat.Left, rtCat.Top, 30, tb.ItemHeight);
                var rtText = Util.FromRect(rtCat, new GoPadding(rtIco.Width, 0, 0, 0));

                Util.DrawBox(canvas, rtCat, cCat, GoRoundType.Rect, thm.Corner);
                Util.DrawIcon(canvas, Expand ? "fa-minus" : "fa-plus", 12, rtIco, cText);
                Util.DrawTextIcon(canvas, Text, tb.FontName, tb.FontStyle, tb.FontSize, IconString, tb.IconSize, GoDirectionHV.Horizon, tb.IconGap, rtText, cText, GoContentAlignment.MiddleLeft);

                itemLoop((i, item) => item.Draw(canvas));
            }
        }
        #endregion

        #region Mouse
        internal void MouseDown(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null)
            {
                #region Expand
                var rtCat = Util.FromRect(Bounds.Left, Bounds.Top, Bounds.Width, ToolBox.ItemHeight);
                var rtIco = Util.FromRect(Bounds.Left, rtCat.Top, ToolBox.ItemHeight, ToolBox.ItemHeight);
                if (CollisionTool.Check(rtIco, x, y)) { Expand = !Expand; Changed = true; }
                #endregion

                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseDown(x, y, button, itemSelected); });
            }
        }

        internal void MouseMove(float x, float y, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null)
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseMove(x, y, itemSelected); });
        }

        internal void MouseUp(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null)
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseUp(x, y, button, itemSelected); });
        }

        internal void MouseClick(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null)
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseClick(x, y, button, itemSelected); });
        }
        internal void MouseDoubleClick(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null)
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseDoubleClick(x, y, button, itemSelected); });
        }

        internal void MouseLongClick(float x, float y, GoMouseButton button, Action<GoToolItem>? itemSelected)
        {
            if (ToolBox != null)
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseLongClick(x, y, button, itemSelected); });
        }
        #endregion
        #endregion

        #region loop
        void itemLoop(Action<int, GoToolItem> loop)
        {
            if (ToolBox != null && Expand)
            {
                var tb = ToolBox;
                var rts = tb.Areas();
                var rtBox = rts["Box"];

                rtBox.Offset(0, -Convert.ToSingle(tb.ScrollPositionWithOffset));
                var (si, ei) = Util.FindRect(Items.Select(x => x.Bounds).ToList(), rtBox);
                if (si >= 0 && si < Items.Count && ei >= 0 && ei < Items.Count)
                    for (var i = si; i <= ei; i++)
                        loop(i, Items[i]);
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
