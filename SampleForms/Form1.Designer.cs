namespace SampleForms
{
    partial class Form1
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
            goInputString1 = new Going.UI.Forms.Controls.GoInputString();
            SuspendLayout();
            // 
            // goInputString1
            // 
            goInputString1.BackColor = Color.FromArgb(50, 50, 50);
            goInputString1.BackgroundColor = "Back";
            goInputString1.BorderColor = "Base3";
            goInputString1.ButtonSize = null;
            goInputString1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goInputString1.FillColor = "Base3";
            goInputString1.FontName = "나눔고딕";
            goInputString1.FontSize = 12F;
            goInputString1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            goInputString1.IconGap = 5F;
            goInputString1.IconSize = 12F;
            goInputString1.IconString = null;
            goInputString1.Location = new Point(12, 12);
            goInputString1.Name = "goInputString1";
            goInputString1.Round = Going.UI.Enums.GoRoundType.All;
            goInputString1.Size = new Size(132, 67);
            goInputString1.TabIndex = 0;
            goInputString1.TabStop = false;
            goInputString1.Text = "goInputString1";
            goInputString1.TextColor = "Fore";
            goInputString1.Title = null;
            goInputString1.TitleSize = null;
            goInputString1.Value = "";
            goInputString1.ValueColor = "Base1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(goInputString1);
            Name = "Form1";
            Text = "Form1";
            Title = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Going.UI.Forms.Controls.GoInputString goInputString1;
    }
}