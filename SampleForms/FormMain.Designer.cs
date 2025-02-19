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
            Going.UI.Datas.GoButtonInfo goButtonInfo1 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo2 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo3 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo4 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo5 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo6 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo7 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo8 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo9 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo10 = new Going.UI.Datas.GoButtonInfo();
            goButton1 = new Going.UI.Forms.Controls.GoButton();
            goLabel1 = new Going.UI.Forms.Controls.GoLabel();
            goPanel2 = new Going.UI.Forms.Containers.GoPanel();
            goInputInt1 = new Going.UI.Forms.Controls.GoInputInt();
            goInputDouble1 = new Going.UI.Forms.Controls.GoInputDouble();
            goInputString2 = new Going.UI.Forms.Controls.GoInputString();
            goInputString1 = new Going.UI.Forms.Controls.GoInputString();
            goButton2 = new Going.UI.Forms.Controls.GoButton();
            goPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // goButton1
            // 
            goButton1.BackColor = Color.FromArgb(60, 60, 60);
            goButton1.BackgroundColor = "Base2";
            goButton1.BackgroundDraw = true;
            goButton1.BorderOnly = false;
            goButton1.ButtonColor = "Base3";
            goButton1.FontName = "나눔고딕";
            goButton1.FontSize = 12F;
            goButton1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goButton1.IconGap = 5F;
            goButton1.IconSize = 12F;
            goButton1.IconString = "fa-check";
            goButton1.Location = new Point(166, 53);
            goButton1.Name = "goButton1";
            goButton1.Round = Going.UI.Enums.GoRoundType.All;
            goButton1.Size = new Size(160, 40);
            goButton1.TabIndex = 0;
            goButton1.TabStop = false;
            goButton1.Text = "확인";
            goButton1.TextColor = "Fore";
            // 
            // goLabel1
            // 
            goLabel1.BackColor = Color.FromArgb(60, 60, 60);
            goLabel1.BackgroundColor = "Base2";
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
            goLabel1.Location = new Point(13, 53);
            goLabel1.Name = "goLabel1";
            goLabel1.Round = Going.UI.Enums.GoRoundType.All;
            goLabel1.Size = new Size(147, 188);
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
            goButtonInfo1.IconString = "fa-plus";
            goButtonInfo1.Name = "add";
            goButtonInfo1.Size = "50%";
            goButtonInfo1.Text = null;
            goButtonInfo2.IconString = "fa-minus";
            goButtonInfo2.Name = "del";
            goButtonInfo2.Size = "50%";
            goButtonInfo2.Text = null;
            goPanel2.Buttons.Add(goButtonInfo1);
            goPanel2.Buttons.Add(goButtonInfo2);
            goPanel2.ButtonWidth = 60F;
            goPanel2.Controls.Add(goInputInt1);
            goPanel2.Controls.Add(goInputDouble1);
            goPanel2.Controls.Add(goInputString2);
            goPanel2.Controls.Add(goInputString1);
            goPanel2.Controls.Add(goButton2);
            goPanel2.Controls.Add(goLabel1);
            goPanel2.Controls.Add(goButton1);
            goPanel2.FontName = "나눔고딕";
            goPanel2.FontSize = 12F;
            goPanel2.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goPanel2.IconGap = 5F;
            goPanel2.IconSize = 12F;
            goPanel2.IconString = "fa-check";
            goPanel2.Location = new Point(60, 35);
            goPanel2.Name = "goPanel2";
            goPanel2.Padding = new Padding(10, 50, 10, 10);
            goPanel2.PanelColor = "Base2";
            goPanel2.Round = Going.UI.Enums.GoRoundType.All;
            goPanel2.Size = new Size(509, 346);
            goPanel2.TabIndex = 2;
            goPanel2.TabStop = false;
            goPanel2.Text = "Panel";
            goPanel2.TextColor = "Fore";
            goPanel2.TitleHeight = 40F;
            // 
            // goInputInt1
            // 
            goInputInt1.BackColor = Color.FromArgb(50, 50, 50);
            goInputInt1.BackgroundColor = "Back";
            goInputInt1.BorderColor = "Base3";
            goButtonInfo3.IconString = "fa-upload";
            goButtonInfo3.Name = "up";
            goButtonInfo3.Size = "50%";
            goButtonInfo3.Text = null;
            goButtonInfo4.IconString = "fa-download";
            goButtonInfo4.Name = "down";
            goButtonInfo4.Size = "50%";
            goButtonInfo4.Text = null;
            goInputInt1.Buttons.Add(goButtonInfo3);
            goInputInt1.Buttons.Add(goButtonInfo4);
            goInputInt1.ButtonSize = 80F;
            goInputInt1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goInputInt1.FillColor = "Base3";
            goInputInt1.FontName = "나눔고딕";
            goInputInt1.FontSize = 12F;
            goInputInt1.IconGap = 5F;
            goInputInt1.IconSize = 12F;
            goInputInt1.IconString = null;
            goInputInt1.Location = new Point(166, 293);
            goInputInt1.Maximum = 100;
            goInputInt1.Minimum = 0;
            goInputInt1.Name = "goInputInt1";
            goInputInt1.Round = Going.UI.Enums.GoRoundType.All;
            goInputInt1.Size = new Size(330, 40);
            goInputInt1.TabIndex = 6;
            goInputInt1.TabStop = false;
            goInputInt1.Text = "정수";
            goInputInt1.TextColor = "Fore";
            goInputInt1.Title = "정수";
            goInputInt1.TitleSize = 100F;
            goInputInt1.Value = 0;
            goInputInt1.ValueColor = "Base1";
            // 
            // goInputDouble1
            // 
            goInputDouble1.BackColor = Color.FromArgb(50, 50, 50);
            goInputDouble1.BackgroundColor = "Back";
            goInputDouble1.BorderColor = "Base3";
            goButtonInfo5.IconString = "fa-upload";
            goButtonInfo5.Name = "up";
            goButtonInfo5.Size = "50%";
            goButtonInfo5.Text = null;
            goButtonInfo6.IconString = "fa-download";
            goButtonInfo6.Name = "down";
            goButtonInfo6.Size = "50%";
            goButtonInfo6.Text = null;
            goInputDouble1.Buttons.Add(goButtonInfo5);
            goInputDouble1.Buttons.Add(goButtonInfo6);
            goInputDouble1.ButtonSize = 80F;
            goInputDouble1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goInputDouble1.FillColor = "Base3";
            goInputDouble1.FontName = "나눔고딕";
            goInputDouble1.FontSize = 12F;
            goInputDouble1.IconGap = 5F;
            goInputDouble1.IconSize = 12F;
            goInputDouble1.IconString = null;
            goInputDouble1.Location = new Point(166, 247);
            goInputDouble1.Maximum = 500D;
            goInputDouble1.Minimum = -20D;
            goInputDouble1.Name = "goInputDouble1";
            goInputDouble1.Round = Going.UI.Enums.GoRoundType.All;
            goInputDouble1.Size = new Size(330, 40);
            goInputDouble1.TabIndex = 5;
            goInputDouble1.TabStop = false;
            goInputDouble1.Text = "goInputDouble1";
            goInputDouble1.TextColor = "Fore";
            goInputDouble1.Title = "실수";
            goInputDouble1.TitleSize = 100F;
            goInputDouble1.Value = 0D;
            goInputDouble1.ValueColor = "Base1";
            // 
            // goInputString2
            // 
            goInputString2.BackColor = Color.FromArgb(60, 60, 60);
            goInputString2.BackgroundColor = "Base2";
            goInputString2.BorderColor = "Base3";
            goButtonInfo7.IconString = "fa-upload";
            goButtonInfo7.Name = "up";
            goButtonInfo7.Size = "50%";
            goButtonInfo7.Text = null;
            goButtonInfo8.IconString = "fa-download";
            goButtonInfo8.Name = "down";
            goButtonInfo8.Size = "50%";
            goButtonInfo8.Text = null;
            goInputString2.Buttons.Add(goButtonInfo7);
            goInputString2.Buttons.Add(goButtonInfo8);
            goInputString2.ButtonSize = 80F;
            goInputString2.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goInputString2.FillColor = "Base3";
            goInputString2.FontName = "나눔고딕";
            goInputString2.FontSize = 12F;
            goInputString2.IconGap = 5F;
            goInputString2.IconSize = 12F;
            goInputString2.IconString = null;
            goInputString2.Location = new Point(166, 201);
            goInputString2.Name = "goInputString2";
            goInputString2.Round = Going.UI.Enums.GoRoundType.All;
            goInputString2.Size = new Size(330, 40);
            goInputString2.TabIndex = 4;
            goInputString2.TabStop = false;
            goInputString2.Text = "goInputString2";
            goInputString2.TextColor = "Fore";
            goInputString2.Title = "테스트 2";
            goInputString2.TitleSize = 100F;
            goInputString2.Value = "";
            goInputString2.ValueColor = "Base1";
            // 
            // goInputString1
            // 
            goInputString1.BackColor = Color.FromArgb(60, 60, 60);
            goInputString1.BackgroundColor = "Base2";
            goInputString1.BorderColor = "Base3";
            goButtonInfo9.IconString = "fa-upload";
            goButtonInfo9.Name = "up";
            goButtonInfo9.Size = "50%";
            goButtonInfo9.Text = null;
            goButtonInfo10.IconString = "fa-download";
            goButtonInfo10.Name = "down";
            goButtonInfo10.Size = "50%";
            goButtonInfo10.Text = null;
            goInputString1.Buttons.Add(goButtonInfo9);
            goInputString1.Buttons.Add(goButtonInfo10);
            goInputString1.ButtonSize = 80F;
            goInputString1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goInputString1.FillColor = "Base3";
            goInputString1.FontName = "나눔고딕";
            goInputString1.FontSize = 12F;
            goInputString1.IconGap = 5F;
            goInputString1.IconSize = 12F;
            goInputString1.IconString = null;
            goInputString1.Location = new Point(166, 155);
            goInputString1.Name = "goInputString1";
            goInputString1.Round = Going.UI.Enums.GoRoundType.All;
            goInputString1.Size = new Size(330, 40);
            goInputString1.TabIndex = 3;
            goInputString1.TabStop = false;
            goInputString1.Text = "goInputString1";
            goInputString1.TextColor = "Fore";
            goInputString1.Title = "테스트 1";
            goInputString1.TitleSize = 100F;
            goInputString1.Value = "";
            goInputString1.ValueColor = "Base1";
            // 
            // goButton2
            // 
            goButton2.BackColor = Color.FromArgb(60, 60, 60);
            goButton2.BackgroundColor = "Base2";
            goButton2.BackgroundDraw = true;
            goButton2.BorderOnly = false;
            goButton2.ButtonColor = "danger";
            goButton2.FontName = "나눔고딕";
            goButton2.FontSize = 12F;
            goButton2.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goButton2.IconGap = 5F;
            goButton2.IconSize = 12F;
            goButton2.IconString = "fa-ban";
            goButton2.Location = new Point(166, 99);
            goButton2.Name = "goButton2";
            goButton2.Round = Going.UI.Enums.GoRoundType.All;
            goButton2.Size = new Size(160, 40);
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
        private Going.UI.Forms.Controls.GoInputString goInputString1;
        private Going.UI.Forms.Controls.GoInputString goInputString2;
        private Going.UI.Forms.Controls.GoInputDouble goInputDouble1;
        private Going.UI.Forms.Controls.GoInputInt goInputInt1;
    }
}
