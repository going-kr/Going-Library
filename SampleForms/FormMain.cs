using Going.UI.Forms.Dialogs;
using Going.UI.Forms.ImageCanvas;
using Going.UI.Forms.Input;
using Going.UI.Themes;

namespace SampleForms
{
    public partial class FormMain : GoForm
    {
        DateTime prev = DateTime.Now;
        System.Windows.Forms.Timer tmr;
        int sec;

        IcOnOff[] onoffs;
        public FormMain()
        {
            InitializeComponent();
            ClientSize = new Size(800, 480);

            onoffs = [icOnOff1, icOnOff2, icOnOff3, icOnOff4, icOnOff5, icOnOff6, icOnOff7, icOnOff8];

            tmr = new System.Windows.Forms.Timer { Interval = 10, Enabled = true };
            tmr.Tick += (o, s) =>
            {
                var now = DateTime.Now;
                if (now.Second != sec)
                {
                    var idx = Convert.ToInt32((now - prev).TotalSeconds) % 8;
                    onoffs[idx].OnOff = !onoffs[idx].OnOff;
                    sec = now.Second;
                }
            };
        }
    }
}
