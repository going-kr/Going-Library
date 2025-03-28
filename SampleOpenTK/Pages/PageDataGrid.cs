using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Tools;
using Going.UI.Utils;
using OpenTK.Graphics.ES11;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK.Pages
{
    public class PageDataGrid : GoPage
    {
        Random rnd = new Random();
        GoDataGrid dg;
        List<Device> devs = [];
        int sec;

        public PageDataGrid()
        {
            Name = "PageDataGrid";

            dg = new GoDataGrid { Fill = true, Margin = new GoPadding(10) };
            Childrens.Add(dg);

            string[] sz1 = ["80px", "80px", "120px", "10.0%", "18.0%", "9.0%", "9.0%", "9.0%", "9.0%", "9.0%", "9.0%", "9.0%", "9.0%"];
            string[] sz2 = ["100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px"];
            var sz = sz1;

            dg.Columns.Add(new GoDataGridInputTextColumn { Name = "Name", HeaderText = "명칭", Size = sz[0], GroupName = "Info", Fixed = true, UseFilter = true });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "DeviceType", HeaderText = "타입", Size = sz[1], GroupName = "Info", Fixed = true, UseFilter = true, TextConverter = ToString });
            dg.Columns.Add(new GoDataGridInputBoolColumn { Name = "Use", HeaderText = "사용여부", Size = sz[2], GroupName = "Info", Fixed = true, OnText = "사용", OffText = "미사용" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Operation", HeaderText = "상태", Size = sz[3], GroupName = "Motor"});
            dg.Columns.Add(new GoDataGridInputTimeColumn { Name = "ReserveTime", HeaderText = "예약 시간", Size = sz[4], GroupName = "Motor", DateFormat = "yyyy.MM.dd", DateTimeStyle = GoDateTimeKind.Date });
            //dg.Columns.Add(new GoDataGridInputColorColumn { Name = "LightColor", HeaderText = "색상", Size = sz[4], GroupName = "Motor" });
            //dg.Columns.Add(new GoDataGridInputComboColumn { Name = "DOW", HeaderText = "요일", Size = sz[4], GroupName = "Motor", Items = Enum.GetValues<DayOfWeek>().Select(x => new GoDataGridInputComboItem { Text = x.ToString(), Value = x }).ToList() });
            dg.Columns.Add(new GoDataGridInputNumberColumn<double> { Name = "Position", HeaderText = "위치", Size = sz[5], GroupName = "Motor", Minimum = 0, Maximum = 2000, FormatString = "0.0" });
            dg.Columns.Add(new GoDataGridInputNumberColumn<double> { Name = "Speed", HeaderText = "속도", Size = sz[6], GroupName = "Motor", Minimum = 0, Maximum = 100, FormatString = "0.0" });
            dg.Columns.Add(new GoDataGridNumberColumn<double> { Name = "Current", HeaderText = "전류", Size = sz[7], GroupName = "Sensor", FormatString = "0.0" });
            dg.Columns.Add(new GoDataGridNumberColumn<double> { Name = "Voltage", HeaderText = "전압", Size = sz[8], GroupName = "Sensor", FormatString = "0.0" });
            dg.Columns.Add(new GoDataGridNumberColumn<double> { Name = "Temp", HeaderText = "온도", Size = sz[9], GroupName = "Sensor", FormatString = "0.0", UseSort = true });
            dg.Columns.Add(new GoDataGridNumberColumn<double> { Name = "Presure", HeaderText = "압력", Size = sz[10], GroupName = "Sensor", FormatString = "0.0" });
            dg.Columns.Add(new GoDataGridLampColumn { Name = "Alarm", HeaderText = "알람", Size = sz[11], GroupName = "Warning", OnColor = "red" });
            dg.Columns.Add(new GoDataGridButtonColumn { Name = "Button", HeaderText = "리셋", Size = sz[12], GroupName = "Warning", IconString = "fa-rotate", Text = "리셋" });

            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "Info", HeaderText = "장치 정보", Fixed = true });
            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "Motor", HeaderText = "구동부", GroupName = "State" });
            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "Sensor", HeaderText = "센서부", GroupName = "State" });
            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "Warning", HeaderText = "알람", GroupName = "State" });
            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "State", HeaderText = "상태" });

            dg.SummaryRows.Add(new GoDataGridSumSummaryRow { Title = "합계", TitleColumnIndex = 0, TitleColSpan = 3,  });

            dg.SelectionMode = GoDataGridSelectionMode.Selector;
            dg.ScrollMode = ScrollMode.Vertical;

            for (int i = 0; i < 100; i++)
            {
                string nm = "";
                DeviceType tp = DeviceType.Upper;
                if (i < 70) { tp = DeviceType.Upper; nm = $"{ToString(tp)}{i + 1}"; }
                else if (i < 80) { tp = DeviceType.Under; nm = $"{ToString(tp)}{i - 70 + 1}"; }
                else if (i < 90) { tp = DeviceType.Wagon; nm = $"{ToString(tp)}{i - 80 + 1}"; }
                else if (i < 100) { tp = DeviceType.Bridge; nm = $"{ToString(tp)}{i - 90 + 1}"; }

                devs.Add(new Device { Name = nm, DeviceType = tp });
                devs[i].Position = i;
            }

            dg.SetDataSource(devs);

            dg.CellButtonClick += (o, s) =>
            {
                var dd = devs;

                Console.WriteLine(s.Cell.Row.Cells[0].Value?.ToString());
            };
        }

        protected override void OnUpdate()
        {
            var bsec = sec != DateTime.Now.Second;
            if (bsec) sec = DateTime.Now.Second;

            foreach (var dev in devs)
            {
                dev.Temp = MathTool.Constrain(dev.Temp + (((rnd.Next(0, 10) - 5.0) / 10.0)), 10, 100);
                if (bsec) dev.Alarm = rnd.Next(0, 100) / 10 == 5;
            }
           
            dg.RefreshRows();
            base.OnUpdate();
        }

        string? ToString(object? val)
        {
            var ret = "";
            if (val is DeviceType tp)
                switch (tp)
                {
                    case DeviceType.Upper: ret = "상부"; break;
                    case DeviceType.Under: ret = "하부"; break;
                    case DeviceType.Wagon: ret = "웨건"; break;
                    case DeviceType.Bridge: ret = "브릿지"; break;
                }
            return ret;
        }
    }

    public enum DeviceType { Upper, Under, Wagon, Bridge }
    public class Device
    {
        public string Name { get; set; } = "Device";
        public DeviceType DeviceType { get; set; } = DeviceType.Upper;
        public bool Use { get; set; }
        public string Operation { get => Use ? opr : "Not Use"; set { if (Use) opr = value; } }
        public DateTime ReserveTime { get; set; }
        //public SKColor LightColor { get; set; }
        //public DayOfWeek DOW { get; set; }
        public double Position { get; set; }
        public double Speed { get; set; }
        public double Current { get; set; }
        public double Voltage { get; set; }
        public double Temp { get; set; }
        public double Presure { get; set; }
        public bool Alarm { get; set; }

        private string opr = "Stop";
    }
}
