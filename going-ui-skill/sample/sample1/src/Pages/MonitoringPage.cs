using Going.UI.Design;
using Going.UI.Controls;

namespace TspMonitor.Pages
{
    public partial class MonitoringPage : GoPage
    {
        public MonitoringPage()
        {
            InitializeComponent();

            btnSettings.ButtonClicked += (o, s) =>
            {
                Main.Window.SettingWindow.Show();
            };
        }

        protected override void OnUpdate()
        {
            var d = Main.DevMgr?.Data;
            if (d == null) { base.OnUpdate(); return; }

            var connected = Main.DevMgr!.IsConnected;

            // 온도 현재값
            lblScrTemp.Text = connected ? $"{d.ScrTemp:F1}\u00B0C" : "---";
            lblIgbtTemp.Text = connected ? $"{d.IgbtTemp:F1}\u00B0C" : "---";
            lblTransTemp.Text = connected ? $"{d.TransTemp:F1}\u00B0C" : "---";
            lblInBrkTemp.Text = connected ? $"{d.InBreakerTemp:F1}\u00B0C" : "---";
            lblOutBrkTemp.Text = connected ? $"{d.OutBreakerTemp:F1}\u00B0C" : "---";
            lblEdlcTemp.Text = connected ? $"{d.EdlcTemp:F1}\u00B0C" : "---";
            lblSmokeVal.Text = connected ? $"{d.Smoke1:F1}%" : "---";

            // Warning 설정값
            lblScrWarn1.Text = $"{d.ScrTempSet1:F1}\u00B0C";
            lblScrWarn2.Text = $"{d.ScrTempSet2:F1}\u00B0C";
            lblIgbtWarn1.Text = $"{d.IgbtTempSet1:F1}\u00B0C";
            lblIgbtWarn2.Text = $"{d.IgbtTempSet2:F1}\u00B0C";
            lblTransWarn1.Text = $"{d.TransTempSet1:F1}\u00B0C";
            lblTransWarn2.Text = $"{d.TransTempSet2:F1}\u00B0C";
            lblInBrkWarn1.Text = $"{d.InBreakerTempSet1:F1}\u00B0C";
            lblInBrkWarn2.Text = $"{d.InBreakerTempSet2:F1}\u00B0C";
            lblOutBrkWarn1.Text = $"{d.OutBreakerTempSet1:F1}\u00B0C";
            lblOutBrkWarn2.Text = $"{d.OutBreakerTempSet2:F1}\u00B0C";
            lblEdlcWarn1.Text = $"{d.EdlcTempSet1:F1}\u00B0C";
            lblEdlcWarn2.Text = $"{d.EdlcTempSet2:F1}\u00B0C";
            lblSmokeWarn1.Text = $"{d.Smoke1Set1:F1}%";
            lblSmokeWarn2.Text = $"{d.Smoke1Set2:F1}%";

            // SENSOR INPUT
            lampCoolingLeak.OnOff = d.SensorLeak;
            lampDcFuse.OnOff = d.SensorFuse;
            lampCoolingFan1.OnOff = d.SensorFan1;
            lampCoolingFan2.OnOff = d.SensorFan2;

            // MCCB OUTPUT
            lampInverter.OnOff = d.MccbInverter;
            lampInput.OnOff = d.MccbInput;
            lampOutput.OnOff = d.MccbOutputBit;

            base.OnUpdate();
        }
    }
}
