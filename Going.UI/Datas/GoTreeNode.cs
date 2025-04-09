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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public class GoTreeNode
    {
        #region Properties
        public string? Text { get; set; }
        public string? IconString { get; set; }
        public object? Tag { get; set; }
        public ObservableList<GoTreeNode> Nodes { get; set; } = [];
        public bool Expand { get; set; } = true;
        
        [JsonIgnore] public int Depth => Parent == null ? 0 : Parent.Depth + 1;
        [JsonIgnore] internal bool Changed { get => Nodes.Changed; set => Nodes.Changed = value; }
        [JsonIgnore] public GoTreeNode? Parent { get; internal set; }

        [JsonIgnore]
        internal GoTreeView? TreeView
        {
            get => treeView;
            set
            {
                treeView = value;
                foreach (var v in Nodes)
                {
                    v.TreeView = treeView;
                    v.Parent = this;
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
                
                chk |= Nodes.Any(x => x.Bounds.Height == 0);
                if (chk)
                {
                    var th = TreeView?.ItemHeight ?? 30;
                    var y = Bounds.Top + th;
                    foreach (var item in Nodes)
                    {
                        var ih = item.GetHeight();
                        item.Bounds = Util.FromRect(Bounds.Left, y, Bounds.Width, ih);
                        y += ih;
                    }
                }
            }
        }
        #endregion

        #region Member Variable
        private SKRect sBounds;
        private GoTreeNode? parent = null;
        private GoTreeView? treeView = null;
        #endregion

        #region Method
        #region Fire
        #region Draw
        internal void Draw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var tv = TreeView;
            if (tv != null)
            {
                var cText = thm.ToColor(tv.TextColor);
                var cBorder = thm.ToColor(tv.BorderColor);
                var cBox = thm.ToColor(tv.BoxColor);
                var cSel = thm.ToColor(tv.SelectColor);

                var (rtIco, rtItem) = bounds();

                if(tv.SelectedNodes.Contains(this))
                {
                    var vrt = rtItem; vrt.Inflate(-1, -1);
                    Util.DrawBox(canvas, vrt, cSel, GoRoundType.All, thm.Corner);
                }

                if (!(tv.Design?.IsDrag ?? false) && CollisionTool.Check(rtItem, tv.ItemMouseX, tv.ItemMouseY))
                {
                    var vrt = rtItem; vrt.Inflate(-1, -1);
                    var c = cSel.BrightnessTransmit(thm.HoverBorderBrightness);
                    Util.DrawBox(canvas, vrt, SKColors.Transparent, c, GoRoundType.All, thm.Corner);
                }

                Util.DrawIcon(canvas, Nodes.Count == 0 ? "fa-circle" : (Expand ? "far fa-square-minus" : "fa-square-plus"), Nodes.Count == 0 ? 2 : 10, rtIco, cText);
                Util.DrawTextIcon(canvas, Text, tv.FontName, tv.FontStyle, tv.FontSize, IconString, tv.IconSize, GoDirectionHV.Horizon, tv.IconGap, rtItem, cText, GoContentAlignment.MiddleCenter);

                itemLoop((i, item) => item.Draw(canvas));
            }
        }
        #endregion

        #region Mouse
        internal void MouseDown(float x, float y, GoMouseButton button, Action<GoTreeNode>? itemSelected)
        {
            if (TreeView != null)
            {
                #region Expand
                var rtNd = Util.FromRect(Bounds.Left + 10 * Depth, Bounds.Top, Bounds.Width, TreeView.ItemHeight);
                var rtIco = Util.FromRect(rtNd.Left, rtNd.Top, 30, TreeView.ItemHeight);
                if (CollisionTool.Check(rtIco, x, y))
                {
                    Expand = !Expand;

                    Changed = true;
                    if (Parent != null) Parent.Changed = true;
                }
                #endregion


                if (CollisionTool.Check(bounds().rtItem, x, y)) itemSelected?.Invoke(this);

                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseDown(x, y, button, itemSelected); });
            }
        }

        internal void MouseMove(float x, float y, Action<GoTreeNode>? itemSelected)
        {
            if (TreeView != null)
            {
                if (CollisionTool.Check(bounds().rtItem, x, y)) itemSelected?.Invoke(this);
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseMove(x, y, itemSelected); });
            }
        }

        internal void MouseUp(float x, float y, GoMouseButton button, Action<GoTreeNode>? itemSelected)
        {
            if (TreeView != null)
            {
                if (CollisionTool.Check(bounds().rtItem, x, y)) itemSelected?.Invoke(this);
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseUp(x, y, button, itemSelected); });
            }
        }

        internal void MouseClick(float x, float y, GoMouseButton button, Action<GoTreeNode>? itemSelected)
        {
            if (TreeView != null)
            {
                if (CollisionTool.Check(bounds().rtItem, x, y)) itemSelected?.Invoke(this);
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseClick(x, y, button, itemSelected); });
            }
        }
        internal void MouseDoubleClick(float x, float y, GoMouseButton button, Action<GoTreeNode>? itemSelected)
        {
            if (TreeView != null)
            {
                if (CollisionTool.Check(bounds().rtItem, x, y)) itemSelected?.Invoke(this);
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseDoubleClick(x, y, button, itemSelected); });
            }
        }

        internal void MouseLongClick(float x, float y, GoMouseButton button, Action<GoTreeNode>? itemSelected)
        {
            if (TreeView != null)
            {
                if (CollisionTool.Check(bounds().rtItem, x, y)) itemSelected?.Invoke(this);
                itemLoop((i, item) => { if (CollisionTool.Check(item.Bounds, x, y)) item.MouseLongClick(x, y, button, itemSelected); });
            }
        }
        #endregion
        #endregion

        #region loop
        void itemLoop(Action<int, GoTreeNode> loop)
        {
            if (TreeView != null && Expand)
            {
                var tv = TreeView;
                var rts = tv.Areas();
                var rtBox = rts["Box"];

                rtBox.Offset(0, -Convert.ToSingle(tv.ScrollPositionWithOffset));
                var (si, ei) = Util.FindRect(Nodes.Select(x => x.Bounds).ToList(), rtBox);
                if (si >= 0 && si < Nodes.Count && ei >= 0 && ei < Nodes.Count)
                    for (var i = si; i <= ei; i++)
                        loop(i, Nodes[i]);
            }
        }
        #endregion

        #region GetHeight
        internal float GetHeight()
        {
            float ret = 0;
            var ih = TreeView?.ItemHeight ?? 30;
            if (!Expand) ret = ih;
            else
            {
                ret = ih + (Nodes.Count > 0 ? Nodes.Sum(x => x.GetHeight()) : 0);
            }
            return ret;
        }
        #endregion

        #region bounds
        (SKRect rtIco, SKRect rtItem) bounds()
        {
            SKRect rtIco = Bounds, rtItem = Bounds;

            var tv = TreeView;
            if (tv != null)
            {
                var rtNd = Util.FromRect(Bounds.Left + 10 * Depth, Bounds.Top, Bounds.Width, tv.ItemHeight);
                var sz = Util.MeasureTextIcon(Text, tv.FontName, tv.FontStyle, tv.FontSize, IconString, tv.IconSize, GoDirectionHV.Horizon, tv.IconGap);

                rtIco = Util.FromRect(rtNd.Left, rtNd.Top, 30, tv.ItemHeight);
                rtItem = Util.FromRect(rtNd.Left + rtIco.Width, rtNd.Top + 1, sz.Width + 20, rtNd.Height - 2);
            }
            return (rtIco, rtItem);
        }
        #endregion

        #region ToString
        public override string ToString() => Text ?? string.Empty;
        #endregion
        #endregion
    }

    public class TreeNodeEventArgs(GoTreeNode itm) : EventArgs
    {
        public GoTreeNode Item { get; } = itm;
    }
}
