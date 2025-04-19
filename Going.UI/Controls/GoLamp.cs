﻿using Going.UI.Enums;
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
    public class GoLamp : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Misc, 0)] public string Text { get; set; } = "label";
        [GoProperty(PCategory.Misc, 1)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Misc, 2)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Misc, 3)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Misc, 4)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Misc, 5)] public string OnColor { get; set; } = "Good";
        [GoProperty(PCategory.Misc, 6)] public string OffColor { get; set; } = "Base2";

        private bool bOnOff = false;
        [GoProperty(PCategory.Misc, 7)]
        public bool OnOff
        {
            get => bOnOff; set
            {
                if (bOnOff != value)
                {
                    bOnOff = value;
                    OnOffChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [GoProperty(PCategory.Misc, 8)] public int LampSize { get; set; } = 24;
        [GoProperty(PCategory.Misc, 9)] public int Gap { get; set; } = 10;
        [GoProperty(PCategory.Misc, 10)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        #endregion

        #region Event
        public event EventHandler? OnOffChanged;
        #endregion

        #region Constructor
        public GoLamp()
        {
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cOff = thm.ToColor(OffColor);
            var cOn = thm.ToColor(OnColor);
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtText = rts["Text"];

            Util.DrawLamp(canvas, rtBox, cOn, cOff, OnOff);

            Util.DrawText(canvas, Text, FontName, FontStyle, FontSize, rtText, cText, GoContentAlignment.MiddleCenter);

            base.OnDraw(canvas);
        }
         
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var (rtBox, rtText) = Util.TextIconBounds(Text, FontName, FontStyle, FontSize, new SKSize(LampSize, LampSize), GoDirectionHV.Horizon, Gap, rtContent, ContentAlignment);

            rts["Box"] = rtBox;
            rts["Text"] = rtText;

            return rts;
        }
        #endregion
    }
}
