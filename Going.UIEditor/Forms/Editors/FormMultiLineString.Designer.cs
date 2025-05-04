namespace Going.UIEditor.Forms
{
    partial class FormMultiLineString
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
            goBoxPanel1 = new UI.Forms.Containers.GoBoxPanel();
            txt = new TextBox();
            goTableLayoutPanel1.SuspendLayout();
            goTableLayoutPanel2.SuspendLayout();
            goBoxPanel1.SuspendLayout();
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
            goTableLayoutPanel1.Location = new Point(5, 216);
            goTableLayoutPanel1.Name = "goTableLayoutPanel1";
            goTableLayoutPanel1.RowCount = 1;
            goTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.Size = new Size(374, 40);
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
            btnOK.Location = new Point(219, 5);
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
            btnCancel.Location = new Point(299, 5);
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
            goTableLayoutPanel2.Controls.Add(goBoxPanel1, 0, 0);
            goTableLayoutPanel2.Dock = DockStyle.Fill;
            goTableLayoutPanel2.Location = new Point(5, 5);
            goTableLayoutPanel2.Name = "goTableLayoutPanel2";
            goTableLayoutPanel2.RowCount = 1;
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.Size = new Size(374, 211);
            goTableLayoutPanel2.TabIndex = 4;
            // 
            // goBoxPanel1
            // 
            goBoxPanel1.BackColor = Color.FromArgb(50, 50, 50);
            goBoxPanel1.BackgroundColor = "Back";
            goBoxPanel1.BackgroundDraw = true;
            goBoxPanel1.BorderColor = "Base3";
            goBoxPanel1.BoxColor = "Base1";
            goBoxPanel1.Controls.Add(txt);
            goBoxPanel1.Dock = DockStyle.Fill;
            goBoxPanel1.Location = new Point(5, 5);
            goBoxPanel1.Margin = new Padding(5);
            goBoxPanel1.Name = "goBoxPanel1";
            goBoxPanel1.Padding = new Padding(10);
            goBoxPanel1.Round = UI.Enums.GoRoundType.All;
            goBoxPanel1.Size = new Size(364, 201);
            goBoxPanel1.TabIndex = 0;
            goBoxPanel1.TabStop = false;
            goBoxPanel1.Text = "goBoxPanel1";
            // 
            // txt
            // 
            txt.BackColor = Color.FromArgb(30, 30, 30);
            txt.BorderStyle = BorderStyle.None;
            txt.Dock = DockStyle.Fill;
            txt.ForeColor = Color.White;
            txt.Location = new Point(10, 10);
            txt.Multiline = true;
            txt.Name = "txt";
            txt.Size = new Size(344, 181);
            txt.TabIndex = 0;
            // 
            // FormMultiLineString
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 261);
            Controls.Add(goTableLayoutPanel2);
            Controls.Add(goTableLayoutPanel1);
            Name = "FormMultiLineString";
            Padding = new Padding(5);
            Text = "Text";
            Title = "Text";
            goTableLayoutPanel1.ResumeLayout(false);
            goTableLayoutPanel2.ResumeLayout(false);
            goBoxPanel1.ResumeLayout(false);
            goBoxPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel1;
        private UI.Forms.Controls.GoButton btnOK;
        private UI.Forms.Controls.GoButton btnCancel;
        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel2;
        private UI.Forms.Containers.GoBoxPanel goBoxPanel1;
        private TextBox txt;
    }
}