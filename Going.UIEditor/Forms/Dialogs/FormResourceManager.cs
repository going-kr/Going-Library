using Going.UI.Forms.Dialogs;
using Going.UIEditor.Controls;
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
            #region event
            #region btnClose
            btnClose.ButtonClicked += (o, s) => DialogResult = DialogResult.OK;
            #endregion
            #region btnAdd
            btnAdd.ButtonClicked += (o, s) =>
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
                                var name = Path.GetFileNameWithoutExtension(filename);
                                var data = File.ReadAllBytes(filename);
                                var imgs = prj.Design.GetImages();
                                if (imgs.Any(x=>x.name == name))
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
            #region btnDel
            btnDel.ButtonClicked += (s, e) =>
            {
                var prj = Program.CurrentProject;
                if (prj != null && grid.SelectedItems.Count > 0)
                {
                    foreach (var item in grid.SelectedItems)
                        if (item is ImageContent c)
                            prj.Design.RemoveImage(c.Name);
                    prj.Edit = true;

                    RefreshGrid();
                }
            };
            #endregion
            #endregion
        }
        #endregion


        #region Method
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

        #region ShowResourceManager
        public void ShowResourceManager()
        {
            Title = LM.Resources;
            lblTitle.Text = LM.ImageList;
            btnClose.Text = LM.Close;
            RefreshGrid();
            ShowDialog();
        }
        #endregion
        #endregion
    }
}
