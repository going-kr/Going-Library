using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Design;
using Going.UI.ImageCanvas;

namespace SenvasSample.Windows
{
    public partial class CalibrationWindow : GoWindow
    {
        public CalibrationWindow()
        {
            InitializeComponent();

            btnClose.ButtonClicked += (o, s) =>
            {
                Close();
            };
        }

        public void ShowCalibration()
        {
            Show();
        }

        protected override void OnUpdate()
        {
            var data = Main.DevMgr.Data;
            bool connected = Main.DevMgr.IsConnected;

            #region Left - Correction values
            lblCalScr.Text = connected ? $"{data.CorrectionTemp1:F1}°C" : "---";
            lblCalIgbt.Text = connected ? $"{data.CorrectionTemp2:F1}°C" : "---";
            lblCalTrans.Text = connected ? $"{data.CorrectionTemp3:F1}°C" : "---";
            lblCalInBreaker.Text = connected ? $"{data.CorrectionTemp4:F1}°C" : "---";
            lblCalOutBreaker.Text = connected ? $"{data.CorrectionTemp5:F1}°C" : "---";
            lblCalEdlc.Text = connected ? $"{data.CorrectionTemp6:F1}°C" : "---";
            lblCalSmoke.Text = connected ? $"{data.CorrectionSmoke1}%" : "---";
            #endregion

            #region Right - Equipment info
            lblInstallDate.Text = Main.DataMgr.Setting.InstallDate;
            lblModelNo.Text = Main.DataMgr.Setting.ModelNumber.ToString();
            #endregion

            #region Right - Current correction values
            lblCorrR.Text = connected ? $"{data.CorrectionCurrent1:F1}A" : "---";
            lblCorrS.Text = connected ? $"{data.CorrectionCurrent2:F1}A" : "---";
            lblCorrT.Text = connected ? $"{data.CorrectionCurrent3:F1}A" : "---";
            #endregion

            #region Right - Current input values
            lblInR.Text = connected ? $"{data.R_Current:F1}A" : "---";
            lblInS.Text = connected ? $"{data.S_Current:F1}A" : "---";
            lblInT.Text = connected ? $"{data.T_Current:F1}A" : "---";
            #endregion

            base.OnUpdate();
        }
    }
}
