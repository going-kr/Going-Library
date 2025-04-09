namespace Going.UIEditor.Forms
{
    partial class FormNewFile
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
            btnOK = new UI.Forms.Controls.GoButton();
            btnCancel = new UI.Forms.Controls.GoButton();
            goTableLayoutPanel2 = new UI.Forms.Containers.GoTableLayoutPanel();
            inHeight = new UI.Forms.Controls.GoInputInteger();
            goLabel3 = new UI.Forms.Controls.GoLabel();
            lblScreenSize = new UI.Forms.Controls.GoLabel();
            lblProjectName = new UI.Forms.Controls.GoLabel();
            inName = new UI.Forms.Controls.GoInputString();
            inWidth = new UI.Forms.Controls.GoInputInteger();
            goTableLayoutPanel1.SuspendLayout();
            goTableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // goTableLayoutPanel1
            // 
            goTableLayoutPanel1.BackColor = Color.FromArgb(50, 50, 50);
            goTableLayoutPanel1.BackgroundColor = "Back";
            goTableLayoutPanel1.ColumnCount = 3;
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            goTableLayoutPanel1.Controls.Add(btnOK, 1, 0);
            goTableLayoutPanel1.Controls.Add(btnCancel, 2, 0);
            goTableLayoutPanel1.Dock = DockStyle.Bottom;
            goTableLayoutPanel1.Location = new Point(5, 97);
            goTableLayoutPanel1.Name = "goTableLayoutPanel1";
            goTableLayoutPanel1.RowCount = 1;
            goTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.Size = new Size(290, 40);
            goTableLayoutPanel1.TabIndex = 2;
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
            btnOK.Location = new Point(135, 5);
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
            btnCancel.Location = new Point(215, 5);
            btnCancel.Margin = new Padding(5);
            btnCancel.Name = "btnCancel";
            btnCancel.Round = UI.Enums.GoRoundType.All;
            btnCancel.Size = new Size(70, 30);
            btnCancel.TabIndex = 1;
            btnCancel.TabStop = false;
            btnCancel.Text = "취소";
            btnCancel.TextColor = "Fore";
            // 
            // goTableLayoutPanel2
            // 
            goTableLayoutPanel2.BackColor = Color.FromArgb(50, 50, 50);
            goTableLayoutPanel2.BackgroundColor = "Back";
            goTableLayoutPanel2.ColumnCount = 4;
            goTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            goTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            goTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.Controls.Add(inHeight, 3, 1);
            goTableLayoutPanel2.Controls.Add(goLabel3, 2, 1);
            goTableLayoutPanel2.Controls.Add(lblScreenSize, 0, 1);
            goTableLayoutPanel2.Controls.Add(lblProjectName, 0, 0);
            goTableLayoutPanel2.Controls.Add(inName, 1, 0);
            goTableLayoutPanel2.Controls.Add(inWidth, 1, 1);
            goTableLayoutPanel2.Dock = DockStyle.Fill;
            goTableLayoutPanel2.Location = new Point(5, 5);
            goTableLayoutPanel2.Name = "goTableLayoutPanel2";
            goTableLayoutPanel2.RowCount = 2;
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.Size = new Size(290, 92);
            goTableLayoutPanel2.TabIndex = 3;
            // 
            // inHeight
            // 
            inHeight.BackColor = Color.FromArgb(50, 50, 50);
            inHeight.BackgroundColor = "Back";
            inHeight.BorderColor = "Base3";
            inHeight.ButtonSize = null;
            inHeight.Direction = UI.Enums.GoDirectionHV.Horizon;
            inHeight.Dock = DockStyle.Fill;
            inHeight.FillColor = "Base3";
            inHeight.FontName = "나눔고딕";
            inHeight.FontSize = 12F;
            inHeight.FontStyle = UI.Enums.GoFontStyle.Normal;
            inHeight.FormatString = null;
            inHeight.IconGap = 5F;
            inHeight.IconSize = 12F;
            inHeight.IconString = null;
            inHeight.Location = new Point(210, 51);
            inHeight.Margin = new Padding(5);
            inHeight.Maximum = null;
            inHeight.Minimum = null;
            inHeight.Name = "inHeight";
            inHeight.Round = UI.Enums.GoRoundType.All;
            inHeight.Size = new Size(75, 36);
            inHeight.TabIndex = 5;
            inHeight.TabStop = false;
            inHeight.Text = "goInputInteger2";
            inHeight.TextColor = "Fore";
            inHeight.Title = null;
            inHeight.TitleSize = null;
            inHeight.Unit = null;
            inHeight.UnitSize = null;
            inHeight.Value = 0;
            inHeight.ValueColor = "Base1";
            // 
            // goLabel3
            // 
            goLabel3.BackColor = Color.FromArgb(50, 50, 50);
            goLabel3.BackgroundColor = "Back";
            goLabel3.BackgroundDraw = false;
            goLabel3.BorderOnly = false;
            goLabel3.ContentAlignment = UI.Enums.GoContentAlignment.MiddleCenter;
            goLabel3.Dock = DockStyle.Fill;
            goLabel3.FontName = "나눔고딕";
            goLabel3.FontSize = 12F;
            goLabel3.FontStyle = UI.Enums.GoFontStyle.Normal;
            goLabel3.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            goLabel3.IconGap = 5F;
            goLabel3.IconSize = 12F;
            goLabel3.IconString = null;
            goLabel3.LabelColor = "Base2";
            goLabel3.Location = new Point(190, 51);
            goLabel3.Margin = new Padding(5);
            goLabel3.Name = "goLabel3";
            goLabel3.Round = UI.Enums.GoRoundType.All;
            goLabel3.Size = new Size(10, 36);
            goLabel3.TabIndex = 2;
            goLabel3.TabStop = false;
            goLabel3.Text = "X";
            goLabel3.TextColor = "Fore";
            goLabel3.TextPadding.Bottom = 0F;
            goLabel3.TextPadding.Left = 0F;
            goLabel3.TextPadding.Right = 0F;
            goLabel3.TextPadding.Top = 0F;
            // 
            // lblScreenSize
            // 
            lblScreenSize.BackColor = Color.FromArgb(50, 50, 50);
            lblScreenSize.BackgroundColor = "Back";
            lblScreenSize.BackgroundDraw = false;
            lblScreenSize.BorderOnly = false;
            lblScreenSize.ContentAlignment = UI.Enums.GoContentAlignment.MiddleRight;
            lblScreenSize.Dock = DockStyle.Fill;
            lblScreenSize.FontName = "나눔고딕";
            lblScreenSize.FontSize = 12F;
            lblScreenSize.FontStyle = UI.Enums.GoFontStyle.Normal;
            lblScreenSize.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            lblScreenSize.IconGap = 5F;
            lblScreenSize.IconSize = 12F;
            lblScreenSize.IconString = null;
            lblScreenSize.LabelColor = "Base2";
            lblScreenSize.Location = new Point(5, 51);
            lblScreenSize.Margin = new Padding(5);
            lblScreenSize.Name = "lblScreenSize";
            lblScreenSize.Round = UI.Enums.GoRoundType.All;
            lblScreenSize.Size = new Size(90, 36);
            lblScreenSize.TabIndex = 1;
            lblScreenSize.TabStop = false;
            lblScreenSize.Text = "Screen Size";
            lblScreenSize.TextColor = "Fore";
            lblScreenSize.TextPadding.Bottom = 0F;
            lblScreenSize.TextPadding.Left = 0F;
            lblScreenSize.TextPadding.Right = 0F;
            lblScreenSize.TextPadding.Top = 0F;
            // 
            // lblProjectName
            // 
            lblProjectName.BackColor = Color.FromArgb(50, 50, 50);
            lblProjectName.BackgroundColor = "Back";
            lblProjectName.BackgroundDraw = false;
            lblProjectName.BorderOnly = false;
            lblProjectName.ContentAlignment = UI.Enums.GoContentAlignment.MiddleRight;
            lblProjectName.Dock = DockStyle.Fill;
            lblProjectName.FontName = "나눔고딕";
            lblProjectName.FontSize = 12F;
            lblProjectName.FontStyle = UI.Enums.GoFontStyle.Normal;
            lblProjectName.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            lblProjectName.IconGap = 5F;
            lblProjectName.IconSize = 12F;
            lblProjectName.IconString = null;
            lblProjectName.LabelColor = "Base2";
            lblProjectName.Location = new Point(5, 5);
            lblProjectName.Margin = new Padding(5);
            lblProjectName.Name = "lblProjectName";
            lblProjectName.Round = UI.Enums.GoRoundType.All;
            lblProjectName.Size = new Size(90, 36);
            lblProjectName.TabIndex = 0;
            lblProjectName.TabStop = false;
            lblProjectName.Text = "Project Name";
            lblProjectName.TextColor = "Fore";
            lblProjectName.TextPadding.Bottom = 0F;
            lblProjectName.TextPadding.Left = 0F;
            lblProjectName.TextPadding.Right = 0F;
            lblProjectName.TextPadding.Top = 0F;
            // 
            // inName
            // 
            inName.BackColor = Color.FromArgb(50, 50, 50);
            inName.BackgroundColor = "Back";
            inName.BorderColor = "Base3";
            inName.ButtonSize = null;
            goTableLayoutPanel2.SetColumnSpan(inName, 3);
            inName.Direction = UI.Enums.GoDirectionHV.Horizon;
            inName.Dock = DockStyle.Fill;
            inName.FillColor = "Base3";
            inName.FontName = "나눔고딕";
            inName.FontSize = 12F;
            inName.FontStyle = UI.Enums.GoFontStyle.Normal;
            inName.IconGap = 5F;
            inName.IconSize = 12F;
            inName.IconString = null;
            inName.Location = new Point(105, 5);
            inName.Margin = new Padding(5);
            inName.Name = "inName";
            inName.Round = UI.Enums.GoRoundType.All;
            inName.Size = new Size(180, 36);
            inName.TabIndex = 3;
            inName.TabStop = false;
            inName.Text = "goInputString1";
            inName.TextColor = "Fore";
            inName.Title = null;
            inName.TitleSize = null;
            inName.Value = "";
            inName.ValueColor = "Base1";
            // 
            // inWidth
            // 
            inWidth.BackColor = Color.FromArgb(50, 50, 50);
            inWidth.BackgroundColor = "Back";
            inWidth.BorderColor = "Base3";
            inWidth.ButtonSize = null;
            inWidth.Direction = UI.Enums.GoDirectionHV.Horizon;
            inWidth.Dock = DockStyle.Fill;
            inWidth.FillColor = "Base3";
            inWidth.FontName = "나눔고딕";
            inWidth.FontSize = 12F;
            inWidth.FontStyle = UI.Enums.GoFontStyle.Normal;
            inWidth.FormatString = null;
            inWidth.IconGap = 5F;
            inWidth.IconSize = 12F;
            inWidth.IconString = null;
            inWidth.Location = new Point(105, 51);
            inWidth.Margin = new Padding(5);
            inWidth.Maximum = null;
            inWidth.Minimum = null;
            inWidth.Name = "inWidth";
            inWidth.Round = UI.Enums.GoRoundType.All;
            inWidth.Size = new Size(75, 36);
            inWidth.TabIndex = 4;
            inWidth.TabStop = false;
            inWidth.Text = "goInputInteger1";
            inWidth.TextColor = "Fore";
            inWidth.Title = null;
            inWidth.TitleSize = null;
            inWidth.Unit = null;
            inWidth.UnitSize = null;
            inWidth.Value = 0;
            inWidth.ValueColor = "Base1";
            // 
            // FormNewFile
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 142);
            Controls.Add(goTableLayoutPanel2);
            Controls.Add(goTableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormNewFile";
            Padding = new Padding(5);
            Text = "새 파일";
            Title = "새 파일";
            TitleIconString = "fa-file";
            goTableLayoutPanel1.ResumeLayout(false);
            goTableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel1;
        private UI.Forms.Controls.GoButton btnOK;
        private UI.Forms.Controls.GoButton btnCancel;
        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel2;
        private UI.Forms.Controls.GoLabel goLabel3;
        private UI.Forms.Controls.GoLabel lblScreenSize;
        private UI.Forms.Controls.GoLabel lblProjectName;
        private UI.Forms.Controls.GoInputInteger inHeight;
        private UI.Forms.Controls.GoInputString inName;
        private UI.Forms.Controls.GoInputInteger inWidth;
    }
}