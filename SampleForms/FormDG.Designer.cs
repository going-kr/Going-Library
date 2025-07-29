namespace SampleForms
{
    partial class FormDG
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
            dgBind = new Going.UI.Forms.Controls.GoDataGrid();
            SuspendLayout();
            // 
            // dgBind
            // 
            dgBind.BackColor = Color.FromArgb(50, 50, 50);
            dgBind.BackgroundColor = "Back";
            dgBind.BoxColor = "Base2";
            dgBind.ColumnColor = "Base1";
            dgBind.ColumnHeight = 30F;
            dgBind.FontName = "나눔고딕";
            dgBind.FontSize = 12F;
            dgBind.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            dgBind.Location = new Point(75, 46);
            dgBind.Name = "dgBind";
            dgBind.RowColor = "Base2";
            dgBind.RowHeight = 30F;
            dgBind.ScrollMode = Going.UI.Utils.ScrollMode.Vertical;
            dgBind.SelectedRowColor = "Select";
            dgBind.SelectionMode = Going.UI.Enums.GoDataGridSelectionMode.Single;
            dgBind.Size = new Size(619, 362);
            dgBind.SummaryRowColor = "Base1";
            dgBind.TabIndex = 0;
            dgBind.TabStop = false;
            dgBind.Text = "goDataGrid1";
            dgBind.TextColor = "Fore";
            // 
            // FormDG
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgBind);
            Name = "FormDG";
            Text = "Form1";
            Title = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Going.UI.Forms.Controls.GoDataGrid dgBind;
    }
}