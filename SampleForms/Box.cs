using Going.UI.Forms.ImageCanvas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleForms
{
    public partial class Box : IcContainer
    {
        public string Title { get => lblTitle.Text; set => lblTitle.Text = value; }
        public string Value1 { get => lblValue1.Text; set => lblValue1.Text = value; }
        public string Value2 { get => lblValue2.Text; set => lblValue2.Text = value; }
        public int IconState { get => stateIco.State; set => stateIco.State = value; }
        public bool Alarm { get => ooAlm.OnOff; set => ooAlm.OnOff = value; }
        public bool Run { get => ooRun.OnOff; set => ooRun.OnOff = value; }

        public event EventHandler? ToggleChanged;
        public event EventHandler? SettingButtonClick;
        public Box()
        {
            InitializeComponent();

            ooSw.ValueChanged += (o, s) => ToggleChanged?.Invoke(this, EventArgs.Empty);
            btnSetting.ButtonClicked += (o, s) => SettingButtonClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
