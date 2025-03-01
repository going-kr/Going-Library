﻿using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using Going.UI.Extensions;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Tools;
using System.Diagnostics.Eventing.Reader;
using Windows.ApplicationModel.Activation;

namespace Going.UI.Forms.ImageCanvas
{
    public class IcButton : SKControl
    {
        #region Properties
        public string? IconString { get => sIconString; set { if (sIconString != value) { sIconString = value; Invalidate(); } } }
        public float IconSize { get => nIconSize; set { if (nIconSize != value) { nIconSize = value; Invalidate(); } } }
        public GoDirectionHV IconDirection { get => eIconDirection; set { if (eIconDirection != value) { eIconDirection = value; Invalidate(); } } }
        public float IconGap { get => nIconGap; set { if (nIconGap != value) { nIconGap = value; Invalidate(); } } }
        public override string Text { get => base.Text; set { if (base.Text != value) { base.Text = value; Invalidate(); } } }
        public string FontName { get => sFontName; set { if (sFontName != value) { sFontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => eFontStyle; set { if (eFontStyle != value) { eFontStyle = value; Invalidate(); } } }
        public float FontSize { get => nFontSize; set { if (nFontSize != value) { nFontSize = value; Invalidate(); } } }
        public string TextColor { get => sTextColor; set { if (sTextColor != value) { sTextColor = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        private bool bDown = false;

        private string? sIconString = null;
        private float nIconSize = 12;
        private GoDirectionHV eIconDirection = GoDirectionHV.Horizon;
        private GoFontStyle eFontStyle = GoFontStyle.Normal;
        private float nIconGap = 5;
        private string sFontName = "나눔고딕";
        private float nFontSize = 12;
        private string sTextColor = "Black";
        #endregion

        #region Event
        public event EventHandler? ButtonClicked;
        #endregion

        #region Constructor
        public IcButton()
        {
            Text = "";
        }
        #endregion

        #region Override
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var thm = GoTheme.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var cText = thm.ToColor(TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);

            var (ip, img, cBack) = vars();
            if (ip != null && img != null && Parent != null && img.On != null && img.Off != null)
            {
                var sx = (double)img.On.Width / Parent.Width;
                var sy = (double)img.On.Height / Parent.Height;
                canvas.DrawBitmap(bDown ? img.On : img.Off, Util.FromRect(Convert.ToInt16(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);

                if (bDown) rtBox.Offset(0, 1);
                Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, GoContentAlignment.MiddleCenter);
            }

            #region Enabled
            if (!Enabled)
            {
                using var p = new SKPaint { IsAntialias = true };

                p.IsStroke = false;
                p.Color = cBack.WithAlpha(180);
                canvas.DrawRect(rtBox, p);
            }
            #endregion
            #region DesignMode
            if (DesignMode)
            {
                using var p = new SkiaSharp.SKPaint { IsAntialias = false };
                using var pe = SKPathEffect.CreateDash([2F, 2F], 2);
                p.IsStroke = true;
                p.Color = SKColors.Red;
                p.PathEffect = pe;
                canvas.DrawRect(Util.FromRect(0, 0, Width - 1, Height - 1), p);
                p.PathEffect = null;
            }
            #endregion

            base.OnPaintSurface(e);
        }

        #region Mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            bDown = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var rtBox = Util.FromRect(0, 0, Width, Height);

            if (bDown)
            {
                bDown = false;
                if(CollisionTool.Check(rtBox, e.X, e.Y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
            base.OnMouseUp(e);
        }
        #endregion
        #endregion

        #region Method
        #region vars
        (IcImageFolder? ip, IcOnOffImage? img, SKColor cBack) vars()
        {
            var thm = GoTheme.Current;
            SKColor cBack = thm.Back;
            IcImageFolder? ip = null;
            IcOnOffImage? img = null;

            if (Parent is IcContainer con)
            {
                ip = IcResources.Get(con.ImageFolder);
                cBack = thm.ToColor(con.BackgroundColor);
                img = ip != null && con.ContainerName != null && ip.Containers.TryGetValue(con.ContainerName, out var v) ? v : null;
            }
            else if (Parent is TabPage tab && tab.Parent is IcCanvas ic)
            {
                ip = IcResources.Get(ic.ImageFolder);
                cBack = thm.ToColor(ic.BackgroundColor);
                img = ip != null && tab.Name != null && ip.Pages.TryGetValue(tab.Name, out var v) ? v : null;
            }

            return (ip, img, cBack);
        }
        #endregion
        #endregion
    }
}
