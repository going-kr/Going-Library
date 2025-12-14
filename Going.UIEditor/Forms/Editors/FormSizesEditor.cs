using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Dialogs;
using Going.UIEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Windows.Data.Text;

namespace Going.UIEditor.Forms.Editors
{
    public partial class FormSizesEditor : GoForm
    {
        List<SizesItem> items = [];

        #region Constructor
        public FormSizesEditor()
        {
            InitializeComponent();

            #region dg
            dg.SelectionMode = GoDataGridSelectionMode.Selector;
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Idx", HeaderText = "순번", Size = "80px", });
            dg.Columns.Add(new GoDataGridInputNumberColumn<double> { Name = "Value", HeaderText = "값", Size = "100%", Minimum = 0, Maximum = 9999 });
            dg.Columns.Add(new GoDataGridInputBoolColumn { Name = "IsPixel", HeaderText = "단위", Size = "100px", OffText = "%", OnText = "px" });
            #endregion

            #region Event
            btnOK.ButtonClicked += (o, s) => { if (ValidCheck()) DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;
         
            btnAdd.ButtonClicked += (o, s) => AddItem();
            btnInsert.ButtonClicked += (o, s) => InsertItem();
            btnDel.ButtonClicked += (o, s) => DelItem();
            #endregion
        }
        #endregion

        #region Method
        #region ValueCheck
        bool ValidCheck()
        {
            var ret = true;
            
            return ret;
        }
        #endregion

        #region AddItem
        void AddItem()
        {
            items.Add(new SizesItem { });
            RefreshItems();
        }
        #endregion
        #region Insert
        void InsertItem()
        {
            var sel = dg.Rows.FirstOrDefault(x => x.Selected);
            var idx = sel != null ? dg.Rows.IndexOf(sel) : -1;

            if (idx == -1) AddItem();
            else if(sel?.Source is SizesItem isz)
            {
                var idx2 = items.IndexOf(isz);
                items.Insert(idx2, new SizesItem { });
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
                    if (v.Source is SizesItem sz) 
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

        #region ShowSizesEditor
        public List<SizesItem>? ShowSizesEditor(string title, List<string> vals)
        {
            List<SizesItem>? ret = null;
            this.Title = title;

            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            goLabel1.Text = LM.Items;

            dg.Columns[0].HeaderText = LM.Index;
            dg.Columns[1].HeaderText = LM.Value;
            dg.Columns[2].HeaderText = LM.Unit;

            items.Clear();
            if (vals != null)
                for (int i = 0; i < vals.Count; i++)
                    items.Add(new SizesItem(i + 1, vals[i]));
            RefreshItems();

            if (ShowDialog() == DialogResult.OK)
            {
                ret = items.ToList();
            }
            return ret;
        }
        #endregion
        #endregion
    }

    #region SizesItem
    public class SizesItem
    {
        public int Idx { get; set; }
        public double Value { get; set; }
        public bool IsPixel { get; set; }

        public int? OldIdx { get; private set; }
        public string? OldValue { get; private set; }

        public bool IsDelete { get; private set; }

        public SizesItem() { }
        public SizesItem(int idx, string value)
        {
            OldIdx = idx;
            OldValue = value;

            Idx= idx;
            if (value.EndsWith("%") &&  double.TryParse(value[..^1], out var v1))
            {
                IsPixel = false;
                Value = v1; 
            }
            else if (value.EndsWith("px") && double.TryParse(value[..^1], out var v2))
            {
                IsPixel = true;
                Value = v2;
            }
        }

        public void Delete() => IsDelete = true;
        public string ToValue() => $"{Value}{(IsPixel ? "px" : "%")}";
    }
    #endregion
}
