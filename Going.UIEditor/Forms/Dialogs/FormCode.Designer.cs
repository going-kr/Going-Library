namespace Going.UIEditor.Forms.Dialogs
{
    partial class FormCode
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
            txtDesign = new TextBox();
            goTableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // goTableLayoutPanel1
            // 
            goTableLayoutPanel1.BackColor = Color.FromArgb(50, 50, 50);
            goTableLayoutPanel1.BackgroundColor = "Back";
            goTableLayoutPanel1.ColumnCount = 3;
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            goTableLayoutPanel1.Controls.Add(txtDesign, 0, 0);
            goTableLayoutPanel1.Dock = DockStyle.Fill;
            goTableLayoutPanel1.Location = new Point(10, 10);
            goTableLayoutPanel1.Name = "goTableLayoutPanel1";
            goTableLayoutPanel1.RowCount = 1;
            goTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.Size = new Size(1235, 903);
            goTableLayoutPanel1.TabIndex = 0;
            // 
            // txtDesign
            // 
            txtDesign.BackColor = Color.FromArgb(30, 30, 30);
            txtDesign.BorderStyle = BorderStyle.None;
            goTableLayoutPanel1.SetColumnSpan(txtDesign, 3);
            txtDesign.Dock = DockStyle.Fill;
            txtDesign.ForeColor = Color.White;
            txtDesign.Location = new Point(3, 3);
            txtDesign.Multiline = true;
            txtDesign.Name = "txtDesign";
            txtDesign.Size = new Size(1229, 897);
            txtDesign.TabIndex = 1;
            // 
            // FormCode
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1255, 923);
            Controls.Add(goTableLayoutPanel1);
            Name = "FormCode";
            Padding = new Padding(10);
            Text = "Code";
            Title = "Code";
            TitleIconString = "fa-check";
            goTableLayoutPanel1.ResumeLayout(false);
            goTableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel1;
        private TextBox txtDesign;
    }
}