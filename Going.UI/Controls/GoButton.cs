using Going.UI.Datas;
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
    public class GoButton : GoControl
    {
        #region Properties
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; }
        public float IconGap { get; set; } = 5;
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string ButtonColor { get; set; } = "Base3";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; } = false;
        #endregion

        #region Event
        public event EventHandler? ButtonClicked;
        #endregion

        #region Constructor
        public GoButton()
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
            var cBtn = thm.ToColor(ButtonColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var rts = Areas();
            var rtBox = rts["Content"];

            if (BackgroundDraw) Util.DrawBox(canvas, rtBox, (BorderOnly ? SKColors.Transparent : cBtn.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0)), cBtn.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), Round, thm.Corner);

            if (bDown) rtBox.Offset(0, 1);
            Util.DrawTextIcon(canvas, Text, FontName, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, GoContentAlignment.MiddleCenter);

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
