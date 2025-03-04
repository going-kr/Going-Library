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
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    public class IcButton : GoControl
    {
        #region Properties
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; } = GoDirectionHV.Horizon;
        public float IconGap { get; set; } = 5;
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;
        public string TextColor { get; set; } = "Black";
        #endregion

        #region Member Variable
        private bool bDown = false;
        #endregion

        #region Event
        public event EventHandler? ButtonClicked;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rts = Areas();
            var rtBox = rts["Content"];
            var cText = thm.ToColor(TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);

            var (ip, img, cBack) = vars();
            if (ip != null && img != null && Parent != null && img.On != null && img.Off != null)
            {
                var sx = (double)img.On.Width / Parent.Bounds.Width;
                var sy = (double)img.On.Height / Parent.Bounds.Height;
                canvas.DrawBitmap(bDown ? img.On : img.Off, Util.FromRect(Convert.ToInt16(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);

                if (bDown) rtBox.Offset(0, 1);
                Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, GoContentAlignment.MiddleCenter);
            }

            base.OnDraw(canvas);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];
            if (CollisionTool.Check(rtBox, x, y)) bDown = true;

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];
           if (bDown)
            {
                bDown = false;
                if (CollisionTool.Check(rtBox, x, y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseUp(x, y, button);
        }
        #endregion

        #region Method
        #region vars
        (IcImageFolder? ip, IcOnOffImage? img, SKColor cBack) vars()
        {
            var thm = GoTheme.Current;
            SKColor cBack = thm.Back;
            IcImageFolder? ip = null;
            IcOnOffImage? img = null;

            if (Design != null)
            {
                ip = Design.GetIC();
                if (Parent is IcContainer con)
                {
                    cBack = thm.ToColor(con.BackgroundColor);
                    img = ip != null && con.ContainerImage != null && ip.Containers.TryGetValue(con.ContainerImage, out var v) ? v : null;
                }
                else if (Parent is IcPage page)
                {
                    cBack = thm.ToColor(page.BackgroundColor);
                    img = ip != null && page.Name != null && ip.Pages.TryGetValue(page.Name, out var v) ? v : null;
                }
            }

            return (ip, img, cBack);
        }
        #endregion
        #endregion
    }
}
