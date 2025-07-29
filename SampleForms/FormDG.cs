using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Forms.Dialogs;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleForms
{
    public partial class FormDG : GoForm
    {
        public FormDG()
        {
            InitializeComponent();

            var mods = new BindMode[] { BindMode.BitRead, BindMode.BitWrite, BindMode.WordRead, BindMode.WordWrite }.Select(x => new GoDataGridInputComboItem { Text = x.ToString(), Value = x }).ToList();

            dgBind.SelectionMode = GoDataGridSelectionMode.Selector;
            dgBind.Columns.Add(new GoDataGridInputNumberColumn<byte> { Name = "Slave", HeaderText = "국번", Size = "15%" });
            dgBind.Columns.Add(new GoDataGridInputComboColumn { Name = "Mode", HeaderText = "펑션", Size = "35%", Items = mods });
            dgBind.Columns.Add(new AddressColumn { Name = "Address", HeaderText = "시작주소", Size = "30%" });
            dgBind.Columns.Add(new GoDataGridInputTextColumn { Name = "Bind", HeaderText = "바인딩", Size = "20%" });


            var ls = new List<ModbusBind>();
            ls.Add(new ModbusBind());
            ls.Add(new ModbusBind());
            ls.Add(new ModbusBind());

            dgBind.SetDataSource(ls);

        }

        #region class : ModbusBind
        public enum BindMode { BitRead, BitWrite, WordRead, WordWrite }
        public class ModbusBind
        {
            public byte Slave { get; set; }
            public int Address { get; set; }
            public BindMode Mode { get; set; } = BindMode.BitRead;

            public string Bind { get; set; } = "";
        }
        #endregion
    }


    #region class : AddressColumn 
    public class AddressColumn : GoDataGridColumn
    {
        public AddressColumn()
        {
            CellType = typeof(AddressCell);
        }
    }
    #endregion
    #region class : AddressCell
    public class AddressCell : GoDataGridCell
    {
        #region Constructor
        public AddressCell(Going.UI.Controls.GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is AddressColumn col)
            {
            }
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            #region var
            float InputBright = -0.2F;
            float CheckBoxBright = -0.2F;
            float BorderBright = 0.35F;

            var thm = GoTheme.Current;
            var br = GoTheme.Current.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = cV.BrightnessTransmit(BorderBright * br);
            #endregion

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (Convert.ToInt32(rt.Right) + 0.5F == Bounds.Right) rt.Right -= 1;

            var text = "";
            if (Value is int addr) text = GetHexString(addr);
            else cV = SKColors.DarkRed;

            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(InputBright * br), cB, GoRoundType.Rect, thm.Corner);
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, rt, cText);
        }
        #endregion

        #region Mouse
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (CollisionTool.Check(rt, x, y))
            {
                var text = Value is int addr ? GetHexString(addr) : "";
                Grid?.InvokeEditText(this, text, (s) =>
                {
                    if (GetHexValue(s) is int r)
                    {
                        var old = Value;
                        Value = r;
                        Grid?.InvokeValueChange(this, old, Value);
                    }
                });
            }
            base.OnMouseClick(x, y, button);
        }
        #endregion

        #region GetHexValue
        public static int? GetHexValue(string s)
        {
            int? ret = null;

            if (s != null)
            {
                int n;
                if (s.StartsWith("0x") && int.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out n))
                    ret = n;
                else if (int.TryParse(s, out n))
                    ret = n;
            }

            return ret;
        }
        #endregion
        #region GetHexString
        public static string GetHexString(int v)
        {
            return $"0x{v.ToString("X4")}";
        }
        #endregion
    }
    #endregion
}
