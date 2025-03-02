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
    public class IcOnOff : GoControl
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

        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value)
                {
                    bOnOff = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool ToggleMode { get; set; } = false;
        #endregion

        #region Member Variable
        private bool bOnOff = false;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rts = Areas();
            var rtBox = rts["Content"];
            var cText = thm.ToColor(TextColor);

            var (ip, img, cBack) = vars();
            if (ip != null && img != null && Parent != null && img.On != null && img.Off != null)
            {
                var sx = (double)img.On.Width / Parent.Bounds.Width;
                var sy = (double)img.On.Height / Parent.Bounds.Height;
                canvas.DrawBitmap(OnOff ? img.On : img.Off, Util.FromRect(Convert.ToInt16(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox);

                Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, GoContentAlignment.MiddleCenter);
            }

            base.OnDraw(canvas);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];
            if (CollisionTool.Check(rtBox, x, y) && ToggleMode) OnOff = !OnOff;

            base.OnMouseDown(x, y, button);
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
