using System;
using System.Collections.Generic;
using System.Linq;

namespace Going.UI.Controls.TrendGraph
{
    /// <summary>
    /// 트렌드 데이터 포인트를 나타냅니다.
    /// </summary>
    public readonly struct TrendDataPoint : IEquatable<TrendDataPoint>
    {
        /// <summary>
        /// 데이터 측정 시간
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// 데이터 값
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// 데이터 포인트 생성자
        /// </summary>
        public TrendDataPoint(DateTime timestamp, double value)
        {
            Timestamp = timestamp;
            Value = value;
        }

        // 값 타입 비교 최적화를 위한 Equals 구현
        public bool Equals(TrendDataPoint other)
        {
            return Timestamp.Equals(other.Timestamp) && Math.Abs(Value - other.Value) < 0.000001;
        }

        public override bool Equals(object obj)
        {
            return obj is TrendDataPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Timestamp, Value);
        }

        public static bool operator ==(TrendDataPoint left, TrendDataPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TrendDataPoint left, TrendDataPoint right)
        {
            return !(left == right);
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
        /// 보이는 범위 내의 데이터 포인트를 필터링합니다.
        /// </summary>
        public IEnumerable<TrendDataPoint> GetVisiblePoints(DateTime minTime, DateTime maxTime)
        {
            if (DataPoints.Count == 0)
                return Array.Empty<TrendDataPoint>();

            // 이진 검색으로 시작 인덱스 찾기 (성능 최적화)
            int startIdx = BinarySearchLowerBound(minTime);

            // 필터링된 결과를 List로 직접 구성 (LINQ 오버헤드 방지)
            var result = new List<TrendDataPoint>();
            for (int i = startIdx; i < DataPoints.Count; i++)
            {
                var point = DataPoints[i];
                if (point.Timestamp > maxTime)
                    break;

                result.Add(point);
            }

            return result;
        }

        /// <summary>
        /// 이진 검색을 사용하여 시작 시간보다 크거나 같은 첫 번째 인덱스를 찾습니다.
        /// </summary>
        private int BinarySearchLowerBound(DateTime minTime)
        {
            int left = 0;
            int right = DataPoints.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                if (DataPoints[mid].Timestamp < minTime)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return left;
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
            if (DataPoints.Count <= 1)
                return;

            DataPoints.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        }

        /// <summary>
        /// 최소/최대 값 구하기 (최적화 버전)
        /// </summary>
        public (double Min, double Max) GetValueRange()
        {
            if (DataPoints.Count == 0)
                return (0, 0);

            double min = double.MaxValue;
            double max = double.MinValue;

            // 루프 한 번으로 최소/최대 계산 (LINQ 오버헤드 방지)
            foreach (var point in DataPoints)
            {
                if (point.Value < min) min = point.Value;
                if (point.Value > max) max = point.Value;
            }

            return (min, max);
        }

        /// <summary>
        /// 시간 범위 구하기 (최적화 버전)
        /// </summary>
        public (DateTime Min, DateTime Max) GetTimeRange()
        {
            if (DataPoints.Count == 0)
                return (DateTime.Now, DateTime.Now);

            // 데이터가 정렬되어 있다면 첫/마지막 요소만 확인 (최적화)
            if (DataPoints.Count > 1 && DataPoints[0].Timestamp <= DataPoints[DataPoints.Count - 1].Timestamp)
            {
                return (DataPoints[0].Timestamp, DataPoints[DataPoints.Count - 1].Timestamp);
            }

            // 정렬되지 않은 경우 전체 순회
            DateTime min = DateTime.MaxValue;
            DateTime max = DateTime.MinValue;

            foreach (var point in DataPoints)
            {
                if (point.Timestamp < min) min = point.Timestamp;
                if (point.Timestamp > max) max = point.Timestamp;
            }

            return (min, max);
        }

        /// <summary>
        /// 필요한 경우 데이터 다운샘플링 (최적화)
        /// </summary>
        public List<TrendDataPoint> GetDownsampledPoints(DateTime min, DateTime max, int maxPoints)
        {
            // 보이는 데이터 가져오기
            var visiblePoints = GetVisiblePoints(min, max).ToList();

            // 포인트 수가 적으면 다운샘플링 불필요
            if (visiblePoints.Count <= maxPoints)
                return visiblePoints;

            // LTTB(Largest-Triangle-Three-Buckets) 다운샘플링 알고리즘
            // maxPoints보다 적은 수의 데이터로 줄이면서 시각적 특성 유지
            var result = new List<TrendDataPoint>(maxPoints);

            // 첫 포인트는 항상 포함
            result.Add(visiblePoints[0]);

            double bucketSize = (double)visiblePoints.Count / (maxPoints - 2);

            for (int i = 1; i < maxPoints - 1; i++)
            {
                int bucketStart = (int)((i - 1) * bucketSize);
                int bucketEnd = (int)(i * bucketSize);

                int nextBucketStart = (int)(i * bucketSize);
                int nextBucketEnd = Math.Min(visiblePoints.Count - 1, (int)((i + 1) * bucketSize));

                // 이전 포인트
                var prevPoint = result[i - 1];

                // 다음 버킷의 대표점 (중간 값)
                int nextBucketMiddle = (nextBucketStart + nextBucketEnd) / 2;
                var nextPoint = visiblePoints[nextBucketMiddle];

                // 현재 버킷에서 최대 삼각형 면적을 가진 포인트 찾기
                double maxArea = 0;
                int maxAreaIdx = bucketStart;

                for (int j = bucketStart; j <= bucketEnd; j++)
                {
                    double area = CalculateTriangleArea(prevPoint, visiblePoints[j], nextPoint);
                    if (area > maxArea)
                    {
                        maxArea = area;
                        maxAreaIdx = j;
                    }
                }

                result.Add(visiblePoints[maxAreaIdx]);
            }

            // 마지막 포인트는 항상 포함
            result.Add(visiblePoints[visiblePoints.Count - 1]);

            return result;
        }

        // 시간-값 좌표계에서 삼각형 면적 계산
        private double CalculateTriangleArea(TrendDataPoint p1, TrendDataPoint p2, TrendDataPoint p3)
        {
            // 시간을 X 축으로, 값을 Y 축으로 사용
            double x1 = p1.Timestamp.Ticks;
            double y1 = p1.Value;
            double x2 = p2.Timestamp.Ticks;
            double y2 = p2.Value;
            double x3 = p3.Timestamp.Ticks;
            double y3 = p3.Value;

            return Math.Abs(
                (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0
            );
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
        public TrendSeries? Series { get; }

        /// <summary>
        /// 선택된 데이터 포인트
        /// </summary>
        public TrendDataPoint DataPoint { get; }

        /// <summary>
        /// 생성자
        /// </summary>
        public DataPointEventArgs(TrendSeries? series, TrendDataPoint dataPoint)
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
        public DateTime VisibleMinimum { get; }

        /// <summary>
        /// 보이는 범위의 최대값
        /// </summary>
        public DateTime VisibleMaximum { get; }

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