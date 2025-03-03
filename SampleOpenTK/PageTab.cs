using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SampleOpenTK
{
    public class PageTab : GoPage
    {
        public PageTab()
        {
            Name = "PageTab";


            var btn = new GoButton { Name = "btn", Left = 10, Top = 10, Width = 80, Height = 40, Text = "메인 페이지" };
            Childrens.Add(btn);
            btn.ButtonClicked += (o, s) => Design?.SetPage("PageMain");

            var tab = new GoTabControl { Fill = true, Margin = new GoPadding(10, 60, 10, 10),  };
            //tab.IconDirection = GoDirectionHV.Vertical; tab.IconSize = 24; tab.IconGap = 10;
            tab.TabPosition = GoDirection.Down;// tab.NavSize = 120;

            var tp1 = new GoTabPage { Text = "Tab 1", IconString = "fa-check" };
            var tp2 = new GoTabPage { Text = "Tab 2", IconString = "fa-check" };
            var tp3 = new GoTabPage { Text = "Tab 3", IconString = "fa-check" };

            tab.TabPages.Add(tp1);
            tab.TabPages.Add(tp2);
            tab.TabPages.Add(tp3);

            Childrens.Add(tab);

            var btnP1 = new GoButton { Text = "페이지 1", Left = 0, Top = 0 };
            var btnP2 = new GoButton { Text = "페이지 2", Left = 0, Top = 0 };
            var btnP3 = new GoButton { Text = "페이지 3", Left = 0, Top = 0 };
            tp1.Childrens.Add(btnP1);
            tp2.Childrens.Add(btnP2);
            tp3.Childrens.Add(btnP3);

            tp1.Childrens.Add(new GoCalendar { Left = 0, Top = 40, Width = 300, Height = 240, MultiSelect = true });

            var tb = new GoToolBox { Left = 0, Top = 40, Width = 300, Height = 400 };
            for (int i = 1; i <= 7; i++)
            {
                var cat = new GoToolCategory { Text = $"Category {i}" };
                tb.Categories.Add(cat);

                for (int j = 1; j <= 10; j++)
                    cat.Items.Add(new GoToolItem { Text = $"Item {i}.{j}" });
            }
            tp2.Childrens.Add(tb);

            btnP2.DragDrop += (o, s) => { if (s.DragItem is GoToolItem i) btnP2.Text = i.Text ?? ""; };

        }

    }
    
}
