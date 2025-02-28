using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Controls;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Going.UI.Forms.Containers
{
    public class GoTabControl : TabControl
    {
        #region Properties
        public GoDirectionHV IconDirection { get => eIconDirection; set { if (eIconDirection != value) { eIconDirection = value; Invalidate(); } } }
        public float IconSize { get => nIconSize; set { if (nIconSize != value) { nIconSize = value; Invalidate(); } } }
        public float IconGap { get => nIconGap; set { if (nIconGap != value) { nIconGap = value; Invalidate(); } } }

        public string FontName { get => sFontName; set { if (sFontName != value) { sFontName = value; Invalidate(); } } }
        public float FontSize { get => nFontSize; set { if (nFontSize != value) { nFontSize = value; Invalidate(); } } }

        public string TextColor { get => sTextColor; set { if (sTextColor != value) { sTextColor = value; Invalidate(); } } }
        public string TabColor { get => sTabColor; set { if (sTabColor != value) { sTabColor = value; Invalidate(); } } }
        public string TabBorderColor { get => sTabBorderColor; set { if (sTabBorderColor != value) { sTabBorderColor = value; Invalidate(); } } }

        public string BackgroundColor
        {
            get => sBackgroundColor;
            set
            {
                if (sBackgroundColor != value)
                {
                    sBackgroundColor = value;

                    var c = GoTheme.Current.ToColor(BackgroundColor);
                    this.BackColor = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);

                    Invalidate();
                }
            }
        }

        public Dictionary<string, string> TabIconString { get; } = new Dictionary<string, string>();
        #endregion

        #region Member Variable
        GoDirectionHV eIconDirection = GoDirectionHV.Horizon;
        float nIconSize = 12;
        float nIconGap = 5;

        string sFontName = "나눔고딕";
        float nFontSize = 12;

        string sTextColor = "Fore";
        string sTabColor = "Base2";
        string sTabBorderColor = "Base3";
        private string sBackgroundColor = "Back";
        #endregion

        #region Constructor
        public GoTabControl()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(120, 40);
            Alignment = TabAlignment.Top;
        }
        #endregion

        #region Override
        #region OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var bitmap = new SKBitmap(this.Width, this.Height))
            using (var canvas = new SKCanvas(bitmap))
            using (var surface = SKSurface.Create(bitmap.Info))
            {
                var cBack = GoTheme.Current.ToColor(BackgroundColor);
                canvas.Clear(cBack);

                using var p = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                var sp = canvas.SaveLayer(p);
                
                OnContentDraw(new ContentDrawEventArgs(canvas));
                
                canvas.RestoreToCount(sp);

                using (var bmp = bitmap.ToBitmap())
                    e.Graphics.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
            }
        }
        #endregion

        #region Draw
        protected virtual void OnContentDraw(ContentDrawEventArgs e)
        {
            var thm = GoTheme.Current;
            var canvas = e.Canvas;
            var cBack = thm.ToColor(BackgroundColor);
            var cText = thm.ToColor(TextColor);
            var cTab = thm.ToColor(TabColor);
            var cBorder = thm.ToColor(TabBorderColor);
            var vcTab = Enabled ? Util.FromArgb(cTab) : MixColorAlpha(Util.FromArgb(cBack), Util.FromArgb(cTab), 255 - GoTheme.DisableAlpha);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtNav = rts["Nav"];
            var rtPage = rts["Page"];

            using var p = new SKPaint { IsAntialias = true };

            foreach (var tp in TabPages.Cast<TabPage>())
                if (tp.BackColor != vcTab)
                    tp.BackColor = vcTab;

            Util.DrawBox(canvas, rtPage, cTab, cBorder, GoRoundType.All, thm.Corner);

            try
            {
                tabLoop((i, tab, rt) =>
                {
                    var iconString = TabIconString.TryGetValue(tab.Name, out var icon) ? icon : null;

                    var bSel = tab == SelectedTab;
                    var hover = CollisionTool.Check(rt, MousePosition.X, MousePosition.Y);
                    if (bSel)
                    {
                        #region retouch 1
                        p.IsStroke = false;
                        p.Color = cTab;
                        var TabPosition = dir(Alignment);
                        switch (TabPosition)
                        {
                            case GoDirection.Up: canvas.DrawRect(Util.FromRect(rt.Left - thm.Corner, rt.Bottom - 1, rt.Width + thm.Corner * 2, 2), p); break;
                            case GoDirection.Down: canvas.DrawRect(Util.FromRect(rt.Left - thm.Corner, rt.Top - 1, rt.Width + thm.Corner * 2, 2), p); break;
                            case GoDirection.Left: canvas.DrawRect(Util.FromRect(rt.Right - 1, rt.Top - thm.Corner, 2, rt.Height + thm.Corner * 2), p); break;
                            case GoDirection.Right: canvas.DrawRect(Util.FromRect(rt.Left - 1, rt.Top - thm.Corner, 2, rt.Height + thm.Corner * 2), p); break;
                        }
                        #endregion

                        #region tab
                        using var pth = PathTool.Tab(rt, TabPosition, thm.Corner);

                        p.IsStroke = false;
                        p.Color = cTab;
                        canvas.DrawPath(pth, p);

                        p.IsStroke = true;
                        p.Color = cBorder;
                        p.StrokeWidth = 1F;
                        canvas.DrawPath(pth, p);
                        #endregion

                        #region retouch 2
                        if (TabPages.Count > 0 && tab == TabPages[0])
                        {
                            var c2 = thm.Corner * 2;
                            switch (TabPosition)
                            {
                                case GoDirection.Up:
                                    {
                                        p.IsStroke = false; p.Color = cTab;
                                        canvas.DrawRect(Util.FromRect(rtPage.Left, rt.Bottom, c2, c2), p);

                                        var n = (int)rtPage.Left + 0.5F;
                                        p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                        canvas.DrawLine(n, rt.Bottom - c2, n, rt.Bottom + c2, p);
                                    }
                                    break;
                                case GoDirection.Down:
                                    {
                                        p.IsStroke = false; p.Color = cTab;
                                        canvas.DrawRect(Util.FromRect(rtPage.Left, rt.Top - c2, c2, c2), p);

                                        var n = (int)rtPage.Left + 0.5F;
                                        p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                        canvas.DrawLine(n, rt.Top + c2, n, rt.Top - c2, p);
                                    }
                                    break;
                                case GoDirection.Left:
                                    {
                                        p.IsStroke = false; p.Color = cTab;
                                        canvas.DrawRect(Util.FromRect(rt.Right, rtPage.Top, c2, c2), p);

                                        var n = (int)rtPage.Top + 0.5F;
                                        p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                        canvas.DrawLine(rt.Right - c2, n, rt.Right + c2, n, p);
                                    }
                                    break;
                                case GoDirection.Right:
                                    {
                                        p.IsStroke = false; p.Color = cTab;
                                        canvas.DrawRect(Util.FromRect(rt.Left - c2, rtPage.Top, c2, c2), p);

                                        var n = (int)rtPage.Top + 0.5F;
                                        p.IsStroke = true; p.StrokeWidth = 1; p.Color = cBorder;
                                        canvas.DrawLine(rt.Left + c2, n, rt.Left - c2, n, p);
                                    }
                                    break;
                            }
                        }
                        #endregion
                    }

                    Util.DrawTextIcon(canvas, tab.Text, FontName, FontSize, iconString, IconSize, IconDirection, IconGap, rt, Util.FromArgb(Convert.ToByte(bSel ? 255 : (hover ? 150 : 60)), cText));

                });
            }
            catch { }
        }
        #endregion

        #region OnEnabledChanged
        protected override void OnEnabledChanged(EventArgs e)
        {
            var thm = GoTheme.Current;
            var cBack = thm.ToColor(BackgroundColor);
            var cTab = thm.ToColor(TabColor);
            var vcTab = Enabled ? Util.FromArgb(cTab) : MixColorAlpha(Util.FromArgb(cBack), Util.FromArgb(cTab), 255 - GoTheme.DisableAlpha);

            foreach (var tp in TabPages.Cast<TabPage>())
                if (tp.BackColor != vcTab)
                    tp.BackColor = vcTab;

            Invalidate();
            base.OnEnabledChanged(e);
        }
        #endregion
        #endregion

        #region Method
        #region Areas
        protected Dictionary<string, SKRect> Areas()
        {
            var dic = new Dictionary<string, SKRect>();
            var rtContent = Util.FromRect(0, 0, this.Width - 1, this.Height - 1);
            var rtPage = rtContent;
            var rtNavi = new SKRect();

            if (TabPages.Count > 0)
            {
                var rt = GetTabRect(0);

                var offset = 2;
                var isz = rt.Height;

                switch (Alignment)
                {
                    case TabAlignment.Left:
                        isz = rt.Width;
                        rtNavi = Util.FromRect(rtContent.Left + offset, rtContent.Top, isz, rtContent.Height);
                        rtPage = Util.FromRect(rtNavi.Right, rtContent.Top, rtContent.Width - isz - offset, rtContent.Height);
                        break;
                    case TabAlignment.Top:
                        isz = rt.Height;
                        rtNavi = Util.FromRect(rtContent.Left, rtContent.Top + offset, rtContent.Width, isz);
                        rtPage = Util.FromRect(rtContent.Left, rtNavi.Bottom, rtContent.Width, rtContent.Height - isz - offset);
                        break;
                    case TabAlignment.Right:
                        isz = rt.Width;
                        rtNavi = Util.FromRect(rtContent.Right - offset - isz, rtContent.Top, isz, rtContent.Height);
                        rtPage = Util.FromRect(rtContent.Left, rtContent.Top, rtContent.Width - isz - offset, rtContent.Height);
                        break;
                    case TabAlignment.Bottom:
                        isz = rt.Height;
                        rtNavi = Util.FromRect(rtContent.Left, rtContent.Bottom - offset - isz, rtContent.Width, isz);
                        rtPage = Util.FromRect(rtContent.Left, rtContent.Top, rtContent.Width, rtContent.Height - isz - offset);
                        break;
                }
            }

            dic["Content"] = rtContent;
            dic["Nav"] = rtNavi;
            dic["Page"] = rtPage;

            return dic;
        }
        #endregion

        #region tabLoop
        void tabLoop(Action<int, TabPage, SKRect> loop)
        {
            for (int i = 0; i < TabPages.Count; i++)
            {
                var tab = TabPages[i];
                var rt = GetTabRect(i);
                if (i == 0) rt = new Rectangle(rt.Left - 2, rt.Top - 2, rt.Width + 2, rt.Height + 2);
                var vrt = Util.FromRect(rt); vrt.Offset(0.5F, 0.5F);
                loop(i, tab, vrt);
            }
        }
        #endregion

        #region dir
        GoDirection dir(TabAlignment align)
        {
            GoDirection ret = GoDirection.Up;
            switch (align)
            {
                case TabAlignment.Left: ret = GoDirection.Left; break;
                case TabAlignment.Right: ret = GoDirection.Right; break;
                case TabAlignment.Top: ret = GoDirection.Up; break;
                case TabAlignment.Bottom: ret = GoDirection.Down; break;
            }
            return ret;
        }
        #endregion

        #region MixColorAlpha
        Color MixColorAlpha(Color dest, Color src, int srcAlpha)
        {
            byte red = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.R, (int)src.R), 0.0, 255.0));
            byte green = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.G, (int)src.G), 0.0, 255.0));
            byte blue = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.B, (int)src.B), 0.0, 255.0));
            return Color.FromArgb(red, green, blue);
        }
        #endregion
        #endregion
    }
}
