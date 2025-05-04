namespace Going.UIEditor.Forms
{
    partial class FormNewPage
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
            lblPageType = new UI.Forms.Controls.GoLabel();
            lblPageName = new UI.Forms.Controls.GoLabel();
            inName = new UI.Forms.Controls.GoInputString();
            inUseIcPage = new UI.Forms.Controls.GoInputBoolean();
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
            goTableLayoutPanel1.TabIndex = 3;
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
            goTableLayoutPanel2.Controls.Add(lblPageType, 0, 1);
            goTableLayoutPanel2.Controls.Add(lblPageName, 0, 0);
            goTableLayoutPanel2.Controls.Add(inName, 1, 0);
            goTableLayoutPanel2.Controls.Add(inUseIcPage, 1, 1);
            goTableLayoutPanel2.Dock = DockStyle.Fill;
            goTableLayoutPanel2.Location = new Point(5, 5);
            goTableLayoutPanel2.Name = "goTableLayoutPanel2";
            goTableLayoutPanel2.RowCount = 2;
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.Size = new Size(290, 92);
            goTableLayoutPanel2.TabIndex = 4;
            // 
            // lblPageType
            // 
            lblPageType.BackColor = Color.FromArgb(50, 50, 50);
            lblPageType.BackgroundColor = "Back";
            lblPageType.BackgroundDraw = false;
            lblPageType.BorderOnly = false;
            lblPageType.ContentAlignment = UI.Enums.GoContentAlignment.MiddleRight;
            lblPageType.Dock = DockStyle.Fill;
            lblPageType.FontName = "나눔고딕";
            lblPageType.FontSize = 12F;
            lblPageType.FontStyle = UI.Enums.GoFontStyle.Normal;
            lblPageType.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            lblPageType.IconGap = 5F;
            lblPageType.IconSize = 12F;
            lblPageType.IconString = null;
            lblPageType.LabelColor = "Base2";
            lblPageType.Location = new Point(5, 51);
            lblPageType.Margin = new Padding(5);
            lblPageType.Name = "lblPageType";
            lblPageType.Round = UI.Enums.GoRoundType.All;
            lblPageType.Size = new Size(90, 36);
            lblPageType.TabIndex = 1;
            lblPageType.TabStop = false;
            lblPageType.Text = "Page Type";
            lblPageType.TextColor = "Fore";
            lblPageType.TextPadding.Bottom = 0F;
            lblPageType.TextPadding.Left = 0F;
            lblPageType.TextPadding.Right = 0F;
            lblPageType.TextPadding.Top = 0F;
            // 
            // lblPageName
            // 
            lblPageName.BackColor = Color.FromArgb(50, 50, 50);
            lblPageName.BackgroundColor = "Back";
            lblPageName.BackgroundDraw = false;
            lblPageName.BorderOnly = false;
            lblPageName.ContentAlignment = UI.Enums.GoContentAlignment.MiddleRight;
            lblPageName.Dock = DockStyle.Fill;
            lblPageName.FontName = "나눔고딕";
            lblPageName.FontSize = 12F;
            lblPageName.FontStyle = UI.Enums.GoFontStyle.Normal;
            lblPageName.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            lblPageName.IconGap = 5F;
            lblPageName.IconSize = 12F;
            lblPageName.IconString = null;
            lblPageName.LabelColor = "Base2";
            lblPageName.Location = new Point(5, 5);
            lblPageName.Margin = new Padding(5);
            lblPageName.Name = "lblPageName";
            lblPageName.Round = UI.Enums.GoRoundType.All;
            lblPageName.Size = new Size(90, 36);
            lblPageName.TabIndex = 0;
            lblPageName.TabStop = false;
            lblPageName.Text = "Page Name";
            lblPageName.TextColor = "Fore";
            lblPageName.TextPadding.Bottom = 0F;
            lblPageName.TextPadding.Left = 0F;
            lblPageName.TextPadding.Right = 0F;
            lblPageName.TextPadding.Top = 0F;
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
            // inUseIcPage
            // 
            inUseIcPage.BackColor = Color.FromArgb(50, 50, 50);
            inUseIcPage.BackgroundColor = "Back";
            inUseIcPage.BorderColor = "Base3";
            inUseIcPage.ButtonSize = null;
            goTableLayoutPanel2.SetColumnSpan(inUseIcPage, 3);
            inUseIcPage.Direction = UI.Enums.GoDirectionHV.Horizon;
            inUseIcPage.Dock = DockStyle.Fill;
            inUseIcPage.FillColor = "Base3";
            inUseIcPage.FontName = "나눔고딕";
            inUseIcPage.FontSize = 12F;
            inUseIcPage.FontStyle = UI.Enums.GoFontStyle.Normal;
            inUseIcPage.IconGap = 5F;
            inUseIcPage.IconSize = 12F;
            inUseIcPage.IconString = null;
            inUseIcPage.Location = new Point(103, 49);
            inUseIcPage.Name = "inUseIcPage";
            inUseIcPage.OffIconString = null;
            inUseIcPage.OffText = "GoPage";
            inUseIcPage.OnIconString = null;
            inUseIcPage.OnText = "IcPage";
            inUseIcPage.Round = UI.Enums.GoRoundType.All;
            inUseIcPage.Size = new Size(184, 40);
            inUseIcPage.TabIndex = 4;
            inUseIcPage.TabStop = false;
            inUseIcPage.Text = "goInputBoolean1";
            inUseIcPage.TextColor = "Fore";
            inUseIcPage.Title = null;
            inUseIcPage.TitleSize = null;
            inUseIcPage.Value = false;
            inUseIcPage.ValueColor = "Base1";
            // 
            // FormNewPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 142);
            Controls.Add(goTableLayoutPanel2);
            Controls.Add(goTableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormNewPage";
            Padding = new Padding(5);
            Text = "페이지 추가";
            Title = "페이지 추가";
            TitleIconString = "fa-file-image";
            goTableLayoutPanel1.ResumeLayout(false);
            goTableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel1;
        private UI.Forms.Controls.GoButton btnOK;
        private UI.Forms.Controls.GoButton btnCancel;
        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel2;
        private UI.Forms.Controls.GoLabel lblPageType;
        private UI.Forms.Controls.GoLabel lblPageName;
        private UI.Forms.Controls.GoInputString inName;
        private UI.Forms.Controls.GoInputBoolean inUseIcPage;
    }
}