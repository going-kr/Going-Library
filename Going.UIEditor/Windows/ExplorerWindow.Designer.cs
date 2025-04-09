namespace Going.UIEditor.Windows
{
    partial class ExplorerWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            UI.Collections.ObservableList<UI.Datas.GoTreeNode> observableList_11 = new UI.Collections.ObservableList<UI.Datas.GoTreeNode>();
            tvExplorer = new UI.Forms.Controls.GoTreeView();
            cmsMenu = new UI.Forms.Menus.GoContextMenuStrip();
            tsmiExplorerAdd = new ToolStripMenuItem();
            tsmiExplorerDelete = new ToolStripMenuItem();
            tsmiSep = new ToolStripSeparator();
            tsmiExplorerRename = new ToolStripMenuItem();
            cmsMenu.SuspendLayout();
            SuspendLayout();
            // 
            // tvExplorer
            // 
            tvExplorer.BackColor = Color.FromArgb(50, 50, 50);
            tvExplorer.BackgroundColor = "Back";
            tvExplorer.BackgroundDraw = false;
            tvExplorer.BorderColor = "Base3";
            tvExplorer.BoxColor = "Base1";
            tvExplorer.ContextMenuStrip = cmsMenu;
            tvExplorer.Dock = DockStyle.Fill;
            tvExplorer.DragMode = false;
            tvExplorer.FontName = "나눔고딕";
            tvExplorer.FontSize = 12F;
            tvExplorer.FontStyle = UI.Enums.GoFontStyle.Normal;
            tvExplorer.IconGap = 5F;
            tvExplorer.IconSize = 12F;
            tvExplorer.ItemHeight = 30F;
            tvExplorer.Location = new Point(5, 5);
            tvExplorer.Name = "tvExplorer";
            observableList_11.Changed = false;
            tvExplorer.Nodes = observableList_11;
            tvExplorer.Round = UI.Enums.GoRoundType.All;
            tvExplorer.SelectColor = "Select";
            tvExplorer.SelectionMode = UI.Enums.GoItemSelectionMode.Single;
            tvExplorer.Size = new Size(790, 440);
            tvExplorer.TabIndex = 0;
            tvExplorer.TabStop = false;
            tvExplorer.Text = "goTreeView1";
            tvExplorer.TextColor = "Fore";
            // 
            // cmsMenu
            // 
            cmsMenu.BackColor = Color.FromArgb(32, 32, 32);
            cmsMenu.ForeColor = Color.FromArgb(150, 150, 150);
            cmsMenu.Items.AddRange(new ToolStripItem[] { tsmiExplorerAdd, tsmiExplorerDelete, tsmiSep, tsmiExplorerRename });
            cmsMenu.Name = "cmsMenu";
            cmsMenu.Size = new Size(133, 76);
            // 
            // tsmiExplorerAdd
            // 
            tsmiExplorerAdd.Name = "tsmiExplorerAdd";
            tsmiExplorerAdd.Size = new Size(132, 22);
            tsmiExplorerAdd.Text = "Add(&A)";
            // 
            // tsmiExplorerDelete
            // 
            tsmiExplorerDelete.Name = "tsmiExplorerDelete";
            tsmiExplorerDelete.Size = new Size(132, 22);
            tsmiExplorerDelete.Text = "Delete(&D)";
            // 
            // tsmiSep
            // 
            tsmiSep.Name = "tsmiSep";
            tsmiSep.Size = new Size(129, 6);
            // 
            // tsmiExplorerRename
            // 
            tsmiExplorerRename.Name = "tsmiExplorerRename";
            tsmiExplorerRename.Size = new Size(132, 22);
            tsmiExplorerRename.Text = "Rename(&R)";
            // 
            // ExplorerWindow
            // 
            ClientSize = new Size(800, 450);
            Controls.Add(tvExplorer);
            Name = "ExplorerWindow";
            Padding = new Padding(5);
            Text = "ExplorerWindow";
            Title = "ExplorerWindow";
            cmsMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Controls.GoTreeView tvExplorer;
        private UI.Forms.Menus.GoContextMenuStrip cmsMenu;
        private ToolStripMenuItem tsmiExplorerAdd;
        private ToolStripMenuItem tsmiExplorerDelete;
        private ToolStripSeparator tsmiSep;
        private ToolStripMenuItem tsmiExplorerRename;
    }
}