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
        public string Name { get; set; } = "";
        public string Alias { get; set; } = "";
        public string Color { get; set; } = "Red";
        public bool Visible { get; set; } = true;
    }
    #endregion
    #region class : LineGraphSeries
    public class GoLineGraphSeries
    {
        public string Name { get; set; } = "";
        public string Alias { get; set; } = "";
        public string Color { get; set; } = "Red";
        public bool Visible { get; set; } = true;

        public double Minimum { get; set; } = 0;
        public double Maximum { get; set; } = 100;
    }
    #endregion
}
