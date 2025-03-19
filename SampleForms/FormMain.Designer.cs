namespace SampleForms
{
    partial class FormMain
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
            rm = new Going.UI.Forms.Components.GoResourceManager();
            dg = new Going.UI.Forms.Controls.GoDataGrid();
            goContainer1 = new Going.UI.Forms.Containers.GoContainer();
            btnMB = new Going.UI.Forms.Controls.GoButton();
            btnSB = new Going.UI.Forms.Controls.GoButton();
            btnIB = new Going.UI.Forms.Controls.GoButton();
            goContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // rm
            // 
            rm.ImageFolder = "D:\\Project\\Going\\library\\src\\Going\\ImageSample";
            // 
            // dg
            // 
            dg.BackColor = Color.FromArgb(50, 50, 50);
            dg.BackgroundColor = "Back";
            dg.ColumnColor = "Base1";
            dg.ColumnHeight = 30F;
            dg.Dock = DockStyle.Fill;
            dg.FontName = "나눔고딕";
            dg.FontSize = 12F;
            dg.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            dg.Location = new Point(10, 10);
            dg.Name = "dg";
            dg.RowColor = "Base2";
            dg.RowHeight = 30F;
            dg.ScrollMode = Going.UI.Utils.ScrollMode.Vertical;
            dg.SelectedRowColor = "Select";
            dg.SelectionMode = Going.UI.Enums.GoDataGridSelectionMode.Single;
            dg.Size = new Size(973, 595);
            dg.SummaryRowColor = "Base1";
            dg.TabIndex = 0;
            dg.TabStop = false;
            dg.Text = "goDataGrid1";
            dg.TextColor = "Fore";
            // 
            // goContainer1
            // 
            goContainer1.BackColor = Color.FromArgb(50, 50, 50);
            goContainer1.BackgroundColor = "Back";
            goContainer1.Controls.Add(btnIB);
            goContainer1.Controls.Add(btnSB);
            goContainer1.Controls.Add(btnMB);
            goContainer1.Dock = DockStyle.Bottom;
            goContainer1.Location = new Point(10, 605);
            goContainer1.Name = "goContainer1";
            goContainer1.Padding = new Padding(0, 10, 0, 0);
            goContainer1.Size = new Size(973, 50);
            goContainer1.TabIndex = 1;
            goContainer1.TabStop = false;
            goContainer1.Text = "goContainer1";
            // 
            // btnMB
            // 
            btnMB.BackColor = Color.FromArgb(50, 50, 50);
            btnMB.BackgroundColor = "Back";
            btnMB.BackgroundDraw = true;
            btnMB.BorderOnly = false;
            btnMB.ButtonColor = "Base3";
            btnMB.FontName = "나눔고딕";
            btnMB.FontSize = 12F;
            btnMB.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnMB.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnMB.IconGap = 5F;
            btnMB.IconSize = 12F;
            btnMB.IconString = null;
            btnMB.Location = new Point(0, 13);
            btnMB.Name = "btnMB";
            btnMB.Round = Going.UI.Enums.GoRoundType.All;
            btnMB.Size = new Size(118, 37);
            btnMB.TabIndex = 0;
            btnMB.TabStop = false;
            btnMB.Text = "MessageBox";
            btnMB.TextColor = "Fore";
            // 
            // btnSB
            // 
            btnSB.BackColor = Color.FromArgb(50, 50, 50);
            btnSB.BackgroundColor = "Back";
            btnSB.BackgroundDraw = true;
            btnSB.BorderOnly = false;
            btnSB.ButtonColor = "Base3";
            btnSB.FontName = "나눔고딕";
            btnSB.FontSize = 12F;
            btnSB.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnSB.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnSB.IconGap = 5F;
            btnSB.IconSize = 12F;
            btnSB.IconString = null;
            btnSB.Location = new Point(124, 13);
            btnSB.Name = "btnSB";
            btnSB.Round = Going.UI.Enums.GoRoundType.All;
            btnSB.Size = new Size(118, 37);
            btnSB.TabIndex = 1;
            btnSB.TabStop = false;
            btnSB.Text = "SelectorBox";
            btnSB.TextColor = "Fore";
            // 
            // btnIB
            // 
            btnIB.BackColor = Color.FromArgb(50, 50, 50);
            btnIB.BackgroundColor = "Back";
            btnIB.BackgroundDraw = true;
            btnIB.BorderOnly = false;
            btnIB.ButtonColor = "Base3";
            btnIB.FontName = "나눔고딕";
            btnIB.FontSize = 12F;
            btnIB.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnIB.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnIB.IconGap = 5F;
            btnIB.IconSize = 12F;
            btnIB.IconString = null;
            btnIB.Location = new Point(248, 13);
            btnIB.Name = "btnIB";
            btnIB.Round = Going.UI.Enums.GoRoundType.All;
            btnIB.Size = new Size(118, 37);
            btnIB.TabIndex = 2;
            btnIB.TabStop = false;
            btnIB.Text = "InputBox";
            btnIB.TextColor = "Fore";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(993, 665);
            Controls.Add(dg);
            Controls.Add(goContainer1);
            Name = "FormMain";
            Padding = new Padding(10);
            Text = "FormMain2";
            Title = "FormMain2";
            goContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Going.UI.Forms.Components.GoResourceManager rm;
        private Going.UI.Forms.Controls.GoDataGrid dg;
        private Going.UI.Forms.Containers.GoContainer goContainer1;
        private Going.UI.Forms.Controls.GoButton btnMB;
        private Going.UI.Forms.Controls.GoButton btnSB;
        private Going.UI.Forms.Controls.GoButton btnIB;
    }
}