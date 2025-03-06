namespace SampleForms
{
    partial class FormCanvas
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
            ic = new Going.UI.Forms.ImageCanvas.IcCanvas();
            main = new TabPage();
            box7 = new Box();
            box6 = new Box();
            box5 = new Box();
            box4 = new Box();
            box3 = new Box();
            box2 = new Box();
            box1 = new Box();
            box8 = new Box();
            M_lblTime = new Going.UI.Forms.ImageCanvas.IcLabel();
            M_lblTitle = new Going.UI.Forms.ImageCanvas.IcLabel();
            M_btnAlarm = new Going.UI.Forms.ImageCanvas.IcButton();
            M_btnSchedule = new Going.UI.Forms.ImageCanvas.IcButton();
            M_btnStop = new Going.UI.Forms.ImageCanvas.IcButton();
            M_btnStart = new Going.UI.Forms.ImageCanvas.IcButton();
            setting = new TabPage();
            C_btnMain = new Going.UI.Forms.ImageCanvas.IcButton();
            schedule = new TabPage();
            S_btnConfirm = new Going.UI.Forms.ImageCanvas.IcButton();
            S_btnSave = new Going.UI.Forms.ImageCanvas.IcButton();
            S_btnMain = new Going.UI.Forms.ImageCanvas.IcButton();
            test = new TabPage();
            T_sld = new Going.UI.Forms.ImageCanvas.IcSlider();
            T_prgs = new Going.UI.Forms.ImageCanvas.IcProgress();
            T_btnMain = new Going.UI.Forms.ImageCanvas.IcButton();
            ic.SuspendLayout();
            main.SuspendLayout();
            setting.SuspendLayout();
            schedule.SuspendLayout();
            test.SuspendLayout();
            SuspendLayout();
            // 
            // ic
            // 
            ic.Appearance = TabAppearance.FlatButtons;
            ic.BackgroundColor = "#f2f2f2";
            ic.Controls.Add(main);
            ic.Controls.Add(setting);
            ic.Controls.Add(schedule);
            ic.Controls.Add(test);
            ic.Dock = DockStyle.Fill;
            ic.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            ic.ItemSize = new Size(0, 1);
            ic.Location = new Point(0, 0);
            ic.Multiline = true;
            ic.Name = "ic";
            ic.SelectedIndex = 0;
            ic.Size = new Size(800, 480);
            ic.SizeMode = TabSizeMode.Fixed;
            ic.TabIndex = 0;
            ic.TabStop = false;
            ic.Text = "icCanvas1";
            // 
            // main
            // 
            main.Controls.Add(box7);
            main.Controls.Add(box6);
            main.Controls.Add(box5);
            main.Controls.Add(box4);
            main.Controls.Add(box3);
            main.Controls.Add(box2);
            main.Controls.Add(box1);
            main.Controls.Add(box8);
            main.Controls.Add(M_lblTime);
            main.Controls.Add(M_lblTitle);
            main.Controls.Add(M_btnAlarm);
            main.Controls.Add(M_btnSchedule);
            main.Controls.Add(M_btnStop);
            main.Controls.Add(M_btnStart);
            main.Location = new Point(0, 0);
            main.Margin = new Padding(0);
            main.Name = "main";
            main.Size = new Size(800, 480);
            main.TabIndex = 0;
            main.Text = "tabPage4";
            main.UseVisualStyleBackColor = true;
            // 
            // box7
            // 
            box7.Alarm = false;
            box7.BackgroundColor = "white";
            box7.ContainerName = "box";
            box7.IconState = 0;
            box7.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            box7.Location = new Point(402, 232);
            box7.Name = "box7";
            box7.Run = false;
            box7.Size = new Size(187, 172);
            box7.TabIndex = 46;
            box7.Title = "A-1";
            box7.Value1 = "0.0";
            box7.Value2 = "0.0";
            // 
            // box6
            // 
            box6.Alarm = false;
            box6.BackgroundColor = "white";
            box6.ContainerName = "box";
            box6.IconState = 0;
            box6.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            box6.Location = new Point(209, 232);
            box6.Name = "box6";
            box6.Run = false;
            box6.Size = new Size(187, 172);
            box6.TabIndex = 45;
            box6.Title = "A-1";
            box6.Value1 = "0.0";
            box6.Value2 = "0.0";
            // 
            // box5
            // 
            box5.Alarm = false;
            box5.BackgroundColor = "white";
            box5.ContainerName = "box";
            box5.IconState = 0;
            box5.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            box5.Location = new Point(16, 232);
            box5.Name = "box5";
            box5.Run = false;
            box5.Size = new Size(187, 172);
            box5.TabIndex = 44;
            box5.Title = "A-1";
            box5.Value1 = "0.0";
            box5.Value2 = "0.0";
            // 
            // box4
            // 
            box4.Alarm = false;
            box4.BackgroundColor = "white";
            box4.ContainerName = "box";
            box4.IconState = 0;
            box4.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            box4.Location = new Point(595, 54);
            box4.Name = "box4";
            box4.Run = false;
            box4.Size = new Size(187, 172);
            box4.TabIndex = 43;
            box4.Title = "A-1";
            box4.Value1 = "0.0";
            box4.Value2 = "0.0";
            // 
            // box3
            // 
            box3.Alarm = false;
            box3.BackgroundColor = "white";
            box3.ContainerName = "box";
            box3.IconState = 0;
            box3.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            box3.Location = new Point(402, 54);
            box3.Name = "box3";
            box3.Run = false;
            box3.Size = new Size(187, 172);
            box3.TabIndex = 42;
            box3.Title = "A-1";
            box3.Value1 = "0.0";
            box3.Value2 = "0.0";
            // 
            // box2
            // 
            box2.Alarm = false;
            box2.BackgroundColor = "white";
            box2.ContainerName = "box";
            box2.IconState = 0;
            box2.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            box2.Location = new Point(209, 54);
            box2.Name = "box2";
            box2.Run = false;
            box2.Size = new Size(187, 172);
            box2.TabIndex = 41;
            box2.Title = "A-1";
            box2.Value1 = "0.0";
            box2.Value2 = "0.0";
            // 
            // box1
            // 
            box1.Alarm = false;
            box1.BackgroundColor = "white";
            box1.ContainerName = "box";
            box1.IconState = 0;
            box1.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            box1.Location = new Point(16, 54);
            box1.Name = "box1";
            box1.Run = false;
            box1.Size = new Size(187, 172);
            box1.TabIndex = 40;
            box1.Title = "A-1";
            box1.Value1 = "0.0";
            box1.Value2 = "0.0";
            // 
            // box8
            // 
            box8.Alarm = false;
            box8.BackgroundColor = "white";
            box8.ContainerName = "box";
            box8.IconState = 0;
            box8.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            box8.Location = new Point(595, 232);
            box8.Name = "box8";
            box8.Run = false;
            box8.Size = new Size(187, 172);
            box8.TabIndex = 39;
            box8.Title = "A-1";
            box8.Value1 = "0.0";
            box8.Value2 = "0.0";
            // 
            // M_lblTime
            // 
            M_lblTime.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleLeft;
            M_lblTime.FontName = "나눔바른고딕";
            M_lblTime.FontSize = 16F;
            M_lblTime.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            M_lblTime.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            M_lblTime.IconGap = 5F;
            M_lblTime.IconSize = 12F;
            M_lblTime.IconString = null;
            M_lblTime.Location = new Point(581, 0);
            M_lblTime.Name = "M_lblTime";
            M_lblTime.Size = new Size(220, 38);
            M_lblTime.TabIndex = 30;
            M_lblTime.Text = "2025/02/28 (금) 03:20:20";
            M_lblTime.TextColor = "Black";
            // 
            // M_lblTitle
            // 
            M_lblTitle.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleLeft;
            M_lblTitle.FontName = "나눔바른고딕";
            M_lblTitle.FontSize = 16F;
            M_lblTitle.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            M_lblTitle.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            M_lblTitle.IconGap = 5F;
            M_lblTitle.IconSize = 12F;
            M_lblTitle.IconString = null;
            M_lblTitle.Location = new Point(10, 0);
            M_lblTitle.Name = "M_lblTitle";
            M_lblTitle.Size = new Size(276, 38);
            M_lblTitle.TabIndex = 29;
            M_lblTitle.Text = "A 구역 난방기 운전 화면";
            M_lblTitle.TextColor = "Black";
            // 
            // M_btnAlarm
            // 
            M_btnAlarm.FontName = "나눔바른고딕";
            M_btnAlarm.FontSize = 16F;
            M_btnAlarm.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            M_btnAlarm.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            M_btnAlarm.IconGap = 5F;
            M_btnAlarm.IconSize = 12F;
            M_btnAlarm.IconString = null;
            M_btnAlarm.Location = new Point(599, 439);
            M_btnAlarm.Name = "M_btnAlarm";
            M_btnAlarm.Size = new Size(201, 41);
            M_btnAlarm.TabIndex = 12;
            M_btnAlarm.Text = "알람 이력";
            M_btnAlarm.TextColor = "white";
            // 
            // M_btnSchedule
            // 
            M_btnSchedule.FontName = "나눔바른고딕";
            M_btnSchedule.FontSize = 16F;
            M_btnSchedule.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            M_btnSchedule.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            M_btnSchedule.IconGap = 5F;
            M_btnSchedule.IconSize = 12F;
            M_btnSchedule.IconString = null;
            M_btnSchedule.Location = new Point(398, 439);
            M_btnSchedule.Name = "M_btnSchedule";
            M_btnSchedule.Size = new Size(201, 41);
            M_btnSchedule.TabIndex = 11;
            M_btnSchedule.Text = "스케쥴 확인";
            M_btnSchedule.TextColor = "white";
            // 
            // M_btnStop
            // 
            M_btnStop.FontName = "나눔바른고딕";
            M_btnStop.FontSize = 16F;
            M_btnStop.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            M_btnStop.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            M_btnStop.IconGap = 5F;
            M_btnStop.IconSize = 12F;
            M_btnStop.IconString = null;
            M_btnStop.Location = new Point(200, 439);
            M_btnStop.Name = "M_btnStop";
            M_btnStop.Size = new Size(201, 41);
            M_btnStop.TabIndex = 10;
            M_btnStop.Text = "전체 정지";
            M_btnStop.TextColor = "white";
            // 
            // M_btnStart
            // 
            M_btnStart.FontName = "나눔바른고딕";
            M_btnStart.FontSize = 16F;
            M_btnStart.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            M_btnStart.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            M_btnStart.IconGap = 5F;
            M_btnStart.IconSize = 12F;
            M_btnStart.IconString = null;
            M_btnStart.Location = new Point(0, 439);
            M_btnStart.Name = "M_btnStart";
            M_btnStart.Size = new Size(201, 41);
            M_btnStart.TabIndex = 9;
            M_btnStart.Text = "전체 운전";
            M_btnStart.TextColor = "white";
            // 
            // setting
            // 
            setting.Controls.Add(C_btnMain);
            setting.Location = new Point(0, 0);
            setting.Name = "setting";
            setting.Size = new Size(800, 480);
            setting.TabIndex = 1;
            setting.Text = "tabPage5";
            setting.UseVisualStyleBackColor = true;
            // 
            // C_btnMain
            // 
            C_btnMain.FontName = "나눔고딕";
            C_btnMain.FontSize = 12F;
            C_btnMain.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            C_btnMain.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            C_btnMain.IconGap = 5F;
            C_btnMain.IconSize = 12F;
            C_btnMain.IconString = null;
            C_btnMain.Location = new Point(628, 0);
            C_btnMain.Name = "C_btnMain";
            C_btnMain.Size = new Size(172, 38);
            C_btnMain.TabIndex = 0;
            C_btnMain.TextColor = "Black";
            // 
            // schedule
            // 
            schedule.Controls.Add(S_btnConfirm);
            schedule.Controls.Add(S_btnSave);
            schedule.Controls.Add(S_btnMain);
            schedule.Location = new Point(0, 0);
            schedule.Name = "schedule";
            schedule.Size = new Size(800, 480);
            schedule.TabIndex = 2;
            schedule.Text = "tabPage6";
            schedule.UseVisualStyleBackColor = true;
            // 
            // S_btnConfirm
            // 
            S_btnConfirm.FontName = "나눔고딕";
            S_btnConfirm.FontSize = 14F;
            S_btnConfirm.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            S_btnConfirm.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            S_btnConfirm.IconGap = 5F;
            S_btnConfirm.IconSize = 12F;
            S_btnConfirm.IconString = null;
            S_btnConfirm.Location = new Point(0, 440);
            S_btnConfirm.Name = "S_btnConfirm";
            S_btnConfirm.Size = new Size(266, 40);
            S_btnConfirm.TabIndex = 2;
            S_btnConfirm.Text = "확인화면";
            S_btnConfirm.TextColor = "white";
            // 
            // S_btnSave
            // 
            S_btnSave.FontName = "나눔고딕";
            S_btnSave.FontSize = 14F;
            S_btnSave.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            S_btnSave.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            S_btnSave.IconGap = 5F;
            S_btnSave.IconSize = 12F;
            S_btnSave.IconString = null;
            S_btnSave.Location = new Point(267, 440);
            S_btnSave.Name = "S_btnSave";
            S_btnSave.Size = new Size(266, 40);
            S_btnSave.TabIndex = 1;
            S_btnSave.Text = "저장";
            S_btnSave.TextColor = "white";
            // 
            // S_btnMain
            // 
            S_btnMain.FontName = "나눔고딕";
            S_btnMain.FontSize = 14F;
            S_btnMain.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            S_btnMain.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            S_btnMain.IconGap = 5F;
            S_btnMain.IconSize = 12F;
            S_btnMain.IconString = null;
            S_btnMain.Location = new Point(534, 440);
            S_btnMain.Name = "S_btnMain";
            S_btnMain.Size = new Size(266, 40);
            S_btnMain.TabIndex = 0;
            S_btnMain.Text = "메인화면";
            S_btnMain.TextColor = "white";
            // 
            // test
            // 
            test.Controls.Add(T_sld);
            test.Controls.Add(T_prgs);
            test.Controls.Add(T_btnMain);
            test.Location = new Point(0, 0);
            test.Name = "test";
            test.Size = new Size(800, 480);
            test.TabIndex = 3;
            test.Text = "tabPage1";
            test.UseVisualStyleBackColor = true;
            // 
            // T_sld
            // 
            T_sld.BarName = "bar";
            T_sld.CurosrName = "sld";
            T_sld.DrawText = true;
            T_sld.FontName = "나눔고딕";
            T_sld.FontSize = 12F;
            T_sld.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            T_sld.FormatString = "0'%'";
            T_sld.Location = new Point(21, 114);
            T_sld.Maximum = 100D;
            T_sld.Minimum = 0D;
            T_sld.Name = "T_sld";
            T_sld.Size = new Size(441, 58);
            T_sld.TabIndex = 2;
            T_sld.Text = "icSlider1";
            T_sld.TextColor = "Black";
            T_sld.Tick = null;
            T_sld.Value = 0D;
            // 
            // T_prgs
            // 
            T_prgs.BarName = "prgs";
            T_prgs.DrawText = true;
            T_prgs.FontName = "나눔고딕";
            T_prgs.FontSize = 12F;
            T_prgs.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            T_prgs.FormatString = "0 '%'";
            T_prgs.Location = new Point(21, 50);
            T_prgs.Maximum = 100D;
            T_prgs.Minimum = 0D;
            T_prgs.Name = "T_prgs";
            T_prgs.Size = new Size(441, 58);
            T_prgs.TabIndex = 1;
            T_prgs.Text = "icProgress1";
            T_prgs.TextColor = "Black";
            T_prgs.Value = 0D;
            // 
            // T_btnMain
            // 
            T_btnMain.FontName = "나눔고딕";
            T_btnMain.FontSize = 12F;
            T_btnMain.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            T_btnMain.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            T_btnMain.IconGap = 5F;
            T_btnMain.IconSize = 12F;
            T_btnMain.IconString = null;
            T_btnMain.Location = new Point(625, 0);
            T_btnMain.Name = "T_btnMain";
            T_btnMain.Size = new Size(175, 39);
            T_btnMain.TabIndex = 0;
            T_btnMain.TextColor = "Black";
            // 
            // FormCanvas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(50, 50, 50);
            ClientSize = new Size(800, 480);
            Controls.Add(ic);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "FormCanvas";
            Text = "Sample Window";
            Title = "Sample Window";
            TitleIconString = "fa-check";
            ic.ResumeLayout(false);
            main.ResumeLayout(false);
            setting.ResumeLayout(false);
            schedule.ResumeLayout(false);
            test.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Going.UI.Forms.Containers.GoPanel goPanel1;
        private Going.UI.Forms.ImageCanvas.IcCanvas ic;
        private TabPage main;
        private TabPage setting;
        private TabPage schedule;
        private Going.UI.Forms.ImageCanvas.IcButton M_btnStart;
        private Going.UI.Forms.ImageCanvas.IcButton M_btnAlarm;
        private Going.UI.Forms.ImageCanvas.IcButton M_btnSchedule;
        private Going.UI.Forms.ImageCanvas.IcButton M_btnStop;
        private Going.UI.Forms.ImageCanvas.IcButton S_btnConfirm;
        private Going.UI.Forms.ImageCanvas.IcButton S_btnSave;
        private Going.UI.Forms.ImageCanvas.IcButton S_btnMain;
        private Going.UI.Forms.ImageCanvas.IcButton C_btnMain;
        private Going.UI.Forms.ImageCanvas.IcLabel M_lblTitle;
        private Going.UI.Forms.ImageCanvas.IcLabel M_lblTime;
        private TabPage test;
        private Going.UI.Forms.ImageCanvas.IcButton T_btnMain;
        private Going.UI.Forms.ImageCanvas.IcProgress T_prgs;
        private Going.UI.Forms.ImageCanvas.IcSlider T_sld;
        private Box box7;
        private Box box6;
        private Box box5;
        private Box box4;
        private Box box3;
        private Box box2;
        private Box box1;
        private Box box8;
    }
}
