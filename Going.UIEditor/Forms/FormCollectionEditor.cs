using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Dialogs;
using Going.UIEditor.Utils;
using Going.UIEditor.Windows;
using GuiLabs.Undo;
using SkiaSharp;
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
using Timer = System.Windows.Forms.Timer;

namespace Going.UIEditor.Forms
{
    public partial class FormCollectionEditor<T> : GoForm
    {
        #region Member Variable
        Timer tmr;
        List<CollectionEditorItem<T>> items = [];
        #endregion

        #region Constructor
        public FormCollectionEditor()
        {
            InitializeComponent();

            #region dg
            dg.SelectionMode = GoDataGridSelectionMode.Single;
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Text", HeaderText = "항목", Size = "100%", });
            #endregion

            #region Timer
            tmr = new Timer { Interval = 10, Enabled = true };
            tmr.Tick += (o, s) =>
            {
                if (Visible)
                {
                    btnDel.Enabled = dg.Rows.Any(x => x.Selected);
                    pg.Invalidate();
                    dg.Invalidate();
                }
            };
            #endregion

            #region Event
            btnOK.ButtonClicked += (o, s) => { if (ValidCheck()) DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;

            dg.SelectedChanged += (o, s) =>
            {
                var v = dg.Rows.FirstOrDefault(x => x.Selected)?.Source as CollectionEditorItem<T>;
                if (v != null && v.Value != null)
                {
                    pg.SelectedObjects = [v.Value];
                }
            };

            btnAdd.ButtonClicked += (o, s) => AddItem();
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

        #region ShowCollectionEditor
        public List<T>? ShowCollectionEditor(string title,  IEnumerable<T>? vals)
        {
            this.Title = title;

            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            dg.Columns[0].HeaderText = LM.Item;
            goLabel1.Text = LM.Items;
            goLabel2.Text = LM.Properties;
            items.Clear();
            if (vals != null) items.AddRange(vals.Select(x => new CollectionEditorItem<T>(x)) ?? []);
            dg.SetDataSource(items);

            List<T>? ret = null;
            if (ShowDialog() == DialogResult.OK)
            {
                ret = items.Select(x => x.Value!).ToList();
            }
            return ret;
        }
        #endregion

        #region AddItem
        void AddItem()
        {
            items.Add(new CollectionEditorItem<T>(Activator.CreateInstance<T>()));
            dg.SetDataSource(items);
            dg.Invalidate();
        }
        #endregion
        #region DelItem
        void DelItem()
        {
            var sel = dg.Rows.FirstOrDefault(x => x.Selected)?.Source as CollectionEditorItem<T>;
            if (sel != null)
            {
                items.Remove(sel);
                dg.SetDataSource(items);
                dg.Invalidate();
            }
        }
        #endregion

        #region IsCollection
        public static bool IsCollection(PropertyInfo? Info) => Info != null && typeof(IEnumerable).IsAssignableFrom(Info.PropertyType) && Info.PropertyType != typeof(string) && !Attribute.IsDefined(Info, typeof(GoSizesPropertyAttribute));
        #endregion
        #endregion

    }

    #region class : CollectionEditorItem
    class CollectionEditorItem<T> (T val)
    {
        public string Text => Value?.ToString() ?? "";
        public T Value => val;
    }
    #endregion
}
