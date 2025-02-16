using Going.UI.Input;
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
        private Action<string>? InputCallback;
        #endregion

        #region Member Variable
        TextBox txt = new TextBox { BorderStyle = BorderStyle.None, TextAlign = HorizontalAlignment.Center };
        #endregion

        #region Constructor
        private FormsInputManager()
        {
            Application.AddMessageFilter(this);

            txt.TextChanged += (o, s) => { if (InputControl != null && InputCallback != null) InputCallback(txt.Text); };
            txt.KeyDown += (o, s) =>
            {
                if (s.KeyCode == Keys.Escape || s.KeyCode == Keys.Enter)
                {
                    if (InputControl != null && InputCallback != null)
                    {
                        InputCallback(txt.Text);
                        InputControl.Controls.Remove(txt);
                        InputControl = null;
                        InputCallback = null;
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

                if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN)
                {
                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                    var point = InputControl.PointToClient(new System.Drawing.Point(x, y));

                    if (!InputControl.ClientRectangle.Contains(point) && !CollisionTool.Check(InputBounds, point.X, point.Y))
                    {
                        InputControl.Controls.Remove(txt);
                        InputControl = null;
                        InputCallback = null;
                    }
                }
            }
            return false;
        }
        #endregion

        #region InputString
        public void InputString(Control control, SKRect bounds, string fontName, float fontSize, string backColor, string textColor, Action<string> callback, string? value = null)
        {
            if (InputControl == null)
            {
                InputControl = control;
                InputBounds = bounds;
                InputCallback = callback;
                InputType = InputType.String;

                if (txt.Font.Name != fontName || txt.Font.Size != fontSize / 0.75F)
                {
                    var old = txt.Font;
                    txt.Font = new Font(fontName, fontSize * 0.75F);
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
    }
}
