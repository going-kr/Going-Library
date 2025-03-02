using Going.UI.Enums;
using Going.UI.ImageCanvas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK
{
    public class Box : IcContainer
    {
        public string Title { get => lblTitle.Text; set => lblTitle.Text = value; }
        public string Value1 { get => lblValue1.Text; set => lblValue1.Text = value; }
        public string Value2 { get => lblValue2.Text; set => lblValue2.Text = value; }
        public int IconState { get => stateIco.State; set => stateIco.State = value; }
        public bool Alarm { get => ooAlm.OnOff; set => ooAlm.OnOff = value; }
        public bool Run { get => ooRun.OnOff; set => ooRun.OnOff = value; }

        public event EventHandler? ToggleChanged;
        public event EventHandler? SettingButtonClick;

        IcButton btnSetting;
        IcOnOff ooAlm, ooRun, ooSw;
        IcLabel lblValue1, lblValue2, lblTitle;
        IcState stateIco;

        public Box()
        {
            ContainerImage = "box";

            btnSetting = new IcButton { Left = 124, Top = 8, Width = 53, Height = 33, Text = "" };
            ooAlm = new IcOnOff { Left = 16, Top = 8, Width = 48, Height = 33, Text = "" };
            ooRun = new IcOnOff { Left = 70, Top = 8, Width = 48, Height = 33, Text = "" };
            ooSw = new IcOnOff { Left = 16, Top = 56, Width = 74, Height = 50, Text = "", ToggleMode = true };
            lblValue1 = new IcLabel { Left = 16, Top = 115, Width = 74, Height = 24, Text = "0.0", FontName = "나눔바른고딕", FontSize = 14, FontStyle = GoFontStyle.Bold, TextColor = "black" };
            lblValue2 = new IcLabel { Left = 100, Top = 115, Width = 70, Height = 24, Text = "0.0", FontName = "나눔바른고딕", FontSize = 14, FontStyle = GoFontStyle.Bold, TextColor = "black" };
            lblTitle = new IcLabel { Left = 16, Top = 148, Width = 154, Height = 24, Text = "", FontName = "나눔바른고딕", FontSize = 12, FontStyle = GoFontStyle.Bold, TextColor = "black" };
            stateIco = new IcState { Left = 100, Top = 58, Width = 70, Height = 48, StateImage = "icon" };

            Childrens.Add(btnSetting);
            Childrens.Add(ooAlm);
            Childrens.Add(ooRun);
            Childrens.Add(ooSw);
            Childrens.Add(lblValue1);
            Childrens.Add(lblValue2);
            Childrens.Add(lblTitle);
            Childrens.Add(stateIco);

            ooSw.ValueChanged += (o, s) => ToggleChanged?.Invoke(this, EventArgs.Empty);
            btnSetting.ButtonClicked += (o, s) => SettingButtonClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
