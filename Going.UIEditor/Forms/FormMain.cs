using Going.UI.Extensions;
using Going.UI.Forms.Dialogs;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using Going.UIEditor.Datas;
using Going.UIEditor.Utils;
using Going.UIEditor.Windows;
using SkiaSharp;
using System.ComponentModel;
using System.Security.Cryptography;
using WeifenLuo.WinFormsUI.Docking;
using Timer = System.Windows.Forms.Timer;

namespace Going.UIEditor
{
    public partial class FormMain : GoForm
    {
        #region Member Variable
        ExplorerWindow? explorer;
        ToolBoxWindow? toolBox;
        PropertiesWindow? properties;
        List<EditorWindow> editors = [];
        bool bDownSaveAs = false;
        Timer tmr;
        #endregion

        #region Constructor
        public FormMain()
        {
            InitializeComponent();
            dockPanel.DockLeftPortion = dockPanel.DockRightPortion = 0.175;
            #region new
            dockPanel.Theme = new xTheme();
            dockPanel.ShowDocumentIcon = true;
            tmr = new Timer { Interval = 10, Enabled = true };
            #endregion
            #region event
            #region btnSaveAs.ContentDraw
            btnSaveAs.MouseDown += (o, s) => bDownSaveAs = true;
            btnSaveAs.MouseUp += (o, s) => bDownSaveAs = false;
            btnSaveAs.ContentDraw += (o, s) =>
            {
                var thm = GoTheme.Current;
                var cT = thm.ToColor(btnSaveAs.TextColor);
                var rt = MathTool.MakeRectangle(Util.FromRect(0, 0, btnSaveAs.Width, btnSaveAs.Height), new SKSize(9, 9));
                rt.Offset(7, 7 + (bDownSaveAs ? 1 : 0));
                Util.DrawIcon(s.Canvas, "fa-asterisk", 9, rt, cT.BrightnessTransmit(bDownSaveAs ? thm.DownBrightness : 0F), Util.FromArgb(32, 32, 32), 3);
            };
            #endregion

            #region Menu
            #region File
            tsmiNew.Click += (o, s) => NewFile();
            tsmiOpen.Click += (o, s) => OpenFile();
            tsmiSave.Click += (o, s) => SaveFile();
            tsmiSaveAs.Click += (o, s) => SaveAsFile();
            tsmiClose.Click += (o, s) => CloseFile();
            tsmiExit.Click += (o, s) => Close();
            #endregion
            #region View
            tsmiExplorer.Click += (o, s) =>
            {
                if ((explorer?.IsDisposed ?? false) && !explorer.Visible)
                {
                    explorer = new ExplorerWindow { Title = LM.Explorer };
                    explorer.FormClosed += (o, s) => { explorer.Dispose(); explorer = null; };
                    explorer.Show(dockPanel, DockState.Float);
                }
                else explorer?.Focus();
            };

            tsmiToolBox.Click += (o, s) =>
            {
                if ((toolBox?.IsDisposed ?? false) && !toolBox.Visible)
                {
                    toolBox = new ToolBoxWindow { Title = LM.ToolBox };
                    toolBox.FormClosed += (o, s) => { toolBox.Dispose(); toolBox = null; };
                    toolBox.Show(dockPanel, DockState.Float);
                }
                else explorer?.Focus();
            };

            tsmiProperties.Click += (o, s) =>
            {
                if ((properties?.IsDisposed ?? false) && !properties.Visible)
                {
                    properties = new  PropertiesWindow { Title = LM.Properties };
                    properties.FormClosed += (o, s) => { properties.Dispose(); properties = null; };
                    properties.Show(dockPanel, DockState.Float);
                }
                else explorer?.Focus();
            };
            #endregion
            tsmiProgramSetting.Click += (o, s) => ProgramSetting();
            #endregion

            #region ToolBar
            btnNew.ButtonClicked += (o, s) => NewFile();
            btnOpen.ButtonClicked += (o, s) => OpenFile();
            btnSave.ButtonClicked += (o, s) => SaveFile();
            btnSaveAs.ButtonClicked += (o, s) => SaveAsFile();
            btnProgramSetting.ButtonClicked += (o, s) => ProgramSetting();

            valPath.ButtonClicked += (o, s) =>
            {
                if(s.Button.Name == "select")
                {
                    var p = Program.CurrentProject;
                    if (p != null)
                        using (var fd = new FolderBrowserDialog())
                        {
                            fd.InitialDirectory = Program.DataMgr.ProjectFolder ?? "";
                            fd.SelectedPath = p.ProjectFolder ?? "";
                            if(fd.ShowDialog() == DialogResult.OK)
                            {
                                p.ProjectFolder = fd.SelectedPath;
                                p.Edit = true;
                            }
                        }
                }
            };
            #endregion

            #region Language
            Program.DataMgr.LanguageChanged += (o, s) => SetLang();
            #endregion
            #region Timer
            tmr.Tick += (o, s) =>
            {
                var p = Program.CurrentProject;
                this.Title = $"Going UI Editor{(p != null ? $" - {p.Name}{(p.Edit || p.FilePath == null ? "*" : "")}" : "")}";
                valPath.Value = Util2.EllipsisPath(p?.ProjectFolder ?? "", valPath.FontName, valPath.FontStyle, valPath.FontSize, valPath.Width - (valPath.TitleSize ?? 0) - (valPath.ButtonSize ?? 0) - 20);
            };
            #endregion
            #endregion

            SetUI();
            SetLang();
        }
        #endregion

        #region Override
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = CloseFile() == DialogResult.Cancel;

            base.OnClosing(e);
        }
        #endregion

        #region Method
        #region SetUI
        void SetUI()
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                #region UI
                tsmiFile.Visible = true;
                tsmiEdit.Visible = true;
                tsmiView.Visible = true;
                tsmiProj.Visible = true;
                tsmiTool.Visible = true;
                tsmiHelp.Visible = true;

                tsmiNew.Enabled = btnNew.Enabled = true;
                tsmiOpen.Enabled = btnOpen.Enabled = true;
                tsmiSave.Enabled = btnSave.Enabled = true;
                tsmiSaveAs.Enabled = btnSaveAs.Enabled = true;
                tsmiClose.Enabled = true;

                valPath.Visible = true;
                btnDeploy.Visible = true;
                #endregion

                explorer = new ExplorerWindow { Title = LM.Explorer};
                toolBox = new ToolBoxWindow { Title = LM.ToolBox };
                properties = new PropertiesWindow { Title = LM.Properties };

                explorer.FormClosed += (o, s) => { explorer.Dispose(); explorer = null; };
                toolBox.FormClosed += (o, s) => { toolBox.Dispose(); toolBox = null; };
                properties.FormClosed += (o, s) => { properties.Dispose(); properties = null; };

                explorer.Show(dockPanel, DockState.DockLeft);
                toolBox.Show(explorer.Pane, DockAlignment.Bottom, 0.7);
                properties.Show(dockPanel, DockState.DockRight);
            }
            else
            {
                #region Empty
                tsmiFile.Visible = true;
                tsmiEdit.Visible = false;
                tsmiView.Visible = false;
                tsmiProj.Visible = false;
                tsmiTool.Visible = false;
                tsmiHelp.Visible = true;

                tsmiNew.Enabled = btnNew.Enabled = true;
                tsmiOpen.Enabled = btnOpen.Enabled = true;
                tsmiSave.Enabled = btnSave.Enabled = false;
                tsmiSaveAs.Enabled = btnSaveAs.Enabled = false;
                tsmiClose.Enabled = false;

                valPath.Visible = false;
                btnDeploy.Visible = false;
                #endregion

                explorer?.Close();
                toolBox?.Close();
                properties?.Close();
                
                var ls = editors.ToList();
                editors.Clear();
                foreach (var w in ls) w.Close();
               
            }
        }
        #endregion
        #region SetLang
        void SetLang()
        {
            #region File
            tsmiFile.Text = $"{LM.File}(&F)";
            tsmiNew.Text = $"{LM.NewFile}(&N)";
            tsmiOpen.Text = $"{LM.Open}(&O)";
            tsmiSave.Text = $"{LM.Save}(&S)";
            tsmiSaveAs.Text = $"{LM.SaveAs}(&A)";
            tsmiClose.Text = $"{LM.Close}(&C)";
            tsmiExit.Text = $"{LM.Exit}(&X)";
            #endregion
            #region Edit
            tsmiEdit.Text = $"{LM.Edit}(&E)";
            tsmiUndo.Text = $"{LM.Undo}(&U)";
            tsmiRedo.Text = $"{LM.Redo}(&R)";
            tsmiCopy.Text = $"{LM.Copy}(&C)";
            tsmiCut.Text = $"{LM.Cut}(&T)";
            tsmiPaste.Text = $"{LM.Paste}(&P)";
            tsmiDelete.Text = $"{LM.Delete}(&D)";
            tsmiSelectAll.Text = $"{LM.SelectAll}(&A)";
            #endregion
            #region View
            tsmiView.Text = $"{LM.View}(&V)";
            tsmiExplorer.Text = $"{LM.Explorer}(&E)";
            tsmiToolBox.Text = $"{LM.ToolBox}(&T)";
            tsmiProperties.Text = $"{LM.Properties}(&P)";
            #endregion
            #region Project
            tsmiProj.Text = $"{LM.Project}(&P)";
            tsmiValidCheck.Text = $"{LM.Validation}(&V)";
            tsmiDeploy.Text = $"{LM.ToolBox}(&D)";
            tsmiProjectProps.Text = $"{LM.ProjectProps}(&P)";
            #endregion
            #region Tool
            tsmiTool.Text = $"{LM.Project}(&T)";
            tsmiResourceManager.Text = $"{LM.Resources}(&R)";
            tsmiProgramSetting.Text = $"{LM.ProgramSetting}(&S)";
            #endregion
            #region Help
            tsmiHelp.Text = $"{LM.Help}(&H)";
            tsmiProgramInfo.Text = $"{LM.ProgramInfo}(&A)";
            #endregion

            valPath.Title = LM.ProjectPath;

            #region Dialogs
            if (explorer != null) explorer.Title = LM.Explorer;
            if (toolBox != null) toolBox.Title = LM.ToolBox;
            if (properties != null) properties.Title = LM.Properties;

            if (Program.MessageBox != null)
            {
                Program.MessageBox.OkText = LM.Ok;
                Program.MessageBox.CancelText = LM.Cancel;
                Program.MessageBox.YesText = LM.Yes;
                Program.MessageBox.NoText = LM.No;
            }

            if (Program.InputBox != null)
            {
                Program.InputBox.OkText = LM.Ok;
                Program.InputBox.CancelText = LM.Cancel;
            }

            if (Program.SelBox != null)
            {
                Program.SelBox.OkText = LM.Ok;
                Program.SelBox.CancelText = LM.Cancel;
            }
            #endregion
        }
        #endregion

        #region NewFile
        void NewFile()
        {
            var r = Program.NewFileForm.ShowNewFile();
            if (r != null)
            {
                Program.CurrentProject = new Datas.Project { Name = r.Name, Width = r.Width, Height = r.Height  };

                SetUI();

            }
        }
        #endregion
        #region OpenFile
        void OpenFile()
        {
            var p = Program.CurrentProject;
            if (p != null && (p.Edit || p.FilePath == null)) p.Save();

            var v = Project.Open();
            if(v != null)
            {
                Program.CurrentProject = v;
                SetUI();
            }
        }
        #endregion
        #region SaveFile
        void SaveFile() => Program.CurrentProject?.Save();
        void SaveAsFile() => Program.CurrentProject?.SaveAs();
        #endregion
        #region CloseFile
        DialogResult CloseFile()
        {
            DialogResult ret = DialogResult.OK;
            var p = Program.CurrentProject;
            if (p != null && (p.Edit || p.FilePath == null))
            {
                ret = Program.MessageBox.ShowMessageBoxYesNoCancel(LM.Save, LM.SaveQuestion);
                if (ret == DialogResult.Yes) p.Save();
            }

            if (ret != DialogResult.Cancel)
            {
                Program.CurrentProject = null;
                SetUI();
            }

            return ret;
        }
        #endregion

        #region ShowEditorWindow
        public void ShowEditorWindow(object? editorTarget)
        {
            if (editorTarget != null)
            {
                var o = editors.FirstOrDefault(x => x.Target == editorTarget);
                if (o == null)
                {
                    var wnd = new EditorWindow(editorTarget);
                    wnd.FormClosed += (o, s) => { if (o is EditorWindow w && editors.Contains(w)) editors.Remove(w); };
                    editors.Add(wnd);

                    wnd.Show(dockPanel, DockState.Document);
                }
                else o.Activate();
            }
        }
        #endregion

        #region ProgramSetting
        void ProgramSetting()
        {
            var ret = Program.SettingForm.ShowSetting();
            if (ret != null)
            {
                Program.DataMgr.ProjectFolder = ret.ProjectFolder;
                Program.DataMgr.Language = ret.Language;
                Program.DataMgr.SaveSetting();
            }
        }
        #endregion
        #endregion

    }
}
