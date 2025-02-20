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
            Going.UI.Datas.GoButtonInfo goButtonInfo11 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo12 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo13 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo14 = new Going.UI.Datas.GoButtonInfo();
            Going.UI.Datas.GoButtonInfo goButtonInfo15 = new Going.UI.Datas.GoButtonInfo();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            goButton1 = new Going.UI.Forms.Controls.GoButton();
            goLabel1 = new Going.UI.Forms.Controls.GoLabel();
            goPanel2 = new Going.UI.Forms.Containers.GoPanel();
            goValueBoolean1 = new Going.UI.Forms.Controls.GoValueBoolean();
            goValueInteger1 = new Going.UI.Forms.Controls.GoValueInteger();
            goValueString1 = new Going.UI.Forms.Controls.GoValueString();
            goInputBoolean1 = new Going.UI.Forms.Controls.GoInputBoolean();
            goInputInt1 = new Going.UI.Forms.Controls.GoInputInteger();
            goInputDouble1 = new Going.UI.Forms.Controls.GoInputFloat();
            goInputString2 = new Going.UI.Forms.Controls.GoInputString();
            goInputString1 = new Going.UI.Forms.Controls.GoInputString();
            goButton2 = new Going.UI.Forms.Controls.GoButton();
            goListBox1 = new Going.UI.Forms.Controls.GoListBox();
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
            goPanel2.Controls.Add(goValueBoolean1);
            goPanel2.Controls.Add(goValueInteger1);
            goPanel2.Controls.Add(goValueString1);
            goPanel2.Controls.Add(goInputBoolean1);
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
            goPanel2.Location = new Point(12, 12);
            goPanel2.Name = "goPanel2";
            goPanel2.Padding = new Padding(10, 50, 10, 10);
            goPanel2.PanelColor = "Base2";
            goPanel2.Round = Going.UI.Enums.GoRoundType.All;
            goPanel2.Size = new Size(509, 530);
            goPanel2.TabIndex = 2;
            goPanel2.TabStop = false;
            goPanel2.Text = "Panel";
            goPanel2.TextColor = "Fore";
            goPanel2.TitleHeight = 40F;
            // 
            // goValueBoolean1
            // 
            goValueBoolean1.BackColor = Color.FromArgb(50, 50, 50);
            goValueBoolean1.BackgroundColor = "Back";
            goValueBoolean1.BorderColor = "Base3";
            goButtonInfo3.IconString = "fa-check";
            goButtonInfo3.Name = "chk";
            goButtonInfo3.Size = "100%";
            goButtonInfo3.Text = null;
            goValueBoolean1.Buttons.Add(goButtonInfo3);
            goValueBoolean1.ButtonSize = 40F;
            goValueBoolean1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goValueBoolean1.FillColor = "Base3";
            goValueBoolean1.FontName = "나눔고딕";
            goValueBoolean1.FontSize = 12F;
            goValueBoolean1.IconGap = 5F;
            goValueBoolean1.IconSize = 12F;
            goValueBoolean1.IconString = null;
            goValueBoolean1.Location = new Point(166, 477);
            goValueBoolean1.Name = "goValueBoolean1";
            goValueBoolean1.Round = Going.UI.Enums.GoRoundType.All;
            goValueBoolean1.Size = new Size(330, 40);
            goValueBoolean1.TabIndex = 9;
            goValueBoolean1.TabStop = false;
            goValueBoolean1.Text = "goValueBoolean1";
            goValueBoolean1.TextColor = "Fore";
            goValueBoolean1.Title = "상태";
            goValueBoolean1.TitleSize = 100F;
            goValueBoolean1.Value = false;
            goValueBoolean1.ValueColor = "Base2";
            // 
            // goValueInteger1
            // 
            goValueInteger1.BackColor = Color.FromArgb(50, 50, 50);
            goValueInteger1.BackgroundColor = "Back";
            goValueInteger1.BorderColor = "Base3";
            goButtonInfo4.IconString = "fa-check";
            goButtonInfo4.Name = "chk";
            goButtonInfo4.Size = "100%";
            goButtonInfo4.Text = null;
            goValueInteger1.Buttons.Add(goButtonInfo4);
            goValueInteger1.ButtonSize = 40F;
            goValueInteger1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goValueInteger1.FillColor = "Base3";
            goValueInteger1.FontName = "나눔고딕";
            goValueInteger1.FontSize = 12F;
            goValueInteger1.FormatString = null;
            goValueInteger1.IconGap = 5F;
            goValueInteger1.IconSize = 12F;
            goValueInteger1.IconString = null;
            goValueInteger1.Location = new Point(166, 431);
            goValueInteger1.Name = "goValueInteger1";
            goValueInteger1.Round = Going.UI.Enums.GoRoundType.All;
            goValueInteger1.Size = new Size(330, 40);
            goValueInteger1.TabIndex = 4;
            goValueInteger1.TabStop = false;
            goValueInteger1.Text = "goValueInteger1";
            goValueInteger1.TextColor = "Fore";
            goValueInteger1.Title = "정수";
            goValueInteger1.TitleSize = 100F;
            goValueInteger1.Unit = null;
            goValueInteger1.UnitSize = null;
            goValueInteger1.Value = 0;
            goValueInteger1.ValueColor = "Base2";
            // 
            // goValueString1
            // 
            goValueString1.BackColor = Color.FromArgb(50, 50, 50);
            goValueString1.BackgroundColor = "Back";
            goValueString1.BorderColor = "Base3";
            goButtonInfo5.IconString = "fa-check";
            goButtonInfo5.Name = "chk";
            goButtonInfo5.Size = "100%";
            goButtonInfo5.Text = null;
            goValueString1.Buttons.Add(goButtonInfo5);
            goValueString1.ButtonSize = 40F;
            goValueString1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goValueString1.FillColor = "Base3";
            goValueString1.FontName = "나눔고딕";
            goValueString1.FontSize = 12F;
            goValueString1.IconGap = 5F;
            goValueString1.IconSize = 12F;
            goValueString1.IconString = null;
            goValueString1.Location = new Point(166, 385);
            goValueString1.Name = "goValueString1";
            goValueString1.Round = Going.UI.Enums.GoRoundType.All;
            goValueString1.Size = new Size(330, 40);
            goValueString1.TabIndex = 8;
            goValueString1.TabStop = false;
            goValueString1.Text = "goValueString1";
            goValueString1.TextColor = "Fore";
            goValueString1.Title = "문자열";
            goValueString1.TitleSize = 100F;
            goValueString1.Value = null;
            goValueString1.ValueColor = "Base2";
            // 
            // goInputBoolean1
            // 
            goInputBoolean1.BackColor = Color.FromArgb(50, 50, 50);
            goInputBoolean1.BackgroundColor = "Back";
            goInputBoolean1.BorderColor = "Base3";
            goButtonInfo6.IconString = "fa-upload";
            goButtonInfo6.Name = "up";
            goButtonInfo6.Size = "50%";
            goButtonInfo6.Text = null;
            goButtonInfo7.IconString = "fa-download";
            goButtonInfo7.Name = "down";
            goButtonInfo7.Size = "50%";
            goButtonInfo7.Text = null;
            goInputBoolean1.Buttons.Add(goButtonInfo6);
            goInputBoolean1.Buttons.Add(goButtonInfo7);
            goInputBoolean1.ButtonSize = 80F;
            goInputBoolean1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goInputBoolean1.FillColor = "Base3";
            goInputBoolean1.FontName = "나눔고딕";
            goInputBoolean1.FontSize = 12F;
            goInputBoolean1.IconGap = 5F;
            goInputBoolean1.IconSize = 12F;
            goInputBoolean1.IconString = null;
            goInputBoolean1.Location = new Point(166, 339);
            goInputBoolean1.Name = "goInputBoolean1";
            goInputBoolean1.Round = Going.UI.Enums.GoRoundType.All;
            goInputBoolean1.Size = new Size(330, 40);
            goInputBoolean1.TabIndex = 7;
            goInputBoolean1.TabStop = false;
            goInputBoolean1.Text = "goInputBoolean1";
            goInputBoolean1.TextColor = "Fore";
            goInputBoolean1.Title = "스위치";
            goInputBoolean1.TitleSize = 100F;
            goInputBoolean1.Value = false;
            goInputBoolean1.ValueColor = "Base1";
            // 
            // goInputInt1
            // 
            goInputInt1.BackColor = Color.FromArgb(50, 50, 50);
            goInputInt1.BackgroundColor = "Back";
            goInputInt1.BorderColor = "Base3";
            goButtonInfo8.IconString = "fa-upload";
            goButtonInfo8.Name = "up";
            goButtonInfo8.Size = "50%";
            goButtonInfo8.Text = null;
            goButtonInfo9.IconString = "fa-download";
            goButtonInfo9.Name = "down";
            goButtonInfo9.Size = "50%";
            goButtonInfo9.Text = null;
            goInputInt1.Buttons.Add(goButtonInfo8);
            goInputInt1.Buttons.Add(goButtonInfo9);
            goInputInt1.ButtonSize = 80F;
            goInputInt1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goInputInt1.FillColor = "Base3";
            goInputInt1.FontName = "나눔고딕";
            goInputInt1.FontSize = 12F;
            goInputInt1.FormatString = null;
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
            goInputInt1.Unit = "mm";
            goInputInt1.UnitSize = 40F;
            goInputInt1.Value = 0;
            goInputInt1.ValueColor = "Base1";
            // 
            // goInputDouble1
            // 
            goInputDouble1.BackColor = Color.FromArgb(50, 50, 50);
            goInputDouble1.BackgroundColor = "Back";
            goInputDouble1.BorderColor = "Base3";
            goButtonInfo10.IconString = "fa-upload";
            goButtonInfo10.Name = "up";
            goButtonInfo10.Size = "50%";
            goButtonInfo10.Text = null;
            goButtonInfo11.IconString = "fa-download";
            goButtonInfo11.Name = "down";
            goButtonInfo11.Size = "50%";
            goButtonInfo11.Text = null;
            goInputDouble1.Buttons.Add(goButtonInfo10);
            goInputDouble1.Buttons.Add(goButtonInfo11);
            goInputDouble1.ButtonSize = 80F;
            goInputDouble1.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
            goInputDouble1.FillColor = "Base3";
            goInputDouble1.FontName = "나눔고딕";
            goInputDouble1.FontSize = 12F;
            goInputDouble1.FormatString = null;
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
            goInputDouble1.Unit = null;
            goInputDouble1.UnitSize = null;
            goInputDouble1.Value = 0D;
            goInputDouble1.ValueColor = "Base1";
            // 
            // goInputString2
            // 
            goInputString2.BackColor = Color.FromArgb(60, 60, 60);
            goInputString2.BackgroundColor = "Base2";
            goInputString2.BorderColor = "Base3";
            goButtonInfo12.IconString = "fa-upload";
            goButtonInfo12.Name = "up";
            goButtonInfo12.Size = "50%";
            goButtonInfo12.Text = null;
            goButtonInfo13.IconString = "fa-download";
            goButtonInfo13.Name = "down";
            goButtonInfo13.Size = "50%";
            goButtonInfo13.Text = null;
            goInputString2.Buttons.Add(goButtonInfo12);
            goInputString2.Buttons.Add(goButtonInfo13);
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
            goButtonInfo14.IconString = "fa-upload";
            goButtonInfo14.Name = "up";
            goButtonInfo14.Size = "50%";
            goButtonInfo14.Text = null;
            goButtonInfo15.IconString = "fa-download";
            goButtonInfo15.Name = "down";
            goButtonInfo15.Size = "50%";
            goButtonInfo15.Text = null;
            goInputString1.Buttons.Add(goButtonInfo14);
            goInputString1.Buttons.Add(goButtonInfo15);
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
            // goListBox1
            // 
            goListBox1.BackColor = Color.FromArgb(50, 50, 50);
            goListBox1.BackgroundColor = "Back";
            goListBox1.BackgroundDraw = true;
            goListBox1.BorderColor = "Base3";
            goListBox1.BoxColor = "Base1";
            goListBox1.FontName = "나눔고딕";
            goListBox1.FontSize = 12F;
            goListBox1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goListBox1.IconGap = 5F;
            goListBox1.IconSize = 12F;
            goListBox1.ItemAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            goListBox1.ItemHeight = 30F;
            goListBox1.Location = new Point(527, 12);
            goListBox1.Name = "goListBox1";
            goListBox1.Round = Going.UI.Enums.GoRoundType.All;
            goListBox1.SelectedColor = "Select";
            goListBox1.SelectionMode = Going.UI.Enums.GoItemSelectionMode.SIngle;
            goListBox1.Size = new Size(277, 258);
            goListBox1.TabIndex = 3;
            goListBox1.TabStop = false;
            goListBox1.Text = "goListBox1";
            goListBox1.TextColor = "Fore";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(50, 50, 50);
            ClientSize = new Size(1051, 555);
            Controls.Add(goListBox1);
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
        private Going.UI.Forms.Controls.GoInputFloat goInputDouble1;
        private Going.UI.Forms.Controls.GoInputInteger goInputInt1;
        private Going.UI.Forms.Controls.GoListBox goListBox1;
        private Going.UI.Forms.Controls.GoInputBoolean goInputBoolean1;
        private Going.UI.Forms.Controls.GoValueString goValueString1;
        private Going.UI.Forms.Controls.GoValueInteger goValueInteger1;
        private Going.UI.Forms.Controls.GoValueBoolean goValueBoolean1;
    }
}
