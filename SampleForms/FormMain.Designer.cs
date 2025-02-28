namespace SampleForms
{
    partial class FormMain
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
            goPanel1 = new Going.UI.Forms.Containers.GoPanel();
            goButton2 = new Going.UI.Forms.Controls.GoButton();
            goButton1 = new Going.UI.Forms.Controls.GoButton();
            goLabel1 = new Going.UI.Forms.Controls.GoLabel();
            goTabControl1 = new Going.UI.Forms.Containers.GoTabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            goPanel1.SuspendLayout();
            goTabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // goPanel1
            // 
            goPanel1.BackColor = Color.FromArgb(50, 50, 50);
            goPanel1.BackgroundColor = "Back";
            goPanel1.BackgroundDraw = true;
            goPanel1.BorderOnly = false;
            goPanel1.ButtonWidth = null;
            goPanel1.Controls.Add(goButton2);
            goPanel1.Controls.Add(goButton1);
            goPanel1.Controls.Add(goLabel1);
            goPanel1.FontName = "나눔고딕";
            goPanel1.FontSize = 12F;
            goPanel1.IconGap = 5F;
            goPanel1.IconSize = 12F;
            goPanel1.IconString = null;
            goPanel1.Location = new Point(12, 12);
            goPanel1.Name = "goPanel1";
            goPanel1.Padding = new Padding(10, 50, 10, 10);
            goPanel1.PanelColor = "Base2";
            goPanel1.Round = Going.UI.Enums.GoRoundType.All;
            goPanel1.Size = new Size(403, 426);
            goPanel1.TabIndex = 0;
            goPanel1.TabStop = false;
            goPanel1.Text = "goPanel1";
            goPanel1.TextColor = "Fore";
            goPanel1.TitleHeight = 40F;
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
            goButton2.IconString = "fa-bell";
            goButton2.Location = new Point(259, 53);
            goButton2.Name = "goButton2";
            goButton2.Round = Going.UI.Enums.GoRoundType.All;
            goButton2.Size = new Size(100, 40);
            goButton2.TabIndex = 2;
            goButton2.TabStop = false;
            goButton2.Text = "버튼 2";
            goButton2.TextColor = "Fore";
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
            goButton1.Location = new Point(153, 53);
            goButton1.Name = "goButton1";
            goButton1.Round = Going.UI.Enums.GoRoundType.All;
            goButton1.Size = new Size(100, 40);
            goButton1.TabIndex = 1;
            goButton1.TabStop = false;
            goButton1.Text = "버튼 1";
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
            goLabel1.LabelColor = "base3";
            goLabel1.Location = new Point(13, 53);
            goLabel1.Name = "goLabel1";
            goLabel1.Round = Going.UI.Enums.GoRoundType.All;
            goLabel1.Size = new Size(134, 145);
            goLabel1.TabIndex = 0;
            goLabel1.TabStop = false;
            goLabel1.Text = "동해물과 백두산이 마르고 닳도록\r\n하느님이 보우하사 우리 나라 만세\r\n무궁화 삼천리 화려강산\r\n대한사람 대한으로 길이 보전하세";
            goLabel1.TextColor = "Fore";
            goLabel1.TextPadding.Bottom = 10F;
            goLabel1.TextPadding.Left = 10F;
            goLabel1.TextPadding.Right = 10F;
            goLabel1.TextPadding.Top = 10F;
            // 
            // goTabControl1
            // 
            goTabControl1.BackgroundColor = "Back";
            goTabControl1.Controls.Add(tabPage1);
            goTabControl1.Controls.Add(tabPage2);
            goTabControl1.FontName = "나눔고딕";
            goTabControl1.FontSize = 12F;
            goTabControl1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goTabControl1.IconGap = 5F;
            goTabControl1.IconSize = 12F;
            goTabControl1.ItemSize = new Size(120, 40);
            goTabControl1.Location = new Point(421, 12);
            goTabControl1.Name = "goTabControl1";
            goTabControl1.SelectedIndex = 0;
            goTabControl1.Size = new Size(560, 426);
            goTabControl1.SizeMode = TabSizeMode.Fixed;
            goTabControl1.TabBorderColor = "Base3";
            goTabControl1.TabColor = "Base2";
            goTabControl1.TabIndex = 1;
            goTabControl1.TextColor = "Fore";
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.FromArgb(60, 60, 60);
            tabPage1.Location = new Point(4, 44);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(552, 378);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            // 
            // tabPage2
            // 
            tabPage2.BackColor = Color.FromArgb(60, 60, 60);
            tabPage2.Location = new Point(4, 44);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(552, 378);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(993, 665);
            Controls.Add(goTabControl1);
            Controls.Add(goPanel1);
            Name = "FormMain";
            Text = "FormMain2";
            Title = "FormMain2";
            goPanel1.ResumeLayout(false);
            goTabControl1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Going.UI.Forms.Containers.GoPanel goPanel1;
        private Going.UI.Forms.Controls.GoButton goButton2;
        private Going.UI.Forms.Controls.GoButton goButton1;
        private Going.UI.Forms.Controls.GoLabel goLabel1;
        private Going.UI.Forms.Containers.GoTabControl goTabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
    }
}