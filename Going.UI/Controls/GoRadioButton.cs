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
    public class GoRadioButton : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 2)] public GoDirectionHV IconDirection { get; set; }
        [GoProperty(PCategory.Control, 3)] public float IconGap { get; set; } = 5;
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "button";
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 7)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 8)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 9)] public string ButtonColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 10)] public string CheckedButtonColor { get; set; } = "Select";
        [GoProperty(PCategory.Control, 11)] public GoRoundType Round { get; set; } = GoRoundType.All;

        private bool bCheck = false;
        [GoProperty(PCategory.Control, 12)]
        public bool Checked
        {
            get => bCheck; set
            {
                if (bCheck != value)
                {
                    bCheck = value;

                    if (Parent != null && bCheck)
                    {
                        var ls = Parent.Childrens.Where(x => x is GoRadioButton && x != this).Select(x => x as GoRadioButton);
                        foreach (var c in ls)
                            if (c != null) c.Checked = false;
                    }

                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Event
        public event EventHandler? ButtonClicked;
        public event EventHandler? CheckedChanged;
        #endregion

        #region Constructor
        public GoRadioButton()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        private bool bHover = false;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cBtn = thm.ToColor(Checked ? CheckedButtonColor : ButtonColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var rts = Areas();
            var rtBox = rts["Content"];

            Util.DrawBox(canvas, rtBox, cBtn.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cBtn.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), Round, thm.Corner);

            if (bDown) rtBox.Offset(0, 1);
            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, GoContentAlignment.MiddleCenter);

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
                if (CollisionTool.Check(rtBox, x, y))
                {
                    Checked = true;
                    ButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            bHover = CollisionTool.Check(rtBox, x, y);

            base.OnMouseMove(x, y);
        }
        #endregion
    }
}
