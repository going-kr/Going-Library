﻿using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Controls;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Themes;
using SkiaSharp.Views.Desktop;
using System.Windows.Forms.Design;
using Going.UI.Containers;
using System.Collections.ObjectModel;
namespace Going.UI.Forms.Containers
{
    public class GoPanel : Going.UI.Forms.Containers.GoContainer
    {
        #region Properties
        public string? IconString { get => control.IconString; set { if (control.IconString != value) { control.IconString = value; Invalidate(); } } }
        public float IconSize { get => control.IconSize; set { if (control.IconSize != value) { control.IconSize = value; Invalidate(); } } }
        public float IconGap { get => control.IconGap; set { if (control.IconGap != value) { control.IconGap = value; Invalidate(); } } }

        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public override string Text { get => base.Text; set { base.Text = value; if (control.Text != value) { control.Text = value; Invalidate(); } } }
        public string FontName { get => control.FontName; set { if (control.FontName != value) { control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => control.FontStyle; set { if (control.FontStyle != value) { control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => control.FontSize; set { if (control.FontSize != value) { control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => control.TextColor; set { if (control.TextColor != value) { control.TextColor = value; Invalidate(); } } }
        public string PanelColor { get => control.PanelColor; set { if (control.PanelColor != value) { control.PanelColor = value; Invalidate(); } } }
        public GoRoundType Round { get => control.Round; set { if (control.Round != value) { control.Round = value; Invalidate(); } } }

        public bool BackgroundDraw { get => control.BackgroundDraw; set { if (control.BackgroundDraw != value) { control.BackgroundDraw = value; Invalidate(); } } }
        public bool BorderOnly { get => control.BorderOnly; set { if (control.BorderOnly != value) { control.BorderOnly = value; Invalidate(); } } }

        public float TitleHeight { get => control.TitleHeight; set { if (control.TitleHeight != value) { control.TitleHeight = value; Invalidate(); } } }

        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GoButtonItem> Buttons { get => control.Buttons; set { if (control.Buttons != value) { control.Buttons = value; Invalidate(); } } }
        public float? ButtonWidth { get => control.ButtonWidth; set { if (control.ButtonWidth != value) { control.ButtonWidth = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked { add => control.ButtonClicked += value; remove => control.ButtonClicked -= value; }
        #endregion

        #region Member Variable
        Going.UI.Containers.GoPanel control = new Going.UI.Containers.GoPanel();
        #endregion

        #region Constructor
        public GoPanel()
        {
            control.Text = base.Text;
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            control.Bounds = Util.FromRect(0, 0, Width, Height);
            control.FireDraw(e.Canvas);
            base.OnContentDraw(e);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            control.FireMouseDown(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            control.FireMouseUp(e.X, e.Y, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            control.FireMouseMove(e.X, e.Y);
            Invalidate();
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            control.FireMouseDoubleClick(e.X, e.Y, ToGoMouseButton(e.Button));
            base.OnMouseDoubleClick(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            control.FireMouseWheel(e.X, e.Y, e.Delta / 120F);
            base.OnMouseWheel(e);
        }
        #endregion
        #region OnMouseEnter
        protected override void OnMouseEnter(EventArgs e)
        {
            control.FireMouseMove(0, 0);
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #region OnMouseLeave
        protected override void OnMouseLeave(EventArgs e)
        {
            control.FireMouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            Invalidate();
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }
}
