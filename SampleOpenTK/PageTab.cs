using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using SkiaSharp;
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
        GoButton btnMain, btnP1, btnP2, btnP3;
        GoCalendar cal;
        GoToolBox tb;
        GoTreeView tv;
        GoInputColor ic;
        GoInputDateTime id;
        public PageTab()
        {
            Name = "PageTab";

            #region MainPage
            btnMain = new GoButton { Name = "btn", Left = 10, Top = 10, Width = 80, Height = 40, Text = "메인 페이지" };
            Childrens.Add(btnMain);
            #endregion
            #region Tab
            var tab = new GoTabControl { Fill = true, Margin = new GoPadding(10, 60, 10, 10),  };
            var tp1 = new GoTabPage { Text = "Tab 1", IconString = "fa-check" };
            var tp2 = new GoTabPage { Text = "Tab 2", IconString = "fa-check" };
            var tp3 = new GoTabPage { Text = "Tab 3", IconString = "fa-check" };

            tab.TabPages.Add(tp1);
            tab.TabPages.Add(tp2);
            tab.TabPages.Add(tp3);

            Childrens.Add(tab);
            #endregion

            #region tp1
            btnP1 = new GoButton { Text = "페이지 1", Left = 0, Top = 0 };
            cal = new GoCalendar { Left = 0, Top = 40, Width = 300, Height = 280, MultiSelect = true };
            ic = new GoInputColor { Left = 310, Top = 40, Width = 300, Height = 40 };
            id = new GoInputDateTime { Left = 310, Top = 90, Width = 300, Height = 40, DateTimeStyle = GoDateTimeKind.DateTime };
            tp1.Childrens.Add(btnP1);
            tp1.Childrens.Add(cal);
            tp1.Childrens.Add(ic);
            tp1.Childrens.Add(id);
            #endregion
            #region tp2
            btnP2 = new GoButton { Text = "페이지 2", Left = 0, Top = 0 };
            tb = new GoToolBox { Left = 0, Top = 40, Width = 300, Height = 400 };
            tp2.Childrens.Add(tb);
            tp2.Childrens.Add(btnP2);

            for (int i = 1; i <= 7; i++)
            {
                var cat = new GoToolCategory { Text = $"Category {i}" };
                tb.Categories.Add(cat);

                for (int j = 1; j <= 10; j++)
                    cat.Items.Add(new GoToolItem { Text = $"Item {i}.{j}" });
            }
            #endregion
            #region tp3
            btnP3 = new GoButton { Text = "페이지 3", Left = 0, Top = 0 };
            tv = new GoTreeView { Left = 0, Top = 40, Width = 300, Height = 400, SelectionMode = GoItemSelectionMode.MultiPC, DragMode = false };
            tp3.Childrens.Add(btnP3);
            tp3.Childrens.Add(tv);

            var rnd = new Random();

            for (int i = 0; i <5; i++)
            {
                var nd0 = new GoTreeNode { Text = $"Node {i}" };
                tv.Nodes.Add(nd0);

                for(int j = 0; j < rnd.Next(0, 10); j++)
                {
                    var nd1 = new GoTreeNode { Text = $"Node {i}.{j}" };
                    nd0.Nodes.Add(nd1);

                    for (int k = 0; k < rnd.Next(0, 10); k++)
                    {
                        var nd2 = new GoTreeNode { Text = $"Node {i}.{j}.{k}" };
                        nd1.Nodes.Add(nd2);
                    }
                }
            }
            #endregion

            btnMain.ButtonClicked += (o, s) => Design?.SetPage("PageMain");
            btnP2.DragDrop += (o, s) => { if (s.DragItem is GoToolItem i) btnP2.Text = i.Text ?? ""; };
            btnP3.DragDrop += (o, s) => { if (s.DragItem is GoTreeNode i) btnP3.Text = i.Text ?? ""; };
        }

    }
    
}
