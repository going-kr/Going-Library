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
    public class IcSlider : GoControl
    {
        #region Properties
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;
        public string TextColor { get; set; } = "Black";

        public string? BarImage { get; set; }
        public string? CursorImage { get; set; }

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
                    if (Tick.HasValue) nValue = Math.Round(nValue / Tick.Value) * Tick.Value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double? Tick { get; set; }

        public bool DrawText { get; set; } = true;
        #endregion

        #region Member Variable
        private double nValue = 0;
        private bool bCurDown = false;
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
            bounds(ip, (_, rtBar, rtCur, rtFill, bmBar, bmCur) =>
            {
                canvas.DrawBitmap(bmBar.Off, rtBar);
                if (Value > Minimum) canvas.DrawBitmap(bmBar.On, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill);
                canvas.DrawBitmap(bCurDown ? bmCur.On : bmCur.Off, rtCur);
                if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtCur, cText);
            });

            base.OnDraw(canvas);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var (ip, img, cBack) = vars();

            bounds(ip, (rtBox, rtBar, rtCur, rtFill, bmBar, bmCur) =>
            {
                if (CollisionTool.Check(rtCur, x, y)) bCurDown = true;
            });

            base.OnMouseDown(x, y, button);
        }
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            if (bCurDown)
            {
                var (ip, img, cBack) = vars();

                bounds(ip, (rtBox, rtBar, rtCur, rtFill, bmBar, bmCur) =>
                {
                    Value = MathTool.Map(MathTool.Constrain(x, rtBar.Left, rtBar.Right), rtBar.Left, rtBar.Right, Minimum, Maximum);
                });

                bCurDown = false;
            }

            base.OnMouseUp(x, y, button);
        }
        protected override void OnMouseMove(float x, float y)
        {
            if (bCurDown)
            {
                var (ip, img, cBack) = vars();

                bounds(ip, (rtBox, rtBar, rtCur, rtFill, bmBar, bmCur) =>
                {
                    Value = MathTool.Map(MathTool.Constrain(x, rtBar.Left, rtBar.Right), rtBar.Left, rtBar.Right, Minimum, Maximum);
                });
            }
            base.OnMouseMove(x, y);
        }

        #endregion

        #region Method
        #region bounds
        void bounds(IcImageFolder? ip, Action<SKRect, SKRect, SKRect, SKRect, IcOnOffImage, IcOnOffImage> act)
        {
            if (ip != null && BarImage != null && CursorImage != null &&
                ip.Miscs.TryGetValue(CursorImage, out var bmCur) && ip.Miscs.TryGetValue(BarImage, out var bmBar) &&
                bmBar.On != null && bmBar.Off != null && bmCur.On != null && bmCur.Off != null)
            {
                var rtBox = Util.FromRect(0, 0, Width, Height);
                var rtBar = MathTool.MakeRectangle(rtBox, new SKSize(bmBar.Off.Width, bmBar.Off.Height));

                var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, rtBar.Width));
                var rtFill = Util.FromRect(rtBar.Left, rtBar.Top, w, rtBar.Height);

                var x = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, rtBar.Left, rtBar.Right));
                var y = rtBar.MidY;
                var rtCur = MathTool.MakeRectangle(new SKPoint(x, y), bmCur.Off.Width / 2F, bmCur.Off.Height / 2F);
                act(rtBox, rtBar, rtCur, rtFill, bmBar, bmCur);
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
