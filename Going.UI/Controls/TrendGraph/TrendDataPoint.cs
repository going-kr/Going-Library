using System;
using System.Collections.Generic;

namespace Going.UI.Controls.TrendGraph
{
    /// <summary>
    /// 트렌드 데이터 포인트를 나타냅니다.
    /// </summary>
    public class TrendDataPoint
    {
        /// <summary>
        /// 데이터 측정 시간
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 데이터 값
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// 트렌드 시리즈를 나타냅니다.
    /// </summary>
    public class TrendSeries
    {
        /// <summary>
        /// 시리즈 이름
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 데이터 포인트 목록
        /// </summary>
        public List<TrendDataPoint> DataPoints { get; set; } = new();

        /// <summary>
        /// 시리즈 색상 (테마 색상 이름)
        /// </summary>
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// 데이터 포인트 선택 이벤트 인자
    /// </summary>
    public class DataPointEventArgs : EventArgs
    {
        /// <summary>
        /// 선택된 시리즈
        /// </summary>
        public TrendSeries? Series { get; set; }

        /// <summary>
        /// 선택된 데이터 포인트
        /// </summary>
        public TrendDataPoint? DataPoint { get; set; }
    }

    /// <summary>
    /// 보이는 범위 변경 이벤트 인자
    /// </summary>
    public class VisibleRangeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 보이는 범위의 최소값
        /// </summary>
        public DateTime VisibleMinimum { get; set; }

        /// <summary>
        /// 보이는 범위의 최대값
        /// </summary>
        public DateTime VisibleMaximum { get; set; }
    }
}
