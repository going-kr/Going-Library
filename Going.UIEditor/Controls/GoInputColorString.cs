using Going.UI.Enums;
using Going.UI.Forms;
using Going.UI.Forms.Controls;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using OpenTK.Windowing.Common;
using SkiaSharp;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.StartScreen;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using Application = System.Windows.Forms.Application;
using GoControl = Going.UI.Forms.Controls.GoControl;

namespace Going.UIEditor.Controls
{
    public class GoInputColorString : GoControl, IMessageFilter
    {
        #region Properties
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public GoDirectionHV Direction { get; set; } = GoDirectionHV.Horizon;
        public string TextColor { get; set; } = "Fore";
        public string BorderColor { get; set; } = "Base3";
        public string FillColor { get; set; } = "Base3";
        public string ValueColor { get; set; } = "Base1";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool Valid => txt.Visible ? ToColor(txt.Text).HasValue : true;

        public SKColor Value
        {
            get => val; 
            set
            {
                if (val != value)
                {
                    val = value;
                    Invalidate();
                }
            }
        }
        #endregion

        #region Member Variable
        SKColor val = SKColors.White;
        TextBox txt;
        #endregion

        #region Constructor
        public GoInputColorString()
        {
            Application.AddMessageFilter(this);

            txt = new TextBox { Multiline = false, BorderStyle = BorderStyle.None, Visible = false, TextAlign = HorizontalAlignment.Center };
            txt.AcceptsReturn = true;
            txt.KeyDown += (o, s) =>
            {
                if (s.KeyCode == Keys.Return)
                {
                    var c = ToColor(txt.Text);
                    if (c.HasValue) Value = c.Value;
                    txt.Visible = false;
                }
                else if (s.KeyCode == Keys.Escape) txt.Visible = false;
                Invalidate();
            };
            txt.LostFocus += (o, s) => txt.Visible = false; 
            txt.GotFocus += (o, s) => Invalidate();
            txt.VisibleChanged += (o, s) => Invalidate();
            Controls.Add(txt);
        }
        #endregion

        #region PreFilterMessage
        public bool PreFilterMessage(ref Message m)
        {
            var cc = Control.FromHandle(m.HWnd);
            if (!IsDisposed)
            {
                int WM_LBUTTONDOWN = 0x0201;
                int WM_RBUTTONDOWN = 0x0204;

                // 해당 마우스 이벤트 메세지를 내가 먼저 볼 수 있게 만드는 객체 : IMessageFilter, Message 객체로 후킹 가능
                // 여기서 Window Message = WM이고 LBUTTONDOWN, RBUTTONDOWN은 마우스 이벤트 메세지로 이벤트가 발생하면 해당 메세지가 발생
                if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN)
                {
                    var rts = Areas();
                    var rtValue = rts["Value"];

                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;

                    var ptS = cc != null ? cc.PointToScreen(new Point(x, y)) : new Point(x, y);
                    var ptC = PointToClient(ptS);

                    if (!CollisionTool.Check(rtValue, ptC.X, ptC.Y))
                    {
                        txt.Visible = false;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            var thm = GoThemeW.Current;
            var canvas = e.Canvas;
            using var p = new SKPaint { IsAntialias = false };

            #region var
            #region color
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var cFill = thm.ToColor(FillColor);
            var cValue = thm.ToColor(ValueColor);
            var cInput = !Valid ? thm.Error : thm.Highlight;
            #endregion
            #region bounds
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtValue = rts["Value"];
            var rtColor = rts["Color"];
            #endregion
            #region round
            var rndColor = GoRoundType.L;
            var rndValue = GoRoundType.R;
            #endregion
            #endregion

            #region Color
            Util.DrawBox(canvas, rtColor, cValue, rndColor, thm.Corner);

            var isz = Height / 3;
            var rtIco = MathTool.MakeRectangle(rtColor, new SKSize(isz, isz));

            Util.DrawBox(canvas, rtIco, Value, GoRoundType.Rect, thm.Corner);
            #endregion

            #region Value
            Util.DrawBox(canvas, rtValue, cValue, rndValue, thm.Corner);

            if (!txt.Visible)
            {
                var v = Value;
                var text = v.Alpha != 255 ? $"#{v.Alpha:X2}{v.Red:X2}{v.Green:X2}{v.Blue:X2}" : $"#{v.Red:X2}{v.Green:X2}{v.Blue:X2}";
                Util.DrawText(canvas, text, FontName, FontStyle, FontSize, rtValue, cText);
            }
            #endregion

            #region sep
            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cBorder);
            p.PathEffect = pe;
            canvas.DrawLine(rtValue.Left, rtValue.Top + 10, rtValue.Left, rtValue.Bottom - 10, p);
            #endregion

            #region Border
            Util.DrawBox(canvas, rtBox, SKColors.Transparent, txt.Visible ? cInput : cBorder, Round, thm.Corner);
            #endregion

            base.OnContentDraw(e);
        }
        #endregion
        #region OnGotFocus
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }
        #endregion
        #region OnLostFocus
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }
        #endregion
        #region OnMouseClick
        protected override void OnMouseClick(MouseEventArgs e)
        {
            var thm = GoThemeW.Current;

            #region var
            #region bounds
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtValue = rts["Value"];
            var rtColor = rts["Color"];
            #endregion
            #endregion

            if(!txt.Visible && CollisionTool.Check(rtValue, e.X,e.Y))
            {
                var x = Convert.ToInt32(rtValue.Left + 5);
                var y = Convert.ToInt32(rtValue.Top + 5);
                var w = Convert.ToInt32(rtValue.Width - 10);
                var h = Convert.ToInt32(rtValue.Height - 10);
                var v = Value;

                txt.Bounds = new Rectangle(x, y, w, h);
                h = txt.Height;

                txt.Top = Convert.ToInt32(rtValue.Height / 2F - h / 2F);
                txt.BackColor = Util.FromArgb(thm.ToColor(ValueColor));
                txt.ForeColor = Util.FromArgb(thm.ToColor(TextColor));
                txt.Text = (v.Alpha != 255 ? $"#{v.Alpha:X2}{v.Red:X2}{v.Green:X2}{v.Blue:X2}" : $"#{v.Red:X2}{v.Green:X2}{v.Blue:X2}");
                txt.Visible = true;
                txt.Focus();
                txt.SelectAll();
            }
            Invalidate();
            base.OnMouseClick(e);
        }
        #endregion
        #endregion

        #region Method
        #region Areas
        Dictionary<string, SKRect> Areas()
        {
            Dictionary<string, SKRect> dic = [];

            dic["Content"] = Util.FromRect(0, 0, Width - 1, Height - 1);

            var rts = Util.Columns(dic["Content"], [$"50px", $"100%"]);
            dic["Color"] = rts[0];
            dic["Value"] = rts[1];

            return dic;
        }
        #endregion

        #region Color(string)
        public SKColor? ToColor(string? color)
        {
            SKColor? ret = null;
            if (color != null)
            {
                var main = color;

                var vs = main?.Split(',').Select(x => x.Trim()).ToArray();

                if (main != null && main.StartsWith("#") && main.Length == 7
                    && byte.TryParse(main.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var r1)
                    && byte.TryParse(main.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var g1)
                    && byte.TryParse(main.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var b1))
                    ret = Util.FromArgb(r1, g1, b1);
                else if (main != null && main.StartsWith("#") && main.Length == 9
                    && byte.TryParse(main.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var r2)
                    && byte.TryParse(main.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var g2)
                    && byte.TryParse(main.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var b2)
                    && byte.TryParse(main.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var a2))
                    ret = Util.FromArgb(a2, r2, g2, b2);
                else if (main != null && main.StartsWith("#") && main.Length == 4
                    && byte.TryParse(string.Concat(main[1], main[1]), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var r5)
                    && byte.TryParse(string.Concat(main[2], main[2]), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var g5)
                    && byte.TryParse(string.Concat(main[3], main[3]), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var b5))
                    ret = Util.FromArgb(r5, g5, b5);
                else if (main != null && main.StartsWith("#") && main.Length == 5
                    && byte.TryParse(string.Concat(main[1], main[1]), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var r6)
                    && byte.TryParse(string.Concat(main[2], main[2]), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var g6)
                    && byte.TryParse(string.Concat(main[3], main[3]), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var b6)
                    && byte.TryParse(string.Concat(main[4], main[4]), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var a6))

                    ret = Util.FromArgb(a6, r6, g6, b6);
                else if (vs?.Length == 4 && byte.TryParse(vs[0], out byte a3) && byte.TryParse(vs[1], out byte r3) && byte.TryParse(vs[2], out byte g3) && byte.TryParse(vs[3], out byte b3))
                    ret = Util.FromArgb(a3, r3, g3, b3);
                else if (vs?.Length == 3 && byte.TryParse(vs[0], out byte r4) && byte.TryParse(vs[1], out byte g4) && byte.TryParse(vs[2], out byte b4))
                    ret = Util.FromArgb(r4, g4, b4);
                else if (int.TryParse(main, out int n))
                    ret = Util.FromArgb(n);
                else if (main != null)
                {
                    var mod = Color.FromName(main);
                    if (!(mod.A == 0 && mod.R == 0 && mod.G == 0 && mod.B == 0)) ret = Util.FromArgb(mod);
                }
            }
            return ret;
        }
        #endregion
        #endregion

    }
}
