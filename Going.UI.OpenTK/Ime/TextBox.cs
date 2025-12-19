using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.OpenTK.Ime
{
    public class TextBox
    {
        #region enum
        public enum TextBoxInputMode
        {
            All,            // 모든 문자
            NumberOnly,     // 숫자만 (0-9)
            IntegerOnly,    // 정수 (0-9, -, +)
            DecimalOnly,    // 실수 (0-9, -, +, .)
            AlphaNumeric,   // 영문+숫자
            AlphaOnly,      // 영문만
            HexOnly         // 16진수 (0-9, A-F)
        }
        #endregion

        #region Properties
        public string Text
        {
            get => stxt; 
            set
            {
                if (stxt != value)
                {
                    stxt = value;
                    if (IsFocused) callback?.Invoke(stxt);
                }
            }
        }
        public IGoControl? Control { get; set; }
        public SKRect Bounds { get; set; }

        public bool IsFocused { get; set; }
        public int CursorPosition { get; set; }
        public int SelectionStart { get; set; } = -1;
        public int SelectionEnd { get; set; } = -1;
        public int Padding { get; set; } = 0;

        public TextBoxInputMode InputMode { get; set; } = TextBoxInputMode.All;
        public double? MinValue { get; set; } = null;
        public double? MaxValue { get; set; } = null;
        #endregion

        #region Member Variable
        private float _scrollOffset = 0;
        private string _compositionText = "";
        private int _compositionCursorPos = 0;
        public bool IsComposing => !string.IsNullOrEmpty(_compositionText);

        private bool _imeCommitted = false; // IME 완료 플래그 추가
        private string stxt = "";
        private Action<string>? callback;
        #endregion

        #region Metdho
        #region InputString
        public void InputString(IGoControl c, SKRect rect, Action<string> callback, string? text)
        {
            if (c != Control)
            {
                IsFocused = true;
                Control = c;
                Bounds = rect;
                Bounds.Offset(c.ScreenX, c.ScreenY);
                Text = text ?? "";
                SelectionStart = 0;
                SelectionEnd = Text.Length;
                CursorPosition = Text.Length;
                InputMode = TextBoxInputMode.All;

                if (Control is GoInput vc)
                {
                    GoInputEventer.Current.SetInputControl(vc);
                    vc._InputModeInvisibleText_ = true;
                }

                this.callback = callback;
            }
        }
        #endregion
        #region InputNumber
        public void InputNumber(IGoControl c, SKRect rect, Action<string> callback, Type valueType, object value, object? min, object? max)
        {
            if (c != Control)
            {
                IsFocused = true;
                Control = c;
                Bounds = rect;
                Bounds.Offset(c.ScreenX, c.ScreenY);
                Text = value?.ToString() ?? "";
                SelectionStart = 0;
                SelectionEnd = Text.Length;
                CursorPosition = Text.Length;

                if (valueType == typeof(byte) || valueType == typeof(ushort) || valueType == typeof(uint) || valueType == typeof(ulong)) InputMode = TextBoxInputMode.NumberOnly;
                else if (valueType == typeof(sbyte) || valueType == typeof(short) || valueType == typeof(int) || valueType == typeof(long)) InputMode = TextBoxInputMode.IntegerOnly;
                else if (valueType == typeof(float) || valueType == typeof(double) || valueType == typeof(decimal)) InputMode = TextBoxInputMode.DecimalOnly;

                if (min != null) min = Convert.ToDouble(min);
                if (max != null) max = Convert.ToDouble(max);

                if (Control is GoInput vc)
                {
                    GoInputEventer.Current.SetInputControl(vc); 
                    vc._InputModeInvisibleText_ = true;
                }

                this.callback = callback;
            }
        }
        #endregion

        #region Update
        public void Update()
        {
            if (IsFocused)
            {
              
            }
        }
        #endregion

        #region Draw
        public void Draw(SKCanvas canvas, GoTheme thm)
        {
            if (IsFocused)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    if (Control is GoInput c)
                    {
                        canvas.Translate(c.ScreenX, c.ScreenY);

                        SKTypeface face = Util.GetTypeface(c.FontName, c.FontStyle);
                        using var font = new SKFont(face, c.FontSize);
                        using var p = new SKPaint { IsAntialias = true };
                        var cText = thm.ToColor(c.TextColor);

                        var textY = Bounds.Top + Bounds.Height / 2 + c.FontSize / 2 - 2;
                        float textStartX = CalculateTextStartX(font, GoContentAlignment.MiddleCenter);

                        string displayText = Text;
                        int compositionStart = -1;
                        int compositionLength = 0;

                        if (IsComposing && IsFocused)
                        {
                            displayText = Text.Insert(CursorPosition, _compositionText);
                            compositionStart = CursorPosition;
                            compositionLength = _compositionText.Length;
                        }

                        #region 선택영역
                        if (HasSelection())
                        {
                            int start = Math.Min(SelectionStart, SelectionEnd);
                            int end = Math.Max(SelectionStart, SelectionEnd);

                            string beforeSelection = displayText.Substring(0, Math.Clamp(start, 0, displayText.Length));
                            string selection = displayText.Substring(
                                Math.Clamp(start, 0, displayText.Length),
                                Math.Clamp(end - start, 0, displayText.Length - start)
                            );

                            float beforeWidth = font.MeasureText(beforeSelection);
                            float selectionWidth = font.MeasureText(selection);

                            p.Color = thm.Select;
                            p.IsStroke = false;
                            canvas.DrawRect(
                                textStartX + beforeWidth,
                                Bounds.Top + (Bounds.Height - c.FontSize) / 2,
                                selectionWidth,
                                c.FontSize + 4,
                                p
                            );
                        }
                        #endregion

                        #region 입력부
                        if (IsComposing && compositionStart >= 0)
                        {
                            string beforeComposition = displayText.Substring(0, compositionStart);
                            string composition = displayText.Substring(compositionStart, compositionLength);

                            float beforeWidth = font.MeasureText(beforeComposition);
                            float compositionWidth = font.MeasureText(composition);

                            p.Color = new SKColor(60, 60, 80, 180);
                            p.IsStroke = false;
                            canvas.DrawRect(
                                textStartX + beforeWidth,
                                Bounds.Top + (Bounds.Height - c.FontSize) / 2,
                                compositionWidth,
                                c.FontSize + 4,
                                p
                            );

                            p.Color = new SKColor(100, 150, 255);
                            p.StrokeWidth = 1;
                            p.IsStroke = true;
                            canvas.DrawLine(
                                textStartX + beforeWidth,
                                Bounds.Top + Bounds.Height - (Bounds.Height - c.FontSize) / 2 + 2,
                                textStartX + beforeWidth + compositionWidth,
                                Bounds.Top + Bounds.Height - (Bounds.Height - c.FontSize) / 2 + 2,
                                p
                            );
                        }
                        #endregion

                        #region 텍스트
                        p.Color = cText;
                        p.IsStroke = false;
                        canvas.DrawText(displayText, textStartX, textY, font, p);
                        #endregion

                        #region 커서
                        if (IsFocused && DateTime.Now.Millisecond > 500)
                        {
                            int cursorPos = CursorPosition + (IsComposing ? _compositionText.Length : 0);
                            string textBeforeCursor = displayText.Substring(0, cursorPos);
                            float cursorX = textStartX + font.MeasureText(textBeforeCursor);

                            p.Color = cText;
                            p.StrokeWidth = 1;
                            p.IsStroke = true;
                            canvas.DrawLine(
                                cursorX,
                                Bounds.Top + (Bounds.Height - c.FontSize) / 2,
                                cursorX,
                                Bounds.Top + Bounds.Height - (Bounds.Height - c.FontSize) / 2,
                                p
                            );
                        }
                        #endregion
                    }
                }
            }
        }
        #endregion

        #region Handler
        #region OnKeyPress
        public void OnKeyPress(KeyboardKeyEventArgs e, string inputChar)
        {
            if (!IsFocused) return;

            switch (e.Key)
            {
                #region Back
                case Keys.Backspace:
                    {
                        if (HasSelection()) DeleteSelection();
                        else if (CursorPosition > 0)
                        {
                            Text = Text.Remove(CursorPosition - 1, 1);
                            CursorPosition--;
                        }
                    }
                    break;
                #endregion
                #region Del
                case Keys.Delete:
                    {
                        if (HasSelection()) DeleteSelection();
                        else if (CursorPosition < Text.Length) { Text = Text.Remove(CursorPosition, 1); }
                    }
                    break;
                #endregion
                #region Left
                case Keys.Left:
                    {
                        if (e.Shift && SelectionStart == -1) SelectionStart = CursorPosition;

                        if (e.Control) CursorPosition = GetPreviousWordPosition();
                        else { if (CursorPosition > 0) CursorPosition--; }

                        if (!e.Shift) ClearSelection();
                        else SelectionEnd = CursorPosition;
                    }
                    break;
                #endregion
                #region Right
                case Keys.Right:
                    {
                        if (e.Shift && SelectionStart == -1) SelectionStart = CursorPosition;

                        if (e.Control) CursorPosition = GetNextWordPosition();
                        else { if (CursorPosition < Text.Length) CursorPosition++; }

                        if (!e.Shift) ClearSelection();
                        else SelectionEnd = CursorPosition;
                    }
                    break;
                #endregion
                #region Home
                case Keys.Home:
                    {
                        if (e.Shift && SelectionStart == -1) SelectionStart = CursorPosition;

                        CursorPosition = 0;

                        if (!e.Shift) ClearSelection();
                        else SelectionEnd = CursorPosition;
                    }
                    break;
                #endregion
                #region End
                case Keys.End:
                    {
                        if (e.Shift && SelectionStart == -1) SelectionStart = CursorPosition;

                        CursorPosition = Text.Length;

                        if (!e.Shift) ClearSelection();
                        else SelectionEnd = CursorPosition;
                    }
                    break;
                #endregion
                #region Ctrl + A
                case Keys.A:
                    if (e.Control)
                    {
                        SelectionStart = 0;
                        SelectionEnd = Text.Length;
                        CursorPosition = Text.Length;
                    }
                    break;
                #endregion
                #region Ctrl + C
                case Keys.C:
                    if (e.Control && HasSelection())
                    {
                        CopyToClipboard();
                    }
                    break;
                #endregion
                #region Ctrl + X
                case Keys.X:
                    if (e.Control && HasSelection())
                    {
                        CopyToClipboard();
                        DeleteSelection();
                    }
                    break;
                #endregion
                #region Ctrl + V
                case Keys.V:
                    if (e.Control)
                    {
                        PasteFromClipboard();
                    }
                    break;
                #endregion
                #region Enter
                case Keys.Enter:
                case Keys.KeyPadEnter:
                    {
                        Complete();
                    }
                    break;
                    #endregion
            }

            UpdateScrollOffset();
        }
        #endregion
        #region OnTextInput
        public void OnTextInput(string text)
        {
            if (!IsFocused || string.IsNullOrEmpty(text)) return;

            if (_imeCommitted)
            {
                _imeCommitted = false;
                return;
            }

            if (!IsInputAllowed(text))
            {
                return; 
            }

            if (HasSelection()) DeleteSelection();

            Text = Text.Insert(CursorPosition, text);
            CursorPosition += text.Length;

            if (!ValidateValueRange())
            {
                Text = Text.Remove(CursorPosition - text.Length, text.Length);
                CursorPosition -= text.Length;
                return;
            }

            UpdateScrollOffset();
        }
        #endregion

        #region OnIMEComposition
        public void OnIMEComposition(string compositionText, int cursorPos)
        {
            if (!IsFocused) return;

            if (!AllowIME())
            {
                _compositionText = "";
                _compositionCursorPos = 0;
                return;
            }


            _compositionText = compositionText ?? "";
            _compositionCursorPos = cursorPos;

            UpdateScrollOffset();
        }
        #endregion
        #region OnIMECommit
        public void OnIMECommit(string text)
        {
            if (!IsFocused) return;

            if (!AllowIME())
            {
                _compositionText = "";
                _compositionCursorPos = 0;
                return;
            }

            _compositionText = "";
            _compositionCursorPos = 0;
            _imeCommitted = true; // 플래그 설정

            if (!string.IsNullOrEmpty(text))
            {
                if (HasSelection())
                {
                    DeleteSelection();
                }

                Text = Text.Insert(CursorPosition, text);
                CursorPosition += text.Length;

                UpdateScrollOffset();
            }
        }
        #endregion

        #region OnMouseDown
        public void OnMouseDown(float mouseX, float mouseY, bool isShiftPressed)
        {
            if (ContainsPoint(mouseX, mouseY))
            {
                CursorPosition = GetCursorPositionFromMouse(mouseX - (Control?.ScreenX ?? 0));

                if (!isShiftPressed) ClearSelection();
                else UpdateSelection();
            }
            else
            {
                Complete();
            }
        }
        #endregion
        #region OnMouseDrag
        public void OnMouseDrag(float mouseX, float mouseY)
        {
            if (!IsFocused) return;

            if (SelectionStart == -1)
            {
                SelectionStart = CursorPosition;
            }

            CursorPosition = GetCursorPositionFromMouse(mouseX - (Control?.ScreenX ?? 0));
            SelectionEnd = CursorPosition;
        }
        #endregion
        #endregion

        #region Tools
        #region ContainsPoint
        public bool ContainsPoint(float x, float y)
        {
            if (Control != null) return CollisionTool.Check(Util.FromRect(Control.ScreenX + Bounds.Left, Control.ScreenY + Bounds.Top, Bounds.Width, Bounds.Height), x, y);
            else return false;
        }
        #endregion
        #region GetCursorPositionFromMouse
        private int GetCursorPositionFromMouse(float mouseX)
        {
            if (Control is GoInput c)
            {
                SKTypeface face = Util.GetTypeface(c.FontName, c.FontStyle);
                using var font = new SKFont(face, c.FontSize);

                // ✅ 수정: 정렬에 따른 텍스트 시작 위치 계산
                float textStartX = CalculateTextStartX(font, GoContentAlignment.MiddleCenter);
                float relativeX = mouseX - textStartX;

                for (int i = 0; i <= Text.Length; i++)
                {
                    string substr = Text.Substring(0, i);
                    float width = font.MeasureText(substr);

                    if (width >= relativeX)
                    {
                        if (i > 0)
                        {
                            string prevSubstr = Text.Substring(0, i - 1);
                            float prevWidth = font.MeasureText(prevSubstr);
                            float charWidth = width - prevWidth;

                            if (relativeX - prevWidth < charWidth / 2)
                                return i - 1;
                        }
                        return i;
                    }
                }
            }
            return Text.Length;
        }
        #endregion
        #region UpdateScrollOffset
        private void UpdateScrollOffset()
        {
            if (Control is GoInput c)
            {
                var align = GoContentAlignment.MiddleCenter;

                if (align == GoContentAlignment.TopCenter ||
                    align == GoContentAlignment.MiddleCenter ||
                    align == GoContentAlignment.BottomCenter ||
                    align == GoContentAlignment.TopRight ||
                    align == GoContentAlignment.MiddleRight ||
                    align == GoContentAlignment.BottomRight)
                {
                    _scrollOffset = 0;
                    return;
                }

                SKTypeface face = Util.GetTypeface(c.FontName, c.FontStyle);
                using var font = new SKFont(face, c.FontSize);

                string textBeforeCursor = Text.Substring(0, CursorPosition);
                float cursorX = font.MeasureText(textBeforeCursor);

                if (IsComposing) cursorX += font.MeasureText(_compositionText);

                float viewWidth = Bounds.Width - Padding * 2;

                if (cursorX - _scrollOffset > viewWidth - 10)
                    _scrollOffset = cursorX - viewWidth + 10;
                else if (cursorX - _scrollOffset < 10)
                    _scrollOffset = Math.Max(0, cursorX - 10);
            }
        }
        #endregion

        #region HasSelection
        private bool HasSelection()
        {
            return SelectionStart != -1 && SelectionEnd != -1 && SelectionStart != SelectionEnd;
        }
        #endregion
        #region ClearSelection
        private void ClearSelection()
        {
            SelectionStart = -1;
            SelectionEnd = -1;
        }
        #endregion
        #region UpdateSelection
        private void UpdateSelection()
        {
            if (SelectionStart == -1) SelectionStart = CursorPosition;
            SelectionEnd = CursorPosition;
        }
        #endregion
        #region DeleteSelection
        private void DeleteSelection()
        {
            if (!HasSelection()) return;

            int start = Math.Min(SelectionStart, SelectionEnd);
            int end = Math.Max(SelectionStart, SelectionEnd);

            Text = Text.Remove(start, end - start);
            CursorPosition = start;
            ClearSelection();
        }
        #endregion

        #region GetPreviousWordPosition
        private int GetPreviousWordPosition()
        {
            if (CursorPosition == 0) return 0;

            int pos = CursorPosition - 1;
            while (pos > 0 && char.IsWhiteSpace(Text[pos])) pos--;
            while (pos > 0 && !char.IsWhiteSpace(Text[pos - 1])) pos--;
            return pos;
        }
        #endregion
        #region GetNextWordPosition
        private int GetNextWordPosition()
        {
            if (CursorPosition >= Text.Length) return Text.Length;

            int pos = CursorPosition;
            while (pos < Text.Length && !char.IsWhiteSpace(Text[pos]))
                pos++;

            while (pos < Text.Length && char.IsWhiteSpace(Text[pos]))
                pos++;

            return pos;
        }
        #endregion

        #region CopyToClipboard
        private void CopyToClipboard()
        {
            if (!HasSelection()) return;

            int start = Math.Min(SelectionStart, SelectionEnd);
            int end = Math.Max(SelectionStart, SelectionEnd);
            string selectedText = Text.Substring(start, end - start);

            try
            {
               TextCopy.ClipboardService.SetText(selectedText);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region PasteFromClipboard
        private void PasteFromClipboard()
        {
            try
            {
                string? clipboardText = TextCopy.ClipboardService.GetText();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    if (!IsInputAllowed(clipboardText))
                    {
                        return; 
                    }

                    if (HasSelection())
                    {
                        DeleteSelection();
                    }

                    Text = Text.Insert(CursorPosition, clipboardText);
                    CursorPosition += clipboardText.Length;

                    if (!ValidateValueRange())
                    {
                        Text = Text.Remove(CursorPosition - clipboardText.Length, clipboardText.Length);
                        CursorPosition -= clipboardText.Length;
                        return;
                    }

                    UpdateScrollOffset();
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region CalculateTextStartX
        private float CalculateTextStartX(SKFont font, GoContentAlignment align)
        {
            string displayText = Text;
            if (IsComposing)
            {
                displayText = Text.Insert(CursorPosition, _compositionText);
            }

            float textWidth = font.MeasureText(displayText);

            switch (align)
            {
                case GoContentAlignment.TopCenter:
                case GoContentAlignment.MiddleCenter:
                case GoContentAlignment.BottomCenter:
                    float centerX = Bounds.Left + Bounds.Width / 2 - textWidth / 2;
                    return Math.Max(Bounds.Left + Padding, centerX);

                case GoContentAlignment.TopRight:
                case GoContentAlignment.MiddleRight:
                case GoContentAlignment.BottomRight:
                    return Bounds.Left + Bounds.Width - Padding - textWidth;

                case GoContentAlignment.TopLeft:
                case GoContentAlignment.MiddleLeft:
                case GoContentAlignment.BottomLeft:
                default:
                    return Bounds.Left + Padding - _scrollOffset;
            }
        }
        #endregion

        #region ValidateValueRange
        private bool ValidateValueRange()
        {
            if (InputMode != TextBoxInputMode.IntegerOnly &&
                InputMode != TextBoxInputMode.DecimalOnly &&
                InputMode != TextBoxInputMode.NumberOnly)
                return true;

            if (MinValue == null && MaxValue == null)
                return true;

            if (string.IsNullOrEmpty(Text) || Text == "-" || Text == "+")
                return true;

            if (!double.TryParse(Text, out double value))
                return true; 

            if (MinValue.HasValue && value < MinValue.Value)
                return false;
            if (MaxValue.HasValue && value > MaxValue.Value)
                return false;

            return true;
        }
        #endregion
        #region IsInputAllowed
        private bool IsInputAllowed(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            switch (InputMode)
            {
                case TextBoxInputMode.All:
                    return true;

                case TextBoxInputMode.NumberOnly:
                    return input.All(c => char.IsDigit(c));

                case TextBoxInputMode.IntegerOnly:
                    foreach (char c in input)
                    {
                        if (!char.IsDigit(c) && c != '-' && c != '+')
                            return false;
                    }
                    if ((input.Contains('-') || input.Contains('+')) && CursorPosition != 0)
                        return false;
                    if ((input.Contains('-') && Text.Contains('-')) ||
                        (input.Contains('+') && Text.Contains('+')))
                        return false;
                    return true;

                case TextBoxInputMode.DecimalOnly:
                    foreach (char c in input)
                    {
                        if (!char.IsDigit(c) && c != '-' && c != '+' && c != '.')
                            return false;
                    }
                    if ((input.Contains('-') || input.Contains('+')) && CursorPosition != 0)
                        return false;
                    if ((input.Contains('-') && Text.Contains('-')) ||
                        (input.Contains('+') && Text.Contains('+')))
                        return false;
                    if (input.Contains('.') && Text.Contains('.'))
                        return false;
                    return true;

                case TextBoxInputMode.AlphaNumeric:
                    return input.All(c => char.IsLetterOrDigit(c));

                case TextBoxInputMode.AlphaOnly:
                    return input.All(c => char.IsLetter(c));

                case TextBoxInputMode.HexOnly:
                    return input.All(c =>
                        char.IsDigit(c) ||
                        (c >= 'A' && c <= 'F') ||
                        (c >= 'a' && c <= 'f'));

                default:
                    return true;
            }
        }
        #endregion
        #region AllowIME
        private bool AllowIME()
        {
            switch (InputMode)
            {
                case TextBoxInputMode.NumberOnly:
                case TextBoxInputMode.IntegerOnly:
                case TextBoxInputMode.DecimalOnly:
                case TextBoxInputMode.HexOnly:
                    return false; 

                default:
                    return true;
            }
        }
        #endregion

        #region Complate
        void Complete()
        {
            IsFocused = false;
            if (Control is GoInput vc)
            {
                vc._InputModeInvisibleText_ = false;
                GoInputEventer.Current.ClearInputControl();
            }
            Control = null;
            Text = "";
            callback = null;
            CursorPosition = 0;


            ClearSelection();
        }
        #endregion
        #endregion
        #endregion
    }
}
