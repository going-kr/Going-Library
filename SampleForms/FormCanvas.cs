using Going.UI.Forms.Dialogs;
using Going.UI.Forms.ImageCanvas;
using Going.UI.Forms.Input;
using Going.UI.Themes;
using System.Globalization;

namespace SampleForms
{
    public partial class FormCanvas : GoForm
    {
        DateTime prev = DateTime.Now;
        System.Windows.Forms.Timer tmr;
        int sec;

        public FormCanvas()
        {
            InitializeComponent();

            tmr = new System.Windows.Forms.Timer { Interval = 10, Enabled = true };
            tmr.Tick += (o, s) =>
            {
                var now = DateTime.Now;
                if (now.Second != sec)
                {
                    var idx = Convert.ToInt32((now - prev).TotalSeconds) % 8;
                    //onoffs[idx].OnOff = !onoffs[idx].OnOff;
                    sec = now.Second;

                    M_lblTime.Text = DateTime.Now.ToString("yyyy/MM/dd (ddd) HH:mm:ss", new CultureInfo("ko-KR"));

                    for (int i = 1; i <= 8; i++)
                    {
                        var box = main.Controls[$"box{i}"] as Box;
                        if (box != null)
                        {
                            box.IconState = sec % 3;
                            box.Alarm = sec % 2 == 0;
                            box.Run = sec % 2 == 1;
                        }
                    }
                }

                T_prgs.Value = (DateTime.Now - prev).TotalMilliseconds / 10 % 100;
            };


            M_btnSchedule.ButtonClicked += (o, s) => ic.SelectedTab = schedule;
            M_btnAlarm.ButtonClicked += (o, s) => ic.SelectedTab = test;
            S_btnMain.ButtonClicked += (o, s) => ic.SelectedTab = main;
            C_btnMain.ButtonClicked += (o, s) => ic.SelectedTab = main;
            T_btnMain.ButtonClicked += (o, s) => ic.SelectedTab = main;

            for (int i = 1; i <= 8; i++)
            {
                var box = main.Controls[$"box{i}"] as Box;
                if (box != null) box.SettingButtonClick += (o, s) => ic.SelectedTab = setting;
                
            }

        }
    }
}
