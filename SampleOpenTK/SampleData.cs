using Going.UI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK
{
    public class SampleData
    {
        public static List<Data1> Datas = new();
        public static List<Data2> Datas2 = new();

        static SampleData()
        {
            Random rand = new();
            for (int i = 1; i <= 12; i++)
            {
                Datas.Add(new Data1
                {
                    Date = $"2026.{i}",
                    Temp = rand.Next(0, 1000) / 10.0,
                    Hum = rand.Next(0, 1000) / 10.0,
                    Rad = rand.Next(0, 1000) / 10.0,
                });
            }

            var now = DateTime.Now;
            double t = rand.Next(0, 1000) / 10.0;
            double h = rand.Next(0, 1000) / 10.0;
            double r = rand.Next(0, 1000) / 10.0;
            double tk = 0.3;
            for (DateTime dt = now.Date; dt < now + TimeSpan.FromHours(6); dt += TimeSpan.FromSeconds(1))
            {
                t = MathTool.Constrain(t + (rand.Next() % 2 == 0 ? tk : -tk), 0, 100);
                h = MathTool.Constrain(h + (rand.Next() % 2 == 0 ? tk : -tk), 0, 100);
                r = MathTool.Constrain(r + (rand.Next() % 2 == 0 ? tk : -tk), 0, 100);

                Datas2.Add(new Data2 { Date = dt, Temp = t, Hum = h, Rad = r, });
            }
        }
    }


    public class Data1
    {
        public string Date { get; set; } = string.Empty;
        public double Temp { get; set; }
        public double Hum { get; set; }
        public double Rad { get; set; }
    }

    public class Data2
    {
        public DateTime Date { get; set; } 
        public double Temp { get; set; }
        public double Hum { get; set; }
        public double Rad { get; set; }
    }
}
