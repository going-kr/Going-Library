using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK
{
    public class PageTest:GoPage
    {
        public PageTest()
        {
            Name = "PageTest";

            var btn = new GoButton { Name = "btn", Left = 10, Top = 10, Width = 80, Height = 40, Text = "메인 페이지" };
            Childrens.Add(btn);
            Childrens.Add(new GoButton { Left = 10, Top = 60, Width = 80, Height = 40, Text = "테스트" });
            Childrens.Add(new GoOnOff { Left = 10, Top = 110, Width = 150, Height = 40 });
            Childrens.Add(new GoSwitch { Left = 10, Top = 160, Width = 150, Height = 40 });
            //Childrens.Add(new GoNumberBox { Left = 10, Top = 210, Width = 200, Height = 40 });
            Childrens.Add(new GoNumberBox { Left = 10, Top = 210, Width = 200, Height = 80, Direction = GoDirectionHV.Vertical, ButtonSize = 30 });

            var tbl = new GoTableLayoutPanel { Left = 150, Top = 10, Width = 200, Height = 150 };
            tbl.Columns = ["50%", "50%"];
            tbl.Rows = ["33.3%", "33.4%", "33.3%"];
            Childrens.Add(tbl);

            tbl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-play", Rotate = 270 }, 0, 0, 2, 1);
            tbl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-play", Rotate = 180 }, 0, 1, 1, 1);
            tbl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-play", Rotate = 0 }, 1, 1, 1, 1);
            tbl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-play", Rotate = 90 }, 0, 2, 2, 1);

            btn.ButtonClicked += (o, s) => Design?.SetPage("PageMain");

        }
    }
}
