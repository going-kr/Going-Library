using Going.UI.Forms.Dialogs;
using Going.UI.Forms.Input;
using Going.UI.Themes;

namespace SampleForms
{
    public partial class FormMain : GoForm
    {
        DateTime prev = DateTime.Now;
        System.Windows.Forms.Timer tmr;
        public FormMain()
        {
            InitializeComponent();

            goListBox1.SelectionMode = Going.UI.Enums.GoItemSelectionMode.MultiPC;
            for (int i = 1; i <= 100; i++)
                goListBox1.Items.Add(new Going.UI.Datas.GoListItem { Text = $"¾ÆÀÌÅÛ {i}" });

            goInputBoolean1.ValueChanged += (o, s) => goValueBoolean1.Value = goInputBoolean1.Value;

            goInputCombo1.Items.AddRange(goListBox1.Items);

            tmr = new System.Windows.Forms.Timer { Interval = 10, Enabled = true };
            tmr.Tick += (o, s) =>
            {
                goMeter1.Value = goGauge1.Value = (DateTime.Now - prev).TotalMilliseconds / 100 % 100;
            };

        }
    }
}
