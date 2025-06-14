﻿using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Dialogs;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Geolocation;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SampleForms
{
    public partial class FormMain : GoForm
    {
        Random rnd = new Random();
        List<GRP> ls = new List<GRP>();
        TGRP tdata = new TGRP { };

        public FormMain()
        {
            InitializeComponent();

            #region Bar
            grpBar.Series.Add(new GoGraphSeries { Name = "Controller", Alias = "제어기", Color = "red" });
            grpBar.Series.Add(new GoGraphSeries { Name = "Touch", Alias = "터치", Color = "GreenYellow" });
            grpBar.Series.Add(new GoGraphSeries { Name = "Cloud", Alias = "클라우드", Color = "DeepSkyBlue", Visible = true });
            #endregion
            #region Line
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Controller", Alias = "제어기", Color = "red", Minimum = 0, Maximum = 10000 });
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Touch", Alias = "터치", Color = "GreenYellow", Minimum = 0, Maximum = 10000 });
            grpLine.Series.Add(new GoLineGraphSeries { Name = "Cloud", Alias = "클라우드", Color = "DeepSkyBlue", Minimum = 0, Maximum = 10000, Visible = true });
            #endregion
            #region Circle
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
            #endregion
            #region Time
            grpTime.Series.Add(new GoLineGraphSeries { Name = "Temp", Alias = "온도", Color = "red", Minimum = 0, Maximum = 200 });
            grpTime.Series.Add(new GoLineGraphSeries { Name = "Hum", Alias = "습도", Color = "GreenYellow", Minimum = 0, Maximum = 100 });
            grpTime.Series.Add(new GoLineGraphSeries { Name = "Noise", Alias = "소음", Color = "DeepSkyBlue", Minimum = 0, Maximum = 400, Visible = true });
            #endregion
            #region Trend
            grpTrend.Series.Add(new GoLineGraphSeries { Name = "Temp", Alias = "온도", Color = "red", Minimum = 0, Maximum = 200 });
            grpTrend.Series.Add(new GoLineGraphSeries { Name = "Hum", Alias = "습도", Color = "GreenYellow", Minimum = 0, Maximum = 100 });
            grpTrend.Series.Add(new GoLineGraphSeries { Name = "Noise", Alias = "소음", Color = "DeepSkyBlue", Minimum = 0, Maximum = 400, Visible = true });
            #endregion

            #region data
            for (int y = 2025; y <= 2028; y++)
                for (int m = 1; m <= 12; m++)
                    ls.Add(new($"{y}.{m}") { Controller = rnd.Next(0, 10000), Cloud = rnd.Next(0, 10000), Touch = rnd.Next(0, 10000), });

            var ls2 = new List<CGRP>();
            var vs = ls.Where(x => x.Month.StartsWith("2025"));
            ls2.Add(new CGRP("Controller", vs, (x) => x.Controller));
            ls2.Add(new CGRP("Touch", vs, (x) => x.Touch));
            ls2.Add(new CGRP("Cloud", vs, (x) => x.Cloud));

            var ls3 = new List<TGRP>();
            double vt = 0, vh = 0, vn = 0;
            for (DateTime time = DateTime.Now.Date; time <= DateTime.Now.Date + TimeSpan.FromDays(1); time += TimeSpan.FromSeconds(1))
            {
                vt = MathTool.Constrain(vt + (rnd.Next() % 2 == 0 ? 0.5 : -0.5), 0, 200);
                vh = MathTool.Constrain(vh + (rnd.Next() % 2 == 0 ? 0.5 : -0.5), 0, 100);
                vn = MathTool.Constrain(vn + (rnd.Next() % 2 == 0 ? 0.5 : -0.5), 0, 400);

                ls3.Add(new TGRP { Time = time, Temp = vt, Hum = vh, Noise = vn });
            }

            #endregion

            grpBar.SetDataSource("Month", ls);
            grpLine.SetDataSource("Month", ls);
            grpCircle.SetDataSource("Product", ls2);
            grpTime.SetDataSource("Time", ls3);

            grpTrend.Start(tdata);
            Task.Run(async () =>
            {
                var rnd2 = new Random();
                while (true)
                {
                    var tk = 0.5F;
                    tdata.Temp = MathTool.Constrain(tdata.Temp + (rnd2.Next() % 2 == 0 ? tk : -tk), 0, 200);
                    tdata.Hum = MathTool.Constrain(tdata.Hum + (rnd2.Next() % 2 == 0 ? tk : -tk), 0, 100);
                    tdata.Noise = MathTool.Constrain(tdata.Noise + (rnd2.Next() % 2 == 0 ? tk : -tk), 0, 400);
                    grpTrend.SetData(tdata);
                    await Task.Delay(grpTrend.Interval);
                }
            });

            var vls = new List<DGI>();
            for (int i = 0; i < 100; i++)
                vls.Add(new DGI { Name = $"Item{i + 1}", Text = "" });

            goButton1.ButtonClicked += (o, s) =>
            {
                var f = new FormDG();

                f.Show();
            };
        }


    }

    #region DGI
    class DGI
    {
        public string Name { get; set; } = "";
        public string Text { get; set; } = "";
        public int Number1 { get; set; }
        public float Number2 { get; set; }
        public double Number3 { get; set; }
    }
    #endregion
    #region GRP
    class GRP(string month)
    {
        public string Month { get; set; } = month;

        public long Controller { get; set; }
        public long Touch { get; set; }
        public long Cloud { get; set; }
    }
    #endregion
    #region CGRP
    class CGRP(string product, IEnumerable<GRP> vs, Func<GRP, long> func)
    {
        public string Product { get; set; } = product;

        public long Jan { get; set; } = vs.Where(x => x.Month.EndsWith(".1")).Sum(func);
        public long Feb { get; set; } = vs.Where(x => x.Month.EndsWith(".")).Sum(func);
        public long Mar { get; set; } = vs.Where(x => x.Month.EndsWith(".3")).Sum(func);
        public long Apr { get; set; } = vs.Where(x => x.Month.EndsWith(".4")).Sum(func);
        public long May { get; set; } = vs.Where(x => x.Month.EndsWith(".5")).Sum(func);
        public long Jun { get; set; } = vs.Where(x => x.Month.EndsWith(".6")).Sum(func);
        public long Jul { get; set; } = vs.Where(x => x.Month.EndsWith(".7")).Sum(func);
        public long Aug { get; set; } = vs.Where(x => x.Month.EndsWith(".8")).Sum(func);
        public long Sep { get; set; } = vs.Where(x => x.Month.EndsWith(".9")).Sum(func);
        public long Oct { get; set; } = vs.Where(x => x.Month.EndsWith(".10")).Sum(func);
        public long Nov { get; set; } = vs.Where(x => x.Month.EndsWith(".11")).Sum(func);
        public long Dec { get; set; } = vs.Where(x => x.Month.EndsWith(".12")).Sum(func);
    }
    #endregion
    #region TGRP
    class TGRP
    {
        public DateTime Time { get; set; }
        public double Temp { get; set; }
        public double Hum { get; set; }
        public double Noise { get; set; }
    }
    #endregion
}
