using Going.UI.Extensions;
using Going.UI.Forms;
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
using System.Text;
using WeifenLuo.WinFormsUI.Docking;
using Timer = System.Windows.Forms.Timer;

namespace Going.UIEditor
{
    public partial class FormMain : GoForm
    {
        #region Const
        const string PATH_LAYOUT = "layout.xml";
        #endregion

        #region Properties
        public DockPanel DockPanel => dockPanel;
        #endregion

        #region Member Variable
        ExplorerWindow? explorer;
        ToolBoxWindow? toolBox;
        PropertiesWindow? properties;
        List<EditorWindow> editors = [];
        bool bDownSaveAs = false;
        bool opening = false;
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
                var thm = GoThemeW.Current;
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
            #region Edit
            tsmiUndo.Click += (o, s) => { var wnd = dockPanel.ActiveContent as EditorWindow;  if (wnd != null) wnd.Undo(); };
            tsmiRedo.Click += (o, s) => { var wnd = dockPanel.ActiveContent as EditorWindow; if (wnd != null) wnd.Redo(); };
            tsmiCopy.Click += (o, s) => { var wnd = dockPanel.ActiveContent as EditorWindow; if (wnd != null) wnd.Copy(); };
            tsmiCut.Click += (o, s) => { var wnd = dockPanel.ActiveContent as EditorWindow; if (wnd != null) wnd.Cut(); };
            tsmiPaste.Click += (o, s) => { var wnd = dockPanel.ActiveContent as EditorWindow; if (wnd != null) wnd.Paste(); };
            tsmiDelete.Click += (o, s) => { var wnd = dockPanel.ActiveContent as EditorWindow; if (wnd != null) wnd.Delete(); };
            tsmiSelectAll.Click += (o, s) => { var wnd = dockPanel.ActiveContent as EditorWindow; if (wnd != null) wnd.SelectAll(); };
            #endregion
            #region View
            tsmiExplorer.Click += (o, s) =>
            {
                if (((explorer?.IsDisposed ?? false) && !explorer.Visible) || explorer == null)
                {
                    explorer = new ExplorerWindow { Title = LM.Explorer };
                    explorer.FormClosed += (o, s) => { explorer.Dispose(); explorer = null; };
                    explorer.Show(dockPanel, DockState.Float);
                }
                else explorer?.Focus();
            };

            tsmiToolBox.Click += (o, s) =>
            {
                if (((toolBox?.IsDisposed ?? false) && !toolBox.Visible) || toolBox == null)
                {
                    toolBox = new ToolBoxWindow { Title = LM.ToolBox };
                    toolBox.FormClosed += (o, s) => { toolBox.Dispose(); toolBox = null; };
                    toolBox.Show(dockPanel, DockState.Float);
                }
                else explorer?.Focus();
            };

            tsmiProperties.Click += (o, s) =>
            {
                if (((properties?.IsDisposed ?? false) && !properties.Visible) || properties == null)
                {
                    properties = new  PropertiesWindow { Title = LM.Properties };
                    properties.FormClosed += (o, s) => { properties.Dispose(); properties = null; };
                    properties.Show(dockPanel, DockState.Float);
                }
                else explorer?.Focus();
            };
            #endregion
            #region Project
            tsmiDeploy.Click += (o, s) => DeployProject();
            tsmiValidCheck.Click += (o, s) => ValidCheckProject();
            #endregion
            #region Tools
            tsmiResourceManager.Click += (o, s) => ResourceManager();
            tsmiProgramSetting.Click += (o, s) => ProgramSetting();
            #endregion
            #endregion

            #region ToolBar
            btnNew.ButtonClicked += (o, s) => NewFile();
            btnOpen.ButtonClicked += (o, s) => OpenFile();
            btnSave.ButtonClicked += (o, s) => SaveFile();
            btnSaveAs.ButtonClicked += (o, s) => SaveAsFile();
            btnProgramSetting.ButtonClicked += (o, s) => ProgramSetting();
            btnResourceManager.ButtonClicked += (o, s) => ResourceManager();
            btnTheme.ButtonClicked += (o, s) => ThemeEditor();

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

            btnValidCheck.ButtonClicked += (o, s) => ValidCheckProject();
            btnDeploy.ButtonClicked += (o, s) => DeployProject();
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

                if (p != null)
                {
                    var wnd = dockPanel.ActiveContent as EditorWindow;
                    tsmiUndo.Enabled = wnd != null && wnd.CanUndo;
                    tsmiRedo.Enabled = wnd != null && wnd.CanRedo;
                    tsmiCopy.Enabled = wnd != null;
                    tsmiCut.Enabled = wnd != null;
                    tsmiPaste.Enabled = wnd != null;
                    tsmiDelete.Enabled = wnd != null && wnd.SelectedItemCount > 0;
                    tsmiSelectAll.Enabled = wnd != null;
                }
            };
            #endregion
            #region Theme
           
            #endregion
            #endregion

            SetLayout();
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
        #region SetLayout
        void SetLayout()
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                #region Close
                explorer?.Close();
                toolBox?.Close();
                properties?.Close();

                var ls = editors.ToList();
                editors.Clear();
                foreach (var w in ls) w.Close();
                #endregion

                #region Layout
                if (File.Exists(PATH_LAYOUT))
                {
                    dockPanel.LoadFromXml(PATH_LAYOUT, new DeserializeDockContent((s) =>
                    {
                        switch (s)
                        {
                            case "Going.UIEditor.Windows.ExplorerWindow":
                                explorer = new ExplorerWindow { Title = LM.Explorer };
                                explorer.FormClosed += (o, s) => { explorer.Dispose(); explorer = null; };
                                return explorer;
                            case "Going.UIEditor.Windows.ToolBoxWindow":
                                toolBox = new ToolBoxWindow { Title = LM.ToolBox };
                                toolBox.FormClosed += (o, s) => { toolBox.Dispose(); toolBox = null; };
                                return toolBox;
                            case "Going.UIEditor.Windows.PropertiesWindow":
                                properties = new PropertiesWindow { Title = LM.Properties };
                                properties.FormClosed += (o, s) => { properties.Dispose(); properties = null; };
                                return properties;
                            default:
                                return null;
                        }

                    }));
                }
                else
                {
                    explorer = new ExplorerWindow { Title = LM.Explorer };
                    toolBox = new ToolBoxWindow { Title = LM.ToolBox };
                    properties = new PropertiesWindow { Title = LM.Properties };

                    explorer.FormClosed += (o, s) => { explorer.Dispose(); explorer = null; };
                    toolBox.FormClosed += (o, s) => { toolBox.Dispose(); toolBox = null; };
                    properties.FormClosed += (o, s) => { properties.Dispose(); properties = null; };

                    explorer.Show(dockPanel, DockState.DockLeft);
                    toolBox.Show(explorer.Pane, DockAlignment.Bottom, 0.7);
                    properties.Show(dockPanel, DockState.DockRight);
                }
                #endregion
            }
            else
            {
                #region Close
                explorer?.Close();
                toolBox?.Close();
                properties?.Close();

                var ls = editors.ToList();
                editors.Clear();
                foreach (var w in ls) w.Close();
                #endregion
            }
        }
        #endregion
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

                btnTheme.Visible = btnResourceManager.Visible = true;
                valPath.Visible = true;
                btnValidCheck.Visible = true;
                btnDeploy.Visible = true;
                #endregion
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

                btnTheme.Visible = btnResourceManager.Visible = false;
                valPath.Visible = false;
                btnValidCheck.Visible = false;
                btnDeploy.Visible = false;
                #endregion
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
            tsmiDeploy.Text = $"{LM.Deploy}(&D)";
            tsmiProjectProps.Text = $"{LM.ProjectProps}(&P)";
            #endregion
            #region Tool
            tsmiTool.Text = $"{LM.Tool}(&T)";
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

                SetLayout();
                SetUI();
            }
        }
        #endregion
        #region OpenFile
        void OpenFile()
        {
            if (!opening)
            {
                opening = true;

                var p = Program.CurrentProject;
                DialogResult ret = DialogResult.OK;
                if (p != null && (p.Edit || p.FilePath == null))
                {
                    ret = Program.MessageBox.ShowMessageBoxYesNoCancel(LM.Save, LM.SaveQuestion);
                    if (ret == DialogResult.Yes) p.Save();
                }

                if (ret != DialogResult.Cancel)
                {
                    var v = Project.Open();
                    if (v != null)
                    {
                        Program.CurrentProject = v;
                        SetUI();
                        SetLayout();
                    }
                }
                opening = false;
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
                if (p != null) dockPanel.SaveAsXml(PATH_LAYOUT);

                Program.CurrentProject = null;
                SetLayout();
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

                    var pwnd = dockPanel.Contents.FirstOrDefault(x => x is PropertiesWindow);
                    if (pwnd is PropertiesWindow props) props.SelectObjects(wnd, null);
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
        #region ResourceManager
        void ResourceManager()
        {
            Program.ResourceForm.ShowResourceManager();
        }
        #endregion
        #region ThemeEditor
        void ThemeEditor()
        {
            var p = Program.CurrentProject;
            if (p != null && p.Design != null)
            {
                var (nouse, thm) = Program.ThemeForm.ShowTheme(p.Design.Theme ?? GoTheme.DarkTheme);
                if (nouse) { p.Design.CustomTheme = null; p.Edit = true; }
                else if (thm != null) { p.Design.CustomTheme = thm; p.Edit = true; }

                var wnd = dockPanel.ActiveContent as EditorWindow; if (wnd != null) wnd.Invalidate();
            }
        }
        #endregion

        #region ValidCheckProject
        void ValidCheckProject()
        {
            var prj = Program.CurrentProject;
            if(prj != null)
            {
                if (Code.ValidCode(prj))
                    Program.MessageBox.ShowMessageBoxOk(LM.Validation, LM.ValidationOK);
                else
                    Program.MessageBox.ShowMessageBoxOk(LM.Validation, LM.ValidationFail);
            }
        }
        #endregion
        #region DeployProject
        void DeployProject()
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                if (Directory.Exists(prj.ProjectFolder))
                {
                    if (Code.ValidCode(prj))
                    {
                        var ret = Code.MakeCode(prj);
                        if (ret != null)
                        {
                            foreach (var k1 in ret.Keys)
                            {
                                var dic1 = ret[k1];
                                foreach (var k2 in dic1.Keys)
                                {
                                    var r = dic1[k2];
                                    var path = prj.ProjectFolder;
                                    var dir = Path.Combine(path, k1);
                                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                    var filename = Path.Combine(dir, r.FileName);

                                    if (r.ExistsCheck)
                                    {
                                        if (!File.Exists(filename)) File.WriteAllText(filename, r.Code, Encoding.Unicode);
                                    }
                                    else File.WriteAllText(filename, r.Code, Encoding.Unicode);

                                    if(k2 == "design.json")
                                    {
                                        try
                                        {
                                            if (Directory.Exists(Path.Combine(dir, "bin", "Debug")))
                                                foreach (var d in Directory.GetDirectories(Path.Combine(dir, "bin", "Debug")))
                                                    File.WriteAllText(Path.Combine(d, r.FileName), r.Code);

                                            if (Directory.Exists(Path.Combine(dir, "bin", "Release")))
                                                foreach (var d in Directory.GetDirectories(Path.Combine(dir, "bin", "Release")))
                                                    File.WriteAllText(Path.Combine(d, r.FileName), r.Code);
                                        }
                                        catch { }
                                    }
                                }
                            }

                            Program.MessageBox.ShowMessageBoxOk(LM.Deploy, LM.DeployComplete);
                        }
                    }
                    else Program.MessageBox.ShowMessageBoxOk(LM.FaildDeploy, LM.ValidationFail);
                }
                else Program.MessageBox.ShowMessageBoxOk(LM.FaildDeploy, LM.InvalidProjectFolder);
            }
        }
        #endregion
        #endregion

    }
}
