using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using OpenTK.Windowing.Common.Input;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.OpenTK.Input
{
    internal class TKKeyboard
    {
        #region Const
        const int HANGUL = 0;
        const int HANGUL_SHIFT = 10;
        const int ENGLISH = 1;
        const int ENGLISH_SHIFT = 11;
        const int NUMBER = 2;
        const int NUMBER_SHIFT = 12;
        #endregion

        #region Properties
        public string? Text { get => lbl.Value; set => lbl.Value = value; }
        public SKRect Bounds { get; set; }
        //public SKSize ScreenSize { get; set; }
        public bool UseShiftHolding { get; set; } = false;

        public bool IsHangul => exts["Han"].Checked;
        public bool IsShift => exts["Shift"].Checked;
        public bool IsNumber => exts["Num"].Checked;

        private int PState => (IsNumber ? NUMBER : IsHangul ? HANGUL : ENGLISH) + (IsShift ? 10 : 0);
        #endregion

        #region Member Variable
        KLbl lbl = new();
        Dictionary<string, KBtn> btns = [];
        Dictionary<string, KBtnEx> exts = [];

        Dictionary<int, Dictionary<string, string>> KeyMap = new Dictionary<int, Dictionary<string, string>>();

        Hangul parseHangul = new Hangul();
        string str = "";
        string strOrigin = "";
        #endregion

        #region Event
        public event Action<string>? Completed;
        #endregion

        #region Constructor
        public TKKeyboard()
        {
            #region Make KeyMap
            KeyMap.Add(HANGUL, new Dictionary<string, string>());
            KeyMap.Add(ENGLISH, new Dictionary<string, string>());
            KeyMap.Add(NUMBER, new Dictionary<string, string>());

            KeyMap.Add(HANGUL_SHIFT, new Dictionary<string, string>());
            KeyMap.Add(ENGLISH_SHIFT, new Dictionary<string, string>());
            KeyMap.Add(NUMBER_SHIFT, new Dictionary<string, string>());

            #region HANGUL
            KeyMap[HANGUL].Add("Ns1", "1");
            KeyMap[HANGUL].Add("Ns2", "2");
            KeyMap[HANGUL].Add("Ns3", "3");
            KeyMap[HANGUL].Add("Ns4", "4");
            KeyMap[HANGUL].Add("Ns5", "5");
            KeyMap[HANGUL].Add("Ns6", "6");
            KeyMap[HANGUL].Add("Ns7", "7");
            KeyMap[HANGUL].Add("Ns8", "8");
            KeyMap[HANGUL].Add("Ns9", "9");
            KeyMap[HANGUL].Add("Ns10", "0");

            KeyMap[HANGUL].Add("Us1", "ㅂ");
            KeyMap[HANGUL].Add("Us2", "ㅈ");
            KeyMap[HANGUL].Add("Us3", "ㄷ");
            KeyMap[HANGUL].Add("Us4", "ㄱ");
            KeyMap[HANGUL].Add("Us5", "ㅅ");
            KeyMap[HANGUL].Add("Us6", "ㅛ");
            KeyMap[HANGUL].Add("Us7", "ㅕ");
            KeyMap[HANGUL].Add("Us8", "ㅑ");
            KeyMap[HANGUL].Add("Us9", "ㅐ");
            KeyMap[HANGUL].Add("Us10", "ㅔ");

            KeyMap[HANGUL].Add("Ms1", "ㅁ");
            KeyMap[HANGUL].Add("Ms2", "ㄴ");
            KeyMap[HANGUL].Add("Ms3", "ㅇ");
            KeyMap[HANGUL].Add("Ms4", "ㄹ");
            KeyMap[HANGUL].Add("Ms5", "ㅎ");
            KeyMap[HANGUL].Add("Ms6", "ㅗ");
            KeyMap[HANGUL].Add("Ms7", "ㅓ");
            KeyMap[HANGUL].Add("Ms8", "ㅏ");
            KeyMap[HANGUL].Add("Ms9", "ㅣ");

            KeyMap[HANGUL].Add("Ds1", "ㅋ");
            KeyMap[HANGUL].Add("Ds2", "ㅌ");
            KeyMap[HANGUL].Add("Ds3", "ㅊ");
            KeyMap[HANGUL].Add("Ds4", "ㅍ");
            KeyMap[HANGUL].Add("Ds5", "ㅠ");
            KeyMap[HANGUL].Add("Ds6", "ㅜ");
            KeyMap[HANGUL].Add("Ds7", "ㅡ");

            KeyMap[HANGUL].Add("Ss1", ",");
            KeyMap[HANGUL].Add("Ss2", " ");
            KeyMap[HANGUL].Add("Ss3", ".");
            #endregion
            #region HANGUL_SHIFT
            KeyMap[HANGUL_SHIFT].Add("Ns1", "1");
            KeyMap[HANGUL_SHIFT].Add("Ns2", "2");
            KeyMap[HANGUL_SHIFT].Add("Ns3", "3");
            KeyMap[HANGUL_SHIFT].Add("Ns4", "4");
            KeyMap[HANGUL_SHIFT].Add("Ns5", "5");
            KeyMap[HANGUL_SHIFT].Add("Ns6", "6");
            KeyMap[HANGUL_SHIFT].Add("Ns7", "7");
            KeyMap[HANGUL_SHIFT].Add("Ns8", "8");
            KeyMap[HANGUL_SHIFT].Add("Ns9", "9");
            KeyMap[HANGUL_SHIFT].Add("Ns10", "0");

            KeyMap[HANGUL_SHIFT].Add("Us1", "ㅃ");
            KeyMap[HANGUL_SHIFT].Add("Us2", "ㅉ");
            KeyMap[HANGUL_SHIFT].Add("Us3", "ㄸ");
            KeyMap[HANGUL_SHIFT].Add("Us4", "ㄲ");
            KeyMap[HANGUL_SHIFT].Add("Us5", "ㅆ");
            KeyMap[HANGUL_SHIFT].Add("Us6", "ㅛ");
            KeyMap[HANGUL_SHIFT].Add("Us7", "ㅕ");
            KeyMap[HANGUL_SHIFT].Add("Us8", "ㅑ");
            KeyMap[HANGUL_SHIFT].Add("Us9", "ㅒ");
            KeyMap[HANGUL_SHIFT].Add("Us10", "ㅖ");

            KeyMap[HANGUL_SHIFT].Add("Ms1", "ㅁ");
            KeyMap[HANGUL_SHIFT].Add("Ms2", "ㄴ");
            KeyMap[HANGUL_SHIFT].Add("Ms3", "ㅇ");
            KeyMap[HANGUL_SHIFT].Add("Ms4", "ㄹ");
            KeyMap[HANGUL_SHIFT].Add("Ms5", "ㅎ");
            KeyMap[HANGUL_SHIFT].Add("Ms6", "ㅗ");
            KeyMap[HANGUL_SHIFT].Add("Ms7", "ㅓ");
            KeyMap[HANGUL_SHIFT].Add("Ms8", "ㅏ");
            KeyMap[HANGUL_SHIFT].Add("Ms9", "ㅣ");

            KeyMap[HANGUL_SHIFT].Add("Ds1", "ㅋ");
            KeyMap[HANGUL_SHIFT].Add("Ds2", "ㅌ");
            KeyMap[HANGUL_SHIFT].Add("Ds3", "ㅊ");
            KeyMap[HANGUL_SHIFT].Add("Ds4", "ㅍ");
            KeyMap[HANGUL_SHIFT].Add("Ds5", "ㅠ");
            KeyMap[HANGUL_SHIFT].Add("Ds6", "ㅜ");
            KeyMap[HANGUL_SHIFT].Add("Ds7", "ㅡ");

            KeyMap[HANGUL_SHIFT].Add("Ss1", ",");
            KeyMap[HANGUL_SHIFT].Add("Ss2", " ");
            KeyMap[HANGUL_SHIFT].Add("Ss3", ".");
            #endregion
            #region ENGLISH
            KeyMap[ENGLISH].Add("Ns1", "1");
            KeyMap[ENGLISH].Add("Ns2", "2");
            KeyMap[ENGLISH].Add("Ns3", "3");
            KeyMap[ENGLISH].Add("Ns4", "4");
            KeyMap[ENGLISH].Add("Ns5", "5");
            KeyMap[ENGLISH].Add("Ns6", "6");
            KeyMap[ENGLISH].Add("Ns7", "7");
            KeyMap[ENGLISH].Add("Ns8", "8");
            KeyMap[ENGLISH].Add("Ns9", "9");
            KeyMap[ENGLISH].Add("Ns10", "0");

            KeyMap[ENGLISH].Add("Us1", "q");
            KeyMap[ENGLISH].Add("Us2", "w");
            KeyMap[ENGLISH].Add("Us3", "e");
            KeyMap[ENGLISH].Add("Us4", "r");
            KeyMap[ENGLISH].Add("Us5", "t");
            KeyMap[ENGLISH].Add("Us6", "y");
            KeyMap[ENGLISH].Add("Us7", "u");
            KeyMap[ENGLISH].Add("Us8", "i");
            KeyMap[ENGLISH].Add("Us9", "o");
            KeyMap[ENGLISH].Add("Us10", "p");

            KeyMap[ENGLISH].Add("Ms1", "a");
            KeyMap[ENGLISH].Add("Ms2", "s");
            KeyMap[ENGLISH].Add("Ms3", "d");
            KeyMap[ENGLISH].Add("Ms4", "f");
            KeyMap[ENGLISH].Add("Ms5", "g");
            KeyMap[ENGLISH].Add("Ms6", "h");
            KeyMap[ENGLISH].Add("Ms7", "j");
            KeyMap[ENGLISH].Add("Ms8", "k");
            KeyMap[ENGLISH].Add("Ms9", "l");

            KeyMap[ENGLISH].Add("Ds1", "z");
            KeyMap[ENGLISH].Add("Ds2", "x");
            KeyMap[ENGLISH].Add("Ds3", "c");
            KeyMap[ENGLISH].Add("Ds4", "v");
            KeyMap[ENGLISH].Add("Ds5", "b");
            KeyMap[ENGLISH].Add("Ds6", "n");
            KeyMap[ENGLISH].Add("Ds7", "m");

            KeyMap[ENGLISH].Add("Ss1", ",");
            KeyMap[ENGLISH].Add("Ss2", " ");
            KeyMap[ENGLISH].Add("Ss3", ".");
            #endregion
            #region ENGLISH_SHIFT
            KeyMap[ENGLISH_SHIFT].Add("Ns1", "1");
            KeyMap[ENGLISH_SHIFT].Add("Ns2", "2");
            KeyMap[ENGLISH_SHIFT].Add("Ns3", "3");
            KeyMap[ENGLISH_SHIFT].Add("Ns4", "4");
            KeyMap[ENGLISH_SHIFT].Add("Ns5", "5");
            KeyMap[ENGLISH_SHIFT].Add("Ns6", "6");
            KeyMap[ENGLISH_SHIFT].Add("Ns7", "7");
            KeyMap[ENGLISH_SHIFT].Add("Ns8", "8");
            KeyMap[ENGLISH_SHIFT].Add("Ns9", "9");
            KeyMap[ENGLISH_SHIFT].Add("Ns10", "0");

            KeyMap[ENGLISH_SHIFT].Add("Us1", "Q");
            KeyMap[ENGLISH_SHIFT].Add("Us2", "W");
            KeyMap[ENGLISH_SHIFT].Add("Us3", "E");
            KeyMap[ENGLISH_SHIFT].Add("Us4", "R");
            KeyMap[ENGLISH_SHIFT].Add("Us5", "T");
            KeyMap[ENGLISH_SHIFT].Add("Us6", "Y");
            KeyMap[ENGLISH_SHIFT].Add("Us7", "U");
            KeyMap[ENGLISH_SHIFT].Add("Us8", "I");
            KeyMap[ENGLISH_SHIFT].Add("Us9", "O");
            KeyMap[ENGLISH_SHIFT].Add("Us10", "P");

            KeyMap[ENGLISH_SHIFT].Add("Ms1", "A");
            KeyMap[ENGLISH_SHIFT].Add("Ms2", "S");
            KeyMap[ENGLISH_SHIFT].Add("Ms3", "D");
            KeyMap[ENGLISH_SHIFT].Add("Ms4", "F");
            KeyMap[ENGLISH_SHIFT].Add("Ms5", "G");
            KeyMap[ENGLISH_SHIFT].Add("Ms6", "H");
            KeyMap[ENGLISH_SHIFT].Add("Ms7", "J");
            KeyMap[ENGLISH_SHIFT].Add("Ms8", "K");
            KeyMap[ENGLISH_SHIFT].Add("Ms9", "L");

            KeyMap[ENGLISH_SHIFT].Add("Ds1", "Z");
            KeyMap[ENGLISH_SHIFT].Add("Ds2", "X");
            KeyMap[ENGLISH_SHIFT].Add("Ds3", "C");
            KeyMap[ENGLISH_SHIFT].Add("Ds4", "V");
            KeyMap[ENGLISH_SHIFT].Add("Ds5", "B");
            KeyMap[ENGLISH_SHIFT].Add("Ds6", "N");
            KeyMap[ENGLISH_SHIFT].Add("Ds7", "M");

            KeyMap[ENGLISH_SHIFT].Add("Ss1", ",");
            KeyMap[ENGLISH_SHIFT].Add("Ss2", " ");
            KeyMap[ENGLISH_SHIFT].Add("Ss3", ".");
            #endregion
            #region NUMBER
            KeyMap[NUMBER].Add("Ns1", "1");
            KeyMap[NUMBER].Add("Ns2", "2");
            KeyMap[NUMBER].Add("Ns3", "3");
            KeyMap[NUMBER].Add("Ns4", "4");
            KeyMap[NUMBER].Add("Ns5", "5");
            KeyMap[NUMBER].Add("Ns6", "6");
            KeyMap[NUMBER].Add("Ns7", "7");
            KeyMap[NUMBER].Add("Ns8", "8");
            KeyMap[NUMBER].Add("Ns9", "9");
            KeyMap[NUMBER].Add("Ns10", "0");

            KeyMap[NUMBER].Add("Us1", "!");
            KeyMap[NUMBER].Add("Us2", "@");
            KeyMap[NUMBER].Add("Us3", "#");
            KeyMap[NUMBER].Add("Us4", "~");
            KeyMap[NUMBER].Add("Us5", "%");
            KeyMap[NUMBER].Add("Us6", "^");
            KeyMap[NUMBER].Add("Us7", "&");
            KeyMap[NUMBER].Add("Us8", "*");
            KeyMap[NUMBER].Add("Us9", "(");
            KeyMap[NUMBER].Add("Us10", ")");

            KeyMap[NUMBER].Add("Ms1", "+");
            KeyMap[NUMBER].Add("Ms2", "×");
            KeyMap[NUMBER].Add("Ms3", "÷");
            KeyMap[NUMBER].Add("Ms4", "=");
            KeyMap[NUMBER].Add("Ms5", "/");
            KeyMap[NUMBER].Add("Ms6", "_");
            KeyMap[NUMBER].Add("Ms7", "<");
            KeyMap[NUMBER].Add("Ms8", ">");
            KeyMap[NUMBER].Add("Ms9", "♡");

            KeyMap[NUMBER].Add("Ds1", "-");
            KeyMap[NUMBER].Add("Ds2", "'");
            KeyMap[NUMBER].Add("Ds3", "\"");
            KeyMap[NUMBER].Add("Ds4", ":");
            KeyMap[NUMBER].Add("Ds5", ";");
            KeyMap[NUMBER].Add("Ds6", ",");
            KeyMap[NUMBER].Add("Ds7", "?");

            KeyMap[NUMBER].Add("Ss1", ",");
            KeyMap[NUMBER].Add("Ss2", " ");
            KeyMap[NUMBER].Add("Ss3", ".");
            #endregion
            #region NUMBER_SHIFT
            KeyMap[NUMBER_SHIFT].Add("Ns1", "1");
            KeyMap[NUMBER_SHIFT].Add("Ns2", "2");
            KeyMap[NUMBER_SHIFT].Add("Ns3", "3");
            KeyMap[NUMBER_SHIFT].Add("Ns4", "4");
            KeyMap[NUMBER_SHIFT].Add("Ns5", "5");
            KeyMap[NUMBER_SHIFT].Add("Ns6", "6");
            KeyMap[NUMBER_SHIFT].Add("Ns7", "7");
            KeyMap[NUMBER_SHIFT].Add("Ns8", "8");
            KeyMap[NUMBER_SHIFT].Add("Ns9", "9");
            KeyMap[NUMBER_SHIFT].Add("Ns10", "0");

            KeyMap[NUMBER_SHIFT].Add("Us1", "•");
            KeyMap[NUMBER_SHIFT].Add("Us2", "○");
            KeyMap[NUMBER_SHIFT].Add("Us3", "●");
            KeyMap[NUMBER_SHIFT].Add("Us4", "□");
            KeyMap[NUMBER_SHIFT].Add("Us5", "■");
            KeyMap[NUMBER_SHIFT].Add("Us6", "◇");
            KeyMap[NUMBER_SHIFT].Add("Us7", "$");
            KeyMap[NUMBER_SHIFT].Add("Us8", "€");
            KeyMap[NUMBER_SHIFT].Add("Us9", "₤");
            KeyMap[NUMBER_SHIFT].Add("Us10", "¥");

            KeyMap[NUMBER_SHIFT].Add("Ms1", "'");
            KeyMap[NUMBER_SHIFT].Add("Ms2", "₩");
            KeyMap[NUMBER_SHIFT].Add("Ms3", "\\");
            KeyMap[NUMBER_SHIFT].Add("Ms4", "|");
            KeyMap[NUMBER_SHIFT].Add("Ms5", "☆");
            KeyMap[NUMBER_SHIFT].Add("Ms6", "{");
            KeyMap[NUMBER_SHIFT].Add("Ms7", "}");
            KeyMap[NUMBER_SHIFT].Add("Ms8", "[");
            KeyMap[NUMBER_SHIFT].Add("Ms9", "]");

            KeyMap[NUMBER_SHIFT].Add("Ds1", "°");
            KeyMap[NUMBER_SHIFT].Add("Ds2", "※");
            KeyMap[NUMBER_SHIFT].Add("Ds3", "¤");
            KeyMap[NUMBER_SHIFT].Add("Ds4", "《");
            KeyMap[NUMBER_SHIFT].Add("Ds5", "》");
            KeyMap[NUMBER_SHIFT].Add("Ds6", "¡");
            KeyMap[NUMBER_SHIFT].Add("Ds7", "¿");

            KeyMap[NUMBER_SHIFT].Add("Ss1", ",");
            KeyMap[NUMBER_SHIFT].Add("Ss2", " ");
            KeyMap[NUMBER_SHIFT].Add("Ss3", ".");
            #endregion
            #endregion

            #region Control
            btns.Add("Left", new("Left") { IconString = "fa-angle-left" });
            btns.Add("Right", new("Right") { IconString = "fa-angle-right" });
            for (int i = 0; i < 10; i++) btns.Add($"Ns{i + 1}", new($"Ns{i + 1}"));
            for (int i = 0; i < 10; i++) btns.Add($"Us{i + 1}", new($"Us{i + 1}"));
            for (int i = 0; i < 9; i++) btns.Add($"Ms{i + 1}", new($"Ms{i + 1}"));
            for (int i = 0; i < 7; i++) btns.Add($"Ds{i + 1}", new($"Ds{i + 1}"));
            for (int i = 0; i < 3; i++) btns.Add($"Ss{i + 1}", new($"Ss{i + 1}"));

            exts.Add($"Ent", new("Ent") { Text = "ENTER" /*, IconString = "fa-turn-down"*/ });
            exts.Add($"Back", new("Back") { Text = "BACKSPACE" /*, IconString = "fa-delete-left"*/ });
            exts.Add($"Clear", new("Clear") { Text = "CLEAR" /*, IconString = "fa-eraser"*/ });
            exts.Add($"Han", new("Han") { Text = "한/영", ToggleMode = true });
            exts.Add($"Num", new("Num") { ToggleMode = true });
            exts.Add($"Shift", new("Shift") { Text = "SHIFT", ToggleMode = true /*IconString = "fa-chevron-up"*/ });
            #endregion

            #region Event
            for (int i = 0; i < 10; i++) btns[$"Ns{i + 1}"].Clicked += Key_ButtonClick;
            for (int i = 0; i < 10; i++) btns[$"Us{i + 1}"].Clicked += Key_ButtonClick;
            for (int i = 0; i < 9; i++) btns[$"Ms{i + 1}"].Clicked += Key_ButtonClick;
            for (int i = 0; i < 7; i++) btns[$"Ds{i + 1}"].Clicked += Key_ButtonClick;
            for (int i = 0; i < 3; i++) btns[$"Ss{i + 1}"].Clicked += Key_ButtonClick;
            exts["Ent"].Clicked += Ent_ButtonClick;
            exts["Back"].Clicked += Back_ButtonClick;
            exts["Clear"].Clicked += Clear_ButtonClick;
            exts["Han"].CheckedChanged += Han_CheckedChanged;
            exts["Num"].CheckedChanged += Num_CheckedChanged;
            exts["Shift"].CheckedChanged += Shift_CheckedChanged;
            #endregion

            SetButtonText();

            parseHangul.InitState();
        }
        #endregion

        #region Method
        #region Draw
        public void Draw(SKCanvas canvas)
        {
            using var p = new SKPaint { IsAntialias = true };

            var thm = GoTheme.DarkTheme;
            Areas();

            p.IsStroke = false;
            p.Color = thm.Base1;
            canvas.DrawRect(Bounds, p);

            lbl.Draw(canvas, thm, thm.Base3, thm.Base1);
            foreach (var c in btns.Values.Where(x => x.Visible)) c.Draw(canvas, thm, GoRoundType.All);
            foreach (var c in exts.Values.Where(x => x.Visible)) c.Draw(canvas, thm, GoRoundType.All);
        }
        #endregion

        #region Mouse
        public bool MouseDown(float x, float y, GoMouseButton button)
        {
            foreach (var c in btns.Values.Where(x => x.Visible)) c.MouseDown(x, y, button);
            foreach (var c in exts.Values.Where(x => x.Visible)) c.MouseDown(x, y, button);

            return CollisionTool.Check(Bounds, x, y);
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            foreach (var c in btns.Values.Where(x => x.Visible)) c.MouseUp(x, y, button);
            foreach (var c in exts.Values.Where(x => x.Visible)) c.MouseUp(x, y, button);
        }

        public void MouseMove(float x, float y)
        {
            foreach (var c in btns.Values.Where(x => x.Visible)) c.MouseMove(x, y);
            foreach (var c in exts.Values.Where(x => x.Visible)) c.MouseMove(x, y);
        }
        #endregion

        #region Areas
        private void Areas()
        {
            var rt = Bounds;
            var rows = Util.Rows(Util.FromRect(rt, new GoPadding(5)), ["15%", "17%", "17%", "17%", "17%", "17%"]);
            var cols0 = Util.Columns(rows[0], ["100%", "10px", "50px", "50px"]);
            var cols1 = Util.Columns(rows[1], ["10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%"]);
            var cols2 = Util.Columns(rows[2], ["10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%"]);
            var cols3 = Util.Columns(rows[3], ["5%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%"]);
            var cols4 = Util.Columns(rows[4], ["15%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "15%"]);
            var cols5 = Util.Columns(rows[5], ["15%", "10%", "10%", "40%", "10%", "15%"]);
            Bounds = rt;

            #region Inflate
            var pad = 5;
            for (int i = 0; i < cols0.Length; i++) cols0[i].Inflate(-pad, -pad);
            for (int i = 0; i < cols1.Length; i++) cols1[i].Inflate(-pad, -pad);
            for (int i = 0; i < cols2.Length; i++) cols2[i].Inflate(-pad, -pad);
            for (int i = 0; i < cols3.Length; i++) cols3[i].Inflate(-pad, -pad);
            for (int i = 0; i < cols4.Length; i++) cols4[i].Inflate(-pad, -pad);
            for (int i = 0; i < cols5.Length; i++) cols5[i].Inflate(-pad, -pad);
            #endregion

            #region Set
            lbl.Bounds = cols0[0];
            btns["Left"].Bounds = cols0[2];
            btns["Right"].Bounds = cols0[3];

            for (int i = 0; i < cols1.Length; i++) btns[$"Ns{i + 1}"].Bounds = cols1[i];
            for (int i = 0; i < cols2.Length; i++) btns[$"Us{i + 1}"].Bounds = cols2[i];
            for (int i = 0; i < cols3.Length - 1; i++) btns[$"Ms{i + 1}"].Bounds = cols3[i + 1];
            for (int i = 0; i < cols4.Length - 2; i++) btns[$"Ds{i + 1}"].Bounds = cols4[i + 1];
            for (int i = 0; i < cols5.Length - 3; i++) btns[$"Ss{i + 1}"].Bounds = cols5[i + 2];

            exts["Shift"].Bounds = cols4[0];
            exts["Back"].Bounds = cols4[8];
            exts["Num"].Bounds = cols5[0];
            exts["Han"].Bounds = cols5[1];
            exts["Ent"].Bounds = cols5[5];
            #endregion
        }
        #endregion

        #region SetText
        void SetButtonText()
        {
            foreach (var c in btns.Values)
            {
                if (c.Name != null && KeyMap[PState].TryGetValue(c.Name, out var text)) c.Text = text;
            }

            exts["Num"].Text = IsNumber ? (IsHangul ? "가" : "Ab") : "@";
        }

        void SetText()
        {
            lbl.Value = str;
        }
        #endregion

        #region CheckShift
        void CheckShift()
        {
            if (!UseShiftHolding)
            {
                if (exts["Shift"].Checked) exts["Shift"].Checked = false;
                SetButtonText();
            }
        }
        #endregion

        #region Set
        public void Set(string? value)
        {
            str = "";
            lbl.Value = strOrigin = value ?? "";
            parseHangul.InitState();

            exts["Han"].Checked = false;
            exts["Num"].Checked = false;
            exts["Shift"].Checked = false;

            SetButtonText();
        }
        #endregion

        public float GetHeight(SKSize ScreenSize) => Math.Max(360, ScreenSize.Height * 0.4F);
        #endregion

        #region Event
        private void Key_ButtonClick(KBtn d)
        {
            if (PState == HANGUL || PState == HANGUL_SHIFT)
            {
                string v = str;
                if (parseHangul.Dic.ContainsKey(KeyMap[PState][d.Name])) parseHangul.Input(ref v, parseHangul.Dic[KeyMap[PState][d.Name]]);
                else v += KeyMap[PState][d.Name];
                str = v;
            }
            else
            {
                string v = str;
                v += KeyMap[PState][d.Name];
                str = v;
            }

            SetText();
            CheckShift();
        }

        private void Ent_ButtonClick(KBtnEx obj)
        {
            if (str.Length != (lbl.Value ?? "").Length && str.Length == 0)
            {
                str = strOrigin;
                Completed?.Invoke(str);
            }
            else
            {
                Completed?.Invoke(str);
            }
        }

        private void Back_ButtonClick(KBtnEx obj)
        {
            if (str != null && str.Length > 0)
            {
                str = str.Substring(0, str.Length - 1);
                parseHangul.InitState();
            }
            SetText();
            CheckShift();
        }

        private void Clear_ButtonClick(KBtnEx obj)
        {
            str = "";
            parseHangul.InitState();
            SetText();
        }

        private void Han_CheckedChanged(KBtnEx obj) { SetButtonText(); }
        private void Num_CheckedChanged(KBtnEx obj) { SetButtonText(); }
        private void Shift_CheckedChanged(KBtnEx obj) { SetButtonText(); }
        #endregion
    }

    #region class : KBtn
    class KBtn(string name)
    {
        #region Properties
        public string Name { get; private set; } = name;
        public string? Text { get; set; }
        public string? IconString { get; set; }
        public bool Hover { get; private set; }
        public bool Down { get; private set; }
        public SKRect Bounds;
        public bool Visible { get; set; } = true;
        #endregion

        #region Event
        public event Action<KBtn>? Clicked;
        #endregion

        #region Method
        public void Draw(SKCanvas canvas, GoTheme thm, GoRoundType round)
        {
            var c = thm.Base3.BrightnessTransmit(Down ? thm.DownBrightness : 0);
            var ct = thm.Fore.BrightnessTransmit(Down ? thm.DownBrightness : 0);

            var rt = Bounds;
            Util.DrawBox(canvas, rt, c.BrightnessTransmit(Hover ? thm.HoverFillBrightness : 0),
                                         c.BrightnessTransmit(Hover ? thm.HoverBorderBrightness : 0), round, thm.Corner);

            if (Down) rt.Offset(0, 1);
            Util.DrawTextIcon(canvas, Text, "나눔고딕", 14, IconString, 18, GoDirectionHV.Horizon, 5, rt, ct);
        }

        public void MouseDown(float x, float y, GoMouseButton button)
        {
            if (CollisionTool.Check(Bounds, x, y)) Down = true;
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            if (Down)
            {
                Down = false;
                if (CollisionTool.Check(Bounds, x, y)) Clicked?.Invoke(this);
            }
        }

        public void MouseMove(float x, float y) => Hover = CollisionTool.Check(Bounds, x, y);
        #endregion
    }
    #endregion
    #region class : KBtnEx
    class KBtnEx(string name)
    {
        #region Properties
        public string Name { get; private set; } = name;
        public bool Checked { get; set; } 
        public bool ToggleMode { get; set; }
        public string? Text { get; set; }
        public string? IconString { get; set; }
        public bool Hover { get; private set; }
        public bool Down { get; private set; }
        public SKRect Bounds;
        public bool Visible { get; set; } = true;
        #endregion

        #region Event
        public event Action<KBtnEx>? Clicked;
        public event Action<KBtnEx>? CheckedChanged;
        #endregion

        #region Method
        public void Draw(SKCanvas canvas, GoTheme thm, GoRoundType round)
        {
            var c = (Checked ? thm.Good : thm.Base2).BrightnessTransmit(Down ? thm.DownBrightness : 0);
            var ct = thm.Fore.BrightnessTransmit(Down ? thm.DownBrightness : 0);

            var rt = Bounds;
            Util.DrawBox(canvas, rt, c.BrightnessTransmit(Hover ? thm.HoverFillBrightness : 0),
                                         c.BrightnessTransmit(Hover ? thm.HoverBorderBrightness : 0), round, thm.Corner);

            if (Down) rt.Offset(0, 1);
            Util.DrawTextIcon(canvas, Text, "나눔고딕", 14, IconString, 18, GoDirectionHV.Vertical, 3, rt, ct);
        }

        public void MouseDown(float x, float y, GoMouseButton button)
        {
            if (CollisionTool.Check(Bounds, x, y)) Down = true;
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            if (Down)
            {
                Down = false;
                if (CollisionTool.Check(Bounds, x, y))
                {
                    Clicked?.Invoke(this);
                    if (ToggleMode)
                    {
                        Checked = !Checked;
                        CheckedChanged?.Invoke(this);
                    }
                }
            }
        }

        public void MouseMove(float x, float y) => Hover = CollisionTool.Check(Bounds, x, y);
        #endregion
    }
    #endregion
    #region class : KLbl
    class KLbl
    {
        public string? Value { get; set; }
        public SKRect Bounds { get; set; }

        public void Draw(SKCanvas canvas, GoTheme thm, SKColor border, SKColor fill)
        {
            Util.DrawBox(canvas, Bounds, fill, border, GoRoundType.All, thm.Corner);
            Util.DrawText(canvas, Value, "나눔고딕", 14, Bounds, thm.Fore);
        }
    }
    #endregion
    #region class : Hangul
    #region enum : KeyboardMode
    public enum KeyboardMode { Korea, English, Number, EnglishOnly }
    #endregion
    #region enum : State
    enum State
    {
        Cho,
        Jung,
        Jong
    }
    #endregion
    #region enum : Cho
    enum Cho
    {
        None = -1,
        r = 0,  //ㄱ
        R = 1,  //ㄲ
        s = 2,  //ㄴ
        e = 3,  //ㄷ
        E = 4,  //ㄸ
        f = 5,  //ㄹ
        a = 6,  //ㅁ
        q = 7,  //ㅂ
        Q = 8,  //ㅃ
        t = 9,  //ㅅ
        T = 10, //ㅆ
        d = 11, //ㅇ
        w = 12, //ㅈ
        W = 13, //ㅉ
        c = 14, //ㅊ
        z = 15, //ㅋ
        x = 16, //ㅌ
        v = 17, //ㅍ
        g = 18  //ㅎ
    }
    #endregion
    #region enum : Jung
    enum Jung
    {
        None = -1,
        k = 0,  //ㅏ
        o = 1,  //ㅐ
        i = 2,  //ㅑ
        O = 3,  //ㅒ
        j = 4,  //ㅓ
        p = 5,  //ㅔ
        u = 6,  //ㅕ
        P = 7,  //ㅖ
        h = 8,  //ㅗ
        hk = 9, //ㅘ
        ho = 10,//ㅙ
        hl = 11,//ㅚ
        y = 12, //ㅛ
        n = 13, //ㅜ
        nj = 14,//ㅝ
        np = 15,//ㅞ
        nl = 16,//ㅟ
        b = 17, //ㅠ
        m = 18, //ㅡ
        ml = 19,//ㅢ
        l = 20 //ㅣ
    }
    #endregion
    #region enum : Jong
    enum Jong
    {
        None = -1,
        r = 1,    //ㄱ
        R = 2,    //ㄲ
        rt = 3,    //ㄳ
        s = 4,    //ㄴ
        sw = 5,    //ㄵ
        sg = 6,    //ㄶ
        e = 7,    //ㄷ
        f = 8,    //ㄹ
        fr = 9,    //ㄺ
        fa = 10,   //ㄻ
        fq = 11,   //ㄼ
        ft = 12,   //ㄽ
        fx = 13,   //ㄾ
        fv = 14,   //ㄿ
        fg = 15,   //ㅀ
        a = 16,   //ㅁ
        q = 17,   //ㅂ
        qt = 18,   //ㅄ
        t = 19,   //ㅅ
        T = 20,   //ㅆ
        d = 21,    //ㅇ
        w = 22,   //ㅈ
        c = 23,   //ㅊ
        z = 24,   //ㅋ
        x = 25,   //ㅌ
        v = 26,   //ㅍ
        g = 27    //ㅎ
    }
    #endregion
    #region Class : Hangul
    class Hangul
    {
        #region Member Variable
        #region Const
        private const int KIYEOK = 0x1100;
        private const int A = 0x1161;
        private const int GA = 0xac00;

        private const int CHO_COUNT = 0x0013;
        private const int JUNG_COUNT = 0x0015;
        private const int JONG_COUNT = 0x001c;

        private const char JUNG_INIT_CHAR = '.';
        private const char JONG_INIT_CHAR = '.';
        #endregion
        #region Private
        private string _currentChar;
        private string _result;
        private State _state;

        private int _cho;
        private int _jung;
        private char _jungFirst;
        private bool _jungPossible;
        private int _jong;
        private char _jongFirst;
        private char _jongLast;
        private bool _jongPossible;
        #endregion
        #region Public
        public Dictionary<string, char> Dic = new Dictionary<string, char>();
        #endregion
        #endregion

        #region Constructor
        public Hangul()
        {
            #region Dic
            Dic.Add("ㅂ", 'q');
            Dic.Add("ㅃ", 'Q');
            Dic.Add("ㅈ", 'w');
            Dic.Add("ㅉ", 'W');
            Dic.Add("ㄷ", 'e');
            Dic.Add("ㄸ", 'E');
            Dic.Add("ㄱ", 'r');
            Dic.Add("ㄲ", 'R');
            Dic.Add("ㅅ", 't');
            Dic.Add("ㅆ", 'T');
            Dic.Add("ㅛ", 'y');
            Dic.Add("ㅕ", 'u');
            Dic.Add("ㅑ", 'i');
            Dic.Add("ㅐ", 'o');
            Dic.Add("ㅒ", 'O');
            Dic.Add("ㅔ", 'p');
            Dic.Add("ㅖ", 'P');

            Dic.Add("ㅁ", 'a');
            Dic.Add("ㄴ", 's');
            Dic.Add("ㅇ", 'd');
            Dic.Add("ㄹ", 'f');
            Dic.Add("ㅎ", 'g');
            Dic.Add("ㅗ", 'h');
            Dic.Add("ㅓ", 'j');
            Dic.Add("ㅏ", 'k');
            Dic.Add("ㅣ", 'l');

            Dic.Add("ㅋ", 'z');
            Dic.Add("ㅌ", 'x');
            Dic.Add("ㅊ", 'c');
            Dic.Add("ㅍ", 'v');
            Dic.Add("ㅠ", 'b');
            Dic.Add("ㅜ", 'n');
            Dic.Add("ㅡ", 'm');
            #endregion

            _currentChar = string.Empty;
            _result = string.Empty;
            _state = State.Cho;

            _cho = -1;
            _jung = -1;
            _jungFirst = JUNG_INIT_CHAR;
            _jong = -1;
            _jongFirst = JONG_INIT_CHAR;
            _jongLast = JONG_INIT_CHAR;
        }
        #endregion

        #region Method
        #region GetSingleJa
        private char GetSingleJa(int value)
        {
            byte[] bytes = BitConverter.GetBytes((short)(0x1100 + value));
            return Convert.ToChar(Encoding.Unicode.GetString(bytes, 0, bytes.Length));

        }
        #endregion 
        #region GetSingleMo
        private char GetSingleMo(int value) //하나의 모음으로만 구성된 완성형 글자를 반환
        {
            byte[] bytes = BitConverter.GetBytes((short)(0x1161 + value));
            return Convert.ToChar(Encoding.Unicode.GetString(bytes, 0, bytes.Length));
        }
        #endregion
        #region GetCompleteChar
        private char GetCompleteChar()
        {
            int tempJong = 0;
            if (_jong < 0)
                tempJong = 0;
            else
                tempJong = _jong;
            int completeChar = (_cho * (JUNG_COUNT * JONG_COUNT)) + (_jung * JONG_COUNT) + tempJong + GA;
            byte[] naeBytes = BitConverter.GetBytes((short)(completeChar));
            return Convert.ToChar(Encoding.Unicode.GetString(naeBytes, 0, naeBytes.Length));
        }
        #endregion
        #region Filter
        private char Filter(char ch)
        {
            if (ch == 'A') ch = 'a';
            if (ch == 'B') ch = 'b';
            if (ch == 'C') ch = 'c';
            if (ch == 'D') ch = 'd';
            if (ch == 'F') ch = 'f';
            if (ch == 'G') ch = 'g';
            if (ch == 'H') ch = 'h';
            if (ch == 'I') ch = 'i';
            if (ch == 'J') ch = 'j';
            if (ch == 'K') ch = 'k';
            if (ch == 'L') ch = 'l';
            if (ch == 'M') ch = 'm';
            if (ch == 'N') ch = 'n';
            if (ch == 'S') ch = 's';
            if (ch == 'U') ch = 'u';
            if (ch == 'V') ch = 'v';
            if (ch == 'X') ch = 'x';
            if (ch == 'Y') ch = 'y';
            if (ch == 'Z') ch = 'z';

            return ch;
        }
        #endregion
        #region Input
        public void Input(ref string source, char ch)
        {
            ch = Filter(ch);

            int code = (int)ch;
            if (code == 8)
            {
                #region Backspace
                if (source.Length <= 0)
                    return;

                if (_state == State.Cho)
                {
                    source = source.Substring(0, source.Length - 1);
                }
                else if (_state == State.Jung && _jungFirst.Equals(JUNG_INIT_CHAR))
                {
                    _state = State.Cho;
                    source = source.Substring(0, source.Length - 1);
                }
                else if (_jungPossible && (_jung != 8 && _jung != 13 && _jung != 18) && _jongFirst.Equals(JONG_INIT_CHAR) && _jongLast.Equals(JONG_INIT_CHAR))
                {
                    _state = State.Jung;
                    source = source.Substring(0, source.Length - 1);
                    _jung = CheckJung(_jungFirst.ToString());
                    _jong = -1;
                    source += GetCompleteChar();
                    _jungPossible = true;
                }
                else if ((_state == State.Jong || _state == State.Jung) && !_jungFirst.Equals(JUNG_INIT_CHAR) && _jongFirst.Equals(JONG_INIT_CHAR) && _jongLast.Equals(JONG_INIT_CHAR))
                {
                    _state = State.Jung;
                    _jungFirst = JUNG_INIT_CHAR;
                    _jungPossible = false;
                    _jung = -1;
                    source = source.Substring(0, source.Length - 1);
                    source += GetSingleJa(_cho);
                }
                else if (_state == State.Jong && !_jongFirst.Equals(JONG_INIT_CHAR) && !_jongLast.Equals(JONG_INIT_CHAR))
                {
                    _state = State.Jong;
                    source = source.Substring(0, source.Length - 1);
                    _jongLast = JONG_INIT_CHAR;
                    _jong = CheckJong(_jongFirst.ToString());
                    source += GetCompleteChar();
                    _jongPossible = true;
                }
                else if (_state == State.Jong && !_jongFirst.Equals(JONG_INIT_CHAR))
                {
                    int temp = CheckJung(_jungFirst.ToString());
                    if (temp == 8 || temp == 13 || temp == 18)
                    {
                        _jungPossible = true;
                        _state = State.Jung;
                    }
                    else
                    {
                        _state = State.Jong;
                    }
                    source = source.Substring(0, source.Length - 1);
                    _jong = -1;
                    _jongFirst = JONG_INIT_CHAR;
                    source += GetCompleteChar();
                }
                return;
                #endregion
            }

            if (!((code >= 97 && code <= 122) || (code >= 65 && code <= 90)))
            {
                #region Alphabet
                _cho = -1;
                _jung = -1;
                _jong = -1;
                _jungFirst = JUNG_INIT_CHAR;
                _jongFirst = JONG_INIT_CHAR;
                _jongLast = JONG_INIT_CHAR;
                _state = State.Cho;
                source += ch;
                return;
                #endregion
            }

            if (_state == State.Cho)
            {
                #region Cho
                _cho = CheckCho(ch);

                if (_cho >= 0)
                {
                    _state = State.Jung;
                    source += GetSingleJa(_cho);
                }
                else
                {
                    _state = State.Jung;
                    Input(ref source, ch);
                }
                #endregion
            }
            else if (_state == State.Jung)
            {
                #region Jung
                if (_jung < 0)
                {
                    _jung = CheckJung(ch.ToString());
                    if (_jung < 0)
                    {
                        _state = State.Cho;
                        Input(ref source, ch);
                        return;
                    }

                    if (_cho < 0)
                    {
                        source += GetSingleMo(CheckJung(ch.ToString()));
                        _state = State.Cho;
                        _jung = -1;
                        return;
                    }
                    else
                    {
                        if (_jung == 8 || _jung == 13 || _jung == 18)
                        {
                            _jungPossible = true;
                            _state = State.Jung;
                        }
                        else
                        {
                            _state = State.Jong;
                        }
                        _jungFirst = ch;
                        source = source.Substring(0, source.Length - 1);
                        source += GetCompleteChar();
                    }
                }
                else
                {
                    string jung = string.Empty;
                    jung += _jungFirst;
                    jung += ch;

                    int temp = CheckJung(jung);
                    if (temp > 0)
                    {
                        _jung = temp;
                        source = source.Substring(0, source.Length - 1);
                        source += GetCompleteChar();
                        _state = State.Jong;
                    }
                    else
                    {
                        _state = State.Jong;
                        Input(ref source, ch);
                    }
                }
                #endregion
            }
            else if (_state == State.Jong)
            {
                #region Jong
                if (_jong < 0)
                {
                    _jong = CheckJong(ch.ToString());

                    if (_jong > 0)
                    {
                        source = source.Substring(0, source.Length - 1);
                        source += GetCompleteChar();

                        _jongFirst = ch;
                        if (_jong == 1 || _jong == 4 || _jong == 8 || _jong == 17)
                        {
                            _jongPossible = true;
                        }
                    }
                    else if (CheckJung(ch.ToString()) >= 0)
                    {
                        _state = State.Jung;
                        _cho = -1;
                        _jung = -1;
                        Input(ref source, ch);
                        return;
                    }
                    else if (CheckCho(ch) >= 0)
                    {
                        _jongPossible = false;
                        _jong = 0;
                        Input(ref source, ch);
                    }
                }
                else
                {
                    if (_jongPossible)
                    {
                        _jongPossible = false;
                        string jong = string.Empty;
                        jong += _jongFirst;
                        jong += ch;

                        int temp = CheckJong(jong);

                        if (temp > 0)
                        {
                            _jongLast = ch;
                            _jong = temp;
                            source = source.Substring(0, source.Length - 1);
                            source += GetCompleteChar();
                        }
                        else
                        {
                            Input(ref source, ch);
                        }
                    }
                    else
                    {
                        if (CheckCho(ch) >= 0)
                        {
                            _jongFirst = JONG_INIT_CHAR;
                            _jongLast = JONG_INIT_CHAR;

                            _state = State.Cho;
                            _jung = -1;
                            _jong = -1;
                            _jungFirst = JUNG_INIT_CHAR;
                            _jungPossible = false;
                            Input(ref source, ch);
                        }
                        else
                        {
                            if (_jongLast.Equals(JONG_INIT_CHAR))
                            {
                                source = source.Substring(0, source.Length - 1);
                                _jong = 0;
                                source += GetCompleteChar();

                                _cho = CheckCho(_jongFirst);
                            }
                            else
                            {
                                source = source.Substring(0, source.Length - 1);
                                _jong = CheckJong(_jongFirst.ToString());
                                source += GetCompleteChar();

                                _cho = CheckCho(_jongLast);
                            }
                            source += GetSingleJa(_cho);

                            _jongFirst = JONG_INIT_CHAR;
                            _jongLast = JONG_INIT_CHAR;
                            _jungPossible = false;
                            _jung = -1;
                            _jong = -1;
                            _state = State.Jung;
                            Input(ref source, ch);
                        }
                    }
                }
                #endregion
            }
        }
        #endregion
        #region CheckCho
        private int CheckCho(char ch)
        {
            string[] ar = { "r", "R", "s", "e", "E", "f", "a", "q", "Q", "t", "T", "d", "w", "W", "c", "z", "x", "v", "g" };

            for (int i = 0; i < ar.Length; i++)
            {
                if (ar[i].ToString().Equals(ch.ToString()))
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion
        #region CheckJung
        private int CheckJung(string ch)
        {
            string[] ar = { "k", "o", "i", "O", "j", "p", "u", "P", "h", "hk", "ho", "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l" };

            for (int i = 0; i < ar.Length; i++)
            {
                if (ar[i].ToString().Equals(ch.ToString()))
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion
        #region CheckJong
        private int CheckJong(string ch)
        {
            string[] ar = { "r", "R", "rt", "s", "sw", "sg", "e", "f", "fr", "fa", "fq", "ft", "fx", "fv", "fg", "a", "q", "qt", "t", "T", "d", "w", "c", "z", "x", "v", "g" };

            for (int i = 0; i < ar.Length; i++)
            {
                if (ar[i].ToString().Equals(ch.ToString()))
                {
                    return i + 1;
                }
            }
            return -1;
        }
        #endregion
        #region InitState
        public void InitState()
        {
            _cho = -1;
            _jung = -1;
            _jong = -1;
            _state = State.Cho;
            _jungPossible = false;
            _jongPossible = false;
            _jungFirst = JUNG_INIT_CHAR;
            _jongFirst = JONG_INIT_CHAR;
            _jongLast = JONG_INIT_CHAR;
        }
        #endregion
        #endregion
    }
    #endregion
    #endregion
}
