﻿using Going.UI.Collections;
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
        public float IconGap { get; set; } = 5;
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; }= GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string BoxColor { get; set; } = "Base1";
        public string BorderColor { get; set; } = "Base3";
        public string SelectColor { get; set; } = "Select";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool BackgroundDraw { get; set; } = true;

        public float ItemHeight { get; set; } = 30;
        public GoContentAlignment ItemAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        public ObservableList<GoListItem> Items { get; set; } = [];
        public GoItemSelectionMode SelectionMode { get; set; } = GoItemSelectionMode.Single;
        [JsonIgnore] public List<GoListItem> SelectedItems { get; } = [];

        [JsonIgnore] public double ScrollPosition { get => scroll.ScrollPosition; set => scroll.ScrollPosition = value; }
        [JsonIgnore] internal double ScrollPositionWithOffset => scroll.ScrollPositionWithOffset;
        #endregion

        #region Member Variable
        private Scroll scroll = new Scroll() { Direction = ScrollDirection.Vertical };

        bool bShift, bControl;
        private GoListItem? first = null;
        private SKRect rtBoxP = new SKRect();
        #endregion

        #region Event 
        public event EventHandler? SelectedChanged;
        public event EventHandler<ListItemEventArgs>? ItemClicked;
        public event EventHandler<ListItemEventArgs>? ItemLongClicked;
        public event EventHandler<ListItemEventArgs>? ItemDoubleClicked;
        #endregion

        #region Constructor
        public GoListBox()
        {
            Selectable = true;

            scroll.GetScrollTotal = () => ItemHeight * Items.Count;
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
                canvas.Translate(rtContent.Left, Convert.ToInt64(spos) + rtContent.Top);
                itemLoop((i, item) =>
                {
                    if(SelectedItems.Contains(item))
                        Util.DrawBox(canvas, item.Bounds, cSel, GoRoundType.Rect, thm.Corner);

                    Util.DrawTextIcon(canvas, item.Text, FontName, FontStyle, FontSize, item.IconString, IconSize, GoDirectionHV.Horizon, IconGap, item.Bounds, cText, ItemAlignment);
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

            base.OnMouseClick(x, y, button);
        }

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

            base.OnMouseClick(x, y, button);
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

            rtBox.Offset(0, -Convert.ToSingle(scroll.ScrollPositionWithOffset));
            var (si, ei) = Util.FindRect(Items.Select(x => x.Bounds).ToList(), rtBox);

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
            else if (SelectionMode == GoItemSelectionMode.MultiPC)
            {
                if (bControl)
                {
                    #region Control
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
