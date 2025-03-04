using System;
using System.Collections.Generic;
using System.Linq;

namespace Going.UI.Controls.TrendGraph
{
    /// <summary>
    /// 트렌드 데이터 포인트를 나타냅니다.
    /// </summary>
    public readonly record struct TrendDataPoint
    {
        /// <summary>
        /// 데이터 측정 시간
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// 데이터 값
        /// </summary>
        public double Value { get; init; }

        /// <summary>
        /// 데이터 포인트 생성자
        /// </summary>
        public TrendDataPoint(DateTime timestamp, double value)
        {
            Timestamp = timestamp;
            Value = value;
        }
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

        /// <summary>
        /// 보이는 데이터 포인트를 반환합니다.
        /// </summary>
        /// <param name="min">시작 시간</param>
        /// <param name="max">종료 시간</param>
        /// <returns>보이는 데이터 포인트 목록</returns>
        public IEnumerable<TrendDataPoint> GetVisiblePoints(DateTime min, DateTime max)
        {
            return DataPoints.Where(p => p.Timestamp >= min && p.Timestamp <= max)
                .OrderBy(p => p.Timestamp);
        }

        /// <summary>
        /// 데이터 포인트 추가
        /// </summary>
        public void AddDataPoint(DateTime timestamp, double value)
        {
            DataPoints.Add(new TrendDataPoint(timestamp, value));
        }

        /// <summary>
        /// 데이터 정렬
        /// </summary>
        public void SortData()
        {
            DataPoints = DataPoints.OrderBy(p => p.Timestamp).ToList();
        }

        /// <summary>
        /// 최소/최대 값 구하기
        /// </summary>
        public (double Min, double Max) GetValueRange()
        {
            if (DataPoints.Count == 0)
                return (0, 0);

            return (DataPoints.Min(p => p.Value), DataPoints.Max(p => p.Value));
        }

        /// <summary>
        /// 시간 범위 구하기
        /// </summary>
        public (DateTime Min, DateTime Max) GetTimeRange()
        {
            if (DataPoints.Count == 0)
                return (DateTime.Now, DateTime.Now);

            return (DataPoints.Min(p => p.Timestamp), DataPoints.Max(p => p.Timestamp));
        }
    }

    /// <summary>
    /// 데이터 포인트 선택 이벤트 인자
    /// </summary>
    public class DataPointEventArgs : EventArgs
    {
        /// <summary>
        /// 선택된 시리즈
        /// </summary>
        public TrendSeries? Series { get; init; }

        /// <summary>
        /// 선택된 데이터 포인트
        /// </summary>
        public TrendDataPoint? DataPoint { get; init; }

        /// <summary>
        /// 생성자
        /// </summary>
        public DataPointEventArgs(TrendSeries? series, TrendDataPoint? dataPoint)
        {
            Series = series;
            DataPoint = dataPoint;
        }
    }

    /// <summary>
    /// 보이는 범위 변경 이벤트 인자
    /// </summary>
    public class VisibleRangeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 보이는 범위의 최소값
        /// </summary>
        public DateTime VisibleMinimum { get; init; }

        /// <summary>
        /// 보이는 범위의 최대값
        /// </summary>
        public DateTime VisibleMaximum { get; init; }

        /// <summary>
        /// 생성자
        /// </summary>
        public VisibleRangeChangedEventArgs(DateTime min, DateTime max)
        {
            VisibleMinimum = min;
            VisibleMaximum = max;
        }
    }
}