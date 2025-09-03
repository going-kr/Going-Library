using System.Drawing;

namespace Going.UIEditor.Forms
{
    partial class FormResourceManager
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
            tpnl1 = new Going.UI.Forms.Containers.GoTableLayoutPanel();
            btnClose = new Going.UI.Forms.Controls.GoButton();
            tpnl2 = new Going.UI.Forms.Containers.GoTableLayoutPanel();
            goContainer2 = new Going.UI.Forms.Containers.GoContainer();
            btnFontAdd = new Going.UI.Forms.Controls.GoButton();
            btnFontDel = new Going.UI.Forms.Controls.GoButton();
            lblFontTitle = new Going.UI.Forms.Controls.GoLabel();
            goContainer1 = new Going.UI.Forms.Containers.GoContainer();
            btnImageAdd = new Going.UI.Forms.Controls.GoButton();
            btnImageDel = new Going.UI.Forms.Controls.GoButton();
            lblImageTitle = new Going.UI.Forms.Controls.GoLabel();
            grid = new Going.UIEditor.Controls.GoContentGrid();
            dgFont = new Going.UI.Forms.Controls.GoDataGrid();
            tpnl1.SuspendLayout();
            tpnl2.SuspendLayout();
            goContainer2.SuspendLayout();
            goContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // tpnl1
            // 
            tpnl1.BackColor = Color.FromArgb(50, 50, 50);
            tpnl1.BackgroundColor = "Back";
            tpnl1.ColumnCount = 2;
            tpnl1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tpnl1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tpnl1.Controls.Add(btnClose, 1, 0);
            tpnl1.Dock = DockStyle.Bottom;
            tpnl1.Location = new Point(5, 516);
            tpnl1.Name = "tpnl1";
            tpnl1.RowCount = 1;
            tpnl1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tpnl1.Size = new Size(998, 40);
            tpnl1.TabIndex = 3;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(50, 50, 50);
            btnClose.BackgroundColor = "Back";
            btnClose.BackgroundDraw = true;
            btnClose.BorderOnly = false;
            btnClose.ButtonColor = "Base3";
            btnClose.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            btnClose.Dock = DockStyle.Fill;
            btnClose.FontName = "나눔고딕";
            btnClose.FontSize = 12F;
            btnClose.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnClose.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnClose.IconGap = 5F;
            btnClose.IconSize = 12F;
            btnClose.IconString = null;
            btnClose.Location = new Point(903, 5);
            btnClose.Margin = new Padding(5);
            btnClose.Name = "btnClose";
            btnClose.Round = Going.UI.Enums.GoRoundType.All;
            btnClose.Size = new Size(90, 30);
            btnClose.TabIndex = 1;
            btnClose.TabStop = false;
            btnClose.Text = "닫기";
            btnClose.TextColor = "Fore";
            // 
            // tpnl2
            // 
            tpnl2.BackColor = Color.FromArgb(50, 50, 50);
            tpnl2.BackgroundColor = "Back";
            tpnl2.ColumnCount = 3;
            tpnl2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tpnl2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tpnl2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tpnl2.Controls.Add(goContainer2, 2, 0);
            tpnl2.Controls.Add(goContainer1, 0, 0);
            tpnl2.Controls.Add(grid, 0, 1);
            tpnl2.Controls.Add(dgFont, 2, 1);
            tpnl2.Dock = DockStyle.Fill;
            tpnl2.Location = new Point(5, 5);
            tpnl2.Name = "tpnl2";
            tpnl2.RowCount = 2;
            tpnl2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tpnl2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tpnl2.Size = new Size(998, 511);
            tpnl2.TabIndex = 4;
            // 
            // goContainer2
            // 
            goContainer2.BackColor = Color.FromArgb(50, 50, 50);
            goContainer2.BackgroundColor = "Back";
            goContainer2.Controls.Add(btnFontAdd);
            goContainer2.Controls.Add(btnFontDel);
            goContainer2.Controls.Add(lblFontTitle);
            goContainer2.Dock = DockStyle.Fill;
            goContainer2.Location = new Point(514, 5);
            goContainer2.Margin = new Padding(5);
            goContainer2.Name = "goContainer2";
            goContainer2.Size = new Size(479, 20);
            goContainer2.TabIndex = 2;
            goContainer2.TabStop = false;
            goContainer2.Text = "goContainer2";
            // 
            // btnFontAdd
            // 
            btnFontAdd.BackColor = Color.FromArgb(50, 50, 50);
            btnFontAdd.BackgroundColor = "Back";
            btnFontAdd.BackgroundDraw = false;
            btnFontAdd.BorderOnly = false;
            btnFontAdd.ButtonColor = "Base3";
            btnFontAdd.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            btnFontAdd.Dock = DockStyle.Right;
            btnFontAdd.FontName = "나눔고딕";
            btnFontAdd.FontSize = 12F;
            btnFontAdd.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnFontAdd.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnFontAdd.IconGap = 5F;
            btnFontAdd.IconSize = 12F;
            btnFontAdd.IconString = "fa-plus";
            btnFontAdd.Location = new Point(419, 0);
            btnFontAdd.Name = "btnFontAdd";
            btnFontAdd.Round = Going.UI.Enums.GoRoundType.All;
            btnFontAdd.Size = new Size(30, 20);
            btnFontAdd.TabIndex = 2;
            btnFontAdd.TabStop = false;
            btnFontAdd.TextColor = "Fore";
            // 
            // btnFontDel
            // 
            btnFontDel.BackColor = Color.FromArgb(50, 50, 50);
            btnFontDel.BackgroundColor = "Back";
            btnFontDel.BackgroundDraw = false;
            btnFontDel.BorderOnly = false;
            btnFontDel.ButtonColor = "Base3";
            btnFontDel.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            btnFontDel.Dock = DockStyle.Right;
            btnFontDel.FontName = "나눔고딕";
            btnFontDel.FontSize = 12F;
            btnFontDel.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnFontDel.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnFontDel.IconGap = 5F;
            btnFontDel.IconSize = 12F;
            btnFontDel.IconString = "fa-minus";
            btnFontDel.Location = new Point(449, 0);
            btnFontDel.Name = "btnFontDel";
            btnFontDel.Round = Going.UI.Enums.GoRoundType.All;
            btnFontDel.Size = new Size(30, 20);
            btnFontDel.TabIndex = 1;
            btnFontDel.TabStop = false;
            btnFontDel.TextColor = "Fore";
            // 
            // lblFontTitle
            // 
            lblFontTitle.BackColor = Color.FromArgb(50, 50, 50);
            lblFontTitle.BackgroundColor = "Back";
            lblFontTitle.BackgroundDraw = false;
            lblFontTitle.BorderOnly = false;
            lblFontTitle.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleLeft;
            lblFontTitle.Dock = DockStyle.Left;
            lblFontTitle.FontName = "나눔고딕";
            lblFontTitle.FontSize = 12F;
            lblFontTitle.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            lblFontTitle.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            lblFontTitle.IconGap = 5F;
            lblFontTitle.IconSize = 12F;
            lblFontTitle.IconString = "fa-list";
            lblFontTitle.LabelColor = "Base2";
            lblFontTitle.Location = new Point(0, 0);
            lblFontTitle.Name = "lblFontTitle";
            lblFontTitle.Round = Going.UI.Enums.GoRoundType.All;
            lblFontTitle.Size = new Size(113, 20);
            lblFontTitle.TabIndex = 0;
            lblFontTitle.TabStop = false;
            lblFontTitle.Text = "폰트 목록";
            lblFontTitle.TextColor = "Fore";
            lblFontTitle.TextPadding.Bottom = 0F;
            lblFontTitle.TextPadding.Left = 5F;
            lblFontTitle.TextPadding.Right = 0F;
            lblFontTitle.TextPadding.Top = 0F;
            // 
            // goContainer1
            // 
            goContainer1.BackColor = Color.FromArgb(50, 50, 50);
            goContainer1.BackgroundColor = "Back";
            goContainer1.Controls.Add(btnImageAdd);
            goContainer1.Controls.Add(btnImageDel);
            goContainer1.Controls.Add(lblImageTitle);
            goContainer1.Dock = DockStyle.Fill;
            goContainer1.Location = new Point(5, 5);
            goContainer1.Margin = new Padding(5);
            goContainer1.Name = "goContainer1";
            goContainer1.Size = new Size(479, 20);
            goContainer1.TabIndex = 0;
            goContainer1.TabStop = false;
            goContainer1.Text = "goContainer1";
            // 
            // btnImageAdd
            // 
            btnImageAdd.BackColor = Color.FromArgb(50, 50, 50);
            btnImageAdd.BackgroundColor = "Back";
            btnImageAdd.BackgroundDraw = false;
            btnImageAdd.BorderOnly = false;
            btnImageAdd.ButtonColor = "Base3";
            btnImageAdd.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            btnImageAdd.Dock = DockStyle.Right;
            btnImageAdd.FontName = "나눔고딕";
            btnImageAdd.FontSize = 12F;
            btnImageAdd.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnImageAdd.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnImageAdd.IconGap = 5F;
            btnImageAdd.IconSize = 12F;
            btnImageAdd.IconString = "fa-plus";
            btnImageAdd.Location = new Point(419, 0);
            btnImageAdd.Name = "btnImageAdd";
            btnImageAdd.Round = Going.UI.Enums.GoRoundType.All;
            btnImageAdd.Size = new Size(30, 20);
            btnImageAdd.TabIndex = 2;
            btnImageAdd.TabStop = false;
            btnImageAdd.TextColor = "Fore";
            // 
            // btnImageDel
            // 
            btnImageDel.BackColor = Color.FromArgb(50, 50, 50);
            btnImageDel.BackgroundColor = "Back";
            btnImageDel.BackgroundDraw = false;
            btnImageDel.BorderOnly = false;
            btnImageDel.ButtonColor = "Base3";
            btnImageDel.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            btnImageDel.Dock = DockStyle.Right;
            btnImageDel.FontName = "나눔고딕";
            btnImageDel.FontSize = 12F;
            btnImageDel.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnImageDel.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnImageDel.IconGap = 5F;
            btnImageDel.IconSize = 12F;
            btnImageDel.IconString = "fa-minus";
            btnImageDel.Location = new Point(449, 0);
            btnImageDel.Name = "btnImageDel";
            btnImageDel.Round = Going.UI.Enums.GoRoundType.All;
            btnImageDel.Size = new Size(30, 20);
            btnImageDel.TabIndex = 1;
            btnImageDel.TabStop = false;
            btnImageDel.TextColor = "Fore";
            // 
            // lblImageTitle
            // 
            lblImageTitle.BackColor = Color.FromArgb(50, 50, 50);
            lblImageTitle.BackgroundColor = "Back";
            lblImageTitle.BackgroundDraw = false;
            lblImageTitle.BorderOnly = false;
            lblImageTitle.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleLeft;
            lblImageTitle.Dock = DockStyle.Left;
            lblImageTitle.FontName = "나눔고딕";
            lblImageTitle.FontSize = 12F;
            lblImageTitle.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            lblImageTitle.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            lblImageTitle.IconGap = 5F;
            lblImageTitle.IconSize = 12F;
            lblImageTitle.IconString = "fa-grip";
            lblImageTitle.LabelColor = "Base2";
            lblImageTitle.Location = new Point(0, 0);
            lblImageTitle.Name = "lblImageTitle";
            lblImageTitle.Round = Going.UI.Enums.GoRoundType.All;
            lblImageTitle.Size = new Size(113, 20);
            lblImageTitle.TabIndex = 0;
            lblImageTitle.TabStop = false;
            lblImageTitle.Text = "이미지 목록";
            lblImageTitle.TextColor = "Fore";
            lblImageTitle.TextPadding.Bottom = 0F;
            lblImageTitle.TextPadding.Left = 5F;
            lblImageTitle.TextPadding.Right = 0F;
            lblImageTitle.TextPadding.Top = 0F;
            // 
            // grid
            // 
            grid.BackColor = Color.FromArgb(50, 50, 50);
            grid.BackgroundColor = "Back";
            grid.ContentSize = new Size(150, 150);
            grid.Dock = DockStyle.Fill;
            grid.Gap = 3;
            grid.Location = new Point(5, 35);
            grid.Margin = new Padding(5);
            grid.MultiSelect = true;
            grid.Name = "grid";
            grid.Selectable = true;
            grid.Size = new Size(479, 471);
            grid.TabIndex = 1;
            grid.Text = "goContentGrid1";
            // 
            // dgFont
            // 
            dgFont.BackColor = Color.FromArgb(50, 50, 50);
            dgFont.BackgroundColor = "Back";
            dgFont.BoxColor = "Base2";
            dgFont.ColumnColor = "Base1";
            dgFont.ColumnHeight = 30F;
            dgFont.Dock = DockStyle.Fill;
            dgFont.FontName = "나눔고딕";
            dgFont.FontSize = 12F;
            dgFont.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            dgFont.Location = new Point(514, 35);
            dgFont.Margin = new Padding(5);
            dgFont.Name = "dgFont";
            dgFont.RowColor = "Base2";
            dgFont.RowHeight = 30F;
            dgFont.ScrollMode = Going.UI.Utils.ScrollMode.Vertical;
            dgFont.SelectedRowColor = "Select";
            dgFont.SelectionMode = Going.UI.Enums.GoDataGridSelectionMode.Single;
            dgFont.Size = new Size(479, 471);
            dgFont.SummaryRowColor = "Base1";
            dgFont.TabIndex = 3;
            dgFont.TabStop = false;
            dgFont.Text = "goDataGrid1";
            dgFont.TextColor = "Fore";
            // 
            // FormResourceManager
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 561);
            Controls.Add(tpnl2);
            Controls.Add(tpnl1);
            MinimumSize = new Size(1024, 600);
            Name = "FormResourceManager";
            Padding = new Padding(5);
            Text = "리소스 관리자";
            Title = "리소스 관리자";
            TitleIconString = "fa-images";
            tpnl1.ResumeLayout(false);
            tpnl2.ResumeLayout(false);
            goContainer2.ResumeLayout(false);
            goContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Going.UI.Forms.Containers.GoTableLayoutPanel tpnl1;
        private Going.UI.Forms.Controls.GoButton btnClose;
        private Going.UI.Forms.Containers.GoTableLayoutPanel tpnl2;
        private Going.UI.Forms.Containers.GoContainer goContainer1;
        private Going.UI.Forms.Controls.GoButton btnImageAdd;
        private Going.UI.Forms.Controls.GoButton btnImageDel;
        private Going.UI.Forms.Controls.GoLabel lblImageTitle;
        private Controls.GoContentGrid grid;
        private Going.UI.Forms.Containers.GoContainer goContainer2;
        private Going.UI.Forms.Controls.GoButton btnFontAdd;
        private Going.UI.Forms.Controls.GoButton btnFontDel;
        private Going.UI.Forms.Controls.GoLabel lblFontTitle;
        private Going.UI.Forms.Controls.GoDataGrid dgFont;
    }
}