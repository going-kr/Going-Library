namespace SampleForms
{
    partial class FormCanvas2
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
            icCanvas1 = new Going.UI.Forms.ImageCanvas.IcCanvas();
            main = new TabPage();
            schedule = new TabPage();
            icOnOff5 = new Going.UI.Forms.ImageCanvas.IcOnOff();
            icOnOff4 = new Going.UI.Forms.ImageCanvas.IcOnOff();
            icOnOff3 = new Going.UI.Forms.ImageCanvas.IcOnOff();
            icOnOff2 = new Going.UI.Forms.ImageCanvas.IcOnOff();
            icOnOff1 = new Going.UI.Forms.ImageCanvas.IcOnOff();
            setting = new TabPage();
            icCanvas1.SuspendLayout();
            schedule.SuspendLayout();
            SuspendLayout();
            // 
            // icCanvas1
            // 
            icCanvas1.Appearance = TabAppearance.FlatButtons;
            icCanvas1.BackgroundColor = "Back";
            icCanvas1.Controls.Add(main);
            icCanvas1.Controls.Add(schedule);
            icCanvas1.Controls.Add(setting);
            icCanvas1.Dock = DockStyle.Fill;
            icCanvas1.ImageFolder = "C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample";
            icCanvas1.ItemSize = new Size(0, 1);
            icCanvas1.Location = new Point(0, 0);
            icCanvas1.Name = "icCanvas1";
            icCanvas1.SelectedIndex = 0;
            icCanvas1.Size = new Size(800, 450);
            icCanvas1.SizeMode = TabSizeMode.Fixed;
            icCanvas1.TabIndex = 0;
            // 
            // main
            // 
            main.Location = new Point(0, 0);
            main.Name = "main";
            main.Padding = new Padding(3);
            main.Size = new Size(800, 450);
            main.TabIndex = 0;
            main.Text = "tabPage1";
            main.UseVisualStyleBackColor = true;
            // 
            // schedule
            // 
            schedule.Controls.Add(icOnOff5);
            schedule.Controls.Add(icOnOff4);
            schedule.Controls.Add(icOnOff3);
            schedule.Controls.Add(icOnOff2);
            schedule.Controls.Add(icOnOff1);
            schedule.Location = new Point(0, 0);
            schedule.Name = "schedule";
            schedule.Padding = new Padding(3);
            schedule.Size = new Size(800, 450);
            schedule.TabIndex = 1;
            schedule.Text = "tabPage2";
            schedule.UseVisualStyleBackColor = true;
            // 
            // icOnOff5
            // 
            icOnOff5.FontName = "나눔고딕";
            icOnOff5.FontSize = 12F;
            icOnOff5.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            icOnOff5.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            icOnOff5.IconGap = 5F;
            icOnOff5.IconSize = 12F;
            icOnOff5.IconString = null;
            icOnOff5.Location = new Point(12, 349);
            icOnOff5.Name = "icOnOff5";
            icOnOff5.OnOff = false;
            icOnOff5.Size = new Size(135, 48);
            icOnOff5.TabIndex = 4;
            icOnOff5.TextColor = "Black";
            icOnOff5.ToggleMode = true;
            // 
            // icOnOff4
            // 
            icOnOff4.FontName = "나눔고딕";
            icOnOff4.FontSize = 12F;
            icOnOff4.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            icOnOff4.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            icOnOff4.IconGap = 5F;
            icOnOff4.IconSize = 12F;
            icOnOff4.IconString = null;
            icOnOff4.Location = new Point(10, 293);
            icOnOff4.Name = "icOnOff4";
            icOnOff4.OnOff = false;
            icOnOff4.Size = new Size(135, 48);
            icOnOff4.TabIndex = 3;
            icOnOff4.TextColor = "Black";
            icOnOff4.ToggleMode = true;
            // 
            // icOnOff3
            // 
            icOnOff3.FontName = "나눔고딕";
            icOnOff3.FontSize = 12F;
            icOnOff3.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            icOnOff3.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            icOnOff3.IconGap = 5F;
            icOnOff3.IconSize = 12F;
            icOnOff3.IconString = null;
            icOnOff3.Location = new Point(12, 236);
            icOnOff3.Name = "icOnOff3";
            icOnOff3.OnOff = false;
            icOnOff3.Size = new Size(135, 48);
            icOnOff3.TabIndex = 2;
            icOnOff3.TextColor = "Black";
            icOnOff3.ToggleMode = true;
            // 
            // icOnOff2
            // 
            icOnOff2.FontName = "나눔고딕";
            icOnOff2.FontSize = 12F;
            icOnOff2.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            icOnOff2.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            icOnOff2.IconGap = 5F;
            icOnOff2.IconSize = 12F;
            icOnOff2.IconString = null;
            icOnOff2.Location = new Point(12, 182);
            icOnOff2.Name = "icOnOff2";
            icOnOff2.OnOff = false;
            icOnOff2.Size = new Size(135, 48);
            icOnOff2.TabIndex = 1;
            icOnOff2.TextColor = "Black";
            icOnOff2.ToggleMode = true;
            // 
            // icOnOff1
            // 
            icOnOff1.FontName = "나눔고딕";
            icOnOff1.FontSize = 12F;
            icOnOff1.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            icOnOff1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            icOnOff1.IconGap = 5F;
            icOnOff1.IconSize = 12F;
            icOnOff1.IconString = null;
            icOnOff1.Location = new Point(12, 128);
            icOnOff1.Name = "icOnOff1";
            icOnOff1.OnOff = false;
            icOnOff1.Size = new Size(135, 48);
            icOnOff1.TabIndex = 0;
            icOnOff1.TextColor = "Black";
            icOnOff1.ToggleMode = true;
            // 
            // setting
            // 
            setting.Location = new Point(0, 0);
            setting.Name = "setting";
            setting.Size = new Size(800, 450);
            setting.TabIndex = 2;
            setting.Text = "tabPage3";
            setting.UseVisualStyleBackColor = true;
            // 
            // FormCanvas2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(icCanvas1);
            Name = "FormCanvas2";
            Text = "FormCanvas2";
            icCanvas1.ResumeLayout(false);
            schedule.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Going.UI.Forms.ImageCanvas.IcCanvas icCanvas1;
        private TabPage main;
        private TabPage schedule;
        private Going.UI.Forms.ImageCanvas.IcOnOff icOnOff5;
        private Going.UI.Forms.ImageCanvas.IcOnOff icOnOff4;
        private Going.UI.Forms.ImageCanvas.IcOnOff icOnOff3;
        private Going.UI.Forms.ImageCanvas.IcOnOff icOnOff2;
        private Going.UI.Forms.ImageCanvas.IcOnOff icOnOff1;
        private TabPage setting;
    }
}