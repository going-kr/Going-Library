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
using static OpenTK.Graphics.OpenGL.GL;

namespace SampleOpenTK.Pages
{
    public class PageGraph : GoPage
    {
        GoTabControl tab;
        GoBarGraph grpBar;
        GoLineGraph grpLine;
        GoCircleGraph grpCircle;

        Random rnd = new Random();
        List<GRP> ls = new List<GRP>();
        public PageGraph()
        {
            Name = "PageGraph";

            tab = new GoTabControl { Fill = true, Margin = new GoPadding(10), TabColor = "back" };
            tab.TabPages.Add(new GoTabPage { Text = "Bar Graph" });
            tab.TabPages.Add(new GoTabPage { Text = "Line Graph" });
            tab.TabPages.Add(new GoTabPage { Text = "Circle Graph" });
            tab.TabPages.Add(new GoTabPage { Text = "Time Graph" });
            tab.TabPages.Add(new GoTabPage { Text = "Trend Graph" });
            Childrens.Add(tab);

            var vd = false;
            #region Bar
            grpBar = new GoBarGraph { Fill = true, Margin = new GoPadding(0), Minimum = 0, Maximum = 10000, Direction = GoDirectionHV.Vertical, Mode = GoBarGraphMode.Stack, ValueDraw = vd };
            grpBar.Series.Add(new GoGraphSeries { Name = "Controller", Alias = "제어기", Color = "red" });
            grpBar.Series.Add(new GoGraphSeries { Name = "Touch", Alias = "터치", Color = "GreenYellow" });
            grpBar.Series.Add(new GoGraphSeries { Name = "Cloud", Alias = "클라우드", Color = "DeepSkyBlue", Visible = true });

            tab.TabPages[0].Childrens.Add(grpBar);
            #endregion
            #region Line
            grpLine = new GoLineGraph { Fill = true, Margin = new GoPadding(0), ValueDraw = vd };
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Controller", Alias = "제어기", Color = "red", Minimum = 0, Maximum = 10000 });
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Touch", Alias = "터치", Color = "GreenYellow", Minimum = 0, Maximum = 10000 });
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Cloud", Alias = "클라우드", Color = "DeepSkyBlue", Minimum = 0, Maximum = 10000, Visible = true });

            tab.TabPages[1].Childrens.Add(grpLine);
            #endregion
            #region Circle
            grpCircle = new GoCircleGraph { Fill = true, Margin = new GoPadding(0), FontSize = 14, ValueDraw = vd };
            /*
            grpCircle.Series.Add(new GoGraphSeries { Name = "Controller", Alias = "제어기", Color = "red" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Touch", Alias = "터치", Color = "GreenYellow"  });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Cloud", Alias = "클라우드", Color = "DeepSkyBlue" });
            */

            grpCircle.Series.Add(new GoGraphSeries { Name = "Jan", Alias = "1월", Color = "#f00" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Feb", Alias = "2월", Color = "#0f0" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Mar", Alias = "3월", Color = "#00f" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Apr", Alias = "4월", Color = "#ff0" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "May", Alias = "5월", Color = "#0ff" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Jun", Alias = "6월", Color = "#f0f" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Jul", Alias = "7월", Color = "#800" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Aug", Alias = "8월", Color = "#080" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Sep", Alias = "9월", Color = "#008" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Oct", Alias = "10월", Color = "#880" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Nov", Alias = "11월", Color = "#088" });
            grpCircle.Series.Add(new GoGraphSeries { Name = "Dec", Alias = "12월", Color = "#808" });

            tab.TabPages[2].Childrens.Add(grpCircle);
            #endregion

            #region data
            for (int y = 2025; y <= 2028; y++)
                for (int m = 1; m <= 12; m++)
                    ls.Add(new($"{y}\r\n{m}") { Controller = rnd.Next(0, 10000), Cloud = rnd.Next(0, 10000), Touch = rnd.Next(0, 10000), });

            var ls2 = new List<CGRP>();
            var vs = ls.Where(x => x.Month.StartsWith("2025"));
            ls2.Add(new CGRP("Controller", vs, (x) => x.Controller));
            ls2.Add(new CGRP("Touch", vs, (x) => x.Touch));
            ls2.Add(new CGRP("Cloud", vs, (x) => x.Cloud));
            #endregion

            grpBar.SetDataSource("Month", ls);
            grpLine.SetDataSource("Month", ls);
            grpCircle.SetDataSource("Product", ls2);
        }

        class GRP(string month)
        {
            public string Month { get; set; } = month;

            public long Controller { get; set; }
            public long Touch { get; set; }
            public long Cloud { get; set; }
        }

        class CGRP(string product, IEnumerable<GRP> vs, Func<GRP, long> func)
        {
            public string Product { get; set; } = product;

            public long Jan { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n1")).Sum(func);
            public long Feb { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n2")).Sum(func);
            public long Mar { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n3")).Sum(func);
            public long Apr { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n4")).Sum(func);
            public long May { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n5")).Sum(func);
            public long Jun { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n6")).Sum(func);
            public long Jul { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n7")).Sum(func);
            public long Aug { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n8")).Sum(func);
            public long Sep { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n9")).Sum(func);
            public long Oct { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n10")).Sum(func);
            public long Nov { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n11")).Sum(func);
            public long Dec { get; set; } = vs.Where(x => x.Month.EndsWith("\r\n12")).Sum(func);
        }
    }
}
