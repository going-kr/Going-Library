﻿using Going.UI.Enums;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Controls
{
    public class GoOnOff : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public string OnText { get; set; } = "On";
        [GoProperty(PCategory.Control, 1)] public string OffText { get; set; } = "Off";
        [GoProperty(PCategory.Control, 2)] public string CursorIconString { get; set; } = "fa-power-off";
        [GoProperty(PCategory.Control, 3)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 4)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 5)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 6)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 7)] public string BoxColor { get; set; } = "Base1";
        [GoProperty(PCategory.Control, 8)] public string BorderColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 9)] public string CursorColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 10)] public string OnColor { get; set; } = "lime";
        [GoProperty(PCategory.Control, 11)] public string OffColor { get; set; } = "gray";

        private bool bOnOff = false;
        [GoProperty(PCategory.Control, 12)]
        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value)
                {
                    ani.Stop();
                    ani.Start(Animation.Time200, value ? "ON" : "OFF", () =>
                    {
                        bOnOff = value;
                        OnOffChanged?.Invoke(this, EventArgs.Empty);
                    });
                }
            }
        }
        #endregion

        #region Member Variable
        private bool bHover = false;
        private bool bDown = false;

        private Animation ani = new Animation();
        private bool downOnOff = false;
        private SKPoint? ptDown = null;
        private SKPoint? ptMove = null;
        #endregion

        #region Event
        public event EventHandler? OnOffChanged;
        #endregion

        #region Constructor
        public GoOnOff()
        {
            Selectable = true;
            ani.Refresh = () => Invalidate?.Invoke();
        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cBox = thm.ToColor(BoxColor);
            var cBorder = thm.ToColor(BorderColor);
            var cCursor = thm.ToColor(CursorColor);
            var cOn = thm.ToColor(OnColor);
            var cOff = thm.ToColor(OffColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtCursor = rts["Cursor"];
            var rtOn = rts["On"];
            var rtOff = rts["Off"];

            Util.DrawBox(canvas, rtContent, cBox, SKColors.Transparent, GoRoundType.All, rtContent.Height);

            var crt = rtContent; crt.Inflate(-2, -2);
            using var pth = PathTool.Box(crt, GoRoundType.All, rtContent.Height);
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipPath(pth);
                Util.DrawBox(canvas, rtCursor, cCursor.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0),
                                               cCursor.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), GoRoundType.All, rtContent.Height);

                Util.DrawText(canvas, OnText, FontName, FontStyle, FontSize, rtOn, cText);
                Util.DrawText(canvas, OffText, FontName, FontStyle, FontSize, rtOff, cText);

                Util.DrawIcon(canvas, CursorIconString, rtCursor.Height / 2, rtCursor, OnOff ? cOn : cOff);
            }

            Util.DrawBox(canvas, rtContent, SKColors.Transparent, cBorder, GoRoundType.All, rtContent.Height);
            base.OnDraw(canvas);
        }


        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtCursor = rts["Cursor"];
            var rtOn = rts["On"];
            var rtOff = rts["Off"];

            if (CollisionTool.Check(rtCursor, x, y))
            {
                downOnOff = OnOff;
                ptDown = ptMove = new SKPoint(x, y);
            }
            else if (CollisionTool.Check(rtOn, x, y)) OnOff = true;
            else if (CollisionTool.Check(rtOff, x, y)) OnOff = false;

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (ptDown.HasValue)
            {
                ptDown = ptMove = null;
                OnOff = x > rtContent.MidX;
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtCursor = rts["Cursor"];

            bHover = CollisionTool.Check(rtCursor, x, y);
            if (ptDown.HasValue)
            {
                ptMove = new SKPoint(x, y);
                bOnOff = x > rtContent.MidX;
            }

            base.OnMouseMove(x, y);
        }

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var CursorPadding = 5;
            var rtArea = Util.FromRect(rtContent); rtArea.Inflate(-CursorPadding, -CursorPadding);

            var cw = rtArea.Width * 0.5F;
            var ow = rtArea.Width * 0.5F;

            var sx = 0F;
            if (ptDown.HasValue && ptMove.HasValue)
            {
                var sv = downOnOff ? -ow : 0;
                var ev = downOnOff ? 0 : ow;
                sx = (downOnOff ? rtArea.Left : rtArea.Left - ow) + MathTool.Constrain(ptMove.Value.X - ptDown.Value.X, sv, ev);
            }
            else
            {
                if (GoTheme.Current.Animation && ani.IsPlaying)
                {
                    var sv = ani.Variable == "ON" ? -ow : 0;
                    var ev = ani.Variable == "ON" ? 0 : -ow;

                    sx = rtArea.Left + ani.Value(AnimationAccel.Linear, sv, ev);
                }
                else sx = OnOff ? rtArea.Left : rtArea.Left - ow;
            }

            var rtOff = Util.FromRect(sx, rtArea.Top, ow, rtArea.Height);
            var rtCursor = Util.FromRect(rtOff.Right, rtArea.Top, cw, rtArea.Height);
            var rtOn = Util.FromRect(rtCursor.Right, rtArea.Top, ow, rtArea.Height);

            rts["On"] = rtOn;
            rts["Off"] = rtOff;
            rts["Cursor"] = rtCursor;

            return rts;
        }
        #endregion
        #endregion
    }
}
