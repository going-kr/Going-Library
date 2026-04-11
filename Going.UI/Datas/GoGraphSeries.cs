using Going.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    /// <summary>
    /// 막대 그래프의 표시 모드를 정의하는 열거형입니다.
    /// </summary>
    public enum GoBarGraphMode
    {
        /// <summary>시리즈를 누적하여 표시</summary>
        Stack,
        /// <summary>시리즈를 나란히 표시</summary>
        List
    }

    #region class : GraphSeries
    /// <summary>
    /// 그래프 시리즈 정보를 나타내는 클래스입니다.
    /// </summary>
    public class GoGraphSeries
    {
        /// <summary>시리즈의 데이터 바인딩 이름</summary>
        [GoProperty(PCategory.Basic, 0)] public string Name { get; set; } = "";
        /// <summary>시리즈의 표시 이름(별칭)</summary>
        [GoProperty(PCategory.Basic, 1)] public string Alias { get; set; } = "";
        /// <summary>시리즈의 색상</summary>
        [GoProperty(PCategory.Basic, 2)] public string Color { get; set; } = "Red";
        /// <summary>시리즈 표시 여부</summary>
        [GoProperty(PCategory.Basic, 3)] public bool Visible { get; set; } = true;

        /// <summary>시리즈 이름을 반환합니다.</summary>
        public override string ToString() => Name;
    }
    #endregion
    #region class : LineGraphSeries
    /// <summary>
    /// 라인 그래프용 시리즈 정보를 나타내는 클래스입니다. 최솟값과 최댓값 범위를 포함합니다.
    /// </summary>
    public class GoLineGraphSeries
    {
        /// <summary>시리즈의 데이터 바인딩 이름</summary>
        [GoProperty(PCategory.Basic, 0)] public string Name { get; set; } = "";
        /// <summary>시리즈의 표시 이름(별칭)</summary>
        [GoProperty(PCategory.Basic, 1)] public string Alias { get; set; } = "";
        /// <summary>시리즈의 색상</summary>
        [GoProperty(PCategory.Basic, 2)] public string Color { get; set; } = "Red";
        /// <summary>시리즈 표시 여부</summary>
        [GoProperty(PCategory.Basic, 3)] public bool Visible { get; set; } = true;

        /// <summary>Y축 최솟값</summary>
        [GoProperty(PCategory.Basic, 4)] public double Minimum { get; set; } = 0;
        /// <summary>Y축 최댓값</summary>
        [GoProperty(PCategory.Basic, 5)] public double Maximum { get; set; } = 100;

        /// <summary>시리즈 이름을 반환합니다.</summary>
        public override string ToString() => Name;
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
