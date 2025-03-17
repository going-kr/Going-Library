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
            rm = new Going.UI.Forms.Components.GoResourceManager();
            dg = new Going.UI.Forms.Controls.GoDataGrid();
            SuspendLayout();
            // 
            // rm
            // 
            rm.ImageFolder = "D:\\Project\\Going\\library\\src\\Going\\ImageSample";
            // 
            // dg
            // 
            dg.BackColor = Color.FromArgb(50, 50, 50);
            dg.BackgroundColor = "Back";
            dg.ColumnColor = "Base1";
            dg.ColumnHeight = 30F;
            dg.Dock = DockStyle.Fill;
            dg.FontName = "나눔고딕";
            dg.FontSize = 12F;
            dg.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            dg.Location = new Point(10, 10);
            dg.Name = "dg";
            dg.RowColor = "Base2";
            dg.RowHeight = 30F;
            dg.ScrollMode = Going.UI.Utils.ScrollMode.Vertical;
            dg.SelectedRowColor = "Select";
            dg.SelectionMode = Going.UI.Enums.GoDataGridSelectionMode.Single;
            dg.Size = new Size(973, 645);
            dg.SummaryRowColor = "Base1";
            dg.TabIndex = 0;
            dg.TabStop = false;
            dg.Text = "goDataGrid1";
            dg.TextColor = "Fore";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(993, 665);
            Controls.Add(dg);
            Name = "FormMain";
            Padding = new Padding(10);
            Text = "FormMain2";
            Title = "FormMain2";
            ResumeLayout(false);
        }

        #endregion
        private Going.UI.Forms.Components.GoResourceManager rm;
        private Going.UI.Forms.Controls.GoDataGrid dg;
    }
}