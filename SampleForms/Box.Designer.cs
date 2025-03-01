namespace SampleForms
{
    partial class Box
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            btnSetting = new Going.UI.Forms.ImageCanvas.IcButton();
            ooAlm = new Going.UI.Forms.ImageCanvas.IcOnOff();
            ooRun = new Going.UI.Forms.ImageCanvas.IcOnOff();
            ooSw = new Going.UI.Forms.ImageCanvas.IcOnOff();
            stateIco = new Going.UI.Forms.ImageCanvas.IcState();
            lblValue1 = new Going.UI.Forms.ImageCanvas.IcLabel();
            lblValue2 = new Going.UI.Forms.ImageCanvas.IcLabel();
            lblTitle = new Going.UI.Forms.ImageCanvas.IcLabel();
            SuspendLayout();
            // 
            // btnSetting
            // 
            btnSetting.FontName = "나눔고딕";
            btnSetting.FontSize = 12F;
            btnSetting.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            btnSetting.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            btnSetting.IconGap = 5F;
            btnSetting.IconSize = 12F;
            btnSetting.IconString = null;
            btnSetting.Location = new Point(124, 8);
            btnSetting.Name = "btnSetting";
            btnSetting.Size = new Size(53, 33);
            btnSetting.TabIndex = 0;
            btnSetting.TextColor = "Black";
            // 
            // ooAlm
            // 
            ooAlm.FontName = "나눔고딕";
            ooAlm.FontSize = 12F;
            ooAlm.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            ooAlm.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            ooAlm.IconGap = 5F;
            ooAlm.IconSize = 12F;
            ooAlm.IconString = null;
            ooAlm.Location = new Point(16, 8);
            ooAlm.Name = "ooAlm";
            ooAlm.OnOff = false;
            ooAlm.Size = new Size(48, 33);
            ooAlm.TabIndex = 1;
            ooAlm.TextColor = "Black";
            ooAlm.ToggleMode = false;
            // 
            // ooRun
            // 
            ooRun.FontName = "나눔고딕";
            ooRun.FontSize = 12F;
            ooRun.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            ooRun.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            ooRun.IconGap = 5F;
            ooRun.IconSize = 12F;
            ooRun.IconString = null;
            ooRun.Location = new Point(70, 8);
            ooRun.Name = "ooRun";
            ooRun.OnOff = false;
            ooRun.Size = new Size(48, 33);
            ooRun.TabIndex = 2;
            ooRun.TextColor = "Black";
            ooRun.ToggleMode = false;
            // 
            // ooSw
            // 
            ooSw.FontName = "나눔고딕";
            ooSw.FontSize = 12F;
            ooSw.FontStyle = Going.UI.Enums.GoFontStyle.Normal;
            ooSw.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            ooSw.IconGap = 5F;
            ooSw.IconSize = 12F;
            ooSw.IconString = null;
            ooSw.Location = new Point(16, 56);
            ooSw.Name = "ooSw";
            ooSw.OnOff = false;
            ooSw.Size = new Size(74, 50);
            ooSw.TabIndex = 3;
            ooSw.TextColor = "Black";
            ooSw.ToggleMode = true;
            // 
            // stateIco
            // 
            stateIco.Location = new Point(100, 58);
            stateIco.Name = "stateIco";
            stateIco.Size = new Size(70, 46);
            stateIco.State = 0;
            stateIco.StateName = "icon";
            stateIco.TabIndex = 4;
            stateIco.Text = "icState1";
            // 
            // lblValue1
            // 
            lblValue1.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            lblValue1.FontName = "나눔바른고딕";
            lblValue1.FontSize = 14F;
            lblValue1.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            lblValue1.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            lblValue1.IconGap = 5F;
            lblValue1.IconSize = 12F;
            lblValue1.IconString = null;
            lblValue1.Location = new Point(16, 110);
            lblValue1.Name = "lblValue1";
            lblValue1.Size = new Size(74, 24);
            lblValue1.TabIndex = 5;
            lblValue1.Text = "0.0";
            lblValue1.TextColor = "Black";
            // 
            // lblValue2
            // 
            lblValue2.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            lblValue2.FontName = "나눔바른고딕";
            lblValue2.FontSize = 14F;
            lblValue2.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            lblValue2.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            lblValue2.IconGap = 5F;
            lblValue2.IconSize = 12F;
            lblValue2.IconString = null;
            lblValue2.Location = new Point(100, 110);
            lblValue2.Name = "lblValue2";
            lblValue2.Size = new Size(70, 24);
            lblValue2.TabIndex = 6;
            lblValue2.Text = "0.0";
            lblValue2.TextColor = "Black";
            // 
            // lblTitle
            // 
            lblTitle.ContentAlignment = Going.UI.Enums.GoContentAlignment.MiddleCenter;
            lblTitle.FontName = "나눔바른고딕";
            lblTitle.FontSize = 12F;
            lblTitle.FontStyle = Going.UI.Enums.GoFontStyle.Bold;
            lblTitle.IconDirection = Going.UI.Enums.GoDirectionHV.Horizon;
            lblTitle.IconGap = 5F;
            lblTitle.IconSize = 12F;
            lblTitle.IconString = null;
            lblTitle.Location = new Point(16, 143);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(154, 24);
            lblTitle.TabIndex = 7;
            lblTitle.Text = "A-1";
            lblTitle.TextColor = "Black";
            // 
            // Box
            // 
            BackgroundColor = "white";
            ContainerName = "box";
            Controls.Add(lblTitle);
            Controls.Add(lblValue2);
            Controls.Add(lblValue1);
            Controls.Add(stateIco);
            Controls.Add(ooSw);
            Controls.Add(ooRun);
            Controls.Add(ooAlm);
            Controls.Add(btnSetting);
            ImageFolder = "D:\\Project\\Going\\library\\src\\Going\\ImageCanvasSample";
            Name = "Box";
            Size = new Size(187, 172);
            ResumeLayout(false);
        }

        #endregion

        private Going.UI.Forms.ImageCanvas.IcButton btnSetting;
        private Going.UI.Forms.ImageCanvas.IcOnOff ooAlm;
        private Going.UI.Forms.ImageCanvas.IcOnOff ooRun;
        private Going.UI.Forms.ImageCanvas.IcOnOff ooSw;
        private Going.UI.Forms.ImageCanvas.IcState stateIco;
        private Going.UI.Forms.ImageCanvas.IcLabel lblValue1;
        private Going.UI.Forms.ImageCanvas.IcLabel lblValue2;
        private Going.UI.Forms.ImageCanvas.IcLabel lblTitle;
    }
}
