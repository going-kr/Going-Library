namespace SampleForms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gwButton1 = new Going.Forms.Controls.GoButton();
            SuspendLayout();
            // 
            // gwButton1
            // 
            gwButton1.BackColor = Color.FromArgb(50, 50, 50);
            gwButton1.BackgroundColor = "Back";
            gwButton1.BackgroundDraw = true;
            gwButton1.BorderOnly = false;
            gwButton1.ButtonColor = "Base3";
            gwButton1.FontName = "나눔고딕";
            gwButton1.FontSize = 12F;
            gwButton1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            gwButton1.IconGap = 5F;
            gwButton1.IconSize = 12F;
            gwButton1.IconString = "fa-check";
            gwButton1.Location = new Point(302, 34);
            gwButton1.Name = "gwButton1";
            gwButton1.Round = Going.UI.Enums.GoRoundType.All;
            gwButton1.Size = new Size(165, 49);
            gwButton1.TabIndex = 0;
            gwButton1.TabStop = false;
            gwButton1.Text = "확인";
            gwButton1.TextColor = "Fore";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(50, 50, 50);
            ClientSize = new Size(800, 450);
            Controls.Add(gwButton1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Going.Forms.Controls.GoButton gwButton1;
    }
}
