using Going.UI.Enums;
using Going.UI.ImageCanvas;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.ImageCanvas
{
    public class IcOnOff : SKControl
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

        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value)
                {
                    bOnOff = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public bool ToggleMode { get; set; } = false;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Constructor
        public IcOnOff()
        {
            Text = "";
        }
        #endregion

        #region Member Variable
        private bool bOnOff = false;

        private string? sIconString = null;
        private float nIconSize = 12;
        private GoDirectionHV eIconDirection = GoDirectionHV.Horizon;
        private GoFontStyle eFontStyle = GoFontStyle.Normal;
        private float nIconGap = 5;
        private string sFontName = "나눔고딕";
        private float nFontSize = 12;
        private string sTextColor = "Black";
        #endregion

        #region Override
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var thm = GoThemeW.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var cText = thm.ToColor(TextColor);

            var (ip, off, on, cBack) = vars();
            if (ip != null && Parent != null && on != null && off != null)
            {
                var sx = on.Width / Parent.Width;
                var sy = on.Height / Parent.Height;
                canvas.DrawImage(OnOff ? on : off, Util.FromRect(Left * sx, Top * sy, Width * sx, Height * sy), rtBox, Util.Sampling);

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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (ToggleMode) OnOff = !OnOff;
            base.OnMouseDown(e);
        }
        #endregion

        #region Method
        #region vars
        (IcFolder? ip, SKImage? on, SKImage? off, SKColor cBack) vars()
        {
            var thm = GoThemeW.Current;
            IcFolder? ip = null;
            SKImage? on = null, off = null;
            SKColor cBack = thm.Back;

            if (Parent is IcContainer con)
            {
                ip = IcResources.Get(con.ImageFolder);
                cBack = thm.ToColor(con.BackgroundColor);
                if (ip != null && ip.GetImage(con.OffImage)?.FirstOrDefault() is SKImage ioff && ip.GetImage(con.OnImage)?.FirstOrDefault() is SKImage ion) { on = ion; off = ioff; }
            }
            else if (Parent is TabPage tab && tab.Parent is IcCanvas ic)
            {
                ip = IcResources.Get(ic.ImageFolder);
                cBack = thm.ToColor(ic.BackgroundColor);
                var nm = ic.TabPageImage.FirstOrDefault(x => x.TabPage == ic.SelectedTab);
                if (ip != null && nm != null && ip.GetImage(nm.OffImage)?.FirstOrDefault() is SKImage ioff && ip.GetImage(nm.OnImage)?.FirstOrDefault() is SKImage ion) { on = ion; off = ioff; }
            }

            return (ip, on, off, cBack);
        }
        #endregion
        #endregion
    }
}
