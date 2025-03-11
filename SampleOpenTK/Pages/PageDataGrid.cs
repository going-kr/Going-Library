using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK.Pages
{
    public class PageDataGrid : GoPage
    {
        GoDataGrid dg;
        public PageDataGrid()
        {
            Name = "PageDataGrid";

            dg = new GoDataGrid { Fill = true, Margin = new GoPadding(10) };
            Childrens.Add(dg);

            string[] sz1 = ["100px", "100px", "12%", "11%", "11%", "11%", "11%", "11%", "11%", "11%", "11%"];
            string[] sz2 = ["100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px", "100px"];
            var sz = sz2;

            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Name", HeaderText = "명칭", Size = sz[0], GroupName = "Info", Fixed = true, UseFilter = true });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "DeviceType", HeaderText = "타입", Size = sz[1], GroupName = "Info", Fixed = true, UseFilter = true });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Operation", HeaderText = "상태", Size = sz[2], GroupName = "Motor", UseSort = true });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Position", HeaderText = "위치", Size = sz[3], GroupName = "Motor" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Speed", HeaderText = "속도", Size = sz[4], GroupName = "Motor" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Current", HeaderText = "전류", Size = sz[5], GroupName = "Motor" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Voltage", HeaderText = "전압", Size = sz[6], GroupName = "Motor" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Temp", HeaderText = "온도", Size = sz[7], GroupName = "Sensor" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Hum", HeaderText = "습도", Size = sz[8], GroupName = "Sensor" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Noise", HeaderText = "소음", Size = sz[9], GroupName = "Sensor" });
            dg.Columns.Add(new GoDataGridLabelColumn { Name = "Presure", HeaderText = "압력", Size = sz[10], GroupName = "Sensor"});

            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "Info", HeaderText = "장치 정보", Fixed = true });
            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "Motor", HeaderText = "구동부", GroupName = "State" });
            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "Sensor", HeaderText = "센서부", GroupName = "State" });
            dg.ColumnGroups.Add(new GoDataGridLabelColumn { Name = "State", HeaderText = "상태" });

            dg.SummaryRows.Add(new GoDataGridSumSummaryRow());

            dg.SelectionMode = GoDataGridSelectionMode.Selector;
            dg.ScrollMode = ScrollMode.Both;

            var ls = new List<Device>();
            for (int i = 0; i < 100; i++)
            {
                string nm = "", tp = "";
                if (i < 70) { nm = $"상부{i + 1}"; tp = "상부"; }
                else if (i < 80) { nm = $"하부{i - 70 + 1}"; tp = "하부"; }
                else if (i < 90) { nm = $"웨건{i - 80 + 1}"; tp = "웨건"; }
                else if (i < 100) { nm = $"브릿지{i - 90 + 1}"; tp = "브릿지"; }

                ls.Add(new Device { Name = nm, DeviceType = tp });
            }

            dg.SetDataSource(ls);
        }
    }

    public class Device
    {
        public string Name { get; set; } = "Device";
        public string DeviceType { get; set; } = "";
        public string Operation { get; set; } = "Stop";
        public double Position { get; set; }
        public double Speed { get; set; }
        public double Current { get; set; }
        public double Voltage { get; set; }
        public double Temp { get; set; }
        public double Hum { get; set; }
        public double Noise { get; set; }
        public double Presure { get; set; }
        
    }
}
