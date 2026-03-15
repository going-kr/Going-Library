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
        public static List<DataGridSampleData> Datas3 = new();

        static SampleData()
        {
            Random rand = new();

            #region Data1
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
            #endregion

            #region Data2
            {
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
            #endregion

            #region Datas3
            {
                var states = Enum.GetNames<DeviceState>();
                var types = Enum.GetNames<DeviceType>();
                var locations = new[] { "1F-A", "1F-B", "2F-A", "2F-B", "3F-A", "3F-B", "외부" };
                var colors = new[] { "#F44336", "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#607D8B" };
                var now = DateTime.Now;

                for (int i = 0; i < 100; i++)
                {
                    Datas3.Add(new DataGridSampleData
                    {
                        Id = 1000 + i,
                        Name = $"DEV-{i + 1:D3}",
                        Location = locations[rand.Next(locations.Length)],
                        Temperature = Math.Round(rand.NextDouble() * 80 + 10, 1),
                        Pressure = (float)Math.Round(rand.NextDouble() * 5 + 1, 2),
                        ErrorCount = rand.Next(0, 20),
                        IsOnline = rand.Next(100) < 85,
                        IsAlarm = rand.Next(100) < 15,
                        UseFilter = rand.Next(100) < 50,
                        LastUpdate = now.AddMinutes(-rand.Next(0, 1440)),
                        InstallDate = now.Date.AddDays(-rand.Next(30, 365)),
                        TagColor = colors[rand.Next(colors.Length)],
                        State = states[rand.Next(states.Length)],
                        Type = types[rand.Next(types.Length)],
                    });
                }
            }
            #endregion

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

    #region Enums
    public enum DeviceState { Idle, Running, Warning, Error, Maintenance }
    public enum DeviceType { Sensor, Actuator, Controller, Gateway }
    #endregion

    public class DataGridSampleData
    {
        // Label
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        // Number
        public int Id { get; set; }
        public double Temperature { get; set; }
        public float Pressure { get; set; }
        public int ErrorCount { get; set; }

        // Bool / CheckBox / Lamp
        public bool IsOnline { get; set; }
        public bool IsAlarm { get; set; }
        public bool UseFilter { get; set; }

        // DateTime
        public DateTime LastUpdate { get; set; }
        public DateTime InstallDate { get; set; }

        // Color
        public string TagColor { get; set; } = "#FF0000";

        // Combo (string)
        public string State { get; set; } = "Idle";
        public string Type { get; set; } = "Sensor";

    }
}
