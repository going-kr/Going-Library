using System;
using SkiaSharp;

namespace Going.UI.Controls.TrendGraph
{
    /// <summary>
    /// 트렌드 그래프 스크롤 관리자
    /// </summary>
    public class TrendScrollManager : IDisposable
    {
        #region Properties

        /// <summary>
        /// 스크롤바 영역
        /// </summary>
        public SKRect ScrollBarArea { get; set; }

        /// <summary>
        /// 그래프 영역
        /// </summary>
        public SKRect GraphArea { get; set; }

        /// <summary>
        /// 전체 데이터 시간 범위 최소값
        /// </summary>
        public DateTime DataMinimum { get; set; } = DateTime.Now.AddDays(-30);

        /// <summary>
        /// 전체 데이터 시간 범위 최대값
        /// </summary>
        public DateTime DataMaximum { get; set; } = DateTime.Now;

        /// <summary>
        /// 현재 보이는 시간 범위 최소값
        /// </summary>
        public DateTime VisibleMinimum { get; private set; } = DateTime.Now.AddDays(-7);

        /// <summary>
        /// 현재 보이는 시간 범위 최대값
        /// </summary>
        public DateTime VisibleMaximum { get; private set; } = DateTime.Now;

        /// <summary>
        /// 줌 최소 시간 범위 (초 단위)
        /// </summary>
        public double MinTimeRange { get; set; } = 60 * 60; // 1시간

        /// <summary>
        /// 줌 최대 시간 범위 (초 단위)
        /// </summary>
        public double MaxTimeRange { get; set; } = 60 * 60 * 24 * 365; // 1년

        /// <summary>
        /// 스크롤 핸들 너비
        /// </summary>
        public float HandleWidth { get; private set; }

        /// <summary>
        /// 스크롤 핸들 X 위치
        /// </summary>
        public float HandleX { get; private set; }

        /// <summary>
        /// 드래그 시작 X 위치
        /// </summary>
        public float DragStartX { get; set; }

        /// <summary>
        /// 드래그 시작 시 최소값
        /// </summary>
        public DateTime DragStartMin { get; set; }

        /// <summary>
        /// 드래그 시작 시 최대값
        /// </summary>
        public DateTime DragStartMax { get; set; }

        /// <summary>
        /// 이 컴포넌트가 폐기되었는지 여부
        /// </summary>
        private bool _disposed;

        #endregion

        #region Events

        /// <summary>
        /// 보이는 범위가 변경되었을 때 발생
        /// </summary>
        public event EventHandler<VisibleRangeChangedEventArgs>? VisibleRangeChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// 스크롤 매니저 생성자
        /// </summary>
        public TrendScrollManager()
        {
            // 기본 값으로 초기화됨
        }

        #endregion

        #region Methods

        /// <summary>
        /// 스크롤 핸들 위치 및 크기 업데이트
        /// </summary>
        public void UpdateScrollHandleMetrics()
        {
            try
            {
                TimeSpan totalRange = DataMaximum - DataMinimum;
                if (totalRange.TotalSeconds <= 0)
                    return;

                float totalWidth = ScrollBarArea.Width;

                // 스크롤 핸들 너비 계산 (전체 데이터 중 현재 보이는 비율)
                TimeSpan visibleRange = VisibleMaximum - VisibleMinimum;
                float visibleRatio = (float)(visibleRange.TotalSeconds / totalRange.TotalSeconds);
                HandleWidth = Math.Max(totalWidth * visibleRatio, 30); // 최소 핸들 너비 보장

                // 핸들 위치 계산
                TimeSpan startOffset = VisibleMinimum - DataMinimum;
                float positionRatio = (float)(startOffset.TotalSeconds / totalRange.TotalSeconds);
                HandleX = ScrollBarArea.Left + positionRatio * (totalWidth - HandleWidth);
            }
            catch (Exception)
            {
                // 예외 발생 시 기본값 유지
                HandleWidth = 30;
                HandleX = ScrollBarArea.Left;
            }
        }

        /// <summary>
        /// 스크롤 핸들 드래그 처리
        /// </summary>
        public void HandleScrollDrag(float x)
        {
            try
            {
                float delta = x - DragStartX;
                float totalWidth = ScrollBarArea.Width - HandleWidth;
                if (totalWidth <= 0) return;

                float newX = Math.Clamp(HandleX + delta, ScrollBarArea.Left, ScrollBarArea.Right - HandleWidth);
                float positionRatio = (newX - ScrollBarArea.Left) / totalWidth;

                // 위치 비율을 기반으로 보이는 시간 범위 조정
                TimeSpan totalRange = DataMaximum - DataMinimum;
                TimeSpan visibleRange = VisibleMaximum - VisibleMinimum;

                DateTime newMin = DataMinimum.AddTicks((long)(totalRange.Ticks * positionRatio));
                DateTime newMax = newMin.AddTicks(visibleRange.Ticks);

                // 범위 초과 방지
                if (newMax > DataMaximum)
                {
                    newMax = DataMaximum;
                    newMin = newMax.AddTicks(-visibleRange.Ticks);
                }

                UpdateVisibleRange(newMin, newMax);
                DragStartX = x;
            }
            catch (Exception)
            {
                // 예외 처리 - 드래깅 무시
            }
        }

        /// <summary>
        /// 그래프 드래그 처리
        /// </summary>
        public void HandleGraphDrag(float x)
        {
            try
            {
                float delta = DragStartX - x;
                if (Math.Abs(delta) < 1) return;

                // 픽셀 변화량을 시간 변화량으로 변환
                TimeSpan visibleRange = VisibleMaximum - VisibleMinimum;
                TimeSpan timePerPixel = new((int)(visibleRange.Ticks / GraphArea.Width));
                TimeSpan timeChange = new((long)(timePerPixel.Ticks * delta));

                DateTime newMin = DragStartMin.Add(timeChange);
                DateTime newMax = DragStartMax.Add(timeChange);

                // 범위 초과 방지
                if (newMin < DataMinimum)
                {
                    newMin = DataMinimum;
                    newMax = newMin.AddTicks(visibleRange.Ticks);
                }
                else if (newMax > DataMaximum)
                {
                    newMax = DataMaximum;
                    newMin = newMax.AddTicks(-visibleRange.Ticks);
                }

                UpdateVisibleRange(newMin, newMax);
            }
            catch (Exception)
            {
                // 예외 처리 - 드래깅 무시
            }
        }

        /// <summary>
        /// 마우스 위치를 중심으로 줌
        /// </summary>
        public void ZoomAtPoint(float x, float delta)
        {
            try
            {
                // 현재 보이는 시간 범위
                TimeSpan currentRange = VisibleMaximum - VisibleMinimum;

                // 확대/축소 비율 계산 (델타가 양수면 확대, 음수면 축소)
                float zoomFactor = delta > 0 ? 0.8f : 1.25f;
                double newRangeSecs = currentRange.TotalSeconds * zoomFactor;

                // 최소/최대 범위 제한
                newRangeSecs = Math.Clamp(newRangeSecs, MinTimeRange, MaxTimeRange);

                // 마우스 위치의 시간 계산
                float relativeX = (x - GraphArea.Left) / GraphArea.Width;
                relativeX = Math.Clamp(relativeX, 0, 1); // 0~1 범위로 제한

                TimeSpan offsetFromMin = new((long)(currentRange.Ticks * relativeX));
                DateTime pivotTime = VisibleMinimum.Add(offsetFromMin);

                // 새 범위 계산
                TimeSpan halfNewRange = new((long)(newRangeSecs * TimeSpan.TicksPerSecond / 2));
                DateTime newMin = pivotTime.AddTicks(-halfNewRange.Ticks);
                DateTime newMax = pivotTime.AddTicks(halfNewRange.Ticks);

                // 범위 초과 방지
                if (newMin < DataMinimum)
                {
                    newMin = DataMinimum;
                }
                if (newMax > DataMaximum)
                {
                    newMax = DataMaximum;
                }

                // 범위가 너무 좁아지면 조정
                if ((newMax - newMin).TotalSeconds < MinTimeRange)
                {
                    // 축소하려는 방향에 따라 조정
                    if (pivotTime - newMin < newMax - pivotTime)
                    {
                        // 왼쪽이 더 가까우면 오른쪽 조정
                        newMax = newMin.AddSeconds(MinTimeRange);
                    }
                    else
                    {
                        // 오른쪽이 더 가까우면 왼쪽 조정
                        newMin = newMax.AddSeconds(-MinTimeRange);
                    }
                }

                UpdateVisibleRange(newMin, newMax);
            }
            catch (Exception)
            {
                // 예외 처리 - 줌 작업 무시
            }
        }

        /// <summary>
        /// 특정 위치로 스크롤
        /// </summary>
        public void ScrollToPosition(float x)
        {
            try
            {
                float handleCenter = Math.Clamp(x, ScrollBarArea.Left + HandleWidth / 2,
                    ScrollBarArea.Right - HandleWidth / 2);

                float positionRatio = (handleCenter - ScrollBarArea.Left - HandleWidth / 2) /
                    (ScrollBarArea.Width - HandleWidth);

                // 위치 비율을 기반으로 보이는 시간 범위 조정
                TimeSpan totalRange = DataMaximum - DataMinimum;
                TimeSpan visibleRange = VisibleMaximum - VisibleMinimum;

                DateTime newMin = DataMinimum.AddTicks((long)(totalRange.Ticks * positionRatio));
                DateTime newMax = newMin.AddTicks(visibleRange.Ticks);

                // 범위 초과 방지
                if (newMax > DataMaximum)
                {
                    newMax = DataMaximum;
                    newMin = newMax.AddTicks(-visibleRange.Ticks);
                }

                UpdateVisibleRange(newMin, newMax);
            }
            catch (Exception)
            {
                // 예외 처리 - 스크롤 무시
            }
        }

        /// <summary>
        /// 보이는 범위 업데이트
        /// </summary>
        public void UpdateVisibleRange(DateTime newMin, DateTime newMax)
        {
            // 범위가 변경되었는지 확인
            if (newMin == VisibleMinimum && newMax == VisibleMaximum)
                return;

            VisibleMinimum = newMin;
            VisibleMaximum = newMax;

            // 이벤트 발생
            VisibleRangeChanged?.Invoke(this, new VisibleRangeChangedEventArgs(VisibleMinimum, VisibleMaximum));
        }

        /// <summary>
        /// 보이는 범위 조정
        /// </summary>
        public void AdjustVisibleRange()
        {
            // 보이는 범위가 데이터 범위 내에 있도록 조정
            if (VisibleMinimum < DataMinimum)
            {
                TimeSpan offset = DataMinimum - VisibleMinimum;
                VisibleMinimum = DataMinimum;
                VisibleMaximum = VisibleMaximum.Add(offset);
            }

            if (VisibleMaximum > DataMaximum)
            {
                TimeSpan offset = VisibleMaximum - DataMaximum;
                VisibleMaximum = DataMaximum;
                VisibleMinimum = VisibleMinimum.Subtract(offset);
            }

            // 보이는 범위가 데이터 범위보다 크면 데이터 범위로 설정
            if (VisibleMaximum - VisibleMinimum > DataMaximum - DataMinimum)
            {
                VisibleMinimum = DataMinimum;
                VisibleMaximum = DataMaximum;
            }

            // 최소 시간 범위 보장
            double visibleRangeSecs = (VisibleMaximum - VisibleMinimum).TotalSeconds;
            if (visibleRangeSecs < MinTimeRange)
            {
                // 중앙을 기준으로 범위 확장
                DateTime center = VisibleMinimum.AddTicks((VisibleMaximum - VisibleMinimum).Ticks / 2);
                TimeSpan halfRange = TimeSpan.FromSeconds(MinTimeRange / 2);
                VisibleMinimum = center.Subtract(halfRange);
                VisibleMaximum = center.Add(halfRange);

                // 범위 조정 후 다시 검사
                AdjustVisibleRange();
            }
        }

        /// <summary>
        /// 일정한 양으로 줌
        /// </summary>
        public void Zoom(float factor)
        {
            try
            {
                // 현재 보이는 시간 범위
                TimeSpan currentRange = VisibleMaximum - VisibleMinimum;
                TimeSpan newRange = new((long)(currentRange.Ticks * factor));

                // 최소/최대 범위 제한
                double newRangeSecs = newRange.TotalSeconds;
                newRangeSecs = Math.Clamp(newRangeSecs, MinTimeRange, MaxTimeRange);
                newRange = TimeSpan.FromSeconds(newRangeSecs);

                // 중앙 기준 확대/축소
                DateTime center = VisibleMinimum.AddTicks(currentRange.Ticks / 2);
                DateTime newMin = center.AddTicks(-newRange.Ticks / 2);
                DateTime newMax = center.AddTicks(newRange.Ticks / 2);

                // 범위 초과 방지
                if (newMin < DataMinimum)
                {
                    newMin = DataMinimum;
                    newMax = newMin.AddTicks(newRange.Ticks);
                }
                if (newMax > DataMaximum)
                {
                    newMax = DataMaximum;
                    newMin = newMax.AddTicks(-newRange.Ticks);
                }

                UpdateVisibleRange(newMin, newMax);
            }
            catch (Exception)
            {
                // 예외 처리 - 줌 무시
            }
        }

        /// <summary>
        /// 주어진 시간만큼 스크롤
        /// </summary>
        public void ScrollByTime(TimeSpan amount)
        {
            try
            {
                DateTime newMin = VisibleMinimum.Add(amount);
                DateTime newMax = VisibleMaximum.Add(amount);

                // 범위 초과 방지
                if (newMin < DataMinimum)
                {
                    newMin = DataMinimum;
                    newMax = newMin.AddTicks((VisibleMaximum - VisibleMinimum).Ticks);
                }
                else if (newMax > DataMaximum)
                {
                    newMax = DataMaximum;
                    newMin = newMax.AddTicks(-(VisibleMaximum - VisibleMinimum).Ticks);
                }

                UpdateVisibleRange(newMin, newMax);
            }
            catch (Exception)
            {
                // 예외 처리 - 스크롤 무시
            }
        }

        #endregion

        #region IDisposable 구현

        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 리소스 해제 구현
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리되는 리소스 해제
                    VisibleRangeChanged = null;
                }

                // 관리되지 않는 리소스 해제 (현재 없음)

                _disposed = true;
            }
        }

        /// <summary>
        /// 소멸자
        /// </summary>
        ~TrendScrollManager()
        {
            Dispose(false);
        }
        
        #endregion
    }
}