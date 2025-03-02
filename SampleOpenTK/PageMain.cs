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
        GoLamp lmp;
        GoGauge gauge;
        GoMeter meter;

        public PageMain()
        {
            Name = "PageMain";

            var tbl = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(10) };
            tbl.Columns = ["60%", "40%"];
            tbl.Rows = ["60%", "40%"];
            Childrens.Add(tbl);

            var pnl = new GoPanel { Fill = true, IconString = "fa-check", ButtonWidth = 60, IconSize = 14 };
            pnl.Buttons.Add(new GoButtonItem { Name = "add", IconString = "fa-plus", Size = "50%" });
            pnl.Buttons.Add(new GoButtonItem { Name = "del", IconString = "fa-minus", Size = "50%" });
            tbl.Childrens.Add(pnl, 0, 0);


            var pnl_tbl = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(5) };
            pnl_tbl.Columns = ["100%", "150px", "150px"];
            pnl_tbl.Rows = ["10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%"];
            pnl.Childrens.Add(pnl_tbl);

            lmp = new GoLamp { Fill = true, Margin = new GoPadding(3), Text = "램프" };

            pnl_tbl.Childrens.Add(new GoLabel { Fill = true, Margin = new GoPadding(3), LabelColor = "Base3", Text = "동해물과 백두산이 마르고 닳도록 \r\n하나님이 보우하사 우리 나라 만세\r\n무궁화 삼천리 화려강산 \r\n대한 사람 대한으로 길이 보전 하세", BorderOnly = true, ContentAlignment = GoContentAlignment.TopLeft, TextPadding = new GoPadding(10) }, 0, 0, 1, 10);
            pnl_tbl.Childrens.Add(new GoButton { Fill = true, Margin = new GoPadding(3), Text = "테스트" }, 1, 0);
            pnl_tbl.Childrens.Add(new GoButton { Fill = true, Margin = new GoPadding(3), Text = "탭", }, 1, 1);
            pnl_tbl.Childrens.Add(new GoButton { Fill = true, Margin = new GoPadding(3), Text = "이미지 캔바스", }, 2, 0);
            pnl_tbl.Childrens.Add(new GoInputString { Fill = true, Margin = new GoPadding(3), TitleSize = 90, Title = "문자열 1", Value = "Test1", ButtonSize = 80, Buttons = [new() { IconString = "fa-download", Size = "50%" }, new() { IconString = "fa-upload", Size = "50%" }] }, 1, 2, 2, 1);
            pnl_tbl.Childrens.Add(new GoInputString { Fill = true, Margin = new GoPadding(3), TitleSize = 90, Title = "문자열 2", Value = "Test2", ButtonSize = 80, Buttons = [new() { IconString = "fa-download", Size = "50%" }, new() { IconString = "fa-upload", Size = "50%" }] }, 1, 3, 2, 1);
            pnl_tbl.Childrens.Add(new GoInputBoolean { Fill = true, Margin = new GoPadding(3), TitleSize = 90, Title = "스위치", Value = false, ButtonSize = 80, Buttons = [new() { IconString = "fa-download", Size = "50%" }, new() { IconString = "fa-upload", Size = "50%" }] }, 1, 4, 2, 1);
            pnl_tbl.Childrens.Add(new GoInputCombo { Fill = true, Margin = new GoPadding(3), TitleSize = 90, Title = "콤보", SelectedIndex = 0, ButtonSize = 80, Buttons = [new() { IconString = "fa-download", Size = "50%" }, new() { IconString = "fa-upload", Size = "50%" }] }, 1, 5, 2, 1);
            pnl_tbl.Childrens.Add(new GoButtons { Fill = true, Margin = new GoPadding(3), Mode = GoButtonsMode.Radio, Buttons = [new() { Text = "A", Size = "25%" }, new() { Text = "B", Size = "25%" }, new() { Text = "C", Size = "25%" }, new() { Text = "None", Size = "25%" }] }, 1, 6, 2, 1);
            pnl_tbl.Childrens.Add(lmp, 1, 7, 2, 1);
            pnl_tbl.Childrens.Add(new GoInputSelector { Fill = true, Margin = new GoPadding(3) , Items = [new GoListItem { Text = "Sun" }, new GoListItem { Text = "Mon" }, new GoListItem { Text = "Tue" }, new GoListItem { Text = "Wed" }, new GoListItem { Text = "Thu" }, new GoListItem { Text = "Fri" }, new GoListItem { Text = "Sat" },] }, 1, 8, 2, 1);
            pnl_tbl.Childrens.Add(new GoSlider { Fill = true, Margin = new GoPadding(3), Text = "슬라이더", Direction = GoDirectionHV.Horizon, ValueFormat = "0.0", Tick = 10 }, 1, 9, 2);

            var lb = new GoListBox { Fill = true, SelectionMode = GoItemSelectionMode.Multi };
            var cmb = pnl_tbl.Childrens.FirstOrDefault(x => x is GoInputCombo) as GoInputCombo;
            tbl.Childrens.Add(lb, 1, 0);
            for (int i = 1; i <= 100; i++)
            {
                lb.Items.Add(new() { Text = $"테스트 {i}" });
                cmb?.Items.Add(new() { Text = $"테스트 {i}" });
            }

            var btn1 = pnl_tbl.Childrens.FirstOrDefault(x => x is GoButton btn && btn.Text == "테스트") as GoButton;
            var btn2 = pnl_tbl.Childrens.FirstOrDefault(x => x is GoButton btn && btn.Text == "탭") as GoButton;
            var btn3 = pnl_tbl.Childrens.FirstOrDefault(x => x is GoButton btn && btn.Text == "이미지 캔바스") as GoButton;
            if (btn1 != null) btn1.ButtonClicked += (o, s) => Design?.SetPage("PageTest");
            if (btn2 != null) btn2.ButtonClicked += (o, s) => Design?.SetPage("PageTab");
            if (btn3 != null) btn3.ButtonClicked += (o, s) => Design?.SetPage("main");

            var btns = pnl_tbl.Childrens.FirstOrDefault(x => x is GoButtons btns ) as GoButtons;



            var pnl_tbl2 = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(5) };
            pnl_tbl2.Columns = ["33.33%", "33.34%", "33.33%"];
            pnl_tbl2.Rows = ["100%"];
            tbl.Childrens.Add(pnl_tbl2, 0, 1, 2, 1);

            gauge = new GoGauge { Fill = true, Format = "0 '%'", Title = "Speed", FontSize = 24, TitleFontSize = 12 };
            pnl_tbl2.Childrens.Add(gauge, 0, 0);

            meter = new GoMeter { Fill = true, Format = "0 '%'", Title = "Speed", FontSize = 24, TitleFontSize = 12 };
            pnl_tbl2.Childrens.Add(meter, 1, 0);

            var knob = new GoKnob { Fill = true, Format = "0 '%'" };
            pnl_tbl2.Childrens.Add(knob, 2, 0);
        }

        DateTime dt = DateTime.Now;
        protected override void OnUpdate()
        {
            lmp.OnOff = DateTime.Now.Second % 2 == 0;
            meter.Value = gauge.Value = ((DateTime.Now - dt).TotalMilliseconds / 100) % 100;

            base.OnUpdate();
        }

    }
}
