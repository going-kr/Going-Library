using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SampleOpenTK
{
    public class PageMain : GoPage
    {
        public PageMain()
        {
            Name = "PageMain";

            var pnl = new GoPanel { Left = 20, Top = 20, Width = 450, Height = 300, IconString = "fa-check", ButtonWidth = 60, IconSize = 14 };
            pnl.Buttons.Add(new GoButtonInfo { Name = "add", IconString = "fa-plus", Size = "50%" });
            pnl.Buttons.Add(new GoButtonInfo { Name = "del", IconString = "fa-minus", Size = "50%" });

            Childrens.Add(pnl);
            pnl.Childrens.Add(new GoLabel { Left = 10, Top = 50, Width = 120, Height = 140, LabelColor = "Base3", Text = "동해물과 백두산이 마르고 닳도록 \r\n하나님이 보우하사 우리 나라 만세\r\n무궁화 삼천리 화려강산 \r\n대한 사람 대한으로 길이 보전 하세", BorderOnly = true, ContentAlignment = GoContentAlignment.TopLeft, TextPadding = new GoPadding(10) });
            pnl.Childrens.Add(new GoButton { Left = 140, Top = 50, Width = 90, Height = 40, IconString = "fa-check", Text = "테스트 1" });
            pnl.Childrens.Add(new GoButton { Left = 240, Top = 50, Width = 90, Height = 40, IconString = "fa-ban", Text = "테스트 2", ButtonColor = "danger" });
            pnl.Childrens.Add(new GoInputString { Left = 140, Top = 100, Width = 300, Height = 40, TitleSize = 90, Title = "문자열 1", Value = "Test1", ButtonSize = 80, Buttons = [new GoButtonInfo { IconString = "fa-download", Size = "50%" }, new GoButtonInfo { IconString = "fa-upload", Size = "50%" }] });
            pnl.Childrens.Add(new GoInputString { Left = 140, Top = 150, Width = 300, Height = 40, TitleSize = 90, Title = "문자열 2", Value = "Test2", ButtonSize = 80, Buttons = [new GoButtonInfo { IconString = "fa-download", Size = "50%" }, new GoButtonInfo { IconString = "fa-upload", Size = "50%" }] });
            pnl.Childrens.Add(new GoInputBoolean { Left = 140, Top = 200, Width = 300, Height = 40, TitleSize = 90, Title = "스위치", Value = false, ButtonSize = 80, Buttons = [new GoButtonInfo { IconString = "fa-download", Size = "50%" }, new GoButtonInfo { IconString = "fa-upload", Size = "50%" }] });
            pnl.Childrens.Add(new GoInputCombo { Left = 140, Top = 250, Width = 300, Height = 40, TitleSize = 90, Title = "콤보", SelectedIndex = 0, ButtonSize = 80, Buttons = [new GoButtonInfo { IconString = "fa-download", Size = "50%" }, new GoButtonInfo { IconString = "fa-upload", Size = "50%" }] });


            Childrens.Add(new GoInputString { Left = 20, Top = 330, Width = 300, Height = 40, TitleSize = 90, Title = "입력 1", });
            Childrens.Add(new GoInputString { Left = 20, Top = 380, Width = 300, Height = 40, TitleSize = 90, Title = "입력 2", });
            Childrens.Add(new GoInputString { Left = 20, Top = 430, Width = 300, Height = 40, TitleSize = 90, Title = "입력 3", });
            Childrens.Add(new GoInputString { Left = 20, Top = 480, Width = 300, Height = 40, TitleSize = 90, Title = "입력 4", });

            Childrens.Add(new GoInputInteger { Left = 330, Top = 330, Width = 300, Height = 40, TitleSize = 90, Title = "정수", });
            Childrens.Add(new GoInputInteger { Left = 330, Top = 380, Width = 300, Height = 40, TitleSize = 90, Title = "정수\r\n( 0 ~ 100)", Minimum = 0, Maximum = 100 });
            Childrens.Add(new GoInputFloat { Left = 330, Top = 430, Width = 300, Height = 40, TitleSize = 90, Title = "실수", });
            Childrens.Add(new GoInputFloat { Left = 330, Top = 480, Width = 300, Height = 40, TitleSize = 90, Title = "실수\r\n( -20 ~ 50 )", Minimum = -20, Maximum = 100 });

            var lb = new GoListBox { Left = 480, Top = 20, Width = 450, Height = 300, SelectionMode = GoItemSelectionMode.Multi };
            var cmb = pnl.Childrens.FirstOrDefault(x => x is GoInputCombo) as GoInputCombo;
            Childrens.Add(lb);
            for (int i = 1; i <= 100; i++)
            {
                lb.Items.Add(new() { Text = $"테스트 {i}" });
                cmb?.Items.Add(new() { Text = $"테스트 {i}" });
            }

            var btn = pnl.Childrens.FirstOrDefault(x => x is GoButton btn && btn.Text == "테스트 1") as GoButton;
            if (btn != null) btn.ButtonClicked += (o, s) =>
            Design?.SetPage("PageTest");

        }

    }
}
