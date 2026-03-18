using Going.UI.Design;
using Going.UI.Controls;

namespace TspMonitor.Windows
{
    public partial class SettingWindow : GoWindow
    {
        public SettingWindow()
        {
            InitializeComponent();

            btnClose.ButtonClicked += (o, s) => Close();
        }

        protected override void OnUpdate()
        {
            var d = Main.DevMgr?.Data;
            if (d == null) { base.OnUpdate(); return; }

            lblWndScrTemp.Text = $"{d.ScrTemp:F1}\u00B0C";
            lblWndIgbtTemp.Text = $"{d.IgbtTemp:F1}\u00B0C";
            lblWndTransTemp.Text = $"{d.TransTemp:F1}\u00B0C";
            lblWndInBrkTemp.Text = $"{d.InBreakerTemp:F1}\u00B0C";
            lblWndOutBrkTemp.Text = $"{d.OutBreakerTemp:F1}\u00B0C";
            lblWndEdlcTemp.Text = $"{d.EdlcTemp:F1}\u00B0C";
            lblWndSmokeVal.Text = $"{d.Smoke1:F1}%";

            lblInstallDate.Text = Main.DataMgr.Setting.InstallDate;
            lblModelNo.Text = Main.DataMgr.Setting.ModelNo.ToString();
            lblCorrR.Text = $"{d.CorrectionR:F1}A";
            lblCorrS.Text = $"{d.CorrectionS:F1}A";
            lblCorrT.Text = $"{d.CorrectionT:F1}A";
            lblInR.Text = $"{d.RCurrent:F1}";
            lblInS.Text = $"{d.SCurrent:F1}";
            lblInT.Text = $"{d.TCurrent:F1}";

            base.OnUpdate();
        }
    }
}
