using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.ImageCanvas;
using Going.UIEditor.Datas;
using Going.UIEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Windows
{
    public partial class ExplorerWindow : xWindow
    {
        #region Const
        const string Master = "Master";
        const string Pages = "Pages";
        const string Windows = "Windows";
        #endregion

        #region Constructor
        public ExplorerWindow()
        {
            InitializeComponent();

            TitleIconString = "fa-folder-tree";
            Title = "Explorer";

            #region set
            tvExplorer.IconSize = 14;
            tvExplorer.IconGap = 10;
            tvExplorer.SelectionMode = UI.Enums.GoItemSelectionMode.Single;
            tvExplorer.Nodes.Add(new GoTreeNode { Text = Master, IconString = "fa-pager" });
            tvExplorer.Nodes.Add(new GoTreeNode { Text = Pages, IconString = "fa-folder-open" });
            tvExplorer.Nodes.Add(new GoTreeNode { Text = Windows, IconString = "fa-window-restore" });
            #endregion
            #region event
            #region tvExplorer.MouseDown
            tvExplorer.MouseDown += (o, s) =>
            {
                if (s.Button == MouseButtons.Right)
                {
                    tsmiExplorerAdd.Text = $"{LM.Add}(&A)";
                    tsmiExplorerDelete.Text = $"{LM.Delete}(&D)";
                    tsmiExplorerRename.Text = $"{LM.Rename}(&N)";

                    cmsMenu.Items.Clear();

                    var itm = tvExplorer.GetTreeNode(s.X, s.Y);
                    if (itm != null)
                    {
                        if (itm.Text == Pages && itm.Parent == null)
                        {
                            cmsMenu.Items.Add(tsmiExplorerAdd);
                        }
                        if (itm.Text == Windows && itm.Parent == null)
                        {
                            cmsMenu.Items.Add(tsmiExplorerAdd);
                        }
                        else if ((itm.Parent?.Text == Pages || itm.Parent?.Text == Windows) && itm.Parent?.Parent == null)
                        {
                            cmsMenu.Items.Add(tsmiExplorerDelete);
                            cmsMenu.Items.Add(tsmiSep);
                            cmsMenu.Items.Add(tsmiExplorerRename);
                        }
                    }
                }
            };
            #endregion
            #region tvExplorer.ItemDoubleClicked
            tvExplorer.ItemDoubleClicked += (o, s) =>
            {
                var itm = tvExplorer.SelectedNodes.FirstOrDefault();

                if (itm != null)
                {
                    var p = Program.CurrentProject;
                    if (itm.Text == Master) Program.MainForm.ShowEditorWindow(p.Design);
                    else if (itm.Tag is GoPage page) Program.MainForm.ShowEditorWindow(page);
                    else if (itm.Tag is GoWindow wnd) Program.MainForm.ShowEditorWindow(wnd);
                }
            };
            #endregion
            #region tsmiExplorerAdd.Click
            tsmiExplorerAdd.Click += (o, s) =>
            {
                var p = Program.CurrentProject;
                if (p != null)
                {
                    var nm = tvExplorer.SelectedNodes.FirstOrDefault()?.Text;

                    if (nm == Pages)
                    {
                        #region Page Add
                        var ret = Program.NewPageForm.ShowNewPage();
                        if (ret != null)
                        {
                            if (!p.Design.Pages.ContainsKey(ret.Name))
                            {
                                p.Design.AddPage(ret.UseIcPage ? new IcPage { Name = ret.Name } : new GoPage { Name = ret.Name });
                                p.Edit = true;
                                RefreshTreeView();
                            }
                            else
                            {
                                Program.MessageBox.ShowMessageBoxOk(LM.Duplication, LM.ExistsPage);
                            }
                        }
                        #endregion
                    }
                    else if (nm == Windows)
                    {
                        #region Window Add
                        var ret = Program.InputBox.ShowInputBox<WindowInfo>($"{LM.Window} {LM.Add}", 1, new());
                        if (ret != null)
                        {
                            if (!p.Design.Windows.ContainsKey(ret.Name))
                            {
                                p.Design.Windows.Add(ret.Name, new GoWindow { Name = ret.Name, Width = ret.Width, Height = ret.Height });
                                p.Edit = true;
                                RefreshTreeView();
                            }
                            else
                            {
                                Program.MessageBox.ShowMessageBoxOk(LM.Duplication, LM.ExistsWindows);
                            }
                        }
                        #endregion
                    }
                }
            };
            #endregion
            #region tsmiExplorerDelete.Click
            tsmiExplorerDelete.Click += (o, s) =>
            {
                var p = Program.CurrentProject;
                if (p != null)
                {
                    var nm = tvExplorer.SelectedNodes.FirstOrDefault()?.Parent?.Text;
                    if (nm == Pages)
                    {
                        #region Page Delete
                        var v = tvExplorer.SelectedNodes.FirstOrDefault()?.Tag as GoPage;
                        if (v != null && v.Name != null)
                        {
                            if (Program.MessageBox.ShowMessageBoxYesNo(LM.Delete, $"'{v.Name}' {LM.DeletePage}") == DialogResult.Yes)
                            {
                                p.Design.Pages.Remove(v.Name);
                                p.Edit = true;
                                RefreshTreeView();

                                var wnd = DockPanel.Contents.FirstOrDefault(x => x is EditorWindow editor && editor.Target == v) as EditorWindow;
                                wnd?.Close();
                            }
                        }
                        #endregion
                    }
                    else if (nm == Windows)
                    {
                        #region Window Delete
                        var v = tvExplorer.SelectedNodes.FirstOrDefault()?.Tag as GoWindow;
                        if (v != null && v.Name != null)
                        {
                            if (Program.MessageBox.ShowMessageBoxYesNo(LM.Delete, $"'{v.Name}' {LM.DeleteWindow}") == DialogResult.Yes)
                            {
                                p.Design.Windows.Remove(v.Name);
                                p.Edit = true;
                                RefreshTreeView();
                            }
                        }
                        #endregion
                    }
                }
            };
            #endregion
            #region tsmiExplorerRename.Click
            tsmiExplorerRename.Click += (o, s) =>
            {
                var ret = Program.InputBox.ShowString(LM.Rename);

                if (ret != null)
                {
                    var p = Program.CurrentProject;
                    if (p != null)
                    {
                        var nm = tvExplorer.SelectedNodes.FirstOrDefault()?.Parent?.Text;
                        if (nm == Pages)
                        {
                            #region Page Rename
                            var v = tvExplorer.SelectedNodes.FirstOrDefault()?.Tag as GoPage;
                            if (v != null)
                            {
                                if (!p.Design.Pages.ContainsKey(ret))
                                {
                                    var ls = p.Design.Pages.Values.ToList();
                                    v.Name = ret;
                                    p.Design.Pages.Clear();
                                    foreach (var i in ls) p.Design.Pages.Add(i.Name!, i);

                                    p.Edit = true;
                                    RefreshTreeView();
                                }
                                else if (ret != v.Name)
                                {
                                    Program.MessageBox.ShowMessageBoxOk(LM.Duplication, LM.ExistsPage);
                                }
                            }
                            #endregion
                        }
                        else if (nm == Windows)
                        {
                            #region Window Rename
                            var v = tvExplorer.SelectedNodes.FirstOrDefault()?.Tag as GoWindow;
                            if (v != null)
                            {
                                if (!p.Design.Windows.ContainsKey(ret))
                                {
                                    var ls = p.Design.Windows.Values.ToList();
                                    v.Name = ret;
                                    p.Design.Windows.Clear();
                                    foreach (var i in ls) p.Design.Windows.Add(i.Name!, i);

                                    p.Edit = true;
                                    RefreshTreeView();
                                }
                                else if (ret != v.Name)
                                {
                                    Program.MessageBox.ShowMessageBoxOk(LM.Duplication, LM.ExistsPage);
                                }
                            }
                            #endregion
                        }

                        if (DockPanel.Contents.FirstOrDefault(x => x is PropertiesWindow) is PropertiesWindow pw) pw.RefreshGrid();

                    }
                }
            };
            #endregion
            #endregion
        }
        #endregion

        #region Override
        #region OnShown
        protected override void OnShown(EventArgs e)
        {
            RefreshTreeView();
            base.OnShown(e);
        }
        #endregion
        #endregion

        #region Method
        public void RefreshTreeView()
        {
            var ndPages = tvExplorer.Nodes[1];
            var ndWnds = tvExplorer.Nodes[2];
            ndPages.Nodes.Clear();
            ndWnds.Nodes.Clear();

            var p = Program.CurrentProject;
            if(p != null)
            {
                ndPages.Nodes.AddRange(p.Design.Pages.Select(x => new GoTreeNode 
                {
                    Text = x.Value.Name,
                    IconString = "fa-file-image",
                    Tag = x.Value
                }));

                ndWnds.Nodes.AddRange(p.Design.Windows.Select(x => new GoTreeNode
                {
                    Text = x.Value.Name,
                    IconString = "fa-window-maximize",
                    Tag = x.Value
                }));
            }

            tvExplorer.Invalidate();
        }
        #endregion
    }
}
