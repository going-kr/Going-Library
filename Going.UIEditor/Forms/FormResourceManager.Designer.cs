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
            tpnl1 = new UI.Forms.Containers.GoTableLayoutPanel();
            btnClose = new UI.Forms.Controls.GoButton();
            tpnl2 = new UI.Forms.Containers.GoTableLayoutPanel();
            goContainer1 = new UI.Forms.Containers.GoContainer();
            btnAdd = new UI.Forms.Controls.GoButton();
            btnDel = new UI.Forms.Controls.GoButton();
            lblTitle = new UI.Forms.Controls.GoLabel();
            grid = new Controls.GoContentGrid();
            tpnl1.SuspendLayout();
            tpnl2.SuspendLayout();
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
            tpnl1.Size = new Size(774, 40);
            tpnl1.TabIndex = 3;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(50, 50, 50);
            btnClose.BackgroundColor = "Back";
            btnClose.BackgroundDraw = true;
            btnClose.BorderOnly = false;
            btnClose.ButtonColor = "Base3";
            btnClose.Dock = DockStyle.Fill;
            btnClose.FontName = "나눔고딕";
            btnClose.FontSize = 12F;
            btnClose.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnClose.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnClose.IconGap = 5F;
            btnClose.IconSize = 12F;
            btnClose.IconString = null;
            btnClose.Location = new Point(679, 5);
            btnClose.Margin = new Padding(5);
            btnClose.Name = "btnClose";
            btnClose.Round = UI.Enums.GoRoundType.All;
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
            tpnl2.TabIndex = 4;
            // 
            // goContainer1
            // 
            goContainer1.BackColor = Color.FromArgb(50, 50, 50);
            goContainer1.BackgroundColor = "Back";
            goContainer1.Controls.Add(btnAdd);
            goContainer1.Controls.Add(btnDel);
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
            // btnAdd
            // 
            btnAdd.BackColor = Color.FromArgb(50, 50, 50);
            btnAdd.BackgroundColor = "Back";
            btnAdd.BackgroundDraw = false;
            btnAdd.BorderOnly = false;
            btnAdd.ButtonColor = "Base3";
            btnAdd.Dock = DockStyle.Right;
            btnAdd.FontName = "나눔고딕";
            btnAdd.FontSize = 12F;
            btnAdd.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnAdd.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnAdd.IconGap = 5F;
            btnAdd.IconSize = 12F;
            btnAdd.IconString = "fa-plus";
            btnAdd.Location = new Point(704, 0);
            btnAdd.Name = "btnAdd";
            btnAdd.Round = UI.Enums.GoRoundType.All;
            btnAdd.Size = new Size(30, 20);
            btnAdd.TabIndex = 2;
            btnAdd.TabStop = false;
            btnAdd.TextColor = "Fore";
            // 
            // btnDel
            // 
            btnDel.BackColor = Color.FromArgb(50, 50, 50);
            btnDel.BackgroundColor = "Back";
            btnDel.BackgroundDraw = false;
            btnDel.BorderOnly = false;
            btnDel.ButtonColor = "Base3";
            btnDel.Dock = DockStyle.Right;
            btnDel.FontName = "나눔고딕";
            btnDel.FontSize = 12F;
            btnDel.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnDel.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnDel.IconGap = 5F;
            btnDel.IconSize = 12F;
            btnDel.IconString = "fa-minus";
            btnDel.Location = new Point(734, 0);
            btnDel.Name = "btnDel";
            btnDel.Round = UI.Enums.GoRoundType.All;
            btnDel.Size = new Size(30, 20);
            btnDel.TabIndex = 1;
            btnDel.TabStop = false;
            btnDel.TextColor = "Fore";
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
            grid.Text = "goContentGrid1";
            // 
            // FormResourceManager
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(tpnl2);
            Controls.Add(tpnl1);
            MinimumSize = new Size(800, 600);
            Name = "FormResourceManager";
            Padding = new Padding(5);
            Text = "리소스 관리자";
            Title = "리소스 관리자";
            TitleIconString = "fa-images";
            tpnl1.ResumeLayout(false);
            tpnl2.ResumeLayout(false);
            goContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoTableLayoutPanel tpnl1;
        private UI.Forms.Controls.GoButton btnClose;
        private UI.Forms.Containers.GoTableLayoutPanel tpnl2;
        private UI.Forms.Containers.GoContainer goContainer1;
        private UI.Forms.Controls.GoButton btnAdd;
        private UI.Forms.Controls.GoButton btnDel;
        private UI.Forms.Controls.GoLabel lblTitle;
        private Controls.GoContentGrid grid;
    }
}