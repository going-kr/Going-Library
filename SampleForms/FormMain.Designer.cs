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
            Going.UI.Collections.ObservableList<Going.UI.Datas.GoToolCategory> observableList_11 = new Going.UI.Collections.ObservableList<Going.UI.Datas.GoToolCategory>();
            Going.UI.Collections.ObservableList<Going.UI.Datas.GoTreeNode> observableList_12 = new Going.UI.Collections.ObservableList<Going.UI.Datas.GoTreeNode>();
            goPanel1 = new Going.UI.Forms.Containers.GoPanel();
            goButton2 = new Going.UI.Forms.Controls.GoButton();
            goButton1 = new Going.UI.Forms.Controls.GoButton();
            goLabel1 = new Going.UI.Forms.Controls.GoLabel();
            goTabControl1 = new Going.UI.Forms.Containers.GoTabControl();
            tabPage1 = new TabPage();
            goCalendar1 = new Going.UI.Forms.Controls.GoCalendar();
            tabPage2 = new TabPage();
            goLabel2 = new Going.UI.Forms.Controls.GoLabel();
            goToolBox1 = new Going.UI.Forms.Controls.GoToolBox();
            rm = new Going.UI.Forms.Components.GoResourceManager();
            goPictureBox1 = new Going.UI.Forms.Controls.GoPictureBox();
            goAnimate1 = new Going.UI.Forms.Controls.GoAnimate();
            tabPage3 = new TabPage();
            goTreeView1 = new Going.UI.Forms.Controls.GoTreeView();
            goPanel1.SuspendLayout();
            goTabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
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
            goPanel1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
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
            goButton2.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
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
            goButton1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
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
            goLabel1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
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
            goTabControl1.Controls.Add(tabPage3);
            goTabControl1.FontName = "나눔고딕";
            goTabControl1.FontSize = 12F;
            goTabControl1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
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
            tabPage1.Controls.Add(goCalendar1);
            tabPage1.Location = new Point(4, 44);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(10);
            tabPage1.Size = new Size(552, 378);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            // 
            // goCalendar1
            // 
            goCalendar1.BackColor = Color.FromArgb(50, 50, 50);
            goCalendar1.BackgroundColor = "Back";
            goCalendar1.BackgroundDraw = true;
            goCalendar1.BoxColor = "Base3";
            goCalendar1.FontName = "나눔고딕";
            goCalendar1.FontSize = 12F;
            goCalendar1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            goCalendar1.Location = new Point(13, 13);
            goCalendar1.MultiSelect = true;
            goCalendar1.Name = "goCalendar1";
            goCalendar1.NoneSelect = false;
            goCalendar1.Round = Going.UI.Enums.GoRoundType.All;
            goCalendar1.SelectColor = "Select";
            goCalendar1.Size = new Size(288, 243);
            goCalendar1.TabIndex = 0;
            goCalendar1.TabStop = false;
            goCalendar1.Text = "goCalendar1";
            goCalendar1.TextColor = "Fore";
            // 
            // tabPage2
            // 
            tabPage2.BackColor = Color.FromArgb(60, 60, 60);
            tabPage2.Controls.Add(goLabel2);
            tabPage2.Controls.Add(goToolBox1);
            tabPage2.Location = new Point(4, 44);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(10);
            tabPage2.Size = new Size(552, 378);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            // 
            // goLabel2
            // 
            goLabel2.BackColor = Color.FromArgb(60, 60, 60);
            goLabel2.BackgroundColor = "base2";
            goLabel2.BackgroundDraw = true;
            goLabel2.BorderOnly = true;
            goLabel2.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            goLabel2.FontName = "나눔고딕";
            goLabel2.FontSize = 12F;
            goLabel2.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            goLabel2.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goLabel2.IconGap = 5F;
            goLabel2.IconSize = 12F;
            goLabel2.IconString = null;
            goLabel2.LabelColor = "Base3";
            goLabel2.Location = new Point(13, 304);
            goLabel2.Name = "goLabel2";
            goLabel2.Round = Going.UI.Enums.GoRoundType.All;
            goLabel2.Size = new Size(283, 40);
            goLabel2.TabIndex = 1;
            goLabel2.TabStop = false;
            goLabel2.Text = "goLabel2";
            goLabel2.TextColor = "Fore";
            goLabel2.TextPadding.Bottom = 0F;
            goLabel2.TextPadding.Left = 0F;
            goLabel2.TextPadding.Right = 0F;
            goLabel2.TextPadding.Top = 0F;
            // 
            // goToolBox1
            // 
            goToolBox1.BackColor = Color.FromArgb(60, 60, 60);
            goToolBox1.BackgroundColor = "Base2";
            goToolBox1.BackgroundDraw = true;
            goToolBox1.BorderColor = "Base3";
            goToolBox1.BoxColor = "Base1";
            observableList_11.Changed = false;
            goToolBox1.Categories = observableList_11;
            goToolBox1.CategoryColor = "Base2";
            goToolBox1.FontName = "나눔고딕";
            goToolBox1.FontSize = 12F;
            goToolBox1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            goToolBox1.IconGap = 5F;
            goToolBox1.IconSize = 12F;
            goToolBox1.ItemHeight = 30F;
            goToolBox1.Location = new Point(13, 13);
            goToolBox1.Name = "goToolBox1";
            goToolBox1.Round = Going.UI.Enums.GoRoundType.All;
            goToolBox1.SelectColor = "Select";
            goToolBox1.Size = new Size(287, 285);
            goToolBox1.TabIndex = 0;
            goToolBox1.TabStop = false;
            goToolBox1.Text = "goToolBox1";
            goToolBox1.TextColor = "Fore";
            // 
            // rm
            // 
            rm.ImageFolder = "D:\\Project\\Going\\library\\src\\Going\\ImageSample";
            // 
            // goPictureBox1
            // 
            goPictureBox1.BackColor = Color.FromArgb(50, 50, 50);
            goPictureBox1.BackgroundColor = "Back";
            goPictureBox1.Image = "nature";
            goPictureBox1.Location = new Point(12, 444);
            goPictureBox1.Name = "goPictureBox1";
            goPictureBox1.Resources = rm;
            goPictureBox1.Round = Going.UI.Enums.GoRoundType.Rect;
            goPictureBox1.ScaleMode = Going.UI.Enums.GoImageScaleMode.Strech;
            goPictureBox1.Size = new Size(331, 209);
            goPictureBox1.TabIndex = 2;
            goPictureBox1.TabStop = false;
            goPictureBox1.Text = "goPictureBox1";
            // 
            // goAnimate1
            // 
            goAnimate1.BackColor = Color.FromArgb(50, 50, 50);
            goAnimate1.BackgroundColor = "Back";
            goAnimate1.Location = new Point(349, 444);
            goAnimate1.Name = "goAnimate1";
            goAnimate1.OffImage = "ani_off";
            goAnimate1.OnImage = "ani";
            goAnimate1.OnOff = false;
            goAnimate1.Resources = rm;
            goAnimate1.Round = Going.UI.Enums.GoRoundType.Rect;
            goAnimate1.ScaleMode = Going.UI.Enums.GoImageScaleMode.Zoom;
            goAnimate1.Size = new Size(209, 209);
            goAnimate1.TabIndex = 3;
            goAnimate1.TabStop = false;
            goAnimate1.Text = "goAnimate1";
            goAnimate1.Time = 30;
            // 
            // tabPage3
            // 
            tabPage3.BackColor = Color.FromArgb(60, 60, 60);
            tabPage3.Controls.Add(goTreeView1);
            tabPage3.Location = new Point(4, 44);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(10);
            tabPage3.Size = new Size(552, 378);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "tabPage3";
            // 
            // goTreeView1
            // 
            goTreeView1.BackColor = Color.FromArgb(60, 60, 60);
            goTreeView1.BackgroundColor = "base2";
            goTreeView1.BackgroundDraw = true;
            goTreeView1.BorderColor = "Base3";
            goTreeView1.BoxColor = "Base1";
            goTreeView1.DragMode = false;
            goTreeView1.FontName = "나눔고딕";
            goTreeView1.FontSize = 12F;
            goTreeView1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            goTreeView1.IconGap = 5F;
            goTreeView1.IconSize = 12F;
            goTreeView1.ItemHeight = 30F;
            goTreeView1.Location = new Point(13, 13);
            goTreeView1.Name = "goTreeView1";
            observableList_12.Changed = false;
            goTreeView1.Nodes = observableList_12;
            goTreeView1.Round = Going.UI.Enums.GoRoundType.All;
            goTreeView1.SelectColor = "Select";
            goTreeView1.SelectionMode = Going.UI.Enums.GoItemSelectionMode.SIngle;
            goTreeView1.Size = new Size(292, 283);
            goTreeView1.TabIndex = 0;
            goTreeView1.TabStop = false;
            goTreeView1.Text = "goTreeView1";
            goTreeView1.TextColor = "Fore";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(993, 665);
            Controls.Add(goAnimate1);
            Controls.Add(goPictureBox1);
            Controls.Add(goTabControl1);
            Controls.Add(goPanel1);
            Name = "FormMain";
            Text = "FormMain2";
            Title = "FormMain2";
            goPanel1.ResumeLayout(false);
            goTabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
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
        private Going.UI.Forms.Components.GoResourceManager rm;
        private Going.UI.Forms.Controls.GoPictureBox goPictureBox1;
        private Going.UI.Forms.Controls.GoAnimate goAnimate1;
        private Going.UI.Forms.Controls.GoCalendar goCalendar1;
        private Going.UI.Forms.Controls.GoToolBox goToolBox1;
        private Going.UI.Forms.Controls.GoLabel goLabel2;
        private TabPage tabPage3;
        private Going.UI.Forms.Controls.GoTreeView goTreeView1;
    }
}