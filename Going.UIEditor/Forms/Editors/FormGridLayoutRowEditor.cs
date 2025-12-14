using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Dialogs;
using Going.UI.Utils;
using Going.UIEditor.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Web.UI;
using Timer = System.Windows.Forms.Timer;

namespace Going.UIEditor.Forms.Editors
{
    public partial class FormGridLayoutRowEditor : GoForm
    {
        #region Member Variable
        List<GridRowItem> items = [];
        #endregion

        #region Constructor
        public FormGridLayoutRowEditor()
        {
            InitializeComponent();

            #region dg
            dg.SelectionMode = GoDataGridSelectionMode.Selector;
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Idx", HeaderText = "순번", Size = "80px", });
            dg.Columns.Add(new GoDataGridInputNumberColumn<double> { Name = "Height", HeaderText = "높이", Size = "100px", });
            dg.Columns.Add(new GoDataGridInputBoolColumn { Name = "IsPixel", HeaderText = "단위", Size = "100px", OffText = "%", OnText = "px" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Columns", HeaderText = "컬럼", Size = "100%", TextConverter = (v) => v is List<string> cols ? $"Columns : {cols.Count}" : "" });
            dg.Columns.Add(new GoDataGridButtonColumn { Name = "", HeaderText = "", Size = "40px", IconString = "fa-pen-to-square" });
            #endregion

            #region Event
            btnOK.ButtonClicked += (o, s) => { if (ValidCheck()) DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;

            dg.CellButtonClick += (o, s) =>
            {
                if (s.Cell.Row.Source is GridRowItem data)
                {
                    using (var dlg = new FormSizesEditor())
                    {
                        var ret = dlg.ShowSizesEditor("Columns", data.Columns);
                        if (ret != null)
                        {
                            var itms = ret.Where(x => !x.IsDelete).OrderBy(x => x.Idx).Select(x => x.ToValue()).ToArray();
                            if (itms != null && Util.ValidSizes(itms))
                            {
                                data.Columns = [.. itms];
                                data.ColumnEditData = ret;
                                dg.Invalidate();
                            }
                        }
                    }
                }
            };

            btnAdd.ButtonClicked += (o, s) => AddItem();
            btnInsert.ButtonClicked += (o, s) => InsertItem();
            btnDel.ButtonClicked += (o, s) => DelItem();
            #endregion

            TitleIconString = "fa-list";
        }
        #endregion

        #region Method
        #region ValidCheck
        bool ValidCheck()
        {
            return true;
        }
        #endregion

        #region AddItem
        void AddItem()
        {
            items.Add(new GridRowItem { });
            RefreshItems();
        }
        #endregion
        #region Insert
        void InsertItem()
        {
            var sel = dg.Rows.FirstOrDefault(x => x.Selected);
            var idx = sel != null ? dg.Rows.IndexOf(sel) : -1;

            if (idx == -1) AddItem();
            else if(sel?.Source is GridRowItem igr)
            {
                var idx2 = items.IndexOf(igr);
                items.Insert(idx2, new GridRowItem { });
                RefreshItems();
            }
        }
        #endregion
        #region DelItem
        void DelItem()
        {
            var sels = dg.Rows.Where(x => x.Selected);
            if (sels != null)
            {
                foreach (var v in sels)
                    if (v.Source is GridRowItem sz)
                        sz.Delete();

                RefreshItems();
            }
        }
        #endregion
        #region RefreshItems
        void RefreshItems()
        {
            var ls = items.Where(x => !x.IsDelete);//.OrderBy(x => x.Idx);
            int i = 1;
            foreach (var x in ls) x.Idx = i++;

            dg.SetDataSource(items.Where(x => !x.IsDelete));
            dg.Invalidate();
        }
        #endregion

        #region ShowGridLayoutRowEditor
        public List<GridRowItem>? ShowGridLayoutRowEditor(string title, IEnumerable<GoGridLayoutPanelRow>? vals)
        {
            this.Title = title;

            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            dg.Columns[0].HeaderText = LM.Index;
            dg.Columns[1].HeaderText = LM.Height;
            dg.Columns[2].HeaderText = LM.Unit;
            dg.Columns[3].HeaderText = LM.Column;
            goLabel1.Text = LM.Items;

            items.Clear();
            if (vals != null && vals.ToList() is List<GoGridLayoutPanelRow> vls)
                for (int i = 0; i < vls.Count; i++)
                    items.Add(new GridRowItem(i + 1, vls[i]));
            RefreshItems();

            List<GridRowItem>? ret = null;
            if (ShowDialog() == DialogResult.OK)
            {
                ret = items.ToList();
            }
            return ret;
        }
        #endregion
        #endregion
    }

    #region class : GridRowItem
    public class GridRowItem 
    {
        public int Idx { get; set; }
        public double Height { get; set; }
        public bool IsPixel { get; set; }

        public List<string> Columns { get; set; } = [];
        
        public int? OldIdx { get; private set; }
        public GoGridLayoutPanelRow? OldValue { get; private set; }

        public bool IsDelete { get; private set; }
        public List<SizesItem>? ColumnEditData { get; set; }

        public GridRowItem() { }
        public GridRowItem(int idx, GoGridLayoutPanelRow value)
        {
            OldIdx = idx;
            OldValue = new GoGridLayoutPanelRow { Height = value.Height, Columns = value.Columns.ToList() };

            Idx = idx;
            if (value.Height.EndsWith("%") && double.TryParse(value.Height[..^1], out var v1))
            {
                IsPixel = false;
                Height = v1;
            }
            else if (value.Height.EndsWith("px") && double.TryParse(value.Height[..^1], out var v2))
            {
                IsPixel = true;
                Height = v2;
            }
            Columns = value.Columns.ToList();
        }

        public void Delete() => IsDelete = true;
        public string ToHeight() => $"{Height}{(IsPixel ? "px" : "%")}";
    }
    #endregion
}
