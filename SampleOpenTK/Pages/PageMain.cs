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

                AlarmSearchBox.Default.ShowSearchBox("알람", DateTime.Now.Date, DateTime.Now, (b, e) =>
                {


                });


            };
            /*
            btnIB.ButtonClicked += (o, s) =>
            {
                //inbox.ShowString("테스트", null, (result) => { Console.WriteLine(result); });
                //inbox.ShowBool("테스트", (result) => { Console.WriteLine(result); });
                inbox.Showinputbox<Data>("테스트", (result) => { if (result != null) Console.WriteLine(JsonSerializer.Serialize(result)); });
            };
            */
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

            GoDialogs.SystemWindows.Add("AlarmSearchBox", AlarmSearchBox.Default);

            btnIB.ButtonClicked += (o, s) => { 
            };
            btnIB.MouseLongClicked += (o, s) => { 
            };
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

    #region Window
    public class AlarmSearchBox : GoWindow
    {
        public static AlarmSearchBox Default { get; } = new AlarmSearchBox();

        #region Properties
        public string OkText { get => btnOK.Text; set => btnOK.Text = value; }
        public string CancelText { get => btnCancel.Text; set => btnCancel.Text = value; }
        #endregion

        #region Member Variable
        GoGridLayoutPanel gpnl;
        GoInputDateTime inBegin, inEnd;
        GoLabel lblBegin, lblEnd;
        GoButton btnOK, btnCancel;

        Action<DateTime?, DateTime?>? callback;
        #endregion

        #region Constructor
        public AlarmSearchBox()
        {
            IconString = "fa-comment-dots";
            IconSize = 18;
            IconGap = 10;

            gpnl = new GoGridLayoutPanel { Fill = true, Margin = new GoPadding(10) };
            gpnl.AddRow("33%", ["60px", "100%"]);
            gpnl.AddRow("33%", ["60px", "100%"]);
            gpnl.AddRow("34%", ["100%", "80px", "80px"]);

            inBegin = new GoInputDateTime { DateTimeStyle = GoDateTimeKind.Date };
            inEnd = new GoInputDateTime { DateTimeStyle = GoDateTimeKind.Date };
            lblBegin = new GoLabel { Text = "시작", BackgroundDraw = false };
            lblEnd = new GoLabel { Text = "종료", BackgroundDraw = false };
            btnOK = new GoButton { Fill = true, Text = "확인" };
            btnCancel = new GoButton { Fill = true, Text = "취소" };

            gpnl.Childrens.Add(lblBegin, 0, 0); gpnl.Childrens.Add(inBegin, 1, 0);
            gpnl.Childrens.Add(lblEnd, 0, 1); gpnl.Childrens.Add(inEnd, 1, 1);
            gpnl.Childrens.Add(btnOK, 1, 2); gpnl.Childrens.Add(btnCancel, 2, 2);

            Childrens.Add(gpnl);

            btnOK.ButtonClicked += (o, s) => { Close(); callback?.Invoke(inBegin.Value, inEnd.Value); };
            btnCancel.ButtonClicked += (o, s) => { Close(); callback?.Invoke(null, null); };
        }
        #endregion

        #region Override
        protected override void OnCloseButtonClick()
        {
            base.OnCloseButtonClick();
            callback?.Invoke(null, null);
        }
        #endregion

        #region Method
        public void ShowSearchBox(string title, DateTime begin, DateTime end, Action<DateTime?, DateTime?> result)
        {
            this.Text = title;
            this.callback = result;
            inBegin.Value = begin;
            inEnd.Value = end;

            Show(300, 200);
        }
        #endregion
    }
    #endregion
}
