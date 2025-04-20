using Going.UI.Enums;
using Going.UI.Forms.Input;
using Going.UI.Managers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoColorSelector : GoWrapperControl<Going.UI.Controls.GoColorSelector>
    {
        #region Properties
        public string FontName { get => Control.FontName; set { if (Control.FontName != value) { Control.FontName = value; Invalidate(); } } }
        public GoFontStyle FontStyle { get => Control.FontStyle; set { if (Control.FontStyle != value) { Control.FontStyle = value; Invalidate(); } } }
        public float FontSize { get => Control.FontSize; set { if (Control.FontSize != value) { Control.FontSize = value; Invalidate(); } } }

        public string TextColor { get => Control.TextColor; set { if (Control.TextColor != value) { Control.TextColor = value; Invalidate(); } } }
        public string InputColor { get => Control.InputColor; set { if (Control.InputColor != value) { Control.InputColor = value; Invalidate(); } } }
        public string BorderColor { get => Control.BorderColor; set { if (Control.BorderColor != value) { Control.BorderColor = value; Invalidate(); } } }

        public GoContentAlignment ContentAlignment { get => Control.ContentAlignment; set { if (Control.ContentAlignment != value) { Control.ContentAlignment = value; Invalidate(); } } }

        public SKColor Value { get => Control.Value; set { if (Control.Value != value) { Control.Value = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        FormsInputManager IM;
        #endregion

        #region Constructor
        public GoColorSelector()
        {
            IM = new FormsInputManager(this);
            GoInputEventer.Current.InputNumber += (c, bounds, callback, type, value, min, max) =>
            {
                if (c == Control)
                {
                    IM.InputNumber<byte>(Control, bounds, FontName, FontStyle, FontSize, InputColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                }
            };
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            IM.ClearInput();
            IM.Dispose();
            base.Dispose(disposing);
        }
        #endregion

        #region Method
        void spkey(Keys key, Keys modifier)
        {

        }
        #endregion
    }
}
