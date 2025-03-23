using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SampleOpenTK.Pages
{
    public class PageGraph : GoPage
    {
        GoTabControl tab;
        GoBarGraph grpBar;
        GoLineGraph grpLine;
        Random rnd = new Random();
        List<GRP> ls = new List<GRP>();
        public PageGraph()
        {
            Name = "PageGraph";

            tab = new GoTabControl { Fill = true, Margin = new GoPadding(10), TabColor = "back" };
            tab.TabPages.Add(new GoTabPage { Text = "Bar Graph" });
            tab.TabPages.Add(new GoTabPage { Text = "Line Graph" });
            tab.TabPages.Add(new GoTabPage { Text = "Circle Graph" });
            tab.TabPages.Add(new GoTabPage { Text = "Time Graph"  });
            tab.TabPages.Add(new GoTabPage { Text = "Trend Graph" });
            Childrens.Add(tab);

            #region Bar
            grpBar = new GoBarGraph { Fill = true, Margin = new GoPadding(0), XAxisName = "Month", Minimum = 0, Maximum = 10000, Direction = GoDirectionHV.Vertical, Mode = GoBarGraphMode.Stack };
            grpBar.Series.Add(new GoGraphSeries { Name = "Controller", Alias = "제어기", Color = "red" });
            grpBar.Series.Add(new GoGraphSeries { Name = "Touch", Alias = "터치", Color = "GreenYellow" });
            grpBar.Series.Add(new GoGraphSeries { Name = "Cloud", Alias = "클라우드", Color = "DeepSkyBlue", Visible = true });

            tab.TabPages[0].Childrens.Add(grpBar);
            #endregion
            #region Line
            grpLine = new GoLineGraph { Fill = true, Margin = new GoPadding(0), XAxisName = "Month" };
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Controller", Alias = "제어기", Color = "red", Minimum = 0, Maximum = 10000 });
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Touch", Alias = "터치", Color = "GreenYellow", Minimum = 0, Maximum = 10000 });
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Cloud", Alias = "클라우드", Color = "DeepSkyBlue", Minimum = 0, Maximum = 10000, Visible = true });

            tab.TabPages[1].Childrens.Add(grpLine);
            #endregion


            for (int y = 2025; y <= 2028; y++)
                for (int m = 1; m <= 12; m++)
                    ls.Add(new($"{y}\r\n{m}") { Controller = rnd.Next(0, 10000), Cloud = rnd.Next(0, 10000), Touch = rnd.Next(0, 10000), });

            grpBar.SetDataSource(ls);
            grpLine.SetDataSource(ls);
        }


        class GRP(string month)
        {
            public string Month { get; set; } = month;

            public long Controller { get; set; }
            public long Touch { get; set; }
            public long Cloud { get; set; }
        }
    }
}
