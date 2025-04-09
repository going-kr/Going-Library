namespace Going.UIEditor.Forms
{
    partial class FormSetting
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
            UI.Datas.GoButtonItem goButtonItem1 = new UI.Datas.GoButtonItem();
            goTableLayoutPanel1 = new UI.Forms.Containers.GoTableLayoutPanel();
            btnOK = new UI.Forms.Controls.GoButton();
            btnCancel = new UI.Forms.Controls.GoButton();
            goTableLayoutPanel2 = new UI.Forms.Containers.GoTableLayoutPanel();
            lblPath = new UI.Forms.Controls.GoValueString();
            inLang = new UI.Forms.Controls.GoInputBoolean();
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
            goTableLayoutPanel1.Location = new Point(5, 86);
            goTableLayoutPanel1.Name = "goTableLayoutPanel1";
            goTableLayoutPanel1.RowCount = 1;
            goTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.Size = new Size(409, 40);
            goTableLayoutPanel1.TabIndex = 1;
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
            btnOK.Location = new Point(254, 5);
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
            btnCancel.Location = new Point(334, 5);
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
            goTableLayoutPanel2.ColumnCount = 1;
            goTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.Controls.Add(lblPath, 0, 0);
            goTableLayoutPanel2.Controls.Add(inLang, 0, 1);
            goTableLayoutPanel2.Dock = DockStyle.Fill;
            goTableLayoutPanel2.Location = new Point(5, 5);
            goTableLayoutPanel2.Name = "goTableLayoutPanel2";
            goTableLayoutPanel2.RowCount = 2;
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.Size = new Size(409, 81);
            goTableLayoutPanel2.TabIndex = 2;
            // 
            // lblPath
            // 
            lblPath.BackColor = Color.FromArgb(50, 50, 50);
            lblPath.BackgroundColor = "Back";
            lblPath.BorderColor = "Base3";
            goButtonItem1.IconString = "fa-ellipsis";
            goButtonItem1.Name = "select";
            goButtonItem1.Size = "100%";
            goButtonItem1.Text = null;
            lblPath.Buttons.Add(goButtonItem1);
            lblPath.ButtonSize = 50F;
            lblPath.Direction = UI.Enums.GoDirectionHV.Horizon;
            lblPath.Dock = DockStyle.Fill;
            lblPath.FillColor = "Base3";
            lblPath.FontName = "나눔고딕";
            lblPath.FontSize = 12F;
            lblPath.FontStyle = UI.Enums.GoFontStyle.Normal;
            lblPath.IconGap = 5F;
            lblPath.IconSize = 16F;
            lblPath.IconString = null;
            lblPath.Location = new Point(5, 5);
            lblPath.Margin = new Padding(5);
            lblPath.Name = "lblPath";
            lblPath.Round = UI.Enums.GoRoundType.All;
            lblPath.Size = new Size(399, 30);
            lblPath.TabIndex = 0;
            lblPath.TabStop = false;
            lblPath.Text = "goValueString1";
            lblPath.TextColor = "Fore";
            lblPath.Title = "Project Folder";
            lblPath.TitleSize = 100F;
            lblPath.Value = null;
            lblPath.ValueColor = "Base1";
            // 
            // inLang
            // 
            inLang.BackColor = Color.FromArgb(50, 50, 50);
            inLang.BackgroundColor = "Back";
            inLang.BorderColor = "Base3";
            inLang.ButtonSize = null;
            inLang.Direction = UI.Enums.GoDirectionHV.Horizon;
            inLang.Dock = DockStyle.Fill;
            inLang.FillColor = "Base3";
            inLang.FontName = "나눔고딕";
            inLang.FontSize = 12F;
            inLang.FontStyle = UI.Enums.GoFontStyle.Normal;
            inLang.IconGap = 5F;
            inLang.IconSize = 12F;
            inLang.IconString = null;
            inLang.Location = new Point(5, 45);
            inLang.Margin = new Padding(5);
            inLang.Name = "inLang";
            inLang.OffIconString = null;
            inLang.OffText = "한국어";
            inLang.OnIconString = null;
            inLang.OnText = "English";
            inLang.Round = UI.Enums.GoRoundType.All;
            inLang.Size = new Size(399, 31);
            inLang.TabIndex = 1;
            inLang.TabStop = false;
            inLang.Text = "goInputBoolean1";
            inLang.TextColor = "Fore";
            inLang.Title = "Language";
            inLang.TitleSize = 100F;
            inLang.Value = false;
            inLang.ValueColor = "Base1";
            // 
            // FormSetting
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(419, 131);
            Controls.Add(goTableLayoutPanel2);
            Controls.Add(goTableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSetting";
            Padding = new Padding(5);
            Text = "프로그램 설정";
            Title = "프로그램 설정";
            TitleIconString = "fa-gears";
            goTableLayoutPanel1.ResumeLayout(false);
            goTableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel1;
        private UI.Forms.Controls.GoButton btnOK;
        private UI.Forms.Controls.GoButton btnCancel;
        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel2;
        private UI.Forms.Controls.GoValueString lblPath;
        private UI.Forms.Controls.GoInputBoolean inLang;
    }
}