namespace Going.UIEditor.Forms
{
    partial class FormCollectionEditor<T>
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
            goTableLayoutPanel1 = new UI.Forms.Containers.GoTableLayoutPanel();
            btnOK = new UI.Forms.Controls.GoButton();
            btnCancel = new UI.Forms.Controls.GoButton();
            goTableLayoutPanel2 = new UI.Forms.Containers.GoTableLayoutPanel();
            goLabel2 = new UI.Forms.Controls.GoLabel();
            dg = new UI.Forms.Controls.GoDataGrid();
            pg = new Controls.GoPropertyGrid();
            goContainer1 = new UI.Forms.Containers.GoContainer();
            btnAdd = new UI.Forms.Controls.GoButton();
            btnDel = new UI.Forms.Controls.GoButton();
            goLabel1 = new UI.Forms.Controls.GoLabel();
            goTableLayoutPanel1.SuspendLayout();
            goTableLayoutPanel2.SuspendLayout();
            goContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // goTableLayoutPanel1
            // 
            goTableLayoutPanel1.BackColor = Color.FromArgb(50, 50, 50);
            goTableLayoutPanel1.BackgroundColor = "Back";
            goTableLayoutPanel1.ColumnCount = 3;
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            goTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            goTableLayoutPanel1.Controls.Add(btnOK, 1, 0);
            goTableLayoutPanel1.Controls.Add(btnCancel, 2, 0);
            goTableLayoutPanel1.Dock = DockStyle.Bottom;
            goTableLayoutPanel1.Location = new Point(5, 405);
            goTableLayoutPanel1.Name = "goTableLayoutPanel1";
            goTableLayoutPanel1.RowCount = 1;
            goTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goTableLayoutPanel1.Size = new Size(790, 40);
            goTableLayoutPanel1.TabIndex = 3;
            // 
            // btnOK
            // 
            btnOK.BackColor = Color.FromArgb(50, 50, 50);
            btnOK.BackgroundColor = "Back";
            btnOK.BackgroundDraw = true;
            btnOK.BorderOnly = false;
            btnOK.ButtonColor = "Base3";
            btnOK.Dock = DockStyle.Fill;
            btnOK.FontName = "나눔고딕";
            btnOK.FontSize = 12F;
            btnOK.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnOK.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnOK.IconGap = 5F;
            btnOK.IconSize = 12F;
            btnOK.IconString = null;
            btnOK.Location = new Point(635, 5);
            btnOK.Margin = new Padding(5);
            btnOK.Name = "btnOK";
            btnOK.Round = UI.Enums.GoRoundType.All;
            btnOK.Size = new Size(70, 30);
            btnOK.TabIndex = 0;
            btnOK.TabStop = false;
            btnOK.Text = "확인";
            btnOK.TextColor = "Fore";
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(50, 50, 50);
            btnCancel.BackgroundColor = "Back";
            btnCancel.BackgroundDraw = true;
            btnCancel.BorderOnly = false;
            btnCancel.ButtonColor = "Base3";
            btnCancel.Dock = DockStyle.Fill;
            btnCancel.FontName = "나눔고딕";
            btnCancel.FontSize = 12F;
            btnCancel.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnCancel.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnCancel.IconGap = 5F;
            btnCancel.IconSize = 12F;
            btnCancel.IconString = null;
            btnCancel.Location = new Point(715, 5);
            btnCancel.Margin = new Padding(5);
            btnCancel.Name = "btnCancel";
            btnCancel.Round = UI.Enums.GoRoundType.All;
            btnCancel.Size = new Size(70, 30);
            btnCancel.TabIndex = 1;
            btnCancel.TabStop = false;
            btnCancel.Text = "취소";
            btnCancel.TextColor = "Fore";
            // 
            // goTableLayoutPanel2
            // 
            goTableLayoutPanel2.BackColor = Color.FromArgb(50, 50, 50);
            goTableLayoutPanel2.BackgroundColor = "Back";
            goTableLayoutPanel2.ColumnCount = 2;
            goTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            goTableLayoutPanel2.Controls.Add(goLabel2, 1, 0);
            goTableLayoutPanel2.Controls.Add(dg, 0, 1);
            goTableLayoutPanel2.Controls.Add(pg, 1, 1);
            goTableLayoutPanel2.Controls.Add(goContainer1, 0, 0);
            goTableLayoutPanel2.Dock = DockStyle.Fill;
            goTableLayoutPanel2.Location = new Point(5, 5);
            goTableLayoutPanel2.Name = "goTableLayoutPanel2";
            goTableLayoutPanel2.RowCount = 2;
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            goTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goTableLayoutPanel2.Size = new Size(790, 400);
            goTableLayoutPanel2.TabIndex = 4;
            // 
            // goLabel2
            // 
            goLabel2.BackColor = Color.FromArgb(50, 50, 50);
            goLabel2.BackgroundColor = "Back";
            goLabel2.BackgroundDraw = false;
            goLabel2.BorderOnly = false;
            goLabel2.ContentAlignment = UI.Enums.GoContentAlignment.MiddleLeft;
            goLabel2.Dock = DockStyle.Fill;
            goLabel2.FontName = "나눔고딕";
            goLabel2.FontSize = 12F;
            goLabel2.FontStyle = UI.Enums.GoFontStyle.Normal;
            goLabel2.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            goLabel2.IconGap = 5F;
            goLabel2.IconSize = 12F;
            goLabel2.IconString = "fa-gears";
            goLabel2.LabelColor = "Base2";
            goLabel2.Location = new Point(400, 5);
            goLabel2.Margin = new Padding(5);
            goLabel2.Name = "goLabel2";
            goLabel2.Round = UI.Enums.GoRoundType.All;
            goLabel2.Size = new Size(385, 20);
            goLabel2.TabIndex = 1;
            goLabel2.TabStop = false;
            goLabel2.Text = "속성";
            goLabel2.TextColor = "Fore";
            goLabel2.TextPadding.Bottom = 0F;
            goLabel2.TextPadding.Left = 5F;
            goLabel2.TextPadding.Right = 0F;
            goLabel2.TextPadding.Top = 0F;
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
            dg.FontStyle = UI.Enums.GoFontStyle.Normal;
            dg.Location = new Point(3, 33);
            dg.Name = "dg";
            dg.RowColor = "Base2";
            dg.RowHeight = 30F;
            dg.ScrollMode = UI.Utils.ScrollMode.Vertical;
            dg.SelectedRowColor = "Select";
            dg.SelectionMode = UI.Enums.GoDataGridSelectionMode.Single;
            dg.Size = new Size(389, 364);
            dg.SummaryRowColor = "Base1";
            dg.TabIndex = 2;
            dg.TabStop = false;
            dg.Text = "goDataGrid1";
            dg.TextColor = "Fore";
            // 
            // pg
            // 
            pg.BackColor = Color.FromArgb(50, 50, 50);
            pg.BackgroundColor = "Back";
            pg.Dock = DockStyle.Fill;
            pg.ItemHeight = 30;
            pg.Location = new Point(398, 33);
            pg.Name = "pg";
            pg.ScrollPosition = 0D;
            pg.SelectedEditor = null;
            pg.SelectedObjects = null;
            pg.Size = new Size(389, 364);
            pg.TabIndex = 3;
            pg.TabStop = false;
            pg.Text = "goPropertyGrid1";
            // 
            // goContainer1
            // 
            goContainer1.BackColor = Color.FromArgb(50, 50, 50);
            goContainer1.BackgroundColor = "Back";
            goContainer1.Controls.Add(btnAdd);
            goContainer1.Controls.Add(btnDel);
            goContainer1.Controls.Add(goLabel1);
            goContainer1.Dock = DockStyle.Fill;
            goContainer1.Location = new Point(5, 5);
            goContainer1.Margin = new Padding(5);
            goContainer1.Name = "goContainer1";
            goContainer1.Size = new Size(385, 20);
            goContainer1.TabIndex = 4;
            goContainer1.TabStop = false;
            goContainer1.Text = "goContainer1";
            // 
            // btnAdd
            // 
            btnAdd.BackColor = Color.FromArgb(50, 50, 50);
            btnAdd.BackgroundColor = "Back";
            btnAdd.BackgroundDraw = false;
            btnAdd.BorderOnly = false;
            btnAdd.ButtonColor = "Base3";
            btnAdd.Dock = DockStyle.Right;
            btnAdd.FontName = "나눔고딕";
            btnAdd.FontSize = 12F;
            btnAdd.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnAdd.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnAdd.IconGap = 5F;
            btnAdd.IconSize = 12F;
            btnAdd.IconString = "fa-plus";
            btnAdd.Location = new Point(325, 0);
            btnAdd.Name = "btnAdd";
            btnAdd.Round = UI.Enums.GoRoundType.All;
            btnAdd.Size = new Size(30, 20);
            btnAdd.TabIndex = 2;
            btnAdd.TabStop = false;
            btnAdd.TextColor = "Fore";
            // 
            // btnDel
            // 
            btnDel.BackColor = Color.FromArgb(50, 50, 50);
            btnDel.BackgroundColor = "Back";
            btnDel.BackgroundDraw = false;
            btnDel.BorderOnly = false;
            btnDel.ButtonColor = "Base3";
            btnDel.Dock = DockStyle.Right;
            btnDel.FontName = "나눔고딕";
            btnDel.FontSize = 12F;
            btnDel.FontStyle = UI.Enums.GoFontStyle.Normal;
            btnDel.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            btnDel.IconGap = 5F;
            btnDel.IconSize = 12F;
            btnDel.IconString = "fa-minus";
            btnDel.Location = new Point(355, 0);
            btnDel.Name = "btnDel";
            btnDel.Round = UI.Enums.GoRoundType.All;
            btnDel.Size = new Size(30, 20);
            btnDel.TabIndex = 1;
            btnDel.TabStop = false;
            btnDel.TextColor = "Fore";
            // 
            // goLabel1
            // 
            goLabel1.BackColor = Color.FromArgb(50, 50, 50);
            goLabel1.BackgroundColor = "Back";
            goLabel1.BackgroundDraw = false;
            goLabel1.BorderOnly = false;
            goLabel1.ContentAlignment = UI.Enums.GoContentAlignment.MiddleLeft;
            goLabel1.Dock = DockStyle.Left;
            goLabel1.FontName = "나눔고딕";
            goLabel1.FontSize = 12F;
            goLabel1.FontStyle = UI.Enums.GoFontStyle.Normal;
            goLabel1.IconDirection = UI.Enums.GoDirectionHV.Horizon;
            goLabel1.IconGap = 5F;
            goLabel1.IconSize = 12F;
            goLabel1.IconString = "fa-list";
            goLabel1.LabelColor = "Base2";
            goLabel1.Location = new Point(0, 0);
            goLabel1.Margin = new Padding(5);
            goLabel1.Name = "goLabel1";
            goLabel1.Round = UI.Enums.GoRoundType.All;
            goLabel1.Size = new Size(151, 20);
            goLabel1.TabIndex = 0;
            goLabel1.TabStop = false;
            goLabel1.Text = "목록";
            goLabel1.TextColor = "Fore";
            goLabel1.TextPadding.Bottom = 0F;
            goLabel1.TextPadding.Left = 5F;
            goLabel1.TextPadding.Right = 0F;
            goLabel1.TextPadding.Top = 0F;
            // 
            // FormCollectionEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(goTableLayoutPanel2);
            Controls.Add(goTableLayoutPanel1);
            Name = "FormCollectionEditor";
            Padding = new Padding(5);
            Text = "FormCollectionEditor";
            Title = "FormCollectionEditor";
            goTableLayoutPanel1.ResumeLayout(false);
            goTableLayoutPanel2.ResumeLayout(false);
            goContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel1;
        private UI.Forms.Controls.GoButton btnOK;
        private UI.Forms.Controls.GoButton btnCancel;
        private UI.Forms.Containers.GoTableLayoutPanel goTableLayoutPanel2;
        private UI.Forms.Controls.GoLabel goLabel1;
        private UI.Forms.Controls.GoLabel goLabel2;
        private UI.Forms.Controls.GoDataGrid dg;
        private Controls.GoPropertyGrid pg;
        private UI.Forms.Containers.GoContainer goContainer1;
        private UI.Forms.Controls.GoButton btnAdd;
        private UI.Forms.Controls.GoButton btnDel;
    }
}