namespace SampleForms
{
    partial class FormMain
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
            goButton1 = new Going.UI.Forms.Controls.GoButton();
            goLabel1 = new Going.UI.Forms.Controls.GoLabel();
            goPanel2 = new Going.UI.Forms.Containers.GoPanel();
            goButton2 = new Going.UI.Forms.Controls.GoButton();
            goPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // goButton1
            // 
            goButton1.BackColor = Color.FromArgb(50, 50, 50);
            goButton1.BackgroundColor = "Back";
            goButton1.BackgroundDraw = true;
            goButton1.BorderOnly = false;
            goButton1.ButtonColor = "Base3";
            goButton1.FontName = "나눔고딕";
            goButton1.FontSize = 12F;
            goButton1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goButton1.IconGap = 5F;
            goButton1.IconSize = 12F;
            goButton1.IconString = "fa-check";
            goButton1.Location = new Point(166, 43);
            goButton1.Name = "goButton1";
            goButton1.Round = Going.UI.Enums.GoRoundType.All;
            goButton1.Size = new Size(161, 49);
            goButton1.TabIndex = 0;
            goButton1.TabStop = false;
            goButton1.Text = "확인";
            goButton1.TextColor = "Fore";
            // 
            // goLabel1
            // 
            goLabel1.BackColor = Color.FromArgb(50, 50, 50);
            goLabel1.BackgroundColor = "Back";
            goLabel1.BackgroundDraw = true;
            goLabel1.BorderOnly = true;
            goLabel1.ContentAlignment = Going.UI.Enums.GoContentAlignment.TopLeft;
            goLabel1.FontName = "나눔고딕";
            goLabel1.FontSize = 12F;
            goLabel1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goLabel1.IconGap = 5F;
            goLabel1.IconSize = 12F;
            goLabel1.IconString = null;
            goLabel1.LabelColor = "Base3";
            goLabel1.Location = new Point(13, 43);
            goLabel1.Name = "goLabel1";
            goLabel1.Round = Going.UI.Enums.GoRoundType.All;
            goLabel1.Size = new Size(147, 147);
            goLabel1.TabIndex = 1;
            goLabel1.TabStop = false;
            goLabel1.Text = "동해물과 백두산이 마르고 닳도록 \r\n하나님이 보우하사 우리 나라 만세\r\n무궁화 삼천리 화려강산 \r\n대한 사람 대한으로 길이 보전 하세";
            goLabel1.TextColor = "Fore";
            goLabel1.TextPadding.Bottom = 10F;
            goLabel1.TextPadding.Left = 10F;
            goLabel1.TextPadding.Right = 10F;
            goLabel1.TextPadding.Top = 10F;
            // 
            // goPanel2
            // 
            goPanel2.BackColor = Color.FromArgb(50, 50, 50);
            goPanel2.BackgroundColor = "Back";
            goPanel2.BackgroundDraw = true;
            goPanel2.BorderOnly = false;
            goPanel2.Controls.Add(goButton2);
            goPanel2.Controls.Add(goLabel1);
            goPanel2.Controls.Add(goButton1);
            goPanel2.FontName = "나눔고딕";
            goPanel2.FontSize = 12F;
            goPanel2.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goPanel2.IconGap = 5F;
            goPanel2.IconSize = 12F;
            goPanel2.IconString = "fa-check";
            goPanel2.Location = new Point(109, 71);
            goPanel2.Name = "goPanel2";
            goPanel2.Padding = new Padding(10, 40, 10, 10);
            goPanel2.PanelColor = "Base2";
            goPanel2.Round = Going.UI.Enums.GoRoundType.All;
            goPanel2.Size = new Size(340, 203);
            goPanel2.TabIndex = 2;
            goPanel2.TabStop = false;
            goPanel2.Text = "Panel";
            goPanel2.TextColor = "Fore";
            // 
            // goButton2
            // 
            goButton2.BackColor = Color.FromArgb(50, 50, 50);
            goButton2.BackgroundColor = "Back";
            goButton2.BackgroundDraw = true;
            goButton2.BorderOnly = false;
            goButton2.ButtonColor = "danger";
            goButton2.FontName = "나눔고딕";
            goButton2.FontSize = 12F;
            goButton2.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goButton2.IconGap = 5F;
            goButton2.IconSize = 12F;
            goButton2.IconString = "fa-ban";
            goButton2.Location = new Point(166, 98);
            goButton2.Name = "goButton2";
            goButton2.Round = Going.UI.Enums.GoRoundType.All;
            goButton2.Size = new Size(161, 49);
            goButton2.TabIndex = 2;
            goButton2.TabStop = false;
            goButton2.Text = "취소";
            goButton2.TextColor = "Fore";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(50, 50, 50);
            ClientSize = new Size(800, 450);
            Controls.Add(goPanel2);
            Name = "FormMain";
            Text = "Sample Window";
            Title = "Sample Window";
            TitleIconString = "fa-check";
            goPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Going.UI.Forms.Controls.GoButton goButton1;
        private Going.UI.Forms.Controls.GoLabel goLabel1;
        private Going.UI.Forms.Containers.GoPanel goPanel1;
        private Going.UI.Forms.Containers.GoPanel goPanel2;
        private Going.UI.Forms.Controls.GoButton goButton2;
    }
}
