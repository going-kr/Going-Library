namespace Going.UIEditor.Windows
{
    partial class PromptWindow
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
            pnl = new Going.UI.Forms.Containers.GoBoxPanel();
            SuspendLayout();
            // 
            // pnl
            // 
            pnl.BackColor = Color.FromArgb(50, 50, 50);
            pnl.BackgroundColor = "Back";
            pnl.BackgroundDraw = true;
            pnl.BorderColor = "Base3";
            pnl.BoxColor = "Base1";
            pnl.Dock = DockStyle.Fill;
            pnl.Location = new Point(10, 10);
            pnl.Margin = new Padding(3, 5, 3, 5);
            pnl.Name = "pnl";
            pnl.Padding = new Padding(10);
            pnl.Round = UI.Enums.GoRoundType.All;
            pnl.Size = new Size(780, 430);
            pnl.TabIndex = 2;
            pnl.TabStop = false;
            pnl.Text = "goBoxPanel1";
            // 
            // PromptWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pnl);
            Name = "PromptWindow";
            Padding = new Padding(10);
            Text = "PromptWindow";
            Title = "PromptWindow";
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoBoxPanel pnl;
    }
}