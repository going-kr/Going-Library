namespace Going.UIEditor.Windows
{
    partial class PropertiesWindow
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
            pg = new Controls.GoPropertyGrid();
            SuspendLayout();
            // 
            // pg
            // 
            pg.BackColor = Color.FromArgb(50, 50, 50);
            pg.BackgroundColor = "Back";
            pg.Dock = DockStyle.Fill;
            pg.Location = new Point(5, 5);
            pg.Name = "pg";
            pg.Size = new Size(790, 440);
            pg.TabIndex = 0;
            pg.TabStop = false;
            pg.Text = "goPropertyGrid1";
            // 
            // PropertiesWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pg);
            Name = "PropertiesWindow";
            Padding = new Padding(5);
            Text = "PropertiesWindow";
            Title = "PropertiesWindow";
            ResumeLayout(false);
        }

        #endregion

        private Controls.GoPropertyGrid pg;
    }
}