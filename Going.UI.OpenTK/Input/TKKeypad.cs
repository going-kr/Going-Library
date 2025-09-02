using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.OpenTK.Input
{
    internal class TKKeypad
    {
        #region Const 
        public int MessageTime = 1000;
        #endregion

        #region Properties
        public string? Text { get => lbl.Value; set => lbl.Value = value; }
        public SKRect Bounds { get; set; }
        #endregion

        #region Member Variable
        KLbl lbl = new();
        Dictionary<string, KBtn> btns = [];

        string sval = "";
        string svalOrigin = "";
        string svalMessage = "";
        Type? valueType;

        int mode = 0;
        double vmin, vmax;
        #endregion

        #region Event
        public event Action<string>? Completed;
        #endregion

        #region Constructor
        public TKKeypad()
        {
            #region Control
            for (int i = 0; i < 10; i++) btns.Add($"N{i}", new($"N{i}") { Text = $"{i}" });

            btns.Add("Back", new("Back") { IconString = "fa-delete-left" });
            btns.Add("Clear", new("Clear") { IconString = "fa-eraser" });
            btns.Add("Sign", new("Sign") { IconString = "fa-plus-minus" });
            btns.Add("Enter", new("Enter") { Text = "ENT" });
            btns.Add("Dot", new("Dot") { Text = "." });
            #endregion

            #region Event
            for (int i = 0; i < 10; i++) btns[$"N{i}"].Clicked += (c) => { sval += c.Text; SetText(); };

            #region Dot
            btns["Dot"].Clicked += (c) =>
            {
                if (sval.IndexOf('.') == -1)
                {
                    if (sval.Length == 0) sval += "0";
                    sval += ".";
                }
                SetText();
            };
            #endregion
            #region Sign
            btns["Sign"].Clicked += (c) =>
            {
                decimal n = 0;
                if (decimal.TryParse(sval, out n))
                {
                    if (n >= 0 && sval.Substring(0, 1) != "-") sval = sval.Insert(0, "-");
                    else if (n <= 0 && sval.Substring(0, 1) == "-") sval = sval.Substring(1);
                }
                SetText();
            };
            #endregion
            #region Back
            btns["Back"].Clicked += (c) =>
            {
                if (sval.Length > 0) sval = sval.Substring(0, sval.Length - 1);
                SetText();
            };
            #endregion
            #region Clear
            btns["Clear"].Clicked += (c) => { sval = ""; SetText(); };
            #endregion
            #region Enter
            btns["Enter"].Clicked += (c) =>
            {
                var cval = lbl.Value ?? "";
                if (sval.Length != cval.Length && sval.Length == 0)
                {
                    if (svalOrigin == cval)
                    {
                        sval = svalOrigin;
                        Completed?.Invoke(sval);
                    }
                    else lbl.Value = "";
                }
                else
                {
                    var valid = true;
                    try
                    {
                        if (mode == 0 && ulong.TryParse(sval, out var v0)) valid = vmin <= v0 && v0 <= vmax;
                        else if (mode == 1 && long.TryParse(sval, out var v1)) valid = vmin <= v1 && v1 <= vmax;
                        else if (mode == 2 && double.TryParse(sval, out var v2)) valid = vmin <= v2 && v2 <= vmax;
                    }
                    catch { valid = false; }

                    if (valid) Completed?.Invoke(sval);
                    else
                    {
                        ThreadPool.QueueUserWorkItem((o) =>
                        {
                            svalMessage = $"범위 초과 ( {vmin} ~ {vmax} )"; SetText();
                            Thread.Sleep(MessageTime);
                            svalMessage = ""; SetText();
                            sval = "";
                        });
                    }

                }
            };
            #endregion
            #endregion
        }
        #endregion

        #region Method
        #region Draw
        public void Draw(SKCanvas canvas, GoTheme thm)
        {
            using var p = new SKPaint { IsAntialias = true };

            Areas();

            p.IsStroke = false;
            p.Color = thm.Base1;
            canvas.DrawRect(Bounds, p);

            lbl.Draw(canvas, thm, double.TryParse(sval, out var v) && v >= vmin && v <= vmax ? thm.Base3 : SKColors.Red, thm.Base1);
            foreach (var c in btns.Values.Where(x => x.Visible)) c.Draw(canvas, thm, GoRoundType.All);
        }
        #endregion

        #region Mouse
        public bool MouseDown(float x, float y, GoMouseButton button)
        {
            foreach (var c in btns.Values.Where(x => x.Visible)) c.MouseDown(x, y, button);

            return CollisionTool.Check(Bounds, x, y);
        }

        public void MouseUp(float x, float y, GoMouseButton button)
        {
            foreach (var c in btns.Values.Where(x => x.Visible)) c.MouseUp(x, y, button);
        }

        public void MouseMove(float x, float y)
        {
            foreach (var c in btns.Values.Where(x => x.Visible)) c.MouseMove(x, y);
        }
        #endregion

        #region Areas
        private void Areas()
        {
            var rt = Bounds;
            var rts = Util.Grid(Util.FromRect(rt, new GoPadding(5)), ["14%", "14%", "14%", "14%", "14%", "14%", "16%"], ["32%", "34%", "34%"]);
            Bounds = rt;

            #region Inflate
            var pad = 5;
            for (int ir = 0; ir < rts.GetLength(0); ir++)
                for (int ic = 0; ic < rts.GetLength(1); ic++)
                    rts[ir, ic].Inflate(-pad, -pad);
            #endregion

            #region Set
            lbl.Bounds = Util.Merge(rts, 0, 0, 6, 1);
            btns["Clear"].Bounds = rts[0, 6];

            if (mode == 0)
            {
                string[] sr1 = ["N1", "N2", "N3", "N4", "N5", "Back"];
                string[] sr2 = ["N6", "N7", "N8", "N9", "N0", "Enter"];
                for (int i = 0; i < 6; i++)
                {
                    if (i == 5)
                    {
                        btns[sr1[i]].Bounds = Util.Merge(rts, 5, 1, 2, 1);
                        btns[sr2[i]].Bounds = Util.Merge(rts, 5, 2, 2, 1);
                    }
                    else
                    {
                        btns[sr1[i]].Bounds = rts[1, i];
                        btns[sr2[i]].Bounds = rts[2, i];
                    }
                }
            }
            else if (mode == 1)
            {
                string[] sr1 = ["N1", "N2", "N3", "N4", "N5", "Sign", "Back"];
                string[] sr2 = ["N6", "N7", "N8", "N9", "N0", "Dot", "Enter"];
                for (int i = 0; i < 7; i++)
                {
                    if (i == 5)
                    {
                        btns[sr1[i]].Bounds = Util.Merge(rts, i, 1, 1, 2);
                    }
                    else
                    {
                        btns[sr1[i]].Bounds = rts[1, i];
                        btns[sr2[i]].Bounds = rts[2, i];
                    }
                }
            }
            else if(mode == 2)
            {
                string[] sr1 = ["N1", "N2", "N3", "N4", "N5", "Sign", "Back"];
                string[] sr2 = ["N6", "N7", "N8", "N9", "N0", "Dot", "Enter"];
                for (int i = 0; i < 7; i++)
                {
                    btns[sr1[i]].Bounds = rts[1, i];
                    btns[sr2[i]].Bounds = rts[2, i];
                }
            }
            #endregion
        }
        #endregion

        #region SetText
        void SetText()
        {
            if (string.IsNullOrWhiteSpace(svalMessage))
            {
                if (sval.Length > 0)
                {
                    if (mode == 0 && ulong.TryParse(sval, out var n0)) sval = lbl.Value = n0.ToString();
                    else if (mode == 1 && long.TryParse(sval, out var n1)) sval = lbl.Value = n1.ToString();
                    else if (mode == 2 && double.TryParse(sval, out var n2))
                    {
                        if (sval.Last() == '.') sval = lbl.Value = n2.ToString() + ".";
                        else sval = lbl.Value = n2.ToString();
                    }
                }
                else sval = lbl.Value = "";
            }
            else
            {
                lbl.Value = svalMessage;
            }
        }
        #endregion

        #region Set
        public void Set(Type valueType, object value, object? min, object? max)
        {
            sval = "";
            lbl.Value = svalOrigin = ValueTool.ToString(value, null) ?? "";

            #region valueType
            var m = -1;
            this.valueType = valueType;

            if (valueType == typeof(sbyte)) { m = 1; vmin = Convert.ToDouble(min ?? sbyte.MinValue); vmax = Convert.ToDouble(max ?? sbyte.MaxValue); }
            else if (valueType == typeof(short)) { m = 1; vmin = Convert.ToDouble(min ?? short.MinValue); vmax = Convert.ToDouble(max ?? short.MaxValue); }
            else if (valueType == typeof(int)) { m = 1; vmin = Convert.ToDouble(min ?? int.MinValue); vmax = Convert.ToDouble(max ?? int.MaxValue); }
            else if (valueType == typeof(long)) { m = 1; vmin = Convert.ToDouble(min ?? long.MinValue); vmax = Convert.ToDouble(max ?? long.MaxValue); }
            else if (valueType == typeof(byte)) { m = 0; vmin = Convert.ToDouble(min ?? byte.MinValue); vmax = Convert.ToDouble(max ?? byte.MaxValue); }
            else if (valueType == typeof(ushort)) { m = 0; vmin = Convert.ToDouble(min ?? ushort.MinValue); vmax = Convert.ToDouble(max ?? ushort.MaxValue); }
            else if (valueType == typeof(uint)) { m = 0; vmin = Convert.ToDouble(min ?? uint.MinValue); vmax = Convert.ToDouble(max ?? uint.MaxValue); }
            else if (valueType == typeof(ulong)) { m = 0; vmin = Convert.ToDouble(min ?? ulong.MinValue); vmax = Convert.ToDouble(max ?? ulong.MaxValue); }
            else if (valueType == typeof(float)) { m = 2; vmin = Convert.ToDouble(min ?? float.MinValue); vmax = Convert.ToDouble(max ?? float.MaxValue); }
            else if (valueType == typeof(double)) { m = 2; vmin = Convert.ToDouble(min ?? double.MinValue); vmax = Convert.ToDouble(max ?? double.MaxValue); }
            else if (valueType == typeof(decimal)) { m = 2; vmin = Convert.ToDouble(min ?? decimal.MinValue); vmax = Convert.ToDouble(max ?? decimal.MaxValue); }
            else { m = -1; throw new Exception("숫자 자료형이 아닙니다"); }


            if (m == 1 && vmin >= 0 && vmax >= 0) m = 0;

            if(m == 0)
            {
                btns["Dot"].Visible = false;
                btns["Sign"].Visible = false;
            }
            else if (m == 1)
            {
                btns["Dot"].Visible = false;
                btns["Sign"].Visible = true;
            }
            else if (m == 2)
            {
                btns["Dot"].Visible = true;
                btns["Sign"].Visible = true;
            }

            mode = m;
            #endregion

            SetText();
        }
        #endregion

        public float GetHeight(SKSize ScreenSize) => Math.Max(240, ScreenSize.Height * 0.3F);
        #endregion

    }
}
