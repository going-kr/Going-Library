using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Forms.Tools;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoTextBox : GoControl
    {
        #region Properties
        public string BorderColor { get => sBorderColor; set { if (sBorderColor != value) { sBorderColor = value; Invalidate(); } } }
        public string BoxColor { get => sBoxColor; set { if (sBoxColor != value) { sBoxColor = value; Invalidate(); } } }
        public string TextColor { get => sTextColor; set { if (sTextColor != value) { sTextColor = value; Invalidate(); } } }
        public string TitleColor { get => sTitleColor; set { if (sTitleColor != value) { sTitleColor = value; Invalidate(); } } }
        public GoRoundType Round { get => eRound; set { if (eRound != value) { eRound = value; Invalidate(); } } }
        public bool BackgroundDraw { get => bBackgroundDraw; set { if (bBackgroundDraw != value) { bBackgroundDraw = value; Invalidate(); } } }
        public GoContentAlignment Alignment
        {
            get => align; set
            {
                if (align != value)
                {
                    align = value;
                    switch (align)
                    {
                        case GoContentAlignment.TopLeft:
                        case GoContentAlignment.MiddleLeft:
                        case GoContentAlignment.BottomLeft:
                            if (txt.TextAlign != HorizontalAlignment.Left)
                                txt.TextAlign = HorizontalAlignment.Left;
                            break;

                        case GoContentAlignment.TopCenter:
                        case GoContentAlignment.MiddleCenter:
                        case GoContentAlignment.BottomCenter:
                            if (txt.TextAlign != HorizontalAlignment.Center)
                                txt.TextAlign = HorizontalAlignment.Center;
                            break;

                        case GoContentAlignment.TopRight:
                        case GoContentAlignment.MiddleRight:
                        case GoContentAlignment.BottomRight:
                            if (txt.TextAlign != HorizontalAlignment.Right)
                                txt.TextAlign = HorizontalAlignment.Right;
                            break;
                    }
                    Invalidate();
                }
            }
        }

        public int ButtonWidth { get => nButtonWidth; set { if (nButtonWidth != value) { nButtonWidth = value; Invalidate(); } } }
        public int TitleWidth { get => nTitleWidth; set { if (nTitleWidth != value) { nTitleWidth = value; Invalidate(); } } }
        public override string Text { get => txt.Text; set { if (txt.Text != value) { txt.Text = base.Text = value; Invalidate(); } } }
        public bool UseButton { get => bUseButton; set { if (bUseButton != value) { bUseButton = value; Invalidate(); } } }
        public bool UseTitle { get => bUseTitle; set { if (bUseTitle != value) { bUseTitle = value; Invalidate(); } } }
        public string? ButtonIconString { get => sButtonIconString; set { if (sButtonIconString != value) { sButtonIconString = value; Invalidate(); } } }
        public float ButtonIconSize { get => nButtonIconSize; set { if (nButtonIconSize != value) { nButtonIconSize = value; Invalidate(); } } }
        public bool MultiLine { get => bMultiLine; set { if (bMultiLine != value) { txt.Multiline = bMultiLine = value; Invalidate(); } } }
        public GoPadding TextPadding { get => pad; set { if (pad != value) { pad = value; Invalidate(); } } }
        public string FontName { get => sFontName; set { if (sFontName != value) { sFontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => eFontStyle; set { if (eFontStyle != value) { eFontStyle = value; Invalidate(); } } }
        public float FontSize { get => nFontSize; set { if (nFontSize != value) { nFontSize = value; Invalidate(); } } }
        public string? Title { get => sTitle; set { if (sTitle != value) { sTitle = value; Invalidate(); } } }

        public TextBox OriginalTextBox => txt;
        #endregion

        #region Event
        public event EventHandler? ButtonClicked;
        #endregion

        #region Member Variable
        bool bBackgroundDraw = true;
        bool bUseButton = false, bUseTitle = false;
        bool bMultiLine = false;
        string? sButtonIconString = null;
        float nButtonIconSize = 14;
        string sBorderColor = "Base3";
        string sTextColor = "Fore";
        string sTitleColor = "Base5";
        GoContentAlignment align = GoContentAlignment.MiddleCenter;
        string sBoxColor = "Base1";
        GoRoundType eRound = GoRoundType.All;
        GoPadding pad = new GoPadding(10);
        int nButtonWidth = 30, nTitleWidth = 70;
        string sFontName = "나눔고딕";
        GoFontStyle eFontStyle = GoFontStyle.Normal;
        float nFontSize = 12;
        string? sTitle = null;

        bool bDown = false;
        TextBox txt;
        bool? isDark = null;
        #endregion

        #region Constructor
        public GoTextBox()
        {
            txt = new TextBox { Text = base.Text, TextAlign = HorizontalAlignment.Center, BorderStyle = BorderStyle.None };
            Controls.Add(txt);

            var thm = GoThemeW.Current;
            txt.BackColor = Util.FromArgb(thm.ToColor(BoxColor));
            txt.ForeColor = Util.FromArgb(thm.ToColor(TextColor));
        }
        #endregion

        #region Override
        protected override void OnHandleCreated(EventArgs e)
        {
            var frm = FindForm();
            if (frm != null) frm.FormClosed += (o, s) => isDark = null;
            base.OnHandleCreated(e);
        }

        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            var canvas = e.Canvas;
            var thm = GoThemeW.Current;
            var cBack = thm.ToColor(BackgroundColor);
            var cBox = thm.ToColor(BoxColor);
            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var cTitle = thm.ToColor(TitleColor);

            if (isDark != thm.Dark)
            {
                isDark = thm.Dark;
                DwmTool.SetDarkMode(txt.Handle, thm.Dark);
            }

            var (rtBox, rtTitle, rtText, rtButton) = bounds();
            if (BackgroundDraw) Util.DrawBox(canvas, rtBox, cBox, cBorder, Round, thm.Corner);

            if (UseTitle)
            {
                Util.DrawText(canvas, Title, FontName, FontStyle, FontSize, rtTitle, cTitle, GoContentAlignment.MiddleCenter);

                using var p = new SKPaint { };
                using var pe = SKPathEffect.CreateDash([1, 1], 1);
                p.Color = cBorder;
                p.StrokeWidth = 1;
                p.IsStroke = true;
                p.PathEffect = pe;
                canvas.DrawLine(rtTitle.Right, rtTitle.MidY - 5, rtTitle.Right, rtTitle.MidY + 5, p);
            }

            if (UseButton)
            {
                var mp = PointToClient(MousePosition);
                var vrt = rtButton;
                var b = CollisionTool.Check(vrt, mp.X, mp.Y);
                var c = cText.BrightnessTransmit(bDown ? thm.DownBrightness : 0).WithAlpha(Convert.ToByte(b ? 255 : 120));
                if (bDown) vrt.Offset(0, 1);
                Util.DrawIcon(canvas, ButtonIconString, ButtonIconSize, vrt, c);
            }

            var rt = MultiLine ? rtText : MathTool.MakeRectangle(rtText, new SKSize(rtText.Width - 20, txt.Height));
            txt.Bounds = new Rectangle(Convert.ToInt32(rt.Left), Convert.ToInt32(rt.Top), Convert.ToInt32(rt.Width), Convert.ToInt32(rt.Height));

            txt.Visible = txt.Enabled;
            if (!txt.Enabled) Util.DrawText(canvas, txt.Text, FontName, FontStyle, FontSize, rt, cText, Alignment);

            base.OnContentDraw(e);
        }

        #region Mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            var (rtBox, rtTitle, rtText, rtButton) = bounds();
            bDown = UseButton && CollisionTool.Check(rtButton, e.X, e.Y);
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var (rtBox, rtTitle, rtText, rtButton) = bounds();
            if (bDown)
            {
                if (UseButton && CollisionTool.Check(rtButton, e.X, e.Y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
                bDown = false;
            }
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #endregion

        #region Method
        (SKRect rtBox, SKRect rtTitle, SKRect rtText, SKRect rtButton) bounds()
        {
            SKRect rtBox = Util.FromRect(0, 0, Width - 1, Height - 1);

            var rto = Util.FromRect(rtBox, pad);

            var rts = Util.Columns(rto, [$"{(UseTitle ? TitleWidth : 0)}px", "100%", $"{(UseButton ? ButtonWidth : 0)}px"]);

            var rtTitle = rts[0];
            var rtText = rts[1];
            var rtButton = rts[2];

            return (rtBox, rtTitle, rtText, rtButton);
        }
        #endregion
    }

    public class GoValueBox : GoControl
    {
        #region Properties
        public string BorderColor { get => sBorderColor; set { if (sBorderColor != value) { sBorderColor = value; Invalidate(); } } }
        public string BoxColor { get => sBoxColor; set { if (sBoxColor != value) { sBoxColor = value; Invalidate(); } } }
        public string TextColor { get => sTextColor; set { if (sTextColor != value) { sTextColor = value; Invalidate(); } } }
        public string TitleColor { get => sTitleColor; set { if (sTitleColor != value) { sTitleColor = value; Invalidate(); } } }
        public GoRoundType Round { get => eRound; set { if (eRound != value) { eRound = value; Invalidate(); } } }
        public bool BackgroundDraw { get => bBackgroundDraw; set { if (bBackgroundDraw != value) { bBackgroundDraw = value; Invalidate(); } } }
        public GoContentAlignment Alignment
        {
            get => align; set
            {
                if (align != value)
                {
                    align = value;

                    Invalidate();
                }
            }
        }

        public int ButtonWidth { get => nButtonWidth; set { if (nButtonWidth != value) { nButtonWidth = value; Invalidate(); } } }
        public int TitleWidth { get => nTitleWidth; set { if (nTitleWidth != value) { nTitleWidth = value; Invalidate(); } } }
        public override string Text { get => base.Text; set { if (base.Text != value) { base.Text = value; Invalidate(); } } }
        public bool UseButton { get => bUseButton; set { if (bUseButton != value) { bUseButton = value; Invalidate(); } } }
        public bool UseTitle { get => bUseTitle; set { if (bUseTitle != value) { bUseTitle = value; Invalidate(); } } }
        public string? ButtonIconString { get => sButtonIconString; set { if (sButtonIconString != value) { sButtonIconString = value; Invalidate(); } } }
        public float ButtonIconSize { get => nButtonIconSize; set { if (nButtonIconSize != value) { nButtonIconSize = value; Invalidate(); } } }
        public GoPadding TextPadding { get => pad; set { if (pad != value) { pad = value; Invalidate(); } } }
        public string FontName { get => sFontName; set { if (sFontName != value) { sFontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => eFontStyle; set { if (eFontStyle != value) { eFontStyle = value; Invalidate(); } } }
        public float FontSize { get => nFontSize; set { if (nFontSize != value) { nFontSize = value; Invalidate(); } } }
        public string? Title { get => sTitle; set { if (sTitle != value) { sTitle = value; Invalidate(); } } }
        #endregion

        #region Event
        public event EventHandler? ButtonClicked;
        #endregion

        #region Member Variable
        bool bBackgroundDraw = true;
        bool bUseButton = false, bUseTitle = false;
        bool bMultiLine = false;
        string? sButtonIconString = null;
        float nButtonIconSize = 14;
        string sBorderColor = "Base3";
        string sTextColor = "Fore";
        string sTitleColor = "Base5";
        GoContentAlignment align = GoContentAlignment.MiddleCenter;
        string sBoxColor = "Base1";
        GoRoundType eRound = GoRoundType.All;
        GoPadding pad = new GoPadding(10);
        int nButtonWidth = 30, nTitleWidth = 70;
        string sFontName = "나눔고딕";
        GoFontStyle eFontStyle = GoFontStyle.Normal;
        float nFontSize = 12;
        string? sTitle = null;

        bool bDown = false;
        #endregion

        #region Constructor
        public GoValueBox()
        {
        }
        #endregion

        #region Override
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            var canvas = e.Canvas;
            var thm = GoThemeW.Current;
            var cBox = thm.ToColor(BoxColor);
            var cBorder = thm.ToColor(BorderColor);
            var cText = thm.ToColor(TextColor);
            var cTitle = thm.ToColor(TitleColor);

            var (rtBox, rtTitle, rtText, rtButton) = bounds();
            if (BackgroundDraw) Util.DrawBox(canvas, rtBox, cBox, cBorder, Round, thm.Corner);

            if (UseTitle)
            {
                Util.DrawText(canvas, Title, FontName, FontStyle, FontSize, rtTitle, cTitle, GoContentAlignment.MiddleCenter);

                using var p = new SKPaint { };
                using var pe = SKPathEffect.CreateDash([1, 1], 1);
                p.Color = cBorder;
                p.StrokeWidth = 1;
                p.IsStroke = true;
                p.PathEffect = pe;
                canvas.DrawLine(rtTitle.Right, rtTitle.MidY - 5, rtTitle.Right, rtTitle.MidY + 5, p);
            }

            if (UseButton)
            {
                var mp = PointToClient(MousePosition);
                var vrt = rtButton;
                var b = CollisionTool.Check(vrt, mp.X, mp.Y);
                var c = cText.BrightnessTransmit(bDown ? thm.DownBrightness : 0).WithAlpha(Convert.ToByte(b ? 255 : 120));
                if (bDown) vrt.Offset(0, 1);
                Util.DrawIcon(canvas, ButtonIconString, ButtonIconSize, vrt, c);
            }

            Util.DrawText(canvas, Text, FontName, FontStyle, FontSize, rtText, cText, Alignment);

            base.OnContentDraw(e);
        }

        #region Mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            var (rtBox, rtTitle, rtText, rtButton) = bounds();
            bDown = UseButton && CollisionTool.Check(rtButton, e.X, e.Y);
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var (rtBox, rtTitle, rtText, rtButton) = bounds();
            if (bDown)
            {
                if (UseButton && CollisionTool.Check(rtButton, e.X, e.Y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
                bDown = false;
            }
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Invalidate();
            base.OnMouseEnter(e);
        }
        #endregion
        #endregion

        #region Method
        (SKRect rtBox, SKRect rtTitle, SKRect rtText, SKRect rtButton) bounds()
        {
            SKRect rtBox = Util.FromRect(0, 0, Width - 1, Height - 1);

            var rto = Util.FromRect(rtBox, pad);

            var rts = Util.Columns(rto, [$"{(UseTitle ? TitleWidth : 0)}px", "100%", $"{(UseButton ? ButtonWidth : 0)}px"]);

            var rtTitle = rts[0];
            var rtText = rts[1];
            var rtButton = rts[2];

            return (rtBox, rtTitle, rtText, rtButton);
        }
        #endregion
    }
}
