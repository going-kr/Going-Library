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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            rm = new Going.UI.Forms.Components.GoResourceManager();
            goTabControl1 = new Going.UI.Forms.Containers.GoTabControl();
            tpBar = new TabPage();
            grpBar = new Going.UI.Forms.Controls.GoBarGraph();
            tpLine = new TabPage();
            grpLine = new Going.UI.Forms.Controls.GoLineGraph();
            tpCircle = new TabPage();
            grpCircle = new Going.UI.Forms.Controls.GoCircleGraph();
            tpTime = new TabPage();
            grpTime = new Going.UI.Forms.Controls.GoTimeGraph();
            tpTrend = new TabPage();
            grpTrend = new Going.UI.Forms.Controls.GoTrendGraph();
            goButton1 = new Going.UI.Forms.Controls.GoButton();
            goTabControl1.SuspendLayout();
            tpBar.SuspendLayout();
            tpLine.SuspendLayout();
            tpCircle.SuspendLayout();
            tpTime.SuspendLayout();
            tpTrend.SuspendLayout();
            SuspendLayout();
            // 
            // rm
            // 
            rm.ImageFolder = "D:\\Project\\Going\\library\\src\\Going\\ImageSample";
            // 
            // goTabControl1
            // 
            goTabControl1.BackgroundColor = "Back";
            goTabControl1.Controls.Add(tpBar);
            goTabControl1.Controls.Add(tpLine);
            goTabControl1.Controls.Add(tpCircle);
            goTabControl1.Controls.Add(tpTime);
            goTabControl1.Controls.Add(tpTrend);
            goTabControl1.Dock = DockStyle.Top;
            goTabControl1.FontName = "나눔고딕";
            goTabControl1.FontSize = 12F;
            goTabControl1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            goTabControl1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goTabControl1.IconGap = 5F;
            goTabControl1.IconSize = 12F;
            goTabControl1.ItemSize = new Size(120, 40);
            goTabControl1.Location = new Point(10, 10);
            goTabControl1.Name = "goTabControl1";
            goTabControl1.SelectedIndex = 0;
            goTabControl1.Size = new Size(973, 405);
            goTabControl1.SizeMode = TabSizeMode.Fixed;
            goTabControl1.TabBorderColor = "Base3";
            goTabControl1.TabColor = "Base2";
            goTabControl1.TabIndex = 0;
            goTabControl1.TextColor = "Fore";
            // 
            // tpBar
            // 
            tpBar.BackColor = Color.FromArgb(60, 60, 60);
            tpBar.Controls.Add(grpBar);
            tpBar.Location = new Point(4, 44);
            tpBar.Name = "tpBar";
            tpBar.Padding = new Padding(10);
            tpBar.Size = new Size(965, 357);
            tpBar.TabIndex = 0;
            tpBar.Text = "Bar";
            // 
            // grpBar
            // 
            grpBar.BackColor = Color.FromArgb(60, 60, 60);
            grpBar.BackgroundColor = "Base2";
            grpBar.BarGap = 20;
            grpBar.BarSize = 20;
            grpBar.Direction = Going.UI.Enums.GoDirectionHV.Vertical;
            grpBar.Dock = DockStyle.Fill;
            grpBar.FontName = "나눔고딕";
            grpBar.FontSize = 12F;
            grpBar.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            grpBar.FormatString = null;
            grpBar.GraduationCount = 10;
            grpBar.GraphColor = "Back";
            grpBar.GridColor = "Base3";
            grpBar.Location = new Point(10, 10);
            grpBar.Maximum = 10000D;
            grpBar.Minimum = 0D;
            grpBar.Mode = Going.UI.Datas.GoBarGraphMode.Stack;
            grpBar.Name = "grpBar";
            grpBar.RemarkColor = "Base2";
            grpBar.Size = new Size(945, 337);
            grpBar.TabIndex = 0;
            grpBar.TabStop = false;
            grpBar.Text = "goBarGraph1";
            grpBar.TextColor = "Fore";
            // 
            // tpLine
            // 
            tpLine.BackColor = Color.FromArgb(60, 60, 60);
            tpLine.Controls.Add(grpLine);
            tpLine.Location = new Point(4, 44);
            tpLine.Name = "tpLine";
            tpLine.Padding = new Padding(10);
            tpLine.Size = new Size(965, 597);
            tpLine.TabIndex = 1;
            tpLine.Text = "Line";
            // 
            // grpLine
            // 
            grpLine.BackColor = Color.FromArgb(60, 60, 60);
            grpLine.BackgroundColor = "Base2";
            grpLine.Dock = DockStyle.Fill;
            grpLine.FontName = "나눔고딕";
            grpLine.FontSize = 12F;
            grpLine.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            grpLine.FormatString = null;
            grpLine.GraduationCount = 10;
            grpLine.GraphColor = "Back";
            grpLine.GridColor = "Base3";
            grpLine.Location = new Point(10, 10);
            grpLine.Name = "grpLine";
            grpLine.PointWidth = 70;
            grpLine.RemarkColor = "Base2";
            grpLine.Size = new Size(945, 577);
            grpLine.TabIndex = 0;
            grpLine.TabStop = false;
            grpLine.Text = "goLineGraph1";
            grpLine.TextColor = "Fore";
            // 
            // tpCircle
            // 
            tpCircle.BackColor = Color.FromArgb(60, 60, 60);
            tpCircle.Controls.Add(grpCircle);
            tpCircle.Location = new Point(4, 44);
            tpCircle.Name = "tpCircle";
            tpCircle.Padding = new Padding(10);
            tpCircle.Size = new Size(965, 597);
            tpCircle.TabIndex = 2;
            tpCircle.Text = "Circle";
            // 
            // grpCircle
            // 
            grpCircle.BackColor = Color.FromArgb(60, 60, 60);
            grpCircle.BackgroundColor = "Base2";
            grpCircle.Dock = DockStyle.Fill;
            grpCircle.FontName = "나눔고딕";
            grpCircle.FontSize = 12F;
            grpCircle.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            grpCircle.GridColor = "Base3";
            grpCircle.Location = new Point(10, 10);
            grpCircle.Name = "grpCircle";
            grpCircle.RemarkColor = "Base2";
            grpCircle.Size = new Size(945, 577);
            grpCircle.TabIndex = 0;
            grpCircle.TabStop = false;
            grpCircle.Text = "goCircleGraph1";
            grpCircle.TextColor = "Fore";
            // 
            // tpTime
            // 
            tpTime.BackColor = Color.FromArgb(60, 60, 60);
            tpTime.Controls.Add(grpTime);
            tpTime.Location = new Point(4, 44);
            tpTime.Name = "tpTime";
            tpTime.Padding = new Padding(10);
            tpTime.Size = new Size(965, 597);
            tpTime.TabIndex = 3;
            tpTime.Text = "Time";
            // 
            // grpTime
            // 
            grpTime.BackColor = Color.FromArgb(60, 60, 60);
            grpTime.BackgroundColor = "Base2";
            grpTime.Dock = DockStyle.Fill;
            grpTime.FontName = "나눔고딕";
            grpTime.FontSize = 12F;
            grpTime.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            grpTime.GraphColor = "Back";
            grpTime.GridColor = "Base3";
            grpTime.Location = new Point(10, 10);
            grpTime.Name = "grpTime";
            grpTime.RemarkColor = "Base2";
            grpTime.Size = new Size(945, 577);
            grpTime.TabIndex = 0;
            grpTime.TabStop = false;
            grpTime.Text = "goTimeGraph1";
            grpTime.TextColor = "Fore";
            grpTime.TimeFormatString = null;
            grpTime.ValueFormatString = null;
            grpTime.XAxisGraduationTime = TimeSpan.Parse("00:10:00");
            grpTime.XScale = TimeSpan.Parse("01:00:00");
            grpTime.YAxisGraduationCount = 10;
            // 
            // tpTrend
            // 
            tpTrend.BackColor = Color.FromArgb(60, 60, 60);
            tpTrend.Controls.Add(grpTrend);
            tpTrend.Location = new Point(4, 44);
            tpTrend.Name = "tpTrend";
            tpTrend.Padding = new Padding(10);
            tpTrend.Size = new Size(965, 597);
            tpTrend.TabIndex = 4;
            tpTrend.Text = "Trend";
            // 
            // grpTrend
            // 
            grpTrend.BackColor = Color.FromArgb(60, 60, 60);
            grpTrend.BackgroundColor = "Base2";
            grpTrend.Dock = DockStyle.Fill;
            grpTrend.FontName = "나눔고딕";
            grpTrend.FontSize = 12F;
            grpTrend.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            grpTrend.GraphColor = "Back";
            grpTrend.GridColor = "Base3";
            grpTrend.Interval = 10;
            grpTrend.Location = new Point(10, 10);
            grpTrend.MaximumXScale = TimeSpan.Parse("00:00:10");
            grpTrend.Name = "grpTrend";
            grpTrend.Pause = false;
            grpTrend.RemarkColor = "Base2";
            grpTrend.Size = new Size(945, 577);
            grpTrend.TabIndex = 0;
            grpTrend.TabStop = false;
            grpTrend.Text = "goTrendGraph1";
            grpTrend.TextColor = "Fore";
            grpTrend.TimeFormatString = "HH:mm:ss.fff";
            grpTrend.ValueFormatString = null;
            grpTrend.XAxisGraduationTime = TimeSpan.Parse("00:00:00.2500000");
            grpTrend.XScale = TimeSpan.Parse("00:00:01");
            grpTrend.YAxisGraduationCount = 10;
            // 
            // goButton1
            // 
            goButton1.BackColor = Color.FromArgb(50, 50, 50);
            goButton1.BackgroundColor = "Back";
            goButton1.BackgroundDraw = true;
            goButton1.BorderOnly = false;
            goButton1.ButtonColor = "Base3";
            goButton1.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            goButton1.FontName = "나눔고딕";
            goButton1.FontSize = 12F;
            goButton1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            goButton1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            goButton1.IconGap = 5F;
            goButton1.IconSize = 12F;
            goButton1.IconString = null;
            goButton1.Location = new Point(456, 556);
            goButton1.Name = "goButton1";
            goButton1.Round = Going.UI.Enums.GoRoundType.All;
            goButton1.Size = new Size(75, 23);
            goButton1.TabIndex = 1;
            goButton1.TabStop = false;
            goButton1.Text = "goButton1";
            goButton1.TextColor = "Fore";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(993, 665);
            Controls.Add(goButton1);
            Controls.Add(goTabControl1);
            Name = "FormMain";
            Padding = new Padding(10);
            Text = "FormMain2";
            Title = "FormMain2";
            goTabControl1.ResumeLayout(false);
            tpBar.ResumeLayout(false);
            tpLine.ResumeLayout(false);
            tpCircle.ResumeLayout(false);
            tpTime.ResumeLayout(false);
            tpTrend.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Going.UI.Forms.Components.GoResourceManager rm;
        private Going.UI.Forms.Containers.GoTabControl goTabControl1;
        private TabPage tpBar;
        private Going.UI.Forms.Controls.GoBarGraph grpBar;
        private TabPage tpLine;
        private TabPage tpCircle;
        private TabPage tpTime;
        private TabPage tpTrend;
        private Going.UI.Forms.Controls.GoLineGraph grpLine;
        private Going.UI.Forms.Controls.GoCircleGraph grpCircle;
        private Going.UI.Forms.Controls.GoTimeGraph grpTime;
        private Going.UI.Forms.Controls.GoTrendGraph grpTrend;
        private Going.UI.Forms.Controls.GoButton goButton1;
    }
}