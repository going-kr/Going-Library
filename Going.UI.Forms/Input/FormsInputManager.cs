using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TextBox = System.Windows.Forms.TextBox;

namespace Going.UI.Forms.Input
{
    public class FormsInputManager : IMessageFilter
    {
        #region Properties
        private static readonly Lazy<FormsInputManager> _instance = new Lazy<FormsInputManager>(() => new FormsInputManager());
        public static FormsInputManager Current => _instance.Value;

        public bool IsInput => InputControl != null;
        public Control? InputControl { get; private set; }
        public SKRect InputBounds { get; private set; }
        public InputType InputType { get; private set; }
        
        #endregion

        #region Member Variable
        private TextBox txt = new TextBox { BorderStyle = BorderStyle.None, TextAlign = HorizontalAlignment.Center };
        private Action<string>? InputCallback;
        private Type? ValueType;
        private object? ValueOrigin;
        private bool IsMinusInput = false;
        double vmin, vmax;
        #endregion

        #region Constructor
        private FormsInputManager()
        {
            Application.AddMessageFilter(this);

            txt.TextChanged += (o, s) =>
            {
                if (InputControl != null)
                {
                    if (InputType == InputType.String) InputCallback?.Invoke(txt.Text);
                    else
                    {
                        var t = ValueType;

                        var selectionStart = txt.SelectionStart;
                        var selectionLength = txt.SelectionLength;
                        var newText = String.Empty;

                        #region parse
                        if (t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                        {
                            var bComma = false;
                            foreach (Char c in txt.Text.ToCharArray())
                            {
                                if (Char.IsDigit(c) || Char.IsControl(c) || (c == '.' && !bComma && txt.Text != ".") || (newText.Length == 0 && (c == '-' || c == '+') && IsMinusInput)) newText += c;
                                if (c == '.' && txt.Text != ".") bComma = true;
                            }
                            txt.Text = newText;
                            txt.SelectionStart = selectionStart <= txt.Text.Length ? selectionStart : txt.Text.Length;
                        }
                        else if (t == typeof(sbyte) || t == typeof(short) || t == typeof(int) || t == typeof(long) ||
                                t == typeof(byte) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong))
                        {
                            foreach (var c in txt.Text.ToCharArray())
                            {
                                if (Char.IsDigit(c) || Char.IsControl(c) || (newText.Length == 0 && (c == '-' || c == '+') && IsMinusInput)) newText += c;
                            }
                        }
                        #endregion

                        txt.Text = newText;
                        txt.SelectionStart = selectionStart <= txt.Text.Length ? selectionStart : txt.Text.Length;
                     
                        InputCallback?.Invoke(txt.Text);
                    }
                }
            };

            txt.KeyDown += (o, s) =>
            {
                if (s.KeyCode == Keys.Escape || s.KeyCode == Keys.Enter)
                {
                    if (InputControl != null)
                    {
                        InputCallback?.Invoke(txt.Text);
                        InputControl.Controls.Remove(txt);
                        InputControl = null;
                        GoInputEventer.Current.ClearInputControl();
                    }
                }
            };
        }
        #endregion

        #region PreFilterMessage
        public bool PreFilterMessage(ref Message m)
        {
            if (InputControl != null)
            {
                int WM_LBUTTONDOWN = 0x0201;
                int WM_RBUTTONDOWN = 0x0204;
                
                // 해당 마우스 이벤트 메세지를 내가 먼저 볼 수 있게 만드는 객체 : IMessageFilter, Message 객체로 후킹 가능
                // 여기서 Window Message = WM이고 LBUTTONDOWN, RBUTTONDOWN은 마우스 이벤트 메세지로 이벤트가 발생하면 해당 메세지가 발생
                if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN)
                {
                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                    var point = InputControl.PointToClient(new System.Drawing.Point(x, y));

                    if (!InputControl.ClientRectangle.Contains(point) && !CollisionTool.Check(InputBounds, point.X, point.Y))
                    {
                        InputControl.Controls.Remove(txt);
                        InputControl = null;
                        GoInputEventer.Current.ClearInputControl();
                    }
                }
            }
            return false;
        }
        #endregion

        #region InputString
        public void InputString(Control control, IGoControl baseControl, SKRect bounds, string fontName, GoFontStyle fontStyle, float fontSize, string backColor, string textColor, Action<string> callback, string? value = null)
        {
            if (InputControl == null)
            {
                InputControl = control;
                InputBounds = bounds;
                InputCallback = callback;
                InputType = InputType.String;
                GoInputEventer.Current.SetInputControl(baseControl);

                FontStyle r = FontStyle.Regular;
                switch (fontStyle)
                {
                    case GoFontStyle.Normal: r = FontStyle.Regular; break;
                    case GoFontStyle.Bold: r = FontStyle.Bold; break;
                    case GoFontStyle.Italic: r = FontStyle.Italic; break;
                    case GoFontStyle.BoldItalic: r = FontStyle.Bold | FontStyle.Italic; break;
                }

                if (txt.Font.Name != fontName || txt.Font.Size != fontSize / 0.75F || txt.Font.Style != r)
                {
                    var old = txt.Font;
                    txt.Font = new Font(fontName, fontSize * 0.75F, r);
                    old.Dispose();
                }

                var rt = bounds;
                var h = txt.Height;
                var ass = 1;
                txt.Bounds = new Rectangle((int)rt.Left + 5, (int)rt.MidY - h / 2 + ass, (int)rt.Width - 10, h);
                txt.BackColor = Util.FromArgb(GoTheme.Current.ToColor(backColor));
                txt.ForeColor = Util.FromArgb(GoTheme.Current.ToColor(textColor));
                txt.Text = value;

                InputControl.Controls.Add(txt);
                txt.Focus();
                txt.SelectAll();
            }
            else txt.Focus();
        }
        #endregion

        #region InputNumber
        public void InputNumber<T>(Control control, IGoControl baseControl, SKRect bounds, string fontName, GoFontStyle fontStyle, float fontSize, string backColor, string textColor, Action<string> callback, Type type, object value, object? min, object? max)
        {
            if (InputControl == null)
            {
                InputControl = control;
                InputBounds = bounds;
                InputCallback = callback;
                InputType = InputType.Number;
                ValueType = type;
                ValueOrigin = value;

                #region MinusInput
                if (type == typeof(sbyte)) { vmax = ((sbyte?)max ?? sbyte.MaxValue); vmin = ((sbyte?)min ?? sbyte.MinValue); IsMinusInput = vmax < 0 || vmin < 0; }
                else if (type == typeof(short)) { vmax = ((short?)max ?? short.MaxValue); vmin = ((short?)min ?? short.MinValue); IsMinusInput = vmax < 0 || vmin < 0; }
                else if (type == typeof(int)) { vmax = ((int?)max ?? int.MaxValue); vmin = ((int?)min ?? int.MinValue); IsMinusInput = vmax < 0 || vmin < 0; }
                else if (type == typeof(long)) { vmax = ((long?)max ?? long.MaxValue); vmin = ((long?)min ?? long.MinValue); IsMinusInput = vmax < 0 || vmin < 0; }
                else if (type == typeof(byte)) { vmax = ((byte?)max ?? byte.MaxValue); vmin = ((byte?)min ?? byte.MinValue); IsMinusInput = false; }
                else if (type == typeof(ushort)) { vmax = ((ushort?)max ?? ushort.MaxValue); vmin = ((ushort?)min ?? ushort.MinValue); IsMinusInput = false; }
                else if (type == typeof(uint)) { vmax = ((uint?)max ?? uint.MaxValue); vmin = ((uint?)min ?? uint.MinValue); IsMinusInput = false; }
                else if (type == typeof(ulong)) { vmax = ((ulong?)max ?? ulong.MaxValue); vmin = ((ulong?)min ?? ulong.MinValue); IsMinusInput = false; }
                else if (type == typeof(float)) { vmax = Convert.ToDouble((float?)max ?? float.MaxValue); vmin = Convert.ToDouble((float?)min ?? float.MinValue); IsMinusInput = vmax < 0 || vmin < 0; }
                else if (type == typeof(double)) { vmax = Convert.ToDouble((double?)max ?? double.MaxValue); vmin = Convert.ToDouble((double?)min ?? double.MinValue); IsMinusInput = vmax < 0 || vmin < 0; }
                else if (type == typeof(decimal)) { vmax = Convert.ToDouble((decimal?)max ?? decimal.MaxValue); vmin = Convert.ToDouble((decimal?)min ?? decimal.MinValue); IsMinusInput = vmax < 0 || vmin < 0; }
                else IsMinusInput = false;
                #endregion

                GoInputEventer.Current.SetInputControl(baseControl);

                FontStyle r = FontStyle.Regular;
                switch (fontStyle)
                {
                    case GoFontStyle.Normal: r = FontStyle.Regular; break;
                    case GoFontStyle.Bold: r = FontStyle.Bold; break;
                    case GoFontStyle.Italic: r = FontStyle.Italic; break;
                    case GoFontStyle.BoldItalic: r = FontStyle.Bold | FontStyle.Italic; break;
                }

                if (txt.Font.Name != fontName || txt.Font.Size != fontSize / 0.75F || txt.Font.Style != r)
                {
                    var old = txt.Font;
                    txt.Font = new Font(fontName, fontSize * 0.75F, r);
                    old.Dispose();
                }

                var rt = bounds;
                var h = txt.Height;
                var ass = 0;
                txt.Bounds = new Rectangle((int)rt.Left + 5, (int)rt.MidY - h / 2 + ass, (int)rt.Width - 10, h);
                txt.BackColor = Util.FromArgb(GoTheme.Current.ToColor(backColor));
                txt.ForeColor = Util.FromArgb(GoTheme.Current.ToColor(textColor));
                txt.Text = ValueTool.ToString(value, null);

                InputControl.Controls.Add(txt);
                txt.Focus();
                txt.SelectAll();
            }
            else txt.Focus();
        }
        #endregion

        #region ClearInput
        public void ClearInput()
        {
            if (InputControl != null)
            {
                InputControl.Controls.Remove(txt);
                InputControl = null;
                GoInputEventer.Current.ClearInputControl();
            }
        }
        #endregion
    }
}
