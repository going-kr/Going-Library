﻿using Going.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public enum GoBarGraphMode { Stack, List }

    #region class : GraphSeries
    public class GoGraphSeries
    {
        [GoProperty(PCategory.Control, 0)] public string Name { get; set; } = "";
        [GoProperty(PCategory.Control, 1)] public string Alias { get; set; } = "";
        [GoProperty(PCategory.Control, 2)] public string Color { get; set; } = "Red";
        [GoProperty(PCategory.Control, 3)] public bool Visible { get; set; } = true;
    }
    #endregion
    #region class : LineGraphSeries
    public class GoLineGraphSeries
    {
        [GoProperty(PCategory.Control, 0)] public string Name { get; set; } = "";
        [GoProperty(PCategory.Control, 1)] public string Alias { get; set; } = "";
        [GoProperty(PCategory.Control, 2)] public string Color { get; set; } = "Red";
        [GoProperty(PCategory.Control, 3)] public bool Visible { get; set; } = true;

        [GoProperty(PCategory.Control, 4)] public double Minimum { get; set; } = 0;
        [GoProperty(PCategory.Control, 5)] public double Maximum { get; set; } = 100;
    }
    #endregion


    class GoGraphValue
    {
        public string Name { get; set; }
        public Dictionary<string, double> Values { get; } = new Dictionary<string, double>();
    }

    class GoTimeGraphValue
    {
        public DateTime Time { get; set; }
        public Dictionary<string, double> Values { get; } = new Dictionary<string, double>();
    }
}
