namespace Going.UIEditor.Windows
{
    partial class ToolBoxWindow
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
            UI.Collections.ObservableList<UI.Datas.GoToolCategory> observableList_12 = new UI.Collections.ObservableList<UI.Datas.GoToolCategory>();
            toolBox = new UI.Forms.Controls.GoToolBox();
            SuspendLayout();
            // 
            // toolBox
            // 
            toolBox.BackColor = Color.FromArgb(50, 50, 50);
            toolBox.BackgroundColor = "Back";
            toolBox.BackgroundDraw = false;
            toolBox.BorderColor = "Base3";
            toolBox.BoxColor = "Base1";
            observableList_12.Changed = false;
            toolBox.Categories = observableList_12;
            toolBox.CategoryColor = "Base2";
            toolBox.Dock = DockStyle.Fill;
            toolBox.FontName = "나눔고딕";
            toolBox.FontSize = 12F;
            toolBox.FontStyle = UI.Enums.GoFontStyle.Normal;
            toolBox.IconGap = 5F;
            toolBox.IconSize = 12F;
            toolBox.ItemHeight = 30F;
            toolBox.Location = new Point(5, 5);
            toolBox.Name = "toolBox";
            toolBox.Round = UI.Enums.GoRoundType.All;
            toolBox.SelectColor = "Select";
            toolBox.Size = new Size(790, 440);
            toolBox.TabIndex = 0;
            toolBox.TabStop = false;
            toolBox.Text = "goToolBox1";
            toolBox.TextColor = "Fore";
            // 
            // ToolBoxWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(toolBox);
            Name = "ToolBoxWindow";
            Padding = new Padding(5);
            Text = "ToolBoxWindow";
            Title = "ToolBoxWindow";
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Controls.GoToolBox toolBox;
    }
}