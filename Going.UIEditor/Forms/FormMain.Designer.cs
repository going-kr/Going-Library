namespace Going.UIEditor
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            UI.Datas.GoButtonItem goButtonItem1 = new UI.Datas.GoButtonItem();
            pnlTool = new UI.Forms.Containers.GoContainer();
            goTableLayoutPanel1 = new UI.Forms.Containers.GoTableLayoutPanel();
            btnValidCheck = new UI.Forms.Controls.GoButton();
            btnResourceManager = new UI.Forms.Controls.GoButton();
            btnDeploy = new UI.Forms.Controls.GoButton();
            btnSaveAs = new UI.Forms.Controls.GoButton();
            btnSave = new UI.Forms.Controls.GoButton();
            btnOpen = new UI.Forms.Controls.GoButton();
            btnNew = new UI.Forms.Controls.GoButton();
            valPath = new UI.Forms.Controls.GoValueString();
            pnlStatus = new UI.Forms.Containers.GoContainer();
            lblStatus = new UI.Forms.Controls.GoLabel();
            goControl1 = new UI.Forms.Controls.GoControl();
            btnProgramSetting = new UI.Forms.Controls.GoButton();
            dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            goMenuStrip1 = new UI.Forms.Menus.GoMenuStrip();
            tsmiFile = new ToolStripMenuItem();
            tsmiNew = new ToolStripMenuItem();
            tsmiOpen = new ToolStripMenuItem();
            tsmiSave = new ToolStripMenuItem();
            tsmiSaveAs = new ToolStripMenuItem();
            tsmiClose = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            tsmiExit = new ToolStripMenuItem();
            tsmiEdit = new ToolStripMenuItem();
            tsmiUndo = new ToolStripMenuItem();
            tsmiRedo = new ToolStripMenuItem();
            tsmiEditSep1 = new ToolStripSeparator();
            tsmiCut = new ToolStripMenuItem();
            tsmiCopy = new ToolStripMenuItem();
            tsmiPaste = new ToolStripMenuItem();
            tsmiDelete = new ToolStripMenuItem();
            tsmiEditSep2 = new ToolStripSeparator();
            tsmiSelectAll = new ToolStripMenuItem();
            tsmiView = new ToolStripMenuItem();
            tsmiExplorer = new ToolStripMenuItem();
            tsmiToolBox = new ToolStripMenuItem();
            tsmiProperties = new ToolStripMenuItem();
            tsmiProj = new ToolStripMenuItem();
            tsmiValidCheck = new ToolStripMenuItem();
            tsmiDeploy = new ToolStripMenuItem();
            tsmiProjSep1 = new ToolStripSeparator();
            tsmiProjectProps = new ToolStripMenuItem();
            tsmiTool = new ToolStripMenuItem();
            tsmiResourceManager = new ToolStripMenuItem();
            tsmiProgramSetting = new ToolStripMenuItem();
            tsmiHelp = new ToolStripMenuItem();
            tsmiProgramInfo = new ToolStripMenuItem();
            pnlTool.SuspendLayout();
            goTableLayoutPanel1.SuspendLayout();
            pnlStatus.SuspendLayout();
            goMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTool
            // 
            pnlTool.BackColor = Color.FromArgb(32, 32, 32);
            pnlTool.BackgroundColor = "window";
            pnlTool.Controls.Add(goTableLayoutPanel1);
            pnlTool.Dock = DockStyle.Top;
            pnlTool.Location = new Point(0, 24);
            pnlTool.Name = "pnlTool";
            pnlTool.Padding = new Padding(5);
            pnlTool.Size = new Size(1424, 50);
            pnlTool.TabIndex = 0;
            pnlTool.TabStop = false;
            pnlTool.Text = "goContainer1";
            // 
            // goTableLayoutPanel1
            // 
            goTableLayoutPanel1.BackColor = Color.FromArgb(32, 32, 32);
            goTableLayoutPanel1.BackgroundColor = "window";
            goTableLayoutPanel1.ColumnCount = 12;
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            goTableLayoutPanel1.Controls.Add(btnValidCheck, 10, 0);
            goTableLayoutPanel1.Controls.Add(btnResourceManager, 5, 0);
            goTableLayoutPanel1.Controls.Add(btnDeploy, 11, 0);
            goTableLayoutPanel1.Controls.Add(btnSaveAs, 3, 0);
            goTableLayoutPanel1.Controls.Add(btnSave, 2, 0);
            goTableLayoutPanel1.Controls.Add(btnOpen, 1, 0);
            goTableLayoutPanel1.Controls.Add(btnNew, 0, 0);
            goTableLayoutPanel1.Controls.Add(valPath, 9, 0);
            goTableLayoutPanel1.Dock = DockStyle.Fill;
            goTableLayoutPanel1.Location = new Point(5, 5);
            goTableLayoutPanel1.Name = "goTableLayoutPanel1";
            goTableLayoutPanel1.RowCount = 1;
            goTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.Size = new Size(1414, 40);
            goTableLayoutPanel1.TabIndex = 1;
            // 
            // btnValidCheck
            // 
            btnValidCheck.BackColor = Color.FromArgb(32, 32, 32);
            btnValidCheck.BackgroundColor = "window";
            btnValidCheck.BackgroundDraw = false;
            btnValidCheck.BorderOnly = true;
            btnValidCheck.ButtonColor = "Base3";
            btnValidCheck.Dock = DockStyle.Fill;
            btnValidCheck.FontName = "나눔고딕";
            btnValidCheck.FontSize = 12F;
            btnValidCheck.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnValidCheck.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnValidCheck.IconGap = 5F;
            btnValidCheck.IconSize = 20F;
            btnValidCheck.IconString = "fa-check";
            btnValidCheck.Location = new Point(1337, 3);
            btnValidCheck.Name = "btnValidCheck";
            btnValidCheck.Round = UI.Enums.GoRoundType.All;
            btnValidCheck.Size = new Size(34, 34);
            btnValidCheck.TabIndex = 7;
            btnValidCheck.TabStop = false;
            btnValidCheck.TextColor = "Fore";
            // 
            // btnResourceManager
            // 
            btnResourceManager.BackColor = Color.FromArgb(32, 32, 32);
            btnResourceManager.BackgroundColor = "window";
            btnResourceManager.BackgroundDraw = false;
            btnResourceManager.BorderOnly = true;
            btnResourceManager.ButtonColor = "Base3";
            btnResourceManager.Dock = DockStyle.Fill;
            btnResourceManager.FontName = "나눔고딕";
            btnResourceManager.FontSize = 12F;
            btnResourceManager.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnResourceManager.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnResourceManager.IconGap = 5F;
            btnResourceManager.IconSize = 20F;
            btnResourceManager.IconString = "fa-images";
            btnResourceManager.Location = new Point(183, 3);
            btnResourceManager.Name = "btnResourceManager";
            btnResourceManager.Round = UI.Enums.GoRoundType.All;
            btnResourceManager.Size = new Size(34, 34);
            btnResourceManager.TabIndex = 6;
            btnResourceManager.TabStop = false;
            btnResourceManager.TextColor = "Fore";
            // 
            // btnDeploy
            // 
            btnDeploy.BackColor = Color.FromArgb(32, 32, 32);
            btnDeploy.BackgroundColor = "window";
            btnDeploy.BackgroundDraw = false;
            btnDeploy.BorderOnly = true;
            btnDeploy.ButtonColor = "Base3";
            btnDeploy.Dock = DockStyle.Fill;
            btnDeploy.FontName = "나눔고딕";
            btnDeploy.FontSize = 12F;
            btnDeploy.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnDeploy.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnDeploy.IconGap = 5F;
            btnDeploy.IconSize = 20F;
            btnDeploy.IconString = "fa-download";
            btnDeploy.Location = new Point(1377, 3);
            btnDeploy.Name = "btnDeploy";
            btnDeploy.Round = UI.Enums.GoRoundType.All;
            btnDeploy.Size = new Size(34, 34);
            btnDeploy.TabIndex = 5;
            btnDeploy.TabStop = false;
            btnDeploy.TextColor = "Fore";
            // 
            // btnSaveAs
            // 
            btnSaveAs.BackColor = Color.FromArgb(32, 32, 32);
            btnSaveAs.BackgroundColor = "window";
            btnSaveAs.BackgroundDraw = false;
            btnSaveAs.BorderOnly = true;
            btnSaveAs.ButtonColor = "Base3";
            btnSaveAs.Dock = DockStyle.Fill;
            btnSaveAs.FontName = "나눔고딕";
            btnSaveAs.FontSize = 12F;
            btnSaveAs.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnSaveAs.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnSaveAs.IconGap = 5F;
            btnSaveAs.IconSize = 20F;
            btnSaveAs.IconString = "fa-floppy-disk";
            btnSaveAs.Location = new Point(123, 3);
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Round = UI.Enums.GoRoundType.All;
            btnSaveAs.Size = new Size(34, 34);
            btnSaveAs.TabIndex = 3;
            btnSaveAs.TabStop = false;
            btnSaveAs.TextColor = "Fore";
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(32, 32, 32);
            btnSave.BackgroundColor = "window";
            btnSave.BackgroundDraw = false;
            btnSave.BorderOnly = true;
            btnSave.ButtonColor = "Base3";
            btnSave.Dock = DockStyle.Fill;
            btnSave.FontName = "나눔고딕";
            btnSave.FontSize = 12F;
            btnSave.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnSave.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnSave.IconGap = 5F;
            btnSave.IconSize = 20F;
            btnSave.IconString = "fa-floppy-disk";
            btnSave.Location = new Point(83, 3);
            btnSave.Name = "btnSave";
            btnSave.Round = UI.Enums.GoRoundType.All;
            btnSave.Size = new Size(34, 34);
            btnSave.TabIndex = 2;
            btnSave.TabStop = false;
            btnSave.TextColor = "Fore";
            // 
            // btnOpen
            // 
            btnOpen.BackColor = Color.FromArgb(32, 32, 32);
            btnOpen.BackgroundColor = "window";
            btnOpen.BackgroundDraw = false;
            btnOpen.BorderOnly = true;
            btnOpen.ButtonColor = "Base3";
            btnOpen.Dock = DockStyle.Fill;
            btnOpen.FontName = "나눔고딕";
            btnOpen.FontSize = 12F;
            btnOpen.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnOpen.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnOpen.IconGap = 5F;
            btnOpen.IconSize = 20F;
            btnOpen.IconString = "fa-folder-open";
            btnOpen.Location = new Point(43, 3);
            btnOpen.Name = "btnOpen";
            btnOpen.Round = UI.Enums.GoRoundType.All;
            btnOpen.Size = new Size(34, 34);
            btnOpen.TabIndex = 1;
            btnOpen.TabStop = false;
            btnOpen.TextColor = "Fore";
            // 
            // btnNew
            // 
            btnNew.BackColor = Color.FromArgb(32, 32, 32);
            btnNew.BackgroundColor = "window";
            btnNew.BackgroundDraw = false;
            btnNew.BorderOnly = true;
            btnNew.ButtonColor = "Base3";
            btnNew.Dock = DockStyle.Fill;
            btnNew.FontName = "나눔고딕";
            btnNew.FontSize = 12F;
            btnNew.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnNew.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnNew.IconGap = 5F;
            btnNew.IconSize = 20F;
            btnNew.IconString = "fa-file";
            btnNew.Location = new Point(3, 3);
            btnNew.Name = "btnNew";
            btnNew.Round = UI.Enums.GoRoundType.All;
            btnNew.Size = new Size(34, 34);
            btnNew.TabIndex = 0;
            btnNew.TabStop = false;
            btnNew.TextColor = "Fore";
            // 
            // valPath
            // 
            valPath.BackColor = Color.FromArgb(32, 32, 32);
            valPath.BackgroundColor = "Window";
            valPath.BorderColor = "Base2";
            goButtonItem1.IconString = "fa-ellipsis";
            goButtonItem1.Name = "select";
            goButtonItem1.Size = "100%";
            goButtonItem1.Text = null;
            valPath.Buttons.Add(goButtonItem1);
            valPath.ButtonSize = 40F;
            valPath.Direction = UI.Enums.GoDirectionHV.Horizon;
            valPath.Dock = DockStyle.Fill;
            valPath.FillColor = "Base2";
            valPath.FontName = "나눔고딕";
            valPath.FontSize = 12F;
            valPath.FontStyle = UI.Enums.GoFontStyle.Normal;
            valPath.IconGap = 5F;
            valPath.IconSize = 12F;
            valPath.IconString = null;
            valPath.Location = new Point(937, 3);
            valPath.Name = "valPath";
            valPath.Round = UI.Enums.GoRoundType.All;
            valPath.Size = new Size(394, 34);
            valPath.TabIndex = 4;
            valPath.TabStop = false;
            valPath.Text = "goValueString1";
            valPath.TextColor = "Fore";
            valPath.Title = "프로젝트 경로";
            valPath.TitleSize = 100F;
            valPath.Value = null;
            valPath.ValueColor = "Base1";
            // 
            // pnlStatus
            // 
            pnlStatus.BackColor = Color.FromArgb(32, 32, 32);
            pnlStatus.BackgroundColor = "window";
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Controls.Add(goControl1);
            pnlStatus.Controls.Add(btnProgramSetting);
            pnlStatus.Dock = DockStyle.Bottom;
            pnlStatus.Location = new Point(0, 711);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Padding = new Padding(8);
            pnlStatus.Size = new Size(1424, 50);
            pnlStatus.TabIndex = 1;
            pnlStatus.TabStop = false;
            pnlStatus.Text = "goContainer2";
            // 
            // lblStatus
            // 
            lblStatus.BackColor = Color.FromArgb(32, 32, 32);
            lblStatus.BackgroundColor = "window";
            lblStatus.BackgroundDraw = true;
            lblStatus.BorderOnly = true;
            lblStatus.ContentAlignment = UI.Enums.GoContentAlignment.MiddleCenter;
            lblStatus.Dock = DockStyle.Left;
            lblStatus.FontName = "나눔고딕";
            lblStatus.FontSize = 12F;
            lblStatus.FontStyle = UI.Enums.GoFontStyle.Normal;
            lblStatus.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            lblStatus.IconGap = 5F;
            lblStatus.IconSize = 12F;
            lblStatus.IconString = null;
            lblStatus.LabelColor = "Base2";
            lblStatus.Location = new Point(52, 8);
            lblStatus.Name = "lblStatus";
            lblStatus.Round = UI.Enums.GoRoundType.All;
            lblStatus.Size = new Size(380, 34);
            lblStatus.TabIndex = 4;
            lblStatus.TabStop = false;
            lblStatus.TextColor = "Fore";
            lblStatus.TextPadding.Bottom = 0F;
            lblStatus.TextPadding.Left = 0F;
            lblStatus.TextPadding.Right = 0F;
            lblStatus.TextPadding.Top = 0F;
            // 
            // goControl1
            // 
            goControl1.BackColor = Color.FromArgb(32, 32, 32);
            goControl1.BackgroundColor = "window";
            goControl1.Dock = DockStyle.Left;
            goControl1.Location = new Point(42, 8);
            goControl1.Name = "goControl1";
            goControl1.Size = new Size(10, 34);
            goControl1.TabIndex = 3;
            goControl1.TabStop = false;
            goControl1.Text = "goControl1";
            // 
            // btnProgramSetting
            // 
            btnProgramSetting.BackColor = Color.FromArgb(32, 32, 32);
            btnProgramSetting.BackgroundColor = "window";
            btnProgramSetting.BackgroundDraw = false;
            btnProgramSetting.BorderOnly = true;
            btnProgramSetting.ButtonColor = "Base3";
            btnProgramSetting.Dock = DockStyle.Left;
            btnProgramSetting.FontName = "나눔고딕";
            btnProgramSetting.FontSize = 12F;
            btnProgramSetting.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnProgramSetting.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnProgramSetting.IconGap = 5F;
            btnProgramSetting.IconSize = 20F;
            btnProgramSetting.IconString = "fa-gears";
            btnProgramSetting.Location = new Point(8, 8);
            btnProgramSetting.Name = "btnProgramSetting";
            btnProgramSetting.Round = UI.Enums.GoRoundType.All;
            btnProgramSetting.Size = new Size(34, 34);
            btnProgramSetting.TabIndex = 1;
            btnProgramSetting.TabStop = false;
            btnProgramSetting.TextColor = "Fore";
            // 
            // dockPanel
            // 
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.DockBackColor = Color.FromArgb(45, 45, 48);
            dockPanel.DockBottomPortion = 0.2D;
            dockPanel.DockLeftPortion = 0.2D;
            dockPanel.DockRightPortion = 0.2D;
            dockPanel.DockTopPortion = 0.2D;
            dockPanel.Location = new Point(0, 74);
            dockPanel.Name = "dockPanel";
            dockPanel.ShowAutoHideContentOnHover = false;
            dockPanel.Size = new Size(1424, 637);
            dockPanel.TabIndex = 2;
            // 
            // goMenuStrip1
            // 
            goMenuStrip1.BackColor = Color.FromArgb(32, 32, 32);
            goMenuStrip1.ForeColor = Color.FromArgb(150, 150, 150);
            goMenuStrip1.Items.AddRange(new ToolStripItem[] { tsmiFile, tsmiEdit, tsmiView, tsmiProj, tsmiTool, tsmiHelp });
            goMenuStrip1.Location = new Point(0, 0);
            goMenuStrip1.Name = "goMenuStrip1";
            goMenuStrip1.Size = new Size(1424, 24);
            goMenuStrip1.TabIndex = 3;
            goMenuStrip1.Text = "goMenuStrip1";
            // 
            // tsmiFile
            // 
            tsmiFile.DropDownItems.AddRange(new ToolStripItem[] { tsmiNew, tsmiOpen, tsmiSave, tsmiSaveAs, tsmiClose, toolStripSeparator2, tsmiExit });
            tsmiFile.Name = "tsmiFile";
            tsmiFile.Size = new Size(57, 20);
            tsmiFile.Text = "파일(&F)";
            // 
            // tsmiNew
            // 
            tsmiNew.Font = new Font("나눔고딕", 9F);
            tsmiNew.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiNew.Name = "tsmiNew";
            tsmiNew.Size = new Size(185, 22);
            tsmiNew.Text = "새 프로젝트(&N)";
            // 
            // tsmiOpen
            // 
            tsmiOpen.Font = new Font("나눔고딕", 9F);
            tsmiOpen.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiOpen.Name = "tsmiOpen";
            tsmiOpen.Size = new Size(185, 22);
            tsmiOpen.Text = "열기 (&O)";
            // 
            // tsmiSave
            // 
            tsmiSave.Font = new Font("나눔고딕", 9F);
            tsmiSave.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiSave.Name = "tsmiSave";
            tsmiSave.Size = new Size(185, 22);
            tsmiSave.Text = "저장(&S)";
            // 
            // tsmiSaveAs
            // 
            tsmiSaveAs.Font = new Font("나눔고딕", 9F);
            tsmiSaveAs.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiSaveAs.Name = "tsmiSaveAs";
            tsmiSaveAs.Size = new Size(185, 22);
            tsmiSaveAs.Text = "다른 이름으로 저장(&A)";
            // 
            // tsmiClose
            // 
            tsmiClose.Font = new Font("나눔고딕", 9F);
            tsmiClose.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiClose.Name = "tsmiClose";
            tsmiClose.Size = new Size(185, 22);
            tsmiClose.Text = "닫기(&C)";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(182, 6);
            // 
            // tsmiExit
            // 
            tsmiExit.Font = new Font("나눔고딕", 9F);
            tsmiExit.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiExit.Name = "tsmiExit";
            tsmiExit.Size = new Size(185, 22);
            tsmiExit.Text = "종료(&X)";
            // 
            // tsmiEdit
            // 
            tsmiEdit.DropDownItems.AddRange(new ToolStripItem[] { tsmiUndo, tsmiRedo, tsmiEditSep1, tsmiCut, tsmiCopy, tsmiPaste, tsmiDelete, tsmiEditSep2, tsmiSelectAll });
            tsmiEdit.Name = "tsmiEdit";
            tsmiEdit.Size = new Size(57, 20);
            tsmiEdit.Text = "편집(&E)";
            // 
            // tsmiUndo
            // 
            tsmiUndo.Font = new Font("나눔고딕", 9F);
            tsmiUndo.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiUndo.Name = "tsmiUndo";
            tsmiUndo.Size = new Size(138, 22);
            tsmiUndo.Text = "실행 취소(&U)";
            // 
            // tsmiRedo
            // 
            tsmiRedo.Font = new Font("나눔고딕", 9F);
            tsmiRedo.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiRedo.Name = "tsmiRedo";
            tsmiRedo.Size = new Size(138, 22);
            tsmiRedo.Text = "다시 실행(&R)";
            // 
            // tsmiEditSep1
            // 
            tsmiEditSep1.Name = "tsmiEditSep1";
            tsmiEditSep1.Size = new Size(135, 6);
            // 
            // tsmiCut
            // 
            tsmiCut.Font = new Font("나눔고딕", 9F);
            tsmiCut.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiCut.Name = "tsmiCut";
            tsmiCut.Size = new Size(138, 22);
            tsmiCut.Text = "잘라내기(&T)";
            // 
            // tsmiCopy
            // 
            tsmiCopy.Font = new Font("나눔고딕", 9F);
            tsmiCopy.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiCopy.Name = "tsmiCopy";
            tsmiCopy.Size = new Size(138, 22);
            tsmiCopy.Text = "복사(&C)";
            // 
            // tsmiPaste
            // 
            tsmiPaste.Font = new Font("나눔고딕", 9F);
            tsmiPaste.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiPaste.Name = "tsmiPaste";
            tsmiPaste.Size = new Size(138, 22);
            tsmiPaste.Text = "붙여넣기(&P)";
            // 
            // tsmiDelete
            // 
            tsmiDelete.Font = new Font("나눔고딕", 9F);
            tsmiDelete.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiDelete.Name = "tsmiDelete";
            tsmiDelete.Size = new Size(138, 22);
            tsmiDelete.Text = "삭제(&D)";
            // 
            // tsmiEditSep2
            // 
            tsmiEditSep2.Name = "tsmiEditSep2";
            tsmiEditSep2.Size = new Size(135, 6);
            // 
            // tsmiSelectAll
            // 
            tsmiSelectAll.Font = new Font("나눔고딕", 9F);
            tsmiSelectAll.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiSelectAll.Name = "tsmiSelectAll";
            tsmiSelectAll.Size = new Size(138, 22);
            tsmiSelectAll.Text = "모두 선택(&A)";
            // 
            // tsmiView
            // 
            tsmiView.DropDownItems.AddRange(new ToolStripItem[] { tsmiExplorer, tsmiToolBox, tsmiProperties });
            tsmiView.Name = "tsmiView";
            tsmiView.Size = new Size(59, 20);
            tsmiView.Text = "보기(&V)";
            // 
            // tsmiExplorer
            // 
            tsmiExplorer.Font = new Font("나눔고딕", 9F);
            tsmiExplorer.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiExplorer.Name = "tsmiExplorer";
            tsmiExplorer.Size = new Size(136, 22);
            tsmiExplorer.Text = "탐색기(&E)";
            // 
            // tsmiToolBox
            // 
            tsmiToolBox.Font = new Font("나눔고딕", 9F);
            tsmiToolBox.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiToolBox.Name = "tsmiToolBox";
            tsmiToolBox.Size = new Size(136, 22);
            tsmiToolBox.Text = "도구 상자(&T)";
            // 
            // tsmiProperties
            // 
            tsmiProperties.Font = new Font("나눔고딕", 9F);
            tsmiProperties.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiProperties.Name = "tsmiProperties";
            tsmiProperties.Size = new Size(136, 22);
            tsmiProperties.Text = "속성 창(&P)";
            // 
            // tsmiProj
            // 
            tsmiProj.DropDownItems.AddRange(new ToolStripItem[] { tsmiValidCheck, tsmiDeploy, tsmiProjSep1, tsmiProjectProps });
            tsmiProj.Name = "tsmiProj";
            tsmiProj.Size = new Size(82, 20);
            tsmiProj.Text = "프로젝트(&P)";
            // 
            // tsmiValidCheck
            // 
            tsmiValidCheck.Font = new Font("나눔고딕", 9F);
            tsmiValidCheck.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiValidCheck.Name = "tsmiValidCheck";
            tsmiValidCheck.Size = new Size(180, 22);
            tsmiValidCheck.Text = "유효성 체크(&C)";
            // 
            // tsmiDeploy
            // 
            tsmiDeploy.Font = new Font("나눔고딕", 9F);
            tsmiDeploy.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiDeploy.Name = "tsmiDeploy";
            tsmiDeploy.Size = new Size(180, 22);
            tsmiDeploy.Text = "배포(&D)";
            // 
            // tsmiProjSep1
            // 
            tsmiProjSep1.Name = "tsmiProjSep1";
            tsmiProjSep1.Size = new Size(177, 6);
            tsmiProjSep1.Visible = false;
            // 
            // tsmiProjectProps
            // 
            tsmiProjectProps.Font = new Font("나눔고딕", 9F);
            tsmiProjectProps.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiProjectProps.Name = "tsmiProjectProps";
            tsmiProjectProps.Size = new Size(180, 22);
            tsmiProjectProps.Text = "프로젝트 속성(&P)";
            tsmiProjectProps.Visible = false;
            // 
            // tsmiTool
            // 
            tsmiTool.DropDownItems.AddRange(new ToolStripItem[] { tsmiResourceManager, tsmiProgramSetting });
            tsmiTool.Name = "tsmiTool";
            tsmiTool.Size = new Size(57, 20);
            tsmiTool.Text = "도구(&T)";
            // 
            // tsmiResourceManager
            // 
            tsmiResourceManager.Font = new Font("나눔고딕", 9F);
            tsmiResourceManager.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiResourceManager.Name = "tsmiResourceManager";
            tsmiResourceManager.Size = new Size(180, 22);
            tsmiResourceManager.Text = "리소스 관리자(&R)";
            // 
            // tsmiProgramSetting
            // 
            tsmiProgramSetting.Font = new Font("나눔고딕", 9F);
            tsmiProgramSetting.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiProgramSetting.Name = "tsmiProgramSetting";
            tsmiProgramSetting.Size = new Size(180, 22);
            tsmiProgramSetting.Text = "프로그램 설정(&S)";
            // 
            // tsmiHelp
            // 
            tsmiHelp.DropDownItems.AddRange(new ToolStripItem[] { tsmiProgramInfo });
            tsmiHelp.Name = "tsmiHelp";
            tsmiHelp.Size = new Size(72, 20);
            tsmiHelp.Text = "도움말(&H)";
            // 
            // tsmiProgramInfo
            // 
            tsmiProgramInfo.Font = new Font("나눔고딕", 9F);
            tsmiProgramInfo.ForeColor = Color.FromArgb(150, 150, 150);
            tsmiProgramInfo.Name = "tsmiProgramInfo";
            tsmiProgramInfo.Size = new Size(154, 22);
            tsmiProgramInfo.Text = "프로그램 정보(&I)";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1424, 761);
            Controls.Add(dockPanel);
            Controls.Add(pnlStatus);
            Controls.Add(pnlTool);
            Controls.Add(goMenuStrip1);
            MinimumSize = new Size(1440, 800);
            Name = "FormMain";
            Text = "Going.UIEditor";
            Title = "Going.UIEditor";
            TitleIconString = "fa-angles-right";
            pnlTool.ResumeLayout(false);
            goTableLayoutPanel1.ResumeLayout(false);
            pnlStatus.ResumeLayout(false);
            goMenuStrip1.ResumeLayout(false);
            goMenuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private UI.Forms.Containers.GoContainer pnlTool;
        private UI.Forms.Containers.GoContainer pnlStatus;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
        private UI.Forms.Menus.GoMenuStrip goMenuStrip1;
        private ToolStripMenuItem tsmiFile;
        private ToolStripMenuItem tsmiNew;
        private ToolStripMenuItem tsmiOpen;
        private ToolStripMenuItem tsmiSave;
        private ToolStripMenuItem tsmiSaveAs;
        private ToolStripMenuItem tsmiExit;
        private ToolStripMenuItem tsmiEdit;
        private ToolStripMenuItem tsmiView;
        private ToolStripMenuItem tsmiProj;
        private ToolStripMenuItem tsmiUndo;
        private ToolStripMenuItem tsmiRedo;
        private ToolStripSeparator tsmiEditSep1;
        private ToolStripMenuItem tsmiCut;
        private ToolStripMenuItem tsmiCopy;
        private ToolStripMenuItem tsmiPaste;
        private ToolStripMenuItem tsmiDelete;
        private ToolStripSeparator tsmiEditSep2;
        private ToolStripMenuItem tsmiSelectAll;
        private ToolStripMenuItem tsmiTool;
        private ToolStripMenuItem tsmiHelp;
        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel1;
        private UI.Forms.Controls.GoButton btnSaveAs;
        private UI.Forms.Controls.GoButton btnSave;
        private UI.Forms.Controls.GoButton btnOpen;
        private UI.Forms.Controls.GoButton btnNew;
        private UI.Forms.Controls.GoValueString valPath;
        private UI.Forms.Controls.GoButton btnDeploy;
        private UI.Forms.Controls.GoLabel lblStatus;
        private UI.Forms.Controls.GoControl goControl1;
        private UI.Forms.Controls.GoButton btnProgramSetting;
        private ToolStripMenuItem tsmiExplorer;
        private ToolStripMenuItem tsmiToolBox;
        private ToolStripMenuItem tsmiProperties;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem tsmiValidCheck;
        private ToolStripMenuItem tsmiDeploy;
        private ToolStripSeparator tsmiProjSep1;
        private ToolStripMenuItem tsmiProjectProps;
        private ToolStripMenuItem tsmiProgramSetting;
        private ToolStripMenuItem tsmiProgramInfo;
        private ToolStripMenuItem tsmiResourceManager;
        private ToolStripMenuItem tsmiClose;
        private ToolStripSeparator toolStripSeparator2;
        private UI.Forms.Controls.GoButton btnResourceManager;
        private UI.Forms.Controls.GoButton btnValidCheck;
    }
}
