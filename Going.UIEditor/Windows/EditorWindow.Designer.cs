namespace Going.UIEditor.Windows
{
    partial class EditorWindow
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
            cms = new Going.UI.Forms.Menus.GoContextMenuStrip();
            tsmiSelect = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            tsmiBringToFront = new ToolStripMenuItem();
            tsmiSendToBack = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            tsmiCut = new ToolStripMenuItem();
            tsmiCopy = new ToolStripMenuItem();
            tsmiPaste = new ToolStripMenuItem();
            tsmiDelete = new ToolStripMenuItem();
            cms.SuspendLayout();
            SuspendLayout();
            // 
            // cms
            // 
            cms.BackColor = Color.FromArgb(32, 32, 32);
            cms.ForeColor = Color.FromArgb(150, 150, 150);
            cms.Items.AddRange(new ToolStripItem[] { tsmiSelect, toolStripMenuItem2, tsmiBringToFront, tsmiSendToBack, toolStripMenuItem1, tsmiCut, tsmiCopy, tsmiPaste, tsmiDelete });
            cms.Name = "cms";
            cms.Size = new Size(181, 192);
            // 
            // tsmiSelect
            // 
            tsmiSelect.Font = new Font("나눔고딕", 9F);
            tsmiSelect.Name = "tsmiSelect";
            tsmiSelect.Size = new Size(180, 22);
            tsmiSelect.Text = "선택(&S)";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(177, 6);
            // 
            // tsmiBringToFront
            // 
            tsmiBringToFront.Font = new Font("나눔고딕", 9F);
            tsmiBringToFront.Name = "tsmiBringToFront";
            tsmiBringToFront.Size = new Size(180, 22);
            tsmiBringToFront.Text = "맨 앞으로(&F)";
            // 
            // tsmiSendToBack
            // 
            tsmiSendToBack.Font = new Font("나눔고딕", 9F);
            tsmiSendToBack.Name = "tsmiSendToBack";
            tsmiSendToBack.Size = new Size(180, 22);
            tsmiSendToBack.Text = "맨 뒤로(&B)";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(177, 6);
            // 
            // tsmiCut
            // 
            tsmiCut.Font = new Font("나눔고딕", 9F);
            tsmiCut.Name = "tsmiCut";
            tsmiCut.Size = new Size(180, 22);
            tsmiCut.Text = "잘라내기(&T)";
            // 
            // tsmiCopy
            // 
            tsmiCopy.Font = new Font("나눔고딕", 9F);
            tsmiCopy.Name = "tsmiCopy";
            tsmiCopy.Size = new Size(180, 22);
            tsmiCopy.Text = "복사(&C)";
            // 
            // tsmiPaste
            // 
            tsmiPaste.Font = new Font("나눔고딕", 9F);
            tsmiPaste.Name = "tsmiPaste";
            tsmiPaste.Size = new Size(180, 22);
            tsmiPaste.Text = "붙여넣기(&P)";
            // 
            // tsmiDelete
            // 
            tsmiDelete.Font = new Font("나눔고딕", 9F);
            tsmiDelete.Name = "tsmiDelete";
            tsmiDelete.Size = new Size(180, 22);
            tsmiDelete.Text = "삭제(&D)";
            // 
            // EditorWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "EditorWindow";
            Text = "MasterEditWindow";
            Title = "MasterEditWindow";
            cms.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Menus.GoContextMenuStrip cms;
        private ToolStripMenuItem tsmiSelect;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem tsmiCut;
        private ToolStripMenuItem tsmiCopy;
        private ToolStripMenuItem tsmiPaste;
        private ToolStripMenuItem tsmiDelete;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem tsmiBringToFront;
        private ToolStripMenuItem tsmiSendToBack;
    }
}