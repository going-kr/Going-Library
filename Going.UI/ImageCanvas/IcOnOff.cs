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
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 2)] public GoDirectionHV IconDirection { get; set; } = GoDirectionHV.Horizon;
        [GoProperty(PCategory.Control, 3)] public float IconGap { get; set; } = 5;
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "onoff";
        [GoProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 7)] public float FontSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 8)] public string TextColor { get; set; } = "Black";
        [GoProperty(PCategory.Control, 9)]
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

        [GoProperty(PCategory.Control, 10)] public bool ToggleMode { get; set; } = false;
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

            if (Design != null && Parent != null)
            {
                List<SKImage>? offs = null, ons = null;
                if (Parent is IcContainer con) { offs = Design.GetImage(con.OffImage); ons = Design.GetImage(con.OffImage); }
                else if (Parent is IcPage page) { offs = Design.GetImage(page.OffImage); ons = Design.GetImage(page.OffImage); }

                if (offs?.Count > 0 && ons?.Count > 0)
                {
                    var off = offs.First();
                    var on = ons.First();

                    var sx = (double)on.Width / Parent.Bounds.Width;
                    var sy = (double)on.Height / Parent.Bounds.Height;
                    canvas.DrawImage(OnOff ? on : off, Util.FromRect(Convert.ToInt16(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox, Util.Sampling);

                    Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, GoContentAlignment.MiddleCenter);
                }
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
    }
}
