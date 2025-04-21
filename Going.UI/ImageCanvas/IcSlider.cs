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
        [GoProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 3)] public string TextColor { get; set; } = "Black";

        [GoProperty(PCategory.Control, 4)] public string? OffBarImage { get; set; }
        [GoProperty(PCategory.Control, 5)] public string? OnBarImage { get; set; }
        [GoProperty(PCategory.Control, 6)] public string? OffCursorImage { get; set; }
        [GoProperty(PCategory.Control, 7)] public string? OnCursorImage { get; set; }

        [GoProperty(PCategory.Control, 8)] public string FormatString { get; set; } = "0";
        [GoProperty(PCategory.Control, 9)] public double Minimum { get; set; } = 0D;
        [GoProperty(PCategory.Control, 10)] public double Maximum { get; set; } = 100D;
        [GoProperty(PCategory.Control, 11)]
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

        [GoProperty(PCategory.Control, 12)] public double? Tick { get; set; }

        [GoProperty(PCategory.Control, 13)] public bool DrawText { get; set; } = true;
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

            if (Design != null && Parent != null)
            {
                var offB = Design.GetImage(OffBarImage).FirstOrDefault();
                var onB = Design.GetImage(OnBarImage).FirstOrDefault();
                var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                if (offB != null && onB != null && offC != null && onC != null)
                {
                    bounds(offB, offC, (_, rtBar, rtCur, rtFill) =>
                    {
                        canvas.DrawImage(offB, rtBar, Util.Sampling);
                        if (Value > Minimum) canvas.DrawImage(onB, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill, Util.Sampling);
                        canvas.DrawImage(bCurDown ? onC : offC, rtCur, Util.Sampling);
                        if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtCur, cText);
                    });
                }
            }

            base.OnDraw(canvas);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            if (Design != null && Parent != null)
            {
                var offB = Design.GetImage(OffBarImage).FirstOrDefault();
                var onB = Design.GetImage(OnBarImage).FirstOrDefault();
                var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                if (offB != null && onB != null && offC != null && onC != null)
                {
                    bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                    {
                        if (CollisionTool.Check(rtCur, x, y)) bCurDown = true;
                    });
                }
            }
            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            if (bCurDown)
            {
                if (Design != null && Parent != null)
                {
                    var offB = Design.GetImage(OffBarImage).FirstOrDefault();
                    var onB = Design.GetImage(OnBarImage).FirstOrDefault();
                    var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                    var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                    if (offB != null && onB != null && offC != null && onC != null)
                    {
                        bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                        {
                            Value = MathTool.Map(MathTool.Constrain(x, rtBar.Left, rtBar.Right), rtBar.Left, rtBar.Right, Minimum, Maximum);
                        });
                    }
                }
             
                bCurDown = false;
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            if (bCurDown)
            {
                if (Design != null && Parent != null)
                {
                    var offB = Design.GetImage(OffBarImage).FirstOrDefault();
                    var onB = Design.GetImage(OnBarImage).FirstOrDefault();
                    var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                    var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                    if (offB != null && onB != null && offC != null && onC != null)
                    {
                        bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                        {
                            Value = MathTool.Map(MathTool.Constrain(x, rtBar.Left, rtBar.Right), rtBar.Left, rtBar.Right, Minimum, Maximum);
                        });
                    }
                }
            }
            base.OnMouseMove(x, y);
        }

        #endregion

        #region Method
        #region bounds
        void bounds(SKImage? bmBar, SKImage bmCur, Action<SKRect, SKRect, SKRect, SKRect> act)
        {
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var rtBar = bmBar != null ? MathTool.MakeRectangle(rtBox, new SKSize(bmBar.Width, bmBar.Height)) : rtBox;

            var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, rtBar.Width));
            var rtFill = Util.FromRect(rtBar.Left, rtBar.Top, w, rtBar.Height);

            var x = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, rtBar.Left, rtBar.Right));
            var y = rtBar.MidY;
            var rtCur = MathTool.MakeRectangle(new SKPoint(x, y), bmCur.Width / 2F, bmCur.Height / 2F);
            act(rtBox, rtBar, rtCur, rtFill);
        }
        #endregion
        #endregion
    }
}
