using Going.UI.Datas;
using Going.UI.Forms.Dialogs;
using Going.UIEditor.Controls;
using Going.UIEditor.Forms.Dialogs;
using Going.UIEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Windows.Services.Maps;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UIEditor.Forms
{
    public partial class FormResourceManager : GoForm
    {
        #region Constructor
        public FormResourceManager()
        {
            InitializeComponent();
            dgFont.SelectionMode = Going.UI.Enums.GoDataGridSelectionMode.Selector;
            dgFont.RowHeight = 40;
            dgFont.Columns.Add(new GoDataGridLabelColumn { Name = "FontName", HeaderText = "폰트명", Size = "200px" });
            dgFont.Columns.Add(new FontColumn { Name = "Text", HeaderText = "예시", Size = "100%" });

            #region event
            #region btnClose
            btnClose.ButtonClicked += (o, s) => DialogResult = DialogResult.OK;
            #endregion
            #region btnImageAdd
            btnImageAdd.ButtonClicked += (o, s) =>
            {
                var prj = Program.CurrentProject;
                if (prj != null)
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Image Files|*.bmp;*.jpg;*.png;*.gif";
                        ofd.Multiselect = true;
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            foreach (var filename in ofd.FileNames)
                            {
                                var name = Path.GetFileNameWithoutExtension(filename).ToLower();
                                var data = File.ReadAllBytes(filename);
                                var imgs = prj.Design.GetImages();
                                if (imgs.Any(x => x.name == name))
                                {
                                    var ls2 = imgs.Where(x => x.name.Split('_').Length == 2);
                                    var max = ls2.Count() > 0 ? ls2.Max(x => int.TryParse(x.name.Split('_')[1], out var n) ? n : 0) : 0;
                                    name = $"{name}_{max + 1}";
                                }

                                prj.Design.AddImage(name, data);
                                prj.Edit = true;

                                RefreshGrid();
                            }
                        }
                    }
                }
            };
            #endregion
            #region btnImageDel
            btnImageDel.ButtonClicked += (s, e) =>
            {
                var prj = Program.CurrentProject;
                if (prj != null && grid.SelectedItems.Count > 0)
                {
                    foreach (var item in grid.SelectedItems)
                        if (item is ImageContent c && c.Name != null)
                            prj.Design.RemoveImage(c.Name);
                    prj.Edit = true;

                    RefreshGrid();
                }
            };
            #endregion

            #region btnFontAdd
            btnFontAdd.ButtonClicked += (o, s) =>
            {
                var prj = Program.CurrentProject;
                if (prj != null)
                {
                    var ret = Program.FontAdder.ShowFontAdd();
                    if (ret != null)
                    {
                        foreach (var name in ret.Keys)
                        {
                            if (name != "나눔고딕")
                            {
                                var ls = prj.Design.GetFonts();
                                if (ls.Count < 4)
                                {
                                    prj.Design.RemoveFont(name);
                                    foreach (var data in ret[name]) prj.Design.AddFont(name, data);
                                }
                                else
                                {
                                    Program.MessageBox.ShowMessageBoxOk(LM.FontAdd, LM.ErrorFontCount);
                                    break;
                                }
                            }
                        }

                        prj.Edit = true;

                        RefreshFonts();
                    }
                }
            };
            #endregion
            #region btnFontDel
            btnFontDel.ButtonClicked += (s, e) =>
            {
                var prj = Program.CurrentProject;
                if (prj != null)
                {
                    var ls = dgFont.Rows.Where(x => x.Selected).Select(x => (FontDataItem)x.Source!).ToList();
                    if (ls.Any(x => x.FontName == "나눔고딕")) Program.MessageBox.ShowMessageBoxOk(LM.Delete, LM.CantDeleteDefaultFont);
                    else
                    {
                        foreach (var v in ls) prj.Design.RemoveFont(v.FontName);
                        RefreshFonts();
                    }
                }
            };
            #endregion
            #endregion
        }
        #endregion

        #region Method
        #region SetLang
        void SetLang()
        {
            Title = LM.Resources;
            lblImageTitle.Text = LM.ImageList;
            lblFontTitle.Text = LM.FontList;
            btnClose.Text = LM.Close;

            dgFont.Columns[0].HeaderText = LM.FontName;
            dgFont.Columns[1].HeaderText = LM.Example;
        }
        #endregion
        #region RefrshGrid
        void RefreshGrid()
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var imgs = prj.Design.GetImages();
                var contents = imgs.Select(x => new ImageContent(prj.Design, grid)
                {
                    Name = x.name,
                    Images = x.images,
                });
                grid.Items.Clear();
                grid.Items.AddRange(contents);
                grid.Arrange();
                grid.Invalidate();
            }
        }
        #endregion
        #region RefreshFonts
        void RefreshFonts()
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var fonts = prj.Design.GetFonts() ?? [];
                var ls = fonts.Select(x => new FontDataItem(x.name) { FontDatas = x.fonts }).ToList();
                ls.Insert(0, new FontDataItem("나눔고딕"));

                dgFont.SetDataSource(ls.OrderBy(x => x.FontName));
                dgFont.Invalidate();
            }
        }
        #endregion

        #region ShowResourceManager
        public void ShowResourceManager()
        {
            SetLang();
            RefreshGrid();
            RefreshFonts();
            ShowDialog();
        }
        #endregion
        #endregion
    }
}
