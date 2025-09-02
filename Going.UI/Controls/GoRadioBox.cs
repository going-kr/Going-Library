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

namespace Going.UI.Controls
{
    public class GoRadioBox : GoControl
    {
        #region Properties
        [GoMultiLineProperty(PCategory.Control, 0)] public string Text { get; set; } = "radiobox";
        [GoFontNameProperty(PCategory.Control, 1)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 2)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 3)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 5)] public string BoxColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control, 6)] public string CheckColor { get; set; } = "Fore";

        private bool bCheck = false;
        [GoProperty(PCategory.Control, 7)]
        public bool Checked
        {
            get => bCheck; set
            {
                if (bCheck != value)
                {
                    bCheck = value;

                    if (Parent != null  && bCheck)
                    {
                        var ls = Parent.Childrens.Where(x => x is GoRadioBox && x != this).Select(x => x as GoRadioBox);
                        foreach (var c in ls)
                            if (c != null) c.Checked = false;
                    }

                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        [GoProperty(PCategory.Control, 8)] public int BoxSize { get; set; } = 24;
        [GoProperty(PCategory.Control, 9)] public int Gap { get; set; } = 10;
        [GoProperty(PCategory.Control,10)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        #endregion

        #region Event
        public event EventHandler? CheckedChanged;
        #endregion

        #region Constructor
        public GoRadioBox()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bHover = false;
        private bool bDown = false;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cBox = thm.ToColor(BoxColor);
            var cChk = thm.ToColor(CheckColor);
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtText = rts["Text"];

            Util.DrawBox(canvas, rtBox, cBox.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cBox.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), GoRoundType.Ellipse, thm.Corner);

            var sz = BoxSize * 0.45F;
            if (Checked) Util.DrawBox(canvas, MathTool.MakeRectangle(rtBox, new SKSize(sz, sz)), cText, GoRoundType.Ellipse, thm.Corner);

            Util.DrawText(canvas, Text, FontName, FontStyle, FontSize, rtText, cText, GoContentAlignment.MiddleCenter);

            base.OnDraw(canvas, thm);
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
                if (CollisionTool.Check(rtBox, x, y))
                {
                    Checked = true;
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            bHover = CollisionTool.Check(rtContent, x, y);


            base.OnMouseMove(x, y);
        }

        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var (rtBox, rtText) = Util.TextIconBounds(Text, FontName, FontStyle, FontSize, new SKSize(BoxSize, BoxSize), GoDirectionHV.Horizon, Gap, rtContent, ContentAlignment);

            rts["Box"] = rtBox;
            rts["Text"] = rtText;

            return rts;
        }
        #endregion
    }
}
