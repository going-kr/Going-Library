﻿using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp.Views.Desktop;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Windows.ApplicationModel;
using Going.UI.Tools;
using Going.UI.ImageCanvas;

namespace Going.UI.Forms.ImageCanvas
{
    public class IcProgress : SKControl
    {
        #region Properties
        public string FontName { get => sFontName; set { if (sFontName != value) { sFontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => eFontStyle; set { if (eFontStyle != value) { eFontStyle = value; Invalidate(); } } }
        public float FontSize { get => nFontSize; set { if (nFontSize != value) { nFontSize = value; Invalidate(); } } }
        public string TextColor { get => sTextColor; set { if (sTextColor != value) { sTextColor = value; Invalidate(); } } }

        public string? BarName { get => sBarName; set { if (sBarName != value) { sBarName = value; Invalidate(); } } }

        public string FormatString { get => sFormatString; set { if (sFormatString != value) { sFormatString = value; Invalidate(); } } }
        public double Minimum { get => nMin; set { if (nMin != value) { nMin = value; Invalidate(); } } }
        public double Maximum { get => nMax; set { if (nMax != value) { nMax = value; Invalidate(); } } }
        public double Value
        {
            get => nValue;
            set
            {
                if (nValue != value)
                {
                    nValue = MathTool.Constrain(value, nMin, nMax);
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public bool DrawText { get => bDrawText; set { if (bDrawText != value) { bDrawText = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public IcProgress()
        {
            Text = "";
        }
        #endregion

        #region Member Variable
        private bool bDrawText = false;

        private double nValue = 0, nMin = 0, nMax = 100;

        private GoFontStyle eFontStyle = GoFontStyle.Normal;
        private string sFontName = "나눔고딕";
        private float nFontSize = 12;
        private string sTextColor = "Black";
        private string sFormatString = "0";

        private string? sBarName;
        #endregion

        #region Override
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var thm = GoTheme.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var cText = thm.ToColor(TextColor);

            var (ip, img, cBack) = vars();
            if (ip != null && img != null && Parent != null && img.On != null && img.Off != null)
            {
                var sx = (double)img.On.Width / Parent.Width;
                var sy = (double)img.On.Height / Parent.Height;
                canvas.DrawBitmap(img.Off, Util.FromRect(Convert.ToInt32(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);
            }

            bounds(ip, (_, rtBar, rtFill, bmBar) =>
            {
                canvas.DrawBitmap(bmBar.Off, rtBar);
                if (Value > Minimum) canvas.DrawBitmap(bmBar.On, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill);
                if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtBar, cText);
            });

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
        #endregion

        #region Method
        #region bounds
        void bounds(IcImageFolder? ip, Action<SKRect, SKRect, SKRect, IcOnOffImage> act)
        {
            if (ip != null && sBarName != null && ip.Miscs.TryGetValue(sBarName, out var bmBar) && bmBar.On != null && bmBar.Off != null)
            {
                var rtBox = Util.FromRect(0, 0, Width, Height);
                var rtBar = MathTool.MakeRectangle(rtBox, new SKSize(bmBar.Off.Width, bmBar.Off.Height));

                var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, rtBar.Width));
                var rtFill = Util.FromRect(rtBar.Left, rtBar.Top, w, rtBar.Height);

                act(rtBox, rtBar, rtFill, bmBar);
            }
        }
        #endregion
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
