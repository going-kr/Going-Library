using Going.UI.Design;
using Going.UI.Extensions;
using Going.UI.Forms;
using Going.UI.Forms.Dialogs;
using Going.UI.Json;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using Going.UIEditor.Utils;
using Going.UIEditor.Windows;
using SkiaSharp;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
        PromptWindow? prompt;

        List<EditorWindow> editors = [];
        bool bDownSaveAs = false;
        bool opening = false;
        Timer tmr;
        FileSystemWatcher? fileWatcher;
        System.Threading.Timer? debounceTimer;
        bool isSaving = false;
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
                else toolBox?.Focus();
            };

            tsmiProperties.Click += (o, s) =>
            {
                if (((properties?.IsDisposed ?? false) && !properties.Visible) || properties == null)
                {
                    properties = new  PropertiesWindow { Title = LM.Properties };
                    properties.FormClosed += (o, s) => { properties.Dispose(); properties = null; };
                    properties.Show(dockPanel, DockState.Float);
                }
                else properties?.Focus();
            };
            #endregion
            #region Project
            tsmiDeploy.Click += (o, s) => DeployProject();
            tsmiValidCheck.Click += (o, s) => ValidCheckProject();
            #endregion
            #region Tools
            tsmiResourceManager.Click += (o, s) => ResourceManager();
            tsmiProgramSetting.Click += (o, s) => ProgramSetting();
            tsmiClaude.Click += (o, s) =>
            {
                if (prompt == null || prompt.IsDisposed)
                {
                    prompt = new PromptWindow();
                    prompt.Show(dockPanel, WeifenLuo.WinFormsUI.Docking.DockState.DockBottom);
                }
                else
                {
                    prompt.Activate();
                }
            };
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
                    var p = Program.CurrentDesign;
                    if (p != null)
                        using (var fd = new FolderBrowserDialog())
                        {
                            fd.InitialDirectory = Program.DataMgr.ProjectFolder ?? "";
                            fd.SelectedPath = p.ProjectFolder ?? "";
                            if(fd.ShowDialog() == DialogResult.OK)
                            {
                                p.ProjectFolder = fd.SelectedPath;
                                Program.Edit = true;
                            }
                        }
                }
            };

            btnHotReload.ButtonClicked += (o, s) => HotReload();
            btnValidCheck.ButtonClicked += (o, s) => ValidCheckProject();
            btnDeploy.ButtonClicked += (o, s) => DeployProject();
            #endregion

            #region Language
            Program.DataMgr.LanguageChanged += (o, s) => SetLang();
            #endregion
            #region Timer
            tmr.Tick += (o, s) =>
            {
                var p = Program.CurrentDesign;
                var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var verStr = ver != null ? $" v{ver.Major}.{ver.Minor}.{ver.Build}" : "";
                this.Title = $"Going UI Editor{verStr}{(p != null ? $" - {p.Name}{(Program.Edit || Program.FilePath == null ? "*" : "")}" : "")}";
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
            if (!e.Cancel) StopFileWatcher();

            base.OnClosing(e);
        }
        #endregion

        #region Method
        #region SetLayout
        void SetLayout()
        {
            var p = Program.CurrentDesign;
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
                            case "Going.UIEditor.Windows.PromptWindow":
                                if (!Program.ClaudeInstalled) return null;
                                prompt = new PromptWindow();
                                prompt.FormClosed += (o, s) => { prompt.Dispose(); prompt = null; };
                                return prompt;
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
            var p = Program.CurrentDesign;
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
                btnHotReload.Visible = true;
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
                btnHotReload.Visible = false;
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
            tsmiClaude.Text = $"{LM.Claude}(&C)";
            tsmiClaude.Visible = Program.ClaudeInstalled;
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
                Program.CurrentDesign = new GoDesign { Name = r.Name, DesignWidth = r.Width, DesignHeight = r.Height };
                Program.FilePath = null;
                Program.Edit = false;

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

                var p = Program.CurrentDesign;
                DialogResult ret = DialogResult.OK;
                if (p != null && (Program.Edit || Program.FilePath == null))
                {
                    ret = Program.MessageBox.ShowMessageBoxYesNoCancel(LM.Save, LM.SaveQuestion);
                    if (ret == DialogResult.Yes) SaveFile();
                }

                if (ret != DialogResult.Cancel)
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Title = LM.Open;
                        ofd.InitialDirectory = Program.DataMgr.LastOpenFolder ?? Program.DataMgr.ProjectFolder;
                        ofd.Multiselect = false;
                        ofd.Filter = "Going UI Editor File|*.gud";

                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            var s = File.ReadAllText(ofd.FileName);
                            var v = LoadDesign(s);
                            if (v != null)
                            {
                                Program.CurrentDesign = v;
                                Program.FilePath = ofd.FileName;
                                Program.Edit = false;
                                SetUI();
                                SetLayout();
                            }

                            Program.DataMgr.LastOpenFolder = Path.GetDirectoryName(ofd.FileName);
                            Program.DataMgr.SaveSetting();
                            if (Program.FilePath != null) StartFileWatcher(Program.FilePath);
                        }
                    }
                }
                opening = false;
            }
        }
        #endregion
        #region SaveFile
        void SaveFile()
        {
            var design = Program.CurrentDesign;
            if (design == null) return;

            if (Program.FilePath != null && Directory.Exists(Path.GetDirectoryName(Program.FilePath)))
            {
                var v = design.JsonSerialize();
                try
                {
                    isSaving = true;
                    File.WriteAllText(Program.FilePath, v);
                    Program.Edit = false;
                }
                catch (UnauthorizedAccessException) { Program.MessageBox.ShowMessageBoxOk(LM.Save, LM.SavePermissions); }
                finally { _ = Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ => isSaving = false); }
            }
            else SaveAsFile();
        }

        void SaveAsFile()
        {
            var design = Program.CurrentDesign;
            if (design == null) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = LM.SaveAs;
                sfd.InitialDirectory = Program.DataMgr.ProjectFolder;
                sfd.Filter = "Going UI Editor File|*.gud";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Program.FilePath = sfd.FileName;
                    var v = design.JsonSerialize();
                    try
                    {
                        File.WriteAllText(Program.FilePath, v);
                        Program.Edit = false;
                    }
                    catch (UnauthorizedAccessException) { Program.MessageBox.ShowMessageBoxOk(LM.Save, LM.SavePermissions); }
                }
            }
        }
        #endregion
        #region CloseFile
        DialogResult CloseFile()
        {
            DialogResult ret = DialogResult.OK;
            var p = Program.CurrentDesign;
            if (p != null && (Program.Edit || Program.FilePath == null))
            {
                ret = Program.MessageBox.ShowMessageBoxYesNoCancel(LM.Save, LM.SaveQuestion);
                if (ret == DialogResult.Yes) SaveFile();
            }

            if (ret != DialogResult.Cancel)
            {
                StopFileWatcher();
                if (p != null) dockPanel.SaveAsXml(PATH_LAYOUT);

                Program.CurrentDesign = null;
                Program.FilePath = null;
                Program.Edit = false;
                SetLayout();
                SetUI();
            }

            return ret;
        }
        #endregion
        #region LoadDesign
        GoDesign? LoadDesign(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Design", out var designProp) && designProp.ValueKind == JsonValueKind.String)
                {
                    // 구 구조: { Name, Width, Height, ProjectFolder, Design: "<escaped JSON>" }
                    var designJson = designProp.GetString();
                    var design = GoDesign.JsonDeserialize(designJson);
                    if (design != null)
                    {
                        design.Name = doc.RootElement.TryGetProperty("Name", out var n) ? n.GetString() : null;
                        design.DesignWidth = doc.RootElement.TryGetProperty("Width", out var w) ? w.GetInt32() : 0;
                        design.DesignHeight = doc.RootElement.TryGetProperty("Height", out var h) ? h.GetInt32() : 0;
                        design.ProjectFolder = doc.RootElement.TryGetProperty("ProjectFolder", out var pf) ? pf.GetString() : null;
                    }
                    return design;
                }
                else
                {
                    // 새 구조: GoDesign 직접 직렬화
                    return GoDesign.JsonDeserialize(json);
                }
            }
            catch { return null; }
        }
        #endregion
        #region FileWatcher
        void StartFileWatcher(string filePath)
        {
            StopFileWatcher();
            var dir = Path.GetDirectoryName(filePath);
            var name = Path.GetFileName(filePath);
            if (dir == null || name == null) return;

            fileWatcher = new FileSystemWatcher(dir, "*" + Path.GetExtension(name))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.CreationTime,
                InternalBufferSize = 65536,
                EnableRaisingEvents = true
            };

            void OnFileChanged(object? o, FileSystemEventArgs e)
            {
                if (!string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)) return;
                if (isSaving) return;
                debounceTimer?.Dispose();
                debounceTimer = new System.Threading.Timer(_ =>
                {
                    this.Invoke(() =>
                    {
                        if (isSaving) return;
                        if (Program.Edit)
                        {
                            var ret = Program.MessageBox.ShowMessageBoxYesNo(LM.HotReload, LM.FileChangedReload);
                            if (ret == DialogResult.Yes) HotReload();
                        }
                        else HotReload();
                    });
                }, null, 500, Timeout.Infinite);
            }

            fileWatcher.Changed += OnFileChanged;
            fileWatcher.Created += OnFileChanged;
            fileWatcher.Renamed += (o, e) => OnFileChanged(o, e);
        }

        void StopFileWatcher()
        {
            debounceTimer?.Dispose();
            debounceTimer = null;
            fileWatcher?.Dispose();
            fileWatcher = null;
        }
        #endregion
        #region HotReload
        void HotReload()
        {
            if (Program.FilePath != null && File.Exists(Program.FilePath))
            {
                var s = File.ReadAllText(Program.FilePath);
                var v = LoadDesign(s);
                if (v != null)
                {
                    Program.CurrentDesign = v;
                    Program.Edit = false;

                    v.Init();

                    foreach (var editor in editors)
                    {
                        if (editor.Target is GoDesign)
                            editor.Target = v;
                        else if (editor.Target is GoPage oldPage && oldPage.Name != null && v.Pages.TryGetValue(oldPage.Name, out var newPage))
                            editor.Target = newPage;
                        else if (editor.Target is GoWindow oldWnd && oldWnd.Name != null && v.Windows.TryGetValue(oldWnd.Name, out var newWnd))
                            editor.Target = newWnd;

                        editor.Invalidate();
                    }

                    explorer?.RefreshTreeView();
                }
            }
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
            var p = Program.CurrentDesign;
            if (p != null)
            {
                var (nouse, thm) = Program.ThemeForm.ShowTheme(p.Theme ?? GoTheme.DarkTheme);
                if (nouse) { p.CustomTheme = null; Program.Edit = true; }
                else if (thm != null) { p.CustomTheme = thm; Program.Edit = true; }

                var wnd = dockPanel.ActiveContent as EditorWindow; if (wnd != null) wnd.Invalidate();
            }
        }
        #endregion

        #region ValidCheckProject
        void ValidCheckProject()
        {
            var prj = Program.CurrentDesign;
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
            var prj = Program.CurrentDesign;
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
                                            File.WriteAllText(Path.Combine(dir, r.FileName), r.Code);

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
