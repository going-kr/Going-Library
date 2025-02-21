using Going.UI.Forms.Dialogs;
using Going.UI.Forms.Input;

namespace SampleForms
{
    public partial class FormMain : GoForm
    {
        public FormMain()
        {
            InitializeComponent();

            goListBox1.SelectionMode = Going.UI.Enums.GoItemSelectionMode.MultiPC;
            for (int i = 1; i <= 100; i++)
                goListBox1.Items.Add(new Going.UI.Datas.GoListItem { Text = $"¾ÆÀÌÅÛ {i}" });

            goInputBoolean1.ValueChanged += (o, s) => goValueBoolean1.Value = goInputBoolean1.Value;

            goInputCombo1.Items.AddRange(goListBox1.Items);
        }
    }
}
