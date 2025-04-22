namespace Going.UIEditor.Forms
{
    partial class FormImageSelector
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
            goTableLayoutPanel1 = new UI.Forms.Containers.GoTableLayoutPanel();
            btnNotSelect = new UI.Forms.Controls.GoButton();
            btnOK = new UI.Forms.Controls.GoButton();
            btnCancel = new UI.Forms.Controls.GoButton();
            tpnl2 = new UI.Forms.Containers.GoTableLayoutPanel();
            goContainer1 = new UI.Forms.Containers.GoContainer();
            lblTitle = new UI.Forms.Controls.GoLabel();
            grid = new Controls.GoContentGrid();
            goTableLayoutPanel1.SuspendLayout();
            tpnl2.SuspendLayout();
            goContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // goTableLayoutPanel1
            // 
            goTableLayoutPanel1.BackColor = Color.FromArgb(50, 50, 50);
            goTableLayoutPanel1.BackgroundColor = "Back";
            goTableLayoutPanel1.ColumnCount = 4;
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            goTableLayoutPanel1.Controls.Add(btnNotSelect, 0, 0);
            goTableLayoutPanel1.Controls.Add(btnOK, 2, 0);
            goTableLayoutPanel1.Controls.Add(btnCancel, 3, 0);
            goTableLayoutPanel1.Dock = DockStyle.Bottom;
            goTableLayoutPanel1.Location = new Point(5, 516);
            goTableLayoutPanel1.Name = "goTableLayoutPanel1";
            goTableLayoutPanel1.RowCount = 1;
            goTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.Size = new Size(774, 40);
            goTableLayoutPanel1.TabIndex = 5;
            // 
            // btnNotSelect
            // 
            btnNotSelect.BackColor = Color.FromArgb(50, 50, 50);
            btnNotSelect.BackgroundColor = "Back";
            btnNotSelect.BackgroundDraw = true;
            btnNotSelect.BorderOnly = false;
            btnNotSelect.ButtonColor = "Base3";
            btnNotSelect.Dock = DockStyle.Fill;
            btnNotSelect.FontName = "나눔고딕";
            btnNotSelect.FontSize = 12F;
            btnNotSelect.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnNotSelect.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnNotSelect.IconGap = 5F;
            btnNotSelect.IconSize = 12F;
            btnNotSelect.IconString = null;
            btnNotSelect.Location = new Point(5, 5);
            btnNotSelect.Margin = new Padding(5);
            btnNotSelect.Name = "btnNotSelect";
            btnNotSelect.Round = UI.Enums.GoRoundType.All;
            btnNotSelect.Size = new Size(90, 30);
            btnNotSelect.TabIndex = 2;
            btnNotSelect.TabStop = false;
            btnNotSelect.Text = "선택 안함";
            btnNotSelect.TextColor = "Fore";
            // 
            // btnOK
            // 
            btnOK.BackColor = Color.FromArgb(50, 50, 50);
            btnOK.BackgroundColor = "Back";
            btnOK.BackgroundDraw = true;
            btnOK.BorderOnly = false;
            btnOK.ButtonColor = "Base3";
            btnOK.Dock = DockStyle.Fill;
            btnOK.FontName = "나눔고딕";
            btnOK.FontSize = 12F;
            btnOK.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnOK.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnOK.IconGap = 5F;
            btnOK.IconSize = 12F;
            btnOK.IconString = null;
            btnOK.Location = new Point(619, 5);
            btnOK.Margin = new Padding(5);
            btnOK.Name = "btnOK";
            btnOK.Round = UI.Enums.GoRoundType.All;
            btnOK.Size = new Size(70, 30);
            btnOK.TabIndex = 0;
            btnOK.TabStop = false;
            btnOK.Text = "확인";
            btnOK.TextColor = "Fore";
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(50, 50, 50);
            btnCancel.BackgroundColor = "Back";
            btnCancel.BackgroundDraw = true;
            btnCancel.BorderOnly = false;
            btnCancel.ButtonColor = "Base3";
            btnCancel.Dock = DockStyle.Fill;
            btnCancel.FontName = "나눔고딕";
            btnCancel.FontSize = 12F;
            btnCancel.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnCancel.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnCancel.IconGap = 5F;
            btnCancel.IconSize = 12F;
            btnCancel.IconString = null;
            btnCancel.Location = new Point(699, 5);
            btnCancel.Margin = new Padding(5);
            btnCancel.Name = "btnCancel";
            btnCancel.Round = UI.Enums.GoRoundType.All;
            btnCancel.Size = new Size(70, 30);
            btnCancel.TabIndex = 1;
            btnCancel.TabStop = false;
            btnCancel.Text = "취소";
            btnCancel.TextColor = "Fore";
            // 
            // tpnl2
            // 
            tpnl2.BackColor = Color.FromArgb(50, 50, 50);
            tpnl2.BackgroundColor = "Back";
            tpnl2.ColumnCount = 1;
            tpnl2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tpnl2.Controls.Add(goContainer1, 0, 0);
            tpnl2.Controls.Add(grid, 0, 1);
            tpnl2.Dock = DockStyle.Fill;
            tpnl2.Location = new Point(5, 5);
            tpnl2.Name = "tpnl2";
            tpnl2.RowCount = 2;
            tpnl2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tpnl2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tpnl2.Size = new Size(774, 511);
            tpnl2.TabIndex = 6;
            // 
            // goContainer1
            // 
            goContainer1.BackColor = Color.FromArgb(50, 50, 50);
            goContainer1.BackgroundColor = "Back";
            goContainer1.Controls.Add(lblTitle);
            goContainer1.Dock = DockStyle.Fill;
            goContainer1.Location = new Point(5, 5);
            goContainer1.Margin = new Padding(5);
            goContainer1.Name = "goContainer1";
            goContainer1.Size = new Size(764, 20);
            goContainer1.TabIndex = 0;
            goContainer1.TabStop = false;
            goContainer1.Text = "goContainer1";
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.FromArgb(50, 50, 50);
            lblTitle.BackgroundColor = "Back";
            lblTitle.BackgroundDraw = false;
            lblTitle.BorderOnly = false;
            lblTitle.ContentAlignment = UI.Enums.GoContentAlignment.MiddleLeft;
            lblTitle.Dock = DockStyle.Left;
            lblTitle.FontName = "나눔고딕";
            lblTitle.FontSize = 12F;
            lblTitle.FontStyle = UI.Enums.GoFontStyle.Normal;
            lblTitle.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            lblTitle.IconGap = 5F;
            lblTitle.IconSize = 12F;
            lblTitle.IconString = "fa-grip";
            lblTitle.LabelColor = "Base2";
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Round = UI.Enums.GoRoundType.All;
            lblTitle.Size = new Size(113, 20);
            lblTitle.TabIndex = 0;
            lblTitle.TabStop = false;
            lblTitle.Text = "이미지 목록";
            lblTitle.TextColor = "Fore";
            lblTitle.TextPadding.Bottom = 0F;
            lblTitle.TextPadding.Left = 5F;
            lblTitle.TextPadding.Right = 0F;
            lblTitle.TextPadding.Top = 0F;
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
            grid.Size = new Size(764, 471);
            grid.TabIndex = 1;
            grid.Text = "이미지 선택";
            // 
            // FormImageSelector
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(tpnl2);
            Controls.Add(goTableLayoutPanel1);
            MinimumSize = new Size(800, 600);
            Name = "FormImageSelector";
            Padding = new Padding(5);
            Text = "이미지 선택";
            Title = "이미지 선택";
            TitleIconString = "fa-check";
            goTableLayoutPanel1.ResumeLayout(false);
            tpnl2.ResumeLayout(false);
            goContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel1;
        private UI.Forms.Controls.GoButton btnNotSelect;
        private UI.Forms.Controls.GoButton btnOK;
        private UI.Forms.Controls.GoButton btnCancel;
        private UI.Forms.Containers.GoTableLayoutPanel tpnl2;
        private UI.Forms.Containers.GoContainer goContainer1;
        private UI.Forms.Controls.GoLabel lblTitle;
        private Controls.GoContentGrid grid;
    }
}