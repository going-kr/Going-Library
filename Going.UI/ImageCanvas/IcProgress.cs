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

namespace Going.UI.ImageCanvas
{
    public class IcProgress : GoControl
    {
        #region Properties
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;
        public string TextColor { get; set; } = "Black";

        public string? BarImage { get; set; }

        public string FormatString { get; set; } = "0";
        public double Minimum { get; set; } = 0D;
        public double Maximum { get; set; } = 100D;
        public double Value
        {
            get => nValue;
            set
            {
                if (nValue != value)
                {
                    nValue = MathTool.Constrain(value, Minimum, Maximum);
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool DrawText { get; set; } = true;
        #endregion

        #region Member Variable
        private double nValue = 0;
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
            bounds(ip, (_, rtBar, rtFill, bmBar) =>
            {
                canvas.DrawBitmap(bmBar.Off, rtBar);
                if (Value > Minimum) canvas.DrawBitmap(bmBar.On, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill);
                if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtBar, cText);
            });


            base.OnDraw(canvas);
        }
         
        #endregion

        #region Method
        #region bounds
        void bounds(IcImageFolder? ip, Action<SKRect, SKRect, SKRect, IcOnOffImage> act)
        {
            if (ip != null && BarImage != null && ip.Miscs.TryGetValue(BarImage, out var bmBar) && bmBar.On != null && bmBar.Off != null)
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
