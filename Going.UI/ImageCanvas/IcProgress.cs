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
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.ImageCanvas
{
    public class IcProgress : GoControl
    {
        #region Properties
        [GoFontNameProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 3)] public string TextColor { get; set; } = "Black";

        [GoImageProperty(PCategory.Control, 4)] public string? OnBarImage { get; set; }
        [GoImageProperty(PCategory.Control, 5)] public string? OffBarImage { get; set; }

        [GoProperty(PCategory.Control, 6)] public string FormatString { get; set; } = "0";
        [GoProperty(PCategory.Control, 7)] public double Minimum { get; set; } = 0D;
        [GoProperty(PCategory.Control, 8)] public double Maximum { get; set; } = 100D;
        [GoProperty(PCategory.Control, 9)]
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

        [GoProperty(PCategory.Control, 10)] public bool DrawText { get; set; } = true;
        #endregion

        #region Member Variable
        private double nValue = 0;
        #endregion

        #region Event
        public event EventHandler? ValueChanged;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtBox = rts["Content"];
            var cText = thm.ToColor(TextColor);

            if (Design != null && Parent != null)
            {
                if (OnBarImage == null && OffBarImage == null)
                {
                    SKImage? offB = null, onB = null;
                    if (Parent is IcContainer con) { offB = Design.GetImage(con.OffImage)?.FirstOrDefault(); onB = Design.GetImage(con.OnImage).FirstOrDefault(); }
                    else if (Parent is IcPage page) { offB = Design.GetImage(page.OffImage).FirstOrDefault(); onB = Design.GetImage(page.OnImage).FirstOrDefault(); }

                    if (onB != null && offB != null)
                    {
                        var sx = (double)onB.Width / Parent.Bounds.Width;
                        var sy = (double)onB.Height / Parent.Bounds.Height;

                        if (offB != null && onB != null)
                        {
                            var vbBS = Util.FromRect(Convert.ToInt32(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy));
                            var vbBD = Util.FromRect(0, 0, Width, Height);

                            var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, Width));
                            var vbFS = Util.FromRect(Convert.ToInt32(Left * sx), Convert.ToInt32(Top * sy), Convert.ToSingle(w * sx), Convert.ToSingle(Height * sy));
                            var vbFD = Util.FromRect(0, 0, w, Height);

                            canvas.DrawImage(offB, vbBS, vbBD, Util.Sampling);
                            if (Value > Minimum) canvas.DrawImage(onB, vbFS, vbFD, Util.Sampling);
                            if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, vbBD, cText);

                        }
                    }
                }
                else
                {
                    var offB = Design.GetImage(OffBarImage)?.FirstOrDefault();
                    var onB = Design.GetImage(OnBarImage)?.FirstOrDefault();

                    if (offB != null && onB != null)
                    {
                        bounds(offB, (_, rtBar, rtFill) =>
                        {
                            canvas.DrawImage(offB, rtBar, Util.Sampling);
                            if (Value > Minimum) canvas.DrawImage(onB, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill, Util.Sampling);
                            if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtBar, cText);
                        });
                    }
                }

                
            }

            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Method
        #region bounds
        void bounds(SKImage? bmBar, Action<SKRect, SKRect, SKRect> act)
        {
            if (Design != null)
            {
                var rtBox = Util.FromRect(0, 0, Width, Height);
                var rtBar = bmBar != null ? MathTool.MakeRectangle(rtBox, new SKSize(bmBar.Width, bmBar.Height)) : rtBox;

                var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, rtBar.Width));
                var rtFill = Util.FromRect(rtBar.Left, rtBar.Top, w, rtBar.Height);

                act(rtBox, rtBar, rtFill);
            }
        }
        #endregion
        #endregion
    }
}
