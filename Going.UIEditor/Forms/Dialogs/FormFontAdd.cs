using Going.UI.Datas;
using Going.UI.Forms.Dialogs;
using Going.UI.Themes;
using Going.UI.Utils;
using Going.UIEditor.Utils;
using Going.UI.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Forms.Dialogs
{
    public partial class FormFontAdd : GoForm
    {
        #region Constructor
        public FormFontAdd()
        {
            InitializeComponent();

            dg.SelectionMode = Going.UI.Enums.GoDataGridSelectionMode.Selector;
            dg.RowHeight = 40;
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "FontName", HeaderText = "폰트명", Size = "200px" });
            dg.Columns.Add(new FontColumn { Name = "Text", HeaderText = "예시", Size = "100%" });

            btnOK.ButtonClicked += (o, s) => { if (ValidCheck()) DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Method
        #region SetLang
        void SetLang()
        {
            Title = LM.FontAdd;
            TitleIconString = "fa-square-plus";
            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            lblTitle.Text = LM.FontList;

            dg.Columns[0].HeaderText = LM.FontName;
            dg.Columns[1].HeaderText = LM.Example;
        }
        #endregion
        #region ValidCheck
        bool ValidCheck() => dg.Rows.Any(x => x.Selected);
        #endregion
        #region RefreshFonts
        void RefreshFonts()
        {
            var pcFonts = SKFontManager.Default.FontFamilies.Where(x => FontPath.IsContainsFont(x)).ToList();
            var ls = pcFonts.Where(x => x != "나눔고딕").Select(x => new FontDataItem(x) { FontPaths = FontPath.GetFontPaths(x) });

            dg.SetDataSource(ls.OrderBy(x => x.FontName));
            dg.Invalidate();
        }
        #endregion
        #region ShowFontAdd
        public Dictionary<string, List<byte[]>>? ShowFontAdd()
        {
            Dictionary<string, List<byte[]>>? ret = null;

            SetLang();

            RefreshFonts();

            var prj = Program.CurrentProject;
            if (prj != null && this.ShowDialog() == DialogResult.OK)
                ret = dg.Rows.Where(x => x.Selected).Select(x => (FontDataItem)x.Source!).ToDictionary(x => x.FontName, y => y.FontPaths.Select(z => File.ReadAllBytes(z)).ToList());

            return ret;
        }
        #endregion
        #endregion
    }

    #region class : FontColumn 
    public class FontColumn : GoDataGridColumn
    {
        public FontColumn()
        {
            CellType = typeof(FontCell);
        }
    }
    #endregion
    #region class : FontCell
    public class FontCell : GoDataGridCell
    {
        #region Constructor
        public FontCell(Going.UI.Controls.GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            #region var
            float InputBright = -0.2F;
            float CheckBoxBright = -0.2F;
            float BorderBright = 0.35F;

            var br = thm.Dark ? 1F : -1F;
            var dwndVisible = Grid.ComboDropDownVisible(this);
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = cV.BrightnessTransmit(BorderBright * br);
            #endregion

            var rt = Bounds;

            if (Row.Source is FontNameItem item)
                Util.DrawText(canvas, item.Text, item.FontName, Grid.FontStyle, 24, rt, cText);
            else if (Row.Source is FontDataItem item2)
                Util.DrawText(canvas, item2.Text, item2.FontName, Grid.FontStyle, 24, rt, cText);
        }
        #endregion
    }
    #endregion
    #region class : FontNameItem
    public class FontNameItem(string fontName)
    {
        public string FontName { get; } = fontName;
        public string Text { get; } = fontName;
        public bool HasDevice { get; set; }
        public bool UsedFont { get; set; }
        public int Order => UsedFont && !HasDevice ? 0 :
                            (UsedFont || HasDevice ? 1 : 2);
    }
    #endregion
    #region class : FontDataItem
    class FontDataItem(string fontName)
    {
        public string FontName => fontName;
        public string Text => fontName;
        public List<string> FontPaths { get; set; } = [];
        public List<byte[]> FontDatas { get; set; } = [];
    }
    #endregion
}
