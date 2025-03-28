using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Dialogs;
using Going.UI.Enums;
using Going.UI.Json;
using Going.UI.Tools;
using Going.UI.Utils;
using OpenTK.Graphics.ES30;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SampleOpenTK.Pages
{
    public class PageMain : GoPage
    {
        GoMessageBox msgbox;
        GoSelectorBox selbox;
        GoInputBox inbox;

        GoGridLayoutPanel grid;

        public PageMain()
        {
            Name = "PageMain";
            var btnMB = new GoButton { Left = 10, Top = 10, Width = 120, Height = 40, Text = "MessageBox" };
            var btnSB = new GoButton { Left = 10, Top = 60, Width = 120, Height = 40, Text = "SelectorBox" };
            var btnIB = new GoButton { Left = 10, Top = 110, Width = 120, Height = 40, Text = "InputBox" };
            Childrens.Add(btnMB);
            Childrens.Add(btnSB);
            Childrens.Add(btnIB);

            msgbox = new GoMessageBox { };
            selbox = new GoSelectorBox { };
            inbox = new GoInputBox { };
            btnMB.ButtonClicked += (o, s) => msgbox.ShowMessageBoxOk("테스트", "이것은 메시지박스 테스트 화면 입니다.\r\n( Test line )", (result) => { Console.WriteLine(result); });
            btnSB.ButtonClicked += (o, s) =>
            {
                var items = Enum.GetValues<DayOfWeek>().Select(x => new GoListItem { Text = x.ToString(), Tag = x }).ToList();
                var sels = new List<GoListItem>([items[0], items[2]]);
                //selbox.ShowCheck("테스트", 2, items, sels, (result) => { if (result != null) foreach (var v in result) Console.WriteLine(v.Text); });
                //selbox.ShowRadio("테스트", 2, items, items[0], (result) => { if (result != null) Console.WriteLine(result.Text); });
                //selbox.ShowCombo("테스트", items, items[0], (result) => { if (result != null) Console.WriteLine(result.Text); });
                //selbox.ShowCheck<DayOfWeek>("테스트", 2, [DayOfWeek.Monday], (result) => { if (result != null) foreach (var v in result) Console.WriteLine(v); });
            };
            btnIB.ButtonClicked += (o, s) =>
            {
                //inbox.ShowString("테스트", null, (result) => { Console.WriteLine(result); });
                //inbox.ShowBool("테스트", (result) => { Console.WriteLine(result); });
                inbox.Showinputbox<Data>("테스트", (result) => { if (result != null) Console.WriteLine(JsonSerializer.Serialize(result)); });
            };

            grid = new GoGridLayoutPanel { Left = 140, Top = 10, Width = 300, Height = 400 };
            grid.AddRow("25%", ["25%", "25%", "25%", "25%"]);
            grid.AddRow("25%", ["25%", "25%", "25%", "25%"]);
            grid.AddRow("25%", ["25%", "25%", "25%", "25%"]);
            grid.AddRow("25%", ["25%", "25%", "25%", "25%"]);
            grid.AddRow("40px", ["100%", "80px", "80px"]);
            Childrens.Add(grid);

            grid.Childrens.Add(new GoButton { Fill = true, Text = "OK" }, 1, 4);
            grid.Childrens.Add(new GoButton { Fill = true, Text = "Cancel" }, 2, 4);

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    grid.Childrens.Add(new GoLabel { Fill = true, Text = $"{i * 4 + j}" }, j, i);

        }
    }

    class Data
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public double Value2 { get; set; }
        public bool Value3 { get; set; }
        public DayOfWeek DOW { get; set; }
        [InputBoxIgnore] public DayOfWeek DOW2 { get; set; }
    }
}
