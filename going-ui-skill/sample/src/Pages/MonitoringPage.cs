using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.ImageCanvas;
using SenvasSample.Datas;

namespace SenvasSample.Pages
{
    public partial class MonitoringPage : GoPage
    {
        private List<HistoryItem> historyItems = new List<HistoryItem>();
        private int prevAlarmStatus = 0;
        private int prevInputStatus = 0;
        private int prevOutputStatus = 0;
        private bool isInitialized = false;

        public MonitoringPage()
        {
            InitializeComponent();

            // DataGrid column setup
            dgHistory.Columns.Clear();
            dgHistory.Columns.Add(new GoDataGridLabelColumn { Name = "Time", HeaderText = "Time", Size = "45%" });
            dgHistory.Columns.Add(new GoDataGridLabelColumn { Name = "Status", HeaderText = "Status", Size = "55%" });

            // Settings button -> open CalibrationWindow
            btnSettings.ButtonClicked += (o, s) =>
            {
                Main.Window.CalibrationWindow.ShowCalibration();
            };
        }

        protected override void OnUpdate()
        {
            var data = Main.DevMgr.Data;
            bool connected = Main.DevMgr.IsConnected;

            #region Temperature values
            lblScrTemp.Text = connected ? $"{data.SCR_Temp:F1}°C" : "---";
            lblIgbtTemp.Text = connected ? $"{data.IGBT_Temp:F1}°C" : "---";
            lblTransTemp.Text = connected ? $"{data.Trans_Temp:F1}°C" : "---";
            lblInBreakerTemp.Text = connected ? $"{data.InBreaker_Temp:F1}°C" : "---";
            lblOutBreakerTemp.Text = connected ? $"{data.OutBreaker_Temp:F1}°C" : "---";
            lblEdlcTemp.Text = connected ? $"{data.EDLC_Temp:F1}°C" : "---";
            lblSmokeLevel.Text = connected ? $"{data.Smoke1}%" : "---";
            #endregion

            #region Warning 1st values
            lblScrW1.Text = connected ? $"{data.SCR_TempSet1:F1}°C" : "---";
            lblIgbtW1.Text = connected ? $"{data.IGBT_TempSet1:F1}°C" : "---";
            lblTransW1.Text = connected ? $"{data.Trans_TempSet1:F1}°C" : "---";
            lblInBreakerW1.Text = connected ? $"{data.InBreaker_TempSet1:F1}°C" : "---";
            lblOutBreakerW1.Text = connected ? $"{data.OutBreaker_TempSet1:F1}°C" : "---";
            lblEdlcW1.Text = connected ? $"{data.EDLC_TempSet1:F1}°C" : "---";
            lblSmokeW1.Text = connected ? $"{data.Smoke1Set1}%" : "---";
            #endregion

            #region Warning 2nd values
            lblScrW2.Text = connected ? $"{data.SCR_TempSet2:F1}°C" : "---";
            lblIgbtW2.Text = connected ? $"{data.IGBT_TempSet2:F1}°C" : "---";
            lblTransW2.Text = connected ? $"{data.Trans_TempSet2:F1}°C" : "---";
            lblInBreakerW2.Text = connected ? $"{data.InBreaker_TempSet2:F1}°C" : "---";
            lblOutBreakerW2.Text = connected ? $"{data.OutBreaker_TempSet2:F1}°C" : "---";
            lblEdlcW2.Text = connected ? $"{data.EDLC_TempSet2:F1}°C" : "---";
            lblSmokeW2.Text = connected ? $"{data.Smoke1Set2}%" : "---";
            #endregion

            #region Sensor Input lamps
            lampLeak.OnOff = connected && data.IsLeak;
            lampFuse.OnOff = connected && data.IsFuse;
            lampFan1.OnOff = connected && data.IsFan1;
            lampFan2.OnOff = connected && data.IsFan2;
            #endregion

            #region MCCB Output lamps
            lampInverter.OnOff = connected && data.IsInverter;
            lampInput.OnOff = connected && data.IsInput;
            lampOutput.OnOff = connected && data.IsOutput;
            #endregion

            #region History logging
            if (connected && isInitialized)
            {
                // Alarm status change
                if (data.AlarmStatus != prevAlarmStatus)
                {
                    AddHistory(data.GetAlarmText());
                    prevAlarmStatus = data.AlarmStatus;
                }

                // Input status change
                if (data.InputStatus != prevInputStatus)
                {
                    var msgs = new List<string>();
                    if (data.IsLeak) msgs.Add("LEAK");
                    if (data.IsFuse) msgs.Add("FUSE");
                    if (data.IsFan1) msgs.Add("FAN1");
                    if (data.IsFan2) msgs.Add("FAN2");
                    AddHistory("Input: " + (msgs.Count > 0 ? string.Join(",", msgs) : "Clear"));
                    prevInputStatus = data.InputStatus;
                }

                // Output status change
                if (data.OutputStatus != prevOutputStatus)
                {
                    var msgs = new List<string>();
                    if (data.IsInverter) msgs.Add("INV");
                    if (data.IsInput) msgs.Add("IN");
                    if (data.IsOutput) msgs.Add("OUT");
                    AddHistory("Output: " + (msgs.Count > 0 ? string.Join(",", msgs) : "Clear"));
                    prevOutputStatus = data.OutputStatus;
                }
            }

            if (connected && !isInitialized)
            {
                prevAlarmStatus = data.AlarmStatus;
                prevInputStatus = data.InputStatus;
                prevOutputStatus = data.OutputStatus;
                isInitialized = true;
                AddHistory("System Started");
            }
            #endregion

            base.OnUpdate();
        }

        private void AddHistory(string status)
        {
            historyItems.Insert(0, new HistoryItem
            {
                Time = DateTime.Now.ToString("MM-dd HH:mm:ss"),
                Status = status
            });

            // Keep max 100 items
            if (historyItems.Count > 100)
                historyItems.RemoveAt(historyItems.Count - 1);

            dgHistory.SetDataSource(historyItems);
        }
    }
}
