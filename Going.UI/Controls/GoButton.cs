﻿using Going.UI.Datas;
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoButton : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Misc, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Misc, 1)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Misc, 2)] public GoDirectionHV IconDirection { get; set; }
        [GoProperty(PCategory.Misc, 3)] public float IconGap { get; set; } = 5;
        [GoProperty(PCategory.Misc, 4)] public string Text { get; set; } = "button";
        [GoProperty(PCategory.Misc, 5)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Misc, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Misc, 7)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Misc, 8)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Misc, 9)] public string ButtonColor { get; set; } = "Base3";
        [GoProperty(PCategory.Misc, 10)] public GoRoundType Round { get; set; } = GoRoundType.All;

        [GoProperty(PCategory.Misc, 11)] public bool BackgroundDraw { get; set; } = true;
        [GoProperty(PCategory.Misc, 12)] public bool BorderOnly { get; set; } = false;
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
